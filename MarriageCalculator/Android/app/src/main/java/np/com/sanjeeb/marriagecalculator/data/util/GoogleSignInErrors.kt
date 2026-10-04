package np.com.sanjeeb.marriagecalculator.data.util

/**
 * Turns Credential Manager / Google Identity failures into something a player can
 * act on.
 *
 * The SDK surfaces developer-facing text — "[16] Account reauth failed" being the
 * common one — which tells a user nothing. The usual real cause is that the
 * running build's signing certificate is not registered for Google Sign-In in
 * Firebase, and that is invisible from the message. See #139.
 */
object GoogleSignInErrors {

    /** Dismissing the account picker is not a failure and must not show an error. */
    fun isUserCancellation(exceptionClassName: String?, message: String?): Boolean {
        val name = exceptionClassName.orEmpty()
        if (name.contains("Cancellation", ignoreCase = true)) return true
        val text = message.orEmpty()
        return text.contains("cancel", ignoreCase = true) ||
            text.contains("User cancelled", ignoreCase = true)
    }

    /**
     * Maps a failure to a user-facing sentence. The original text is appended as a
     * short diagnostic so a bug report still carries the underlying cause.
     */
    fun toUserMessage(exceptionClassName: String?, message: String?): String {
        val name = exceptionClassName.orEmpty()
        val raw = message.orEmpty()

        val explanation = when {
            // "[16] Account reauth failed" — Google could not re-verify the account on
            // this device. Observed on an emulator after repeated sign-in attempts; the
            // device account itself needs re-authentication.
            raw.contains("reauth", ignoreCase = true) || raw.contains("[16]") ->
                "Google couldn't verify your account on this device. Re-add your Google account in Settings, or sign in with your email and password."

            // DEVELOPER_ERROR (status 10) is the signature/client-id mismatch: this build's
            // signing certificate is not registered for Google Sign-In. Play App Signing
            // re-signs with a different key than the upload key, so this can appear for
            // Store users while local builds work. See #139.
            raw.contains("[10]") ||
                raw.contains("DEVELOPER_ERROR", ignoreCase = true) ||
                name.contains("GetCredentialUnsupportedException") ->
                "Google Sign-In isn't set up for this build. Sign in with your email and password, or continue as a guest."

            name.contains("NoCredentialException") ||
                raw.contains("No credentials available", ignoreCase = true) ->
                "No Google account was found on this device. Add one in Settings, or sign in with your email and password."

            name.contains("GetCredentialInterruptedException") ||
                raw.contains("interrupted", ignoreCase = true) ->
                "Google Sign-In was interrupted. Please try again."

            raw.contains("network", ignoreCase = true) ||
                raw.contains("Unable to resolve host", ignoreCase = true) ->
                "Couldn't reach Google. Check your connection and try again."

            else ->
                "Google Sign-In couldn't complete. Sign in with your email and password, or continue as a guest."
        }

        val diagnostic = raw.take(80).trim()
        return if (diagnostic.isEmpty()) explanation else "$explanation ($diagnostic)"
    }
}
