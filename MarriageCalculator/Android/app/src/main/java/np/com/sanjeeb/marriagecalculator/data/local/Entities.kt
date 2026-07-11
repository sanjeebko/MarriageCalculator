package np.com.sanjeeb.marriagecalculator.data.local

import androidx.room.*
import kotlinx.coroutines.flow.Flow

// ── Entities ──

@Entity(tableName = "players")
data class PlayerEntity(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val name: String,
    val email: String = "",
    val phone: String = "",
    val isGuest: Boolean = true,
    val remoteId: String? = null,
    val photoUri: String? = null,
    val createdAt: Long = System.currentTimeMillis()
)

@Entity(tableName = "game_settings")
data class GameSettingsEntity(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val murder: Boolean = true,
    val kidnap: Boolean = false,
    val seenPoint: Int = 3,
    val unseenPoint: Int = 10,
    val pointRate: Double = 10.0,
    val currency: Int = 0,
    val dublee: Boolean = true,
    val dubleePointLess: Boolean = true,
    val remoteId: String? = null
)

@Entity(
    tableName = "game_sets",
    foreignKeys = [
        ForeignKey(entity = GameSettingsEntity::class, parentColumns = ["id"], childColumns = ["settingsId"])
    ]
)
data class GameSetEntity(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val settingsId: Int,
    val name: String = "",
    val isActive: Boolean = true,
    val isSettled: Boolean = false,
    val createdAt: Long = System.currentTimeMillis(),
    val remoteId: String? = null,
    val synced: Boolean = false
)

@Entity(
    tableName = "game_set_players",
    primaryKeys = ["gameSetId", "playerId"],
    foreignKeys = [
        ForeignKey(entity = GameSetEntity::class, parentColumns = ["id"], childColumns = ["gameSetId"]),
        ForeignKey(entity = PlayerEntity::class, parentColumns = ["id"], childColumns = ["playerId"])
    ]
)
data class GameSetPlayerEntity(
    val gameSetId: Int,
    val playerId: Int,
    val seatPosition: Int = 0
)

@Entity(
    tableName = "rounds",
    foreignKeys = [
        ForeignKey(entity = GameSetEntity::class, parentColumns = ["id"], childColumns = ["gameSetId"]),
        ForeignKey(entity = PlayerEntity::class, parentColumns = ["id"], childColumns = ["winnerId"])
    ]
)
data class RoundEntity(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val gameSetId: Int,
    /** Overall game sequence number within the game set (1-indexed) - NOT the logical round number. */
    val roundNumber: Int,
    val winnerId: Int,
    val dealerId: Int = 0,
    val totalMaal: Int = 0,
    /** True if this game is the last one of its logical round (either N games were reached, or the round was closed early). */
    val closesRound: Boolean = false,
    /** Comma-separated player ids in seat order at the time this game was played - a later reshuffle must not rewrite history. */
    val seatOrder: String = "",
    val createdAt: Long = System.currentTimeMillis(),
    val remoteId: String? = null,
    val synced: Boolean = false
)

@Entity(
    tableName = "round_scores",
    foreignKeys = [
        ForeignKey(entity = RoundEntity::class, parentColumns = ["id"], childColumns = ["roundId"]),
        ForeignKey(entity = PlayerEntity::class, parentColumns = ["id"], childColumns = ["playerId"])
    ]
)
data class RoundScoreEntity(
    @PrimaryKey(autoGenerate = true) val id: Int = 0,
    val roundId: Int,
    val playerId: Int,
    val score: Int,
    val maal: Int = 0,
    val isSeen: Boolean = false,
    val isWinner: Boolean = false,
    val isDublee: Boolean = false
)

// ── DAOs ──

@Dao
interface PlayerDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(player: PlayerEntity): Long

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(players: List<PlayerEntity>): List<Long>

    @Query("SELECT * FROM players ORDER BY name")
    fun getAllPlayers(): Flow<List<PlayerEntity>>

    @Query("SELECT * FROM players WHERE id = :id")
    suspend fun getById(id: Int): PlayerEntity?

    @Query("SELECT * FROM players WHERE email = :email LIMIT 1")
    suspend fun getByEmail(email: String): PlayerEntity?

    @Query("SELECT * FROM players WHERE isGuest = 1")
    fun getGuestPlayers(): Flow<List<PlayerEntity>>

    @Delete
    suspend fun delete(player: PlayerEntity)
}

@Dao
interface GameSettingsDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(settings: GameSettingsEntity): Long

    @Query("SELECT * FROM game_settings WHERE id = :id")
    suspend fun getById(id: Int): GameSettingsEntity?
}

