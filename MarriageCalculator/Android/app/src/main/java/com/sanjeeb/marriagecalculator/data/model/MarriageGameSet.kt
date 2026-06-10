package com.sanjeeb.marriagecalculator.data.model

import com.google.gson.annotations.SerializedName

data class MarriageGameSet(
    @SerializedName("id") val id: String = "",
    @SerializedName("name") val name: String = "",
    @SerializedName("lastPlayed") val lastPlayed: String = "",
    @SerializedName("created") val created: String = "",
    @SerializedName("isActive") val isActive: Boolean = true,
    @SerializedName("gameSettingsId") val gameSettingsId: String = "",
    @SerializedName("gameSettings") val gameSettings: GameSettings? = null,
    @SerializedName("gameSetPlayers") val gameSetPlayers: Map<String, MarriageGameSetPlayer>? = null,
    @SerializedName("rounds") val rounds: List<MarriageGameRound>? = null
)

data class MarriageGameSetPlayer(
    @SerializedName("id") val id: String = "",
    @SerializedName("marriageGameSetId") val marriageGameSetId: String = "",
    @SerializedName("playerId") val playerId: String = "",
    @SerializedName("position") val position: Int = 0,
    @SerializedName("isActive") val isActive: Boolean = true,
    @SerializedName("player") val player: Player? = null
)

data class CreateGameSetRequest(
    @SerializedName("name") val name: String,
    @SerializedName("gameSettingsId") val gameSettingsId: String,
    @SerializedName("playerIds") val playerIds: List<String> = emptyList()
)
