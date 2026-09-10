package np.com.sanjeeb.marriagecalculator.data.repository

import io.mockk.*
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.runTest
import np.com.sanjeeb.marriagecalculator.data.local.*
import org.junit.Assert.assertEquals
import org.junit.Before
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class ActivityLogRepositoryTest {

    private val activityLogDao: ActivityLogDao = mockk(relaxed = true)
    private val gameSetDao: GameSetDao = mockk(relaxed = true)
    private val roundDao: RoundDao = mockk(relaxed = true)
    private val gameSetPlayerDao: GameSetPlayerDao = mockk(relaxed = true)
    private val playerDao: PlayerDao = mockk(relaxed = true)
    private val roundScoreDao: RoundScoreDao = mockk(relaxed = true)

    private lateinit var repository: ActivityLogRepository

    @Before
    fun setUp() {
        repository = ActivityLogRepository(
            activityLogDao = activityLogDao,
            gameSetDao = gameSetDao,
            roundDao = roundDao,
            gameSetPlayerDao = gameSetPlayerDao,
            playerDao = playerDao,
            roundScoreDao = roundScoreDao
        )
    }

    @Test
    fun `logAppOpen records APP_OPEN event`() = runTest {
        val slot = slot<ActivityLogEntity>()
        coEvery { activityLogDao.insert(capture(slot)) } returns 1L

        repository.logAppOpen("Sanjeeb")

        coVerify { activityLogDao.insert(any()) }
        assertEquals(ActivityType.APP_OPEN.name, slot.captured.eventType)
        assertEquals("App Opened", slot.captured.title)
        assertEquals("Sanjeeb", slot.captured.metadata)
    }

    @Test
    fun `logLogin records USER_LOGIN for email and guest`() = runTest {
        val slot = slot<ActivityLogEntity>()
        coEvery { activityLogDao.insert(capture(slot)) } returns 1L

        repository.logLogin("test@user.com", isGuest = false)
        assertEquals(ActivityType.USER_LOGIN.name, slot.captured.eventType)
        assertEquals("User Login", slot.captured.title)
        assertEquals("test@user.com", slot.captured.metadata)

        repository.logLogin("Guest", isGuest = true)
        assertEquals(ActivityType.USER_LOGIN.name, slot.captured.eventType)
        assertEquals("Guest Login", slot.captured.title)
        assertEquals("guest", slot.captured.metadata)
    }

    @Test
    fun `logGameCreated stores participants and name`() = runTest {
        val slot = slot<ActivityLogEntity>()
        coEvery { activityLogDao.insert(capture(slot)) } returns 1L

        repository.logGameCreated("123", "Saturday Night", listOf("Sanjeeb", "Aariya", "Apple"))

        assertEquals(ActivityType.GAME_CREATED.name, slot.captured.eventType)
        assertEquals("Game Created", slot.captured.title)
        assertEquals("123", slot.captured.gameSetId)
        assertEquals("Sanjeeb;Aariya;Apple", slot.captured.metadata)
    }

    @Test
    fun `logGamePlayed records round and game sequence with winner`() = runTest {
        val slot = slot<ActivityLogEntity>()
        coEvery { activityLogDao.insert(capture(slot)) } returns 1L

        repository.logGamePlayed("123", roundSequence = 2, gameSequence = 3, winnerName = "Aariya", points = 35)

        assertEquals(ActivityType.GAME_PLAYED.name, slot.captured.eventType)
        assertEquals("Round 2 · Game 3", slot.captured.title)
        assertEquals("Winner: Aariya (35 pts)", slot.captured.description)
        assertEquals("Aariya:35", slot.captured.metadata)
    }

    @Test
    fun `logPaymentCleared records round cleared`() = runTest {
        val slot = slot<ActivityLogEntity>()
        coEvery { activityLogDao.insert(capture(slot)) } returns 1L

        repository.logPaymentCleared("123", roundSequence = 1, summary = "£4.50")

        assertEquals(ActivityType.PAYMENT_CLEARED.name, slot.captured.eventType)
        assertEquals("Payment Cleared", slot.captured.title)
        assertEquals("Round 1 payment cleared & settled (£4.50)", slot.captured.description)
    }

    @Test
    fun `logGameSettled records game settled`() = runTest {
        val slot = slot<ActivityLogEntity>()
        coEvery { activityLogDao.insert(capture(slot)) } returns 1L

        repository.logGameSettled("123", name = "Test Game")

        assertEquals(ActivityType.GAME_SETTLED.name, slot.captured.eventType)
        assertEquals("Game Settled", slot.captured.title)
    }

    @Test
    fun `backfillFromHistory synthesizes logs for unlogged game sets`() = runTest {
        val gameSet = GameSetEntity(
            id = 10,
            settingsId = 1,
            name = "Festive Game",
            isSettled = true,
            createdAt = 1000L
        )
        val player1 = PlayerEntity(id = 1, name = "Player 1")
        val player2 = PlayerEntity(id = 2, name = "Player 2")
        val round1 = RoundEntity(
            id = 100,
            gameSetId = 10,
            roundNumber = 1,
            winnerId = 1,
            isPaymentCleared = true,
            createdAt = 2000L
        )

        coEvery { gameSetDao.getAllGameSetsList() } returns listOf(gameSet)
        coEvery { activityLogDao.getCountForGameSet("10") } returns 0
        coEvery { gameSetPlayerDao.getPlayersForGameSet(10) } returns listOf(player1, player2)
        coEvery { roundDao.getRoundsForGameSetList(10) } returns listOf(round1)
        coEvery { playerDao.getById(1) } returns player1
        coEvery { roundScoreDao.getScoresForRound(100) } returns listOf(
            RoundScoreEntity(id = 1, roundId = 100, playerId = 1, score = 25, isWinner = true)
        )

        val inserted = mutableListOf<ActivityLogEntity>()
        coEvery { activityLogDao.insert(capture(inserted)) } returns 1L

        repository.backfillFromHistory()

        // Expect: 1 GAME_CREATED, 1 GAME_PLAYED, 1 PAYMENT_CLEARED, 1 GAME_SETTLED = 4
        assertEquals(4, inserted.size)
        assertEquals(ActivityType.GAME_CREATED.name, inserted[0].eventType)
        assertEquals(ActivityType.GAME_PLAYED.name, inserted[1].eventType)
        assertEquals(ActivityType.PAYMENT_CLEARED.name, inserted[2].eventType)
        assertEquals(ActivityType.GAME_SETTLED.name, inserted[3].eventType)
    }
}
