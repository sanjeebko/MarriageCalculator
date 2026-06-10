package com.sanjeeb.marriagecalculator.data.model

import com.google.gson.annotations.SerializedName

data class MarriageGame(
    @SerializedName("id") val id: String = "",
    @SerializedName("sequence") val sequence: Int = 0,
    @SerializedName("marriageGameRoundId") val marriageGameRoundId: String = "",
    @SerializedName("winnerId") val winnerId: String = "",
    @SerializedName("dealerId") val dealerId: String = "",
    @SerializedName("totalMaal") val totalMaal: Int = 0,
    @SerializedName("closedRound") val closedRound: Boolean = false,
    @SerializedName("createdTime") val createdTime: String = "",
    @SerializedName("marriageGameScores") val marriageGameScores: Map<String, MarriageGameScore>? = null
)

data class CreateMarriageGameRequest(
    @SerializedName("sequence") val sequence: Int,
    @SerializedName("marriageGameRoundId") val marriageGameRoundId: String,
    @SerializedName("winnerId") val winnerId: String,
    @SerializedName("dealerId") val dealerId: String,
    @SerializedName("totalMaal") val totalMaal: Int = 0,
    @SerializedName("closedRound") val closedRound: Boolean = false,
    @SerializedName("scores") val scores: List<CreateScoreRequest> = emptyList()
)

data class CreateScoreRequest(
    @SerializedName("playerId") val playerId: String,
    @SerializedName("seen") val seen: Boolean = false,
    @SerializedName("playing") val playing: Boolean = true,
    @SerializedName("maal") val maal: Int = 0,
    @SerializedName("bonusPoint") val bonusPoint: Int = 0,
    @SerializedName("duply") val duply: Boolean = false,
    @SerializedName("winner") val winner: Boolean = false,
    @SerializedName("deal") val deal: Boolean = false,
    @SerializedName("position") val position: Int = 0
)
