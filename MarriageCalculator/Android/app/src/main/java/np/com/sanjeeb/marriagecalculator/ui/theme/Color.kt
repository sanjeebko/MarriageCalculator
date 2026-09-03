
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
    val danger: Color,        // error feedback text, readable on this theme's background
    val numberPositive: Color,// high-contrast positive points/money
    val numberNegative: Color,// high-contrast negative points/money
    val numberZero: Color,    // clear readable neutral for zero values
    val cardSurface: Color,   // backdrop for table and round cards
    val chipUncleared: Color, // readable accent for uncleared payment button
    val chipCleared: Color    // readable success color for payment cleared button
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
            danger = Color(0xFFFF8888),
            numberPositive = Color(0xFF4ADE80),
            numberNegative = Color(0xFFFF6B6B),
            numberZero = Color(0xFFB0BEC5),
            cardSurface = Color(0xFF1E1E34),
            chipUncleared = Color(0xFFFFA726),
            chipCleared = Color(0xFF4ADE80)
        )
    ),
    HIGH_CONTRAST_DARK(
        "High Contrast Dark",
        AppPalette(
            isDark = true,
            backgroundTop = Color(0xFF0F141C),
            backgroundBottom = Color(0xFF080B10),
            surface = Color(0xFF161E2E),
            accent = Color(0xFFFBBF24),
            accentAlt = Color(0xFF38BDF8),
            cta = Color(0xFFEF4444),
            frostText = Color(0xFFFFFFFF),
            frostAccent = Color(0xFF93C5FD),
            textPrimary = Color(0xFFF8FAFC),
            tint = Color.White,
            success = Color(0xFF22C55E),
            danger = Color(0xFFEF4444),
            numberPositive = Color(0xFF22C55E),
            numberNegative = Color(0xFFF87171),
            numberZero = Color(0xFF94A3B8),
            cardSurface = Color(0xFF1B2433),
            chipUncleared = Color(0xFFFBBF24),
            chipCleared = Color(0xFF22C55E)
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
            danger = Color(0xFFFF8888),
            numberPositive = Color(0xFF4ADE80),
            numberNegative = Color(0xFFFF6B6B),
            numberZero = Color(0xFF94A3B8),
            cardSurface = Color(0xFF1A263C),
            chipUncleared = Color(0xFF5FD0C0),
            chipCleared = Color(0xFF4ADE80)
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
            danger = Color(0xFFB71C1C),
            numberPositive = Color(0xFF15803D),
            numberNegative = Color(0xFFB91C1C),
            numberZero = Color(0xFF6B7280),
            cardSurface = Color(0xFFFFF7EA),
            chipUncleared = Color(0xFFC85E12),
            chipCleared = Color(0xFF15803D)
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
            danger = Color(0xFFB71C1C),
            numberPositive = Color(0xFF15803D),
            numberNegative = Color(0xFFB91C1C),
            numberZero = Color(0xFF64748B),
            cardSurface = Color(0xFFF1F6FB),
            chipUncleared = Color(0xFF3D5A80),
            chipCleared = Color(0xFF15803D)
        )
    );

    companion object {
        fun fromName(name: String?): AppThemeOption =
            entries.firstOrNull { it.name == name } ?: TIHAR_NIGHT
    }
}
