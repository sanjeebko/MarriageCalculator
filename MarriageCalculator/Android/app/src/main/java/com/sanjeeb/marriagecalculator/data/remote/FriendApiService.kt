package com.sanjeeb.marriagecalculator.data.remote

import com.google.gson.annotations.SerializedName
import com.sanjeeb.marriagecalculator.data.model.User
import retrofit2.Response
import retrofit2.http.*

data class FriendshipDto(
    @SerializedName("id") val id: String = "",
    @SerializedName("requesterUserId") val requesterUserId: String = "",
    @SerializedName("requesterName") val requesterName: String = "",
    @SerializedName("requesterEmail") val requesterEmail: String = "",
    @SerializedName("receiverUserId") val receiverUserId: String = "",
    @SerializedName("receiverName") val receiverName: String = "",
    @SerializedName("receiverEmail") val receiverEmail: String = "",
    @SerializedName("status") val status: String = "",
    @SerializedName("createdAt") val createdAt: String = ""
)

data class SendFriendRequestDto(
    @SerializedName("receiverEmailOrUsername") val receiverEmailOrUsername: String
)

data class RespondFriendRequestDto(
    @SerializedName("accept") val accept: Boolean
)

data class TransferHostDto(
    @SerializedName("newHostUserId") val newHostUserId: String
)

interface FriendApiService {
    @GET("Friendships")
    suspend fun getFriends(): Response<List<User>>

    @GET("Friendships/pending")
    suspend fun getPendingRequests(): Response<List<FriendshipDto>>

    @GET("Friendships/sent")
    suspend fun getSentRequests(): Response<List<FriendshipDto>>

    @POST("Friendships/request")
    suspend fun sendFriendRequest(@Body request: SendFriendRequestDto): Response<FriendshipDto>

    @POST("Friendships/respond/{id}")
    suspend fun respondFriendRequest(
        @Path("id") id: String,
        @Body response: RespondFriendRequestDto
    ): Response<FriendshipDto>

    @DELETE("Friendships/{id}")
    suspend fun removeFriend(@Path("id") id: String): Response<Unit>
}
