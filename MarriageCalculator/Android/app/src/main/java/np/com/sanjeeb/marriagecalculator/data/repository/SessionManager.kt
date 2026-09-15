package np.com.sanjeeb.marriagecalculator.data.repository

import android.content.Context
import android.content.SharedPreferences
import com.google.gson.Gson
import np.com.sanjeeb.marriagecalculator.data.model.User
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class SessionManager @Inject constructor(
    @param:ApplicationContext private val context: Context,
    private val gson: Gson
) {
    private val prefs: SharedPreferences = context.getSharedPreferences("user_session", Context.MODE_PRIVATE)

    private val _userKeyFlow = kotlinx.coroutines.flow.MutableStateFlow(getCurrentUserKey())
    val userKeyFlow: kotlinx.coroutines.flow.StateFlow<String> = _userKeyFlow

    /** Fires once when the server rejects the stored token (HTTP 401/403). Observe to navigate to login. */
    private val _sessionExpiredEvent = MutableSharedFlow<Unit>(extraBufferCapacity = 1)
    val sessionExpiredEvent: SharedFlow<Unit> = _sessionExpiredEvent.asSharedFlow()

    fun emitSessionExpired() {
        _sessionExpiredEvent.tryEmit(Unit)
    }

    fun getCurrentUserKey(): String {
        if (isGuestMode() || !isLoggedIn()) {
            return "guest"
        }
        val user = getUserProfile() ?: return "guest"
        val identifier = when {
            user.userId.isNotBlank() && user.userId != "google-temp" -> user.userId
            user.id.isNotBlank() && user.id != "google-temp" -> user.id
            user.email.isNotBlank() -> user.email
            user.userId.isNotBlank() -> user.userId
            user.id.isNotBlank() -> user.id
            else -> "guest"
        }
        if (identifier == "guest") return "guest"
        val sanitized = identifier.replace(Regex("[^a-zA-Z0-9_]"), "_")
        return "user_$sanitized"
    }

    fun saveSession(token: String, user: User) {
        prefs.edit()
            .putString("auth_token", token)
            .putString("user_profile", gson.toJson(user))
            .putBoolean("is_online_mode", token != "guest-token")
            .apply()
        _userKeyFlow.value = getCurrentUserKey()
    }

    fun getAuthToken(): String? {
        return try {
            prefs.getString("auth_token", null)
        } catch (e: Exception) {
            null
        }
    }

    fun getUserProfile(): User? {
        val userJson = try {
            prefs.getString("user_profile", null)
        } catch (e: Exception) {
            null
        } ?: return null
        return try {
            gson.fromJson(userJson, User::class.java)
        } catch (e: Exception) {
            null
        }
    }

    fun clearSession() {
        prefs.edit().clear().apply()
        _userKeyFlow.value = getCurrentUserKey()
    }

    fun isLoggedIn(): Boolean {
        return getAuthToken() != null && getAuthToken() != "guest-token"
    }

    fun isGuestMode(): Boolean {
        return getAuthToken() == "guest-token"
    }

    fun setOnlineMode(online: Boolean) {
        prefs.edit().putBoolean("is_online_mode", online).apply()
        if (!online) {
            // Logged out of online or switched to guest
            prefs.edit().putString("auth_token", "guest-token").apply()
        }
        _userKeyFlow.value = getCurrentUserKey()
    }

    fun isOnlineMode(): Boolean {
        return try {
            prefs.getBoolean("is_online_mode", true) && isLoggedIn()
        } catch (e: Exception) {
            false
        }
    }

    fun saveFcmToken(token: String) {
        prefs.edit().putString("fcm_token", token).apply()
    }

    fun getFcmToken(): String? {
        return try {
            prefs.getString("fcm_token", null)
        } catch (e: Exception) {
            null
        }
    }
}
