package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.animation.core.FastOutSlowInEasing
import androidx.compose.animation.core.LinearEasing
import androidx.compose.animation.core.RepeatMode
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.infiniteRepeatable
import androidx.compose.animation.core.rememberInfiniteTransition
import androidx.compose.animation.core.tween
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.BoxWithConstraints
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.SwapHoriz
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.drawBehind
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.Path
import androidx.compose.ui.graphics.Shadow
import androidx.compose.ui.graphics.graphicsLayer
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.PlatformTextStyle
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.IntOffset
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.zIndex
import coil.compose.AsyncImage
import coil.request.ImageRequest
import np.com.sanjeeb.marriagecalculator.R
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme
import java.io.File
import kotlin.math.PI
import kotlin.math.cos
import kotlin.math.roundToInt
import kotlin.math.sin
import kotlin.math.sqrt

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
 * Traditional Handcrafted Nepali Carved Wood Table & Visual Seating Ring (Issue #44):
 * - Authentic Nepali carved wooden card table asset with Newari floral/peacock border and brass mandala.
 * - Players seated cleanly OUTSIDE the table perimeter, keeping the table surface clear.
 * - Dynamic clockwise dealer rotation arc with traveling warm brass comet beam and directional chevrons.
 * - Antique embossed brass dealer coin ("D") and next dealer token.
 * - Perfectly centered and positioned brass seat tokens (1..N).
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

    val infiniteTransition = rememberInfiniteTransition(label = "nepaliTableAnimations")
    val dealerGlowScale by infiniteTransition.animateFloat(
        initialValue = 1f,
        targetValue = 1.20f,
        animationSpec = infiniteRepeatable(
            animation = tween(1000),
            repeatMode = RepeatMode.Reverse
        ),
        label = "dealerGlowScale"
    )

    val orbitProgress by infiniteTransition.animateFloat(
        initialValue = 0f,
        targetValue = 1f,
        animationSpec = infiniteRepeatable(
            animation = tween(1800, easing = LinearEasing),
            repeatMode = RepeatMode.Restart
        ),
        label = "orbitProgress"
    )

    val smokePulse by infiniteTransition.animateFloat(
        initialValue = 0.45f,
        targetValue = 0.95f,
        animationSpec = infiniteRepeatable(
            animation = tween(1300, easing = FastOutSlowInEasing),
            repeatMode = RepeatMode.Reverse
        ),
        label = "smokePulse"
    )

    val smokeDrift by infiniteTransition.animateFloat(
        initialValue = 0f,
        targetValue = (2 * PI).toFloat(),
        animationSpec = infiniteRepeatable(
            animation = tween(2800, easing = LinearEasing),
            repeatMode = RepeatMode.Restart
        ),
        label = "smokeDrift"
    )

    val resolvedCurrentDealerIndex = players.indexOfFirst { it.id == currentDealerId }
    val resolvedNextDealerIndex = if (nextDealerId != null) {
        players.indexOfFirst { it.id == nextDealerId }
    } else if (resolvedCurrentDealerIndex >= 0 && players.size > 1) {
        (resolvedCurrentDealerIndex + 1) % players.size
    } else {
        -1
    }
    val effectiveNextDealerId = if (resolvedNextDealerIndex >= 0) players[resolvedNextDealerIndex].id else null

    Column(
        modifier = modifier
            .fillMaxWidth()
            .padding(horizontal = 4.dp, vertical = 2.dp)
    ) {
        // Optional Top Action Row (e.g. Arrange Seats)
        if (onArrangeSeatsClick != null) {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(bottom = 2.dp),
                horizontalArrangement = Arrangement.End,
                verticalAlignment = Alignment.CenterVertically
            ) {
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
                        text = "Arrange Seats",
                        color = AppTheme.palette.accent,
                        fontSize = 11.sp,
                        fontWeight = FontWeight.Bold
                    )
                }
            }
        }

        // Traditional Nepali Carved Wooden Table Canvas Area
        BoxWithConstraints(
            modifier = Modifier
                .fillMaxWidth()
                .height(264.dp),
            contentAlignment = Alignment.Center
        ) {
            val density = LocalDensity.current.density
            val totalWidth = constraints.maxWidth.toFloat()
            val totalHeight = constraints.maxHeight.toFloat()

            // Calculate table dimensions maintaining the authentic Nepali carved table aspect ratio (~1.624:1)
            // Table is prominently sized to fill the card nicely without extra blank space
            val availableWidthDp = totalWidth / density
            val targetTableWidthDp = (availableWidthDp - 80f).coerceIn(230f, 260f)
            val tableWidth = targetTableWidthDp * density
            val tableHeight = tableWidth / 1.6238f

            // Player orbit radii: positioned cleanly and comfortably OUTSIDE the carved wood rim
            val radiusX = (tableWidth / 2f) + (24f * density)
            val radiusY = (tableHeight / 2f) + (28f * density)
            val verticalCenterShift = 10f * density

            // 1. Traditional Nepali Carved Wooden Table Image Background
            Box(
                modifier = Modifier
                    .offset { IntOffset(0, verticalCenterShift.roundToInt()) }
                    .size(
                        width = (tableWidth / density).dp,
                        height = (tableHeight / density).dp
                    )
                    .shadow(16.dp, RoundedCornerShape(80.dp), ambientColor = Color.Black, spotColor = Color.Black),
                contentAlignment = Alignment.Center
            ) {
                Image(
                    painter = painterResource(id = R.drawable.nepali_wood_table),
                    contentDescription = "Nepali Carved Table",
                    contentScale = ContentScale.FillBounds,
                    modifier = Modifier.fillMaxSize()
                )

                // Dynamic Dealer Rotation Beam & Trajectory Canvas
                DealerRotationCanvas(
                    modifier = Modifier.fillMaxSize(),
                    orbitPhase = orbitProgress,
                    currentDealerIndex = resolvedCurrentDealerIndex,
                    nextDealerIndex = resolvedNextDealerIndex,
                    totalPlayers = players.size,
                    dealerGlowScale = dealerGlowScale
                )
            }

            // 2. Players Ring seated cleanly OUTSIDE around the table
            players.forEachIndexed { index, player ->
                val (offsetX, offsetY) = calculateSeatOffset(
                    index = index,
                    totalPlayers = players.size,
                    radiusX = radiusX,
                    radiusY = radiusY
                )

                val isCurrentDealer = player.id == currentDealerId
                val isNextDealer = player.id == effectiveNextDealerId && !isCurrentDealer
                val isTopSeat = offsetY < (-10f * density)

                Box(
                    modifier = Modifier
                        .offset { IntOffset(offsetX.roundToInt(), (offsetY + verticalCenterShift).roundToInt()) }
                        .zIndex(if (isCurrentDealer) 3f else if (isNextDealer) 2f else 1f),
                    contentAlignment = Alignment.Center
                ) {
                    PlayerSeatNode(
                        player = player,
                        seatNumber = index + 1,
                        isCurrentDealer = isCurrentDealer,
                        isNextDealer = isNextDealer,
                        isTopSeat = isTopSeat,
                        dealerGlowScale = dealerGlowScale,
                        smokePulse = smokePulse,
                        smokeDrift = smokeDrift,
                        onClick = { onPlayerClick?.invoke(player) }
                    )
                }
            }
        }
    }
}

