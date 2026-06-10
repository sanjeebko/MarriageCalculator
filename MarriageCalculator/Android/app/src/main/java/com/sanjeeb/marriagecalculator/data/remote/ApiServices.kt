package com.sanjeeb.marriagecalculator.data.remote

import com.sanjeeb.marriagecalculator.data.model.*
import retrofit2.Response
import retrofit2.http.*

interface PlayerApiService {
    @GET("Players")
    suspend fun getPlayers(): Response<List<Player>>

    @GET("Players/{id}")
    suspend fun getPlayer(@Path("id") id: String): Response<Player>

    @POST("Players")
    suspend fun createPlayer(@Body request: CreatePlayerRequest): Response<Player>

    @PUT("Players/{id}")
    suspend fun updatePlayer(@Path("id") id: String, @Body request: UpdatePlayerRequest): Response<Player>

    @DELETE("Players/{id}")
    suspend fun deletePlayer(@Path("id") id: String): Response<Unit>
}

interface GameSettingsApiService {
    @GET("GameSettings")
    suspend fun getGameSettings(): Response<List<GameSettings>>

    @GET("GameSettings/{id}")
    suspend fun getGameSetting(@Path("id") id: String): Response<GameSettings>

    @POST("GameSettings")
    suspend fun createGameSettings(@Body request: CreateGameSettingsRequest): Response<GameSettings>

    @PUT("GameSettings/{id}")
    suspend fun updateGameSettings(@Path("id") id: String, @Body request: CreateGameSettingsRequest): Response<GameSettings>

    @DELETE("GameSettings/{id}")
    suspend fun deleteGameSettings(@Path("id") id: String): Response<Unit>
}

interface MarriageGameSetApiService {
    @GET("MarriageGameSets")
    suspend fun getGameSets(): Response<List<MarriageGameSet>>

    @GET("MarriageGameSets/{id}")
    suspend fun getGameSet(@Path("id") id: String): Response<MarriageGameSet>

    @POST("MarriageGameSets")
    suspend fun createGameSet(@Body request: CreateGameSetRequest): Response<MarriageGameSet>

    @PUT("MarriageGameSets/{id}")
    suspend fun updateGameSet(@Path("id") id: String, @Body request: CreateGameSetRequest): Response<MarriageGameSet>

    @DELETE("MarriageGameSets/{id}")
    suspend fun deleteGameSet(@Path("id") id: String): Response<Unit>

    @POST("MarriageGameSets/{id}/transfer-host")
    suspend fun transferHost(
        @Path("id") id: String,
        @Body request: TransferHostDto
    ): Response<Unit>
}

interface MarriageGameApiService {
    @GET("MarriageGames")
    suspend fun getGames(): Response<List<MarriageGame>>

    @GET("MarriageGames/{id}")
    suspend fun getGame(@Path("id") id: String): Response<MarriageGame>

    @GET("MarriageGames/round/{roundId}")
    suspend fun getGamesByRound(@Path("roundId") roundId: String): Response<List<MarriageGame>>

    @POST("MarriageGames")
    suspend fun createGame(@Body request: CreateMarriageGameRequest): Response<MarriageGame>

    @PUT("MarriageGames/{id}")
    suspend fun updateGame(@Path("id") id: String, @Body request: CreateMarriageGameRequest): Response<MarriageGame>

    @DELETE("MarriageGames/{id}")
    suspend fun deleteGame(@Path("id") id: String): Response<Unit>
}
