package np.com.sanjeeb.marriagecalculator.data.util

/**
 * Shared validation for user-entered values.
 *
 * Player names and usernames have their own rules in [UsernameValidator]; this
 * covers everything else the user can type. Two concerns drive it:
 *
 *  - **Never throw.** Parsing uses the `...OrNull` forms, so no input can raise
 *    NumberFormatException (see #138).
 *  - **Never accept nonsense.** Scores are summed as `Int`; an unbounded point
 *    value overflows silently and corrupts a scoreboard without ever crashing,
 *    which is harder to notice than a crash.
 */
object InputValidator {

    const val MAX_GAME_NAME_LENGTH = 40

    /** A game's per-hand point values. Real games use single or double digits. */
    val POINT_RANGE = 0..1_000

    /** Money per point. Upper bound keeps `points * rate` well inside Double precision. */
    val POINT_RATE_RANGE = 0.0..100_000.0

    /** Maal held by one player in one hand. The theoretical maximum is far below this. */
    val MAAL_RANGE = 0..9_999

    const val OTP_LENGTH = 6

    const val MAX_INVITE_CODE_LENGTH = 12

    private val emailRegex = Regex("^[A-Za-z0-9._%+\\-]+@[A-Za-z0-9.\\-]+\\.[A-Za-z]{2,}$")

    /**
     * Live sanitizer for the optional game name: strips control characters,
     * collapses runs of whitespace, and caps the length. Does not trim the
     * trailing space, so typing "Friday " then "night" still works.
     */
    fun sanitizeGameName(input: String): String {
        val cleaned = buildString {
            for (ch in input) {
                // Order matters: tab and newline are both whitespace and ISO control
                // characters, and should become a space rather than vanish.
                when {
                    ch.isWhitespace() -> append(' ')
                    !ch.isISOControl() -> append(ch)
                }
            }
        }
        return Regex(" {2,}").replace(cleaned.trimStart(), " ").take(MAX_GAME_NAME_LENGTH)
    }

    /**
     * Parses a point value, returning null when the text is not a number or
     * falls outside [POINT_RANGE]. Null means "reject this keystroke".
     */
    fun parsePoint(input: String): Int? = input.toIntOrNull()?.takeIf { it in POINT_RANGE }

    /** Parses a money-per-point rate, bounded by [POINT_RATE_RANGE]. */
    fun parsePointRate(input: String): Double? =
        input.toDoubleOrNull()?.takeIf { it.isFinite() && it in POINT_RATE_RANGE }

    /** Parses maal for one player, clamped into [MAAL_RANGE]; blank reads as 0. */
    fun parseMaal(input: String): Int {
        if (input.isBlank()) return 0
        val value = input.toIntOrNull() ?: return 0
        return value.coerceIn(MAAL_RANGE.first, MAAL_RANGE.last)
    }

    /** Keeps only digits and caps at [OTP_LENGTH]. */
    fun sanitizeOtp(input: String): String = input.filter { it.isDigit() }.take(OTP_LENGTH)

    fun isCompleteOtp(input: String): Boolean =
        input.length == OTP_LENGTH && input.all { it.isDigit() }

    /** Uppercase alphanumerics only, capped at [MAX_INVITE_CODE_LENGTH]. */
    fun sanitizeInviteCode(input: String): String =
        input.filter { it.isLetterOrDigit() }.uppercase().take(MAX_INVITE_CODE_LENGTH)

    /**
     * Structural email check only. The server remains the authority on whether an
     * address exists — by design it answers identically either way, so the client
     * must not imply otherwise (see the friend-privacy rules).
     */
    fun isValidEmail(input: String?): Boolean {
        val candidate = input?.trim().orEmpty()
        return candidate.length in 3..254 && emailRegex.matches(candidate)
    }
}
