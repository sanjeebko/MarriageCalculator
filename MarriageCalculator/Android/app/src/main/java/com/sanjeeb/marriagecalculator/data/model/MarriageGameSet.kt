package com.sanjeeb.marriagecalculator.data.model

import com.google.gson.annotations.SerializedName

data class MarriageGameSet(
    @SerializedName("id") val id: Int = 0,
    @SerializedName("name") val name: String = "",
    @SerializedName("lastPlayed") val lastPlayed: String = "",
    @SerializedName("created") val created: String = "",
    @SerializedName("isActive") val isActive: Boolean = true,
    @SerializedName("gameSettingsId") val gameSettingsId: Int = 0,
    @SerializedName("gameSettings") val gameSettings: GameSettings? = null,
    @SerializedName("gameSetPlayers") val gameSetPlayers: Map<String, MarriageGameSetPlayer>? = null,
    @SerializedName("rounds") val rounds: List<MarriageGameRound>? = null
)

data class MarriageGameSetPlayer(
    @SerializedName("id") val id: Int = 0,
    @SerializedName("marriageGameSetId") val marriageGameSetId: Int = 0,
    @SerializedName("playerId") val playerId: Int = 0,
    @SerializedName("position") val position: Int = 0,
    @SerializedName("isActive") val isActive: Boolean = true,
    @SerializedName("player") val player: Player? = null
)

data class CreateGameSetRequest(
    @SerializedName("name") val name: String,
    @SerializedName("gameSettingsId") val gameSettingsId: Int,
    @SerializedName("playerIds") val playerIds: List<Int> = emptyList()
)
