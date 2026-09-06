package np.com.sanjeeb.marriagecalculator

import io.mockk.coEvery
import io.mockk.coVerify
import io.mockk.mockk
import kotlinx.coroutines.test.runTest
import np.com.sanjeeb.marriagecalculator.data.local.*
import np.com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import org.junit.Before
import org.junit.Test

class OfflineGameRepositoryTest {

    private val playerDao: PlayerDao = mockk(relaxed = true)
    private val gameSetDao: GameSetDao = mockk(relaxed = true)
    private val gameSetPlayerDao: GameSetPlayerDao = mockk(relaxed = true)
    private val gameSettingsDao: GameSettingsDao = mockk(relaxed = true)
    private val roundDao: RoundDao = mockk(relaxed = true)
    private val roundScoreDao: RoundScoreDao = mockk(relaxed = true)

    private lateinit var repository: OfflineGameRepository

    @Before
    fun setUp() {
        repository = OfflineGameRepository(
            playerDao = playerDao,
            gameSetDao = gameSetDao,
            gameSetPlayerDao = gameSetPlayerDao,
            gameSettingsDao = gameSettingsDao,
            roundDao = roundDao,
            roundScoreDao = roundScoreDao
        )
    }

    @Test
    fun `closeCurrentRound calls closeRoundAt on latest game`() = runTest {
        val gameSetId = 42
        val latestGame = RoundEntity(id = 101, gameSetId = gameSetId, roundNumber = 1, winnerId = 2)
        coEvery { roundDao.getLatestGame(gameSetId) } returns latestGame

        repository.closeCurrentRound(gameSetId)

        coVerify { roundDao.closeRoundAt(101) }
    }

    @Test
    fun `reopenCurrentRound calls reopenRoundAt on latest game`() = runTest {
        val gameSetId = 42
        val latestGame = RoundEntity(id = 101, gameSetId = gameSetId, roundNumber = 1, winnerId = 2, closesRound = true)
        coEvery { roundDao.getLatestGame(gameSetId) } returns latestGame

        repository.reopenCurrentRound(gameSetId)

        coVerify { roundDao.reopenRoundAt(101) }
    }
}
