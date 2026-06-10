package com.sanjeeb.marriagecalculator.data.repository

import com.sanjeeb.marriagecalculator.data.model.User
import com.sanjeeb.marriagecalculator.data.remote.*
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class FriendRepository @Inject constructor(
    private val api: FriendApiService,
    private val userApi: UserApiService
) {
    suspend fun getFriends(): ApiResult<List<User>> = safeApiCall { api.getFriends() }

    suspend fun getPendingRequests(): ApiResult<List<FriendshipDto>> = safeApiCall { api.getPendingRequests() }

    suspend fun getSentRequests(): ApiResult<List<FriendshipDto>> = safeApiCall { api.getSentRequests() }

    suspend fun sendFriendRequest(emailOrUsername: String): ApiResult<FriendshipDto> = safeApiCall {
        api.sendFriendRequest(SendFriendRequestDto(emailOrUsername))
    }

    suspend fun respondFriendRequest(id: String, accept: Boolean): ApiResult<FriendshipDto> = safeApiCall {
        api.respondFriendRequest(id, RespondFriendRequestDto(accept))
    }

    suspend fun removeFriend(id: String): ApiResult<Unit> = safeApiCall { api.removeFriend(id) }

    suspend fun searchUsers(query: String): ApiResult<List<User>> = safeApiCall { userApi.searchUsers(query) }
}
