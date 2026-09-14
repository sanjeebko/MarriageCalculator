package np.com.sanjeeb.marriagecalculator.di

import np.com.sanjeeb.marriagecalculator.data.local.*
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent

@Module
@InstallIn(SingletonComponent::class)
object DatabaseModule {

    @Provides
    fun provideDatabase(provider: MarriageDatabaseProvider): MarriageDatabase {
        return provider.getDatabase()
    }

    @Provides
    fun providePlayerDao(provider: MarriageDatabaseProvider): PlayerDao = provider.getDatabase().playerDao()

    @Provides
    fun provideGameSettingsDao(provider: MarriageDatabaseProvider): GameSettingsDao = provider.getDatabase().gameSettingsDao()

    @Provides
    fun provideGameSetDao(provider: MarriageDatabaseProvider): GameSetDao = provider.getDatabase().gameSetDao()

    @Provides
    fun provideGameSetPlayerDao(provider: MarriageDatabaseProvider): GameSetPlayerDao = provider.getDatabase().gameSetPlayerDao()

    @Provides
    fun provideRoundDao(provider: MarriageDatabaseProvider): RoundDao = provider.getDatabase().roundDao()

    @Provides
    fun provideRoundScoreDao(provider: MarriageDatabaseProvider): RoundScoreDao = provider.getDatabase().roundScoreDao()

    @Provides
    fun provideActivityLogDao(provider: MarriageDatabaseProvider): ActivityLogDao = provider.getDatabase().activityLogDao()
}

