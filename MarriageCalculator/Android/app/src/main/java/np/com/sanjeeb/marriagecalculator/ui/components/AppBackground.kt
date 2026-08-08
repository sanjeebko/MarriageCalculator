package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.animation.core.*
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.drawscope.rotate
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.unit.dp
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme
import kotlin.math.cos
import kotlin.math.sin
import kotlin.random.Random

/**
 * A professional, animated background that enhances glassmorphism effects.
 * It uses theme-aware gradients, moving "blobs", twinkling stars, and an angled grid
 * to provide depth and texture behind semi-transparent UI elements.
 */
@Composable
fun AppBackground(
    modifier: Modifier = Modifier,
    content: @Composable () -> Unit
) {
    val palette = AppTheme.palette
    
    // 1. Setup Infinite Animation
    val infiniteTransition = rememberInfiniteTransition(label = "background")
    
    // Slow, fluid movement values for blobs
    val animTime by infiniteTransition.animateFloat(
        initialValue = 0f,
        targetValue = 2f * Math.PI.toFloat(),
        animationSpec = infiniteRepeatable(
            animation = durationBasedTween(40000), // Very slow 40s cycle
            repeatMode = RepeatMode.Restart
        ),
        label = "time"
    )

    // Subtle pulsing for the grid/lines and stars
    val pulseAlpha by infiniteTransition.animateFloat(
        initialValue = 0.6f,
        targetValue = 1f,
        animationSpec = infiniteRepeatable(
            animation = tween(8000, easing = LinearEasing),
            repeatMode = RepeatMode.Reverse
        ),
        label = "pulse"
    )

    // Generate random stars once
    val stars = remember {
        List(40) {
            Star(
                x = Random.nextFloat(),
                y = Random.nextFloat(),
                size = Random.nextFloat() * 2f + 1f,
                alphaBase = Random.nextFloat() * 0.3f + 0.1f,
                phase = Random.nextFloat() * 2f * Math.PI.toFloat()
            )
        }
    }

    Box(modifier = modifier.fillMaxSize()) {
        Canvas(modifier = Modifier.fillMaxSize()) {
            // 2. Primary Base Gradient
            drawRect(
                brush = Brush.verticalGradient(
                    colors = listOf(palette.backgroundTop, palette.backgroundBottom)
                )
            )

            // 3. Twinkling Stars
            stars.forEach { star ->
                val twinkle = (0.5f + 0.5f * sin((animTime + star.phase).toDouble())).toFloat()
                val starColor = palette.accent.copy(alpha = star.alphaBase * twinkle * pulseAlpha)
                drawCircle(
                    color = starColor,
                    radius = star.size.dp.toPx(),
                    center = Offset(size.width * star.x, size.height * star.y)
                )
            }

            // 4. Decorative Animated Blobs
            
            // Top-Left Accent Glow
            val blob1Offset = Offset(
                x = size.width * (0.15f + 0.05f * cos(animTime.toDouble()).toFloat()),
                y = size.height * (0.2f + 0.08f * sin(animTime.toDouble()).toFloat())
            )
            drawCircle(
                brush = Brush.radialGradient(
                    colors = listOf(palette.accent.copy(alpha = 0.22f), Color.Transparent),
                    center = blob1Offset,
                    radius = size.width * 0.9f
                ),
                center = blob1Offset,
                radius = size.width * 0.9f
            )

            // Middle-Right Secondary Glow
            val blob2Offset = Offset(
                x = size.width * (0.85f + 0.07f * sin(animTime.toDouble() * 0.7).toFloat()),
                y = size.height * (0.45f + 0.1f * cos(animTime.toDouble() * 0.7).toFloat())
            )
            drawCircle(
                brush = Brush.radialGradient(
                    colors = listOf(palette.accentAlt.copy(alpha = 0.16f), Color.Transparent),
                    center = blob2Offset,
                    radius = size.width * 0.7f
                ),
                center = blob2Offset,
                radius = size.width * 0.7f
            )

            // Bottom-Left Deep CTA Glow
            val blob3Offset = Offset(
                x = size.width * (0.25f + 0.06f * cos(animTime.toDouble() * 1.2).toFloat()),
                y = size.height * (0.85f + 0.05f * sin(animTime.toDouble() * 1.2).toFloat())
            )
            drawCircle(
                brush = Brush.radialGradient(
                    colors = listOf(palette.cta.copy(alpha = 0.12f), Color.Transparent),
                    center = blob3Offset,
                    radius = size.width * 0.8f
                ),
                center = blob3Offset,
                radius = size.width * 0.8f
            )

            // 5. Angled & Pulsing Grid
            // Rotating slightly (-12 degrees) to look more dynamic
            rotate(degrees = -12f, pivot = Offset(size.width / 2, size.height / 2)) {
                val strokeWidth = 0.6.dp.toPx()
                val spacing = 60.dp.toPx()
                val gridColor = palette.tint.copy(alpha = 0.035f * pulseAlpha)

                // Draw larger than screen to cover corners during rotation
                val gridBound = size.width.coerceAtLeast(size.height) * 1.5f
                
                var x = -gridBound / 2
                while (x < gridBound) {
                    drawLine(
                        color = gridColor,
                        start = Offset(x + size.width / 2, -gridBound / 2 + size.height / 2),
                        end = Offset(x + size.width / 2, gridBound / 2 + size.height / 2),
                        strokeWidth = strokeWidth
                    )
                    x += spacing
                }

                var y = -gridBound / 2
                while (y < gridBound) {
                    drawLine(
                        color = gridColor,
                        start = Offset(-gridBound / 2 + size.width / 2, y + size.height / 2),
                        end = Offset(gridBound / 2 + size.width / 2, y + size.height / 2),
                        strokeWidth = strokeWidth
                    )
                    y += spacing
                }
            }

            // 6. Abstract Geometric Sweeps
            rotate(degrees = 35f, pivot = Offset(size.width * 0.5f, size.height * 0.2f)) {
                drawLine(
                    brush = Brush.linearGradient(
                        colors = listOf(Color.Transparent, palette.accent.copy(alpha = 0.07f * pulseAlpha), Color.Transparent)
                    ),
                    start = Offset(-size.width, size.height * 0.2f),
                    end = Offset(size.width * 2f, size.height * 0.2f),
                    strokeWidth = 1.2.dp.toPx()
                )
            }

            rotate(degrees = -20f, pivot = Offset(size.width * 0.5f, size.height * 0.75f)) {
                drawLine(
                    brush = Brush.linearGradient(
                        colors = listOf(Color.Transparent, palette.accentAlt.copy(alpha = 0.06f * pulseAlpha), Color.Transparent)
                    ),
                    start = Offset(-size.width, size.height * 0.75f),
                    end = Offset(size.width * 2f, size.height * 0.75f),
                    strokeWidth = 1.8.dp.toPx()
                )
            }
        }

        // Subtle, faint card background image matching the theme
        Image(
            painter = painterResource(id = np.com.sanjeeb.marriagecalculator.R.drawable.card_bg_pattern),
            contentDescription = null,
            contentScale = ContentScale.Crop,
            modifier = Modifier.fillMaxSize(),
            alpha = 0.05f
        )
        
        // Render the screen content on top
        content()
    }
}

/** Internal model for twinkling stars */
private data class Star(
    val x: Float,
    val y: Float,
    val size: Float,
    val alphaBase: Float,
    val phase: Float
)

/** Helper to ensure consistent duration naming in Jetpack Compose animations */
private fun durationBasedTween(duration: Int) = tween<Float>(
    durationMillis = duration,
    easing = LinearEasing
)
