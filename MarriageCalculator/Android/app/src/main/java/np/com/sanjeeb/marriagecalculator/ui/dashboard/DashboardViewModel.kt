package np.com.sanjeeb.marriagecalculator.ui.dashboard

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import np.com.sanjeeb.marriagecalculator.data.model.MarriageGameSet
import np.com.sanjeeb.marriagecalculator.data.model.User
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
    val user: User? = null,
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
        _uiState.value = _uiState.value.copy(user = sessionManager.getUserProfile())
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
            
            val localGames = try {
                offlineGameRepository.getActiveGameSetsWithDetails()
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

                        _uiState.value = _uiState.value.copy(
                            isLoading = false,
                            activeGames = filteredLocalGames + remoteGames,
                            isOfflineMode = false
                        )
                    }
                    is ApiResult.Error -> {
                        _uiState.value = _uiState.value.copy(
                            isLoading = false,
                            activeGames = localGames,
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
                    isOfflineMode = true
                )
            }
        }
    }
}
