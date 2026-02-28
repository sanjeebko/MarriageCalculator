package com.sanjeeb.marriagecalculator.ui.dashboard

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.sanjeeb.marriagecalculator.data.model.MarriageGameSet
import com.sanjeeb.marriagecalculator.data.repository.ApiResult
import com.sanjeeb.marriagecalculator.data.repository.GameSetRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class DashboardUiState(
    val isLoading: Boolean = false,
    val activeGames: List<MarriageGameSet> = emptyList(),
    val error: String? = null,
    val isOfflineMode: Boolean = true // Default to offline until API is available
)

@HiltViewModel
class DashboardViewModel @Inject constructor(
    private val gameSetRepository: GameSetRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(DashboardUiState())
    val uiState: StateFlow<DashboardUiState> = _uiState.asStateFlow()

    init {
        loadActiveGames()
    }

    fun loadActiveGames() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            when (val result = gameSetRepository.getGameSets()) {
                is ApiResult.Success -> {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        activeGames = result.data.filter { it.isActive },
                        isOfflineMode = false
                    )
                }
                is ApiResult.Error -> {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        isOfflineMode = true,
                        error = null // Silently fall back to offline
                    )
                }
                is ApiResult.Loading -> {}
            }
        }
    }
}
