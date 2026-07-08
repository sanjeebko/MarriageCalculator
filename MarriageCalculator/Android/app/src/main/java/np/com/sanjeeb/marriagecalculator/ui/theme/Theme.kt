package np.com.sanjeeb.marriagecalculator.ui.theme

import android.app.Activity
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.SideEffect
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.platform.LocalView
import androidx.core.view.WindowCompat

private val DarkColorScheme = darkColorScheme(
    primary = DeepRedTika,
    secondary = MarigoldOrange,
    tertiary = GoldAccent,
    background = TiharNightBlue,
    surface = TiharNightBlue,
    onPrimary = GoldAccent,
    onSecondary = GoldAccent,
    onBackground = GoldAccent,
    onSurface = GoldAccent
)

@Composable
fun MarriageCalculatorTheme(content: @Composable () -> Unit) {
    val view = LocalView.current
    if (!view.isInEditMode) {
        SideEffect {
            val window = (view.context as Activity).window
            window.statusBarColor = Color.Transparent.toArgb()
            WindowCompat.getInsetsController(window, view).isAppearanceLightStatusBars = false
        }
    }

    MaterialTheme(
        colorScheme = DarkColorScheme,
        typography = FestiveTypography,
        content = content
    )
}
