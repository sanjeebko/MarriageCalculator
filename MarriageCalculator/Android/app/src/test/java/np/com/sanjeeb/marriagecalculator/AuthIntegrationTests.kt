package np.com.sanjeeb.marriagecalculator

import android.content.Context
import android.content.SharedPreferences
import com.google.gson.Gson
import io.mockk.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.*
import np.com.sanjeeb.marriagecalculator.data.model.AuthTokenResult
import np.com.sanjeeb.marriagecalculator.data.model.SendVerificationCodeResult
import np.com.sanjeeb.marriagecalculator.data.remote.AuthApiService
import np.com.sanjeeb.marriagecalculator.data.repository.AuthRepository
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import org.junit.After
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test
import retrofit2.Response

@OptIn(ExperimentalCoroutinesApi::class)
class AuthIntegrationTests {

    private val testDispatcher = StandardTestDispatcher()
    private val authApiService: AuthApiService = mockk()
    private val context: Context = mockk()
    private val sharedPreferences: SharedPreferences = mockk()
    private val editor: SharedPreferences.Editor = mockk()
    private val gson = Gson()

    private lateinit var sessionManager: SessionManager
    private lateinit var authRepository: AuthRepository

    @Before
    fun setUp() {
        Dispatchers.setMain(testDispatcher)
        every { context.getSharedPreferences(any(), any()) } returns sharedPreferences
        every { sharedPreferences.edit() } returns editor
        every { editor.putString(any(), any()) } returns editor
        every { editor.putBoolean(any(), any()) } returns editor
        every { editor.apply() } just Runs

        sessionManager = SessionManager(context, gson)
        authRepository = AuthRepository(authApiService, sessionManager)
    }

    @After
    fun tearDown() {
        Dispatchers.resetMain()
    }

    @Test
    fun sendVerificationCode_Success_ReturnsSuccessResult() = runTest {
        val expectedResult = SendVerificationCodeResult(success = true, message = "Code sent.")
        coEvery { authApiService.sendVerificationCode(any()) } returns Response.success(expectedResult)

        val result = authRepository.sendVerificationCode("user@example.com")

        assertTrue(result.isSuccess)
        assertEquals("Code sent.", result.getOrNull()?.message)
    }

    @Test
    fun register_Success_SavesSessionToken() = runTest {
        val authResult = AuthTokenResult(
            token = "jwt.valid.token",
            userId = "user_123",
            username = "testuser",
            email = "user@example.com",
            displayName = "Test User",
            expiresAt = "2026-08-15T00:00:00Z"
        )
        coEvery { authApiService.register(any()) } returns Response.success(authResult)

        val result = authRepository.register("user@example.com", "123456", "testuser", "Password123!", "Test User")

        assertTrue(result.isSuccess)
        assertEquals("jwt.valid.token", result.getOrNull()?.token)
        verify { editor.putString("auth_token", "jwt.valid.token") }
    }

    @Test
    fun login_Success_SavesSessionToken() = runTest {
        val authResult = AuthTokenResult(
            token = "jwt.login.token",
            userId = "user_123",
            username = "testuser",
            email = "user@example.com",
            displayName = "Test User",
            expiresAt = "2026-08-15T00:00:00Z"
        )
        coEvery { authApiService.login(any()) } returns Response.success(authResult)

        val result = authRepository.login("testuser", "Password123!")

        assertTrue(result.isSuccess)
        assertEquals("jwt.login.token", result.getOrNull()?.token)
        verify { editor.putString("auth_token", "jwt.login.token") }
    }
}
