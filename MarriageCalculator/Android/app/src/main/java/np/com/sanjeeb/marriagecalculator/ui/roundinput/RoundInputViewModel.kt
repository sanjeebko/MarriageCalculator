package np.com.sanjeeb.marriagecalculator.ui.roundinput

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import np.com.sanjeeb.marriagecalculator.data.model.GameSettings
import np.com.sanjeeb.marriagecalculator.data.model.MaalItem
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.data.model.RoundPlayerInput
import np.com.sanjeeb.marriagecalculator.data.model.SubmitRoundRequest
import np.com.sanjeeb.marriagecalculator.data.remote.*
import np.com.sanjeeb.marriagecalculator.data.repository.ApiResult
import np.com.sanjeeb.marriagecalculator.data.repository.GameSetRepository
import np.com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class PlayerRoundState(
    val player: Player,
    val seen: Boolean = false,
    val seenPoints: Int = 0,
    val unseenPoints: Int = 0,
    val duply: Boolean = false,
    val isWinner: Boolean = false,
    val isDealer: Boolean = false,
    // Calculated preview
    val previewScore: Int = 0,
    val previewMoney: Double = 0.0
)

data class RoundInputUiState(
    val gameSetId: String? = null,
    /** Non-null when re-scoring an already-played game instead of adding a new one. */
    val editGameId: String? = null,
    val playerStates: List<PlayerRoundState> = emptyList(),
    val settings: GameSettings = GameSettings.default(),
    val winnerId: String? = null,
    val dealerId: String? = null,
    /** 1-indexed number of the game being entered within its round (null in edit mode). */
    val gameNumber: Int? = null,
    val isLoading: Boolean = false,
    val error: String? = null,
    val submitted: Boolean = false,
    val showPreview: Boolean = false,
    /** Maal calculator card counts per player id (kept so reopening the dialog restores counts). */
    val maalCounts: Map<String, Map<MaalItem, Int>> = emptyMap()
)

