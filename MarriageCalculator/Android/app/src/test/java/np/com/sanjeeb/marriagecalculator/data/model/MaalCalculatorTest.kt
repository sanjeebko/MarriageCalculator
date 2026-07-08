package np.com.sanjeeb.marriagecalculator.data.model

import org.junit.Assert.*
import org.junit.Test

class MaalCalculatorTest {

    @Test
    fun `empty counts give zero total`() {
        assertEquals(0, MaalCalculator.total(emptyMap()))
    }

    @Test
    fun `single tiplu uses default value`() {
        val counts = mapOf(MaalItem.TIPLU to 1)
        assertEquals(3, MaalCalculator.total(counts))
    }

    @Test
    fun `mixed counts sum with default values`() {
        // 1 Tiplu(3) + 2 Poplu(2*2=4) + 1 Marriage(10) + 3 Alter(3*1=3) = 20
        val counts = mapOf(
            MaalItem.TIPLU to 1,
            MaalItem.POPLU to 2,
            MaalItem.MARRIAGE to 1,
            MaalItem.ALTER to 3
        )
        assertEquals(20, MaalCalculator.total(counts))
    }

    @Test
    fun `custom values override defaults`() {
        val counts = mapOf(MaalItem.TIPLU to 2)
        val values = mapOf(MaalItem.TIPLU to 5)
        assertEquals(10, MaalCalculator.total(counts, values))
    }

    @Test
    fun `total is clamped to max 99`() {
        val counts = mapOf(MaalItem.MARRIAGE to 20) // 200 raw
        assertEquals(MaalCalculator.MAX_TOTAL, MaalCalculator.total(counts))
    }

    @Test
    fun `negative counts are treated as zero`() {
        val counts = mapOf(MaalItem.TIPLU to -5, MaalItem.POPLU to 1)
        assertEquals(2, MaalCalculator.total(counts))
    }

    @Test
    fun `negative values are treated as zero`() {
        val counts = mapOf(MaalItem.TIPLU to 3)
        val values = mapOf(MaalItem.TIPLU to -2)
        assertEquals(0, MaalCalculator.total(counts, values))
    }

    @Test
    fun `increment adds one and clamps at max`() {
        var counts = emptyMap<MaalItem, Int>()
        counts = MaalCalculator.increment(counts, MaalItem.TUNNEL)
        assertEquals(1, counts[MaalItem.TUNNEL])

        counts = mapOf(MaalItem.TUNNEL to MaalCalculator.MAX_COUNT)
        counts = MaalCalculator.increment(counts, MaalItem.TUNNEL)
        assertEquals(MaalCalculator.MAX_COUNT, counts[MaalItem.TUNNEL])
    }

    @Test
    fun `decrement subtracts one and clamps at zero`() {
        var counts = mapOf(MaalItem.MANUK to 2)
        counts = MaalCalculator.decrement(counts, MaalItem.MANUK)
        assertEquals(1, counts[MaalItem.MANUK])

        counts = MaalCalculator.decrement(counts, MaalItem.MANUK)
        counts = MaalCalculator.decrement(counts, MaalItem.MANUK)
        assertEquals(0, counts[MaalItem.MANUK])
    }

    @Test
    fun `default values map contains every item`() {
        val defaults = MaalItem.defaultValues()
        assertEquals(MaalItem.entries.size, defaults.size)
        assertEquals(3, defaults[MaalItem.TIPLU])
        assertEquals(2, defaults[MaalItem.POPLU])
        assertEquals(2, defaults[MaalItem.JHIPLU])
        assertEquals(1, defaults[MaalItem.ALTER])
        assertEquals(10, defaults[MaalItem.MARRIAGE])
        assertEquals(5, defaults[MaalItem.TUNNEL])
        assertEquals(1, defaults[MaalItem.MANUK])
    }
}
