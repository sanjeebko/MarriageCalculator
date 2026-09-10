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

    // Select background image dynamically based on active theme
    val bgDrawableId = when (palette) {
        AppThemeOption.TIHAR_NIGHT.palette -> R.drawable.history_bg_dashain
        AppThemeOption.MIDNIGHT_FROST.palette -> R.drawable.history_bg_luxury
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
        Box(
            modifier = Modifier
                .fillMaxSize()
                .background(
                    Brush.verticalGradient(
                        colors = listOf(
                            palette.backgroundTop.copy(alpha = 0.50f),
                            Color.Transparent,
                            palette.backgroundBottom.copy(alpha = 0.70f)
                        )
                    )
                )
        )

        // 3. Render the screen content on top
        content()
    }
}

