package np.com.sanjeeb.marriagecalculator.data.repository

import np.com.sanjeeb.marriagecalculator.data.model.*
import np.com.sanjeeb.marriagecalculator.data.remote.AuthApiService
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class AuthRepository @Inject constructor(
    private val authApiService: AuthApiService,
    private val sessionManager: SessionManager
) {
    suspend fun sendVerificationCode(email: String): Result<SendVerificationCodeResult> {
        return try {
            val response = authApiService.sendVerificationCode(SendVerificationCodeRequest(email))
            if (response.isSuccessful && response.body() != null) {
                Result.success(response.body()!!)
            } else {
                val errorMsg = response.errorBody()?.string() ?: "Failed to send verification code."
                Result.failure(Exception(errorMsg))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun register(
        email: String,
        code: String,
        username: String,
        password: String,
        displayName: String
    ): Result<AuthTokenResult> {
        return try {
            val request = RegisterUserRequest(
                email = email,
                verificationCode = code,
                username = username,
                password = password,
                displayName = displayName.ifBlank { username }
            )
            val response = authApiService.register(request)
            if (response.isSuccessful && response.body() != null) {
                val authResult = response.body()!!
                saveSessionFromResult(authResult)
                Result.success(authResult)
            } else {
                val errorMsg = parseErrorMessage(response.errorBody()?.string()) ?: "Registration failed."
                Result.failure(Exception(errorMsg))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun login(usernameOrEmail: String, password: String): Result<AuthTokenResult> {
        return try {
            val request = LoginRequest(usernameOrEmail, password)
            val response = authApiService.login(request)
            if (response.isSuccessful && response.body() != null) {
                val authResult = response.body()!!
                saveSessionFromResult(authResult)
                Result.success(authResult)
            } else {
                val errorMsg = parseErrorMessage(response.errorBody()?.string()) ?: "Invalid username/email or password."
                Result.failure(Exception(errorMsg))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    private fun saveSessionFromResult(authResult: AuthTokenResult) {
        val user = User(
            id = authResult.userId,
            userId = authResult.userId,
            displayName = authResult.displayName,
            email = authResult.email
        )
        sessionManager.saveSession(authResult.token, user)
    }

    private fun parseErrorMessage(errorJson: String?): String? {
        if (errorJson.isNullOrBlank()) return null
        return try {
            if (errorJson.contains("\"message\":")) {
                val regex = "\"message\"\\s*:\\s*\"([^\"]+)\"".toRegex()
                regex.find(errorJson)?.groupValues?.get(1) ?: errorJson
            } else {
                errorJson
            }
        } catch (e: Exception) {
            errorJson
        }
    }
}
