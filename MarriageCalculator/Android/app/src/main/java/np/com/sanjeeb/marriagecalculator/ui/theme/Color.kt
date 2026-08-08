
package np.com.sanjeeb.marriagecalculator.ui.theme

import androidx.compose.ui.graphics.Color

val DeepRedTika = Color(0xFF660000)
val TiharNightBlue = Color(0xFF1A1A2E)
val GoldAccent = Color(0xFFD4AF37)
val MarigoldOrange = Color(0xFFFF8C00)

// Glassmorphism palette for the game page tables: cool frosted tones instead of gold.
val FrostWhite = Color(0xFFE9EEF6)   // primary text on glass surfaces
val FrostBlue = Color(0xFFA6BEE0)    // secondary labels, headers, accents
val FrostBorder = Color(0x33FFFFFF)  // subtle glass edge

/**
 * A complete app color theme. Screens read these via [AppTheme.palette] instead of hardcoded
 * colors, so switching the selected theme restyles the whole app instantly.
 *
 * [tint] is the glassmorphism base: overlays are built as tint.copy(alpha = ...) - white-ish on
 * dark themes, ink on light themes - so frosted pills/borders stay visible in both modes.
 */
data class AppPalette(
    val isDark: Boolean,
    val backgroundTop: Color,
    val backgroundBottom: Color,
    val surface: Color,       // top bars, cards, dialogs
    val accent: Color,        // branding accent (gold on the festive theme)
    val accentAlt: Color,     // secondary accent (marigold on the festive theme)
    val cta: Color,           // strong action color (deep red on the festive theme)
    val frostText: Color,     // primary text on glass tables
    val frostAccent: Color,   // labels/headers on glass tables
    val textPrimary: Color,
    val tint: Color,
    val success: Color,       // positive feedback text, readable on this theme's background
    val danger: Color         // error feedback text, readable on this theme's background
)

enum class AppThemeOption(val displayName: String, val palette: AppPalette) {
    TIHAR_NIGHT(
        "Tihar Night",
        AppPalette(
            isDark = true,
            backgroundTop = Color(0xFF1A1A2E),
            backgroundBottom = Color(0xFF0D0D1A),
            surface = Color(0xFF1A1A2E),
            accent = Color(0xFFD4AF37),
            accentAlt = Color(0xFFFF8C00),
            cta = Color(0xFF660000),
            frostText = Color(0xFFE9EEF6),
            frostAccent = Color(0xFFA6BEE0),
            textPrimary = Color.White,
            tint = Color.White,
            success = Color(0xFF81C784),
            danger = Color(0xFFFF8888)
        )
    ),
    MIDNIGHT_FROST(
        "Midnight Frost",
        AppPalette(
            isDark = true,
            backgroundTop = Color(0xFF101724),
            backgroundBottom = Color(0xFF090D14),
            surface = Color(0xFF162032),
            accent = Color(0xFF8FB8E8),
            accentAlt = Color(0xFF5FD0C0),
            cta = Color(0xFF24466E),
            frostText = Color(0xFFEAF0F8),
            frostAccent = Color(0xFF9DC1E8),
            textPrimary = Color(0xFFE8EDF5),
            tint = Color.White,
            success = Color(0xFF81C784),
            danger = Color(0xFFFF8888)
        )
    ),
    MARIGOLD_DAY(
        "Marigold Day",
        AppPalette(
            isDark = false,
            backgroundTop = Color(0xFFFDF6EA),
            backgroundBottom = Color(0xFFF2E5CE),
            surface = Color(0xFFFFFBF2),
            accent = Color(0xFF9A7215),
            accentAlt = Color(0xFFC85E12),
            cta = Color(0xFF8B1A1A),
            frostText = Color(0xFF3A3226),
            frostAccent = Color(0xFF8C6A3F),
            textPrimary = Color(0xFF2B2118),
            tint = Color(0xFF3A2E1E),
            success = Color(0xFF1B5E20),
            danger = Color(0xFFB71C1C)
        )
    ),
    HIMALAYAN_MIST(
        "Himalayan Mist",
        AppPalette(
            isDark = false,
            backgroundTop = Color(0xFFF2F6FB),
            backgroundBottom = Color(0xFFE1E9F2),
            surface = Color(0xFFFBFDFF),
            accent = Color(0xFF3D5A80),
            accentAlt = Color(0xFF4C7FA6),
            cta = Color(0xFF2F4B6E),
            frostText = Color(0xFF2A3A50),
            frostAccent = Color(0xFF52708F),
            textPrimary = Color(0xFF1E2733),
            tint = Color(0xFF22344A),
            success = Color(0xFF1B5E20),
            danger = Color(0xFFB71C1C)
        )
    );

    companion object {
        fun fromName(name: String?): AppThemeOption =
            entries.firstOrNull { it.name == name } ?: TIHAR_NIGHT
    }
}
