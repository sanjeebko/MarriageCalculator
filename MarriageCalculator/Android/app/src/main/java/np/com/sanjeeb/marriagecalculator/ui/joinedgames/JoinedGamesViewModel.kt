package np.com.sanjeeb.marriagecalculator.ui.joinedgames

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import np.com.sanjeeb.marriagecalculator.data.model.*
import np.com.sanjeeb.marriagecalculator.data.repository.ApiResult
import np.com.sanjeeb.marriagecalculator.data.repository.GameSetRepository
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import np.com.sanjeeb.marriagecalculator.ui.dashboard.EnrichedActiveGame
import javax.inject.Inject

data class JoinedGamesCareerStats(
    val totalGamesPlayed: Int = 0,
    val totalMatchesJoined: Int = 0,
    val totalWins: Int = 0,
    val netMoney: Double = 0.0,
    val netPoints: Int = 0,
    val currency: Currency = Currency.NPR_Rupee
)

enum class JoinedGamesFilter {
    ACTIVE,
    ALL
}

data class JoinedGamesUiState(
    val isLoading: Boolean = false,
    val error: String? = null,
    val joinedGames: List<EnrichedActiveGame> = emptyList(),
    val filteredGames: List<EnrichedActiveGame> = emptyList(),
    val selectedFilter: JoinedGamesFilter = JoinedGamesFilter.ACTIVE,
    val careerStats: JoinedGamesCareerStats = JoinedGamesCareerStats(),
    val currentUser: User? = null
)

@HiltViewModel
class JoinedGamesViewModel @Inject constructor(
    private val gameSetRepository: GameSetRepository,
    private val sessionManager: SessionManager
) : ViewModel() {

    private val _uiState = MutableStateFlow(JoinedGamesUiState())
    val uiState: StateFlow<JoinedGamesUiState> = _uiState.asStateFlow()

    init {
        val user = sessionManager.getUserProfile()
        _uiState.value = _uiState.value.copy(currentUser = user)
        loadJoinedGames()
    }

    fun setFilter(filter: JoinedGamesFilter) {
        val games = _uiState.value.joinedGames
        val filtered = if (filter == JoinedGamesFilter.ACTIVE) {
            games.filter { !it.isSettled }
        } else {
            games
        }
        _uiState.value = _uiState.value.copy(
            selectedFilter = filter,
            filteredGames = filtered
        )
    }

    fun loadJoinedGames() {
        if (sessionManager.isGuestMode() || !sessionManager.isOnlineMode()) {
            _uiState.value = _uiState.value.copy(
                isLoading = false,
                joinedGames = emptyList(),
                filteredGames = emptyList(),
                careerStats = JoinedGamesCareerStats(),
                currentUser = sessionManager.getUserProfile(),
                error = null
            )
            return
        }
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            val currentUser = sessionManager.getUserProfile()
            val myUserId = currentUser?.userId ?: ""
            val myEmail = currentUser?.email ?: ""

            when (val result = gameSetRepository.getJoinedGameSets()) {
                is ApiResult.Success -> {
                    val rawGames = result.data
                    val enriched = rawGames.map { game ->
                        enrichJoinedGame(game, myUserId, myEmail)
                    }

                    val stats = computeCareerStats(enriched)
                    val filtered = if (_uiState.value.selectedFilter == JoinedGamesFilter.ACTIVE) {
                        enriched.filter { !it.isSettled }
                    } else {
                        enriched
                    }

                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        joinedGames = enriched,
                        filteredGames = filtered,
                        careerStats = stats
                    )
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
    }

    private fun enrichJoinedGame(game: MarriageGameSet, myUserId: String, myEmail: String): EnrichedActiveGame {
        val settings = game.gameSettings ?: GameSettings()
        val players = game.gameSetPlayers?.values?.mapNotNull { it.player } ?: emptyList()
        val rounds = game.rounds ?: emptyList()

        val standings = players.map { p ->
            var netPoints = 0
            rounds.forEach { r ->
                val scoreMap = r.totalScore ?: emptyMap()
                netPoints += scoreMap[p.id]?.toInt() ?: 0
            }
            val money = netPoints * settings.pointRate
            Triple(p, netPoints, money)
        }.sortedByDescending { it.third }

        val highest = standings.firstOrNull()

        // Find current user among players
        val myEntryIndex = standings.indexOfFirst {
            (myEmail.isNotEmpty() && it.first.email.equals(myEmail, ignoreCase = true)) ||
            it.first.id == myUserId
        }

        val myStanding = if (myEntryIndex != -1) standings[myEntryIndex] else null
        val myRank = if (myEntryIndex != -1) myEntryIndex + 1 else null

        val totalGames = rounds.sumOf { it.marriageGames?.size ?: 0 }
        val roundStatus = if (rounds.isEmpty()) "Not started" else if (game.isActive) "Round ${rounds.size} in progress" else "Completed"

        val suits = listOf("♠", "♥", "♦", "♣")
        val cardSuit = suits[kotlin.math.abs(game.id.hashCode()) % suits.size]

        return EnrichedActiveGame(
            id = game.id,
            name = game.name.ifEmpty { "Marriage Game" },
            lastPlayed = game.lastPlayed.take(10).ifEmpty { "Recent" },
            players = players,
            leaderName = highest?.first?.name?.takeIf { highest.third > 0.0 },
            leaderScoreText = highest?.third?.let { if (it > 0.0) "+${settings.currency.formatMoney(it)}" else null },
            roundStatusText = roundStatus,
            totalGamesPlayed = totalGames,
            isSettled = !game.isActive,
            cardSuit = cardSuit,
            hostUserName = game.hostUserName?.takeIf { it.isNotBlank() } ?: "Host",
            hostUserId = game.hostUserId,
            myMoney = myStanding?.third,
            myNetPoints = myStanding?.second,
            myRank = myRank,
            isHost = false
        )
    }

    private fun computeCareerStats(games: List<EnrichedActiveGame>): JoinedGamesCareerStats {
        var totalGamesPlayed = 0
        var totalWins = 0
        var netMoney = 0.0
        var netPoints = 0

        for (g in games) {
            totalGamesPlayed += g.totalGamesPlayed
            if (g.myRank == 1 && (g.myMoney ?: 0.0) > 0.0) {
                totalWins++
            }
            netMoney += g.myMoney ?: 0.0
            netPoints += g.myNetPoints ?: 0
        }

        return JoinedGamesCareerStats(
            totalGamesPlayed = totalGamesPlayed,
            totalMatchesJoined = games.size,
            totalWins = totalWins,
            netMoney = netMoney,
            netPoints = netPoints
        )
    }
}
