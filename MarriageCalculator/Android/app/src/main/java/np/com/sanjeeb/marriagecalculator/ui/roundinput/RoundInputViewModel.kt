package np.com.sanjeeb.marriagecalculator.ui.roundinput

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import np.com.sanjeeb.marriagecalculator.data.model.GameSettings
import np.com.sanjeeb.marriagecalculator.data.model.MaalItem
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.data.remote.*
import np.com.sanjeeb.marriagecalculator.data.repository.ApiResult
import np.com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
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
    val playerStates: List<PlayerRoundState> = emptyList(),
    val settings: GameSettings = GameSettings.default(),
    val winnerId: String? = null,
    val dealerId: String? = null,
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
    private val offlineGameRepository: OfflineGameRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(RoundInputUiState())
    val uiState: StateFlow<RoundInputUiState> = _uiState.asStateFlow()

    fun initPlayers(players: List<Player>, settings: GameSettings) {
        _uiState.value = _uiState.value.copy(
            playerStates = players.map { PlayerRoundState(player = it) },
            settings = settings
        )
    }

    fun loadGameData(gameSetIdStr: String, roundNumber: Int) {
        viewModelScope.launch {
            val gameSetId = gameSetIdStr.toIntOrNull() ?: return@launch
            val players = offlineGameRepository.getGameSetPlayers(gameSetId)
            val gameSetEntity = offlineGameRepository.getGameSet(gameSetId) ?: return@launch
            val settings = offlineGameRepository.getGameSettings(gameSetEntity.settingsId) ?: GameSettings.default()

            val dealerIndex = if (players.isNotEmpty()) (roundNumber - 2 + players.size) % players.size else -1
            val dealer = players.getOrNull(dealerIndex)

            _uiState.value = _uiState.value.copy(
                gameSetId = gameSetIdStr,
                playerStates = players.mapIndexed { idx, player ->
                    PlayerRoundState(
                        player = player,
                        isDealer = idx == dealerIndex
                    )
                },
                settings = settings,
                dealerId = dealer?.id
            )
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
            winnerId = playerId
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
                    seenPoints = if (newSeen) it.seenPoints else 0
                )
            } else it
        }
        _uiState.value = current.copy(playerStates = newStates)
        calculatePreview()
    }

    fun toggleDuply(playerId: String) {
        val current = _uiState.value
        val newStates = current.playerStates.map {
            if (it.player.id == playerId) it.copy(duply = !it.duply) else it
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

        // Fixed penalties
        for (i in players.indices) {
            if (players[i].isWinner) continue
            val penalty = if (!seenFlags[i]) settings.unseenPoint else settings.seenPoint
            val bonus = if (players[winnerIdx].duply && settings.dublee) settings.dubleePointBonus else 0
            val total = penalty + bonus
            scores[i] -= total
            scores[winnerIdx] += total
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
        val gameSetId = gameSetIdStr.toIntOrNull() ?: return
        val winnerId = state.winnerId?.toIntOrNull() ?: run {
            _uiState.value = state.copy(error = "Please select a winner")
            return
        }

        viewModelScope.launch {
            try {
                val totalMaal = state.playerStates.sumOf { if (it.seen) it.seenPoints else 0 }

                val scores = state.playerStates.map { ps ->
                    val isPlayerWinner = ps.player.id == state.winnerId
                    np.com.sanjeeb.marriagecalculator.data.repository.RoundScoreData(
                        playerId = ps.player.id.toInt(),
                        score = ps.previewScore,
                        maal = if (ps.seen) ps.seenPoints else 0,
                        isSeen = ps.seen || isPlayerWinner,
                        isWinner = isPlayerWinner,
                        isDublee = ps.duply
                    )
                }

                offlineGameRepository.saveRound(
                    gameSetId = gameSetId,
                    winnerId = winnerId,
                    totalMaal = totalMaal,
                    playerScores = scores
                )

                _uiState.value = state.copy(submitted = true, error = null)
            } catch (e: Exception) {
                _uiState.value = state.copy(error = "Failed to save round: ${e.message}")
            }
        }
    }

    fun clearError() {
        _uiState.value = _uiState.value.copy(error = null)
    }
}
