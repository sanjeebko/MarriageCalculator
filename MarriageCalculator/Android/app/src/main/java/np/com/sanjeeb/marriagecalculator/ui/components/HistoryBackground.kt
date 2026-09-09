package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.animation.core.*
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.Path
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.unit.dp
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme

/**
 * A dedicated theme-aware background for History & Analytics logs.
 * Preserves the active theme's obsidian/black base gradient, contrast, and depth,
 * overlaid with subtle, elegant financial/game analytics motifs:
 * - Ambient accent glows
 * - Subtle horizontal ledger guide lines
 * - Smooth data line graphs (bezier waves)
 * - Stylized bar chart silhouettes at low opacity (0.035f - 0.05f)
 */
@Composable
fun HistoryBackground(
    modifier: Modifier = Modifier,
    content: @Composable () -> Unit
) {
    val palette = AppTheme.palette

    val infiniteTransition = rememberInfiniteTransition(label = "history_bg")

    // Slow ambient wave pulse
    val wavePulse by infiniteTransition.animateFloat(
        initialValue = 0.7f,
        targetValue = 1f,
        animationSpec = infiniteRepeatable(
            animation = tween(12000, easing = LinearEasing),
            repeatMode = RepeatMode.Reverse
        ),
        label = "wave_pulse"
    )

    Box(modifier = modifier.fillMaxSize()) {
        Canvas(modifier = Modifier.fillMaxSize()) {
            val width = size.width
            val height = size.height

            // 1. Base Vertical Gradient from active theme palette
            drawRect(
                brush = Brush.verticalGradient(
                    colors = listOf(palette.backgroundTop, palette.backgroundBottom)
                )
            )

            // 2. Ambient top & bottom glow blobs
            drawCircle(
                brush = Brush.radialGradient(
                    colors = listOf(palette.accent.copy(alpha = 0.12f), Color.Transparent),
                    center = Offset(width * 0.85f, height * 0.15f),
                    radius = width * 0.8f
                ),
                center = Offset(width * 0.85f, height * 0.15f),
                radius = width * 0.8f
            )

            drawCircle(
                brush = Brush.radialGradient(
                    colors = listOf(palette.cta.copy(alpha = 0.10f), Color.Transparent),
                    center = Offset(width * 0.15f, height * 0.85f),
                    radius = width * 0.75f
                ),
                center = Offset(width * 0.15f, height * 0.85f),
                radius = width * 0.75f
            )

            // 3. Subtle horizontal ledger guide lines (spacing 48dp)
            val lineSpacing = 48.dp.toPx()
            val ledgerColor = palette.tint.copy(alpha = 0.025f)
            var yPos = lineSpacing
            while (yPos < height) {
                drawLine(
                    color = ledgerColor,
                    start = Offset(0f, yPos),
                    end = Offset(width, yPos),
                    strokeWidth = 0.8.dp.toPx()
                )
                yPos += lineSpacing
            }

            // 4. Stylized subtle bar chart silhouettes in lower quadrant
            val barCount = 9
            val barWidth = width / (barCount * 2.2f)
            val baseBarY = height * 0.88f
            val barHeights = listOf(0.18f, 0.32f, 0.25f, 0.45f, 0.38f, 0.55f, 0.42f, 0.65f, 0.50f)

            barHeights.forEachIndexed { index, fraction ->
                val barHeight = height * 0.25f * fraction * wavePulse
                val barX = width * 0.08f + index * (barWidth * 1.8f)
                drawRect(
                    brush = Brush.verticalGradient(
                        colors = listOf(
                            palette.accentAlt.copy(alpha = 0.045f * wavePulse),
                            palette.accent.copy(alpha = 0.015f)
                        ),
                        startY = baseBarY - barHeight,
                        endY = baseBarY
                    ),
                    topLeft = Offset(barX, baseBarY - barHeight),
                    size = androidx.compose.ui.geometry.Size(barWidth, barHeight)
                )
            }

            // 5. Stylized analytics bezier wave running across the middle
            val wavePath = Path().apply {
                moveTo(0f, height * 0.42f)
                cubicTo(
                    width * 0.25f, height * (0.36f * wavePulse),
                    width * 0.45f, height * 0.48f,
                    width * 0.70f, height * (0.38f * wavePulse)
                )
                cubicTo(
                    width * 0.85f, height * 0.32f,
                    width * 0.95f, height * 0.44f,
                    width, height * 0.40f
                )
            }

            drawPath(
                path = wavePath,
                brush = Brush.horizontalGradient(
                    colors = listOf(
                        palette.accent.copy(alpha = 0.02f),
                        palette.accent.copy(alpha = 0.07f * wavePulse),
                        palette.accentAlt.copy(alpha = 0.03f)
                    )
                ),
                style = Stroke(width = 1.5.dp.toPx())
            )

            // Second harmonic wave
            val wavePath2 = Path().apply {
                moveTo(0f, height * 0.58f)
                cubicTo(
                    width * 0.30f, height * 0.64f,
                    width * 0.60f, height * (0.52f * wavePulse),
                    width, height * 0.62f
                )
            }

            drawPath(
                path = wavePath2,
                brush = Brush.horizontalGradient(
                    colors = listOf(
                        palette.accentAlt.copy(alpha = 0.01f),
                        palette.accentAlt.copy(alpha = 0.05f),
                        Color.Transparent
                    )
                ),
                style = Stroke(width = 1.2.dp.toPx())
            )
        }

        // Render content on top
        content()
    }
}
