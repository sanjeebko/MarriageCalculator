package np.com.sanjeeb.marriagecalculator.ui.playgame

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import np.com.sanjeeb.marriagecalculator.data.model.*
import np.com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import np.com.sanjeeb.marriagecalculator.data.repository.GameSetRepository
import np.com.sanjeeb.marriagecalculator.data.repository.PlayerRepository
import np.com.sanjeeb.marriagecalculator.data.repository.FriendRepository
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import np.com.sanjeeb.marriagecalculator.data.repository.ApiResult
import np.com.sanjeeb.marriagecalculator.ui.scoreboard.RoundPlayerEntry
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch
import javax.inject.Inject

data class PlayerStandings(
    val player: Player,
    val netPoints: Int = 0,
    val totalMoney: Double = 0.0,
    val isNextDealer: Boolean = false
)

/** One hand/deal within a round - one player deals, everyone else plays. */
data class GameEntry(
    val gameId: String,
    val gameSequenceInRound: Int,
    val dealerId: String,
    val winnerId: String,
    val winnerName: String,
    val totalMaal: Int,
    val playerEntries: List<RoundPlayerEntry> = emptyList()
)

/** A round is complete once every player has dealt once (games.size == player count), or is closed early. */
data class RoundGroup(
    val roundId: String,
    val roundSequence: Int,
    val isCompleted: Boolean,
    val games: List<GameEntry> = emptyList(),
    val totalScoreByPlayer: Map<String, Int> = emptyMap()
)

data class PlayGameUiState(
    val gameName: String = "",
    val players: List<PlayerStandings> = emptyList(),
    val roundGroups: List<RoundGroup> = emptyList(),
    val totalGamesPlayed: Int = 0,
    val nextDealerId: String = "",
    val nextDealerName: String = "",
    val isSettled: Boolean = false,
    val isLoading: Boolean = false,
    val isHost: Boolean = true,
    val isOnlineMode: Boolean = false,
    val friendsList: List<User> = emptyList(),
    val currentUserEmail: String = "",
    val error: String? = null,
    val gameSettingsId: String = "",
    val settings: GameSettings = GameSettings.default()
)

