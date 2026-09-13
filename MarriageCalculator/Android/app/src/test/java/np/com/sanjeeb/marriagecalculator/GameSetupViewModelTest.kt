package np.com.sanjeeb.marriagecalculator

import np.com.sanjeeb.marriagecalculator.data.model.*
import np.com.sanjeeb.marriagecalculator.data.repository.*
import np.com.sanjeeb.marriagecalculator.ui.gamesetup.GameSetupViewModel
import io.mockk.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.flow.flowOf
import kotlinx.coroutines.test.*
import org.junit.After
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class GameSetupViewModelTest {

    private val testDispatcher = UnconfinedTestDispatcher()

    private val playerRepository: PlayerRepository = mockk(relaxed = true)
    private val friendRepository: FriendRepository = mockk(relaxed = true)
    private val gameSettingsRepository: GameSettingsRepository = mockk(relaxed = true)
    private val gameSetRepository: GameSetRepository = mockk(relaxed = true)
    private val offlineGameRepository: OfflineGameRepository = mockk(relaxed = true)
    private val sessionManager: SessionManager = mockk(relaxed = true)

    @Before
    fun setup() {
        Dispatchers.setMain(testDispatcher)
        coEvery { friendRepository.getFriends() } returns ApiResult.Success(emptyList())
    }

    @After
    fun tearDown() {
        Dispatchers.resetMain()
    }

    private fun createViewModel(): GameSetupViewModel {
        return GameSetupViewModel(
            playerRepository,
            friendRepository,
            gameSettingsRepository,
            gameSetRepository,
            offlineGameRepository,
            sessionManager,
            mockk(relaxed = true)
        )
    }

    @Test
    fun `getAllPlayers deduplicates players by email prioritizing remote player`() = runTest {
        val localUser = Player(id = "1", name = "Aariya Local", email = "aariyaojha@gmail.com")
        val remoteUser = Player(id = "remote_id_123", name = "Aariya Remote", email = "aariyaojha@gmail.com")
        
        every { sessionManager.getUserProfile() } returns User(id = "user_id", userId = "user_id", email = "aariyaojha@gmail.com", displayName = "Aariya Remote")
        every { sessionManager.isOnlineMode() } returns true
        coEvery { offlineGameRepository.getAllPlayers() } returns flowOf(listOf(localUser))
        coEvery { playerRepository.getPlayers() } returns ApiResult.Success(listOf(remoteUser))

        val viewModel = createViewModel()
        val all = viewModel.getAllPlayers()
        
        // Should only return one Aariya Ojha, and it should be the remote one (prioritizing remote player)
        assertEquals(1, all.size)
        assertEquals("remote_id_123", all[0].id)
        assertEquals("Aariya Remote", all[0].name)
    }

    @Test
    fun `togglePlayerSelection removes duplicate identity (same email) already selected`() = runTest {
        val localUser = Player(id = "1", name = "Aariya Local", email = "aariyaojha@gmail.com")
        val remoteUser = Player(id = "remote_id_123", name = "Aariya Remote", email = "aariyaojha@gmail.com")
        val otherPlayer = Player(id = "2", name = "Guest Player", email = "")

        every { sessionManager.getUserProfile() } returns User(id = "user_id", userId = "user_id", email = "aariyaojha@gmail.com", displayName = "Aariya Remote")
        every { sessionManager.isOnlineMode() } returns true
        coEvery { offlineGameRepository.getAllPlayers() } returns flowOf(listOf(localUser, otherPlayer))
        coEvery { playerRepository.getPlayers() } returns ApiResult.Success(listOf(remoteUser))

        val viewModel = createViewModel()
        
        // Select the local one initially
        viewModel.togglePlayerSelection("1")
        
        // Toggle the remote one (same email)
        viewModel.togglePlayerSelection("remote_id_123")

        // Local one (1) should be removed because they share the same email, and remote one should be added
        assertFalse(viewModel.uiState.value.selectedPlayerIds.contains("1"))
        assertTrue(viewModel.uiState.value.selectedPlayerIds.contains("remote_id_123"))
    }

    @Test
    fun `createGame local saves players in exact specified order`() = runTest {
        val playerA = Player(id = "10", name = "Alice")
        val playerB = Player(id = "20", name = "Bob")
        val playerC = Player(id = "30", name = "Charlie")

        every { sessionManager.isOnlineMode() } returns false
        coEvery { offlineGameRepository.getAllPlayers() } returns flowOf(listOf(playerA, playerB, playerC))

        val viewModel = createViewModel()
        viewModel.togglePlayerSelection("10")
        viewModel.togglePlayerSelection("20")
        viewModel.togglePlayerSelection("30")

        val expectedOrder = listOf("30", "10", "20")
        viewModel.createGame(expectedOrder)

        coVerify { 
            offlineGameRepository.createGameSet(
                name = any(),
                settings = any(),
                playerIds = listOf(30, 10, 20)
            )
        }
    }

    @Test
    fun `updateSettings with fewer than 4 players forces dublee false`() = runTest {
        val p1 = Player(id = "1", name = "P1")
        val p2 = Player(id = "2", name = "P2")
        val p3 = Player(id = "3", name = "P3")
        every { sessionManager.isOnlineMode() } returns false
        coEvery { offlineGameRepository.getAllPlayers() } returns flowOf(listOf(p1, p2, p3))

        val viewModel = createViewModel()
        viewModel.togglePlayerSelection("1")
        viewModel.togglePlayerSelection("2")
        viewModel.togglePlayerSelection("3")

        viewModel.updateSettings(GameSettings.default().copy(dublee = true))

        assertFalse(viewModel.uiState.value.settings.dublee)
    }

    @Test
    fun `updateSettings with 4 or more players allows dublee true`() = runTest {
        val p1 = Player(id = "1", name = "P1")
        val p2 = Player(id = "2", name = "P2")
        val p3 = Player(id = "3", name = "P3")
        val p4 = Player(id = "4", name = "P4")
        every { sessionManager.isOnlineMode() } returns false
        coEvery { offlineGameRepository.getAllPlayers() } returns flowOf(listOf(p1, p2, p3, p4))

        val viewModel = createViewModel()
        viewModel.togglePlayerSelection("1")
        viewModel.togglePlayerSelection("2")
        viewModel.togglePlayerSelection("3")
        viewModel.togglePlayerSelection("4")

        viewModel.updateSettings(GameSettings.default().copy(dublee = true))

        assertTrue(viewModel.uiState.value.settings.dublee)
    }

    @Test
    fun `togglePlayerSelection dropping below 4 players resets dublee to false`() = runTest {
        val p1 = Player(id = "1", name = "P1")
        val p2 = Player(id = "2", name = "P2")
        val p3 = Player(id = "3", name = "P3")
        val p4 = Player(id = "4", name = "P4")
        every { sessionManager.isOnlineMode() } returns false
        coEvery { offlineGameRepository.getAllPlayers() } returns flowOf(listOf(p1, p2, p3, p4))

        val viewModel = createViewModel()
        viewModel.togglePlayerSelection("1")
        viewModel.togglePlayerSelection("2")
        viewModel.togglePlayerSelection("3")
        viewModel.togglePlayerSelection("4")

        viewModel.updateSettings(GameSettings.default().copy(dublee = true))
        assertTrue(viewModel.uiState.value.settings.dublee)

        // Deselect player 4 -> down to 3 players
        viewModel.togglePlayerSelection("4")
        assertFalse(viewModel.uiState.value.settings.dublee)
    }

    @Test
    fun `createGame with 3 players enforces dublee false in saved settings`() = runTest {
        val p1 = Player(id = "1", name = "P1")
        val p2 = Player(id = "2", name = "P2")
        val p3 = Player(id = "3", name = "P3")
        every { sessionManager.isOnlineMode() } returns false
        coEvery { offlineGameRepository.getAllPlayers() } returns flowOf(listOf(p1, p2, p3))

        val viewModel = createViewModel()
        viewModel.togglePlayerSelection("1")
        viewModel.togglePlayerSelection("2")
        viewModel.togglePlayerSelection("3")

        viewModel.createGame(listOf("1", "2", "3"))

        coVerify {
            offlineGameRepository.createGameSet(
                name = any(),
                settings = match { !it.dublee },
                playerIds = listOf(1, 2, 3)
            )
        }
    }
}
