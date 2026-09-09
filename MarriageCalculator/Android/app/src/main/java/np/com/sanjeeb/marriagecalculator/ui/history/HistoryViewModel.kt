package np.com.sanjeeb.marriagecalculator.ui.history

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import np.com.sanjeeb.marriagecalculator.data.local.ActivityLogEntity
import np.com.sanjeeb.marriagecalculator.data.local.ActivityType
import np.com.sanjeeb.marriagecalculator.data.repository.ActivityLogRepository
import java.text.SimpleDateFormat
import java.util.Calendar
import java.util.Date
import java.util.Locale
import javax.inject.Inject

enum class HistoryFilter(val label: String) {
    ALL("All"),
    GAMES("Games"),
    PAYMENTS("Payments"),
    LOGINS("Logins")
}

sealed interface HistoryDisplayItem {
    data class DateSectionHeader(
        val dateKey: String,
        val displayDate: String
    ) : HistoryDisplayItem

    data class LoginSummaryCard(
        val dateKey: String,
        val count: Int,
        val timeSpan: String,
        val primaryUser: String,
        val isExpanded: Boolean = false,
        val logs: List<ActivityLogEntity> = emptyList()
    ) : HistoryDisplayItem

    data class ActivityCard(
        val log: ActivityLogEntity
    ) : HistoryDisplayItem
}

data class HistoryUiState(
    val items: List<HistoryDisplayItem> = emptyList(),
    val filter: HistoryFilter = HistoryFilter.ALL,
    val isLoading: Boolean = false,
    val isPaging: Boolean = false,
    val hasMore: Boolean = true,
    val totalCount: Int = 0,
    val jumpToTargetIndex: Int? = null
)

