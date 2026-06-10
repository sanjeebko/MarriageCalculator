package com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

// Preset rim/face color pairs matching the Tihar Night theme

/** Deep red metallic — use for primary actions (New Game, Start, Submit) */
val MetallicRedRim = listOf(Color(0xFFCC3333), Color(0xFF330000))
val MetallicRedFace = listOf(Color(0xFF880000), Color(0xFF4A0000))

/** Dark gold metallic — use for secondary actions (Scoreboard, etc.) */
val MetallicGoldRim = listOf(Color(0xFFD4AF37), Color(0xFF7A6418))
val MetallicGoldFace = listOf(Color(0xFF2A2010), Color(0xFF1A1510))

/**
 * Shared metallic button used across all screens.
 *
 * @param onClick      Click handler.
 * @param text         Button label.
 * @param rimColors    Outer bezel gradient (top → bottom).
 * @param faceColors   Inner face gradient (top → bottom).
 * @param textColor    Label color.
 * @param modifier     Optional modifier; fill-width is applied internally; add height here.
 * @param enabled      Whether the button accepts input.
 * @param isLoading    When true, shows a spinner instead of content.
 * @param leadingIcon  Optional composable icon placed to the left of the label.
 */
@Composable
fun MetallicButton(
    onClick: () -> Unit,
    text: String,
    rimColors: List<Color>,
    faceColors: List<Color>,
    textColor: Color,
    modifier: Modifier = Modifier,
    enabled: Boolean = true,
    isLoading: Boolean = false,
    leadingIcon: (@Composable () -> Unit)? = null
) {
    val active = enabled && !isLoading

    Button(
        onClick = onClick,
        enabled = active,
        modifier = modifier
            .fillMaxWidth()
            .shadow(
                elevation = if (active) 12.dp else 4.dp,
                shape = RoundedCornerShape(16.dp),
                spotColor = Color.Black
            ),
        colors = ButtonDefaults.buttonColors(
            containerColor = Color.Transparent,
            disabledContainerColor = Color.Transparent
        ),
        contentPadding = PaddingValues(0.dp),
        shape = RoundedCornerShape(16.dp)
    ) {
        // Outer rim gradient
        Box(
            modifier = Modifier
                .fillMaxSize()
                .background(
                    brush = Brush.linearGradient(
                        colors = if (active) rimColors else rimColors.map { it.copy(alpha = 0.4f) },
                        start = androidx.compose.ui.geometry.Offset(0f, 0f),
                        end = androidx.compose.ui.geometry.Offset(0f, Float.POSITIVE_INFINITY)
                    )
                )
                .padding(4.dp),
            contentAlignment = Alignment.Center
        ) {
            // Inner face
            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .background(
                        brush = Brush.verticalGradient(
                            colors = if (active) faceColors else faceColors.map { it.copy(alpha = 0.4f) }
                        ),
                        shape = RoundedCornerShape(12.dp)
                    )
                    .border(
                        width = 1.dp,
                        brush = Brush.verticalGradient(
                            colors = listOf(Color.White.copy(alpha = 0.5f), Color.Transparent)
                        ),
                        shape = RoundedCornerShape(12.dp)
                    ),
                contentAlignment = Alignment.Center
            ) {
                // Horizontal sheen 1
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .alpha(0.1f)
                        .background(
                            brush = Brush.horizontalGradient(
                                colorStops = arrayOf(
                                    0.0f to Color.Black,
                                    0.25f to Color.White,
                                    0.35f to Color.Black,
                                    0.65f to Color.Black,
                                    0.85f to Color.White,
                                    1.0f to Color.Black
                                )
                            ),
                            shape = RoundedCornerShape(12.dp)
                        )
                )
                // Horizontal sheen 2
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .alpha(0.1f)
                        .background(
                            brush = Brush.horizontalGradient(
                                colorStops = arrayOf(
                                    0.0f to Color.Black,
                                    0.25f to Color.White,
                                    0.35f to Color.Black,
                                    0.80f to Color.White,
                                    1.0f to Color.Black
                                )
                            ),
                            shape = RoundedCornerShape(12.dp)
                        )
                )
                // Vertical sheen
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .alpha(0.05f)
                        .background(
                            brush = Brush.verticalGradient(
                                colorStops = arrayOf(
                                    0.0f to Color.Black,
                                    0.15f to Color.White,
                                    0.25f to Color.White,
                                    0.35f to Color.Black,
                                    0.75f to Color.Black,
                                    0.85f to Color.Gray,
                                    0.90f to Color.Black,
                                    1.0f to Color.Black
                                )
                            ),
                            shape = RoundedCornerShape(12.dp)
                        )
                )

                // Content
                if (isLoading) {
                    CircularProgressIndicator(
                        color = textColor,
                        modifier = Modifier.size(24.dp),
                        strokeWidth = 2.dp
                    )
                } else {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        modifier = Modifier.padding(horizontal = 16.dp)
                    ) {
                        if (leadingIcon != null) {
                            leadingIcon()
                            Spacer(Modifier.width(12.dp))
                        }
                        // Embossed text
                        Box {
                            Text(
                                text = text,
                                color = Color.White.copy(alpha = 0.6f),
                                style = MaterialTheme.typography.titleMedium.copy(
                                    fontWeight = FontWeight.Bold,
                                    fontSize = 18.sp
                                ),
                                modifier = Modifier.padding(top = 1.2.dp)
                            )
                            Text(
                                text = text,
                                color = textColor,
                                style = MaterialTheme.typography.titleMedium.copy(
                                    fontWeight = FontWeight.Bold,
                                    fontSize = 18.sp
                                )
                            )
                        }
                    }
                }
            }
        }
    }
}
