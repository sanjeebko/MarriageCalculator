package np.com.sanjeeb.marriagecalculator.data.remote

import np.com.sanjeeb.marriagecalculator.data.model.GameSettings
import com.google.gson.annotations.SerializedName
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

data class CalculateScoreRequest(
    @SerializedName("settings") val settings: GameSettings,
    @SerializedName("winnerId") val winnerId: Int,
    @SerializedName("playerScores") val playerScores: List<PlayerScoreInput>
)

data class PlayerScoreInput(
    @SerializedName("playerId") val playerId: Int,
    @SerializedName("playerName") val playerName: String = "",
    @SerializedName("seen") val seen: Boolean = false,
    @SerializedName("playing") val playing: Boolean = true,
    @SerializedName("maal") val maal: Int = 0,
    @SerializedName("duply") val duply: Boolean = false
)

data class CalculateScoreResponse(
    @SerializedName("isValid") val isValid: Boolean = false,
    @SerializedName("message") val message: String = "",
    @SerializedName("results") val results: List<PlayerScoreResult> = emptyList(),
    @SerializedName("totalMaal") val totalMaal: Int = 0
)

data class PlayerScoreResult(
    @SerializedName("playerId") val playerId: Int = 0,
    @SerializedName("playerName") val playerName: String = "",
    @SerializedName("seen") val seen: Boolean = false,
    @SerializedName("winner") val winner: Boolean = false,
    @SerializedName("duply") val duply: Boolean = false,
    @SerializedName("maal") val maal: Int = 0,
    @SerializedName("score") val score: Int = 0,
    @SerializedName("moneyWon") val moneyWon: Double = 0.0
)

interface ScoringApiService {
    @POST("Scoring/calculate")
    suspend fun calculateScore(@Body request: CalculateScoreRequest): Response<CalculateScoreResponse>
}