/**
 * Draws the luminous dealer rotation beam, comet particle, chevrons, and dealer spotlight onto the wooden table.
 */
@Composable
private fun DealerRotationCanvas(
    modifier: Modifier = Modifier,
    orbitPhase: Float,
    currentDealerIndex: Int,
    nextDealerIndex: Int,
    totalPlayers: Int,
    dealerGlowScale: Float
) {
    Canvas(modifier = modifier) {
        val tableW = size.width
        val tableH = size.height
        val center = Offset(tableW / 2f, tableH / 2f)

        // Trajectory radius curving gracefully between the central brass mandala and carved timber rim
        val rBetX = tableW * 0.38f
        val rBetY = tableH * 0.34f

        if (currentDealerIndex in 0 until totalPlayers && nextDealerIndex in 0 until totalPlayers && totalPlayers > 1) {
            val currentAngleRad = (-PI / 2.0) + (currentDealerIndex * 2.0 * PI / totalPlayers)
            val nextAngleRad = (-PI / 2.0) + (nextDealerIndex * 2.0 * PI / totalPlayers)
            var sweepRad = nextAngleRad - currentAngleRad
            while (sweepRad <= 0.0) sweepRad += 2.0 * PI

            // Draw luminous golden trajectory arc
            val numSteps = 28
            val beamPath = Path()
            for (k in 0..numSteps) {
                val t = currentAngleRad + (k.toDouble() / numSteps) * sweepRad
                val px = center.x + (rBetX * cos(t)).toFloat()
                val py = center.y + (rBetY * sin(t)).toFloat()
                if (k == 0) beamPath.moveTo(px, py) else beamPath.lineTo(px, py)
            }

            drawPath(
                path = beamPath,
                brush = Brush.linearGradient(
                    listOf(
                        Color(0xFFFFD54F).copy(alpha = 0.70f),
                        Color(0xFFFFE082).copy(alpha = 0.95f),
                        Color(0xFFFFD54F).copy(alpha = 0.50f)
                    )
                ),
                style = Stroke(width = 2.5.dp.toPx())
            )

            // Midpoint Directional Chevrons
            val midSteps = listOf(0.40, 0.75)
            for (midPct in midSteps) {
                val midAngle = currentAngleRad + (midPct * sweepRad)
                val midX = center.x + (rBetX * cos(midAngle)).toFloat()
                val midY = center.y + (rBetY * sin(midAngle)).toFloat()
                val tanX = (-rBetX * sin(midAngle)).toFloat()
                val tanY = (rBetY * cos(midAngle)).toFloat()
                val len = sqrt(tanX * tanX + tanY * tanY).coerceAtLeast(0.001f)
                val uX = tanX / len
                val uY = tanY / len
                val nX = -uY
                val nY = uX

                val chvSize = 4.5.dp.toPx()
                val chvPath = Path().apply {
                    moveTo(midX - uX * chvSize + nX * chvSize, midY - uY * chvSize + nY * chvSize)
                    lineTo(midX, midY)
                    lineTo(midX - uX * chvSize - nX * chvSize, midY - uY * chvSize - nY * chvSize)
                }
                drawPath(
                    path = chvPath,
                    color = Color(0xFFFFD54F).copy(alpha = 0.85f),
                    style = Stroke(width = 1.5.dp.toPx())
                )
            }

            // Traveling Comet / Light Pulse
            val cometAngle = currentAngleRad + (orbitPhase * sweepRad)
            val cometX = center.x + (rBetX * cos(cometAngle)).toFloat()
            val cometY = center.y + (rBetY * sin(cometAngle)).toFloat()

            drawCircle(
                brush = Brush.radialGradient(
                    listOf(Color(0xFFFFE082), Color(0x77FFD54F), Color.Transparent),
                    center = Offset(cometX, cometY),
                    radius = 13.dp.toPx()
                ),
                radius = 13.dp.toPx(),
                center = Offset(cometX, cometY)
            )
            drawCircle(
                color = Color.White,
                radius = 2.5.dp.toPx(),
                center = Offset(cometX, cometY)
            )

            // Tangential Arrowhead at target (next dealer)
            val arrowHeadX = center.x + (rBetX * cos(nextAngleRad)).toFloat()
            val arrowHeadY = center.y + (rBetY * sin(nextAngleRad)).toFloat()
            val tanX = (-rBetX * sin(nextAngleRad)).toFloat()
            val tanY = (rBetY * cos(nextAngleRad)).toFloat()
            val len = sqrt(tanX * tanX + tanY * tanY).coerceAtLeast(0.001f)
            val uX = tanX / len
            val uY = tanY / len
            val nX = -uY
            val nY = uX

            val arrowSize = 6.dp.toPx()
            val arrowPath = Path().apply {
                moveTo(arrowHeadX + uX * arrowSize * 0.5f, arrowHeadY + uY * arrowSize * 0.5f)
                lineTo(arrowHeadX - uX * arrowSize + nX * (arrowSize * 0.7f), arrowHeadY - uY * arrowSize + nY * (arrowSize * 0.7f))
                lineTo(arrowHeadX - uX * (arrowSize * 0.4f), arrowHeadY - uY * (arrowSize * 0.4f))
                lineTo(arrowHeadX - uX * arrowSize - nX * (arrowSize * 0.7f), arrowHeadY - uY * arrowSize - nY * (arrowSize * 0.7f))
                close()
            }
            drawPath(
                path = arrowPath,
                color = Color(0xFFFFD54F)
            )

            // Spotlight on the wooden tabletop beneath current dealer
            val dealerWoodX = center.x + (rBetX * 0.95f * cos(currentAngleRad)).toFloat()
            val dealerWoodY = center.y + (rBetY * 0.95f * sin(currentAngleRad)).toFloat()
            drawCircle(
                brush = Brush.radialGradient(
                    listOf(
                        Color(0xFFFFD54F).copy(alpha = 0.30f * dealerGlowScale),
                        Color.Transparent
                    ),
                    center = Offset(dealerWoodX, dealerWoodY),
                    radius = 22.dp.toPx()
                ),
                radius = 22.dp.toPx(),
                center = Offset(dealerWoodX, dealerWoodY)
            )
        }
    }
}

