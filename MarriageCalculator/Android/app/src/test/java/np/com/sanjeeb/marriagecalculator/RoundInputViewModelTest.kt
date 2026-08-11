package np.com.sanjeeb.marriagecalculator

import io.mockk.mockk
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.StandardTestDispatcher
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.setMain
import np.com.sanjeeb.marriagecalculator.data.model.GameSettings
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.ui.roundinput.RoundInputViewModel
import org.junit.After
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

@OptIn(ExperimentalCoroutinesApi::class)
class RoundInputViewModelTest {

    private val testDispatcher = StandardTestDispatcher()
    private lateinit var viewModel: RoundInputViewModel

    @Before
    fun setUp() {
        Dispatchers.setMain(testDispatcher)
        viewModel = RoundInputViewModel(
            scoringApi = mockk(relaxed = true),
            offlineGameRepository = mockk(relaxed = true),
            gameSetRepository = mockk(relaxed = true),
            sessionManager = mockk(relaxed = true)
        )
    }

    @After
    fun tearDown() {
        Dispatchers.resetMain()
    }

    @Test
    fun `toggleDuply to true automatically selects seen`() {
        val players = listOf(
            Player(id = "1", name = "San"),
            Player(id = "2", name = "Aar")
        )
        viewModel.initPlayers(players, GameSettings.default())

        assertFalse(viewModel.uiState.value.playerStates[0].seen)
        assertFalse(viewModel.uiState.value.playerStates[0].duply)

        viewModel.toggleDuply("1")

        assertTrue(viewModel.uiState.value.playerStates[0].duply)
        assertTrue(viewModel.uiState.value.playerStates[0].seen)
    }

    @Test
    fun `toggleDuply to false keeps seen checked`() {
        val players = listOf(
            Player(id = "1", name = "San"),
            Player(id = "2", name = "Aar")
        )
        viewModel.initPlayers(players, GameSettings.default())

        viewModel.toggleDuply("1") // turns dub on, seen auto-checked
        assertTrue(viewModel.uiState.value.playerStates[0].duply)
        assertTrue(viewModel.uiState.value.playerStates[0].seen)

        viewModel.toggleDuply("1") // turns dub off
        assertFalse(viewModel.uiState.value.playerStates[0].duply)
        assertTrue(viewModel.uiState.value.playerStates[0].seen) // seen stays checked!
    }

    @Test
    fun `toggleSeen to false clears duply`() {
        val players = listOf(
            Player(id = "1", name = "San"),
            Player(id = "2", name = "Aar")
        )
        viewModel.initPlayers(players, GameSettings.default())

        viewModel.toggleDuply("1")
        assertTrue(viewModel.uiState.value.playerStates[0].duply)
        assertTrue(viewModel.uiState.value.playerStates[0].seen)

        viewModel.toggleSeen("1") // untick seen
        assertFalse(viewModel.uiState.value.playerStates[0].seen)
        assertFalse(viewModel.uiState.value.playerStates[0].duply) // duply also unticked
    }
}
