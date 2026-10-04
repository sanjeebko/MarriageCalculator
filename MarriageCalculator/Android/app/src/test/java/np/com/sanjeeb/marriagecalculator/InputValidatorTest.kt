package np.com.sanjeeb.marriagecalculator

import np.com.sanjeeb.marriagecalculator.data.util.InputValidator
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test

class InputValidatorTest {

    // --- the #138 regression: an ObjectId must never reach a throwing parse ---

    @Test
    fun `mongo object id is rejected by every numeric parser rather than throwing`() {
        val objectId = "68d2f1a4c3b2e19f4a7c1234"
        assertNull(InputValidator.parsePoint(objectId))
        assertNull(InputValidator.parsePointRate(objectId))
        assertEquals(0, InputValidator.parseMaal(objectId))
        assertNull(objectId.toIntOrNull())
    }

    // --- points ---

    @Test
    fun `points accept the ordinary range and reject out-of-range values`() {
        assertEquals(3, InputValidator.parsePoint("3"))
        assertEquals(0, InputValidator.parsePoint("0"))
        assertEquals(1000, InputValidator.parsePoint("1000"))
        assertNull(InputValidator.parsePoint("1001"))
        assertNull(InputValidator.parsePoint("-1"))
    }

    @Test
    fun `points reject values that would overflow an Int sum`() {
        assertNull(InputValidator.parsePoint("2147483647"))
        assertNull(InputValidator.parsePoint("99999999999999"))
    }

    @Test
    fun `points reject non-numeric and empty input`() {
        assertNull(InputValidator.parsePoint(""))
        assertNull(InputValidator.parsePoint(" "))
        assertNull(InputValidator.parsePoint("3.5"))
        assertNull(InputValidator.parsePoint("abc"))
    }

    // --- point rate ---

    @Test
    fun `point rate accepts decimals within range`() {
        assertEquals(0.5, InputValidator.parsePointRate("0.5")!!, 0.0001)
        assertEquals(100000.0, InputValidator.parsePointRate("100000")!!, 0.0001)
    }

    @Test
    fun `point rate rejects negatives, overflow and non-finite values`() {
        assertNull(InputValidator.parsePointRate("-0.5"))
        assertNull(InputValidator.parsePointRate("100000.01"))
        assertNull(InputValidator.parsePointRate("NaN"))
        assertNull(InputValidator.parsePointRate("Infinity"))
    }

    // --- maal ---

    @Test
    fun `maal clamps rather than rejecting so the field stays usable`() {
        assertEquals(0, InputValidator.parseMaal(""))
        assertEquals(15, InputValidator.parseMaal("15"))
        assertEquals(9999, InputValidator.parseMaal("100000"))
        assertEquals(0, InputValidator.parseMaal("-5"))
        assertEquals(0, InputValidator.parseMaal("not a number"))
    }

    // --- game name ---

    @Test
    fun `game name is capped and stripped of control characters`() {
        assertEquals(
            InputValidator.MAX_GAME_NAME_LENGTH,
            InputValidator.sanitizeGameName("x".repeat(200)).length
        )
        assertEquals("ab", InputValidator.sanitizeGameName("a\u0000b"))
        assertEquals("a b", InputValidator.sanitizeGameName("a\tb"))
        assertEquals("a b", InputValidator.sanitizeGameName("a    b"))
        assertEquals("ab", InputValidator.sanitizeGameName("   ab"))
    }

    @Test
    fun `game name keeps a trailing space so typing the next word still works`() {
        assertEquals("Friday ", InputValidator.sanitizeGameName("Friday "))
    }

    // --- otp ---

    @Test
    fun `otp keeps digits only and caps at six`() {
        assertEquals("123456", InputValidator.sanitizeOtp("123456"))
        assertEquals("123456", InputValidator.sanitizeOtp("12a3b4c5d6e789"))
        assertEquals("", InputValidator.sanitizeOtp("abcdef"))
        assertTrue(InputValidator.isCompleteOtp("123456"))
        assertFalse(InputValidator.isCompleteOtp("12345"))
    }

    // --- invite code ---

    @Test
    fun `invite code is uppercased, stripped and capped`() {
        assertEquals("ABC123", InputValidator.sanitizeInviteCode("abc-123"))
        assertEquals("ABC123", InputValidator.sanitizeInviteCode(" abc 123 "))
        assertEquals(
            InputValidator.MAX_INVITE_CODE_LENGTH,
            InputValidator.sanitizeInviteCode("a".repeat(50)).length
        )
    }

    // --- email ---

    @Test
    fun `email accepts ordinary addresses`() {
        assertTrue(InputValidator.isValidEmail("someone@example.com"))
        assertTrue(InputValidator.isValidEmail("first.last+tag@sub.example.co.uk"))
        assertTrue(InputValidator.isValidEmail("  spaced@example.com  "))
    }

    @Test
    fun `email rejects malformed addresses`() {
        assertFalse(InputValidator.isValidEmail(null))
        assertFalse(InputValidator.isValidEmail(""))
        assertFalse(InputValidator.isValidEmail("no-at-sign"))
        assertFalse(InputValidator.isValidEmail("@example.com"))
        assertFalse(InputValidator.isValidEmail("someone@"))
        assertFalse(InputValidator.isValidEmail("someone@example"))
        assertFalse(InputValidator.isValidEmail("a@b.c"))
        assertFalse(InputValidator.isValidEmail("two @spaces.com"))
    }
}
