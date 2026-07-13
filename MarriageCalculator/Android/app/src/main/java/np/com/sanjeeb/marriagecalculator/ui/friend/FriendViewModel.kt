package np.com.sanjeeb.marriagecalculator.ui.friend

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import np.com.sanjeeb.marriagecalculator.data.model.User
import np.com.sanjeeb.marriagecalculator.data.remote.FriendshipDto
import np.com.sanjeeb.marriagecalculator.data.repository.ApiResult
import np.com.sanjeeb.marriagecalculator.data.repository.FriendRepository
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class FriendUiState(
    val isLoading: Boolean = false,
    val friends: List<User> = emptyList(),
    val pendingReceived: List<FriendshipDto> = emptyList(),
    val pendingSent: List<FriendshipDto> = emptyList(),
    val searchResults: List<User> = emptyList(),
    val currentUser: User? = null,
    val error: String? = null,
    val searchError: String? = null,
    val searchLoading: Boolean = false
)

@HiltViewModel
class FriendViewModel @Inject constructor(
    private val friendRepository: FriendRepository,
    private val sessionManager: SessionManager
) : ViewModel() {

    private val _uiState = MutableStateFlow(FriendUiState())
    val uiState: StateFlow<FriendUiState> = _uiState.asStateFlow()

    init {
        _uiState.value = _uiState.value.copy(currentUser = sessionManager.getUserProfile())
        loadData()
    }

    fun loadData() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            
            // Load friends
            val friendsRes = friendRepository.getFriends()
            val pendingRes = friendRepository.getPendingRequests()
            val sentRes = friendRepository.getSentRequests()

            if (friendsRes is ApiResult.Success && pendingRes is ApiResult.Success && sentRes is ApiResult.Success) {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    friends = friendsRes.data,
                    pendingReceived = pendingRes.data,
                    pendingSent = sentRes.data
                )
            } else {
                val errorMsg = when {
                    friendsRes is ApiResult.Error -> friendsRes.message
                    pendingRes is ApiResult.Error -> pendingRes.message
                    sentRes is ApiResult.Error -> sentRes.message
                    else -> "Failed to load social data."
                }
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    error = errorMsg
                )
            }
        }
    }

    fun searchUsers(query: String) {
        if (query.trim().isEmpty()) {
            _uiState.value = _uiState.value.copy(searchResults = emptyList(), searchError = null)
            return
        }

        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(searchLoading = true, searchError = null)
            when (val result = friendRepository.searchUsers(query)) {
                is ApiResult.Success -> {
                    _uiState.value = _uiState.value.copy(
                        searchLoading = false,
                        searchResults = result.data
                    )
                }
                is ApiResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        searchLoading = false,
                        searchError = result.message
                    )
                }
                is ApiResult.Loading -> {}
            }
        }
    }

    fun sendFriendRequest(emailOrUsername: String, onSuccess: () -> Unit = {}) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            when (val result = friendRepository.sendFriendRequest(emailOrUsername)) {
                is ApiResult.Success -> {
                    loadData()
                    onSuccess()
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

    fun respondToRequest(friendshipId: String, accept: Boolean) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            when (val result = friendRepository.respondFriendRequest(friendshipId, accept)) {
                is ApiResult.Success -> {
                    loadData()
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

    fun removeFriend(friendshipId: String) {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            when (val result = friendRepository.removeFriend(friendshipId)) {
                is ApiResult.Success -> {
                    loadData()
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

    fun clearError() {
        _uiState.value = _uiState.value.copy(error = null, searchError = null)
    }
}
