package np.com.sanjeeb.marriagecalculator.data.sync

import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.flow.distinctUntilChanged
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch
import np.com.sanjeeb.marriagecalculator.data.model.CreateGameSetRequest
import np.com.sanjeeb.marriagecalculator.data.model.CreateGameSettingsRequest
import np.com.sanjeeb.marriagecalculator.data.model.CreatePlayerRequest
import np.com.sanjeeb.marriagecalculator.data.model.GameSettings
import np.com.sanjeeb.marriagecalculator.data.model.RoundPlayerInput
import np.com.sanjeeb.marriagecalculator.data.model.SubmitRoundRequest
import np.com.sanjeeb.marriagecalculator.data.network.NetworkMonitor
import np.com.sanjeeb.marriagecalculator.data.repository.ApiResult
import np.com.sanjeeb.marriagecalculator.data.repository.GameSetRepository
import np.com.sanjeeb.marriagecalculator.data.repository.GameSettingsRepository
import np.com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import np.com.sanjeeb.marriagecalculator.data.repository.PlayerRepository
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import javax.inject.Inject
import javax.inject.Singleton

sealed interface SyncStatus {
    data class Offline(val pendingCount: Int = 0) : SyncStatus
    data class PendingSync(val pendingCount: Int) : SyncStatus
    data class Syncing(val pendingCount: Int = 0) : SyncStatus
    data object Synced : SyncStatus
    /** Sync was attempted but failed — message describes the reason (e.g. "Session expired"). */
    data class Error(val message: String, val pendingCount: Int = 0) : SyncStatus

    val isSynced: Boolean get() = this is Synced
    val isOffline: Boolean get() = this is Offline
}

