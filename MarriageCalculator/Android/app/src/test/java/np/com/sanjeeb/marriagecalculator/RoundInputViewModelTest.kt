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

    @Test
    fun `addMaalPoints increments points and marks seen`() {
        val players = listOf(
            Player(id = "1", name = "San"),
            Player(id = "2", name = "Aar")
        )
        viewModel.initPlayers(players, GameSettings.default())

        assertFalse(viewModel.uiState.value.playerStates[0].seen)
        org.junit.Assert.assertEquals(0, viewModel.uiState.value.playerStates[0].seenPoints)

        // Adding preset +5 should mark seen = true and seenPoints = 5
        viewModel.addMaalPoints("1", 5)
        assertTrue(viewModel.uiState.value.playerStates[0].seen)
        org.junit.Assert.assertEquals(5, viewModel.uiState.value.playerStates[0].seenPoints)

        // Adding preset +8 should accumulate to 13
        viewModel.addMaalPoints("1", 8)
        org.junit.Assert.assertEquals(13, viewModel.uiState.value.playerStates[0].seenPoints)
    }

    @Test
    fun `addMaalPoints respects bounds between 0 and 99`() {
        val players = listOf(
            Player(id = "1", name = "San")
        )
        viewModel.initPlayers(players, GameSettings.default())

        viewModel.setSeenPoints("1", 95)
        viewModel.addMaalPoints("1", 10) // 95 + 10 -> capped at 99
        org.junit.Assert.assertEquals(99, viewModel.uiState.value.playerStates[0].seenPoints)

        viewModel.addMaalPoints("1", -150) // capped at 0
        org.junit.Assert.assertEquals(0, viewModel.uiState.value.playerStates[0].seenPoints)
    }
}
