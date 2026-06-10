package com.sanjeeb.marriagecalculator

import com.sanjeeb.marriagecalculator.data.local.GameSetEntity
import com.sanjeeb.marriagecalculator.data.local.RoundEntity
import com.sanjeeb.marriagecalculator.data.local.RoundScoreEntity
import com.sanjeeb.marriagecalculator.data.model.Currency
import com.sanjeeb.marriagecalculator.data.model.GameSettings
import com.sanjeeb.marriagecalculator.data.model.Player
import com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import com.sanjeeb.marriagecalculator.ui.scoreboard.ScoreboardViewModel
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
class ScoreboardViewModelTest {

    private val testDispatcher = UnconfinedTestDispatcher()

    private lateinit var viewModel: ScoreboardViewModel
    private val repository: OfflineGameRepository = mockk(relaxed = true)

    private val testPlayers = listOf(
        Player(id = "1", name = "Alice"),
        Player(id = "2", name = "Bob"),
        Player(id = "3", name = "Charlie"),
        Player(id = "4", name = "Dave")
    )

    private val settings = GameSettings(
        id = "1",
        pointRate = 10.0,
        seenPoint = 3,
        unseenPoint = 10,
        murder = true,
        kidnap = false,
        currency = Currency.NPR_Rupee
    )

    private val gameSetEntity = GameSetEntity(
        id = 1,
        settingsId = 1,
        isActive = true,
        isSettled = false
    )

    @Before
    fun setup() {
        Dispatchers.setMain(testDispatcher)
        viewModel = ScoreboardViewModel(repository)
    }

    @After
    fun tearDown() {
        Dispatchers.resetMain()
    }

    @Test
    fun `loadScoreboardData successfully updates uiState`() = runTest {
        val gameSetId = 1
        
        coEvery { repository.getGameSetPlayers(gameSetId) } returns testPlayers
        coEvery { repository.getGameSet(gameSetId) } returns gameSetEntity
        coEvery { repository.getGameSettings(1) } returns settings

        val roundEntity = RoundEntity(id = 10, gameSetId = gameSetId, roundNumber = 1, winnerId = 1, totalMaal = 15)
        every { repository.getRounds(gameSetId) } returns flowOf(listOf(roundEntity))

        val scores = listOf(
            RoundScoreEntity(id = 101, roundId = 10, playerId = 1, score = 30, maal = 15, isSeen = true, isWinner = true),
            RoundScoreEntity(id = 102, roundId = 10, playerId = 2, score = -10, maal = 0, isSeen = true, isWinner = false),
            RoundScoreEntity(id = 103, roundId = 10, playerId = 3, score = -10, maal = 0, isSeen = true, isWinner = false),
            RoundScoreEntity(id = 104, roundId = 10, playerId = 4, score = -10, maal = 0, isSeen = true, isWinner = false)
        )
        coEvery { repository.getAllScoresForGameSet(gameSetId) } returns scores

        viewModel.loadScoreboardData("1")

        val state = viewModel.uiState.value
        assertEquals("1", state.gameSetId)
        assertEquals(4, state.players.size)
        assertEquals(30, state.players.find { it.player.id == "1" }!!.totalPoints)
        assertEquals(-10, state.players.find { it.player.id == "2" }!!.totalPoints)
        assertEquals(300.0, state.players.find { it.player.id == "1" }!!.totalMoney, 0.01)
        assertEquals(-100.0, state.players.find { it.player.id == "2" }!!.totalMoney, 0.01)
        assertEquals(1, state.rounds.size)
        assertEquals("Alice", state.rounds[0].winnerName)
        assertEquals(15, state.rounds[0].totalMaal)
    }

    @Test
    fun `multiple rounds accumulate scores`() = runTest {
        val gameSetId = 1
        
        coEvery { repository.getGameSetPlayers(gameSetId) } returns testPlayers
        coEvery { repository.getGameSet(gameSetId) } returns gameSetEntity
        coEvery { repository.getGameSettings(1) } returns settings

        val round1 = RoundEntity(id = 10, gameSetId = gameSetId, roundNumber = 1, winnerId = 1, totalMaal = 10)
        val round2 = RoundEntity(id = 11, gameSetId = gameSetId, roundNumber = 2, winnerId = 2, totalMaal = 12)
        every { repository.getRounds(gameSetId) } returns flowOf(listOf(round1, round2))

        val scores = listOf(
            RoundScoreEntity(id = 101, roundId = 10, playerId = 1, score = 20, isWinner = true),
            RoundScoreEntity(id = 102, roundId = 10, playerId = 2, score = -5, isWinner = false),
            RoundScoreEntity(id = 103, roundId = 10, playerId = 3, score = -5, isWinner = false),
            RoundScoreEntity(id = 104, roundId = 10, playerId = 4, score = -10, isWinner = false),
            RoundScoreEntity(id = 105, roundId = 11, playerId = 1, score = -8, isWinner = false),
            RoundScoreEntity(id = 106, roundId = 11, playerId = 2, score = 24, isWinner = true),
            RoundScoreEntity(id = 107, roundId = 11, playerId = 3, score = -8, isWinner = false),
            RoundScoreEntity(id = 108, roundId = 11, playerId = 4, score = -8, isWinner = false)
        )
        coEvery { repository.getAllScoresForGameSet(gameSetId) } returns scores

        viewModel.loadScoreboardData("1")

        val state = viewModel.uiState.value
        assertEquals(12, state.players.find { it.player.id == "1" }!!.totalPoints) // 20 + (-8)
        assertEquals(19, state.players.find { it.player.id == "2" }!!.totalPoints) // -5 + 24
        assertEquals(2, state.rounds.size)
        assertEquals("Alice", state.rounds[0].winnerName)
        assertEquals("Bob", state.rounds[1].winnerName)
    }

