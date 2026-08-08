package np.com.sanjeeb.marriagecalculator.data.model

import com.google.gson.annotations.SerializedName

data class MarriageGameSet(
    @SerializedName("id") val id: String = "",
    @SerializedName("hostUserId") val hostUserId: String = "",
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

data class SubmitRoundRequest(
    @SerializedName("winnerId") val winnerId: String,
    @SerializedName("dealerId") val dealerId: String,
    @SerializedName("players") val players: List<RoundPlayerInput>
)

data class RoundPlayerInput(
    @SerializedName("playerId") val playerId: String,
    @SerializedName("seen") val seen: Boolean = false,
    @SerializedName("duply") val duply: Boolean = false,
    @SerializedName("maal") val maal: Int = 0
)