@HiltViewModel
class PlayGameViewModel @Inject constructor(
    private val offlineGameRepository: OfflineGameRepository,
    private val gameSetRepository: GameSetRepository,
    private val playerRepository: PlayerRepository,
    private val friendRepository: FriendRepository,
    private val sessionManager: SessionManager
) : ViewModel() {

    private val _uiState = MutableStateFlow(PlayGameUiState())
    val uiState: StateFlow<PlayGameUiState> = _uiState.asStateFlow()

    fun loadGame(gameSetIdStr: String) {
        val isLocalId = gameSetIdStr.toIntOrNull() != null
        val isOnline = sessionManager.isOnlineMode() && !isLocalId
        _uiState.value = _uiState.value.copy(isLoading = true, isOnlineMode = isOnline, error = null)

        if (isOnline) {
            viewModelScope.launch {
                when (val result = gameSetRepository.getGameSet(gameSetIdStr)) {
                    is ApiResult.Success -> {
                        val gameSet = result.data
                        val gameSetPlayers = gameSet.gameSetPlayers?.values?.sortedBy { it.position } ?: emptyList()
                        val players = gameSetPlayers.mapNotNull { it.player }
                        val settings = gameSet.gameSettings ?: GameSettings.default()

                        val roundGroups = gameSet.rounds?.sortedBy { it.sequence }?.map { r ->
                            val games = r.marriageGames?.sortedBy { it.sequence }?.map { g ->
                                val winnerName = players.find { it.id == g.winnerId }?.name ?: "Unknown"
                                val playerEntries = players.map { p ->
                                    val score = g.marriageGameScores?.get(p.id)
                                    RoundPlayerEntry(
                                        playerId = p.id,
                                        playerName = p.name,
                                        isSeen = score?.seen ?: false,
                                        isDublee = score?.duply ?: false,
                                        isWinner = score?.winner ?: false,
                                        maal = score?.maal ?: 0,
                                        score = score?.score ?: 0,
                                        money = (score?.score ?: 0) * settings.pointRate
                                    )
                                }
                                GameEntry(
                                    gameId = g.id,
                                    gameSequenceInRound = g.sequence,
                                    dealerId = g.dealerId,
                                    winnerId = g.winnerId,
                                    winnerName = winnerName,
                                    totalMaal = g.totalMaal,
                                    playerEntries = playerEntries
                                )
                            } ?: emptyList()

                            RoundGroup(
                                roundId = r.id,
                                roundSequence = r.sequence,
                                isCompleted = r.completed,
                                games = games,
                                totalScoreByPlayer = r.totalScore?.mapValues { it.value.toInt() } ?: emptyMap()
                            )
                        } ?: emptyList()

                        val totalGamesPlayed = roundGroups.sumOf { it.games.size }
                        // Must match RoundInputViewModel.loadGameData's dealer formula (roundNumber - 2 + size) % size,
                        // where roundNumber = totalGamesPlayed + 1: simplifies to (totalGamesPlayed - 1 + size) % size.
                        val nextDealerIndex = if (players.isNotEmpty()) (totalGamesPlayed - 1 + players.size) % players.size else 0
                        val nextDealer = players.getOrNull(nextDealerIndex)

                        val standings = gameSetPlayers.mapIndexed { index, gsp ->
                            val p = gsp.player ?: Player(id = gsp.playerId, name = "Unknown")
                            var netPoints = 0
                            gameSet.rounds?.forEach { r ->
                                val scoreMap = r.totalScore ?: emptyMap()
                                netPoints += scoreMap[p.id]?.toInt() ?: 0
                            }
                            PlayerStandings(
                                player = p,
                                netPoints = netPoints,
                                totalMoney = netPoints * settings.pointRate,
                                isNextDealer = index == nextDealerIndex
                            )
                        }

                        val isCurrentUserHost = gameSet.hostUserId == sessionManager.getUserProfile()?.userId
                        val userEmail = sessionManager.getUserProfile()?.email ?: ""

                        _uiState.value = PlayGameUiState(
                            gameName = gameSet.name,
                            players = standings,
                            roundGroups = roundGroups,
                            totalGamesPlayed = totalGamesPlayed,
                            nextDealerId = nextDealer?.id ?: "",
                            nextDealerName = nextDealer?.name ?: "None",
                            isSettled = !gameSet.isActive,
                            isLoading = false,
                            isHost = isCurrentUserHost,
                            isOnlineMode = true,
                            friendsList = emptyList(),
                            currentUserEmail = userEmail,
                            gameSettingsId = gameSet.gameSettingsId,
                            settings = settings
                        )
                        loadFriends()
                    }
                    is ApiResult.Error -> {
                        _uiState.value = _uiState.value.copy(
                            isLoading = false,
                            error = result.message
                        )
                    }
                    is ApiResult.Loading -> {}
                }
            }
        } else {
            val gameSetId = gameSetIdStr.toIntOrNull() ?: return
            viewModelScope.launch {
                val gameSet = offlineGameRepository.getGameSet(gameSetId) ?: return@launch
                val players = offlineGameRepository.getGameSetPlayers(gameSetId)
                val settings = offlineGameRepository.getGameSettings(gameSet.settingsId) ?: GameSettings.default()

                offlineGameRepository.getRounds(gameSetId).collect { roundEntities ->
                    val allScores = offlineGameRepository.getAllScoresForGameSet(gameSetId)
                    val scoresByRound = allScores.groupBy { it.roundId }
                    val playerCount = players.size

                    val orderedGames = roundEntities.sortedBy { it.roundNumber }
                    val roundGroups = mutableListOf<RoundGroup>()
                    var bucket = mutableListOf<GameEntry>()
                    var roundSeq = 1

                    fun flushBucket(isCompleted: Boolean) {
                        if (bucket.isEmpty()) return
                        roundGroups.add(
                            RoundGroup(
                                roundId = "local-$roundSeq",
                                roundSequence = roundSeq,
                                isCompleted = isCompleted,
                                games = bucket.toList(),
                                totalScoreByPlayer = bucket.flatMap { it.playerEntries }
                                    .groupBy { it.playerId }
                                    .mapValues { entry -> entry.value.sumOf { e -> e.score } }
                            )
                        )
                        bucket = mutableListOf()
                        roundSeq++
                    }

                    for (r in orderedGames) {
                        val roundScores = scoresByRound[r.id] ?: emptyList()
                        val winnerScore = roundScores.find { it.isWinner }
                        val winnerName = players.find { it.id == winnerScore?.playerId?.toString() }?.name ?: "Unknown"
                        val playerEntries = players.map { p ->
                            val pScore = roundScores.find { it.playerId.toString() == p.id }
                            RoundPlayerEntry(
                                playerId = p.id,
                                playerName = p.name,
                                isSeen = pScore?.isSeen ?: false,
                                isDublee = pScore?.isDublee ?: false,
                                isWinner = pScore?.isWinner ?: false,
                                maal = pScore?.maal ?: 0,
                                score = pScore?.score ?: 0,
                                money = (pScore?.score ?: 0) * settings.pointRate
                            )
                        }
                        bucket.add(
                            GameEntry(
                                gameId = r.id.toString(),
                                gameSequenceInRound = bucket.size + 1,
                                dealerId = r.dealerId.toString(),
                                winnerId = r.winnerId.toString(),
                                winnerName = winnerName,
                                totalMaal = r.totalMaal,
                                playerEntries = playerEntries
                            )
                        )
                        if (bucket.size >= playerCount || r.closesRound) {
                            flushBucket(isCompleted = true)
                        }
                    }
                    flushBucket(isCompleted = false)

                    val totalGamesPlayed = orderedGames.size
                    // Must match RoundInputViewModel's dealer formula: (totalGamesPlayed - 1 + size) % size.
                    val nextDealerIndex = if (players.isNotEmpty()) (totalGamesPlayed - 1 + players.size) % players.size else 0
                    val nextDealer = players.getOrNull(nextDealerIndex)

                    val standings = players.mapIndexed { index, p ->
                        val pScores = allScores.filter { it.playerId == p.id.toInt() }
                        val netPoints = pScores.sumOf { it.score }
                        PlayerStandings(
                            player = p,
                            netPoints = netPoints,
                            totalMoney = netPoints * settings.pointRate,
                            isNextDealer = index == nextDealerIndex
                        )
                    }

                    _uiState.value = PlayGameUiState(
                        gameName = gameSet.name.ifEmpty { "Game Set #$gameSetId" },
                        players = standings,
                        roundGroups = roundGroups,
                        totalGamesPlayed = totalGamesPlayed,
                        nextDealerId = nextDealer?.id ?: "",
                        nextDealerName = nextDealer?.name ?: "None",
                        isSettled = gameSet.isSettled,
                        isLoading = false,
                        isHost = true,
                        isOnlineMode = false,
                        gameSettingsId = gameSet.settingsId.toString(),
                        settings = settings
                    )
                }
            }
        }
    }

    fun reorderPlayers(orderedPlayerIds: List<String>, gameSetIdStr: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            val isLocalId = gameSetIdStr.toIntOrNull() != null
            if (isLocalId) {
                val gameSetId = gameSetIdStr.toInt()
                val localPlayerIds = orderedPlayerIds.mapNotNull { it.toIntOrNull() }
                try {
                    offlineGameRepository.updateGameSetPlayerPositions(gameSetId, localPlayerIds)
                } catch (e: Exception) {
                    _uiState.value = _uiState.value.copy(isLoading = false, error = e.message)
                    return@launch
                }
            } else {
                if (sessionManager.isOnlineMode()) {
                    val req = CreateGameSetRequest(
                        name = _uiState.value.gameName,
                        gameSettingsId = _uiState.value.gameSettingsId,
                        playerIds = orderedPlayerIds
                    )
                    when (val result = gameSetRepository.updateGameSet(gameSetIdStr, req)) {
                        is ApiResult.Success -> {
                            // Successfully updated online
                        }
                        is ApiResult.Error -> {
                            _uiState.value = _uiState.value.copy(isLoading = false, error = result.message)
                            return@launch
                        }
                        is ApiResult.Loading -> {}
                    }
                }
            }
            loadGame(gameSetIdStr)
        }
    }

    private fun loadFriends() {
        viewModelScope.launch {
            when (val result = friendRepository.getFriends()) {
                is ApiResult.Success -> {
                    _uiState.value = _uiState.value.copy(friendsList = result.data)
                }
                else -> {}
            }
        }
    }

    fun mapPlayerToFriend(playerIdStr: String, friend: User, gameSetIdStr: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            val localId = playerIdStr.toIntOrNull()
            if (localId != null) {
                offlineGameRepository.updatePlayerEmailAndName(localId, friend.email, friend.displayName)
            }

            if (sessionManager.isOnlineMode()) {
                if (!playerIdStr.startsWith("local_")) {
                    val req = UpdatePlayerRequest(
                        name = friend.displayName,
                        email = friend.email
                    )
                    playerRepository.updatePlayer(playerIdStr, req)
                }
            }
            loadGame(gameSetIdStr)
        }
    }

    fun transferHost(newHostUserId: String, gameSetIdStr: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            when (val result = gameSetRepository.transferHost(gameSetIdStr, newHostUserId)) {
                is ApiResult.Success -> {
                    loadGame(gameSetIdStr)
                }
                is ApiResult.Error -> {
                    _uiState.value = _uiState.value.copy(isLoading = false, error = result.message)
                }
                else -> {}
            }
        }
    }

    /** Ends the current round early (fewer than N games played), e.g. to add a player or restart. */
    fun closeCurrentRound(gameSetIdStr: String) {
        viewModelScope.launch {
            val isLocalId = gameSetIdStr.toIntOrNull() != null
            val isOnline = sessionManager.isOnlineMode() && !isLocalId

            if (isOnline) {
                val openRoundId = _uiState.value.roundGroups.find { !it.isCompleted }?.roundId
                if (openRoundId != null) {
                    when (val result = gameSetRepository.closeRound(gameSetIdStr, openRoundId)) {
                        is ApiResult.Error -> _uiState.value = _uiState.value.copy(error = result.message)
                        else -> {}
                    }
                }
            } else {
                val gameSetId = gameSetIdStr.toIntOrNull() ?: return@launch
                offlineGameRepository.closeCurrentRound(gameSetId)
            }
            loadGame(gameSetIdStr)
        }
    }

    /** Removes only the most recently played game (undo a mistake). Blocked once settled. */
    fun deleteLastGame(gameSetIdStr: String) {
        viewModelScope.launch {
            val isLocalId = gameSetIdStr.toIntOrNull() != null
            val isOnline = sessionManager.isOnlineMode() && !isLocalId

            if (isOnline) {
                when (val result = gameSetRepository.deleteLastGame(gameSetIdStr)) {
                    is ApiResult.Error -> {
                        _uiState.value = _uiState.value.copy(error = result.message)
                        return@launch
                    }
                    else -> {}
                }
            } else {
                val gameSetId = gameSetIdStr.toIntOrNull() ?: return@launch
                offlineGameRepository.deleteLastGame(gameSetId)
            }
            loadGame(gameSetIdStr)
        }
    }

    /** Removes an entire round - all its games and scores. Blocked once settled. */
    fun deleteRound(gameSetIdStr: String, round: RoundGroup) {
        viewModelScope.launch {
            val isLocalId = gameSetIdStr.toIntOrNull() != null
            val isOnline = sessionManager.isOnlineMode() && !isLocalId

            if (isOnline) {
                when (val result = gameSetRepository.deleteRound(gameSetIdStr, round.roundId)) {
                    is ApiResult.Error -> {
                        _uiState.value = _uiState.value.copy(error = result.message)
                        return@launch
                    }
                    else -> {}
                }
            } else {
                val gameSetId = gameSetIdStr.toIntOrNull() ?: return@launch
                val gameIds = round.games.mapNotNull { it.gameId.toIntOrNull() }
                offlineGameRepository.deleteRoundGames(gameSetId, gameIds)
            }
            loadGame(gameSetIdStr)
        }
    }

    /** Permanently deletes the whole game set. Irreversible - caller must confirm first. */
    fun deleteGameSet(gameSetIdStr: String, onDeleted: () -> Unit) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            val isLocalId = gameSetIdStr.toIntOrNull() != null
            val isOnline = sessionManager.isOnlineMode() && !isLocalId

            if (isOnline) {
                when (val result = gameSetRepository.deleteGameSet(gameSetIdStr)) {
                    is ApiResult.Success -> onDeleted()
                    is ApiResult.Error -> _uiState.value = _uiState.value.copy(isLoading = false, error = result.message)
                    is ApiResult.Loading -> {}
                }
            } else {
                val gameSetId = gameSetIdStr.toIntOrNull()
                if (gameSetId == null) {
                    _uiState.value = _uiState.value.copy(isLoading = false, error = "Invalid game set")
                    return@launch
                }
                offlineGameRepository.deleteGameSet(gameSetId)
                onDeleted()
            }
        }
    }

    fun nudgePlayer(playerId: String, gameSetIdStr: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            when (val result = gameSetRepository.nudgePlayer(gameSetIdStr, playerId)) {
                is ApiResult.Success -> {
                    _uiState.value = _uiState.value.copy(isLoading = false)
                }
                is ApiResult.Error -> {
                    _uiState.value = _uiState.value.copy(isLoading = false, error = result.message)
                }
                else -> {
                    _uiState.value = _uiState.value.copy(isLoading = false)
                }
            }
        }
    }

    fun clearError() {
        _uiState.value = _uiState.value.copy(error = null)
    }
}
