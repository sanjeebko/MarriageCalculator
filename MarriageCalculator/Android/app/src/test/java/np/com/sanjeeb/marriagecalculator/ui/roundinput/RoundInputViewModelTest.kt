package np.com.sanjeeb.marriagecalculator.ui.roundinput

import np.com.sanjeeb.marriagecalculator.data.model.GameSettings
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.data.remote.ScoringApiService
import np.com.sanjeeb.marriagecalculator.data.repository.GameSetRepository
import np.com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import io.mockk.mockk
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test

class RoundInputViewModelTest {

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
        viewModel = RoundInputViewModel(
            scoringApi = scoringApi,
            offlineGameRepository = offlineGameRepository,
            gameSetRepository = gameSetRepository,
            sessionManager = sessionManager
        )
        viewModel.initPlayers(testPlayers, GameSettings.default())
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
}
