package np.com.sanjeeb.marriagecalculator.data.repository

import np.com.sanjeeb.marriagecalculator.data.model.User
import np.com.sanjeeb.marriagecalculator.data.model.RegisterFcmTokenRequest
import np.com.sanjeeb.marriagecalculator.data.remote.UserApiService
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class UserRepository @Inject constructor(
    private val api: UserApiService
) {
    suspend fun login(): ApiResult<User> = safeApiCall { api.login() }
    suspend fun registerFcmToken(token: String): ApiResult<Unit> = safeApiCall { 
        api.registerFcmToken(RegisterFcmTokenRequest(token)) 
    }
    suspend fun getUsers(): ApiResult<List<User>> = safeApiCall { api.getUsers() }
    suspend fun getUser(id: String): ApiResult<User> = safeApiCall { api.getUser(id) }
    suspend fun getUserByUid(userId: String): ApiResult<User> = safeApiCall { api.getUserByUid(userId) }
}
