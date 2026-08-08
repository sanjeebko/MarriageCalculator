package np.com.sanjeeb.marriagecalculator.data.remote

import np.com.sanjeeb.marriagecalculator.data.model.User
import np.com.sanjeeb.marriagecalculator.data.model.RegisterFcmTokenRequest
import retrofit2.Response
import retrofit2.http.*

interface UserApiService {
    @POST("Users/login")
    suspend fun login(): Response<User>

    @POST("Users/fcm-token")
    suspend fun registerFcmToken(@Body request: RegisterFcmTokenRequest): Response<Unit>

    @GET("Users")
    suspend fun getUsers(): Response<List<User>>

    @GET("Users/{id}")
    suspend fun getUser(@Path("id") id: String): Response<User>

    @GET("Users/uid/{userId}")
    suspend fun getUserByUid(@Path("userId") userId: String): Response<User>

    @GET("Users/search")
    suspend fun searchUsers(@Query("query") query: String): Response<List<User>>
}
