package com.sanjeeb.marriagecalculator.ui.scoreboard

import androidx.lifecycle.ViewModel
import com.sanjeeb.marriagecalculator.data.model.GameSettings
import com.sanjeeb.marriagecalculator.data.model.Player
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

data class PlayerTotalScore(
    val player: Player,
    val totalPoints: Int = 0,
    val totalMoney: Double = 0.0,
    val gamesPlayed: Int = 0,
    val gamesWon: Int = 0,
    val roundScores: List<Int> = emptyList()
)

data class RoundSummary(
    val roundNumber: Int,
    val winnerId: Int,
    val winnerName: String,
    val scores: Map<Int, Int> = emptyMap(), // playerId -> score
    val totalMaal: Int = 0
)

data class ScoreboardUiState(
    val players: List<PlayerTotalScore> = emptyList(),
    val rounds: List<RoundSummary> = emptyList(),
    val settings: GameSettings = GameSettings.default(),
    val isLoading: Boolean = false,
    val showHistory: Boolean = false,
    val isSettled: Boolean = false
)

@HiltViewModel
class ScoreboardViewModel @Inject constructor() : ViewModel() {

    private val _uiState = MutableStateFlow(ScoreboardUiState())
    val uiState: StateFlow<ScoreboardUiState> = _uiState.asStateFlow()

    fun initScoreboard(players: List<Player>, settings: GameSettings) {
        _uiState.value = _uiState.value.copy(
            players = players.map { PlayerTotalScore(player = it) },
            settings = settings
        )
    }

    fun addRoundResult(winnerId: Int, scores: Map<Int, Int>, totalMaal: Int) {
        val state = _uiState.value
        val roundNum = state.rounds.size + 1
        val winnerName = state.players.find { it.player.id == winnerId }?.player?.name ?: "Unknown"

        val newRound = RoundSummary(
            roundNumber = roundNum,
            winnerId = winnerId,
            winnerName = winnerName,
            scores = scores,
            totalMaal = totalMaal
        )

        val updatedPlayers = state.players.map { ps ->
            val roundScore = scores[ps.player.id] ?: 0
            ps.copy(
                totalPoints = ps.totalPoints + roundScore,
                totalMoney = (ps.totalPoints + roundScore) * state.settings.pointRate,
                gamesPlayed = ps.gamesPlayed + 1,
                gamesWon = ps.gamesWon + if (ps.player.id == winnerId) 1 else 0,
                roundScores = ps.roundScores + roundScore
            )
        }

        _uiState.value = state.copy(
            players = updatedPlayers,
            rounds = state.rounds + newRound
        )
    }

    fun toggleHistory() {
        _uiState.value = _uiState.value.copy(showHistory = !_uiState.value.showHistory)
    }

    fun settleGame() {
        _uiState.value = _uiState.value.copy(isSettled = true)
    }
}
