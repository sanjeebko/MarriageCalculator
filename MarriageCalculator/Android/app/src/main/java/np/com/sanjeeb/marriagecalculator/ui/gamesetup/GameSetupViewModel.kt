package np.com.sanjeeb.marriagecalculator.ui.gamesetup

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import np.com.sanjeeb.marriagecalculator.data.model.*
import np.com.sanjeeb.marriagecalculator.data.repository.*
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
    val nextLocalId: Int = -1,
    val currentUser: User? = null
)

@HiltViewModel
class GameSetupViewModel @Inject constructor(
    private val playerRepository: PlayerRepository,
    private val gameSettingsRepository: GameSettingsRepository,
    private val gameSetRepository: GameSetRepository,
    private val offlineGameRepository: OfflineGameRepository,
    private val sessionManager: SessionManager
) : ViewModel() {

    private val _uiState = MutableStateFlow(GameSetupUiState())
    val uiState: StateFlow<GameSetupUiState> = _uiState.asStateFlow()

    init {
        _uiState.value = _uiState.value.copy(currentUser = sessionManager.getUserProfile())
        loadPlayers()
    }

    fun loadPlayers() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            
            // Start by loading local players first
            var localDbPlayers = try {
                offlineGameRepository.getAllPlayers().first()
            } catch (e: Exception) {
                emptyList()
            }

            val currentUser = sessionManager.getUserProfile()
            if (currentUser != null && currentUser.email.isNotEmpty()) {
                val hasLocalMe = localDbPlayers.any { it.email.equals(currentUser.email, ignoreCase = true) }
                if (!hasLocalMe) {
                    try {
                        offlineGameRepository.createRegisteredPlayer(
                            name = currentUser.displayName,
                            email = currentUser.email,
                            photoUri = currentUser.photoUrl
                        )
                        localDbPlayers = offlineGameRepository.getAllPlayers().first()
                    } catch (e: Exception) {
                        // Ignore local save errors
                    }
                }
            }

            val isOnline = sessionManager.isOnlineMode()
            if (isOnline) {
                when (val result = playerRepository.getPlayers()) {
                    is ApiResult.Success -> {
                        val remotePlayersList = result.data.filter { !it.deleted }.toMutableList()
                        val hasRemoteMe = remotePlayersList.any { it.email.equals(currentUser?.email, ignoreCase = true) }
                        
                        if (currentUser != null && currentUser.email.isNotEmpty() && !hasRemoteMe) {
                            try {
                                val createReq = CreatePlayerRequest(
                                    name = currentUser.displayName,
                                    email = currentUser.email,
                                    photoUri = currentUser.photoUrl
                                )
                                when (val createResult = playerRepository.createPlayer(createReq)) {
                                    is ApiResult.Success -> {
                                        val newRemotePlayer = createResult.data
                                        remotePlayersList.add(newRemotePlayer)
                                        // Link local player entity to remote ID
                                        val localMe = localDbPlayers.find { it.email.equals(currentUser.email, ignoreCase = true) }
                                        localMe?.id?.toIntOrNull()?.let { localId ->
                                            offlineGameRepository.updatePlayerRemoteId(localId, newRemotePlayer.id)
                                        }
                                        localDbPlayers = offlineGameRepository.getAllPlayers().first()
                                    }
                                    else -> {}
                                }
                            } catch (e: Exception) {
                                // Ignore remote player creation errors
                            }
                        }

                        _uiState.value = _uiState.value.copy(
                            isLoading = false,
                            players = remotePlayersList,
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
            } else {
                _uiState.value = _uiState.value.copy(
                    isLoading = false,
                    players = localDbPlayers,
                    isOfflineMode = true
                )
                if (_uiState.value.gameName.isEmpty()) {
                    _uiState.value = _uiState.value.copy(gameName = fetchDefaultGameName(true))
                }
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
        val playerToToggle = allPlayers.find { it.id == playerId } ?: return

        val newSelection = if (current.contains(playerId)) {
            current - playerId
        } else {
            if (current.size >= 6) {
                _uiState.value = _uiState.value.copy(error = "Maximum 6 players allowed")
                return
            }
            val withoutDuplicates = current.filter { id ->
                val p = allPlayers.find { it.id == id }
                if (p == null) false
                else if (playerToToggle.email.isNotEmpty() && p.email.isNotEmpty()) {
                    !p.email.equals(playerToToggle.email, ignoreCase = true)
                } else {
                    !p.name.equals(playerToToggle.name, ignoreCase = true)
                }
            }.toSet()
            withoutDuplicates + playerId
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
        val rawList = _uiState.value.players + _uiState.value.localPlayers
        val uniquePlayers = mutableListOf<Player>()
        for (player in rawList) {
            if (uniquePlayers.any { it.id == player.id }) {
                continue
            }
            if (player.email.isNotEmpty()) {
                val existingIndex = uniquePlayers.indexOfFirst {
                    it.email.isNotEmpty() && it.email.equals(player.email, ignoreCase = true)
                }
                if (existingIndex != -1) {
                    val existing = uniquePlayers[existingIndex]
                    val existingIsLocal = existing.id.toIntOrNull() != null || existing.id.startsWith("local_")
                    val currentIsLocal = player.id.toIntOrNull() != null || player.id.startsWith("local_")
                    if (existingIsLocal && !currentIsLocal) {
                        uniquePlayers[existingIndex] = player
                    }
                    continue
                }
            }
            if (player.email.isEmpty()) {
                val existingIndex = uniquePlayers.indexOfFirst {
                    it.name.equals(player.name, ignoreCase = true)
                }
                if (existingIndex != -1) {
                    val existing = uniquePlayers[existingIndex]
                    val existingIsLocal = existing.id.toIntOrNull() != null || existing.id.startsWith("local_")
                    val currentIsLocal = player.id.toIntOrNull() != null || player.id.startsWith("local_")
                    if (existingIsLocal && !currentIsLocal) {
                        uniquePlayers[existingIndex] = player
                    }
                    continue
                }
            }
            uniquePlayers.add(player)
        }
        return uniquePlayers
    }

    fun getSelectedPlayers(): List<Player> {
        val allPlayers = getAllPlayers()
        return allPlayers.filter { _uiState.value.selectedPlayerIds.contains(it.id) }
    }

    fun createGame(orderedPlayerIds: List<String> = emptyList()) {
        val state = _uiState.value
        val finalOrder = orderedPlayerIds.ifEmpty { state.selectedPlayerIds.toList() }
        if (state.isLoading) return  // guard against double-tap
        if (finalOrder.size < 2) {
            _uiState.value = state.copy(error = "Select at least 2 players")
            return
        }
        if (finalOrder.size > 6) {
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

            val localList = try {
                offlineGameRepository.getAllPlayers().first()
            } catch (e: Exception) {
                emptyList()
            }

            val isOnline = !state.isOfflineMode && sessionManager.isOnlineMode()

            if (isOnline) {
                try {
                    val remotePlayerIds = mutableListOf<String>()

                    for (playerId in finalOrder) {
                        val player = getSelectedPlayers().find { it.id == playerId }
                        if (player != null) {
                            if (playerId.startsWith("local_") || playerId.toIntOrNull() != null) {
                                val localPlayerEntity = if (playerId.startsWith("local_")) {
                                    offlineGameRepository.getPlayerEntityByName(player.name)
                                } else {
                                    val intId = playerId.toInt()
                                    offlineGameRepository.getPlayerEntity(intId)
                                }

                                val existingRemoteId = localPlayerEntity?.remoteId
                                if (!existingRemoteId.isNullOrEmpty()) {
                                    remotePlayerIds.add(existingRemoteId)
                                } else {
                                    val createPlayerReq = CreatePlayerRequest(
                                        name = player.name,
                                        email = player.email,
                                        photoUri = player.photoUri
                                    )
                                    when (val pResult = playerRepository.createPlayer(createPlayerReq)) {
                                        is ApiResult.Success -> {
                                            val remoteId = pResult.data.id
                                            remotePlayerIds.add(remoteId)
                                            localPlayerEntity?.let { entity ->
                                                offlineGameRepository.updatePlayerRemoteId(entity.id, remoteId)
                                            }
                                        }
                                        else -> {
                                            throw Exception("Failed to create remote player for ${player.name}")
                                        }
                                    }
                                }
                            } else {
                                remotePlayerIds.add(playerId)
                            }
                        }
                    }

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
                            playerIds = remotePlayerIds
                        )

                        when (val gameSetResult = gameSetRepository.createGameSet(gameSetRequest)) {
                            is ApiResult.Success -> {
                                val remoteGameSet = gameSetResult.data
                                val localPlayerIds = finalOrder.map { id ->
                                    val player = getSelectedPlayers().find { it.id == id } ?: return@map -1
                                    val intId = id.toIntOrNull()
                                    if (intId != null) {
                                        intId
                                    } else {
                                        val existing = localList.find { it.name.equals(player.name, ignoreCase = true) }
                                        existing?.id?.toIntOrNull() ?: offlineGameRepository.createGuestPlayer(player.name)
                                    }
                                }.filter { it != -1 }

                                val localSettingsId = offlineGameRepository.createGameSettingsWithRemoteId(state.settings, settingsResult.data.id)
                                val localGameSetId = offlineGameRepository.createGameSetWithRemoteId(
                                    name = finalGameName,
                                    settingsId = localSettingsId,
                                    playerIds = localPlayerIds,
                                    remoteId = remoteGameSet.id
                                )

                                _uiState.value = _uiState.value.copy(
                                    isLoading = false,
                                    createdGameSetId = remoteGameSet.id
                                )
                                return@launch
                            }
                            else -> throw Exception("Failed to create remote game set")
                        }
                    } else throw Exception("Failed to create remote settings")

                } catch (e: Exception) {
                    // Fallback to local
                }
            }

            val localPlayerIds = finalOrder.map { id ->
                val player = getSelectedPlayers().find { it.id == id } ?: return@map -1
                val intId = id.toIntOrNull()
                if (intId != null) {
                    intId
                } else {
                    val existing = localList.find { it.name.equals(player.name, ignoreCase = true) }
                    existing?.id?.toIntOrNull() ?: offlineGameRepository.createGuestPlayer(player.name)
                }
            }.filter { it != -1 }

            val localGameSetId = offlineGameRepository.createGameSet(
                name = finalGameName,
                settings = state.settings,
                playerIds = localPlayerIds
            )

            _uiState.value = _uiState.value.copy(
                isLoading = false,
                createdGameSetId = localGameSetId.toString()
            )
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
