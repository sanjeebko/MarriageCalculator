package np.com.sanjeeb.marriagecalculator.data.model

import com.google.gson.annotations.SerializedName

data class MarriageGameRound(
    @SerializedName("id") val id: String = "",
    @SerializedName("sequence") val sequence: Int = 0,
    @SerializedName("marriageGameSetId") val marriageGameSetId: String = "",
    @SerializedName("completed") val completed: Boolean = false,
    @SerializedName("marriageGames") val marriageGames: List<MarriageGame>? = null,
    @SerializedName("totalScore") val totalScore: Map<String, Double>? = null
)
