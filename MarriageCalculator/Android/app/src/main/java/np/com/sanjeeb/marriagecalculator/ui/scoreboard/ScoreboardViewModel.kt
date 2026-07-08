package np.com.sanjeeb.marriagecalculator.ui.scoreboard

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import np.com.sanjeeb.marriagecalculator.data.model.GameSettings
import np.com.sanjeeb.marriagecalculator.data.model.Player
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

data class PlayerTotalScore(
    val player: Player,
    val totalPoints: Int = 0,
    val totalMoney: Double = 0.0,
    val gamesPlayed: Int = 0,
    val gamesWon: Int = 0,
    val roundScores: List<Int> = emptyList()
)

data class RoundPlayerEntry(
    val playerId: String,
    val playerName: String,
    val isSeen: Boolean,
    val isDublee: Boolean,
    val maal: Int,
    val score: Int,
    val money: Double
)

data class RoundSummary(
    val roundNumber: Int,
    val winnerId: String,
    val winnerName: String,
    val playerEntries: List<RoundPlayerEntry> = emptyList(),
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
    private val offlineGameRepository: OfflineGameRepository,
    private val gameSetRepository: GameSetRepository,
    private val sessionManager: SessionManager
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
        val isLocalId = gameSetIdStr.toIntOrNull() != null
        val isOnline = sessionManager.isOnlineMode() && !isLocalId

        if (isOnline) {
            loadOnlineScoreboardData(gameSetIdStr)
            return
        }

        val gameSetId = gameSetIdStr.toIntOrNull() ?: return
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, gameSetId = gameSetIdStr)
            val players = offlineGameRepository.getGameSetPlayers(gameSetId)
            val gameSet = offlineGameRepository.getGameSet(gameSetId) ?: return@launch
            val settings = offlineGameRepository.getGameSettings(gameSet.settingsId) ?: GameSettings.default()

            offlineGameRepository.getRounds(gameSetId).collect { roundEntities ->
                val allScores = offlineGameRepository.getAllScoresForGameSet(gameSetId)
                val scoresByRound = allScores.groupBy { it.roundId }

                val roundsList = roundEntities.sortedBy { it.roundNumber }.map { r ->
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
                            maal = pScore?.maal ?: 0,
                            score = pScore?.score ?: 0,
                            money = (pScore?.score ?: 0) * settings.pointRate
                        )
                    }

                    RoundSummary(
                        roundNumber = r.roundNumber,
                        winnerId = r.winnerId.toString(),
                        winnerName = winnerName,
                        playerEntries = playerEntries,
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

    private fun loadOnlineScoreboardData(gameSetIdStr: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, gameSetId = gameSetIdStr)

            when (val result = gameSetRepository.getGameSet(gameSetIdStr)) {
                is ApiResult.Success -> {
                    val gameSet = result.data
                    val players = gameSet.gameSetPlayers?.values
                        ?.sortedBy { it.position }
                        ?.mapNotNull { it.player } ?: emptyList()
                    val settings = gameSet.gameSettings ?: GameSettings.default()
                    val rounds = gameSet.rounds ?: emptyList()

                    val roundsList = rounds.sortedBy { it.sequence }.map { r ->
                        val firstGame = r.marriageGames?.firstOrNull()
                        val winnerId = firstGame?.winnerId ?: ""
                        val winnerName = players.find { it.id == winnerId }?.name ?: "Unknown"

                        val playerEntries = players.map { p ->
                            val score = firstGame?.marriageGameScores?.get(p.id)
                            RoundPlayerEntry(
                                playerId = p.id,
                                playerName = p.name,
                                isSeen = score?.seen ?: false,
                                isDublee = score?.duply ?: false,
                                maal = score?.maal ?: 0,
                                score = score?.score ?: 0,
                                money = (score?.score ?: 0) * settings.pointRate
                            )
                        }

                        RoundSummary(
                            roundNumber = r.sequence,
                            winnerId = winnerId,
                            winnerName = winnerName,
                            playerEntries = playerEntries,
                            totalMaal = firstGame?.totalMaal ?: 0
                        )
                    }

                    val playerTotalScores = players.map { p ->
                        val roundScoresForPlayer = rounds.mapNotNull { r -> r.totalScore?.get(p.id)?.toInt() }
                        val totalPoints = roundScoresForPlayer.sum()
                        val gamesWon = rounds.count { it.marriageGames?.firstOrNull()?.winnerId == p.id }

                        PlayerTotalScore(
                            player = p,
                            totalPoints = totalPoints,
                            totalMoney = totalPoints * settings.pointRate,
                            gamesPlayed = rounds.size,
                            gamesWon = gamesWon,
                            roundScores = roundScoresForPlayer
                        )
                    }

                    _uiState.value = _uiState.value.copy(
                        players = playerTotalScores,
                        rounds = roundsList,
                        settings = settings,
                        isSettled = !gameSet.isActive,
                        isLoading = false
                    )
                }
                is ApiResult.Error -> {
                    _uiState.value = _uiState.value.copy(isLoading = false)
                }
                is ApiResult.Loading -> {}
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
