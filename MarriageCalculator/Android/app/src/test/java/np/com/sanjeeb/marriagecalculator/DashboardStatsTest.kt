package np.com.sanjeeb.marriagecalculator

import np.com.sanjeeb.marriagecalculator.data.model.Currency
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.data.model.UserCareerStats
import np.com.sanjeeb.marriagecalculator.ui.dashboard.EnrichedActiveGame
import org.junit.Assert.*
import org.junit.Test

class DashboardStatsTest {

    @Test
    fun careerStats_defaultValues_areZero() {
        val stats = UserCareerStats()
        assertEquals(0, stats.totalGames)
        assertEquals(0, stats.winRatePercent)
        assertEquals(0.0, stats.netProfitLoss, 0.001)
        assertEquals("₨0", stats.netProfitLossFormatted)
        assertTrue(stats.isZero)
        assertTrue(stats.isPositive)
        assertEquals(0, stats.highestMaal)
    }

    @Test
    fun careerStats_positiveProfit_formatsWithPlus() {
        val stats = UserCareerStats(
            totalGames = 10,
            winRatePercent = 60,
            netProfitLoss = 450.0,
            netProfitLossFormatted = "+₨450",
            isPositive = true,
            isZero = false,
            highestMaal = 25
        )
        assertEquals(10, stats.totalGames)
        assertEquals(60, stats.winRatePercent)
        assertEquals("+₨450", stats.netProfitLossFormatted)
        assertTrue(stats.isPositive)
        assertFalse(stats.isZero)
        assertEquals(25, stats.highestMaal)
    }

    @Test
    fun careerStats_negativeLoss_formatsCorrectly() {
        val stats = UserCareerStats(
            totalGames = 5,
            winRatePercent = 20,
            netProfitLoss = -120.0,
            netProfitLossFormatted = "-₨120",
            isPositive = false,
            isZero = false,
            highestMaal = 8
        )
        assertEquals("-₨120", stats.netProfitLossFormatted)
        assertFalse(stats.isPositive)
        assertFalse(stats.isZero)
    }

    @Test
    fun enrichedActiveGame_properties_populatedCorrectly() {
        val players = listOf(
            Player(id = "1", name = "Sanjeeb"),
            Player(id = "2", name = "Alex"),
            Player(id = "3", name = "Bob")
        )
        val game = EnrichedActiveGame(
            id = "101",
            name = "Saturday Night Match",
            lastPlayed = "2026-09-04",
            players = players,
            leaderName = "Sanjeeb",
            leaderScoreText = "+₨240",
            roundStatusText = "Round 2 · Game 3",
            totalGamesPlayed = 5,
            isSettled = false,
            cardSuit = "♠"
        )

        assertEquals("101", game.id)
        assertEquals("Saturday Night Match", game.name)
        assertEquals(3, game.players.size)
        assertEquals("Sanjeeb", game.leaderName)
        assertEquals("+₨240", game.leaderScoreText)
        assertEquals("Round 2 · Game 3", game.roundStatusText)
        assertEquals(5, game.totalGamesPlayed)
        assertFalse(game.isSettled)
        assertEquals("♠", game.cardSuit)
    }

    @Test
    fun enrichedActiveGame_suitCyclesDeterministically() {
        val suits = listOf("♠", "♥", "♦", "♣")
        val ids = listOf(0, 1, 2, 3, 4, 5)
        val mappedSuits = ids.map { suits[it % suits.size] }

        assertEquals("♠", mappedSuits[0])
        assertEquals("♥", mappedSuits[1])
        assertEquals("♦", mappedSuits[2])
        assertEquals("♣", mappedSuits[3])
        assertEquals("♠", mappedSuits[4])
        assertEquals("♥", mappedSuits[5])
    }
}