@HiltViewModel
class HistoryViewModel @Inject constructor(
    private val activityLogRepository: ActivityLogRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(HistoryUiState())
    val uiState: StateFlow<HistoryUiState> = _uiState.asStateFlow()

    private val rawLogs = mutableListOf<ActivityLogEntity>()
    private var currentOffset = 0
    private val pageSize = 40
    private val expandedLoginDates = mutableSetOf<String>()

    private val dateFormatKey = SimpleDateFormat("yyyy-MM-dd", Locale.getDefault())
    private val timeFormat = SimpleDateFormat("hh:mm a", Locale.getDefault())

    init {
        loadInitial()
    }

    fun setFilter(filter: HistoryFilter) {
        if (_uiState.value.filter == filter) return
        _uiState.value = _uiState.value.copy(filter = filter)
        loadInitial()
    }

    fun refresh() {
        loadInitial()
    }

    private fun getFilterTypes(filter: HistoryFilter): List<String>? {
        return when (filter) {
            HistoryFilter.ALL -> null
            HistoryFilter.GAMES -> listOf(
                ActivityType.GAME_CREATED.name,
                ActivityType.GAME_PLAYED.name,
                ActivityType.ROUND_COMPLETED.name
            )
            HistoryFilter.PAYMENTS -> listOf(
                ActivityType.PAYMENT_CLEARED.name,
                ActivityType.GAME_SETTLED.name
            )
            HistoryFilter.LOGINS -> listOf(
                ActivityType.APP_OPEN.name,
                ActivityType.USER_LOGIN.name
            )
        }
    }

    fun loadInitial() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            currentOffset = 0
            rawLogs.clear()

            // Run backfill once in case historical records exist in database
            activityLogRepository.backfillFromHistory()

            val types = getFilterTypes(_uiState.value.filter)
            val logs = activityLogRepository.getPagedLogs(limit = pageSize, offset = 0, types = types)
            rawLogs.addAll(logs)
            currentOffset = logs.size

            val count = activityLogRepository.getLogCount()
            val processed = processLogsIntoDisplayItems(rawLogs)

            _uiState.value = _uiState.value.copy(
                items = processed,
                isLoading = false,
                hasMore = logs.size >= pageSize,
                totalCount = count
            )
        }
    }

    fun loadMore() {
        val state = _uiState.value
        if (state.isLoading || state.isPaging || !state.hasMore) return

        viewModelScope.launch {
            _uiState.value = state.copy(isPaging = true)
            val types = getFilterTypes(state.filter)
            val nextLogs = activityLogRepository.getPagedLogs(limit = pageSize, offset = currentOffset, types = types)

            if (nextLogs.isNotEmpty()) {
                rawLogs.addAll(nextLogs)
                currentOffset += nextLogs.size
                val processed = processLogsIntoDisplayItems(rawLogs)
                _uiState.value = _uiState.value.copy(
                    items = processed,
                    isPaging = false,
                    hasMore = nextLogs.size >= pageSize
                )
            } else {
                _uiState.value = _uiState.value.copy(isPaging = false, hasMore = false)
            }
        }
    }

    fun toggleLoginExpanded(dateKey: String) {
        if (expandedLoginDates.contains(dateKey)) {
            expandedLoginDates.remove(dateKey)
        } else {
            expandedLoginDates.add(dateKey)
        }
        _uiState.value = _uiState.value.copy(
            items = processLogsIntoDisplayItems(rawLogs)
        )
    }

    fun jumpToDate(dateMillis: Long) {
        val targetDateKey = dateFormatKey.format(Date(dateMillis))
        val items = _uiState.value.items
        var targetIndex = items.indexOfFirst {
            it is HistoryDisplayItem.DateSectionHeader && it.dateKey == targetDateKey
        }

        // If exact date header not found, jump to closest date prior to it
        if (targetIndex == -1) {
            targetIndex = items.indexOfFirst {
                it is HistoryDisplayItem.DateSectionHeader && it.dateKey <= targetDateKey
            }
        }

        if (targetIndex != -1) {
            _uiState.value = _uiState.value.copy(jumpToTargetIndex = targetIndex)
        }
    }

    fun onScrollHandled() {
        _uiState.value = _uiState.value.copy(jumpToTargetIndex = null)
    }

    private fun processLogsIntoDisplayItems(logs: List<ActivityLogEntity>): List<HistoryDisplayItem> {
        val result = mutableListOf<HistoryDisplayItem>()
        if (logs.isEmpty()) return result

        val groupedByDate = logs.groupBy { dateFormatKey.format(Date(it.timestamp)) }

        for ((dateKey, dayLogs) in groupedByDate) {
            // 1. Add Date Header
            val displayHeader = formatDisplayDate(dateKey, dayLogs.first().timestamp)
            result.add(HistoryDisplayItem.DateSectionHeader(dateKey, displayHeader))

            // 2. Separate login/app-open vs other activity
            val loginLogs = dayLogs.filter {
                it.eventType == ActivityType.APP_OPEN.name || it.eventType == ActivityType.USER_LOGIN.name
            }
            val gameLogs = dayLogs.filter {
                it.eventType != ActivityType.APP_OPEN.name && it.eventType != ActivityType.USER_LOGIN.name
            }

            // If logins exist on this day, aggregate into single card
            if (loginLogs.isNotEmpty()) {
                val isExpanded = expandedLoginDates.contains(dateKey)
                val count = loginLogs.size
                val earliest = loginLogs.minOf { it.timestamp }
                val latest = loginLogs.maxOf { it.timestamp }
                val timeSpan = if (earliest == latest) {
                    timeFormat.format(Date(earliest))
                } else {
                    "${timeFormat.format(Date(earliest))} - ${timeFormat.format(Date(latest))}"
                }
                val primaryUser = loginLogs.firstOrNull { !it.metadata.isNullOrBlank() }?.metadata ?: "User"

                result.add(
                    HistoryDisplayItem.LoginSummaryCard(
                        dateKey = dateKey,
                        count = count,
                        timeSpan = timeSpan,
                        primaryUser = primaryUser,
                        isExpanded = isExpanded,
                        logs = loginLogs
                    )
                )

                // If expanded, show the sub-logs
                if (isExpanded) {
                    loginLogs.forEach { log ->
                        result.add(HistoryDisplayItem.ActivityCard(log))
                    }
                }
            }

            // 3. Add game/payment cards
            gameLogs.forEach { log ->
                result.add(HistoryDisplayItem.ActivityCard(log))
            }
        }

        return result
    }

    private fun formatDisplayDate(dateKey: String, timestamp: Long): String {
        val todayCal = Calendar.getInstance()
        val todayKey = dateFormatKey.format(todayCal.time)

        todayCal.add(Calendar.DAY_OF_YEAR, -1)
        val yesterdayKey = dateFormatKey.format(todayCal.time)

        return when (dateKey) {
            todayKey -> "Today · ${SimpleDateFormat("d MMMM yyyy", Locale.getDefault()).format(Date(timestamp))}"
            yesterdayKey -> "Yesterday · ${SimpleDateFormat("d MMMM yyyy", Locale.getDefault()).format(Date(timestamp))}"
            else -> {
                val cal = Calendar.getInstance().apply { timeInMillis = timestamp }
                val day = cal.get(Calendar.DAY_OF_MONTH)
                val suffix = getDayOfMonthSuffix(day)
                val monthYear = SimpleDateFormat("MMMM yyyy", Locale.getDefault()).format(Date(timestamp))
                "$day$suffix $monthYear"
            }
        }
    }

    private fun getDayOfMonthSuffix(n: Int): String {
        if (n in 11..13) return "th"
        return when (n % 10) {
            1 -> "st"
            2 -> "nd"
            3 -> "rd"
            else -> "th"
        }
    }
}
