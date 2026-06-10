package com.sanjeeb.marriagecalculator

import com.sanjeeb.marriagecalculator.data.model.Player
import com.sanjeeb.marriagecalculator.ui.session.SessionViewModel
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test

class SessionViewModelTest {

    private lateinit var viewModel: SessionViewModel
    private val players = listOf(
        Player(id = "1", name = "Alice"),
        Player(id = "2", name = "Bob"),
        Player(id = "3", name = "Charlie"),
        Player(id = "4", name = "Dave")
    )

    @Before
    fun setup() {
        viewModel = SessionViewModel()
        viewModel.initSession(players)
    }

    @Test
    fun `initSession creates seated players`() {
        val state = viewModel.uiState.value
        assertEquals(4, state.players.size)
        assertEquals(0, state.players[0].seatPosition)
        assertEquals(3, state.players[3].seatPosition)
        assertTrue(state.players.all { it.isActive })
    }

    @Test
    fun `addPlayer succeeds when under max`() {
        val newPlayer = Player(id = "5", name = "Eve")
        assertTrue(viewModel.addPlayer(newPlayer))
        assertEquals(5, viewModel.uiState.value.players.size)
    }

    @Test
    fun `addPlayer fails for duplicate`() {
        assertFalse(viewModel.addPlayer(Player(id = "1", name = "Alice")))
        assertEquals(4, viewModel.uiState.value.players.size)
    }

    @Test
    fun `addPlayer fails when at max`() {
        viewModel.addPlayer(Player(id = "5", name = "Eve"))
        viewModel.addPlayer(Player(id = "6", name = "Frank"))
        assertFalse(viewModel.addPlayer(Player(id = "7", name = "Grace")))
        assertEquals(6, viewModel.uiState.value.players.size)
    }

    @Test
    fun `removePlayer succeeds when more than 2`() {
        assertTrue(viewModel.removePlayer("4"))
        assertEquals(3, viewModel.uiState.value.players.size)
    }

    @Test
    fun `removePlayer fails at minimum 2`() {
        viewModel.removePlayer("3")
        viewModel.removePlayer("4")
        assertFalse(viewModel.removePlayer("2"))
        assertEquals(2, viewModel.uiState.value.players.size)
    }

    @Test
    fun `togglePlayerActive works`() {
        viewModel.togglePlayerActive("1")
        assertFalse(viewModel.uiState.value.players.find { it.player.id == "1" }!!.isActive)
        viewModel.togglePlayerActive("1")
        assertTrue(viewModel.uiState.value.players.find { it.player.id == "1" }!!.isActive)
    }

    @Test
    fun `swapSeats exchanges positions`() {
        viewModel.swapSeats("1", "3")
        val state = viewModel.uiState.value
        assertEquals(2, state.players.find { it.player.id == "1" }!!.seatPosition)
        assertEquals(0, state.players.find { it.player.id == "3" }!!.seatPosition)
    }

    @Test
    fun `getActivePlayers excludes inactive`() {
        viewModel.togglePlayerActive("2")
        val active = viewModel.getActivePlayers()
        assertEquals(3, active.size)
        assertFalse(active.any { it.id == "2" })
    }

    @Test
    fun `settle marks game as settled`() {
        assertFalse(viewModel.uiState.value.isSettled)
        viewModel.settle()
        assertTrue(viewModel.uiState.value.isSettled)
    }
}
