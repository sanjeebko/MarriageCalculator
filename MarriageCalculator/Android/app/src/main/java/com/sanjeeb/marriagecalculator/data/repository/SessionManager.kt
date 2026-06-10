package com.sanjeeb.marriagecalculator.data.repository

import android.content.Context
import android.content.SharedPreferences
import com.google.gson.Gson
import com.sanjeeb.marriagecalculator.data.model.User
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class SessionManager @Inject constructor(
    @ApplicationContext private val context: Context,
    private val gson: Gson
) {
    private val prefs: SharedPreferences = context.getSharedPreferences("user_session", Context.MODE_PRIVATE)

    fun saveSession(token: String, user: User) {
        prefs.edit()
            .putString("auth_token", token)
            .putString("user_profile", gson.toJson(user))
            .putBoolean("is_online_mode", token != "guest-token")
            .apply()
    }

    fun getAuthToken(): String? {
        return prefs.getString("auth_token", null)
    }

    fun getUserProfile(): User? {
        val userJson = prefs.getString("user_profile", null) ?: return null
        return try {
            gson.fromJson(userJson, User::class.java)
        } catch (e: Exception) {
            null
        }
    }

    fun clearSession() {
        prefs.edit().clear().apply()
    }

    fun isLoggedIn(): Boolean {
        return getAuthToken() != null && getAuthToken() != "guest-token"
    }

    fun isGuestMode(): Boolean {
        return getAuthToken() == "guest-token"
    }

    fun setOnlineMode(online: Boolean) {
        prefs.edit().putBoolean("is_online_mode", online).apply()
        if (!online) {
            // Logged out of online or switched to guest
            prefs.edit().putString("auth_token", "guest-token").apply()
        }
    }

    fun isOnlineMode(): Boolean {
        return prefs.getBoolean("is_online_mode", false) && isLoggedIn()
    }
}
