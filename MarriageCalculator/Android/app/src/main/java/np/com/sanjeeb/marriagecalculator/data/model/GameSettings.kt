package np.com.sanjeeb.marriagecalculator.data.model

import com.google.gson.annotations.SerializedName

enum class Currency {
    @SerializedName("0") NPR_Rupee,
    @SerializedName("1") INR_Rupee,
    @SerializedName("2") GBP_Pence,
    @SerializedName("3") USD_Cent,
    @SerializedName("4") AUD_Cent;

    fun displayName(): String = when (this) {
        NPR_Rupee -> "NPR (₨)"
        INR_Rupee -> "INR (₹)"
        GBP_Pence -> "GBP (p)"
        USD_Cent -> "USD (¢)"
        AUD_Cent -> "AUD (¢)"
    }
}

enum class FoulPointBonusType {
    @SerializedName("0") NEXT_GAME,
    @SerializedName("1") CURRENT_GAME;

    fun displayName(): String = when (this) {
        NEXT_GAME -> "Next Game"
        CURRENT_GAME -> "Current Game"
    }
}

data class GameSettings(
    @SerializedName("id") val id: String = "",
    @SerializedName("murder") val murder: Boolean = true,
    @SerializedName("kidnap") val kidnap: Boolean = false,
    @SerializedName("seenPoint") val seenPoint: Int = 3,
    @SerializedName("unseenPoint") val unseenPoint: Int = 10,
    @SerializedName("pointRate") val pointRate: Double = 10.0,
    @SerializedName("currency") val currency: Currency = Currency.NPR_Rupee,
    @SerializedName("dublee") val dublee: Boolean = true,
    @SerializedName("dubleePointLess") val dubleePointLess: Boolean = true,
    @SerializedName("dubleePointBonus") val dubleePointBonus: Int = 0,
    @SerializedName("foulPoint") val foulPoint: Int = 15,
    @SerializedName("foulPointBonus") val foulPointBonus: FoulPointBonusType = FoulPointBonusType.NEXT_GAME,
    @SerializedName("audio") val audio: Boolean = true
) {
    companion object {
        fun default() = GameSettings()
    }
}

data class CreateGameSettingsRequest(
    @SerializedName("murder") val murder: Boolean = true,
    @SerializedName("kidnap") val kidnap: Boolean = false,
    @SerializedName("seenPoint") val seenPoint: Int = 3,
    @SerializedName("unseenPoint") val unseenPoint: Int = 10,
    @SerializedName("pointRate") val pointRate: Double = 10.0,
    @SerializedName("currency") val currency: Currency = Currency.NPR_Rupee,
    @SerializedName("dublee") val dublee: Boolean = true,
    @SerializedName("dubleePointLess") val dubleePointLess: Boolean = true,
    @SerializedName("dubleePointBonus") val dubleePointBonus: Int = 0,
    @SerializedName("foulPoint") val foulPoint: Int = 15,
    @SerializedName("foulPointBonus") val foulPointBonus: FoulPointBonusType = FoulPointBonusType.NEXT_GAME,
    @SerializedName("audio") val audio: Boolean = true
)
