package com.sanjeeb.marriagecalculator.ui.playgame

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.sanjeeb.marriagecalculator.data.model.Player
import com.sanjeeb.marriagecalculator.data.model.User
import com.sanjeeb.marriagecalculator.data.model.UpdatePlayerRequest
import com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import com.sanjeeb.marriagecalculator.data.repository.GameSetRepository
import com.sanjeeb.marriagecalculator.data.repository.PlayerRepository
import com.sanjeeb.marriagecalculator.data.repository.FriendRepository
import com.sanjeeb.marriagecalculator.data.repository.SessionManager
import com.sanjeeb.marriagecalculator.data.repository.ApiResult
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
    val isLoading: Boolean = false,
    val isHost: Boolean = true,
    val isOnlineMode: Boolean = false,
    val friendsList: List<User> = emptyList(),
    val error: String? = null
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
        val isOnline = sessionManager.isOnlineMode()
        _uiState.value = _uiState.value.copy(isLoading = true, isOnlineMode = isOnline, error = null)

        if (isOnline) {
            viewModelScope.launch {
                when (val result = gameSetRepository.getGameSet(gameSetIdStr)) {
                    is ApiResult.Success -> {
                        val gameSet = result.data
                        val gameSetPlayers = gameSet.gameSetPlayers?.values?.toList() ?: emptyList()
                        val players = gameSetPlayers.mapNotNull { it.player }
                        
                        val roundsList = gameSet.rounds?.map { r ->
                            val firstGame = r.marriageGames?.firstOrNull()
                            val winnerName = players.find { it.id == firstGame?.winnerId }?.name ?: "Unknown"
                            val totalMaal = firstGame?.totalMaal ?: 0
                            val winnerScoreMap = r.totalScore ?: emptyMap()
                            val winnerScore = winnerScoreMap[firstGame?.winnerId ?: ""]?.toInt() ?: 0
                            
                            RoundItem(
                                roundId = r.id.hashCode(),
                                roundNumber = r.sequence,
                                winnerName = winnerName,
                                totalMaal = totalMaal,
                                winnerScore = winnerScore
                            )
                        } ?: emptyList()

                        val nextDealerIndex = if (players.isNotEmpty()) roundsList.size % players.size else 0
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
                                isNextDealer = index == nextDealerIndex
                            )
                        }

                        val isCurrentUserHost = gameSet.hostUserId == sessionManager.getUserProfile()?.userId

                        _uiState.value = PlayGameUiState(
                            gameName = gameSet.name,
                            players = standings,
                            rounds = roundsList,
                            nextDealerName = nextDealer?.name ?: "None",
                            isSettled = !gameSet.isActive,
                            isLoading = false,
                            isHost = isCurrentUserHost,
                            isOnlineMode = true,
                            friendsList = emptyList()
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
                        isLoading = false,
                        isHost = true,
                        isOnlineMode = false
                    )
                }
            }
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

    fun clearError() {
        _uiState.value = _uiState.value.copy(error = null)
    }
}
