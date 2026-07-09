package np.com.sanjeeb.marriagecalculator.data.repository

import np.com.sanjeeb.marriagecalculator.data.local.*
import np.com.sanjeeb.marriagecalculator.data.model.*
import np.com.sanjeeb.marriagecalculator.data.remote.MarriageGameSetApiService
import np.com.sanjeeb.marriagecalculator.data.remote.PlayerApiService
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.first
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Offline-first repository: saves locally first, then syncs to API when available.
 */
@Singleton
class OfflineGameRepository @Inject constructor(
    private val playerDao: PlayerDao,
    private val gameSetDao: GameSetDao,
    private val gameSetPlayerDao: GameSetPlayerDao,
    private val gameSettingsDao: GameSettingsDao,
    private val roundDao: RoundDao,
    private val roundScoreDao: RoundScoreDao
) {

    // ── Players ──

    fun getAllPlayers(): Flow<List<Player>> = playerDao.getAllPlayers().map { entities ->
        entities.map { it.toDomainModel() }
    }

    suspend fun createGuestPlayer(name: String): Int {
        val entity = PlayerEntity(name = name, isGuest = true)
        return playerDao.insert(entity).toInt()
    }

    suspend fun createRegisteredPlayer(name: String, email: String, photoUri: String? = null, remoteId: String? = null): Int {
        val entity = PlayerEntity(name = name, email = email, isGuest = false, photoUri = photoUri, remoteId = remoteId)
        return playerDao.insert(entity).toInt()
    }

    suspend fun updatePlayerRemoteId(playerId: Int, remoteId: String) {
        val player = playerDao.getById(playerId) ?: return
        playerDao.insert(player.copy(remoteId = remoteId))
    }

    suspend fun updateGameSetPlayerPositions(gameSetId: Int, playerIds: List<Int>) {
        val playerLinks = playerIds.mapIndexed { index, playerId ->
            GameSetPlayerEntity(gameSetId = gameSetId, playerId = playerId, seatPosition = index)
        }
        gameSetPlayerDao.insertAll(playerLinks)
    }

    suspend fun getPlayerEntity(id: Int): PlayerEntity? = playerDao.getById(id)

    suspend fun getPlayerEntityByName(name: String): PlayerEntity? {
        return playerDao.getAllPlayers().first().find { it.name.equals(name, ignoreCase = true) }
    }

    suspend fun getPlayerByEmail(email: String): Player? {
        return playerDao.getByEmail(email)?.toDomainModel()
    }

    suspend fun updatePlayerEmailAndName(playerId: Int, email: String, name: String) {
        val player = playerDao.getById(playerId) ?: return
        val updated = player.copy(email = email, name = name, isGuest = false)
        playerDao.insert(updated)
    }

    suspend fun createGuestPlayers(names: List<String>): List<Int> {
        val entities = names.map { PlayerEntity(name = it, isGuest = true) }
        return playerDao.insertAll(entities).map { it.toInt() }
    }

    // ── Game Set ──

    suspend fun createGameSet(name: String, settings: GameSettings, playerIds: List<Int>): Int {
        val settingsEntity = settings.toEntity()
        val settingsId = gameSettingsDao.insert(settingsEntity).toInt()

        val gameSetEntity = GameSetEntity(name = name, settingsId = settingsId)
        val gameSetId = gameSetDao.insert(gameSetEntity).toInt()

        val playerLinks = playerIds.mapIndexed { index, playerId ->
            GameSetPlayerEntity(gameSetId = gameSetId, playerId = playerId, seatPosition = index)
        }
        gameSetPlayerDao.insertAll(playerLinks)

        return gameSetId
    }

    suspend fun createGameSetWithRemoteId(name: String, settingsId: Int, playerIds: List<Int>, remoteId: String): Int {
        val gameSetEntity = GameSetEntity(name = name, settingsId = settingsId, remoteId = remoteId, synced = true)
        val gameSetId = gameSetDao.insert(gameSetEntity).toInt()

        val playerLinks = playerIds.mapIndexed { index, playerId ->
            GameSetPlayerEntity(gameSetId = gameSetId, playerId = playerId, seatPosition = index)
        }
        gameSetPlayerDao.insertAll(playerLinks)

        return gameSetId
    }

    suspend fun createGameSettingsWithRemoteId(settings: GameSettings, remoteId: String): Int {
        val settingsEntity = settings.toEntity().copy(remoteId = remoteId)
        return gameSettingsDao.insert(settingsEntity).toInt()
    }

    suspend fun getActiveGameSetsWithDetails(): List<MarriageGameSet> {
        val dateFormat = java.text.SimpleDateFormat("yyyy-MM-dd HH:mm:ss", java.util.Locale.getDefault())
        val entities = try {
            gameSetDao.getActiveGameSets().first()
        } catch (e: Exception) {
            emptyList()
        }

        return entities.map { entity ->
            val settings = getGameSettings(entity.settingsId)
            val players = getGameSetPlayers(entity.id)
            val playersMap = players.associate { p ->
                p.id to MarriageGameSetPlayer(
                    id = p.id,
                    marriageGameSetId = entity.id.toString(),
                    playerId = p.id,
                    player = p
                )
            }
            MarriageGameSet(
                id = entity.id.toString(),
                name = entity.name.ifEmpty { "Local Game #${entity.id}" },
                hostUserId = "",
                lastPlayed = dateFormat.format(java.util.Date(entity.createdAt)),
                created = dateFormat.format(java.util.Date(entity.createdAt)),
                isActive = entity.isActive,
                gameSettingsId = entity.settingsId.toString(),
                gameSettings = settings,
                gameSetPlayers = playersMap
            )
        }
    }

    fun getActiveGameSets(): Flow<List<GameSetEntity>> = gameSetDao.getActiveGameSets()

    suspend fun getGameSetPlayers(gameSetId: Int): List<Player> {
        return gameSetPlayerDao.getPlayersForGameSet(gameSetId).map { it.toDomainModel() }
    }

    suspend fun getGameSet(gameSetId: Int): GameSetEntity? {
        return gameSetDao.getById(gameSetId)
    }

    suspend fun getGameSettings(settingsId: Int): GameSettings? {
        return gameSettingsDao.getById(settingsId)?.toDomainModel()
    }

    suspend fun settleGame(gameSetId: Int) {
        gameSetDao.settle(gameSetId)
    }

    // ── Rounds ──

    suspend fun saveRound(
        gameSetId: Int,
        winnerId: Int,
        dealerId: Int,
        totalMaal: Int,
        playerScores: List<RoundScoreData>
    ): Int {
        val roundNumber = roundDao.getRoundCount(gameSetId) + 1
        val roundEntity = RoundEntity(
            gameSetId = gameSetId,
            roundNumber = roundNumber,
            winnerId = winnerId,
            dealerId = dealerId,
            totalMaal = totalMaal
        )
        val roundId = roundDao.insert(roundEntity).toInt()

        val scoreEntities = playerScores.map {
            RoundScoreEntity(
                roundId = roundId,
                playerId = it.playerId,
                score = it.score,
                maal = it.maal,
                isSeen = it.isSeen,
                isWinner = it.isWinner,
                isDublee = it.isDublee
            )
        }
        roundScoreDao.insertAll(scoreEntities)
        return roundId
    }

    /** Marks the most recently played game as closing out its logical round early. */
    suspend fun closeCurrentRound(gameSetId: Int) {
        val latest = roundDao.getLatestGame(gameSetId) ?: return
        roundDao.closeRoundAt(latest.id)
    }

    /** Removes only the most recently played game (undo), e.g. to fix a mistake. */
    suspend fun deleteLastGame(gameSetId: Int) {
        val latest = roundDao.getLatestGame(gameSetId) ?: return
        roundScoreDao.deleteForRound(latest.id)
        roundDao.deleteById(latest.id)
    }

    /**
     * Removes every game (and score) belonging to one logical round, then compacts the
     * remaining games' sequence numbers so they stay contiguous - matching the server's
     * "renumber later rounds down" behavior.
     */
    suspend fun deleteRoundGames(gameSetId: Int, gameIds: List<Int>) {
        if (gameIds.isEmpty()) return
        roundScoreDao.deleteForRounds(gameIds)
        roundDao.deleteByIds(gameIds)

        val remaining = roundDao.getRoundsForGameSet(gameSetId).first().sortedBy { it.roundNumber }
        remaining.forEachIndexed { index, game ->
            val newNumber = index + 1
            if (game.roundNumber != newNumber) {
                roundDao.renumber(game.id, newNumber)
            }
        }
    }

    /** Deletes an entire local game set: every round, score, and player link, then the set itself. */
    suspend fun deleteGameSet(gameSetId: Int) {
        roundScoreDao.deleteForGameSet(gameSetId)
        roundDao.deleteAllForGameSet(gameSetId)
        gameSetPlayerDao.deleteForGameSet(gameSetId)
        gameSetDao.deleteById(gameSetId)
    }

    /**
     * Cleans up the local offline mirror of an online game set (created at online-creation time
     * so the Dashboard has something to show before the first sync). Without this, deleting a
     * game set only online would leave the stale local mirror row behind, and the Dashboard's
     * online/offline merge logic - which shows local rows not yet present in the remote list -
     * would misread the leftover as "not yet synced" and resurrect it into the list.
     */
    suspend fun deleteGameSetByRemoteId(remoteId: String) {
        val entity = gameSetDao.getByRemoteId(remoteId) ?: return
        deleteGameSet(entity.id)
    }

    fun getRounds(gameSetId: Int): Flow<List<RoundEntity>> =
        roundDao.getRoundsForGameSet(gameSetId)

    suspend fun getRoundScores(roundId: Int): List<RoundScoreEntity> =
        roundScoreDao.getScoresForRound(roundId)

    suspend fun getAllScoresForGameSet(gameSetId: Int): List<RoundScoreEntity> =
        roundScoreDao.getAllScoresForGameSet(gameSetId)
}

data class RoundScoreData(
    val playerId: Int,
    val score: Int,
    val maal: Int,
    val isSeen: Boolean,
    val isWinner: Boolean,
    val isDublee: Boolean
)

// ── Mapping Extensions ──

fun PlayerEntity.toDomainModel() = Player(
    id = id.toString(),
    name = name,
    email = email,
    photoUri = photoUri
)

fun GameSettings.toEntity() = GameSettingsEntity(
    murder = murder,
    kidnap = kidnap,
    seenPoint = seenPoint,
    unseenPoint = unseenPoint,
    pointRate = pointRate,
    currency = currency.ordinal,
    dublee = dublee,
    dubleePointLess = dubleePointLess
)

fun GameSettingsEntity.toDomainModel() = GameSettings(
    id = id.toString(),
    murder = murder,
    kidnap = kidnap,
    seenPoint = seenPoint,
    unseenPoint = unseenPoint,
    pointRate = pointRate,
    currency = Currency.entries.getOrElse(currency) { Currency.NPR_Rupee },
    dublee = dublee,
    dubleePointLess = dubleePointLess
)
