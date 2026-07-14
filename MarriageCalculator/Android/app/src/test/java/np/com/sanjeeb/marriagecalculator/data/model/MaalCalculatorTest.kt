package np.com.sanjeeb.marriagecalculator.data.model

import org.junit.Assert.*
import org.junit.Test

class MaalCalculatorTest {

    @Test
    fun `empty counts give zero total`() {
        assertEquals(0, MaalCalculator.total(emptyMap()))
    }

    @Test
    fun `tiplu scores 3 then 8 and is capped at two`() {
        assertEquals(3, MaalCalculator.total(mapOf(MaalItem.TIPLU to 1)))
        assertEquals(8, MaalCalculator.total(mapOf(MaalItem.TIPLU to 2)))
        // Only 2 tiplu are available to players (the third is the maal card on the table)
        assertEquals(2, MaalItem.TIPLU.maxCount)
        assertEquals(8, MaalCalculator.total(mapOf(MaalItem.TIPLU to 5)))
    }

    @Test
    fun `poplu scores 2 5 10 and is capped at three`() {
        assertEquals(2, MaalCalculator.total(mapOf(MaalItem.POPLU to 1)))
        assertEquals(5, MaalCalculator.total(mapOf(MaalItem.POPLU to 2)))
        assertEquals(10, MaalCalculator.total(mapOf(MaalItem.POPLU to 3)))
        assertEquals(3, MaalItem.POPLU.maxCount)
    }

    @Test
    fun `jhiplu scores the same tiers as poplu`() {
        assertEquals(2, MaalCalculator.total(mapOf(MaalItem.JHIPLU to 1)))
        assertEquals(5, MaalCalculator.total(mapOf(MaalItem.JHIPLU to 2)))
        assertEquals(10, MaalCalculator.total(mapOf(MaalItem.JHIPLU to 3)))
        assertEquals(3, MaalItem.JHIPLU.maxCount)
    }

    @Test
    fun `marriage scores 10 then 25 and is capped at two`() {
        assertEquals(10, MaalCalculator.total(mapOf(MaalItem.MARRIAGE to 1)))
        assertEquals(25, MaalCalculator.total(mapOf(MaalItem.MARRIAGE to 2)))
        assertEquals(2, MaalItem.MARRIAGE.maxCount)
    }

    @Test
    fun `tunnela scores 5 15 30 45 and is capped at four`() {
        assertEquals(5, MaalCalculator.total(mapOf(MaalItem.TUNNELA to 1)))
        assertEquals(15, MaalCalculator.total(mapOf(MaalItem.TUNNELA to 2)))
        assertEquals(30, MaalCalculator.total(mapOf(MaalItem.TUNNELA to 3)))
        assertEquals(45, MaalCalculator.total(mapOf(MaalItem.TUNNELA to 4)))
        assertEquals(4, MaalItem.TUNNELA.maxCount)
    }

    @Test
    fun `poplu jhiplu tunnela scores 10 30 45 and is capped at three`() {
        assertEquals(10, MaalCalculator.total(mapOf(MaalItem.POPLU_JHIPLU_TUNNELA to 1)))
        assertEquals(30, MaalCalculator.total(mapOf(MaalItem.POPLU_JHIPLU_TUNNELA to 2)))
        assertEquals(45, MaalCalculator.total(mapOf(MaalItem.POPLU_JHIPLU_TUNNELA to 3)))
        assertEquals(3, MaalItem.POPLU_JHIPLU_TUNNELA.maxCount)
    }

    @Test
    fun `alter tunnela is a flat 35`() {
        assertEquals(35, MaalCalculator.total(mapOf(MaalItem.ALTER_TUNNELA to 1)))
        assertEquals(1, MaalItem.ALTER_TUNNELA.maxCount)
        assertEquals(35, MaalCalculator.total(mapOf(MaalItem.ALTER_TUNNELA to 3)))
    }

    @Test
    fun `joker tunnela is a flat 35 like alter tunnela`() {
        assertEquals(35, MaalCalculator.total(mapOf(MaalItem.JOKER_TUNNELA to 1)))
        assertEquals(1, MaalItem.JOKER_TUNNELA.maxCount)
    }

