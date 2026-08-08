package np.com.sanjeeb.marriagecalculator.data.remote

import com.google.gson.annotations.SerializedName
import np.com.sanjeeb.marriagecalculator.data.model.User
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

/** My shareable friend invite code (7-day, multi-use). */
data class InviteCodeDto(
    @SerializedName("code") val code: String = "",
    @SerializedName("expiresAt") val expiresAt: String = ""
)

data class RedeemInviteCodeDto(
    @SerializedName("code") val code: String
)

/** Result of redeeming a code: instant, auto-accepted friendship. */
data class RedeemInviteCodeResultDto(
    @SerializedName("message") val message: String = "",
    @SerializedName("friendship") val friendship: FriendshipDto? = null
)

/**
 * Result of a complete-email friend request. The message is deliberately
 * identical whether or not the email belongs to a registered user.
 */
data class FriendRequestResultDto(
    @SerializedName("status") val status: String = "",
    @SerializedName("message") val message: String = "",
    @SerializedName("friendship") val friendship: FriendshipDto? = null
)

/** Result of claiming pending email invites after login. */
data class ClaimInvitesResultDto(
    @SerializedName("claimed") val claimed: Int = 0
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
    suspend fun sendFriendRequest(@Body request: SendFriendRequestDto): Response<FriendRequestResultDto>

    @POST("Friendships/invite-code")
    suspend fun getInviteCode(): Response<InviteCodeDto>

    @POST("Friendships/invite-code/redeem")
    suspend fun redeemInviteCode(@Body request: RedeemInviteCodeDto): Response<RedeemInviteCodeResultDto>

    @POST("Friendships/claim-invites")
    suspend fun claimInvites(): Response<ClaimInvitesResultDto>

    @POST("Friendships/respond/{id}")
    suspend fun respondFriendRequest(
        @Path("id") id: String,
        @Body response: RespondFriendRequestDto
    ): Response<FriendshipDto>

    @DELETE("Friendships/{id}")
    suspend fun removeFriend(@Path("id") id: String): Response<Unit>
}
