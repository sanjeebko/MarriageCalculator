package np.com.sanjeeb.marriagecalculator.data.repository

import np.com.sanjeeb.marriagecalculator.data.local.*
import np.com.sanjeeb.marriagecalculator.data.model.*
import np.com.sanjeeb.marriagecalculator.data.remote.MarriageGameSetApiService
import np.com.sanjeeb.marriagecalculator.data.remote.PlayerApiService
import np.com.sanjeeb.marriagecalculator.ui.dashboard.EnrichedActiveGame
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
        val oldOrder = gameSetPlayerDao.getPlayersForGameSet(gameSetId)
        if (oldOrder.isNotEmpty() && oldOrder.map { it.id } != playerIds) {
            // A round's seat order is fixed once it starts - reshuffling only happens between
            // rounds, so reject order changes while a round is still open.
            if (getOpenRoundState(gameSetId, oldOrder.size).gamesInOpenRound > 0) {
                throw IllegalStateException("Seats can only be rearranged after the current round is completed.")
            }

            // Freeze history: games saved before per-game seat snapshots existed have a blank
            // seatOrder and would otherwise re-render in the new order. Stamp them with the
            // outgoing order - the seating they were actually played with.
            roundDao.backfillSeatOrder(gameSetId, oldOrder.joinToString(",") { it.id.toString() })
        }

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
        val seatOrder = gameSetPlayerDao.getPlayersForGameSet(gameSetId)
            .joinToString(",") { it.id.toString() }
        val roundEntity = RoundEntity(
            gameSetId = gameSetId,
            roundNumber = roundNumber,
            winnerId = winnerId,
            dealerId = dealerId,
            totalMaal = totalMaal,
            seatOrder = seatOrder
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

    /**
     * Re-scores an already-played game with corrected inputs. Dealer, seat order, and position
     * in the round stay fixed; winner, maal, and the per-player score rows are replaced.
     */
    suspend fun updateGame(
        gameId: Int,
        winnerId: Int,
        totalMaal: Int,
        playerScores: List<RoundScoreData>
    ) {
        val game = roundDao.getGameById(gameId) ?: return
        roundDao.insert(game.copy(winnerId = winnerId, totalMaal = totalMaal))
        roundScoreDao.deleteForRound(gameId)
        roundScoreDao.insertAll(playerScores.map {
            RoundScoreEntity(
                roundId = gameId,
                playerId = it.playerId,
                score = it.score,
                maal = it.maal,
                isSeen = it.isSeen,
                isWinner = it.isWinner,
                isDublee = it.isDublee
            )
        })
    }

    /** Returns one game row plus its per-player scores, for prefilling the edit screen. */
    suspend fun getGameWithScores(gameId: Int): Pair<RoundEntity, List<RoundScoreEntity>>? {
        val game = roundDao.getGameById(gameId) ?: return null
        return game to roundScoreDao.getScoresForRound(gameId)
    }

    /** Marks the most recently played game as closing out its logical round early. */
    suspend fun closeCurrentRound(gameSetId: Int) {
        val latest = roundDao.getLatestGame(gameSetId) ?: return
        roundDao.closeRoundAt(latest.id)
    }

    /** Reopens the most recently closed logical round if the new round has not yet started. */
    suspend fun reopenCurrentRound(gameSetId: Int) {
        val latest = roundDao.getLatestGame(gameSetId) ?: return
        roundDao.reopenRoundAt(latest.id)
    }

    /** Updates paymentCleared flag on all games belonging to a logical round. */
    suspend fun toggleRoundPaymentCleared(gameIds: List<Int>, isCleared: Boolean) {
        if (gameIds.isEmpty()) return
        roundDao.setPaymentClearedForGames(gameIds, isCleared)
    }

    /**
     * One-time backfill so round history is stored data, never derived: games saved before seat
     * snapshots existed get the current seating persisted as theirs, and games saved before
     * dealer tracking (dealerId 0) get their dealer computed once from the round's rotation
     * (last seat deals a round's first game, then the deal wraps to the top) and persisted.
     */
    suspend fun backfillRoundHistory(gameSetId: Int) {
        val players = gameSetPlayerDao.getPlayersForGameSet(gameSetId)
        if (players.isEmpty()) return
        roundDao.backfillSeatOrder(gameSetId, players.joinToString(",") { it.id.toString() })

        val games = roundDao.getRoundsForGameSet(gameSetId).first().sortedBy { it.roundNumber }
        var bucketSize = 0
        var bucketSeats: List<Int> = players.map { it.id }
        for (g in games) {
            if (bucketSize == 0) {
                bucketSeats = g.seatOrder.split(",").mapNotNull { it.toIntOrNull() }
                    .takeIf { it.size == players.size } ?: players.map { it.id }
            }
            if (g.dealerId == 0) {
                roundDao.setDealer(g.id, bucketSeats[(bucketSeats.size - 1 + bucketSize) % bucketSeats.size])
            }
            bucketSize++
            if (bucketSize >= players.size || g.closesRound) bucketSize = 0
        }
    }

    /**
     * State of the current open logical round: how many games it has so far (0 if the last round
     * closed and a new one hasn't started), and the seat order its first game was played with
     * (null if the round is empty or predates seat-order snapshots).
     * Mirrors the bucket-chunking rule used for display: a round closes after playerCount games
     * or when a game carries closesRound.
     */
    suspend fun getOpenRoundState(gameSetId: Int, playerCount: Int): OpenRoundState {
        if (playerCount <= 0) return OpenRoundState(0, null)
        val games = roundDao.getRoundsForGameSet(gameSetId).first().sortedBy { it.roundNumber }
        var bucket = mutableListOf<RoundEntity>()
        for (g in games) {
            bucket.add(g)
            if (bucket.size >= playerCount || g.closesRound) bucket = mutableListOf()
        }
        val seatCsv = bucket.firstOrNull()?.seatOrder?.takeIf { it.isNotBlank() }
        return OpenRoundState(
            gamesInOpenRound = bucket.size,
            seatOrderIds = seatCsv?.split(",")
        )
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

    // ── Dashboard Career Stats & Enriched Games ──

    suspend fun getUserCareerStats(user: User?): UserCareerStats {
        val allPlayers = try {
            playerDao.getAllPlayers().first()
        } catch (e: Exception) {
            emptyList()
        }
        if (allPlayers.isEmpty()) return UserCareerStats()

        val matchedPlayer = when {
            user != null && user.email.isNotBlank() ->
                allPlayers.find { it.email.equals(user.email, ignoreCase = true) }
            user != null && user.displayName.isNotBlank() ->
                allPlayers.find { it.name.equals(user.displayName, ignoreCase = true) }
            else ->
                allPlayers.firstOrNull()
        } ?: return UserCareerStats()

        val playerScores = try {
            roundScoreDao.getScoresForPlayer(matchedPlayer.id)
        } catch (e: Exception) {
            emptyList()
        }
        if (playerScores.isEmpty()) return UserCareerStats()

        val totalGames = playerScores.size
        val wins = playerScores.count { it.isWinner }
        val winRate = if (totalGames > 0) (wins * 100) / totalGames else 0
        val highestMaal = playerScores.maxOfOrNull { it.maal } ?: 0

        val allRounds = try {
            roundDao.getAllRounds()
        } catch (e: Exception) {
            emptyList()
        }
        val roundMap = allRounds.associateBy { it.id }

        val allGameSets = try {
            gameSetDao.getAllGameSets().first()
        } catch (e: Exception) {
            emptyList()
        }
        val gameSetMap = allGameSets.associateBy { it.id }

        val allSettings = try {
            gameSettingsDao.getAllSettings()
        } catch (e: Exception) {
            emptyList()
        }
        val settingsMap = allSettings.associateBy { it.id }

        var netMoney = 0.0
        var preferredCurrency = Currency.NPR_Rupee

        for (score in playerScores) {
            val round = roundMap[score.roundId]
            val gameSet = round?.let { gameSetMap[it.gameSetId] }
            val settings = gameSet?.let { settingsMap[it.settingsId] }
            val rate = settings?.pointRate ?: 10.0
            if (settings != null) {
                preferredCurrency = Currency.entries.getOrElse(settings.currency) { Currency.NPR_Rupee }
            }
            netMoney += score.score * rate
        }

        val formattedMoney = preferredCurrency.formatMoney(netMoney)
        val formattedWithSign = if (netMoney > 0) "+$formattedMoney" else formattedMoney

        return UserCareerStats(
            totalGames = totalGames,
            winRatePercent = winRate,
            netProfitLoss = netMoney,
            netProfitLossFormatted = formattedWithSign,
            isPositive = netMoney > 0,
            isZero = kotlin.math.abs(netMoney) < 0.001,
            highestMaal = highestMaal
        )
    }

    suspend fun getRecentPlayers(limit: Int = 4): List<Player> {
        val latestGameSet = try {
            gameSetDao.getLatestGameSet()
        } catch (e: Exception) {
            null
        }
        if (latestGameSet != null) {
            val players = getGameSetPlayers(latestGameSet.id)
            if (players.size >= 2) {
                return players.take(limit)
            }
        }
        return try {
            playerDao.getAllPlayers().first().take(limit).map { it.toDomainModel() }
        } catch (e: Exception) {
            emptyList()
        }
    }

    suspend fun quickCreateGame(name: String, playerIds: List<Int>): Int {
        val defaultSettings = GameSettings()
        return createGameSet(
            name = name.ifBlank { "Quick Game" },
            settings = defaultSettings,
            playerIds = playerIds
        )
    }

    suspend fun getEnrichedActiveGameSets(): List<EnrichedActiveGame> {
        val dateFormat = java.text.SimpleDateFormat("yyyy-MM-dd", java.util.Locale.getDefault())
        val entities = try {
            gameSetDao.getActiveGameSets().first()
        } catch (e: Exception) {
            emptyList()
        }

        return entities.map { entity ->
            val settings = getGameSettings(entity.settingsId) ?: GameSettings()
            val players = getGameSetPlayers(entity.id)
            val rounds = try {
                roundDao.getRoundsForGameSet(entity.id).first()
            } catch (e: Exception) {
                emptyList()
            }
            val allScores = try {
                roundScoreDao.getAllScoresForGameSet(entity.id)
            } catch (e: Exception) {
                emptyList()
            }

            val standings = players.map { p ->
                val pScores = allScores.filter { it.playerId == p.id.toIntOrNull() }
                val netPoints = pScores.sumOf { it.score }
                val money = netPoints * settings.pointRate
                p to money
            }
            val highest = standings.maxByOrNull { it.second }
            val leaderBadge = if (highest != null && highest.second > 0.0) {
                "👑 ${highest.first.name} (+${settings.currency.formatMoney(highest.second)})"
            } else null

            val playerCount = if (players.isNotEmpty()) players.size else 4
            val openRound = getOpenRoundState(entity.id, playerCount)
            val completedRoundsCount = if (playerCount > 0) (rounds.size - openRound.gamesInOpenRound) / playerCount else 0
            val roundStatus = when {
                rounds.isEmpty() -> "Not started"
                openRound.gamesInOpenRound == 0 && completedRoundsCount > 0 -> "Round $completedRoundsCount completed"
                else -> "Round ${completedRoundsCount + 1} · Game ${openRound.gamesInOpenRound + 1}"
            }

            val suits = listOf("♠", "♥", "♦", "♣")
            val cardSuit = suits[kotlin.math.abs(entity.id) % suits.size]

            EnrichedActiveGame(
                id = entity.id.toString(),
                name = entity.name.ifEmpty { "Game #${entity.id}" },
                lastPlayed = dateFormat.format(java.util.Date(entity.createdAt)),
                players = players,
                leaderName = highest?.first?.name?.takeIf { (highest.second) > 0.0 },
                leaderScoreText = highest?.second?.let { if (it > 0.0) "+${settings.currency.formatMoney(it)}" else null },
                roundStatusText = roundStatus,
                totalGamesPlayed = rounds.size,
                isSettled = entity.isSettled,
                cardSuit = cardSuit
            )
        }
    }
}

data class RoundScoreData(
    val playerId: Int,
    val score: Int,
    val maal: Int,
    val isSeen: Boolean,
    val isWinner: Boolean,
    val isDublee: Boolean
)

data class OpenRoundState(
    val gamesInOpenRound: Int,
    val seatOrderIds: List<String>?
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
