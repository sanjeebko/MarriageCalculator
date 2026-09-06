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
import np.com.sanjeeb.marriagecalculator.data.model.RoundPlayerInput
import np.com.sanjeeb.marriagecalculator.data.model.SubmitRoundRequest
import np.com.sanjeeb.marriagecalculator.data.network.NetworkMonitor
import np.com.sanjeeb.marriagecalculator.data.repository.ApiResult
import np.com.sanjeeb.marriagecalculator.data.repository.GameSetRepository
import np.com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import javax.inject.Inject
import javax.inject.Singleton

sealed interface SyncStatus {
    data class Offline(val pendingCount: Int = 0) : SyncStatus
    data class PendingSync(val pendingCount: Int) : SyncStatus
    data class Syncing(val pendingCount: Int = 0) : SyncStatus
    data object Synced : SyncStatus

    val isSynced: Boolean get() = this is Synced
    val isOffline: Boolean get() = this is Offline
}

@Singleton
class SyncManager @Inject constructor(
    private val networkMonitor: NetworkMonitor,
    private val offlineGameRepository: OfflineGameRepository,
    private val gameSetRepository: GameSetRepository,
    private val sessionManager: SessionManager
) {
    private val syncScope = CoroutineScope(SupervisorJob() + Dispatchers.IO)
    private val _isSyncing = MutableStateFlow(false)

    val syncStatus: StateFlow<SyncStatus> = combine(
        networkMonitor.isOnline,
        offlineGameRepository.unsyncedCountFlow,
        _isSyncing
    ) { isOnline, pendingCount, isSyncing ->
        when {
            !isOnline -> SyncStatus.Offline(pendingCount)
            isSyncing -> SyncStatus.Syncing(pendingCount)
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
        try {
            // 1. Sync pending rounds for game sets that have a remoteId
            val unsyncedRounds = offlineGameRepository.getUnsyncedRounds()
            for (round in unsyncedRounds) {
                val gameSet = offlineGameRepository.getGameSet(round.gameSetId) ?: continue
                val remoteGameSetId = gameSet.remoteId
                if (remoteGameSetId.isNullOrEmpty()) continue

                val scores = offlineGameRepository.getRoundScores(round.id)
                val players = offlineGameRepository.getGameSetPlayers(round.gameSetId)

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
            syncPendingData()
        }
    }
}
