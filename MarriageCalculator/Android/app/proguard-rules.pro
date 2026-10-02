# Project specific ProGuard rules

# Preserve line numbers and source file names for stack trace deobfuscation
-keepattributes SourceFile,LineNumberTable
-renamesourcefileattribute SourceFile

# Preserve data models, Room entities, DAOs, and network DTOs
-keep class np.com.sanjeeb.marriagecalculator.data.** { *; }
-keepclassmembers class np.com.sanjeeb.marriagecalculator.data.** { *; }

# Preserve UI models and state classes used across Navigation and ViewModels
-keep class np.com.sanjeeb.marriagecalculator.ui.dashboard.EnrichedActiveGame { *; }
-keep class np.com.sanjeeb.marriagecalculator.ui.dashboard.DashboardUiState { *; }
-keep class np.com.sanjeeb.marriagecalculator.ui.playgame.PlayGameUiState { *; }
-keep class np.com.sanjeeb.marriagecalculator.ui.playgame.PlayerStandings { *; }
-keep class np.com.sanjeeb.marriagecalculator.ui.playgame.GameEntry { *; }
-keep class np.com.sanjeeb.marriagecalculator.ui.playgame.RoundGroup { *; }
-keep class np.com.sanjeeb.marriagecalculator.ui.scoreboard.RoundPlayerEntry { *; }

# Room
-keep class * extends androidx.room.RoomDatabase
-dontwarn androidx.room.paging.**

# Gson rules
-keepattributes Signature
-keepattributes *Annotation*
-keepclassmembers class * {
    @com.google.gson.annotations.SerializedName <fields>;
}

# Retrofit / OkHttp rules
-keepattributes EnclosingMethod
-keepclassmembers enum * { *; }
-dontwarn retrofit2.**
-dontwarn okhttp3.**