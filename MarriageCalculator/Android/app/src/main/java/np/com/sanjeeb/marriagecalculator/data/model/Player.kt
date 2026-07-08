package np.com.sanjeeb.marriagecalculator.data.model

import com.google.gson.annotations.SerializedName

data class Player(
    @SerializedName("id") val id: String = "",
    @SerializedName("name") val name: String = "",
    @SerializedName("email") val email: String = "",
    @SerializedName("deleted") val deleted: Boolean = false,
    @SerializedName("selected") val selected: Boolean = false,
    @SerializedName("photoUri") val photoUri: String? = null
)

data class CreatePlayerRequest(
    @SerializedName("name") val name: String,
    @SerializedName("email") val email: String = "",
    @SerializedName("photoUri") val photoUri: String? = null
)

data class UpdatePlayerRequest(
    @SerializedName("name") val name: String,
    @SerializedName("email") val email: String = "",
    @SerializedName("deleted") val deleted: Boolean = false,
    @SerializedName("selected") val selected: Boolean = false,
    @SerializedName("photoUri") val photoUri: String? = null
)
