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

import kotlinx.coroutines.flow.first

data class GameSetupUiState(
    val gameName: String = "",
    val players: List<Player> = emptyList(),
    val selectedPlayerIds: Set<String> = emptySet(),
    val newPlayerName: String = "",
    val settings: GameSettings = GameSettings.default(),
    val isLoading: Boolean = false,
    val error: String? = null,
    val createdGameSetId: String? = null,
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
    private val gameSetRepository: GameSetRepository,
    private val offlineGameRepository: OfflineGameRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(GameSetupUiState())
    val uiState: StateFlow<GameSetupUiState> = _uiState.asStateFlow()

    init {
        loadPlayers()
    }

    fun loadPlayers() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            
            // Start by loading local players first
            val localDbPlayers = try {
                offlineGameRepository.getAllPlayers().first()
            } catch (e: Exception) {
                emptyList()
            }

            when (val result = playerRepository.getPlayers()) {
                is ApiResult.Success -> {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        players = result.data.filter { !it.deleted },
                        localPlayers = localDbPlayers,
                        isOfflineMode = false
                    )
                    if (_uiState.value.gameName.isEmpty()) {
                        _uiState.value = _uiState.value.copy(gameName = fetchDefaultGameName(false))
                    }
                }
                is ApiResult.Error -> {
                    // Fallback to offline mode with local players
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        players = localDbPlayers,
                        isOfflineMode = true
                    )
                    if (_uiState.value.gameName.isEmpty()) {
                        _uiState.value = _uiState.value.copy(gameName = fetchDefaultGameName(true))
                    }
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

    fun addNewPlayer(name: String, photoUri: String?) {
        val trimmedName = name.trim()
        if (trimmedName.isBlank()) return
        val id = "local_${_uiState.value.nextLocalId}"
        val player = Player(id = id, name = trimmedName, photoUri = photoUri)
        _uiState.value = _uiState.value.copy(
            localPlayers = _uiState.value.localPlayers + player,
            selectedPlayerIds = _uiState.value.selectedPlayerIds + id,
            newPlayerName = "",
            nextLocalId = _uiState.value.nextLocalId - 1,
            showAddPlayer = false
        )
    }

    fun togglePlayerSelection(playerId: String) {
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
        if (state.isLoading) return  // guard against double-tap
        if (state.selectedPlayerIds.size < 2) {
            _uiState.value = state.copy(error = "Select at least 2 players")
            return
        }
        if (state.selectedPlayerIds.size > 6) {
            _uiState.value = state.copy(error = "Maximum 6 players")
            return
        }

        // Set loading immediately before launching coroutine to block re-entry
        _uiState.value = state.copy(isLoading = true, error = null)

        viewModelScope.launch {
            var finalGameName = state.gameName.trim()
            if (finalGameName.isEmpty()) {
                finalGameName = fetchDefaultGameName(state.isOfflineMode)
            }

            // 1. Resolve selected players inside the local Room database to get their local integer IDs
            val localList = try {
                offlineGameRepository.getAllPlayers().first()
            } catch (e: Exception) {
                emptyList()
            }

            val localPlayerIds = state.selectedPlayerIds.map { id ->
                val player = getSelectedPlayers().find { it.id == id } ?: return@map -1
                
                // If it is already a local DB player (integer ID string), use it directly
                val intId = id.toIntOrNull()
                if (intId != null) {
                    intId
                } else {
                    // Look up by name to avoid duplicates
                    val existing = localList.find { it.name.equals(player.name, ignoreCase = true) }
                    existing?.id?.toIntOrNull() ?: offlineGameRepository.createGuestPlayer(player.name)
                }
            }.filter { it != -1 }

            // 2. Save settings and game set in local SQLite database
            val localGameSetId = offlineGameRepository.createGameSet(state.settings, localPlayerIds)

            // 3. Update uiState with the local game set ID to navigate instantly
            _uiState.value = _uiState.value.copy(
                isLoading = false,
                createdGameSetId = localGameSetId.toString()
            )

            // 4. Background Sync: if online, try to create the settings and game set on API
            if (!state.isOfflineMode) {
                launch {
                    try {
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
                        val settingsResult = gameSettingsRepository.createGameSettings(settingsRequest)
                        if (settingsResult is ApiResult.Success) {
                            val gameSetRequest = CreateGameSetRequest(
                                name = finalGameName,
                                gameSettingsId = settingsResult.data.id,
                                playerIds = state.selectedPlayerIds.filter { !it.startsWith("local_") }.toList()
                            )
                            gameSetRepository.createGameSet(gameSetRequest)
                        }
                    } catch (e: Exception) {
                        // Silently handle API sync errors in the background
                    }
                }
            }
        }
    }

    fun clearError() {
        _uiState.value = _uiState.value.copy(error = null)
    }

    private suspend fun fetchDefaultGameName(isOfflineMode: Boolean): String {
        val currentDate = java.text.SimpleDateFormat("yyyy-MM-dd", java.util.Locale.getDefault()).format(java.util.Date())
        if (isOfflineMode) return currentDate
        return try {
            val setsResult = gameSetRepository.getGameSets()
            if (setsResult is ApiResult.Success) {
                val count = setsResult.data.count { it.name.startsWith(currentDate) }
                if (count == 0) currentDate else "$currentDate -$count"
            } else {
                currentDate
            }
        } catch (e: Exception) {
            currentDate
        }
    }
}
