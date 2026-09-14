package np.com.sanjeeb.marriagecalculator.data.util

data class UsernameValidationResult(
    val isValid: Boolean,
    val errorMessage: String? = null
)

object UsernameValidator {
    const val MAX_LENGTH = 15

    private val consecutiveSpacesRegex = Regex("\\s{2,}")
    private val allowedCharsRegex = Regex("^[a-zA-Z0-9_\\- ]+$")

    /**
     * Normalizes username by trimming leading and trailing spaces,
     * and automatically collapsing 2 or more spaces into a single space.
     */
    fun normalize(raw: String?): String {
        if (raw.isNullOrBlank()) return ""
        val trimmed = raw.trim()
        return consecutiveSpacesRegex.replace(trimmed, " ")
    }

    /**
     * Validates a username against the naming rules:
     * - Max length 15 characters (and at least 1 character).
     * - Only characters a-z, A-Z, 0-9, _, -, and space.
     * - Spaces only in the middle (neither leading nor trailing).
     * - At most 2 spaces total in the middle, and no consecutive spaces.
     */
    fun validate(username: String?): UsernameValidationResult {
        if (username.isNullOrEmpty()) {
            return UsernameValidationResult(false, "Username cannot be empty.")
        }

        if (username.startsWith(" ") || username.endsWith(" ")) {
            return UsernameValidationResult(false, "Username cannot start or end with spaces.")
        }

        if (username.length > MAX_LENGTH) {
            return UsernameValidationResult(false, "Username cannot exceed $MAX_LENGTH characters.")
        }

        if (!allowedCharsRegex.matches(username)) {
            return UsernameValidationResult(false, "Username can only contain letters, numbers, underscores, hyphens, and spaces.")
        }

        if (username.contains("  ")) {
            return UsernameValidationResult(false, "Username cannot contain consecutive spaces.")
        }

        val spaceCount = username.count { it == ' ' }
        if (spaceCount > 2) {
            return UsernameValidationResult(false, "Username can have at most 2 spaces in the middle.")
        }

        return UsernameValidationResult(true, null)
    }

    /**
     * Live input sanitizer for text fields:
     * - Filters out any character not in [a-zA-Z0-9_\- ].
     * - Strips leading spaces.
     * - Automatically collapses 2 or more spaces to 1 space.
     * - Disallows adding a 3rd space if 2 spaces are already present.
     * - Enforces maximum 15 characters.
     */
    fun sanitizeInput(input: String): String {
        val filtered = buildString {
            for (ch in input) {
                if (ch in 'a'..'z' || ch in 'A'..'Z' || ch in '0'..'9' || ch == '_' || ch == '-' || ch == ' ') {
                    append(ch)
                }
            }
        }

        var noLeading = if (filtered.startsWith(" ")) filtered.trimStart() else filtered
        var collapsed = consecutiveSpacesRegex.replace(noLeading, " ")

        if (collapsed.count { it == ' ' } > 2) {
            var spaceSeen = 0
            collapsed = buildString {
                for (ch in collapsed) {
                    if (ch == ' ') {
                        spaceSeen++
                        if (spaceSeen <= 2) append(ch)
                    } else {
                        append(ch)
                    }
                }
            }
        }

        return collapsed.take(MAX_LENGTH)
    }
}
