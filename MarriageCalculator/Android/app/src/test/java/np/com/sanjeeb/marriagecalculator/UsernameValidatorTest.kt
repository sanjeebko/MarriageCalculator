package np.com.sanjeeb.marriagecalculator

import np.com.sanjeeb.marriagecalculator.data.util.UsernameValidator
import org.junit.Assert.*
import org.junit.Test

class UsernameValidatorTest {

    @Test
    fun normalize_trimsAndCollapsesConsecutiveSpaces() {
        assertEquals("John", UsernameValidator.normalize("  John  "))
        assertEquals("John Doe", UsernameValidator.normalize("John   Doe"))
        assertEquals("John B Doe", UsernameValidator.normalize("  John    B   Doe  "))
        assertEquals("", UsernameValidator.normalize("   "))
        assertEquals("", UsernameValidator.normalize(null))
    }

    @Test
    fun validate_validUsernames() {
        assertTrue(UsernameValidator.validate("John").isValid)
        assertTrue(UsernameValidator.validate("user_1").isValid)
        assertTrue(UsernameValidator.validate("player-10").isValid)
        assertTrue(UsernameValidator.validate("12345").isValid)
        assertTrue(UsernameValidator.validate("John Doe").isValid)
        assertTrue(UsernameValidator.validate("John B Doe").isValid)
        assertTrue(UsernameValidator.validate("A B C").isValid)
        assertTrue(UsernameValidator.validate("123456789012345").isValid) // 15 chars
        assertTrue(UsernameValidator.validate("A 1234567890123").isValid) // 15 chars
    }

    @Test
    fun validate_invalidUsernames() {
        assertFalse(UsernameValidator.validate("").isValid)
        assertFalse(UsernameValidator.validate(null).isValid)
        assertFalse(UsernameValidator.validate(" John").isValid)
        assertFalse(UsernameValidator.validate("John ").isValid)
        assertFalse(UsernameValidator.validate("1234567890123456").isValid) // 16 chars
        assertFalse(UsernameValidator.validate("A B C D").isValid) // 3 spaces
        assertFalse(UsernameValidator.validate("A  B").isValid) // 2 consecutive spaces
        assertFalse(UsernameValidator.validate("User@1").isValid) // invalid char
        assertFalse(UsernameValidator.validate("User#1").isValid)
        assertFalse(UsernameValidator.validate("User.Name").isValid)
    }

    @Test
    fun sanitizeInput_liveTypingSanitizer() {
        // Disallows non-allowed chars
        assertEquals("John", UsernameValidator.sanitizeInput("John@!#"))
        // Strips leading space
        assertEquals("John", UsernameValidator.sanitizeInput("   John"))
        // Collapses consecutive spaces
        assertEquals("John ", UsernameValidator.sanitizeInput("John  "))
        assertEquals("John Doe", UsernameValidator.sanitizeInput("John   Doe"))
        // Caps at 15 chars
        assertEquals("123456789012345", UsernameValidator.sanitizeInput("12345678901234567890"))
        // Blocks 3rd space
        assertEquals("A B CD", UsernameValidator.sanitizeInput("A B C D"))
    }
}
