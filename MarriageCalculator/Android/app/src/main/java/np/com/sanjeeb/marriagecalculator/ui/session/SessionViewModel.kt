package np.com.sanjeeb.marriagecalculator.ui.session

import androidx.lifecycle.ViewModel
import np.com.sanjeeb.marriagecalculator.data.model.Player
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

data class SeatedPlayer(
    val player: Player,
    val seatPosition: Int,
    val isActive: Boolean = true
)

data class SessionUiState(
    val players: List<SeatedPlayer> = emptyList(),
    val maxPlayers: Int = 6,
    val isSettled: Boolean = false,
    val showAddPlayer: Boolean = false
)

@HiltViewModel
class SessionViewModel @Inject constructor() : ViewModel() {

    private val _uiState = MutableStateFlow(SessionUiState())
    val uiState: StateFlow<SessionUiState> = _uiState.asStateFlow()

    fun initSession(players: List<Player>) {
        _uiState.value = _uiState.value.copy(
            players = players.mapIndexed { i, p -> SeatedPlayer(p, i) }
        )
    }

    fun addPlayer(player: Player): Boolean {
        val state = _uiState.value
        if (state.players.size >= state.maxPlayers) return false
        if (state.players.any { it.player.id == player.id }) return false

        val nextSeat = state.players.maxOfOrNull { it.seatPosition + 1 } ?: 0
        val newSeated = SeatedPlayer(player, nextSeat)
        _uiState.value = state.copy(
            players = state.players + newSeated,
            showAddPlayer = false
        )
        return true
    }

    fun removePlayer(playerId: String): Boolean {
        val state = _uiState.value
        if (state.players.size <= 2) return false
        _uiState.value = state.copy(
            players = state.players.filter { it.player.id != playerId }
        )
        return true
    }

    fun togglePlayerActive(playerId: String) {
        val state = _uiState.value
        _uiState.value = state.copy(
            players = state.players.map {
                if (it.player.id == playerId) it.copy(isActive = !it.isActive) else it
            }
        )
    }

    fun swapSeats(playerId1: String, playerId2: String) {
        val state = _uiState.value
        val p1 = state.players.find { it.player.id == playerId1 } ?: return
        val p2 = state.players.find { it.player.id == playerId2 } ?: return
        _uiState.value = state.copy(
            players = state.players.map {
                when (it.player.id) {
                    playerId1 -> it.copy(seatPosition = p2.seatPosition)
                    playerId2 -> it.copy(seatPosition = p1.seatPosition)
                    else -> it
                }
            }
        )
    }

    fun getActivePlayers(): List<Player> =
        _uiState.value.players.filter { it.isActive }.map { it.player }

    fun getSeatedPlayers(): List<SeatedPlayer> =
        _uiState.value.players.sortedBy { it.seatPosition }

    fun settle() {
        _uiState.value = _uiState.value.copy(isSettled = true)
    }

    fun toggleAddPlayer() {
        _uiState.value = _uiState.value.copy(showAddPlayer = !_uiState.value.showAddPlayer)
    }
}
