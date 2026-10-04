package np.com.sanjeeb.marriagecalculator

import np.com.sanjeeb.marriagecalculator.data.util.GoogleSignInErrors
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class GoogleSignInErrorsTest {

    @Test
    fun `the observed reauth failure is reported as an account problem`() {
        // Exactly what the emulator produced once its Google account needed
        // re-authentication (#139). Status 16 is NOT a certificate problem.
        val message = GoogleSignInErrors.toUserMessage(
            "GetCredentialException",
            "[16] Account reauth failed."
        )
        assertTrue(message.contains("couldn't verify your account"))
        assertTrue(message.contains("email and password"))
        // The raw cause is kept so a bug report is still diagnosable.
        assertTrue(message.contains("[16]"))
    }

    @Test
    fun `a signing-certificate mismatch is reported as a build problem`() {
        // DEVELOPER_ERROR (status 10) is the real signature / client-id mismatch,
        // which is what Play App Signing can cause for Store users.
        val message = GoogleSignInErrors.toUserMessage(
            "GetCredentialException",
            "[10] DEVELOPER_ERROR"
        )
        assertTrue(message.contains("isn't set up for this build"))
        assertTrue(message.contains("email and password"))
    }

    @Test
    fun `reauth and developer error are not confused with each other`() {
        val reauth = GoogleSignInErrors.toUserMessage("E", "[16] Account reauth failed.")
        val devError = GoogleSignInErrors.toUserMessage("E", "[10] DEVELOPER_ERROR")
        assertFalse(reauth.contains("isn't set up for this build"))
        assertFalse(devError.contains("couldn't verify your account"))
    }

    @Test
    fun `a missing google account tells the user to add one`() {
        val message = GoogleSignInErrors.toUserMessage(
            "NoCredentialException",
            "No credentials available"
        )
        assertTrue(message.contains("No Google account was found"))
    }

    @Test
    fun `a network failure is reported as a network failure`() {
        val message = GoogleSignInErrors.toUserMessage(
            "GetCredentialException",
            "Unable to resolve host \"example.com\""
        )
        assertTrue(message.contains("Couldn't reach Google"))
    }

    @Test
    fun `an unrecognised failure still offers a usable way forward`() {
        val message = GoogleSignInErrors.toUserMessage("SomeNewException", "totally unexpected")
        assertTrue(message.contains("email and password"))
        assertTrue(message.contains("guest"))
    }

    @Test
    fun `an empty cause produces a clean message with no empty parentheses`() {
        val message = GoogleSignInErrors.toUserMessage("SomeException", null)
        assertFalse(message.contains("()"))
        assertTrue(message.isNotBlank())
    }

    @Test
    fun `dismissing the picker counts as cancellation, not failure`() {
        assertTrue(
            GoogleSignInErrors.isUserCancellation(
                "GetCredentialCancellationException",
                "User cancelled the selector"
            )
        )
        assertTrue(GoogleSignInErrors.isUserCancellation("SomeException", "activity was cancelled"))
    }

    @Test
    fun `a real failure is not mistaken for cancellation`() {
        assertFalse(GoogleSignInErrors.isUserCancellation("GetCredentialException", "[16] Account reauth failed."))
        assertFalse(GoogleSignInErrors.isUserCancellation("NoCredentialException", "No credentials available"))
        assertFalse(GoogleSignInErrors.isUserCancellation(null, null))
    }
}
