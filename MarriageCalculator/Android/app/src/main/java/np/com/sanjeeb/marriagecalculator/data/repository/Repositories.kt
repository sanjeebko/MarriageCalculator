package np.com.sanjeeb.marriagecalculator.data.repository

import np.com.sanjeeb.marriagecalculator.data.model.*
import np.com.sanjeeb.marriagecalculator.data.remote.*
import javax.inject.Inject
import javax.inject.Singleton

sealed class ApiResult<out T> {
    data class Success<T>(val data: T) : ApiResult<T>()
    data class Error(val message: String, val code: Int = 0) : ApiResult<Nothing>()
    data object Loading : ApiResult<Nothing>()
}

@Singleton
class PlayerRepository @Inject constructor(
    private val api: PlayerApiService
) {
    suspend fun getPlayers(): ApiResult<List<Player>> = safeApiCall { api.getPlayers() }
    suspend fun getPlayer(id: String): ApiResult<Player> = safeApiCall { api.getPlayer(id) }
    suspend fun createPlayer(request: CreatePlayerRequest): ApiResult<Player> = safeApiCall { api.createPlayer(request) }
    suspend fun updatePlayer(id: String, request: UpdatePlayerRequest): ApiResult<Player> = safeApiCall { api.updatePlayer(id, request) }
    suspend fun deletePlayer(id: String): ApiResult<Unit> = safeApiCall { api.deletePlayer(id) }
}

@Singleton
class GameSettingsRepository @Inject constructor(
    private val api: GameSettingsApiService
) {
    suspend fun getGameSettings(): ApiResult<List<GameSettings>> = safeApiCall { api.getGameSettings() }
    suspend fun getGameSetting(id: String): ApiResult<GameSettings> = safeApiCall { api.getGameSetting(id) }
    suspend fun createGameSettings(request: CreateGameSettingsRequest): ApiResult<GameSettings> = safeApiCall { api.createGameSettings(request) }
    suspend fun updateGameSettings(id: String, request: CreateGameSettingsRequest): ApiResult<GameSettings> = safeApiCall { api.updateGameSettings(id, request) }
}

@Singleton
class GameSetRepository @Inject constructor(
    private val api: MarriageGameSetApiService
) {
    suspend fun getGameSets(): ApiResult<List<MarriageGameSet>> = safeApiCall { api.getGameSets() }
    suspend fun getGameSet(id: String): ApiResult<MarriageGameSet> = safeApiCall { api.getGameSet(id) }
    suspend fun createGameSet(request: CreateGameSetRequest): ApiResult<MarriageGameSet> = safeApiCall { api.createGameSet(request) }
    suspend fun updateGameSet(id: String, request: CreateGameSetRequest): ApiResult<MarriageGameSet> = safeApiCall { api.updateGameSet(id, request) }
    suspend fun transferHost(id: String, newHostUserId: String): ApiResult<Unit> = safeApiCall {
        api.transferHost(id, TransferHostDto(newHostUserId))
    }
    suspend fun nudgePlayer(id: String, playerId: String): ApiResult<Unit> = safeApiCall {
        api.nudgePlayer(id, playerId)
    }
    suspend fun submitRound(id: String, request: SubmitRoundRequest): ApiResult<MarriageGameRound> = safeApiCall {
        api.submitRound(id, request)
    }
    suspend fun closeRound(id: String, roundId: String): ApiResult<MarriageGameRound> = safeApiCall {
        api.closeRound(id, roundId)
    }
    suspend fun togglePaymentCleared(id: String, roundId: String, paymentCleared: Boolean): ApiResult<MarriageGameRound> = safeApiCall {
        api.togglePaymentCleared(id, roundId, TogglePaymentClearedRequest(paymentCleared))
    }
    suspend fun updateGame(id: String, gameId: String, request: SubmitRoundRequest): ApiResult<MarriageGameRound> = safeApiCall {
        api.updateGame(id, gameId, request)
    }
    // These hit endpoints that can legitimately return 204 No Content (no body), so they can't
    // use safeApiCall - it treats a null body as an error even on success.
    suspend fun deleteLastGame(id: String): ApiResult<Unit> = safeUnitApiCall { api.deleteLastGame(id) }
    suspend fun deleteRound(id: String, roundId: String): ApiResult<Unit> = safeUnitApiCall { api.deleteRound(id, roundId) }
    suspend fun deleteGameSet(id: String): ApiResult<Unit> = safeUnitApiCall { api.deleteGameSet(id) }
}

internal suspend fun <T> safeApiCall(call: suspend () -> retrofit2.Response<T>): ApiResult<T> {
    return try {
        val response = call()
        if (response.isSuccessful) {
            response.body()?.let { ApiResult.Success(it) }
                ?: ApiResult.Error("Empty response body")
        } else {
            ApiResult.Error("API error: ${response.code()} ${response.message()}", response.code())
        }
    } catch (e: Exception) {
        ApiResult.Error(e.message ?: "Unknown error")
    }
}

/** Like [safeApiCall], but for endpoints where a successful response may have no body (e.g. 204). */
internal suspend fun safeUnitApiCall(call: suspend () -> retrofit2.Response<Unit>): ApiResult<Unit> {
    return try {
        val response = call()
        if (response.isSuccessful) {
            ApiResult.Success(Unit)
        } else {
            ApiResult.Error("API error: ${response.code()} ${response.message()}", response.code())
        }
    } catch (e: Exception) {
        ApiResult.Error(e.message ?: "Unknown error")
    }
}
