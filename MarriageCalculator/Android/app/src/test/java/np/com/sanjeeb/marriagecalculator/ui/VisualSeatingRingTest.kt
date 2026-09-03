package np.com.sanjeeb.marriagecalculator.ui

import np.com.sanjeeb.marriagecalculator.ui.components.calculateSeatOffset
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test
import kotlin.math.abs

class VisualSeatingRingTest {

    @Test
    fun `calculateSeatOffset returns 0 for 0 players`() {
        val (x, y) = calculateSeatOffset(0, 0, 100f, 100f)
        assertEquals(0f, x, 0.001f)
        assertEquals(0f, y, 0.001f)
    }

    @Test
    fun `calculateSeatOffset first player is at top center`() {
        val (x, y) = calculateSeatOffset(0, 4, 100f, 80f)
        // At -90 degrees (-pi/2), cos is 0, sin is -1
        assertEquals(0f, x, 0.001f)
        assertEquals(-80f, y, 0.001f)
    }

    @Test
    fun `calculateSeatOffset 4 players progresses clockwise`() {
        val radiusX = 100f
        val radiusY = 60f

        // Player 0: Top (0, -radiusY)
        val (x0, y0) = calculateSeatOffset(0, 4, radiusX, radiusY)
        assertEquals(0f, x0, 0.01f)
        assertEquals(-60f, y0, 0.01f)

        // Player 1: Right (+radiusX, 0)
        val (x1, y1) = calculateSeatOffset(1, 4, radiusX, radiusY)
        assertEquals(100f, x1, 0.01f)
        assertEquals(0f, y1, 0.01f)

        // Player 2: Bottom (0, +radiusY)
        val (x2, y2) = calculateSeatOffset(2, 4, radiusX, radiusY)
        assertEquals(0f, x2, 0.01f)
        assertEquals(60f, y2, 0.01f)

        // Player 3: Left (-radiusX, 0)
        val (x3, y3) = calculateSeatOffset(3, 4, radiusX, radiusY)
        assertEquals(-100f, x3, 0.01f)
        assertEquals(0f, y3, 0.01f)
    }

    @Test
    fun `calculateSeatOffset 6 players all stay within bounds`() {
        val radiusX = 120f
        val radiusY = 75f

        for (i in 0 until 6) {
            val (x, y) = calculateSeatOffset(i, 6, radiusX, radiusY)
            assertTrue("x $x should be within [-$radiusX, $radiusX]", abs(x) <= radiusX + 0.01f)
            assertTrue("y $y should be within [-$radiusY, $radiusY]", abs(y) <= radiusY + 0.01f)
        }
    }
}
