package com.sanjeeb.marriagecalculator.data.model

import com.google.gson.annotations.SerializedName

data class Player(
    @SerializedName("id") val id: Int = 0,
    @SerializedName("name") val name: String = "",
    @SerializedName("email") val email: String = "",
    @SerializedName("deleted") val deleted: Boolean = false,
    @SerializedName("selected") val selected: Boolean = false
)

data class CreatePlayerRequest(
    @SerializedName("name") val name: String,
    @SerializedName("email") val email: String = ""
)

data class UpdatePlayerRequest(
    @SerializedName("name") val name: String,
    @SerializedName("email") val email: String = "",
    @SerializedName("deleted") val deleted: Boolean = false,
    @SerializedName("selected") val selected: Boolean = false
)
