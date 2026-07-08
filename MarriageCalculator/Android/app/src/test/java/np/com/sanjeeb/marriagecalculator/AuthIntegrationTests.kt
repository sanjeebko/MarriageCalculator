package np.com.sanjeeb.marriagecalculator

import android.content.Context
import android.content.SharedPreferences
import com.google.gson.Gson
import np.com.sanjeeb.marriagecalculator.data.model.User
import np.com.sanjeeb.marriagecalculator.data.repository.ApiResult
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import np.com.sanjeeb.marriagecalculator.data.repository.UserRepository
import np.com.sanjeeb.marriagecalculator.ui.LoginViewModel
import np.com.sanjeeb.marriagecalculator.ui.LoginUiState
import io.mockk.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.*
import okhttp3.Interceptor
import okhttp3.Request
import org.junit.After
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class AuthIntegrationTests {

    private val testDispatcher = UnconfinedTestDispatcher()

    private val context: Context = mockk(relaxed = true)
    private val sharedPreferences: SharedPreferences = mockk(relaxed = true)
    private val editor: SharedPreferences.Editor = mockk(relaxed = true)
    private val gson = Gson()

    private lateinit var sessionManager: SessionManager
    private lateinit var userRepository: UserRepository
    private lateinit var loginViewModel: LoginViewModel

    @Before
    fun setup() {
        Dispatchers.setMain(testDispatcher)

        // Mock SharedPreferences behavior
        every { context.getSharedPreferences("user_session", Context.MODE_PRIVATE) } returns sharedPreferences
        every { sharedPreferences.edit() } returns editor
        every { editor.putString(any(), any()) } returns editor
        every { editor.putBoolean(any(), any()) } returns editor
        every { editor.clear() } returns editor

        sessionManager = SessionManager(context, gson)
        userRepository = mockk(relaxed = true)
        loginViewModel = LoginViewModel(userRepository, sessionManager)
    }

    @After
    fun tearDown() {
        Dispatchers.resetMain()
    }

    @Test
    fun `SessionManager saves token and user profile correctly`() {
        val token = "mock-sanjeeb"
        val user = User(id = "1", userId = token, displayName = "Sanjeeb", email = "sanjeeb@test.com")

        sessionManager.saveSession(token, user)

        verify(exactly = 1) { editor.putString("auth_token", token) }
        verify(exactly = 1) { editor.putString("user_profile", gson.toJson(user)) }
        verify(exactly = 1) { editor.apply() }
    }

    @Test
    fun `SessionManager gets token and profile correctly`() {
        val token = "mock-sanjeeb"
        val user = User(id = "1", userId = token, displayName = "Sanjeeb", email = "sanjeeb@test.com")

        every { sharedPreferences.getString("auth_token", null) } returns token
        every { sharedPreferences.getString("user_profile", null) } returns gson.toJson(user)

        assertEquals(token, sessionManager.getAuthToken())
        assertEquals(user, sessionManager.getUserProfile())
        assertTrue(sessionManager.isLoggedIn())
    }

    @Test
    fun `SessionManager clearSession clears shared preferences`() {
        sessionManager.clearSession()

        verify(exactly = 1) { editor.clear() }
        verify(exactly = 1) { editor.apply() }
    }

    @Test
    fun `OkHttpClient Interceptor appends Bearer Token when present`() {
        // Set token
        val token = "mock-sanjeeb"
        every { sharedPreferences.getString("auth_token", null) } returns token

        // Define interceptor behavior
        val interceptor = Interceptor { chain ->
            val requestBuilder = chain.request().newBuilder()
            sessionManager.getAuthToken()?.let { t ->
                requestBuilder.addHeader("Authorization", "Bearer $t")
            }
            chain.proceed(requestBuilder.build())
        }

        // Mock Chain
        val request = Request.Builder().url("http://localhost/").build()
        val chain: Interceptor.Chain = mockk()
        every { chain.request() } returns request

        val capturedRequest = slot<Request>()
        every { chain.proceed(capture(capturedRequest)) } returns mockk()

        // Act
        interceptor.intercept(chain)

        // Assert
        assertTrue(capturedRequest.isCaptured)
        assertEquals("Bearer mock-sanjeeb", capturedRequest.captured.header("Authorization"))
    }

    @Test
    fun `LoginViewModel updates to Success state on successful login`() = runTest {
        val username = "sanjeeb"
        val expectedUser = User(id = "123", userId = "mock-sanjeeb", displayName = "sanjeeb")

        coEvery { userRepository.login() } returns ApiResult.Success(expectedUser)

        loginViewModel.loginWithMockToken(username)

        assertTrue(loginViewModel.uiState.value is LoginUiState.Success)
        assertEquals(expectedUser, (loginViewModel.uiState.value as LoginUiState.Success).user)
        verify { editor.putString("auth_token", "mock-sanjeeb") }
    }

    @Test
    fun `LoginViewModel updates to Error state on failed login`() = runTest {
        val username = "sanjeeb"
        coEvery { userRepository.login() } returns ApiResult.Error("Unauthorized request", 401)

        loginViewModel.loginWithMockToken(username)

        assertTrue(loginViewModel.uiState.value is LoginUiState.Error)
        assertEquals("Unauthorized request", (loginViewModel.uiState.value as LoginUiState.Error).message)
        // verify session was cleared
        verify { editor.clear() }
    }
}
