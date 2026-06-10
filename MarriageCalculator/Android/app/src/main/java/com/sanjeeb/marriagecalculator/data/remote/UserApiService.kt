package com.sanjeeb.marriagecalculator.data.remote

import com.sanjeeb.marriagecalculator.data.model.User
import retrofit2.Response
import retrofit2.http.*

interface UserApiService {
    @POST("Users/login")
    suspend fun login(): Response<User>

    @GET("Users")
    suspend fun getUsers(): Response<List<User>>

    @GET("Users/{id}")
    suspend fun getUser(@Path("id") id: String): Response<User>

    @GET("Users/uid/{userId}")
    suspend fun getUserByUid(@Path("userId") userId: String): Response<User>
}
