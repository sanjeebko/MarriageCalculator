package com.sanjeeb.marriagecalculator.di

import android.content.Context
import com.sanjeeb.marriagecalculator.data.local.*
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object DatabaseModule {

    @Provides
    @Singleton
    fun provideDatabase(@ApplicationContext context: Context): MarriageDatabase {
        return MarriageDatabase.getInstance(context)
    }

    @Provides
    fun providePlayerDao(db: MarriageDatabase): PlayerDao = db.playerDao()

    @Provides
    fun provideGameSettingsDao(db: MarriageDatabase): GameSettingsDao = db.gameSettingsDao()

    @Provides
    fun provideGameSetDao(db: MarriageDatabase): GameSetDao = db.gameSetDao()

    @Provides
    fun provideGameSetPlayerDao(db: MarriageDatabase): GameSetPlayerDao = db.gameSetPlayerDao()

    @Provides
    fun provideRoundDao(db: MarriageDatabase): RoundDao = db.roundDao()

    @Provides
    fun provideRoundScoreDao(db: MarriageDatabase): RoundScoreDao = db.roundScoreDao()
}
