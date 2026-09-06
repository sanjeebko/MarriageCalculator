package np.com.sanjeeb.marriagecalculator.ui.roundinput

import np.com.sanjeeb.marriagecalculator.data.model.GameSettings
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.data.remote.ScoringApiService
import np.com.sanjeeb.marriagecalculator.data.repository.GameSetRepository
import np.com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import io.mockk.mockk
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.test.setMain
import kotlinx.coroutines.test.resetMain
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test

@kotlinx.coroutines.ExperimentalCoroutinesApi
class RoundInputViewModelTest {

    private val testDispatcher = kotlinx.coroutines.test.UnconfinedTestDispatcher()

    private val scoringApi: ScoringApiService = mockk(relaxed = true)
    private val offlineGameRepository: OfflineGameRepository = mockk(relaxed = true)
    private val gameSetRepository: GameSetRepository = mockk(relaxed = true)
    private val sessionManager: SessionManager = mockk(relaxed = true)

    private lateinit var viewModel: RoundInputViewModel

    private val testPlayers = listOf(
        Player(id = "1", name = "Player 1"),
        Player(id = "2", name = "Player 2"),
        Player(id = "3", name = "Player 3")
    )

    @Before
    fun setUp() {
        kotlinx.coroutines.Dispatchers.setMain(testDispatcher)
        viewModel = RoundInputViewModel(
            scoringApi = scoringApi,
            offlineGameRepository = offlineGameRepository,
            gameSetRepository = gameSetRepository,
            sessionManager = sessionManager
        )
        viewModel.initPlayers(testPlayers, GameSettings.default())
    }

    @org.junit.After
    fun tearDown() {
        kotlinx.coroutines.Dispatchers.resetMain()
    }

    @Test
    fun toggleDuply_WhenSelectingDub_AutomaticallyTicksSeen() {
        // Player 1 initial state: seen = false, duply = false
        val initialPlayer = viewModel.uiState.value.playerStates.first { it.player.id == "1" }
        assertFalse(initialPlayer.seen)
        assertFalse(initialPlayer.duply)

        // Toggle Duply (Select Dub)
        viewModel.toggleDuply("1")

        val updatedPlayer = viewModel.uiState.value.playerStates.first { it.player.id == "1" }
        assertTrue(updatedPlayer.duply)
        assertTrue(updatedPlayer.seen)
    }

    @Test
    fun toggleDuply_WhenDeselectingDub_LeavesSeenAsTicked() {
        // First select Dub -> seen becomes true, duply becomes true
        viewModel.toggleDuply("1")
        val playerWithDub = viewModel.uiState.value.playerStates.first { it.player.id == "1" }
        assertTrue(playerWithDub.duply)
        assertTrue(playerWithDub.seen)

        // Now deselect Dub -> duply becomes false, seen MUST remain true
        viewModel.toggleDuply("1")

        val playerWithoutDub = viewModel.uiState.value.playerStates.first { it.player.id == "1" }
        assertFalse(playerWithoutDub.duply)
        assertTrue(playerWithoutDub.seen)
    }

    @Test
    fun submitRound_WhenOfflineLocal_PersistsRoundToLocalDatabase() {
        io.mockk.coEvery {
            offlineGameRepository.saveRound(
                gameSetId = any(),
                winnerId = any(),
                dealerId = any(),
                totalMaal = any(),
                playerScores = any(),
                synced = any(),
                remoteId = any()
            )
        } returns 10

        viewModel.loadGameData("1", 1)
        viewModel.setWinner("1")
        viewModel.submitRound()

        io.mockk.coVerify {
            offlineGameRepository.saveRound(
                gameSetId = 1,
                winnerId = 1,
                dealerId = any(),
                totalMaal = any(),
                playerScores = any(),
                synced = false,
                remoteId = null
            )
        }
        assertTrue(viewModel.uiState.value.submitted)
    }

    @Test
    fun submitRound_WhenOnlineFails_FallsBackToLocalPersistenceWithUnsyncedStatus() {
        io.mockk.every { sessionManager.isOnlineMode() } returns true
        val mockOnlineGameSet = np.com.sanjeeb.marriagecalculator.data.model.MarriageGameSet(
            id = "guid-123",
            gameSetPlayers = mapOf(
                "1" to np.com.sanjeeb.marriagecalculator.data.model.MarriageGameSetPlayer(
                    id = "1",
                    playerId = "1",
                    player = Player(id = "1", name = "Player 1")
                )
            )
        )
        io.mockk.coEvery { gameSetRepository.getGameSet("guid-123") } returns
                np.com.sanjeeb.marriagecalculator.data.repository.ApiResult.Success(mockOnlineGameSet)
        io.mockk.coEvery { gameSetRepository.submitRound(any(), any()) } returns
                np.com.sanjeeb.marriagecalculator.data.repository.ApiResult.Error("Network error: timeout")
        val mockLocalGameSet = np.com.sanjeeb.marriagecalculator.data.local.GameSetEntity(id = 5, settingsId = 1, remoteId = "guid-123")
        io.mockk.coEvery { offlineGameRepository.getGameSetByRemoteId("guid-123") } returns mockLocalGameSet
        io.mockk.coEvery { offlineGameRepository.getPlayerEntityByName(any()) } returns
                np.com.sanjeeb.marriagecalculator.data.local.PlayerEntity(id = 1, name = "Player 1")
        io.mockk.coEvery {
            offlineGameRepository.saveRound(
                gameSetId = any(),
                winnerId = any(),
                dealerId = any(),
                totalMaal = any(),
                playerScores = any(),
                synced = any(),
                remoteId = any()
            )
        } returns 20

        viewModel.loadGameData("guid-123", 1)
        viewModel.setWinner("1")
        viewModel.submitRound()

        io.mockk.coVerify {
            offlineGameRepository.saveRound(
                gameSetId = 5,
                winnerId = 1,
                dealerId = any(),
                totalMaal = any(),
                playerScores = any(),
                synced = false,
                remoteId = null
            )
        }
        assertTrue(viewModel.uiState.value.submitted)
    }
}
