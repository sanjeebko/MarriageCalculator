package com.sanjeeb.marriagecalculator

import com.sanjeeb.marriagecalculator.data.model.Currency
import com.sanjeeb.marriagecalculator.data.model.GameSettings
import com.sanjeeb.marriagecalculator.data.model.Player
import com.sanjeeb.marriagecalculator.ui.scoreboard.ScoreboardViewModel
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test

class ScoreboardViewModelTest {

    private lateinit var viewModel: ScoreboardViewModel
    private val testPlayers = listOf(
        Player(id = 1, name = "Alice"),
        Player(id = 2, name = "Bob"),
        Player(id = 3, name = "Charlie"),
        Player(id = 4, name = "Dave")
    )
    private val settings = GameSettings(
        pointRate = 10.0,
        seenPoint = 3,
        unseenPoint = 10,
        murder = true,
        kidnap = false,
        currency = Currency.NPR_Rupee
    )

    @Before
    fun setup() {
        viewModel = ScoreboardViewModel()
        viewModel.initScoreboard(testPlayers, settings)
    }

    @Test
    fun `initScoreboard sets players with zero scores`() {
        val state = viewModel.uiState.value
        assertEquals(4, state.players.size)
        assertTrue(state.players.all { it.totalPoints == 0 })
        assertTrue(state.players.all { it.totalMoney == 0.0 })
    }

    @Test
    fun `addRoundResult updates player scores`() {
        val scores = mapOf(1 to 30, 2 to -10, 3 to -10, 4 to -10)
        viewModel.addRoundResult(winnerId = 1, scores = scores, totalMaal = 15)

        val state = viewModel.uiState.value
        assertEquals(30, state.players.find { it.player.id == 1 }!!.totalPoints)
        assertEquals(-10, state.players.find { it.player.id == 2 }!!.totalPoints)
        assertEquals(1, state.rounds.size)
    }

    @Test
    fun `multiple rounds accumulate scores`() {
        viewModel.addRoundResult(1, mapOf(1 to 20, 2 to -5, 3 to -5, 4 to -10), 10)
        viewModel.addRoundResult(2, mapOf(1 to -8, 2 to 24, 3 to -8, 4 to -8), 12)

        val state = viewModel.uiState.value
        assertEquals(12, state.players.find { it.player.id == 1 }!!.totalPoints) // 20 + (-8)
        assertEquals(19, state.players.find { it.player.id == 2 }!!.totalPoints) // -5 + 24
        assertEquals(2, state.rounds.size)
    }

    @Test
    fun `winner count tracks correctly`() {
        viewModel.addRoundResult(1, mapOf(1 to 20, 2 to -5, 3 to -5, 4 to -10), 10)
        viewModel.addRoundResult(1, mapOf(1 to 15, 2 to -5, 3 to -5, 4 to -5), 8)
        viewModel.addRoundResult(3, mapOf(1 to -5, 2 to -5, 3 to 15, 4 to -5), 8)

        val state = viewModel.uiState.value
        assertEquals(2, state.players.find { it.player.id == 1 }!!.gamesWon)
        assertEquals(0, state.players.find { it.player.id == 2 }!!.gamesWon)
        assertEquals(1, state.players.find { it.player.id == 3 }!!.gamesWon)
    }

    @Test
    fun `money calculation uses point rate`() {
        viewModel.addRoundResult(1, mapOf(1 to 30, 2 to -10, 3 to -10, 4 to -10), 15)

        val state = viewModel.uiState.value
        assertEquals(300.0, state.players.find { it.player.id == 1 }!!.totalMoney, 0.01)
        assertEquals(-100.0, state.players.find { it.player.id == 2 }!!.totalMoney, 0.01)
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
    fun `settleGame marks as settled`() {
        assertFalse(viewModel.uiState.value.isSettled)
        viewModel.settleGame()
        assertTrue(viewModel.uiState.value.isSettled)
    }

    @Test
    fun `round history records winner name`() {
        viewModel.addRoundResult(2, mapOf(1 to -10, 2 to 30, 3 to -10, 4 to -10), 12)

        val round = viewModel.uiState.value.rounds.first()
        assertEquals("Bob", round.winnerName)
        assertEquals(2, round.winnerId)
        assertEquals(1, round.roundNumber)
    }
}