    @Test
    fun `alter scores 5 15 30 and is capped at three`() {
        assertEquals(5, MaalCalculator.total(mapOf(MaalItem.ALTER to 1)))
        assertEquals(15, MaalCalculator.total(mapOf(MaalItem.ALTER to 2)))
        assertEquals(30, MaalCalculator.total(mapOf(MaalItem.ALTER to 3)))
        assertEquals(3, MaalItem.ALTER.maxCount)
    }

    @Test
    fun `joker printed card uses the same tiers as alter`() {
        assertEquals(5, MaalCalculator.total(mapOf(MaalItem.JOKER to 1)))
        assertEquals(15, MaalCalculator.total(mapOf(MaalItem.JOKER to 2)))
        assertEquals(30, MaalCalculator.total(mapOf(MaalItem.JOKER to 3)))
        assertEquals(3, MaalItem.JOKER.maxCount)
    }

    @Test
    fun `mixed counts sum their tier values`() {
        // 2 Tiplu(8) + 2 Poplu(5) + 1 Marriage(10) + 2 Tunnela(15) = 38
        val counts = mapOf(
            MaalItem.TIPLU to 2,
            MaalItem.POPLU to 2,
            MaalItem.MARRIAGE to 1,
            MaalItem.TUNNELA to 2
        )
        assertEquals(38, MaalCalculator.total(counts))
    }

    @Test
    fun `total is clamped to max 99`() {
        val counts = mapOf(
            MaalItem.TUNNELA to 4,               // 45
            MaalItem.POPLU_JHIPLU_TUNNELA to 3,  // 45
            MaalItem.ALTER_TUNNELA to 1,         // 35 -> raw 125
        )
        assertEquals(MaalCalculator.MAX_TOTAL, MaalCalculator.total(counts))
    }

    @Test
    fun `negative counts are treated as zero`() {
        val counts = mapOf(MaalItem.TIPLU to -5, MaalItem.POPLU to 1)
        assertEquals(2, MaalCalculator.total(counts))
    }

    @Test
    fun `oversized counts clamp to the item max`() {
        // e.g. from stale persisted state
        assertEquals(30, MaalCalculator.total(mapOf(MaalItem.JOKER to 7)))
        assertEquals(45, MaalCalculator.total(mapOf(MaalItem.TUNNELA to 20)))
    }

    @Test
    fun `increment adds one and stops at the item max`() {
        var counts = emptyMap<MaalItem, Int>()
        counts = MaalCalculator.increment(counts, MaalItem.TIPLU)
        assertEquals(1, counts[MaalItem.TIPLU])
        counts = MaalCalculator.increment(counts, MaalItem.TIPLU)
        assertEquals(2, counts[MaalItem.TIPLU])
        // Third increment must not go past tiplu's max of 2
        counts = MaalCalculator.increment(counts, MaalItem.TIPLU)
        assertEquals(2, counts[MaalItem.TIPLU])

        counts = mapOf(MaalItem.ALTER_TUNNELA to 1)
        counts = MaalCalculator.increment(counts, MaalItem.ALTER_TUNNELA)
        assertEquals(1, counts[MaalItem.ALTER_TUNNELA])
    }

    @Test
    fun `decrement subtracts one and clamps at zero`() {
        var counts = mapOf(MaalItem.JOKER to 2)
        counts = MaalCalculator.decrement(counts, MaalItem.JOKER)
        assertEquals(1, counts[MaalItem.JOKER])

        counts = MaalCalculator.decrement(counts, MaalItem.JOKER)
        counts = MaalCalculator.decrement(counts, MaalItem.JOKER)
        assertEquals(0, counts[MaalItem.JOKER])
    }

    @Test
    fun `points at zero count is zero for every item`() {
        MaalItem.entries.forEach { item ->
            assertEquals(0, item.points(0))
        }
    }

    @Test
    fun `every item max count matches its tier table size`() {
        MaalItem.entries.forEach { item ->
            assertEquals(item.tiers.size, item.maxCount)
            assertEquals(item.tiers.last(), item.points(item.maxCount))
        }
    }
}
