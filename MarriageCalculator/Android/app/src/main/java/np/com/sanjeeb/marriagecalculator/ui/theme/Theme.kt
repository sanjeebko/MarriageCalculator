package np.com.sanjeeb.marriagecalculator.ui.theme

import android.app.Activity
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.CompositionLocalProvider
import androidx.compose.runtime.SideEffect
import androidx.compose.runtime.staticCompositionLocalOf
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.platform.LocalView
import androidx.core.view.WindowCompat

val LocalAppPalette = staticCompositionLocalOf { AppThemeOption.TIHAR_NIGHT.palette }

/** Accessor for the active theme palette: `AppTheme.palette.accent` etc. */
object AppTheme {
    val palette: AppPalette
        @Composable get() = LocalAppPalette.current
}

@Composable
fun MarriageCalculatorTheme(
    theme: AppThemeOption = AppThemeOption.TIHAR_NIGHT,
    content: @Composable () -> Unit
) {
    val palette = theme.palette

    val view = LocalView.current
    if (!view.isInEditMode) {
        SideEffect {
            val window = (view.context as Activity).window
            window.statusBarColor = Color.Transparent.toArgb()
            WindowCompat.getInsetsController(window, view).isAppearanceLightStatusBars = !palette.isDark
        }
    }

    val colorScheme = if (palette.isDark) {
        darkColorScheme(
            primary = palette.cta,
            secondary = palette.accentAlt,
            tertiary = palette.accent,
            background = palette.backgroundTop,
            surface = palette.surface,
            onPrimary = palette.accent,
            onSecondary = palette.textPrimary,
            onBackground = palette.textPrimary,
            onSurface = palette.textPrimary,
            onSurfaceVariant = palette.textPrimary.copy(alpha = 0.7f)
        )
    } else {
        lightColorScheme(
            primary = palette.cta,
            secondary = palette.accentAlt,
            tertiary = palette.accent,
            background = palette.backgroundTop,
            surface = palette.surface,
            onPrimary = palette.surface,
            onSecondary = palette.textPrimary,
            onBackground = palette.textPrimary,
            onSurface = palette.textPrimary,
            onSurfaceVariant = palette.textPrimary.copy(alpha = 0.7f)
        )
    }

    CompositionLocalProvider(LocalAppPalette provides palette) {
        MaterialTheme(
            colorScheme = colorScheme,
            typography = FestiveTypography,
            content = content
        )
    }
}
