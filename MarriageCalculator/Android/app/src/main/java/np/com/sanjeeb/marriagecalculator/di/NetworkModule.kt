package np.com.sanjeeb.marriagecalculator.di

import com.google.gson.Gson
import np.com.sanjeeb.marriagecalculator.BuildConfig
import np.com.sanjeeb.marriagecalculator.data.remote.*
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
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
    fun provideGson(): Gson = Gson()

    @Provides
    @Singleton
    fun provideOkHttpClient(sessionManager: SessionManager): OkHttpClient {
        val logging = HttpLoggingInterceptor().apply {
            level = if (BuildConfig.DEBUG)
                HttpLoggingInterceptor.Level.BODY
            else
                HttpLoggingInterceptor.Level.NONE
        }
        return OkHttpClient.Builder()
            .addInterceptor { chain ->
                val requestBuilder = chain.request().newBuilder()
                sessionManager.getAuthToken()?.let { token ->
                    requestBuilder.addHeader("Authorization", "Bearer $token")
                }
                chain.proceed(requestBuilder.build())
            }
            .addInterceptor(logging) // Add logging AFTER auth to see headers in log
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(30, TimeUnit.SECONDS)
            .build()
    }

    @Provides
    @Singleton
    fun provideRetrofit(okHttpClient: OkHttpClient, gson: Gson): Retrofit {
        return Retrofit.Builder()
            .baseUrl(BuildConfig.API_BASE_URL)
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create(gson))
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

    @Provides
    @Singleton
    fun provideScoringApiService(retrofit: Retrofit): ScoringApiService {
        return retrofit.create(ScoringApiService::class.java)
    }

    @Provides
    @Singleton
    fun provideUserApiService(retrofit: Retrofit): UserApiService {
        return retrofit.create(UserApiService::class.java)
    }

    @Provides
    @Singleton
    fun provideFriendApiService(retrofit: Retrofit): FriendApiService {
        return retrofit.create(FriendApiService::class.java)
    }

    @Provides
    @Singleton
    fun provideAuthApiService(retrofit: Retrofit): AuthApiService {
        return retrofit.create(AuthApiService::class.java)
    }
}
