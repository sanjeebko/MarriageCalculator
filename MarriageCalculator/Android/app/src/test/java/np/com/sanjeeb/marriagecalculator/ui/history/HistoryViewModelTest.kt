package np.com.sanjeeb.marriagecalculator.ui.history

import io.mockk.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.*
import np.com.sanjeeb.marriagecalculator.data.local.ActivityLogEntity
import np.com.sanjeeb.marriagecalculator.data.local.ActivityType
import np.com.sanjeeb.marriagecalculator.data.repository.ActivityLogRepository
import org.junit.After
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test
import java.text.SimpleDateFormat
import java.util.*

@OptIn(ExperimentalCoroutinesApi::class)
class HistoryViewModelTest {

    private val testDispatcher = StandardTestDispatcher()
    private val repository: ActivityLogRepository = mockk(relaxed = true)
    private lateinit var viewModel: HistoryViewModel

    @Before
    fun setUp() {
        Dispatchers.setMain(testDispatcher)
    }

    @After
    fun tearDown() {
        Dispatchers.resetMain()
    }

    @Test
    fun `loadInitial aggregates multiple logins on same day into LoginSummaryCard`() = runTest {
        val cal = Calendar.getInstance()
        val time1 = cal.timeInMillis
        cal.add(Calendar.HOUR, 1)
        val time2 = cal.timeInMillis
        cal.add(Calendar.HOUR, 1)
        val time3 = cal.timeInMillis

        val logs = listOf(
            ActivityLogEntity(id = 1, eventType = ActivityType.USER_LOGIN.name, timestamp = time3, title = "User Login", description = "Logged in", metadata = "user@test.com"),
            ActivityLogEntity(id = 2, eventType = ActivityType.APP_OPEN.name, timestamp = time2, title = "App Opened", description = "Session started", metadata = "user@test.com"),
            ActivityLogEntity(id = 3, eventType = ActivityType.USER_LOGIN.name, timestamp = time1, title = "User Login", description = "Logged in", metadata = "user@test.com"),
            ActivityLogEntity(id = 4, eventType = ActivityType.GAME_CREATED.name, timestamp = time1, title = "Game Created", description = "Saturday Night", metadata = "San;Aar")
        )

        coEvery { repository.getPagedLogs(limit = any(), offset = 0, types = null) } returns logs
        coEvery { repository.getLogCount() } returns 4

        viewModel = HistoryViewModel(repository)
        testScheduler.advanceUntilIdle()

        val state = viewModel.uiState.value
        assertEquals(4, state.totalCount)

        // Expect: DateSectionHeader, LoginSummaryCard (count = 3), ActivityCard (Game Created)
        assertEquals(3, state.items.size)
        assertTrue(state.items[0] is HistoryDisplayItem.DateSectionHeader)

        val loginCard = state.items[1] as HistoryDisplayItem.LoginSummaryCard
        assertEquals(3, loginCard.count)
        assertEquals("user@test.com", loginCard.primaryUser)
        assertFalse(loginCard.isExpanded)

        val gameCard = state.items[2] as HistoryDisplayItem.ActivityCard
        assertEquals("Game Created", gameCard.log.title)
    }

    @Test
    fun `toggleLoginExpanded expands and collapses sub-logs`() = runTest {
        val now = System.currentTimeMillis()
        val logs = listOf(
            ActivityLogEntity(id = 1, eventType = ActivityType.USER_LOGIN.name, timestamp = now, title = "User Login", description = "Logged in", metadata = "user@test.com"),
            ActivityLogEntity(id = 2, eventType = ActivityType.APP_OPEN.name, timestamp = now - 1000L, title = "App Opened", description = "Session started", metadata = "user@test.com")
        )

        coEvery { repository.getPagedLogs(limit = any(), offset = 0, types = null) } returns logs
        viewModel = HistoryViewModel(repository)
        testScheduler.advanceUntilIdle()

        val dateKey = SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).format(Date(now))

        // Before toggle: DateHeader, LoginSummaryCard (collapsed)
        assertEquals(2, viewModel.uiState.value.items.size)

        // Toggle expand
        viewModel.toggleLoginExpanded(dateKey)
        val expandedItems = viewModel.uiState.value.items
        // Expect: DateHeader, LoginSummaryCard (expanded), 2 ActivityCards
        assertEquals(4, expandedItems.size)
        assertTrue((expandedItems[1] as HistoryDisplayItem.LoginSummaryCard).isExpanded)

        // Toggle collapse
        viewModel.toggleLoginExpanded(dateKey)
        assertEquals(2, viewModel.uiState.value.items.size)
        assertFalse((viewModel.uiState.value.items[1] as HistoryDisplayItem.LoginSummaryCard).isExpanded)
    }

    @Test
    fun `setFilter reloads logs with specific types`() = runTest {
        coEvery { repository.getPagedLogs(any(), any(), any()) } returns emptyList()
        viewModel = HistoryViewModel(repository)
        testScheduler.advanceUntilIdle()

        viewModel.setFilter(HistoryFilter.PAYMENTS)
        testScheduler.advanceUntilIdle()

        assertEquals(HistoryFilter.PAYMENTS, viewModel.uiState.value.filter)
        coVerify {
            repository.getPagedLogs(
                limit = any(),
                offset = 0,
                types = listOf(ActivityType.PAYMENT_CLEARED.name, ActivityType.GAME_SETTLED.name)
            )
        }
    }

    @Test
    fun `jumpToDate finds matching date header and sets jumpToTargetIndex`() = runTest {
        val cal = Calendar.getInstance()
        val today = cal.timeInMillis
        cal.add(Calendar.DAY_OF_YEAR, -2)
        val twoDaysAgo = cal.timeInMillis

        val logs = listOf(
            ActivityLogEntity(id = 1, eventType = ActivityType.GAME_CREATED.name, timestamp = today, title = "Today Game", description = ""),
            ActivityLogEntity(id = 2, eventType = ActivityType.GAME_CREATED.name, timestamp = twoDaysAgo, title = "Old Game", description = "")
        )

        coEvery { repository.getPagedLogs(any(), any(), any()) } returns logs
        viewModel = HistoryViewModel(repository)
        testScheduler.advanceUntilIdle()

        // Jump to twoDaysAgo
        viewModel.jumpToDate(twoDaysAgo)

        val targetIndex = viewModel.uiState.value.jumpToTargetIndex
        assertNotNull(targetIndex)
        // Verify targetIndex points to a DateSectionHeader
        val targetItem = viewModel.uiState.value.items[targetIndex!!]
        assertTrue(targetItem is HistoryDisplayItem.DateSectionHeader)

        viewModel.onScrollHandled()
        assertNull(viewModel.uiState.value.jumpToTargetIndex)
    }
}
