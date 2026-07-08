package np.com.sanjeeb.marriagecalculator.data.model

import org.junit.Assert.*
import org.junit.Test

/**
 * Tests for the local scoring algorithm that mirrors the C# ScoringEngine.
 * This validates the Android-side calculation used for instant preview.
 */
class LocalScoringTest {

    private fun calculateScores(
        settings: GameSettings,
        winnerId: Int,
        players: List<Triple<Int, Boolean, Int>> // (id, seen, maal)
    ): Map<Int, Int> {
        val maalValues = players.map { it.third }.toMutableList()
        val seenFlags = players.map { it.second || it.first == winnerId }.toMutableList()
        val winnerIdx = players.indexOfFirst { it.first == winnerId }

        if (settings.kidnap) {
            for (i in players.indices) {
                if (!seenFlags[i] && players[i].first != winnerId) {
                    maalValues[winnerIdx] += maalValues[i]
                    maalValues[i] = 0
                }
            }
        } else if (settings.murder) {
            for (i in players.indices) {
                if (!seenFlags[i] && players[i].first != winnerId) {
                    maalValues[i] = 0
                }
            }
        }

        val scores = MutableList(players.size) { 0 }

        // Fixed penalties
        for (i in players.indices) {
            if (players[i].first == winnerId) continue
            val penalty = if (!seenFlags[i]) settings.unseenPoint else settings.seenPoint
            scores[i] -= penalty
            scores[winnerIdx] += penalty
        }

        // Maal distribution
        val seenIndices = players.indices.filter { seenFlags[it] }
        val unseenIndices = players.indices.filter { !seenFlags[it] && players[it].first != winnerId }

        for (u in unseenIndices) {
            for (s in seenIndices) {
                val diff = maalValues[s] - maalValues[u]
                scores[s] += diff
                scores[u] -= diff
            }
        }

        for (i in seenIndices.indices) {
            for (j in i + 1 until seenIndices.size) {
                val a = seenIndices[i]
                val b = seenIndices[j]
                val diff = maalValues[a] - maalValues[b]
                scores[a] += diff
                scores[b] -= diff
            }
        }

        return players.mapIndexed { idx, p -> p.first to scores[idx] }.toMap()
    }

    @Test
    fun `murder mode - two players - zero sum`() {
        val settings = GameSettings(murder = true, kidnap = false, seenPoint = 3, unseenPoint = 10, pointRate = 10.0)
        val scores = calculateScores(
            settings, winnerId = 1,
            listOf(Triple(1, true, 15), Triple(2, true, 10))
        )
        assertEquals(0, scores.values.sum())
        assertEquals(8, scores[1]) // 3 (seen penalty) + 5 (maal diff)
        assertEquals(-8, scores[2])
    }

    @Test
    fun `murder mode - unseen maal zeroed - zero sum`() {
        val settings = GameSettings(murder = true, kidnap = false, seenPoint = 3, unseenPoint = 10, pointRate = 10.0)
        val scores = calculateScores(
            settings, winnerId = 1,
            listOf(Triple(1, true, 10), Triple(2, true, 5), Triple(3, false, 8))
        )
        assertEquals(0, scores.values.sum())
    }

    @Test
    fun `kidnap mode - winner steals maal - zero sum`() {
        val settings = GameSettings(murder = false, kidnap = true, seenPoint = 3, unseenPoint = 10, pointRate = 10.0)
        val scores = calculateScores(
            settings, winnerId = 1,
            listOf(Triple(1, true, 10), Triple(2, true, 5), Triple(3, false, 8))
        )
        assertEquals(0, scores.values.sum())
    }

    @Test
    fun `normal mode - unseen keeps maal - zero sum`() {
        val settings = GameSettings(murder = false, kidnap = false, seenPoint = 3, unseenPoint = 10, pointRate = 10.0)
        val scores = calculateScores(
            settings, winnerId = 1,
            listOf(Triple(1, true, 10), Triple(2, true, 5), Triple(3, false, 8))
        )
        assertEquals(0, scores.values.sum())
    }

    @Test
    fun `six players - all modes zero sum`() {
        val modes = listOf(
            GameSettings(murder = true, kidnap = false),
            GameSettings(murder = false, kidnap = true),
            GameSettings(murder = false, kidnap = false)
        )
        val players = listOf(
            Triple(1, true, 25), Triple(2, true, 15), Triple(3, true, 10),
            Triple(4, false, 8), Triple(5, false, 3), Triple(6, false, 0)
        )
        for (settings in modes) {
            val scores = calculateScores(settings, winnerId = 1, players)
            assertEquals("Failed for settings: $settings", 0, scores.values.sum())
        }
    }

    @Test
    fun `all unseen except winner - murder`() {
        val settings = GameSettings(murder = true, seenPoint = 3, unseenPoint = 10)
        val scores = calculateScores(
            settings, winnerId = 1,
            listOf(Triple(1, true, 20), Triple(2, false, 5), Triple(3, false, 3), Triple(4, false, 0))
        )
        assertEquals(0, scores.values.sum())
        assertEquals(90, scores[1]) // 10*3 penalties + 20*3 maal
    }
}
