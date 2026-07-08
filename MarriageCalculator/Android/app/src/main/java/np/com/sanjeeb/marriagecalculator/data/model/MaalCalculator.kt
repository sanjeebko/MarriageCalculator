package np.com.sanjeeb.marriagecalculator.data.model

/**
 * Maal (bonus point) item types for the Marriage card game.
 * Default point values follow common Nepali house rules but are
 * adjustable in the calculator dialog since values vary by table.
 */
enum class MaalItem(val displayName: String, val defaultValue: Int) {
    TIPLU("Tiplu (Main Joker)", 3),
    POPLU("Poplu (Card Above)", 2),
    JHIPLU("Jhiplu (Card Below)", 2),
    ALTER("Alter (Same Rank, Alt Color)", 1),
    MARRIAGE("Marriage (Jhiplu+Tiplu+Poplu)", 10),
    TUNNEL("Tunnel (3 Identical Cards)", 5),
    MANUK("Manuk (Printed Joker)", 1);

    companion object {
        fun defaultValues(): Map<MaalItem, Int> = entries.associateWith { it.defaultValue }
    }
}

/**
 * Pure logic helper that turns per-item counts into a total Maal score.
 * Used by the Round Input screen's Maal calculator dialog.
 */
object MaalCalculator {

    const val MAX_COUNT = 20
    const val MAX_TOTAL = 99

    /**
     * Computes total maal from item counts and per-item values.
     * Missing items count as zero. Result is never negative.
     */
    fun total(
        counts: Map<MaalItem, Int>,
        values: Map<MaalItem, Int> = MaalItem.defaultValues()
    ): Int {
        val sum = MaalItem.entries.sumOf { item ->
            val count = (counts[item] ?: 0).coerceIn(0, MAX_COUNT)
            val value = (values[item] ?: item.defaultValue).coerceAtLeast(0)
            count * value
        }
        return sum.coerceIn(0, MAX_TOTAL)
    }

    /** Returns a new counts map with [item] incremented (clamped to [MAX_COUNT]). */
    fun increment(counts: Map<MaalItem, Int>, item: MaalItem): Map<MaalItem, Int> {
        val current = counts[item] ?: 0
        return counts + (item to (current + 1).coerceAtMost(MAX_COUNT))
    }

    /** Returns a new counts map with [item] decremented (clamped to 0). */
    fun decrement(counts: Map<MaalItem, Int>, item: MaalItem): Map<MaalItem, Int> {
        val current = counts[item] ?: 0
        return counts + (item to (current - 1).coerceAtLeast(0))
    }
}
