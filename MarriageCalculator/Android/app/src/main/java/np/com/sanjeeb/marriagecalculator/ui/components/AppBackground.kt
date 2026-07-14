package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.drawscope.rotate
import androidx.compose.ui.unit.dp
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme

/**
 * A professional, procedurally generated background that enhances glassmorphism effects.
 * It uses theme-aware gradients, soft blurred "blobs", and architectural lines to provide
 * depth and texture behind semi-transparent UI elements.
 */
@Composable
fun AppBackground(
    modifier: Modifier = Modifier,
    content: @Composable () -> Unit
) {
    val palette = AppTheme.palette
    
    Box(modifier = modifier.fillMaxSize()) {
        Canvas(modifier = Modifier.fillMaxSize()) {
            // 1. Primary Base Gradient
            drawRect(
                brush = Brush.verticalGradient(
                    colors = listOf(palette.backgroundTop, palette.backgroundBottom)
                )
            )

            // 2. Decorative Soft Blobs (The "Engine" of Glassmorphism)
            // These provide the color variations that become visible through frosted glass.
            
            // Top-Left Accent Glow
            drawCircle(
                brush = Brush.radialGradient(
                    colors = listOf(palette.accent.copy(alpha = 0.12f), Color.Transparent),
                    center = Offset(size.width * 0.15f, size.height * 0.2f),
                    radius = size.width * 0.8f
                ),
                center = Offset(size.width * 0.15f, size.height * 0.2f),
                radius = size.width * 0.8f
            )

            // Middle-Right Secondary Glow
            drawCircle(
                brush = Brush.radialGradient(
                    colors = listOf(palette.accentAlt.copy(alpha = 0.08f), Color.Transparent),
                    center = Offset(size.width * 0.85f, size.height * 0.45f),
                    radius = size.width * 0.6f
                ),
                center = Offset(size.width * 0.85f, size.height * 0.45f),
                radius = size.width * 0.6f
            )

            // Bottom-Left Deep CTA Glow
            drawCircle(
                brush = Brush.radialGradient(
                    colors = listOf(palette.cta.copy(alpha = 0.06f), Color.Transparent),
                    center = Offset(size.width * 0.2f, size.height * 0.85f),
                    radius = size.width * 0.7f
                ),
                center = Offset(size.width * 0.2f, size.height * 0.85f),
                radius = size.width * 0.7f
            )

            // 3. Faint Architectural Grid
            // Subtly grounds the UI and adds a "technical" feel.
            val strokeWidth = 0.5.dp.toPx()
            val spacing = 48.dp.toPx()
            val gridColor = palette.tint.copy(alpha = 0.025f)

            var x = 0f
            while (x < size.width) {
                drawLine(
                    color = gridColor,
                    start = Offset(x, 0f),
                    end = Offset(x, size.height),
                    strokeWidth = strokeWidth
                )
                x += spacing
            }

            var y = 0f
            while (y < size.height) {
                drawLine(
                    color = gridColor,
                    start = Offset(0f, y),
                    end = Offset(size.width, y),
                    strokeWidth = strokeWidth
                )
                y += spacing
            }

            // 4. Abstract Geometric Sweeps
            // Large diagonal lines that break the symmetry.
            rotate(degrees = 35f, pivot = Offset(size.width * 0.5f, size.height * 0.2f)) {
                drawLine(
                    brush = Brush.linearGradient(
                        colors = listOf(Color.Transparent, palette.accent.copy(alpha = 0.04f), Color.Transparent)
                    ),
                    start = Offset(-size.width, size.height * 0.2f),
                    end = Offset(size.width * 2f, size.height * 0.2f),
                    strokeWidth = 1.dp.toPx()
                )
            }

            rotate(degrees = -20f, pivot = Offset(size.width * 0.5f, size.height * 0.75f)) {
                drawLine(
                    brush = Brush.linearGradient(
                        colors = listOf(Color.Transparent, palette.accentAlt.copy(alpha = 0.03f), Color.Transparent)
                    ),
                    start = Offset(-size.width, size.height * 0.75f),
                    end = Offset(size.width * 2f, size.height * 0.75f),
                    strokeWidth = 1.5.dp.toPx()
                )
            }
        }
        
        // Render the screen content on top of the generated background
        content()
    }
}
