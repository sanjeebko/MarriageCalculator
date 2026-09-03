package np.com.sanjeeb.marriagecalculator.ui.dashboard

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import np.com.sanjeeb.marriagecalculator.data.model.GameSettings
import np.com.sanjeeb.marriagecalculator.data.model.MarriageGameSet
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.data.model.User
import np.com.sanjeeb.marriagecalculator.data.model.UserCareerStats
import np.com.sanjeeb.marriagecalculator.data.repository.ApiResult
import np.com.sanjeeb.marriagecalculator.data.repository.FriendRepository
import np.com.sanjeeb.marriagecalculator.data.repository.GameSetRepository
import np.com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import np.com.sanjeeb.marriagecalculator.data.repository.ThemePreference
import np.com.sanjeeb.marriagecalculator.ui.theme.AppThemeOption
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch
import javax.inject.Inject

data class DashboardUiState(
    val isLoading: Boolean = false,
    val activeGames: List<MarriageGameSet> = emptyList(),
    val enrichedGames: List<EnrichedActiveGame> = emptyList(),
    val user: User? = null,
    val careerStats: UserCareerStats = UserCareerStats(),
    val recentPlayers: List<Player> = emptyList(),
    val isQuickStarting: Boolean = false,
    val error: String? = null,
    val isOfflineMode: Boolean = true
)

@HiltViewModel
class DashboardViewModel @Inject constructor(
    private val gameSetRepository: GameSetRepository,
    private val offlineGameRepository: OfflineGameRepository,
    private val friendRepository: FriendRepository,
    private val sessionManager: SessionManager,
    private val themePreference: ThemePreference
) : ViewModel() {

    private val _uiState = MutableStateFlow(DashboardUiState())
    val uiState: StateFlow<DashboardUiState> = _uiState.asStateFlow()

    /** Device-local color theme - persisted in SharedPreferences only, never synced. */
    val theme: StateFlow<AppThemeOption> = themePreference.theme

    fun setTheme(option: AppThemeOption) = themePreference.setTheme(option)

    init {
        val user = sessionManager.getUserProfile()
        _uiState.value = _uiState.value.copy(user = user)
        loadActiveGames()
        claimPendingInvites()
    }

    /**
     * Converts email invites addressed to this account into pending friend
     * requests (requirement §4.4). Fire-and-forget: runs once per session
     * start, idempotent on the server, failures are silently ignored.
     */
    private fun claimPendingInvites() {
        if (!sessionManager.isOnlineMode()) return
        viewModelScope.launch {
            friendRepository.claimInvites()
        }
    }

    fun loadActiveGames() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            val currentUser = sessionManager.getUserProfile()
            val careerStats = try {
                offlineGameRepository.getUserCareerStats(currentUser)
            } catch (e: Exception) {
                UserCareerStats()
            }
            val recentPlayers = try {
                offlineGameRepository.getRecentPlayers()
            } catch (e: Exception) {
                emptyList()
            }

            val localGames = try {
                offlineGameRepository.getActiveGameSetsWithDetails()
            } catch (e: Exception) {
                emptyList()
            }

            val localEnriched = try {
                offlineGameRepository.getEnrichedActiveGameSets()
            } catch (e: Exception) {
                emptyList()
            }

            val isOnline = sessionManager.isOnlineMode()
            if (isOnline) {
                when (val result = gameSetRepository.getGameSets()) {
                    is ApiResult.Success -> {
                        val remoteGames = result.data.filter { it.isActive }
                        val remoteIds = remoteGames.map { it.id }.toSet()

                        val localEntities = try {
                            offlineGameRepository.getActiveGameSets().first()
                        } catch (e: Exception) {
                            emptyList()
                        }

                        // Filter out local games that have already synced to the API and are in the remote list
                        val filteredLocalGames = localGames.filter { localGame ->
                            val localIdInt = localGame.id.toIntOrNull()
                            val entity = localEntities.find { it.id == localIdInt }
                            entity?.remoteId == null || !remoteIds.contains(entity.remoteId)
                        }

                        val filteredEnriched = localEnriched.filter { enriched ->
                            val localIdInt = enriched.id.toIntOrNull()
                            val entity = localEntities.find { it.id == localIdInt }
                            entity?.remoteId == null || !remoteIds.contains(entity.remoteId)
                        }

                        val remoteEnriched = remoteGames.map { enrichRemoteGame(it) }

                        _uiState.value = _uiState.value.copy(
                            isLoading = false,
                            activeGames = filteredLocalGames + remoteGames,
                            enrichedGames = filteredEnriched + remoteEnriched,
                            careerStats = careerStats,
                            recentPlayers = recentPlayers,
                            isOfflineMode = false
                        )
                    }
                    is ApiResult.Error -> {
                        _uiState.value = _uiState.value.copy(
                            isLoading = false,
                            activeGames = localGames,
                            enrichedGames = localEnriched,
                            careerStats = careerStats,
                            recentPlayers = recentPlayers,
                            isOfflineMode = true,
                            error = null
                        )
                    }
                    is ApiResult.Loading -> {}
                }
            } else {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    activeGames = localGames,
                    enrichedGames = localEnriched,
                    careerStats = careerStats,
                    recentPlayers = recentPlayers,
                    isOfflineMode = true
                )
            }
        }
    }

    private fun enrichRemoteGame(game: MarriageGameSet): EnrichedActiveGame {
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
            p to money
        }
        val highest = standings.maxByOrNull { it.second }
        val leaderBadge = if (highest != null && highest.second > 0.0) {
            "👑 ${highest.first.name} (+${settings.currency.formatMoney(highest.second)})"
        } else null

        val totalGames = rounds.sumOf { it.marriageGames?.size ?: 0 }
        val roundStatus = if (rounds.isEmpty()) "Not started" else "Round ${rounds.size} in progress"

        val suits = listOf("♠", "♥", "♦", "♣")
        val cardSuit = suits[kotlin.math.abs(game.id.hashCode()) % suits.size]

        return EnrichedActiveGame(
            id = game.id,
            name = game.name.ifEmpty { "Online Game" },
            lastPlayed = game.lastPlayed.take(10).ifEmpty { "Recent" },
            players = players,
            leaderName = highest?.first?.name?.takeIf { highest.second > 0.0 },
            leaderScoreText = highest?.second?.let { if (it > 0.0) "+${settings.currency.formatMoney(it)}" else null },
            roundStatusText = roundStatus,
            totalGamesPlayed = totalGames,
            isSettled = !game.isActive,
            cardSuit = cardSuit
        )
    }

    fun quickStartGame(onGameCreated: (String) -> Unit) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isQuickStarting = true)
            try {
                val players = _uiState.value.recentPlayers
                if (players.size < 2) {
                    _uiState.value = _uiState.value.copy(isQuickStarting = false)
                    return@launch
                }
                val playerIds = players.mapNotNull { it.id.toIntOrNull() }
                val gameSetId = offlineGameRepository.quickCreateGame("Quick Game", playerIds)
                _uiState.value = _uiState.value.copy(isQuickStarting = false)
                onGameCreated(gameSetId.toString())
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(isQuickStarting = false, error = e.message)
            }
        }
    }
}
