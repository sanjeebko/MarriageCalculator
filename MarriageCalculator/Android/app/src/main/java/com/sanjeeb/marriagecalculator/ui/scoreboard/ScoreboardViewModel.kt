package com.sanjeeb.marriagecalculator.ui.scoreboard

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.sanjeeb.marriagecalculator.data.model.GameSettings
import com.sanjeeb.marriagecalculator.data.model.Player
import com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
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
    val winnerId: String,
    val winnerName: String,
    val scores: Map<String, Int> = emptyMap(), // playerId -> score
    val totalMaal: Int = 0
)

data class ScoreboardUiState(
    val gameSetId: String? = null,
    val players: List<PlayerTotalScore> = emptyList(),
    val rounds: List<RoundSummary> = emptyList(),
    val settings: GameSettings = GameSettings.default(),
    val isLoading: Boolean = false,
    val showHistory: Boolean = false,
    val isSettled: Boolean = false
)

@HiltViewModel
class ScoreboardViewModel @Inject constructor(
    private val offlineGameRepository: OfflineGameRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(ScoreboardUiState())
    val uiState: StateFlow<ScoreboardUiState> = _uiState.asStateFlow()

    fun initScoreboard(players: List<Player>, settings: GameSettings) {
        _uiState.value = _uiState.value.copy(
            players = players.map { PlayerTotalScore(player = it) },
            settings = settings
        )
    }

    fun loadScoreboardData(gameSetIdStr: String) {
        val gameSetId = gameSetIdStr.toIntOrNull() ?: return
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, gameSetId = gameSetIdStr)
            val players = offlineGameRepository.getGameSetPlayers(gameSetId)
            val gameSet = offlineGameRepository.getGameSet(gameSetId) ?: return@launch
            val settings = offlineGameRepository.getGameSettings(gameSet.settingsId) ?: GameSettings.default()

            offlineGameRepository.getRounds(gameSetId).collect { roundEntities ->
                val allScores = offlineGameRepository.getAllScoresForGameSet(gameSetId)
                val scoresByRound = allScores.groupBy { it.roundId }

                val roundsList = roundEntities.map { r ->
                    val roundScores = scoresByRound[r.id] ?: emptyList()
                    val winnerScore = roundScores.find { it.isWinner }
                    val winnerName = players.find { it.id == winnerScore?.playerId?.toString() }?.name ?: "Unknown"

                    RoundSummary(
                        roundNumber = r.roundNumber,
                        winnerId = r.winnerId.toString(),
                        winnerName = winnerName,
                        scores = roundScores.associate { it.playerId.toString() to it.score },
                        totalMaal = r.totalMaal
                    )
                }

                val playerTotalScores = players.map { p ->
                    val pId = p.id.toInt()
                    val pScores = allScores.filter { it.playerId == pId }
                    val totalPoints = pScores.sumOf { it.score }
                    val gamesPlayed = pScores.size
                    val gamesWon = pScores.count { it.isWinner }

                    PlayerTotalScore(
                        player = p,
                        totalPoints = totalPoints,
                        totalMoney = totalPoints * settings.pointRate,
                        gamesPlayed = gamesPlayed,
                        gamesWon = gamesWon,
                        roundScores = pScores.map { it.score }
                    )
                }

                _uiState.value = _uiState.value.copy(
                    players = playerTotalScores,
                    rounds = roundsList,
                    settings = settings,
                    isSettled = gameSet.isSettled,
                    isLoading = false
                )
            }
        }
    }

    fun toggleHistory() {
        _uiState.value = _uiState.value.copy(showHistory = !_uiState.value.showHistory)
    }

    fun settleGame() {
        val gameSetIdStr = _uiState.value.gameSetId ?: return
        val gameSetId = gameSetIdStr.toIntOrNull() ?: return
        viewModelScope.launch {
            offlineGameRepository.settleGame(gameSetId)
            _uiState.value = _uiState.value.copy(isSettled = true)
        }
    }
}
