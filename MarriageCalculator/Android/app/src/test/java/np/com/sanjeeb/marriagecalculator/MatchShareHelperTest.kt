package np.com.sanjeeb.marriagecalculator

import np.com.sanjeeb.marriagecalculator.data.model.Currency
import np.com.sanjeeb.marriagecalculator.ui.share.MatchShareData
import np.com.sanjeeb.marriagecalculator.ui.share.MatchShareHelper
import np.com.sanjeeb.marriagecalculator.ui.share.PlayerShareEntry
import org.junit.Assert.*
import org.junit.Test

class MatchShareHelperTest {

    @Test
    fun computeSettlements_simpleThreePlayers() {
        val balances = listOf(
            "San" to 3.80,
            "Rem" to -0.80,
            "Aar" to -3.00
        )
        val settlements = MatchShareHelper.computeSettlements(balances)

        assertEquals(2, settlements.size)
        // Aar owes 3.00 and Rem owes 0.80, both pay San
        val aarTransfer = settlements.find { it.fromPlayer == "Aar" && it.toPlayer == "San" }
        val remTransfer = settlements.find { it.fromPlayer == "Rem" && it.toPlayer == "San" }

        assertNotNull(aarTransfer)
        assertNotNull(remTransfer)
        assertEquals(3.00, aarTransfer!!.amount, 0.001)
        assertEquals(0.80, remTransfer!!.amount, 0.001)
    }

    @Test
    fun computeSettlements_allBalanced_returnsEmpty() {
        val balances = listOf(
            "San" to 0.0,
            "Rem" to 0.0,
            "Aar" to 0.0
        )
        val settlements = MatchShareHelper.computeSettlements(balances)
        assertTrue(settlements.isEmpty())
    }

    @Test
    fun computeSettlements_fourPlayersMultipleDebtorsCreditors() {
        val balances = listOf(
            "Winner1" to 5.00,
            "Winner2" to 2.00,
            "Loser1" to -4.00,
            "Loser2" to -3.00
        )
        val settlements = MatchShareHelper.computeSettlements(balances)

        val totalTransferred = settlements.sumOf { it.amount }
        assertEquals(7.00, totalTransferred, 0.001)

        // All transfers must be from a loser to a winner
        settlements.forEach { transfer ->
            assertTrue(transfer.fromPlayer.startsWith("Loser"))
            assertTrue(transfer.toPlayer.startsWith("Winner"))
        }
    }

    @Test
    fun formatMatchSummaryText_includesHeaderAndStandings() {
        val data = MatchShareData(
            matchName = "Dashain Final",
            dateFormatted = "2026-10-15",
            roundsCount = 3,
            gamesCount = 12,
            currency = Currency.GBP_Pence,
            standings = listOf(
                PlayerShareEntry(name = "San", totalMaal = 20, totalScore = 40, totalMoney = 4.00, rank = 1),
                PlayerShareEntry(name = "Aar", totalMaal = 5, totalScore = -40, totalMoney = -4.00, rank = 2)
            ),
            settlements = listOf(
                np.com.sanjeeb.marriagecalculator.ui.share.SettlementTransfer("Aar", "San", 4.00)
            )
        )

        val text = MatchShareHelper.formatMatchSummaryText(data)

        assertTrue(text.contains("Marriage Calculator - Match Results"))
        assertTrue(text.contains("Dashain Final"))
        assertTrue(text.contains("3 Rounds"))
        assertTrue(text.contains("🥇 San"))
        assertTrue(text.contains("🥈 Aar"))
        assertTrue(text.contains("Aar ➔ San"))
        assertTrue(text.contains("Calculated with Marriage Calculator"))
    }
}
