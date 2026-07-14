package np.com.sanjeeb.marriagecalculator.data.repository

import np.com.sanjeeb.marriagecalculator.data.model.User
import np.com.sanjeeb.marriagecalculator.data.remote.*
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class FriendRepository @Inject constructor(
    private val api: FriendApiService
) {
    suspend fun getFriends(): ApiResult<List<User>> = safeApiCall { api.getFriends() }

    suspend fun getPendingRequests(): ApiResult<List<FriendshipDto>> = safeApiCall { api.getPendingRequests() }

    suspend fun getSentRequests(): ApiResult<List<FriendshipDto>> = safeApiCall { api.getSentRequests() }

    /** Complete-email friend request; the result message never reveals whether the email is registered. */
    suspend fun sendFriendRequest(email: String): ApiResult<FriendRequestResultDto> = safeApiCall {
        api.sendFriendRequest(SendFriendRequestDto(email))
    }

    suspend fun respondFriendRequest(id: String, accept: Boolean): ApiResult<FriendshipDto> = safeApiCall {
        api.respondFriendRequest(id, RespondFriendRequestDto(accept))
    }

    suspend fun removeFriend(id: String): ApiResult<Unit> = safeApiCall { api.removeFriend(id) }

    /** My shareable 7-day invite code (server creates one if none is active). */
    suspend fun getInviteCode(): ApiResult<InviteCodeDto> = safeApiCall { api.getInviteCode() }

    /** Redeem a friend's code — instant, auto-accepted friendship. */
    suspend fun redeemInviteCode(code: String): ApiResult<RedeemInviteCodeResultDto> = safeApiCall {
        api.redeemInviteCode(RedeemInviteCodeDto(code))
    }

    /** Convert email invites addressed to me into pending requests (call after login). */
    suspend fun claimInvites(): ApiResult<ClaimInvitesResultDto> = safeApiCall { api.claimInvites() }
}