/**
 * Authentic Handcrafted Embossed Brass/Bronze Nepali Dealer Coin:
 * - Handcrafted antique brass disc with concentric engravings.
 * - Warm sun-wheel / beaded rim pattern.
 * - Deep bronze coin patina and embossed bold "D".
 */
@Composable
fun NepaliDealerButton(
    modifier: Modifier = Modifier,
    size: androidx.compose.ui.unit.Dp = 20.dp
) {
    Box(
        modifier = modifier
            .size(size)
            .shadow(4.dp, CircleShape, ambientColor = Color.Black, spotColor = Color.Black)
            .clip(CircleShape),
        contentAlignment = Alignment.Center
    ) {
        Canvas(modifier = Modifier.fillMaxSize()) {
            val center = Offset(this.size.width / 2f, this.size.height / 2f)
            val radius = this.size.minDimension / 2f

            // 1. Base antique brass disc
            drawCircle(
                brush = Brush.radialGradient(
                    listOf(Color(0xFFFFDF00), Color(0xFFD4AF37), Color(0xFF8D6E63)),
                    center = center,
                    radius = radius
                ),
                radius = radius,
                center = center
            )

            // 2. Beaded sun-ray rim pattern
            val numBeads = 12
            val beadRadius = radius * 0.12f
            for (i in 0 until numBeads) {
                val angle = (i * 2.0 * PI / numBeads)
                val bx = center.x + ((radius - beadRadius - 1.dp.toPx()) * cos(angle)).toFloat()
                val by = center.y + ((radius - beadRadius - 1.dp.toPx()) * sin(angle)).toFloat()
                drawCircle(
                    color = Color(0xFF3E2723),
                    radius = beadRadius,
                    center = Offset(bx, by)
                )
            }

            // 3. Concentric golden bevel ring
            drawCircle(
                brush = Brush.sweepGradient(
                    listOf(
                        Color(0xFFFFDF00),
                        Color(0xFFD4AF37),
                        Color(0xFFFFF9A6),
                        Color(0xFF996515),
                        Color(0xFFFFDF00)
                    ),
                    center = center
                ),
                radius = radius * 0.72f,
                center = center,
                style = Stroke(width = 1.2.dp.toPx())
            )

            // 4. Recessed deep bronze inner core
            drawCircle(
                brush = Brush.radialGradient(
                    listOf(Color(0xFF4E342E), Color(0xFF2B1810), Color(0xFF1E100A)),
                    center = center,
                    radius = radius * 0.68f
                ),
                radius = radius * 0.68f,
                center = center
            )
        }

        // 5. Embossed bold "D" in bright gold
        Text(
            text = "D",
            fontFamily = FontFamily.Serif,
            fontWeight = FontWeight.Black,
            fontSize = 10.sp,
            color = Color(0xFFFFD54F),
            modifier = Modifier.align(Alignment.Center)
        )
    }
}

