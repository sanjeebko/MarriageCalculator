package com.sanjeeb.marriagecalculator.data.model

import com.google.gson.annotations.SerializedName

data class User(
    @SerializedName("id") val id: String = "",
    @SerializedName("userId") val userId: String = "",
    @SerializedName("displayName") val displayName: String = "",
    @SerializedName("email") val email: String = "",
    @SerializedName("createdAt") val createdAt: String = ""
)
