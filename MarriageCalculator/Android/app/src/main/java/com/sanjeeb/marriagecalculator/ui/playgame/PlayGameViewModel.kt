package com.sanjeeb.marriagecalculator.ui.playgame

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.sanjeeb.marriagecalculator.data.model.Player
import com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class PlayerStandings(
    val player: Player,
    val netPoints: Int = 0,
    val isNextDealer: Boolean = false
)

data class RoundItem(
    val roundId: Int,
    val roundNumber: Int,
    val winnerName: String,
    val totalMaal: Int,
    val winnerScore: Int
)

data class PlayGameUiState(
    val gameName: String = "",
    val players: List<PlayerStandings> = emptyList(),
    val rounds: List<RoundItem> = emptyList(),
    val nextDealerName: String = "",
    val isSettled: Boolean = false,
    val isLoading: Boolean = false
)

@HiltViewModel
class PlayGameViewModel @Inject constructor(
    private val offlineGameRepository: OfflineGameRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(PlayGameUiState())
    val uiState: StateFlow<PlayGameUiState> = _uiState.asStateFlow()

    fun loadGame(gameSetIdStr: String) {
        val gameSetId = gameSetIdStr.toIntOrNull() ?: return
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            val gameSet = offlineGameRepository.getGameSet(gameSetId) ?: return@launch
            val players = offlineGameRepository.getGameSetPlayers(gameSetId)

            offlineGameRepository.getRounds(gameSetId).collect { roundEntities ->
                val allScores = offlineGameRepository.getAllScoresForGameSet(gameSetId)
                val scoresByRound = allScores.groupBy { it.roundId }

                val roundsList = roundEntities.map { r ->
                    val roundScores = scoresByRound[r.id] ?: emptyList()
                    val winnerScore = roundScores.find { it.isWinner }?.score ?: 0
                    val winnerName = players.find { it.id == roundScores.find { it.isWinner }?.playerId?.toString() }?.name ?: "Unknown"

                    RoundItem(
                        roundId = r.id,
                        roundNumber = r.roundNumber,
                        winnerName = winnerName,
                        totalMaal = r.totalMaal,
                        winnerScore = winnerScore
                    )
                }

                val nextDealerIndex = if (players.isNotEmpty()) roundsList.size % players.size else 0
                val nextDealer = players.getOrNull(nextDealerIndex)

                val standings = players.mapIndexed { index, p ->
                    val pScores = allScores.filter { it.playerId == p.id.toInt() }
                    PlayerStandings(
                        player = p,
                        netPoints = pScores.sumOf { it.score },
                        isNextDealer = index == nextDealerIndex
                    )
                }

                _uiState.value = PlayGameUiState(
                    gameName = "Game Set #$gameSetId",
                    players = standings,
                    rounds = roundsList,
                    nextDealerName = nextDealer?.name ?: "None",
                    isSettled = gameSet.isSettled,
                    isLoading = false
                )
            }
        }
    }
}