/**
 * Next Dealer Antique Silver/Brass Coin ("›").
 */
@Composable
fun NepaliNextDealerButton(
    modifier: Modifier = Modifier,
    size: androidx.compose.ui.unit.Dp = 17.dp
) {
    Box(
        modifier = modifier
            .size(size)
            .shadow(3.dp, CircleShape, ambientColor = Color.Black, spotColor = Color.Black)
            .clip(CircleShape),
        contentAlignment = Alignment.Center
    ) {
        Canvas(modifier = Modifier.fillMaxSize()) {
            val center = Offset(this.size.width / 2f, this.size.height / 2f)
            val radius = this.size.minDimension / 2f

            // Antique silver/brass disc
            drawCircle(
                brush = Brush.radialGradient(
                    listOf(Color(0xFFE0E0E0), Color(0xFFBCAAA4), Color(0xFF5D4037)),
                    center = center,
                    radius = radius
                ),
                radius = radius,
                center = center
            )

            // Inner ring
            drawCircle(
                color = Color(0xFF8D6E63),
                radius = radius * 0.72f,
                center = center,
                style = Stroke(width = 1.dp.toPx())
            )
        }

        Text(
            text = "›",
            fontWeight = FontWeight.Black,
            fontSize = 11.sp,
            color = Color(0xFFFFE082),
            modifier = Modifier.align(Alignment.Center)
        )
    }
}

