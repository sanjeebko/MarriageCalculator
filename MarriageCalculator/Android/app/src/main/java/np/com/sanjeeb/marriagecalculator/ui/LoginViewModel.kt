package np.com.sanjeeb.marriagecalculator.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import np.com.sanjeeb.marriagecalculator.data.model.User
import np.com.sanjeeb.marriagecalculator.data.repository.ApiResult
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import np.com.sanjeeb.marriagecalculator.data.repository.UserRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

sealed class LoginUiState {
    object Idle : LoginUiState()
    object Loading : LoginUiState()
    data class Success(val user: User) : LoginUiState()
    data class Error(val message: String) : LoginUiState()
}

@HiltViewModel
class LoginViewModel @Inject constructor(
    private val userRepository: UserRepository,
    private val sessionManager: SessionManager
) : ViewModel() {

    private val _uiState = MutableStateFlow<LoginUiState>(LoginUiState.Idle)
    val uiState: StateFlow<LoginUiState> = _uiState.asStateFlow()

    fun loginWithMockToken(username: String) {
        val trimmed = username.trim()
        if (trimmed.isEmpty()) {
            _uiState.value = LoginUiState.Error("Username cannot be empty")
            return
        }

        // Mock token prefix matches FirebaseOrMockAuthenticationHandler.cs
        val token = "mock-${trimmed.lowercase()}"
        _uiState.value = LoginUiState.Loading

        viewModelScope.launch {
            // Save token temporarily so subsequent API calls (including the login call itself) are signed
            sessionManager.saveSession(token, User(userId = token, displayName = trimmed))
            
            when (val result = userRepository.login()) {
                is ApiResult.Success -> {
                    sessionManager.saveSession(token, result.data)
                    sessionManager.getFcmToken()?.let { fcmToken ->
                        userRepository.registerFcmToken(fcmToken)
                    }
                    _uiState.value = LoginUiState.Success(result.data)
                }
                is ApiResult.Error -> {
                    sessionManager.clearSession()
                    _uiState.value = LoginUiState.Error(result.message)
                }
                is ApiResult.Loading -> {
                    _uiState.value = LoginUiState.Loading
                }
            }
        }
    }

    fun loginWithGoogle(idToken: String, displayName: String?, photoUrl: String?) {
        _uiState.value = LoginUiState.Loading
        viewModelScope.launch {
            sessionManager.saveSession(idToken, User(userId = "google-temp", displayName = displayName ?: "Google User", photoUrl = photoUrl))
            when (val result = userRepository.login()) {
                is ApiResult.Success -> {
                    val finalUser = result.data.copy(
                        photoUrl = result.data.photoUrl ?: photoUrl
                    )
                    sessionManager.saveSession(idToken, finalUser)
                    sessionManager.getFcmToken()?.let { fcmToken ->
                        userRepository.registerFcmToken(fcmToken)
                    }
                    _uiState.value = LoginUiState.Success(finalUser)
                }
                is ApiResult.Error -> {
                    sessionManager.clearSession()
                    _uiState.value = LoginUiState.Error(result.message)
                }
                is ApiResult.Loading -> {
                    _uiState.value = LoginUiState.Loading
                }
            }
        }
    }

    fun loginAsGuest() {
        _uiState.value = LoginUiState.Loading
        viewModelScope.launch {
            val guestUser = User(
                id = "guest-id",
                userId = "guest-id",
                displayName = "Guest User",
                email = "guest@marriagecalculator.local"
            )
            sessionManager.saveSession("guest-token", guestUser)
            _uiState.value = LoginUiState.Success(guestUser)
        }
    }
}