@Dao
interface GameSetDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(gameSet: GameSetEntity): Long

    @Query("SELECT * FROM game_sets WHERE isActive = 1 ORDER BY createdAt DESC")
    fun getActiveGameSets(): Flow<List<GameSetEntity>>

    @Query("SELECT * FROM game_sets ORDER BY createdAt DESC")
    fun getAllGameSets(): Flow<List<GameSetEntity>>

    @Query("SELECT * FROM game_sets WHERE id = :id")
    suspend fun getById(id: Int): GameSetEntity?

    @Query("SELECT * FROM game_sets WHERE remoteId = :remoteId LIMIT 1")
    suspend fun getByRemoteId(remoteId: String): GameSetEntity?

    @Query("UPDATE game_sets SET isSettled = 1 WHERE id = :id")
    suspend fun settle(id: Int)

    @Query("SELECT * FROM game_sets WHERE synced = 0")
    suspend fun getUnsynced(): List<GameSetEntity>

    @Query("UPDATE game_sets SET synced = 1, remoteId = :remoteId WHERE id = :localId")
    suspend fun markSynced(localId: Int, remoteId: String)

    @Query("DELETE FROM game_sets WHERE id = :id")
    suspend fun deleteById(id: Int)
}

@Dao
interface GameSetPlayerDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(gameSetPlayer: GameSetPlayerEntity)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(players: List<GameSetPlayerEntity>)

    @Query("SELECT p.* FROM players p INNER JOIN game_set_players gsp ON p.id = gsp.playerId WHERE gsp.gameSetId = :gameSetId ORDER BY gsp.seatPosition")
    suspend fun getPlayersForGameSet(gameSetId: Int): List<PlayerEntity>

    @Query("DELETE FROM game_set_players WHERE gameSetId = :gameSetId")
    suspend fun deleteForGameSet(gameSetId: Int)
}

@Dao
interface RoundDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(round: RoundEntity): Long

    @Query("SELECT * FROM rounds WHERE gameSetId = :gameSetId ORDER BY roundNumber")
    fun getRoundsForGameSet(gameSetId: Int): Flow<List<RoundEntity>>

    @Query("SELECT COUNT(*) FROM rounds WHERE gameSetId = :gameSetId")
    suspend fun getRoundCount(gameSetId: Int): Int

    @Query("SELECT * FROM rounds WHERE gameSetId = :gameSetId ORDER BY roundNumber DESC LIMIT 1")
    suspend fun getLatestGame(gameSetId: Int): RoundEntity?

    @Query("UPDATE rounds SET closesRound = 1 WHERE id = :id")
    suspend fun closeRoundAt(id: Int)

    @Query("SELECT * FROM rounds WHERE synced = 0")
    suspend fun getUnsynced(): List<RoundEntity>

    @Query("UPDATE rounds SET synced = 1, remoteId = :remoteId WHERE id = :localId")
    suspend fun markSynced(localId: Int, remoteId: String)

    @Query("DELETE FROM rounds WHERE id = :id")
    suspend fun deleteById(id: Int)

    @Query("DELETE FROM rounds WHERE id IN (:ids)")
    suspend fun deleteByIds(ids: List<Int>)

    @Query("DELETE FROM rounds WHERE gameSetId = :gameSetId")
    suspend fun deleteAllForGameSet(gameSetId: Int)

    @Query("UPDATE rounds SET roundNumber = :roundNumber WHERE id = :id")
    suspend fun renumber(id: Int, roundNumber: Int)

    @Query("UPDATE rounds SET seatOrder = :seatOrderCsv WHERE gameSetId = :gameSetId AND seatOrder = ''")
    suspend fun backfillSeatOrder(gameSetId: Int, seatOrderCsv: String)

    @Query("UPDATE rounds SET dealerId = :dealerId WHERE id = :id")
    suspend fun setDealer(id: Int, dealerId: Int)
}

@Dao
interface RoundScoreDao {
    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(score: RoundScoreEntity): Long

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(scores: List<RoundScoreEntity>)

    @Query("SELECT * FROM round_scores WHERE roundId = :roundId")
    suspend fun getScoresForRound(roundId: Int): List<RoundScoreEntity>

    @Query("SELECT rs.* FROM round_scores rs INNER JOIN rounds r ON rs.roundId = r.id WHERE r.gameSetId = :gameSetId")
    suspend fun getAllScoresForGameSet(gameSetId: Int): List<RoundScoreEntity>

    @Query("DELETE FROM round_scores WHERE roundId = :roundId")
    suspend fun deleteForRound(roundId: Int)

    @Query("DELETE FROM round_scores WHERE roundId IN (:roundIds)")
    suspend fun deleteForRounds(roundIds: List<Int>)

    @Query("DELETE FROM round_scores WHERE roundId IN (SELECT id FROM rounds WHERE gameSetId = :gameSetId)")
    suspend fun deleteForGameSet(gameSetId: Int)
}
