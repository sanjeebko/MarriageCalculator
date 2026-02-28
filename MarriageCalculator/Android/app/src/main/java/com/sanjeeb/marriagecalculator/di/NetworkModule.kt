package com.sanjeeb.marriagecalculator.di

import com.sanjeeb.marriagecalculator.BuildConfig
import com.sanjeeb.marriagecalculator.data.remote.*
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object NetworkModule {

    @Provides
    @Singleton
    fun provideOkHttpClient(): OkHttpClient {
        val logging = HttpLoggingInterceptor().apply {
            level = if (BuildConfig.DEBUG)
                HttpLoggingInterceptor.Level.BODY
            else
                HttpLoggingInterceptor.Level.NONE
        }
        return OkHttpClient.Builder()
            .addInterceptor(logging)
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(30, TimeUnit.SECONDS)
            .build()
    }

    @Provides
    @Singleton
    fun provideRetrofit(okHttpClient: OkHttpClient): Retrofit {
        return Retrofit.Builder()
            .baseUrl(BuildConfig.API_BASE_URL)
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }

    @Provides
    @Singleton
    fun providePlayerApiService(retrofit: Retrofit): PlayerApiService {
        return retrofit.create(PlayerApiService::class.java)
    }

    @Provides
    @Singleton
    fun provideGameSettingsApiService(retrofit: Retrofit): GameSettingsApiService {
        return retrofit.create(GameSettingsApiService::class.java)
    }

    @Provides
    @Singleton
    fun provideMarriageGameSetApiService(retrofit: Retrofit): MarriageGameSetApiService {
        return retrofit.create(MarriageGameSetApiService::class.java)
    }

    @Provides
    @Singleton
    fun provideMarriageGameApiService(retrofit: Retrofit): MarriageGameApiService {
        return retrofit.create(MarriageGameApiService::class.java)
    }
}