@Composable
private fun FrostedNamePlaque(
    name: String,
    seatNumber: Int,
    isCurrentDealer: Boolean,
    isNextDealer: Boolean,
    smokePulse: Float = 1f,
    smokeDrift: Float = 0f
) {
    val plaqueShape = RoundedCornerShape(12.dp)

    Row(
        modifier = Modifier
            .drawBehind {
                if (isCurrentDealer) {
                    val w = size.width
                    val h = size.height
                    val centerY = h / 2f
                    val pulse = smokePulse.coerceIn(0.25f, 1f)
                    val smokeLength = 24.dp.toPx()

                    // 1. Dual-sided organic golden smoke plumes (elliptical falloff with zero flat edges)
                    // Left smoke plume (wafting outward to the left)
                    drawOval(
                        brush = Brush.radialGradient(
                            colors = listOf(
                                Color(0xFFFFE082).copy(alpha = 0.55f * pulse),
                                Color(0xFFFFD54F).copy(alpha = 0.30f * pulse),
                                Color(0xFFD4AF37).copy(alpha = 0.10f * pulse),
                                Color.Transparent
                            ),
                            center = Offset(2.dp.toPx(), centerY),
                            radius = smokeLength
                        ),
                        topLeft = Offset(-smokeLength, centerY - (h * 0.85f)),
                        size = Size(smokeLength + 8.dp.toPx(), h * 1.7f)
                    )

                    // Right smoke plume (wafting outward to the right)
                    drawOval(
                        brush = Brush.radialGradient(
                            colors = listOf(
                                Color(0xFFFFE082).copy(alpha = 0.55f * pulse),
                                Color(0xFFFFD54F).copy(alpha = 0.30f * pulse),
                                Color(0xFFD4AF37).copy(alpha = 0.10f * pulse),
                                Color.Transparent
                            ),
                            center = Offset(w - 2.dp.toPx(), centerY),
                            radius = smokeLength
                        ),
                        topLeft = Offset(w - 8.dp.toPx(), centerY - (h * 0.85f)),
                        size = Size(smokeLength + 8.dp.toPx(), h * 1.7f)
                    )

                    // 2. Billowing organic smoke wisps / curls on both sides
                    val drift1 = sin(smokeDrift) * 3.dp.toPx()
                    val drift2 = cos(smokeDrift) * 2.5.dp.toPx()
                    val driftScale = 1f + (sin(smokeDrift * 1.5f) * 0.12f)

                    // LEFT SIDE SMOKE WISPS:
                    // Primary plume puff
                    drawCircle(
                        brush = Brush.radialGradient(
                            colors = listOf(
                                Color(0xFFFFD54F).copy(alpha = 0.50f * pulse),
                                Color(0xFFD4AF37).copy(alpha = 0.18f * pulse),
                                Color.Transparent
                            ),
                            center = Offset(-4.dp.toPx(), centerY + drift1),
                            radius = 13.dp.toPx() * driftScale
                        ),
                        radius = 13.dp.toPx() * driftScale,
                        center = Offset(-4.dp.toPx(), centerY + drift1)
                    )
                    // Secondary outer wisp drifting away
                    drawCircle(
                        brush = Brush.radialGradient(
                            colors = listOf(
                                Color(0xFFFFE082).copy(alpha = 0.38f * pulse),
                                Color.Transparent
                            ),
                            center = Offset(-14.dp.toPx(), centerY - drift2),
                            radius = 10.dp.toPx() * driftScale
                        ),
                        radius = 10.dp.toPx() * driftScale,
                        center = Offset(-14.dp.toPx(), centerY - drift2)
                    )

                    // RIGHT SIDE SMOKE WISPS:
                    // Primary plume puff
                    drawCircle(
                        brush = Brush.radialGradient(
                            colors = listOf(
                                Color(0xFFFFD54F).copy(alpha = 0.50f * pulse),
                                Color(0xFFD4AF37).copy(alpha = 0.18f * pulse),
                                Color.Transparent
                            ),
                            center = Offset(w + 4.dp.toPx(), centerY - drift1),
                            radius = 13.dp.toPx() * driftScale
                        ),
                        radius = 13.dp.toPx() * driftScale,
                        center = Offset(w + 4.dp.toPx(), centerY - drift1)
                    )
                    // Secondary outer wisp drifting away
                    drawCircle(
                        brush = Brush.radialGradient(
                            colors = listOf(
                                Color(0xFFFFE082).copy(alpha = 0.38f * pulse),
                                Color.Transparent
                            ),
                            center = Offset(w + 14.dp.toPx(), centerY + drift2),
                            radius = 10.dp.toPx() * driftScale
                        ),
                        radius = 10.dp.toPx() * driftScale,
                        center = Offset(w + 14.dp.toPx(), centerY + drift2)
                    )

                    // 3. Subtle ambient golden glow around the plaque
                    drawRoundRect(
                        brush = Brush.radialGradient(
                            colors = listOf(
                                Color(0xFFFFD54F).copy(alpha = 0.25f * pulse),
                                Color.Transparent
                            ),
                            center = Offset(w / 2f, centerY),
                            radius = (w / 1.6f)
                        ),
                        topLeft = Offset(-5.dp.toPx(), -2.dp.toPx()),
                        size = Size(w + 10.dp.toPx(), h + 4.dp.toPx()),
                        cornerRadius = CornerRadius(14.dp.toPx())
                    )
                }
            }
            .clip(plaqueShape)
            .background(Color(0xEE1E130C))
            .border(
                width = 1.dp,
                color = if (isCurrentDealer) Color(0xFFFFD54F).copy(alpha = 0.85f)
                else if (isNextDealer) Color(0xFFD4AF37).copy(alpha = 0.65f)
                else Color(0x44D4AF37),
                shape = plaqueShape
            )
            .padding(start = 3.dp, end = 8.dp, top = 2.dp, bottom = 2.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        // Handcrafted Brass Circular Seat Number Token
        Box(
            modifier = Modifier
                .size(16.dp)
                .background(
                    if (isCurrentDealer) Color(0xFF5D3A1A) else Color(0xFF2B1810),
                    CircleShape
                )
                .border(
                    1.dp,
                    if (isCurrentDealer) Color(0xFFFFD54F) else Color(0xFFD4AF37),
                    CircleShape
                ),
            contentAlignment = Alignment.Center
        ) {
            Text(
                text = "$seatNumber",
                color = if (isCurrentDealer) Color(0xFFFFE082) else Color(0xFFEDE0D4),
                fontSize = 9.sp,
                fontWeight = FontWeight.Black,
                style = TextStyle(
                    platformStyle = PlatformTextStyle(includeFontPadding = false),
                    textAlign = TextAlign.Center,
                    lineHeight = 16.sp
                )
            )
        }
        Spacer(modifier = Modifier.width(4.dp))
        Text(
            text = name,
            color = if (isCurrentDealer) Color(0xFFFFE082) else Color(0xFFEDE0D4),
            fontSize = 10.5.sp,
            fontWeight = if (isCurrentDealer) FontWeight.Bold else FontWeight.Medium,
            maxLines = 1,
            overflow = TextOverflow.Ellipsis
        )
    }
}

@Composable
private fun PlayerSeatNode(
    player: Player,
    seatNumber: Int,
    isCurrentDealer: Boolean,
    isNextDealer: Boolean,
    isTopSeat: Boolean,
    dealerGlowScale: Float,
    smokePulse: Float = 1f,
    smokeDrift: Float = 0f,
    onClick: () -> Unit
) {
    Column(
        horizontalAlignment = Alignment.CenterHorizontally,
        modifier = Modifier
            .clickable(onClick = onClick)
            .padding(2.dp)
    ) {
        // When player is on the top half of the table, place name tag ABOVE the avatar
        // so the player sits outside the table looking inwards.
        if (isTopSeat) {
            FrostedNamePlaque(
                name = player.name,
                seatNumber = seatNumber,
                isCurrentDealer = isCurrentDealer,
                isNextDealer = isNextDealer,
                smokePulse = smokePulse,
                smokeDrift = smokeDrift
            )
            Spacer(modifier = Modifier.height(3.dp))
        }

        // Avatar outer Box has fixed 46.dp size: zero layout shifts, completely rock-solid!
        Box(
            modifier = Modifier.size(46.dp),
            contentAlignment = Alignment.Center
        ) {
            // Dealer animated warm golden halo scaled via graphicsLayer (GPU transform, zero layout re-measurement!)
            if (isCurrentDealer) {
                Box(
                    modifier = Modifier
                        .size(44.dp)
                        .graphicsLayer {
                            scaleX = dealerGlowScale
                            scaleY = dealerGlowScale
                        }
                        .background(
                            Brush.radialGradient(
                                listOf(
                                    Color(0xFFFFD54F).copy(alpha = 0.45f),
                                    Color(0xFFD4AF37).copy(alpha = 0.15f),
                                    Color.Transparent
                                )
                            ),
                            shape = CircleShape
                        )
                )
            } else if (isNextDealer) {
                Box(
                    modifier = Modifier
                        .size(40.dp)
                        .background(
                            Brush.radialGradient(
                                listOf(
                                    Color(0xFFFFD54F).copy(alpha = 0.25f),
                                    Color.Transparent
                                )
                            ),
                            shape = CircleShape
                        )
                )
            }

            // Avatar Anchor Container: precisely 34dp x 34dp
            Box(
                modifier = Modifier.size(34.dp),
                contentAlignment = Alignment.Center
            ) {
                val uri = player.photoUri
                val model = if (uri != null && (uri.startsWith("android.resource") || uri.startsWith("http"))) {
                    uri
                } else if (uri != null) {
                    File(uri)
                } else null

                val avatarModifier = Modifier
                    .fillMaxSize()
                    .shadow(4.dp, CircleShape)
                    .clip(CircleShape)

                val borderBrush = when {
                    isCurrentDealer -> Brush.sweepGradient(
                        listOf(
                            Color(0xFFFFDF00),
                            Color(0xFFD4AF37),
                            Color(0xFFFFF9A6),
                            Color(0xFF996515),
                            Color(0xFFFFDF00)
                        )
                    )
                    isNextDealer -> Brush.linearGradient(
                        listOf(Color(0xFFFFE082), Color(0xFFD4AF37), Color(0xFFFFD54F))
                    )
                    else -> Brush.linearGradient(
                        listOf(Color(0xFF8D6E63), Color(0xFF4E342E))
                    )
                }

                val borderWidth = if (isCurrentDealer) 2.5.dp else if (isNextDealer) 1.5.dp else 1.2.dp

                if (model != null) {
                    AsyncImage(
                        model = ImageRequest.Builder(LocalContext.current)
                            .data(model)
                            .crossfade(true)
                            .build(),
                        contentDescription = null,
                        contentScale = ContentScale.Crop,
                        modifier = avatarModifier.border(borderWidth, borderBrush, CircleShape)
                    )
                } else {
                    Box(
                        modifier = avatarModifier
                            .background(
                                Brush.radialGradient(
                                    listOf(
                                        if (isCurrentDealer) Color(0xFF4E342E) else Color(0xFF2B1810),
                                        if (isCurrentDealer) Color(0xFF26180F) else Color(0xFF170D08)
                                    )
                                )
                            )
                            .border(borderWidth, borderBrush, CircleShape),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            text = player.name.take(1).uppercase(),
                            color = if (isCurrentDealer) Color(0xFFFFD54F) else Color(0xFFEDE0D4),
                            fontWeight = FontWeight.Bold,
                            fontSize = 13.5.sp
                        )
                    }
                }

                // Antique Brass Dealer Coin / Next Coin (Pinned to avatar's bottom-right)
                if (isCurrentDealer) {
                    NepaliDealerButton(
                        size = 20.dp,
                        modifier = Modifier
                            .align(Alignment.BottomEnd)
                            .offset(x = 5.dp, y = 5.dp)
                    )
                } else if (isNextDealer) {
                    NepaliNextDealerButton(
                        size = 17.dp,
                        modifier = Modifier
                            .align(Alignment.BottomEnd)
                            .offset(x = 4.dp, y = 4.dp)
                    )
                }
            }
        }

        // When player is on the bottom half of the table, place name tag BELOW the avatar
        if (!isTopSeat) {
            Spacer(modifier = Modifier.height(3.dp))
            FrostedNamePlaque(
                name = player.name,
                seatNumber = seatNumber,
                isCurrentDealer = isCurrentDealer,
                isNextDealer = isNextDealer,
                smokePulse = smokePulse,
                smokeDrift = smokeDrift
            )
        }
    }
}
