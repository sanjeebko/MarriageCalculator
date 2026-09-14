package np.com.sanjeeb.marriagecalculator.data.local

import android.content.Context
import androidx.room.Room
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.StateFlow
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import java.io.File
import java.util.concurrent.ConcurrentHashMap
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class MarriageDatabaseProvider @Inject constructor(
    @param:ApplicationContext private val context: Context,
    private val sessionManager: SessionManager
) {
    private val databases = ConcurrentHashMap<String, MarriageDatabase>()

    val userKeyFlow: StateFlow<String>
        get() = sessionManager.userKeyFlow

    fun getDatabase(): MarriageDatabase {
        val key = sessionManager.getCurrentUserKey()
        return getDatabaseForUserKey(key)
    }

    @Synchronized
    fun getDatabaseForUserKey(userKey: String): MarriageDatabase {
        val safeKey = userKey.ifBlank { "guest" }
        return databases.getOrPut(safeKey) {
            val dbName = "marriage_calculator_${safeKey}.db"
            maybeMigrateLegacyDatabase(dbName, safeKey)
            Room.databaseBuilder(
                context.applicationContext,
                MarriageDatabase::class.java,
                dbName
            ).fallbackToDestructiveMigration(dropAllTables = true)
             .build()
        }
    }

    private fun maybeMigrateLegacyDatabase(newDbName: String, userKey: String) {
        if (userKey == "guest") {
            try {
                val legacyDbFile = context.getDatabasePath("marriage_calculator.db")
                val targetDbFile = context.getDatabasePath(newDbName)
                if (legacyDbFile.exists() && !targetDbFile.exists()) {
                    targetDbFile.parentFile?.mkdirs()
                    legacyDbFile.copyTo(targetDbFile, overwrite = false)
                    val legacyWal = File(legacyDbFile.path + "-wal")
                    if (legacyWal.exists()) {
                        legacyWal.copyTo(File(targetDbFile.path + "-wal"), overwrite = false)
                    }
                    val legacyShm = File(legacyDbFile.path + "-shm")
                    if (legacyShm.exists()) {
                        legacyShm.copyTo(File(targetDbFile.path + "-shm"), overwrite = false)
                    }
                }
            } catch (e: Exception) {
                // Ignore migration errors; Room will create an empty database
            }
        }
    }

    fun closeAndClear() {
        databases.values.forEach { db ->
            try {
                if (db.isOpen) {
                    db.close()
                }
            } catch (ignored: Exception) {}
        }
        databases.clear()
    }
}
