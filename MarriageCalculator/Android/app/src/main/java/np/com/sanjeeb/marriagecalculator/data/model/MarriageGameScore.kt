package np.com.sanjeeb.marriagecalculator.data.model

import com.google.gson.annotations.SerializedName

data class MarriageGameScore(
    @SerializedName("id") val id: String = "",
    @SerializedName("marriageGameId") val marriageGameId: String = "",
    @SerializedName("playerId") val playerId: String = "",
    @SerializedName("seen") val seen: Boolean = false,
    @SerializedName("playing") val playing: Boolean = false,
    @SerializedName("maal") val maal: Int = 0,
    @SerializedName("bonusPoint") val bonusPoint: Int = 0,
    @SerializedName("duply") val duply: Boolean = false,
    @SerializedName("winner") val winner: Boolean = false,
    @SerializedName("score") val score: Int = 0,
    @SerializedName("moneyWon") val moneyWon: Double = 0.0,
    @SerializedName("deal") val deal: Boolean = false,
    @SerializedName("position") val position: Int = 0
)
