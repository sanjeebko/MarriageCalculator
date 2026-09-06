package np.com.sanjeeb.marriagecalculator

import io.mockk.coEvery
import io.mockk.coVerify
import io.mockk.every
import io.mockk.mockk
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.test.runTest
import np.com.sanjeeb.marriagecalculator.data.local.GameSetEntity
import np.com.sanjeeb.marriagecalculator.data.local.PlayerEntity
import np.com.sanjeeb.marriagecalculator.data.local.RoundEntity
import np.com.sanjeeb.marriagecalculator.data.local.RoundScoreEntity
import np.com.sanjeeb.marriagecalculator.data.model.MarriageGameRound
import np.com.sanjeeb.marriagecalculator.data.network.NetworkMonitor
import np.com.sanjeeb.marriagecalculator.data.repository.ApiResult
import np.com.sanjeeb.marriagecalculator.data.repository.GameSetRepository
import np.com.sanjeeb.marriagecalculator.data.repository.OfflineGameRepository
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import np.com.sanjeeb.marriagecalculator.data.sync.SyncManager
import np.com.sanjeeb.marriagecalculator.data.sync.SyncStatus
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test

class SyncManagerTest {

    private val isOnlineFlow = MutableStateFlow(true)
    private val unsyncedCountFlow = MutableStateFlow(0)

    private val networkMonitor: NetworkMonitor = mockk {
        every { isOnline } returns isOnlineFlow
    }
    private val offlineGameRepository: OfflineGameRepository = mockk(relaxed = true) {
        every { this@mockk.unsyncedCountFlow } returns this@SyncManagerTest.unsyncedCountFlow
    }
    private val gameSetRepository: GameSetRepository = mockk(relaxed = true)
    private val sessionManager: SessionManager = mockk(relaxed = true) {
        every { isOnlineMode() } returns true
    }

    private lateinit var syncManager: SyncManager

    @Before
    fun setUp() {
        syncManager = SyncManager(
            networkMonitor = networkMonitor,
            offlineGameRepository = offlineGameRepository,
            gameSetRepository = gameSetRepository,
            sessionManager = sessionManager
        )
    }

    @Test
    fun `syncStatus is Synced when online and zero unsynced items`() = runTest {
        isOnlineFlow.value = true
        unsyncedCountFlow.value = 0

        val status = syncManager.syncStatus.first { it is SyncStatus.Synced }
        assertTrue(status.isSynced)
        assertFalse(status.isOffline)
    }

    @Test
    fun `syncStatus is Offline when internet is not available`() = runTest {
        isOnlineFlow.value = false
        unsyncedCountFlow.value = 2

        val status = syncManager.syncStatus.first { it is SyncStatus.Offline }
        assertTrue(status.isOffline)
        assertEquals(2, (status as SyncStatus.Offline).pendingCount)
    }

    @Test
    fun `syncStatus is PendingSync when online but has unsynced items`() = runTest {
        isOnlineFlow.value = true
        unsyncedCountFlow.value = 3

        val status = syncManager.syncStatus.first { it is SyncStatus.PendingSync }
        assertEquals(3, (status as SyncStatus.PendingSync).pendingCount)
    }

    @Test
    fun `syncPendingData uploads unsynced rounds and marks them synced`() = runTest {
        val unsyncedRound = RoundEntity(
            id = 10,
            gameSetId = 1,
            roundNumber = 1,
            winnerId = 101,
            dealerId = 102,
            totalMaal = 5,
            synced = false
        )
        val gameSet = GameSetEntity(id = 1, settingsId = 1, remoteId = "remote-set-123")
        val score = RoundScoreEntity(roundId = 10, playerId = 101, score = 15, maal = 5, isWinner = true)
        val winnerPlayer = PlayerEntity(id = 101, name = "Winner", remoteId = "remote-p101")
        val dealerPlayer = PlayerEntity(id = 102, name = "Dealer", remoteId = "remote-p102")

        coEvery { offlineGameRepository.getUnsyncedRounds() } returns listOf(unsyncedRound)
        coEvery { offlineGameRepository.getGameSet(1) } returns gameSet
        coEvery { offlineGameRepository.getRoundScores(10) } returns listOf(score)
        coEvery { offlineGameRepository.getPlayerEntity(101) } returns winnerPlayer
        coEvery { offlineGameRepository.getPlayerEntity(102) } returns dealerPlayer
        coEvery { gameSetRepository.submitRound(any(), any()) } returns ApiResult.Success(
            MarriageGameRound(id = "remote-round-999")
        )

        val result = syncManager.syncPendingData()
        assertTrue(result)
        coVerify { gameSetRepository.submitRound("remote-set-123", any()) }
        coVerify { offlineGameRepository.markRoundSynced(10, "remote-round-999") }
    }
}
