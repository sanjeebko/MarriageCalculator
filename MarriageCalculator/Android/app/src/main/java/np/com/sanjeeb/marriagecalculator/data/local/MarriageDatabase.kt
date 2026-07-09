package np.com.sanjeeb.marriagecalculator.data.local

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase

@Database(
    entities = [
        PlayerEntity::class,
        GameSettingsEntity::class,
        GameSetEntity::class,
        GameSetPlayerEntity::class,
        RoundEntity::class,
        RoundScoreEntity::class
    ],
    version = 3,
    exportSchema = false
)
abstract class MarriageDatabase : RoomDatabase() {
    abstract fun playerDao(): PlayerDao
    abstract fun gameSettingsDao(): GameSettingsDao
    abstract fun gameSetDao(): GameSetDao
    abstract fun gameSetPlayerDao(): GameSetPlayerDao
    abstract fun roundDao(): RoundDao
    abstract fun roundScoreDao(): RoundScoreDao

    companion object {
        @Volatile
        private var INSTANCE: MarriageDatabase? = null

        fun getInstance(context: Context): MarriageDatabase {
            return INSTANCE ?: synchronized(this) {
                INSTANCE ?: Room.databaseBuilder(
                    context.applicationContext,
                    MarriageDatabase::class.java,
                    "marriage_calculator.db"
                ).fallbackToDestructiveMigration()
                 .build().also { INSTANCE = it }
            }
        }
    }
}
