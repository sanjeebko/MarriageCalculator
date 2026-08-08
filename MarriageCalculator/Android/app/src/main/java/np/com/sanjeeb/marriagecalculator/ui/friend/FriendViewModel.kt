package np.com.sanjeeb.marriagecalculator.ui.friend

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import np.com.sanjeeb.marriagecalculator.data.model.User
import np.com.sanjeeb.marriagecalculator.data.remote.FriendshipDto
import np.com.sanjeeb.marriagecalculator.data.remote.InviteCodeDto
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
    val currentUser: User? = null,
    val error: String? = null,
    /** My shareable invite code (fetched lazily for the Add Friends tab). */
    val inviteCode: InviteCodeDto? = null,
    val inviteCodeLoading: Boolean = false,
    val redeemLoading: Boolean = false,
    val addEmailLoading: Boolean = false,
    /** Success feedback from redeeming a code or sending an email request. */
    val actionMessage: String? = null
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

    /** Fetches (or creates) my shareable invite code. No-op if already loaded. */
    fun loadInviteCode() {
        if (_uiState.value.inviteCode != null || _uiState.value.inviteCodeLoading) return
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(inviteCodeLoading = true)
            when (val result = friendRepository.getInviteCode()) {
                is ApiResult.Success -> {
                    _uiState.value = _uiState.value.copy(inviteCodeLoading = false, inviteCode = result.data)
                }
                is ApiResult.Error -> {
                    _uiState.value = _uiState.value.copy(inviteCodeLoading = false, error = result.message)
                }
                is ApiResult.Loading -> {}
            }
        }
    }

    /** Redeems a friend's invite code — success creates an instant friendship. */
    fun redeemInviteCode(code: String, onSuccess: () -> Unit = {}) {
        val trimmed = code.trim()
        if (trimmed.isEmpty()) return
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(redeemLoading = true, error = null, actionMessage = null)
            when (val result = friendRepository.redeemInviteCode(trimmed)) {
                is ApiResult.Success -> {
                    _uiState.value = _uiState.value.copy(redeemLoading = false, actionMessage = result.data.message)
                    loadData()
                    onSuccess()
                }
                is ApiResult.Error -> {
                    _uiState.value = _uiState.value.copy(redeemLoading = false, error = result.message)
                }
                is ApiResult.Loading -> {}
            }
        }
    }

    /** Complete-email friend request; the reply never reveals whether the email is registered. */
    fun sendFriendRequest(email: String, onSuccess: () -> Unit = {}) {
        val trimmed = email.trim()
        if (trimmed.isEmpty()) return
        if (!trimmed.contains("@") || !trimmed.substringAfter("@").contains(".")) {
            _uiState.value = _uiState.value.copy(error = "Please enter a complete email address")
            return
        }
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(addEmailLoading = true, error = null, actionMessage = null)
            when (val result = friendRepository.sendFriendRequest(trimmed)) {
                is ApiResult.Success -> {
                    _uiState.value = _uiState.value.copy(addEmailLoading = false, actionMessage = result.data.message)
                    loadData()
                    onSuccess()
                }
                is ApiResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        addEmailLoading = false,
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
        _uiState.value = _uiState.value.copy(error = null)
    }

    fun clearActionMessage() {
        _uiState.value = _uiState.value.copy(actionMessage = null)
    }
}
