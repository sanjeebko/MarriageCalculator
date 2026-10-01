# Project specific ProGuard rules

# Preserve line numbers and source file names for stack trace deobfuscation
-keepattributes SourceFile,LineNumberTable
-renamesourcefileattribute SourceFile

# Preserve data models and entities for Gson serialization and Room
-keep class np.com.sanjeeb.marriagecalculator.data.model.** { *; }
-keep class np.com.sanjeeb.marriagecalculator.data.local.entity.** { *; }

# Gson rules
-keepattributes Signature
-keepattributes *Annotation*
-keepclassmembers class * {
    @com.google.gson.annotations.SerializedName <fields>;
}

# Retrofit / OkHttp rules
-keepattributes EnclosingMethod
-keepclassmembers enum * { *; }