@Singleton
class SyncManager @Inject constructor(
    private val networkMonitor: NetworkMonitor,
    private val offlineGameRepository: OfflineGameRepository,
    private val gameSetRepository: GameSetRepository,
    private val playerRepository: PlayerRepository,
    private val gameSettingsRepository: GameSettingsRepository,
    private val sessionManager: SessionManager
) {
    private val syncScope = CoroutineScope(SupervisorJob() + Dispatchers.IO)
    private val _isSyncing = MutableStateFlow(false)
    private val _lastError = MutableStateFlow<String?>(null)

    val syncStatus: StateFlow<SyncStatus> = combine(
        networkMonitor.isOnline,
        offlineGameRepository.unsyncedCountFlow,
        _isSyncing,
        _lastError
    ) { isOnline, pendingCount, isSyncing, lastError ->
        when {
            !isOnline -> SyncStatus.Offline(pendingCount)
            isSyncing -> SyncStatus.Syncing(pendingCount)
            lastError != null -> SyncStatus.Error(lastError, pendingCount)
            pendingCount > 0 -> SyncStatus.PendingSync(pendingCount)
            else -> SyncStatus.Synced
        }
    }.stateIn(
        scope = syncScope,
        started = SharingStarted.Eagerly,
        initialValue = SyncStatus.Synced
    )

    init {
        // Auto-sync whenever network connectivity is restored and we have pending unsynced records
        syncScope.launch {
            networkMonitor.isOnline.distinctUntilChanged().collect { isOnline ->
                if (isOnline && sessionManager.isOnlineMode()) {
                    syncPendingData()
                }
            }
        }
    }

    suspend fun syncPendingData(): Boolean {
        if (_isSyncing.value) return false
        if (!sessionManager.isOnlineMode()) return false

        _isSyncing.value = true
        _lastError.value = null  // Clear any previous error on new sync attempt
        try {
            // 1. Sync pending game sets (and their players/settings) that do not yet have a remoteId
            val unsyncedGameSets = offlineGameRepository.getUnsyncedGameSets()
            for (gameSet in unsyncedGameSets) {
                val existingRemoteId = gameSet.remoteId
                if (!existingRemoteId.isNullOrEmpty()) {
                    offlineGameRepository.markGameSetSynced(gameSet.id, existingRemoteId)
                    continue
                }

                val settings = offlineGameRepository.getGameSettings(gameSet.settingsId) ?: GameSettings()
                val settingsRequest = CreateGameSettingsRequest(
                    murder = settings.murder,
                    kidnap = settings.kidnap,
                    seenPoint = settings.seenPoint,
                    unseenPoint = settings.unseenPoint,
                    pointRate = settings.pointRate,
                    currency = settings.currency,
                    dublee = settings.dublee,
                    dubleePointLess = settings.dubleePointLess,
                    dubleePointBonus = settings.dubleePointBonus,
                    foulPoint = settings.foulPoint,
                    foulPointBonus = settings.foulPointBonus,
                    audio = settings.audio
                )
                val settingsResult = gameSettingsRepository.createGameSettings(settingsRequest)
                if (settingsResult is ApiResult.Unauthorized) {
                    _lastError.value = "Session expired. Please sign in again."
                    sessionManager.emitSessionExpired()
                    return false
                }
                val remoteSettingsId = if (settingsResult is ApiResult.Success) settingsResult.data.id else continue

                val players = offlineGameRepository.getGameSetPlayers(gameSet.id)
                val remotePlayerIds = mutableListOf<String>()
                var playerFailed = false
                for (player in players) {
                    val localPlayerId = player.id.toIntOrNull()
                    val entity = if (localPlayerId != null) offlineGameRepository.getPlayerEntity(localPlayerId) else null
                    val playerRemoteId = entity?.remoteId ?: player.id.takeIf { it.toIntOrNull() == null }
                    if (!playerRemoteId.isNullOrEmpty()) {
                        remotePlayerIds.add(playerRemoteId)
                    } else {
                        val createReq = CreatePlayerRequest(
                            name = player.name,
                            email = player.email,
                            photoUri = player.photoUri
                        )
                        when (val pResult = playerRepository.createPlayer(createReq)) {
                            is ApiResult.Success -> {
                                val newRemoteId = pResult.data.id
                                remotePlayerIds.add(newRemoteId)
                                if (localPlayerId != null) {
                                    offlineGameRepository.updatePlayerRemoteId(localPlayerId, newRemoteId)
                                }
                            }
                            is ApiResult.Unauthorized -> {
                                _lastError.value = "Session expired. Please sign in again."
                                sessionManager.emitSessionExpired()
                                return false
                            }
                            else -> {
                                playerFailed = true
                                break
                            }
                        }
                    }
                }
                if (playerFailed) continue

                val gameSetRequest = CreateGameSetRequest(
                    name = gameSet.name.ifEmpty { "Game Set #${gameSet.id}" },
                    gameSettingsId = remoteSettingsId,
                    playerIds = remotePlayerIds
                )
                val createResult = gameSetRepository.createGameSet(gameSetRequest)
                if (createResult is ApiResult.Unauthorized) {
                    _lastError.value = "Session expired. Please sign in again."
                    sessionManager.emitSessionExpired()
                    return false
                }
                if (createResult is ApiResult.Success) {
                    offlineGameRepository.markGameSetSynced(gameSet.id, createResult.data.id)
                }
            }

            // 2. Sync pending rounds for game sets that have a remoteId
            val unsyncedRounds = offlineGameRepository.getUnsyncedRounds()
            for (round in unsyncedRounds) {
                val gameSet = offlineGameRepository.getGameSet(round.gameSetId) ?: continue
                val remoteGameSetId = gameSet.remoteId
                if (remoteGameSetId.isNullOrEmpty()) continue

                val scores = offlineGameRepository.getRoundScores(round.id)

                val winnerEntity = offlineGameRepository.getPlayerEntity(round.winnerId)
                val remoteWinnerId = winnerEntity?.remoteId ?: round.winnerId.toString()

                val dealerEntity = if (round.dealerId > 0) offlineGameRepository.getPlayerEntity(round.dealerId) else null
                val remoteDealerId = dealerEntity?.remoteId ?: (if (round.dealerId > 0) round.dealerId.toString() else "")

                val roundPlayerInputs = scores.map { score ->
                    val playerEntity = offlineGameRepository.getPlayerEntity(score.playerId)
                    val remotePlayerId = playerEntity?.remoteId ?: score.playerId.toString()
                    RoundPlayerInput(
                        playerId = remotePlayerId,
                        seen = score.isSeen,
                        duply = score.isDublee,
                        maal = score.maal
                    )
                }

                val submitRequest = SubmitRoundRequest(
                    winnerId = remoteWinnerId,
                    dealerId = remoteDealerId,
                    players = roundPlayerInputs
                )

                val apiResult = gameSetRepository.submitRound(remoteGameSetId, submitRequest)
                if (apiResult is ApiResult.Unauthorized) {
                    _lastError.value = "Session expired. Please sign in again."
                    sessionManager.emitSessionExpired()
                    return false
                }
                if (apiResult is ApiResult.Success) {
                    offlineGameRepository.markRoundSynced(round.id, apiResult.data.id)
                }
            }
            return true
        } catch (e: Exception) {
            return false
        } finally {
            _isSyncing.value = false
        }
    }

    fun triggerSync() {
        syncScope.launch {
            _lastError.value = null  // Clear error state when user manually triggers sync
            syncPendingData()
        }
    }
}
