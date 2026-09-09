package np.com.sanjeeb.marriagecalculator.data.repository

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.withContext
import np.com.sanjeeb.marriagecalculator.data.local.*
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class ActivityLogRepository @Inject constructor(
    private val activityLogDao: ActivityLogDao,
    private val gameSetDao: GameSetDao,
    private val roundDao: RoundDao,
    private val gameSetPlayerDao: GameSetPlayerDao,
    private val playerDao: PlayerDao,
    private val roundScoreDao: RoundScoreDao
) {
    val allLogsFlow: Flow<List<ActivityLogEntity>> = activityLogDao.getAllLogsFlow()

    suspend fun getPagedLogs(
        limit: Int = 30,
        offset: Int = 0,
        types: List<String>? = null
    ): List<ActivityLogEntity> = withContext(Dispatchers.IO) {
        if (types.isNullOrEmpty()) {
            activityLogDao.getPagedLogs(limit, offset)
        } else {
            activityLogDao.getPagedLogsByTypes(types, limit, offset)
        }
    }

    suspend fun getLogsInRange(startTime: Long, endTime: Long): List<ActivityLogEntity> =
        withContext(Dispatchers.IO) {
            activityLogDao.getLogsInRange(startTime, endTime)
        }

    suspend fun getLogCount(): Int = withContext(Dispatchers.IO) {
        activityLogDao.getCount()
    }

    suspend fun logAppOpen(userIdentifier: String) = withContext(Dispatchers.IO) {
        activityLogDao.insert(
            ActivityLogEntity(
                eventType = ActivityType.APP_OPEN.name,
                title = "App Opened",
                description = "App session started for $userIdentifier",
                metadata = userIdentifier
            )
        )
    }

    suspend fun logLogin(userEmail: String, isGuest: Boolean = false) = withContext(Dispatchers.IO) {
        activityLogDao.insert(
            ActivityLogEntity(
                eventType = ActivityType.USER_LOGIN.name,
                title = if (isGuest) "Guest Login" else "User Login",
                description = if (isGuest) "Logged in as Guest" else "Logged in as $userEmail",
                metadata = if (isGuest) "guest" else userEmail
            )
        )
    }

    suspend fun logGameCreated(
        gameSetId: String,
        name: String,
        playerNames: List<String>,
        timestamp: Long = System.currentTimeMillis()
    ) = withContext(Dispatchers.IO) {
        val count = playerNames.size
        val playersStr = if (count > 0) playerNames.joinToString(", ") else "Players"
        activityLogDao.insert(
            ActivityLogEntity(
                eventType = ActivityType.GAME_CREATED.name,
                timestamp = timestamp,
                gameSetId = gameSetId,
                title = "Game Created",
                description = "$name ($count players: $playersStr)",
                metadata = playerNames.joinToString(";")
            )
        )
    }

    suspend fun logGamePlayed(
        gameSetId: String,
        roundSequence: Int,
        gameSequence: Int,
        winnerName: String,
        points: Int,
        timestamp: Long = System.currentTimeMillis()
    ) = withContext(Dispatchers.IO) {
        activityLogDao.insert(
            ActivityLogEntity(
                eventType = ActivityType.GAME_PLAYED.name,
                timestamp = timestamp,
                gameSetId = gameSetId,
                title = "Round $roundSequence · Game $gameSequence",
                description = "Winner: $winnerName ($points pts)",
                metadata = "$winnerName:$points"
            )
        )
    }

    suspend fun logPaymentCleared(
        gameSetId: String,
        roundSequence: Int,
        summary: String? = null,
        timestamp: Long = System.currentTimeMillis()
    ) = withContext(Dispatchers.IO) {
        val extra = if (!summary.isNullOrBlank()) " ($summary)" else ""
        activityLogDao.insert(
            ActivityLogEntity(
                eventType = ActivityType.PAYMENT_CLEARED.name,
                timestamp = timestamp,
                gameSetId = gameSetId,
                title = "Payment Cleared",
                description = "Round $roundSequence payment cleared & settled$extra",
                metadata = summary
            )
        )
    }

    suspend fun logGameSettled(
        gameSetId: String,
        name: String,
        timestamp: Long = System.currentTimeMillis()
    ) = withContext(Dispatchers.IO) {
        activityLogDao.insert(
            ActivityLogEntity(
                eventType = ActivityType.GAME_SETTLED.name,
                timestamp = timestamp,
                gameSetId = gameSetId,
                title = "Game Settled",
                description = "$name session settled and frozen",
                metadata = name
            )
        )
    }

    /**
     * One-time backfill: imports existing GameSets, Rounds, and payments from Room
     * into activity_logs if they haven't been logged yet.
     */
    suspend fun backfillFromHistory() = withContext(Dispatchers.IO) {
        val gameSets = gameSetDao.getAllGameSetsList()
        for (gameSet in gameSets) {
            val gameSetIdStr = gameSet.id.toString()
            val existingCount = activityLogDao.getCountForGameSet(gameSetIdStr)
            if (existingCount > 0) continue // Already backfilled or logged

            val players = gameSetPlayerDao.getPlayersForGameSet(gameSet.id)
            val playerNames = players.map { it.name }
            val displayName = gameSet.name.ifBlank { "Game #${gameSet.id}" }

            // 1. Log Game Created
            activityLogDao.insert(
                ActivityLogEntity(
                    eventType = ActivityType.GAME_CREATED.name,
                    timestamp = gameSet.createdAt,
                    gameSetId = gameSetIdStr,
                    title = "Game Created",
                    description = "$displayName (${playerNames.size} players: ${playerNames.joinToString(", ")})",
                    metadata = playerNames.joinToString(";")
                )
            )

            // 2. Log Games & Rounds Played
            val games = roundDao.getRoundsForGameSetList(gameSet.id).sortedBy { it.roundNumber }
            val playerCount = if (players.isNotEmpty()) players.size else 4
            var logicalRound = 1
            var gameInRound = 0

            for (game in games) {
                gameInRound++
                val winner = playerDao.getById(game.winnerId)
                val winnerName = winner?.name ?: "Player #${game.winnerId}"
                val winnerScore = roundScoreDao.getScoresForRound(game.id)
                    .firstOrNull { it.playerId == game.winnerId }?.score ?: 0

                activityLogDao.insert(
                    ActivityLogEntity(
                        eventType = ActivityType.GAME_PLAYED.name,
                        timestamp = game.createdAt,
                        gameSetId = gameSetIdStr,
                        roundId = game.id.toString(),
                        title = "Round $logicalRound · Game $gameInRound",
                        description = "Winner: $winnerName ($winnerScore pts)",
                        metadata = "$winnerName:$winnerScore"
                    )
                )

                // If payment cleared for this round
                if (game.isPaymentCleared) {
                    activityLogDao.insert(
                        ActivityLogEntity(
                            eventType = ActivityType.PAYMENT_CLEARED.name,
                            timestamp = game.createdAt + 1000L,
                            gameSetId = gameSetIdStr,
                            roundId = game.id.toString(),
                            title = "Payment Cleared",
                            description = "Round $logicalRound payment cleared & settled",
                            metadata = "round:$logicalRound"
                        )
                    )
                }

                if (gameInRound >= playerCount || game.closesRound) {
                    logicalRound++
                    gameInRound = 0
                }
            }

            // 3. Log Settlement
            if (gameSet.isSettled) {
                activityLogDao.insert(
                    ActivityLogEntity(
                        eventType = ActivityType.GAME_SETTLED.name,
                        timestamp = if (games.isNotEmpty()) games.last().createdAt + 2000L else gameSet.createdAt + 1000L,
                        gameSetId = gameSetIdStr,
                        title = "Game Settled",
                        description = "$displayName session settled and frozen",
                        metadata = displayName
                    )
                )
            }
        }
    }
}
