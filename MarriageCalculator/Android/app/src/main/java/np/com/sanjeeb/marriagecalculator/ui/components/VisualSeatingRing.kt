package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.animation.core.RepeatMode
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.infiniteRepeatable
import androidx.compose.animation.core.rememberInfiniteTransition
import androidx.compose.animation.core.tween
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.BoxWithConstraints
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Casino
import androidx.compose.material.icons.filled.Navigation
import androidx.compose.material.icons.filled.SwapHoriz
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.rotate
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.IntOffset
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.zIndex
import coil.compose.AsyncImage
import coil.request.ImageRequest
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme
import java.io.File
import kotlin.math.cos
import kotlin.math.roundToInt
import kotlin.math.sin

/**
 * Calculates (x, y) coordinates on an ellipse perimeter for `count` players.
 * Angle starts at top (-90 degrees) and progresses clockwise.
 */
fun calculateSeatOffset(
    index: Int,
    totalPlayers: Int,
    radiusX: Float,
    radiusY: Float
): Pair<Float, Float> {
    if (totalPlayers <= 0) return 0f to 0f
    val angle = (-Math.PI / 2.0) + (index * 2.0 * Math.PI / totalPlayers)
    val x = (radiusX * cos(angle)).toFloat()
    val y = (radiusY * sin(angle)).toFloat()
    return x to y
}

/**
 * Visual Poker Seating Ring (Issue #30):
 * Displays players positioned clockwise around an oval/circular felt table with:
 * - Animated dealer button ("D") on the current dealer.
 * - Directional rotation indicator showing deal sequence.
 * - Next dealer indicator chip.
 */
