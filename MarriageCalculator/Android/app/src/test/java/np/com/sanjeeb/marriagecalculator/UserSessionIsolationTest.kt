package np.com.sanjeeb.marriagecalculator

import android.content.Context
import android.content.SharedPreferences
import com.google.gson.Gson
import io.mockk.every
import io.mockk.mockk
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.test.runTest
import np.com.sanjeeb.marriagecalculator.data.model.User
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test

class UserSessionIsolationTest {

    private val context: Context = mockk(relaxed = true)
    private val prefs: SharedPreferences = mockk(relaxed = true)
    private val editor: SharedPreferences.Editor = mockk(relaxed = true)
    private val gson = Gson()

    private val prefStorage = mutableMapOf<String, Any?>()

    private lateinit var sessionManager: SessionManager

    @Before
    fun setUp() {
        prefStorage.clear()

        every { context.getSharedPreferences("user_session", Context.MODE_PRIVATE) } returns prefs
        every { prefs.getString(any(), any()) } answers {
            val key = firstArg<String>()
            val defVal = secondArg<String?>()
            (prefStorage[key] as? String) ?: defVal
        }
        every { prefs.getBoolean(any(), any()) } answers {
            val key = firstArg<String>()
            val defVal = secondArg<Boolean>()
            (prefStorage[key] as? Boolean) ?: defVal
        }
        every { prefs.edit() } returns editor
        every { editor.putString(any(), any()) } answers {
            prefStorage[firstArg<String>()] = secondArg<String>()
            editor
        }
        every { editor.putBoolean(any(), any()) } answers {
            prefStorage[firstArg<String>()] = secondArg<Boolean>()
            editor
        }
        every { editor.clear() } answers {
            prefStorage.clear()
            editor
        }
        every { editor.apply() } answers {}

        sessionManager = SessionManager(context, gson)
    }

    @Test
    fun `default unauthenticated state yields guest user key`() {
        assertEquals("guest", sessionManager.getCurrentUserKey())
        assertFalse(sessionManager.isLoggedIn())
        assertFalse(sessionManager.isOnlineMode())
    }

    @Test
    fun `guest session yields guest user key and offline mode`() = runTest {
        val guestUser = User(userId = "guest", displayName = "Guest", email = "")
        sessionManager.saveSession("guest-token", guestUser)

        assertEquals("guest", sessionManager.getCurrentUserKey())
        assertTrue(sessionManager.isGuestMode())
        assertFalse(sessionManager.isLoggedIn())
        assertFalse(sessionManager.isOnlineMode())
        assertEquals("guest", sessionManager.userKeyFlow.first())
    }

    @Test
    fun `registered user yields isolated user key`() = runTest {
        val userA = User(
            id = "usr-101",
            userId = "auth0|12345",
            displayName = "Player One",
            email = "player1@test.com"
        )
        sessionManager.saveSession("token-user-a", userA)

        assertEquals("user_auth0_12345", sessionManager.getCurrentUserKey())
        assertTrue(sessionManager.isLoggedIn())
        assertTrue(sessionManager.isOnlineMode())
        assertEquals("user_auth0_12345", sessionManager.userKeyFlow.first())
    }

    @Test
    fun `switching users completely isolates user keys`() = runTest {
        // Log in User A
        val userA = User(id = "1", userId = "user_alpha", displayName = "Alpha", email = "alpha@test.com")
        sessionManager.saveSession("token-alpha", userA)
        assertEquals("user_user_alpha", sessionManager.getCurrentUserKey())

        // Logout
        sessionManager.clearSession()
        assertEquals("guest", sessionManager.getCurrentUserKey())

        // Guest calculation
        sessionManager.saveSession("guest-token", User(userId = "guest", displayName = "Guest"))
        assertEquals("guest", sessionManager.getCurrentUserKey())

        // Log in User B
        val userB = User(id = "2", userId = "user_beta", displayName = "Beta", email = "beta@test.com")
        sessionManager.saveSession("token-beta", userB)
        assertEquals("user_user_beta", sessionManager.getCurrentUserKey())

        // Switch back to User A
        sessionManager.saveSession("token-alpha", userA)
        assertEquals("user_user_alpha", sessionManager.getCurrentUserKey())
    }

    @Test
    fun `special characters in user id or email are safely sanitized`() {
        val userWithSpecialChars = User(
            id = "temp",
            userId = "google-oauth2|1029384756!#$",
            displayName = "Google User",
            email = "special+user@example.com"
        )
        sessionManager.saveSession("google-token", userWithSpecialChars)

        val key = sessionManager.getCurrentUserKey()
        assertTrue(key.startsWith("user_"))
        assertFalse(key.contains("|"))
        assertFalse(key.contains("!"))
        assertFalse(key.contains("#"))
        assertFalse(key.contains("$"))
        assertEquals("user_google_oauth2_1029384756___", key)
    }
}
