package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.painterResource
import np.com.sanjeeb.marriagecalculator.R
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme
import np.com.sanjeeb.marriagecalculator.ui.theme.AppThemeOption

/**
 * A dedicated theme-aware background for History & Activity logs.
 * Replaces programmatic SVG/canvas vector graphics with high-fidelity
 * AI-generated artwork (Nano Banana) featuring deep card suit motifs,
 * ambient vignetting, and theme-adaptive gradient overlays for superior text legibility.
 */
@Composable
fun HistoryBackground(
    modifier: Modifier = Modifier,
    content: @Composable () -> Unit
) {
    val palette = AppTheme.palette

    // Each theme has its own unique, custom-tailored AI background artwork
    val bgDrawableId = when (palette) {
        AppThemeOption.BLACK_AND_WHITE.palette -> R.drawable.history_bg_monochrome
        AppThemeOption.TIHAR_NIGHT.palette -> R.drawable.history_bg_dashain
        AppThemeOption.HIGH_CONTRAST_DARK.palette -> R.drawable.history_bg_contrast
        AppThemeOption.MIDNIGHT_FROST.palette -> R.drawable.history_bg_frost
        AppThemeOption.MARIGOLD_DAY.palette -> R.drawable.history_bg_marigold
        AppThemeOption.HIMALAYAN_MIST.palette -> R.drawable.history_bg_mist
        else -> R.drawable.history_bg_monochrome
    }

    Box(modifier = modifier.fillMaxSize()) {
        // 1. High-fidelity AI-generated Background Image
        Image(
            painter = painterResource(id = bgDrawableId),
            contentDescription = null,
            contentScale = ContentScale.Crop,
            modifier = Modifier.fillMaxSize()
        )

        // 2. Theme-Adaptive Vignette & Depth Gradient Overlay
        val vignetteColors = if (palette.isDark) {
            listOf(
                palette.backgroundTop.copy(alpha = 0.45f),
                Color.Transparent,
                palette.backgroundBottom.copy(alpha = 0.65f)
            )
        } else {
            listOf(
                palette.backgroundTop.copy(alpha = 0.25f),
                Color.Transparent,
                palette.backgroundBottom.copy(alpha = 0.45f)
            )
        }

        Box(
            modifier = Modifier
                .fillMaxSize()
                .background(Brush.verticalGradient(colors = vignetteColors))
        )

        // 3. Render the screen content on top
        content()
    }
}