@Composable
fun VisualSeatingRing(
    players: List<Player>,
    currentDealerId: String?,
    nextDealerId: String?,
    modifier: Modifier = Modifier,
    onPlayerClick: ((Player) -> Unit)? = null,
    onArrangeSeatsClick: (() -> Unit)? = null
) {
    if (players.isEmpty()) return

    val infiniteTransition = rememberInfiniteTransition(label = "tableRingPulse")
    val dealerGlowScale by infiniteTransition.animateFloat(
        initialValue = 1f,
        targetValue = 1.15f,
        animationSpec = infiniteRepeatable(
            animation = tween(900),
            repeatMode = RepeatMode.Reverse
        ),
        label = "dealerGlowScale"
    )

    Card(
        modifier = modifier
            .fillMaxWidth()
            .clip(RoundedCornerShape(16.dp)),
        shape = RoundedCornerShape(16.dp),
        colors = CardDefaults.cardColors(containerColor = AppTheme.palette.tint.copy(alpha = 0.04f)),
        border = BorderStroke(1.dp, AppTheme.palette.tint.copy(alpha = 0.12f))
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 12.dp, vertical = 10.dp)
        ) {
            // Header
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Icon(
                    imageVector = Icons.Default.Casino,
                    contentDescription = null,
                    tint = AppTheme.palette.accent,
                    modifier = Modifier.size(16.dp)
                )
                Spacer(modifier = Modifier.width(6.dp))
                Text(
                    text = "TABLE SEATING & DEALER ROTATION",
                    color = AppTheme.palette.frostAccent,
                    fontSize = 11.sp,
                    fontWeight = FontWeight.Bold,
                    letterSpacing = 1.sp,
                    modifier = Modifier.weight(1f)
                )

                if (onArrangeSeatsClick != null) {
                    Row(
                        modifier = Modifier
                            .clip(RoundedCornerShape(6.dp))
                            .clickable(onClick = onArrangeSeatsClick)
                            .background(AppTheme.palette.accent.copy(alpha = 0.12f))
                            .border(1.dp, AppTheme.palette.accent.copy(alpha = 0.35f), RoundedCornerShape(6.dp))
                            .padding(horizontal = 8.dp, vertical = 3.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Icon(
                            imageVector = Icons.Default.SwapHoriz,
                            contentDescription = null,
                            tint = AppTheme.palette.accent,
                            modifier = Modifier.size(13.dp)
                        )
                        Spacer(modifier = Modifier.width(4.dp))
                        Text(
                            text = "Arrange",
                            color = AppTheme.palette.accent,
                            fontSize = 11.sp,
                            fontWeight = FontWeight.Bold
                        )
                    }
                }
            }

            Spacer(modifier = Modifier.height(8.dp))

            // Oval Table Canvas Area
            BoxWithConstraints(
                modifier = Modifier
                    .fillMaxWidth()
                    .height(190.dp),
                contentAlignment = Alignment.Center
            ) {
                val density = LocalDensity.current.density
                val totalWidth = constraints.maxWidth.toFloat()
                val totalHeight = constraints.maxHeight.toFloat()

                // Ellipse radii for player centers
                val radiusX = (totalWidth * 0.38f).coerceAtMost(160f * density)
                val radiusY = (totalHeight * 0.36f).coerceAtMost(65f * density)

                // The Felt Table Surface
                Box(
                    modifier = Modifier
                        .size(width = (radiusX * 1.55f / density).dp, height = (radiusY * 1.55f / density).dp)
                        .shadow(12.dp, RoundedCornerShape(100.dp))
                        .clip(RoundedCornerShape(100.dp))
                        .background(
                            Brush.radialGradient(
                                listOf(
                                    Color(0xFF133E2B), // Classic casino felt green
                                    Color(0xFF0A2217),
                                    Color(0xFF04120C)
                                )
                            )
                        )
                        .border(
                            width = 2.5.dp,
                            brush = Brush.linearGradient(
                                listOf(
                                    AppTheme.palette.accent.copy(alpha = 0.8f),
                                    AppTheme.palette.accentAlt.copy(alpha = 0.5f),
                                    AppTheme.palette.accent.copy(alpha = 0.8f)
                                )
                            ),
                            shape = RoundedCornerShape(100.dp)
                        ),
                    contentAlignment = Alignment.Center
                ) {
                    // Center Felt Watermark & Rotation Indicator
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Text(
                                text = "MARRIAGE",
                                color = AppTheme.palette.accent.copy(alpha = 0.55f),
                                fontSize = 10.sp,
                                fontFamily = FontFamily.Serif,
                                fontWeight = FontWeight.Bold,
                                letterSpacing = 2.sp
                            )
                        }
                        Row(
                            verticalAlignment = Alignment.CenterVertically,
                            modifier = Modifier.padding(top = 2.dp)
                        ) {
                            Icon(
                                imageVector = Icons.Default.Navigation,
                                contentDescription = "Clockwise rotation",
                                tint = AppTheme.palette.accent.copy(alpha = 0.45f),
                                modifier = Modifier
                                    .size(10.dp)
                                    .rotate(90f) // pointing clockwise
                            )
                            Spacer(modifier = Modifier.width(3.dp))
                            Text(
                                text = "Clockwise Deal",
                                color = Color.White.copy(alpha = 0.45f),
                                fontSize = 8.sp,
                                fontWeight = FontWeight.Medium
                            )
                        }
                    }
                }

                // Players Ring
                players.forEachIndexed { index, player ->
                    val (offsetX, offsetY) = calculateSeatOffset(
                        index = index,
                        totalPlayers = players.size,
                        radiusX = radiusX,
                        radiusY = radiusY
                    )

                    val isCurrentDealer = player.id == currentDealerId
                    val isNextDealer = player.id == nextDealerId && !isCurrentDealer

                    Box(
                        modifier = Modifier
                            .offset { IntOffset(offsetX.roundToInt(), offsetY.roundToInt()) }
                            .zIndex(if (isCurrentDealer) 2f else 1f),
                        contentAlignment = Alignment.Center
                    ) {
                        PlayerSeatNode(
                            player = player,
                            seatNumber = index + 1,
                            isCurrentDealer = isCurrentDealer,
                            isNextDealer = isNextDealer,
                            dealerGlowScale = dealerGlowScale,
                            onClick = { onPlayerClick?.invoke(player) }
                        )
                    }
                }
            }
        }
    }
}

