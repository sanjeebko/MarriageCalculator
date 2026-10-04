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
-keepattributes InnerClasses,EnclosingMethod
-keep class com.google.gson.** { *; }
-keep class * extends com.google.gson.TypeAdapter
-keep class * implements com.google.gson.TypeAdapterFactory
-keep class * implements com.google.gson.JsonSerializer
-keep class * implements com.google.gson.JsonDeserializer
-keep,allowobfuscation,allowshrinking class * extends com.google.gson.reflect.TypeToken
-keepclassmembers class * {
    @com.google.gson.annotations.SerializedName <fields>;
}

# Retrofit / OkHttp / Coroutines rules
-keep,allowobfuscation,allowshrinking interface retrofit2.Call
-keep,allowobfuscation,allowshrinking class retrofit2.Response
-keep,allowobfuscation,allowshrinking class kotlin.coroutines.Continuation
-keep class retrofit2.** { *; }
-keep interface retrofit2.** { *; }
-keep interface np.com.sanjeeb.marriagecalculator.data.remote.** { *; }
-keepclassmembers interface np.com.sanjeeb.marriagecalculator.data.remote.** {
    <methods>;
}
-keepclassmembers class * {
    @retrofit2.http.* <methods>;
}
-keepclassmembers enum * { *; }
-dontwarn retrofit2.**
-dontwarn okhttp3.**
# Crashlytics — keep exception metadata so stack traces deobfuscate usefully (#137)
-keepattributes *Annotation*
-keepattributes SourceFile,LineNumberTable
-keep public class * extends java.lang.Exception
