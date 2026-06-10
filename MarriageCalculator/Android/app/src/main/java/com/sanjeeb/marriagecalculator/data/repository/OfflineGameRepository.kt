package com.sanjeeb.marriagecalculator.data.repository

import com.sanjeeb.marriagecalculator.data.local.*
import com.sanjeeb.marriagecalculator.data.model.*
import com.sanjeeb.marriagecalculator.data.remote.MarriageGameSetApiService
import com.sanjeeb.marriagecalculator.data.remote.PlayerApiService
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map
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

    suspend fun createGuestPlayers(names: List<String>): List<Int> {
        val entities = names.map { PlayerEntity(name = it, isGuest = true) }
        return playerDao.insertAll(entities).map { it.toInt() }
    }

    // ── Game Set ──

    suspend fun createGameSet(settings: GameSettings, playerIds: List<Int>): Int {
        val settingsEntity = settings.toEntity()
        val settingsId = gameSettingsDao.insert(settingsEntity).toInt()

        val gameSetEntity = GameSetEntity(settingsId = settingsId)
        val gameSetId = gameSetDao.insert(gameSetEntity).toInt()

        val playerLinks = playerIds.mapIndexed { index, playerId ->
            GameSetPlayerEntity(gameSetId = gameSetId, playerId = playerId, seatPosition = index)
        }
        gameSetPlayerDao.insertAll(playerLinks)

        return gameSetId
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
        totalMaal: Int,
        playerScores: List<RoundScoreData>
    ): Int {
        val roundNumber = roundDao.getRoundCount(gameSetId) + 1
        val roundEntity = RoundEntity(
            gameSetId = gameSetId,
            roundNumber = roundNumber,
            winnerId = winnerId,
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