@Composable
private fun PlayerSeatNode(
    player: Player,
    seatNumber: Int,
    isCurrentDealer: Boolean,
    isNextDealer: Boolean,
    dealerGlowScale: Float,
    onClick: () -> Unit
) {
    Column(
        horizontalAlignment = Alignment.CenterHorizontally,
        modifier = Modifier
            .clickable(onClick = onClick)
            .padding(2.dp)
    ) {
        Box(contentAlignment = Alignment.Center) {
            // Dealer Chip animated glow
            if (isCurrentDealer) {
                Box(
                    modifier = Modifier
                        .size((36 * dealerGlowScale).dp)
                        .background(
                            Brush.radialGradient(
                                listOf(
                                    AppTheme.palette.accent.copy(alpha = 0.45f),
                                    Color.Transparent
                                )
                            ),
                            shape = CircleShape
                        )
                )
            }

            // Player Avatar Bubble
            val uri = player.photoUri
            val model = if (uri != null && (uri.startsWith("android.resource") || uri.startsWith("http"))) {
                uri
            } else if (uri != null) {
                File(uri)
            } else null

            if (model != null) {
                AsyncImage(
                    model = ImageRequest.Builder(LocalContext.current)
                        .data(model)
                        .crossfade(true)
                        .build(),
                    contentDescription = null,
                    contentScale = ContentScale.Crop,
                    modifier = Modifier
                        .size(28.dp)
                        .clip(CircleShape)
                        .border(
                            width = if (isCurrentDealer) 2.dp else 1.dp,
                            color = if (isCurrentDealer) AppTheme.palette.accent else AppTheme.palette.tint.copy(alpha = 0.35f),
                            shape = CircleShape
                        )
                )
            } else {
                Box(
                    modifier = Modifier
                        .size(28.dp)
                        .clip(CircleShape)
                        .background(
                            if (isCurrentDealer) AppTheme.palette.accent.copy(alpha = 0.25f)
                            else AppTheme.palette.tint.copy(alpha = 0.15f)
                        )
                        .border(
                            width = if (isCurrentDealer) 2.dp else 1.dp,
                            color = if (isCurrentDealer) AppTheme.palette.accent else AppTheme.palette.tint.copy(alpha = 0.35f),
                            shape = CircleShape
                        ),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = player.name.take(1).uppercase(),
                        color = if (isCurrentDealer) AppTheme.palette.accent else AppTheme.palette.textPrimary,
                        fontWeight = FontWeight.Bold,
                        fontSize = 12.sp
                    )
                }
            }

            // Dealer Chip Badge "D" pinned to bottom-right of avatar
            if (isCurrentDealer) {
                Box(
                    modifier = Modifier
                        .align(Alignment.BottomEnd)
                        .offset(x = 6.dp, y = 6.dp)
                        .size(16.dp)
                        .background(
                            Brush.linearGradient(
                                listOf(AppTheme.palette.accent, AppTheme.palette.accentAlt)
                            ),
                            shape = CircleShape
                        )
                        .border(1.dp, Color.White, CircleShape),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = "D",
                        color = Color.Black,
                        fontSize = 9.sp,
                        fontWeight = FontWeight.ExtraBold
                    )
                }
            } else if (isNextDealer) {
                // Next Dealer indicator
                Box(
                    modifier = Modifier
                        .align(Alignment.BottomEnd)
                        .offset(x = 6.dp, y = 6.dp)
                        .size(14.dp)
                        .background(AppTheme.palette.cardSurface, shape = CircleShape)
                        .border(1.dp, AppTheme.palette.frostAccent, CircleShape),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = "›",
                        color = AppTheme.palette.frostAccent,
                        fontSize = 10.sp,
                        fontWeight = FontWeight.Bold
                    )
                }
            }
        }

        Spacer(modifier = Modifier.height(2.dp))

        // Player Name & Seat Number
        Row(verticalAlignment = Alignment.CenterVertically) {
            Text(
                text = "${seatNumber}.",
                color = AppTheme.palette.tint.copy(alpha = 0.45f),
                fontSize = 9.sp,
                fontWeight = FontWeight.Bold
            )
            Spacer(modifier = Modifier.width(2.dp))
            Text(
                text = player.name,
                color = if (isCurrentDealer) AppTheme.palette.accent else AppTheme.palette.textPrimary,
                fontSize = 11.sp,
                fontWeight = if (isCurrentDealer) FontWeight.Bold else FontWeight.Medium,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )
        }
    }
}