@HiltViewModel
class RoundInputViewModel @Inject constructor(
    private val scoringApi: ScoringApiService,
    private val offlineGameRepository: OfflineGameRepository,
    private val gameSetRepository: GameSetRepository,
    private val sessionManager: SessionManager
) : ViewModel() {

    companion object {
        /**
         * Extra Maal a dublee winner scores on top of their actual maal.
         * Fixed game rule mirroring C# ScoringEngine.DubleeWinnerMaalBonus.
         */
        const val DUBLEE_WINNER_MAAL_BONUS = 5
    }

    private val _uiState = MutableStateFlow(RoundInputUiState())
    val uiState: StateFlow<RoundInputUiState> = _uiState.asStateFlow()

    fun initPlayers(players: List<Player>, settings: GameSettings) {
        _uiState.value = _uiState.value.copy(
            playerStates = players.map { PlayerRoundState(player = it) },
            settings = settings
        )
    }

    fun loadGameData(gameSetIdStr: String, roundNumber: Int, editGameId: String? = null) {
        viewModelScope.launch {
            val isLocalId = gameSetIdStr.toIntOrNull() != null
            val isOnline = sessionManager.isOnlineMode() && !isLocalId

            if (editGameId != null) {
                loadForEdit(gameSetIdStr, editGameId, isOnline)
                return@launch
            }

            // Seat order for the round this game belongs to (the open round's snapshot, or the
            // game set's current - possibly just reshuffled - order if a fresh round is starting),
            // plus how many games that round already has. Dealer rotation is relative to the
            // round: its first game is dealt by the LAST player in the seat order (the lowest-card
            // picker at reshuffle), then the deal wraps to the top of the list. Must match
            // PlayGameViewModel's nextDealerFor.
            var seatOrder: List<Player> = emptyList()
            var gamesInOpenRound: Int = 0
            var settings: GameSettings = GameSettings.default()
            var loadedOnline = false

            if (isOnline) {
                when (val result = gameSetRepository.getGameSet(gameSetIdStr)) {
                    is ApiResult.Success -> {
                        loadedOnline = true
                        val gameSet = result.data
                        val players = gameSet.gameSetPlayers?.values
                            ?.sortedBy { it.position }
                            ?.mapNotNull { it.player } ?: emptyList()
                        settings = gameSet.gameSettings ?: GameSettings.default()

                        val openRound = gameSet.rounds?.sortedBy { it.sequence }?.lastOrNull { !it.completed }
                        gamesInOpenRound = openRound?.marriageGames?.size ?: 0
                        seatOrder = openRound?.playerIds
                            ?.mapNotNull { pid -> players.find { it.id == pid } }
                            ?.takeIf { it.size == players.size }
                            ?: players
                    }
                    else -> {
                        // Will fallback to local mirror below
                    }
                }
            }

            if (!loadedOnline) {
                val gameSetId = gameSetIdStr.toIntOrNull()
                    ?: offlineGameRepository.getGameSetByRemoteId(gameSetIdStr)?.id
                    ?: run {
                        _uiState.value = _uiState.value.copy(error = "Unable to load game data offline")
                        return@launch
                    }
                val players = offlineGameRepository.getGameSetPlayers(gameSetId)
                val gameSetEntity = offlineGameRepository.getGameSet(gameSetId) ?: return@launch
                settings = offlineGameRepository.getGameSettings(gameSetEntity.settingsId) ?: GameSettings.default()

                val openState = offlineGameRepository.getOpenRoundState(gameSetId, players.size)
                gamesInOpenRound = openState.gamesInOpenRound
                seatOrder = openState.seatOrderIds
                    ?.mapNotNull { id -> players.find { it.id == id } }
                    ?.takeIf { it.size == players.size }
                    ?: players
            }

            val dealerIndex = if (seatOrder.isNotEmpty()) (seatOrder.size - 1 + gamesInOpenRound) % seatOrder.size else -1
            val dealer = seatOrder.getOrNull(dealerIndex)

            _uiState.value = _uiState.value.copy(
                gameSetId = gameSetIdStr,
                playerStates = seatOrder.mapIndexed { idx, player ->
                    PlayerRoundState(
                        player = player,
                        isDealer = idx == dealerIndex
                    )
                },
                settings = settings,
                dealerId = dealer?.id,
                gameNumber = gamesInOpenRound + 1
            )
        }
    }

    /** Prefills the screen with an already-played game's inputs so it can be re-scored. */
    private suspend fun loadForEdit(gameSetIdStr: String, editGameId: String, isOnline: Boolean) {
        if (isOnline) {
            when (val result = gameSetRepository.getGameSet(gameSetIdStr)) {
                is ApiResult.Success -> {
                    val gameSet = result.data
                    val players = gameSet.gameSetPlayers?.values
                        ?.sortedBy { it.position }
                        ?.mapNotNull { it.player } ?: emptyList()
                    val settings = gameSet.gameSettings ?: GameSettings.default()

                    val round = gameSet.rounds?.firstOrNull { r -> r.marriageGames?.any { it.id == editGameId } == true }
                    val game = round?.marriageGames?.firstOrNull { it.id == editGameId }
                    if (round == null || game == null) {
                        _uiState.value = _uiState.value.copy(error = "Game not found")
                        return
                    }

                    val seatOrder = round.playerIds
                        ?.mapNotNull { pid -> players.find { it.id == pid } }
                        ?.takeIf { it.size == players.size }
                        ?: players

                    _uiState.value = _uiState.value.copy(
                        gameSetId = gameSetIdStr,
                        editGameId = editGameId,
                        playerStates = seatOrder.map { player ->
                            val score = game.marriageGameScores?.get(player.id)
                            PlayerRoundState(
                                player = player,
                                seen = score?.seen ?: false,
                                seenPoints = score?.maal ?: 0,
                                duply = score?.duply ?: false,
                                isWinner = player.id == game.winnerId,
                                isDealer = player.id == game.dealerId
                            )
                        },
                        settings = settings,
                        winnerId = game.winnerId,
                        dealerId = game.dealerId
                    )
                    calculatePreview()
                }
                is ApiResult.Error -> _uiState.value = _uiState.value.copy(error = result.message)
                is ApiResult.Loading -> {}
            }
        } else {
            val gameSetId = gameSetIdStr.toIntOrNull() ?: return
            val gameId = editGameId.toIntOrNull() ?: return
            val players = offlineGameRepository.getGameSetPlayers(gameSetId)
            val gameSetEntity = offlineGameRepository.getGameSet(gameSetId) ?: return
            val settings = offlineGameRepository.getGameSettings(gameSetEntity.settingsId) ?: GameSettings.default()
            val (game, scores) = offlineGameRepository.getGameWithScores(gameId) ?: run {
                _uiState.value = _uiState.value.copy(error = "Game not found")
                return
            }

            val seatOrder = game.seatOrder.split(",")
                .mapNotNull { id -> players.find { it.id == id } }
                .takeIf { it.size == players.size }
                ?: players

            _uiState.value = _uiState.value.copy(
                gameSetId = gameSetIdStr,
                editGameId = editGameId,
                playerStates = seatOrder.map { player ->
                    val score = scores.find { it.playerId.toString() == player.id }
                    PlayerRoundState(
                        player = player,
                        seen = score?.isSeen ?: false,
                        seenPoints = score?.maal ?: 0,
                        duply = score?.isDublee ?: false,
                        isWinner = player.id == game.winnerId.toString(),
                        isDealer = player.id == game.dealerId.toString()
                    )
                },
                settings = settings,
                winnerId = game.winnerId.toString(),
                dealerId = game.dealerId.toString()
            )
            calculatePreview()
        }
    }

    fun setWinner(playerId: String) {
        val current = _uiState.value
        val newStates = current.playerStates.map {
            val isWinner = it.player.id == playerId
            it.copy(
                isWinner = isWinner,
                seen = if (isWinner) true else it.seen
            )
        }
        _uiState.value = current.copy(
            playerStates = newStates,
            winnerId = playerId,
            // Picking a winner resolves the "Please select a winner" error
            error = null
        )
        calculatePreview()
    }

    fun setDealer(playerId: String) {
        val current = _uiState.value
        val newStates = current.playerStates.map {
            it.copy(isDealer = it.player.id == playerId)
        }
        _uiState.value = current.copy(
            playerStates = newStates,
            dealerId = playerId
        )
    }

    fun toggleSeen(playerId: String) {
        val current = _uiState.value
        val newStates = current.playerStates.map {
            if (it.player.id == playerId && !it.isWinner) {
                val newSeen = !it.seen
                it.copy(
                    seen = newSeen,
                    seenPoints = if (newSeen) it.seenPoints else 0,
                    duply = if (!newSeen) false else it.duply
                )
            } else it
        }
        _uiState.value = current.copy(playerStates = newStates)
        calculatePreview()
    }

    fun toggleDuply(playerId: String) {
        val current = _uiState.value
        val newStates = current.playerStates.map {
            if (it.player.id == playerId) {
                val newDuply = !it.duply
                it.copy(
                    duply = newDuply,
                    seen = if (newDuply) true else it.seen
                )
            } else it
        }
        _uiState.value = current.copy(playerStates = newStates)
        calculatePreview()
    }

    fun setSeenPoints(playerId: String, points: Int) {
        val current = _uiState.value
        val validatedPoints = points.coerceIn(0, 99)
        val newStates = current.playerStates.map {
            if (it.player.id == playerId) it.copy(seenPoints = validatedPoints) else it
        }
        _uiState.value = current.copy(playerStates = newStates)
        calculatePreview()
    }

    /**
     * Increments/decrements seen Maal points by a preset delta (e.g., +3, +5, +8, +10).
     * If player hasn't seen joker yet and delta > 0, seen is automatically marked true.
     */
    fun addMaalPoints(playerId: String, delta: Int) {
        val current = _uiState.value
        val newStates = current.playerStates.map {
            if (it.player.id == playerId) {
                val newSeen = if (delta > 0) true else it.seen
                val newPoints = (it.seenPoints + delta).coerceIn(0, 99)
                it.copy(
                    seen = newSeen,
                    seenPoints = if (newSeen) newPoints else 0
                )
            } else it
        }
        _uiState.value = current.copy(playerStates = newStates)
        calculatePreview()
    }

    fun setUnseenPoints(playerId: String, points: Int) {
        // No longer used, but kept for compatibility or set to 0
        val current = _uiState.value
        val newStates = current.playerStates.map {
            if (it.player.id == playerId) it.copy(unseenPoints = 0) else it
        }
        _uiState.value = current.copy(playerStates = newStates)
        calculatePreview()
    }

    /** Applies the Maal calculator result: stores counts and sets the player's Maal total. */
    fun applyMaalCounts(playerId: String, counts: Map<MaalItem, Int>, total: Int) {
        val current = _uiState.value
        _uiState.value = current.copy(maalCounts = current.maalCounts + (playerId to counts))
        setSeenPoints(playerId, total)
    }

    private fun calculatePreview() {
        val state = _uiState.value
        if (state.winnerId == null) return

        // Local calculation (mirrors C# ScoringEngine logic)
        val settings = state.settings
        val players = state.playerStates.toMutableList()
        val winnerIdx = players.indexOfFirst { it.isWinner }
        if (winnerIdx < 0) return

        // Copy maal values and apply game mode
        val maalValues = players.map { if (it.seen) it.seenPoints else 0 }.toMutableList()
        val seenFlags = players.map { it.seen || it.isWinner }.toMutableList()

        // Dublee winner rule: their maal counts DUBLEE_WINNER_MAAL_BONUS above
        // the maal they actually held (mirrors C# ScoringEngine).
        if (players[winnerIdx].duply && settings.dublee) {
            maalValues[winnerIdx] += DUBLEE_WINNER_MAAL_BONUS
        }

        if (settings.kidnap) {
            for (i in players.indices) {
                if (!seenFlags[i] && !players[i].isWinner) {
                    maalValues[winnerIdx] += maalValues[i]
                    maalValues[i] = 0
                }
            }
        } else if (settings.murder) {
            for (i in players.indices) {
                if (!seenFlags[i] && !players[i].isWinner) {
                    maalValues[i] = 0
                }
            }
        }

        val scores = MutableList(players.size) { 0 }

        // Fixed penalties. A seen loser playing dublee is exempt from the seen penalty.
        for (i in players.indices) {
            if (players[i].isWinner) continue
            val penalty = when {
                !seenFlags[i] -> settings.unseenPoint
                players[i].duply && settings.dublee -> 0
                else -> settings.seenPoint
            }
            scores[i] -= penalty
            scores[winnerIdx] += penalty
        }

        // Maal distribution
        val seenIndices = players.indices.filter { seenFlags[it] }
        val unseenIndices = players.indices.filter { !seenFlags[it] && !players[it].isWinner }

        for (u in unseenIndices) {
            for (s in seenIndices) {
                val diff = maalValues[s] - maalValues[u]
                scores[s] += diff
                scores[u] -= diff
            }
        }

        for (i in seenIndices.indices) {
            for (j in i + 1 until seenIndices.size) {
                val a = seenIndices[i]
                val b = seenIndices[j]
                val diff = maalValues[a] - maalValues[b]
                scores[a] += diff
                scores[b] -= diff
            }
        }

        val updatedStates = players.mapIndexed { idx, ps ->
            ps.copy(
                previewScore = scores[idx],
                previewMoney = scores[idx] * settings.pointRate
            )
        }

        _uiState.value = state.copy(playerStates = updatedStates, showPreview = true)
    }

    fun submitRound() {
        val state = _uiState.value
        val gameSetIdStr = state.gameSetId ?: return
        val winnerId = state.winnerId ?: run {
            _uiState.value = state.copy(error = "Please select a winner")
            return
        }

        val isLocalId = gameSetIdStr.toIntOrNull() != null
        val isOnline = sessionManager.isOnlineMode() && !isLocalId

        viewModelScope.launch {
            if (isOnline) {
                val request = SubmitRoundRequest(
                    winnerId = winnerId,
                    dealerId = state.dealerId ?: "",
                    players = state.playerStates.map { ps ->
                        val isPlayerWinner = ps.player.id == winnerId
                        RoundPlayerInput(
                            playerId = ps.player.id,
                            seen = ps.seen || isPlayerWinner,
                            duply = ps.duply,
                            maal = if (ps.seen) ps.seenPoints else 0
                        )
                    }
                )
                val result = if (state.editGameId != null) {
                    gameSetRepository.updateGame(gameSetIdStr, state.editGameId, request)
                } else {
                    gameSetRepository.submitRound(gameSetIdStr, request)
                }
                when (result) {
                    is ApiResult.Success -> {
                        saveLocalMirror(gameSetIdStr, state, winnerId, synced = true, remoteId = result.data.id)
                        _uiState.value = state.copy(submitted = true, error = null)
                    }
                    is ApiResult.Error -> {
                        val savedLocally = saveLocalMirror(gameSetIdStr, state, winnerId, synced = false, remoteId = null)
                        if (savedLocally) {
                            _uiState.value = state.copy(submitted = true, error = null)
                        } else {
                            _uiState.value = state.copy(error = "Failed to save round: ${result.message}")
                        }
                    }
                    is ApiResult.Loading -> {}
                }
                return@launch
            }

            val gameSetId = gameSetIdStr.toIntOrNull() ?: return@launch
            try {
                val scores = state.playerStates.map { ps ->
                    val isPlayerWinner = ps.player.id == state.winnerId
                    // Persist the dublee winner's maal with the fixed +5 bonus applied,
                    // matching what the C# ScoringEngine stores in online mode.
                    val dubleeBonus =
                        if (isPlayerWinner && ps.duply && state.settings.dublee) DUBLEE_WINNER_MAAL_BONUS else 0
                    np.com.sanjeeb.marriagecalculator.data.repository.RoundScoreData(
                        playerId = ps.player.id.toInt(),
                        score = ps.previewScore,
                        maal = (if (ps.seen) ps.seenPoints else 0) + dubleeBonus,
                        isSeen = ps.seen || isPlayerWinner,
                        isWinner = isPlayerWinner,
                        isDublee = ps.duply
                    )
                }
                val totalMaal = scores.filter { it.isSeen }.sumOf { it.maal }

                val editGameId = state.editGameId?.toIntOrNull()
                if (editGameId != null) {
                    offlineGameRepository.updateGame(
                        gameId = editGameId,
                        winnerId = winnerId.toIntOrNull() ?: return@launch,
                        totalMaal = totalMaal,
                        playerScores = scores
                    )
                } else {
                    offlineGameRepository.saveRound(
                        gameSetId = gameSetId,
                        winnerId = winnerId.toIntOrNull() ?: return@launch,
                        dealerId = state.dealerId?.toIntOrNull() ?: 0,
                        totalMaal = totalMaal,
                        playerScores = scores
                    )
                }

                _uiState.value = state.copy(submitted = true, error = null)
            } catch (e: Exception) {
                _uiState.value = state.copy(error = "Failed to save round: ${e.message}")
            }
        }
    }

    private suspend fun saveLocalMirror(
        gameSetRemoteId: String,
        state: RoundInputUiState,
        winnerId: String,
        synced: Boolean,
        remoteId: String?
    ): Boolean {
        return try {
            val localGameSet = offlineGameRepository.getGameSetByRemoteId(gameSetRemoteId) ?: return false
            val scores = state.playerStates.mapNotNull { ps ->
                val localPlayerId = ps.player.id.toIntOrNull()
                    ?: offlineGameRepository.getPlayerEntityByName(ps.player.name)?.id
                    ?: return@mapNotNull null
                val isPlayerWinner = ps.player.id == winnerId
                val dubleeBonus =
                    if (isPlayerWinner && ps.duply && state.settings.dublee) DUBLEE_WINNER_MAAL_BONUS else 0
                np.com.sanjeeb.marriagecalculator.data.repository.RoundScoreData(
                    playerId = localPlayerId,
                    score = ps.previewScore,
                    maal = (if (ps.seen) ps.seenPoints else 0) + dubleeBonus,
                    isSeen = ps.seen || isPlayerWinner,
                    isWinner = isPlayerWinner,
                    isDublee = ps.duply
                )
            }
            if (scores.isEmpty()) return false
            val totalMaal = scores.filter { it.isSeen }.sumOf { it.maal }
            val localWinnerId = scores.find { it.isWinner }?.playerId ?: scores.first().playerId
            val dealerState = state.playerStates.find { it.player.id == state.dealerId }
            val localDealerId: Int = state.dealerId?.toIntOrNull()
                ?: dealerState?.player?.name?.let { offlineGameRepository.getPlayerEntityByName(it)?.id }
                ?: 0

            offlineGameRepository.saveRound(
                gameSetId = localGameSet.id,
                winnerId = localWinnerId,
                dealerId = localDealerId,
                totalMaal = totalMaal,
                playerScores = scores,
                synced = synced,
                remoteId = remoteId
            )
            true
        } catch (e: Exception) {
            false
        }
    }

    fun clearError() {
        _uiState.value = _uiState.value.copy(error = null)
    }
}