    @Test
    fun `winner count tracks correctly`() = runTest {
        val gameSetId = 1
        
        coEvery { repository.getGameSetPlayers(gameSetId) } returns testPlayers
        coEvery { repository.getGameSet(gameSetId) } returns gameSetEntity
        coEvery { repository.getGameSettings(1) } returns settings

        val round1 = RoundEntity(id = 10, gameSetId = gameSetId, roundNumber = 1, winnerId = 1, totalMaal = 10)
        val round2 = RoundEntity(id = 11, gameSetId = gameSetId, roundNumber = 2, winnerId = 1, totalMaal = 8)
        val round3 = RoundEntity(id = 12, gameSetId = gameSetId, roundNumber = 3, winnerId = 3, totalMaal = 8)
        every { repository.getRounds(gameSetId) } returns flowOf(listOf(round1, round2, round3))

        val scores = listOf(
            RoundScoreEntity(id = 101, roundId = 10, playerId = 1, score = 20, isWinner = true),
            RoundScoreEntity(id = 102, roundId = 10, playerId = 2, score = -5, isWinner = false),
            RoundScoreEntity(id = 103, roundId = 10, playerId = 3, score = -5, isWinner = false),
            RoundScoreEntity(id = 104, roundId = 10, playerId = 4, score = -10, isWinner = false),
            RoundScoreEntity(id = 105, roundId = 11, playerId = 1, score = 15, isWinner = true),
            RoundScoreEntity(id = 106, roundId = 11, playerId = 2, score = -5, isWinner = false),
            RoundScoreEntity(id = 107, roundId = 11, playerId = 3, score = -5, isWinner = false),
            RoundScoreEntity(id = 108, roundId = 11, playerId = 4, score = -5, isWinner = false),
            RoundScoreEntity(id = 109, roundId = 12, playerId = 1, score = -5, isWinner = false),
            RoundScoreEntity(id = 110, roundId = 12, playerId = 2, score = -5, isWinner = false),
            RoundScoreEntity(id = 111, roundId = 12, playerId = 3, score = 15, isWinner = true),
            RoundScoreEntity(id = 112, roundId = 12, playerId = 4, score = -5, isWinner = false)
        )
        coEvery { repository.getAllScoresForGameSet(gameSetId) } returns scores

        viewModel.loadScoreboardData("1")

        val state = viewModel.uiState.value
        assertEquals(2, state.players.find { it.player.id == "1" }!!.gamesWon)
        assertEquals(0, state.players.find { it.player.id == "2" }!!.gamesWon)
        assertEquals(1, state.players.find { it.player.id == "3" }!!.gamesWon)
    }

    @Test
    fun `toggleHistory flips show state`() {
        assertFalse(viewModel.uiState.value.showHistory)
        viewModel.toggleHistory()
        assertTrue(viewModel.uiState.value.showHistory)
        viewModel.toggleHistory()
        assertFalse(viewModel.uiState.value.showHistory)
    }

    @Test
    fun `settleGame calls repository and marks as settled`() = runTest {
        val gameSetId = 1
        
        coEvery { repository.getGameSetPlayers(gameSetId) } returns testPlayers
        coEvery { repository.getGameSet(gameSetId) } returns gameSetEntity
        coEvery { repository.getGameSettings(1) } returns settings
        every { repository.getRounds(gameSetId) } returns flowOf(emptyList())
        coEvery { repository.getAllScoresForGameSet(gameSetId) } returns emptyList()

        viewModel.loadScoreboardData("1")
        
        assertFalse(viewModel.uiState.value.isSettled)

        coEvery { repository.settleGame(gameSetId) } just Runs

        viewModel.settleGame()

        assertTrue(viewModel.uiState.value.isSettled)
        coVerify(exactly = 1) { repository.settleGame(gameSetId) }
    }
}
