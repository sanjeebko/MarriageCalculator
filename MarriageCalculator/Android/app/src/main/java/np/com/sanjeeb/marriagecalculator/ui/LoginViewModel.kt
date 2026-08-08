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

import np.com.sanjeeb.marriagecalculator.data.repository.AuthRepository

sealed class LoginUiState {
    object Idle : LoginUiState()
    object Loading : LoginUiState()
    data class CodeSent(val message: String) : LoginUiState()
    data class Success(val user: User) : LoginUiState()
    data class Error(val message: String) : LoginUiState()
}

@HiltViewModel
class LoginViewModel @Inject constructor(
    private val userRepository: UserRepository,
    private val authRepository: AuthRepository,
    private val sessionManager: SessionManager
) : ViewModel() {

    private val _uiState = MutableStateFlow<LoginUiState>(LoginUiState.Idle)
    val uiState: StateFlow<LoginUiState> = _uiState.asStateFlow()

    fun sendVerificationCode(email: String) {
        val trimmedEmail = email.trim()
        if (trimmedEmail.isEmpty() || !trimmedEmail.contains("@")) {
            _uiState.value = LoginUiState.Error("Please enter a valid email address.")
            return
        }

        _uiState.value = LoginUiState.Loading
        viewModelScope.launch {
            authRepository.sendVerificationCode(trimmedEmail)
                .onSuccess { result ->
                    _uiState.value = LoginUiState.CodeSent(result.message)
                }
                .onFailure { error ->
                    _uiState.value = LoginUiState.Error(error.message ?: "Failed to send code.")
                }
        }
    }

    fun register(email: String, code: String, username: String, password: String, displayName: String) {
        if (email.isBlank() || code.isBlank() || username.isBlank() || password.isBlank()) {
            _uiState.value = LoginUiState.Error("Please fill in all required registration fields.")
            return
        }

        _uiState.value = LoginUiState.Loading
        viewModelScope.launch {
            authRepository.register(email, code, username, password, displayName)
                .onSuccess { result ->
                    val user = User(
                        id = result.userId,
                        userId = result.userId,
                        displayName = result.displayName,
                        email = result.email
                    )
                    sessionManager.getFcmToken()?.let { fcmToken ->
                        userRepository.registerFcmToken(fcmToken)
                    }
                    _uiState.value = LoginUiState.Success(user)
                }
                .onFailure { error ->
                    _uiState.value = LoginUiState.Error(error.message ?: "Registration failed.")
                }
        }
    }

    fun loginWithEmailOrUsername(usernameOrEmail: String, password: String) {
        if (usernameOrEmail.isBlank() || password.isBlank()) {
            _uiState.value = LoginUiState.Error("Please enter your username/email and password.")
            return
        }

        _uiState.value = LoginUiState.Loading
        viewModelScope.launch {
            authRepository.login(usernameOrEmail, password)
                .onSuccess { result ->
                    val user = User(
                        id = result.userId,
                        userId = result.userId,
                        displayName = result.displayName,
                        email = result.email
                    )
                    sessionManager.getFcmToken()?.let { fcmToken ->
                        userRepository.registerFcmToken(fcmToken)
                    }
                    _uiState.value = LoginUiState.Success(user)
                }
                .onFailure { error ->
                    _uiState.value = LoginUiState.Error(error.message ?: "Invalid credentials.")
                }
        }
    }

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
