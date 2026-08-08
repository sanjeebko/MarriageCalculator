package np.com.sanjeeb.marriagecalculator.data.model

/**
 * Maal (bonus point) item types for the Marriage card game (played with 3 decks).
 *
 * Point values are fixed rules, tiered by count rather than per-card multiples:
 * [tiers]`[n-1]` is the TOTAL points for holding n of the item (e.g. 2 tiplu = 8, not 6).
 * [maxCount] is the tier count, reflecting what a player can physically hold:
 * - Tiplu: 3 exist but one is always face-down on the table as the maal card, so max 2.
 * - Poplu / Jhiplu: 3 each in the deck.
 * - Marriage (jhiplu+tiplu+poplu set): limited to 2 by the 2 available tiplu.
 * - Tunnela (3 identical cards): capped at 4 — more is possible but vanishingly rare.
 * - Poplu/Jhiplu tunnela: max 3. A tiplu tunnela cannot exist (one tiplu is on the table).
 * - Alter / Joker tunnela: flat 35 each.
 */
enum class MaalItem(
    val displayName: String,
    val tiers: List<Int>
) {
    TIPLU("Tiplu (Main Joker)", listOf(3, 8)),
    POPLU("Poplu (Card Above)", listOf(2, 5, 10)),
    JHIPLU("Jhiplu (Card Below)", listOf(2, 5, 10)),
    ALTER("Alter (Same Rank, Alt Color)", listOf(5, 15, 30)),
    MARRIAGE("Marriage (Jhiplu+Tiplu+Poplu)", listOf(10, 25)),
    TUNNELA("Tunnela (3 Identical Cards)", listOf(5, 15, 30, 45)),
    POPLU_JHIPLU_TUNNELA("Poplu/Jhiplu Tunnela", listOf(10, 30, 45)),
    ALTER_TUNNELA("Alter Tunnela", listOf(35)),
    JOKER_TUNNELA("Joker Tunnela", listOf(35)),
    JOKER("Joker (Printed Card)", listOf(5, 15, 30));

    /** How many of this item a player can possibly hold. */
    val maxCount: Int get() = tiers.size

    /** Total points for holding [count] of this item (0 if none; count clamps to [maxCount]). */
    fun points(count: Int): Int {
        val clamped = count.coerceIn(0, maxCount)
        return if (clamped == 0) 0 else tiers[clamped - 1]
    }
}

/**
 * Pure logic helper that turns per-item counts into a total Maal score.
 * Used by the Round Input screen's Maal calculator dialog.
 */
object MaalCalculator {

    const val MAX_TOTAL = 99

    /**
     * Computes total maal from item counts. Missing items count as zero,
     * out-of-range counts are clamped, and the result stays in 0..[MAX_TOTAL].
     */
    fun total(counts: Map<MaalItem, Int>): Int =
        MaalItem.entries.sumOf { it.points(counts[it] ?: 0) }.coerceIn(0, MAX_TOTAL)

    /** Returns a new counts map with [item] incremented (clamped to the item's max). */
    fun increment(counts: Map<MaalItem, Int>, item: MaalItem): Map<MaalItem, Int> {
        val current = counts[item] ?: 0
        return counts + (item to (current + 1).coerceAtMost(item.maxCount))
    }

    /** Returns a new counts map with [item] decremented (clamped to 0). */
    fun decrement(counts: Map<MaalItem, Int>, item: MaalItem): Map<MaalItem, Int> {
        val current = counts[item] ?: 0
        return counts + (item to (current - 1).coerceAtLeast(0))
    }
}
