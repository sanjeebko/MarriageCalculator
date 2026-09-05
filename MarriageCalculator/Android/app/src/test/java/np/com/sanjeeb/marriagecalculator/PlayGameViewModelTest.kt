package np.com.sanjeeb.marriagecalculator

import np.com.sanjeeb.marriagecalculator.data.model.*
import np.com.sanjeeb.marriagecalculator.data.repository.*
import np.com.sanjeeb.marriagecalculator.ui.playgame.PlayGameViewModel
import io.mockk.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.*
import org.junit.After
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class PlayGameViewModelTest {

    private val testDispatcher = UnconfinedTestDispatcher()

    private val offlineGameRepository: OfflineGameRepository = mockk(relaxed = true)
    private val gameSetRepository: GameSetRepository = mockk(relaxed = true)
    private val playerRepository: PlayerRepository = mockk(relaxed = true)
    private val friendRepository: FriendRepository = mockk(relaxed = true)
    private val sessionManager: SessionManager = mockk(relaxed = true)
    private val themePreference: ThemePreference = mockk(relaxed = true)

    private lateinit var viewModel: PlayGameViewModel

    @Before
    fun setup() {
        Dispatchers.setMain(testDispatcher)
        viewModel = PlayGameViewModel(
            offlineGameRepository,
            gameSetRepository,
            playerRepository,
            friendRepository,
            sessionManager,
            themePreference
        )
    }

    @After
    fun tearDown() {
        Dispatchers.resetMain()
    }

    @Test
    fun `loadGame online sets host status to true when current user is host`() = runTest {
        val gameSetId = "online-set-id"
        val loggedInUser = User(id = "user-host-id", userId = "user-host-id", displayName = "Host User")
        
        val gameSet = MarriageGameSet(
            id = gameSetId,
            hostUserId = "user-host-id",
            name = "Test Online Game"
        )

        every { sessionManager.isOnlineMode() } returns true
        every { sessionManager.getUserProfile() } returns loggedInUser
        coEvery { gameSetRepository.getGameSet(gameSetId) } returns ApiResult.Success(gameSet)
        coEvery { friendRepository.getFriends() } returns ApiResult.Success(emptyList())

        viewModel.loadGame(gameSetId)

        val state = viewModel.uiState.value
        assertEquals("Test Online Game", state.gameName)
        assertTrue(state.isOnlineMode)
        assertTrue(state.isHost)
    }

    @Test
    fun `loadGame online sets host status to false when current user is not host`() = runTest {
        val gameSetId = "online-set-id"
        val loggedInUser = User(id = "user-guest-id", userId = "user-guest-id", displayName = "Guest User")
        
        val gameSet = MarriageGameSet(
            id = gameSetId,
            hostUserId = "user-host-id",
            name = "Test Online Game"
        )

        every { sessionManager.isOnlineMode() } returns true
        every { sessionManager.getUserProfile() } returns loggedInUser
        coEvery { gameSetRepository.getGameSet(gameSetId) } returns ApiResult.Success(gameSet)
        coEvery { friendRepository.getFriends() } returns ApiResult.Success(emptyList())

        viewModel.loadGame(gameSetId)

        val state = viewModel.uiState.value
        assertEquals("Test Online Game", state.gameName)
        assertTrue(state.isOnlineMode)
        assertFalse(state.isHost)
    }

    @Test
    fun `loadGame offline sets host status to true`() = runTest {
        val gameSetId = "123"

        every { sessionManager.isOnlineMode() } returns false
        coEvery { offlineGameRepository.getGameSet(123) } returns mockk(relaxed = true)
        coEvery { offlineGameRepository.getGameSetPlayers(123) } returns emptyList()

        viewModel.loadGame(gameSetId)

        val state = viewModel.uiState.value
        assertFalse(state.isOnlineMode)
        assertTrue(state.isHost) // offline modes default to host true
    }

    @Test
    fun `reorderPlayers updates player seating positions and reloads`() = runTest {
        val gameSetId = "123"
        val newOrder = listOf("3", "1", "2")

        every { sessionManager.isOnlineMode() } returns false
        coEvery { offlineGameRepository.getGameSet(123) } returns mockk(relaxed = true)
        coEvery { offlineGameRepository.getGameSetPlayers(123) } returns emptyList()

        viewModel.reorderPlayers(newOrder, gameSetId)

        coVerify { 
            offlineGameRepository.updateGameSetPlayerPositions(123, listOf(3, 1, 2))
        }
    }

    @Test
    fun `toggleRoundPaymentCleared calls repository method and reloads game`() = runTest {
        val gameSetId = "123"
        val roundGroup = np.com.sanjeeb.marriagecalculator.ui.playgame.RoundGroup(
            roundId = "local-1",
            roundSequence = 1,
            isCompleted = true,
            games = listOf(
                np.com.sanjeeb.marriagecalculator.ui.playgame.GameEntry(
                    gameId = "10",
                    gameSequenceInRound = 1,
                    dealerId = "1",
                    winnerId = "2",
                    winnerName = "Winner",
                    totalMaal = 10
                )
            )
        )

        every { sessionManager.isOnlineMode() } returns false
        coEvery { offlineGameRepository.getGameSet(123) } returns mockk(relaxed = true)

        viewModel.toggleRoundPaymentCleared(gameSetId, roundGroup, true)

        coVerify {
            offlineGameRepository.toggleRoundPaymentCleared(listOf(10), true)
        }
    }

    @Test
    fun `toggleRoundPaymentCleared toggles from cleared back to uncleared`() = runTest {
        val gameSetId = "123"
        val roundGroup = np.com.sanjeeb.marriagecalculator.ui.playgame.RoundGroup(
            roundId = "local-1",
            roundSequence = 1,
            isCompleted = true,
            isPaymentCleared = true,
            games = listOf(
                np.com.sanjeeb.marriagecalculator.ui.playgame.GameEntry(
                    gameId = "10",
                    gameSequenceInRound = 1,
                    dealerId = "1",
                    winnerId = "2",
                    winnerName = "Winner",
                    totalMaal = 10
                )
            )
        )

        every { sessionManager.isOnlineMode() } returns false
        coEvery { offlineGameRepository.getGameSet(123) } returns mockk(relaxed = true)

        viewModel.toggleRoundPaymentCleared(gameSetId, roundGroup, false)

        coVerify {
            offlineGameRepository.toggleRoundPaymentCleared(listOf(10), false)
        }
    }

    @Test
    fun `toggleRoundPaymentCleared online mode calls gameSetRepository togglePaymentCleared`() = runTest {
        val gameSetId = "online-set-123"
        val roundGroup = np.com.sanjeeb.marriagecalculator.ui.playgame.RoundGroup(
            roundId = "round-1",
            roundSequence = 1,
            isCompleted = true,
            games = listOf(
                np.com.sanjeeb.marriagecalculator.ui.playgame.GameEntry(
                    gameId = "10",
                    gameSequenceInRound = 1,
                    dealerId = "1",
                    winnerId = "2",
                    winnerName = "Winner",
                    totalMaal = 10
                )
            )
        )

        val loggedInUser = User(id = "user-1", userId = "user-1", displayName = "User 1")
        val gameSet = MarriageGameSet(
            id = gameSetId,
            hostUserId = "user-1",
            name = "Test Online Game"
        )

        every { sessionManager.isOnlineMode() } returns true
        every { sessionManager.getUserProfile() } returns loggedInUser
        coEvery { gameSetRepository.getGameSet(gameSetId) } returns ApiResult.Success(gameSet)
        coEvery { friendRepository.getFriends() } returns ApiResult.Success(emptyList())
        coEvery { gameSetRepository.togglePaymentCleared(gameSetId, "round-1", true) } returns ApiResult.Success(mockk())

        viewModel.toggleRoundPaymentCleared(gameSetId, roundGroup, true)

        coVerify {
            gameSetRepository.togglePaymentCleared(gameSetId, "round-1", true)
        }
    }

    @Test
    fun `closeCurrentRound offline calls offlineGameRepository closeCurrentRound`() = runTest {
        val gameSetId = "123"
        every { sessionManager.isOnlineMode() } returns false
        coEvery { offlineGameRepository.getGameSet(123) } returns mockk(relaxed = true)

        viewModel.closeCurrentRound(gameSetId)

        coVerify {
            offlineGameRepository.closeCurrentRound(123)
        }
    }

    @Test
    fun `reopenRound offline calls offlineGameRepository reopenCurrentRound`() = runTest {
        val gameSetId = "123"
        every { sessionManager.isOnlineMode() } returns false
        coEvery { offlineGameRepository.getGameSet(123) } returns mockk(relaxed = true)

        viewModel.reopenRound(gameSetId)

        coVerify {
            offlineGameRepository.reopenCurrentRound(123)
        }
    }

    @Test
    fun `reopenRound online calls gameSetRepository reopenRound`() = runTest {
        val gameSetId = "online-set-123"
        val loggedInUser = User(id = "user-1", userId = "user-1", displayName = "User 1")
        val gameSet = MarriageGameSet(
            id = gameSetId,
            hostUserId = "user-1",
            name = "Test Online Game"
        )

        every { sessionManager.isOnlineMode() } returns true
        every { sessionManager.getUserProfile() } returns loggedInUser
        coEvery { gameSetRepository.getGameSet(gameSetId) } returns ApiResult.Success(gameSet)
        coEvery { friendRepository.getFriends() } returns ApiResult.Success(emptyList())
        coEvery { gameSetRepository.reopenRound(gameSetId, "round-1") } returns ApiResult.Success(mockk())

        viewModel.reopenRound(gameSetId, "round-1")

        coVerify {
            gameSetRepository.reopenRound(gameSetId, "round-1")
        }
    }
}
