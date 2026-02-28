package com.sanjeeb.marriagecalculator.ui.roundinput

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.sanjeeb.marriagecalculator.data.model.GameSettings
import com.sanjeeb.marriagecalculator.data.model.Player
import com.sanjeeb.marriagecalculator.data.remote.*
import com.sanjeeb.marriagecalculator.data.repository.ApiResult
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class PlayerRoundState(
    val player: Player,
    val seen: Boolean = false,
    val maal: Int = 0,
    val duply: Boolean = false,
    val isWinner: Boolean = false,
    val isDealer: Boolean = false,
    // Calculated preview
    val previewScore: Int = 0,
    val previewMoney: Double = 0.0
)

data class RoundInputUiState(
    val playerStates: List<PlayerRoundState> = emptyList(),
    val settings: GameSettings = GameSettings.default(),
    val winnerId: Int? = null,
    val dealerId: Int? = null,
    val isLoading: Boolean = false,
    val error: String? = null,
    val submitted: Boolean = false,
    val showPreview: Boolean = false
)

@HiltViewModel
class RoundInputViewModel @Inject constructor(
    private val scoringApi: ScoringApiService
) : ViewModel() {

    private val _uiState = MutableStateFlow(RoundInputUiState())
    val uiState: StateFlow<RoundInputUiState> = _uiState.asStateFlow()

    fun initPlayers(players: List<Player>, settings: GameSettings) {
        _uiState.value = _uiState.value.copy(
            playerStates = players.map { PlayerRoundState(player = it) },
            settings = settings
        )
    }

    fun setWinner(playerId: Int) {
        val current = _uiState.value
        val newStates = current.playerStates.map {
            it.copy(
                isWinner = it.player.id == playerId,
                seen = if (it.player.id == playerId) true else it.seen
            )
        }
        _uiState.value = current.copy(
            playerStates = newStates,
            winnerId = playerId
        )
        calculatePreview()
    }

    fun setDealer(playerId: Int) {
        val current = _uiState.value
        val newStates = current.playerStates.map {
            it.copy(isDealer = it.player.id == playerId)
        }
        _uiState.value = current.copy(
            playerStates = newStates,
            dealerId = playerId
        )
    }

    fun toggleSeen(playerId: Int) {
        val current = _uiState.value
        val newStates = current.playerStates.map {
            if (it.player.id == playerId && !it.isWinner) {
                it.copy(seen = !it.seen)
            } else it
        }
        _uiState.value = current.copy(playerStates = newStates)
        calculatePreview()
    }

    fun toggleDuply(playerId: Int) {
        val current = _uiState.value
        val newStates = current.playerStates.map {
            if (it.player.id == playerId) it.copy(duply = !it.duply) else it
        }
        _uiState.value = current.copy(playerStates = newStates)
        calculatePreview()
    }

    fun setMaal(playerId: Int, maal: Int) {
        val current = _uiState.value
        val newStates = current.playerStates.map {
            if (it.player.id == playerId) it.copy(maal = maal.coerceAtLeast(0)) else it
        }
        _uiState.value = current.copy(playerStates = newStates)
        calculatePreview()
    }

    private fun calculatePreview() {
        val state = _uiState.value
        if (state.winnerId == null) return

        // Local calculation (mirrors C# ScoringEngine logic)
        val settings = state.settings
        val players = state.playerStates.toMutableList()
        val winnerIdx = players.indexOfFirst { it.isWinner }
        if (winnerIdx < 0) return

        // Copy maal values and apply game mode
        val maalValues = players.map { it.maal }.toMutableList()
        val seenFlags = players.map { it.seen || it.isWinner }.toMutableList()

        if (settings.kidnap) {
            for (i in players.indices) {
                if (!seenFlags[i] && !players[i].isWinner) {
                    maalValues[winnerIdx] += maalValues[i]
                    maalValues[i] = 0
                }
            }
        } else if (settings.murder) {
            for (i in players.indices) {
                if (!seenFlags[i] && !players[i].isWinner) {
                    maalValues[i] = 0
                }
            }
        }

        val scores = MutableList(players.size) { 0 }

        // Fixed penalties
        for (i in players.indices) {
            if (players[i].isWinner) continue
            val penalty = if (!seenFlags[i]) settings.unseenPoint else settings.seenPoint
            val bonus = if (players[winnerIdx].duply && settings.dublee) settings.dubleePointBonus else 0
            val total = penalty + bonus
            scores[i] -= total
            scores[winnerIdx] += total
        }

        // Maal distribution
        val seenIndices = players.indices.filter { seenFlags[it] }
        val unseenIndices = players.indices.filter { !seenFlags[it] && !players[it].isWinner }

        for (u in unseenIndices) {
            for (s in seenIndices) {
                val diff = maalValues[s] - maalValues[u]
                scores[s] += diff
                scores[u] -= diff
            }
        }

        for (i in seenIndices.indices) {
            for (j in i + 1 until seenIndices.size) {
                val a = seenIndices[i]
                val b = seenIndices[j]
                val diff = maalValues[a] - maalValues[b]
                scores[a] += diff
                scores[b] -= diff
            }
        }

        val updatedStates = players.mapIndexed { idx, ps ->
            ps.copy(
                previewScore = scores[idx],
                previewMoney = scores[idx] * settings.pointRate
            )
        }

        _uiState.value = state.copy(playerStates = updatedStates, showPreview = true)
    }

    fun submitRound() {
        val state = _uiState.value
        if (state.winnerId == null) {
            _uiState.value = state.copy(error = "Please select a winner")
            return
        }
        // For now, mark as submitted (API integration later when online)
        _uiState.value = state.copy(submitted = true, error = null)
    }

    fun clearError() {
        _uiState.value = _uiState.value.copy(error = null)
    }
}
