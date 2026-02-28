package com.sanjeeb.marriagecalculator.ui.gamesetup

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.sanjeeb.marriagecalculator.data.model.*
import com.sanjeeb.marriagecalculator.data.repository.*
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class GameSetupUiState(
    val gameName: String = "",
    val players: List<Player> = emptyList(),
    val selectedPlayerIds: Set<Int> = emptySet(),
    val newPlayerName: String = "",
    val settings: GameSettings = GameSettings.default(),
    val isLoading: Boolean = false,
    val error: String? = null,
    val createdGameSetId: Int? = null,
    val showAddPlayer: Boolean = false,
    // Offline mode: use local dummy players
    val isOfflineMode: Boolean = true,
    val localPlayers: List<Player> = emptyList(),
    val nextLocalId: Int = -1
)

@HiltViewModel
class GameSetupViewModel @Inject constructor(
    private val playerRepository: PlayerRepository,
    private val gameSettingsRepository: GameSettingsRepository,
    private val gameSetRepository: GameSetRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(GameSetupUiState())
    val uiState: StateFlow<GameSetupUiState> = _uiState.asStateFlow()

    init {
        loadPlayers()
    }

    fun loadPlayers() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            when (val result = playerRepository.getPlayers()) {
                is ApiResult.Success -> {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        players = result.data.filter { !it.deleted },
                        isOfflineMode = false
                    )
                }
                is ApiResult.Error -> {
                    // Fallback to offline mode with local players
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        isOfflineMode = true
                    )
                }
                is ApiResult.Loading -> {}
            }
        }
    }

    fun setGameName(name: String) {
        _uiState.value = _uiState.value.copy(gameName = name)
    }

    fun setNewPlayerName(name: String) {
        _uiState.value = _uiState.value.copy(newPlayerName = name)
    }

    fun toggleShowAddPlayer() {
        _uiState.value = _uiState.value.copy(showAddPlayer = !_uiState.value.showAddPlayer)
    }

    fun addLocalPlayer() {
        val name = _uiState.value.newPlayerName.trim()
        if (name.isBlank()) return
        val id = _uiState.value.nextLocalId
        val player = Player(id = id, name = name)
        _uiState.value = _uiState.value.copy(
            localPlayers = _uiState.value.localPlayers + player,
            selectedPlayerIds = _uiState.value.selectedPlayerIds + id,
            newPlayerName = "",
            nextLocalId = id - 1,
            showAddPlayer = false
        )
    }

    fun togglePlayerSelection(playerId: Int) {
        val current = _uiState.value.selectedPlayerIds
        val allPlayers = getAllPlayers()
        val newSelection = if (current.contains(playerId)) {
            current - playerId
        } else {
            if (current.size >= 6) {
                _uiState.value = _uiState.value.copy(error = "Maximum 6 players allowed")
                return
            }
            current + playerId
        }
        _uiState.value = _uiState.value.copy(
            selectedPlayerIds = newSelection,
            error = null
        )
    }

    fun updateSettings(settings: GameSettings) {
        _uiState.value = _uiState.value.copy(settings = settings)
    }

    fun getAllPlayers(): List<Player> {
        return _uiState.value.players + _uiState.value.localPlayers
    }

    fun getSelectedPlayers(): List<Player> {
        val allPlayers = getAllPlayers()
        return allPlayers.filter { _uiState.value.selectedPlayerIds.contains(it.id) }
    }

    fun createGame() {
        val state = _uiState.value
        if (state.selectedPlayerIds.size < 2) {
            _uiState.value = state.copy(error = "Select at least 2 players")
            return
        }
        if (state.selectedPlayerIds.size > 6) {
            _uiState.value = state.copy(error = "Maximum 6 players")
            return
        }

        if (state.isOfflineMode) {
            // In offline mode, just return a local game set ID
            _uiState.value = state.copy(createdGameSetId = -1, error = null)
            return
        }

        viewModelScope.launch {
            _uiState.value = state.copy(isLoading = true, error = null)

            // Create settings first
            val settingsRequest = CreateGameSettingsRequest(
                murder = state.settings.murder,
                kidnap = state.settings.kidnap,
                seenPoint = state.settings.seenPoint,
                unseenPoint = state.settings.unseenPoint,
                pointRate = state.settings.pointRate,
                currency = state.settings.currency,
                dublee = state.settings.dublee,
                dubleePointLess = state.settings.dubleePointLess,
                dubleePointBonus = state.settings.dubleePointBonus,
                foulPoint = state.settings.foulPoint,
                foulPointBonus = state.settings.foulPointBonus,
                audio = state.settings.audio
            )

            when (val settingsResult = gameSettingsRepository.createGameSettings(settingsRequest)) {
                is ApiResult.Success -> {
                    val gameSetRequest = CreateGameSetRequest(
                        name = state.gameName.ifBlank { "Game ${System.currentTimeMillis()}" },
                        gameSettingsId = settingsResult.data.id,
                        playerIds = state.selectedPlayerIds.filter { it > 0 }.toList()
                    )
                    when (val gameSetResult = gameSetRepository.createGameSet(gameSetRequest)) {
                        is ApiResult.Success -> {
                            _uiState.value = state.copy(
                                isLoading = false,
                                createdGameSetId = gameSetResult.data.id
                            )
                        }
                        is ApiResult.Error -> {
                            _uiState.value = state.copy(isLoading = false, error = gameSetResult.message)
                        }
                        is ApiResult.Loading -> {}
                    }
                }
                is ApiResult.Error -> {
                    _uiState.value = state.copy(isLoading = false, error = settingsResult.message)
                }
                is ApiResult.Loading -> {}
            }
        }
    }

    fun clearError() {
        _uiState.value = _uiState.value.copy(error = null)
    }
}
