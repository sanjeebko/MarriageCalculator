package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.animation.core.LinearEasing
import androidx.compose.animation.core.RepeatMode
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.infiniteRepeatable
import androidx.compose.animation.core.rememberInfiniteTransition
import androidx.compose.animation.core.tween
import androidx.compose.foundation.Canvas
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
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.Path
import androidx.compose.ui.graphics.PathEffect
import androidx.compose.ui.graphics.Shadow
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.text.TextStyle
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
 * Authentic Casino Poker Table & Visual Seating Ring (Issue #40):
 * - Deep mahogany and padded leather armrest bumper with 3D drop shadow.
 * - Multi-layered emerald felt with realistic overhead spotlight radial gradient.
 * - Inlaid gold betting rail with animated clockwise flow.
 * - Dynamic clockwise dealer rotation arc with traveling glowing comet beam and chevrons.
 * - Realistic 3D ceramic tournament dealer button ("DEALER / D").
 * - High-roller player pods with metallic bezels, anchored seat tokens, and rail-aligned name badges.
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

    val infiniteTransition = rememberInfiniteTransition(label = "casinoTableAnimations")
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

        // Casino Table Canvas Area
        BoxWithConstraints(
            modifier = Modifier
                .fillMaxWidth()
                .height(220.dp),
            contentAlignment = Alignment.Center
        ) {
            val density = LocalDensity.current.density
            val totalWidth = constraints.maxWidth.toFloat()
            val totalHeight = constraints.maxHeight.toFloat()

            // Ellipse radii for player centers
            val maxRadiusX = (totalWidth / 2f) - (42f * density)
            val radiusX = (totalWidth * 0.36f).coerceAtMost(maxRadiusX).coerceAtMost(145f * density)
            val radiusY = (totalHeight * 0.32f).coerceAtMost(66f * density)

            // The Casino Table: extends slightly beyond player centers so players sit right on the leather armrest rail
            val railPadX = 18f * density
            val railPadY = 18f * density
            val tableWidth = (2 * (radiusX + railPadX)).coerceAtMost(totalWidth - 8f * density)
            val tableHeight = (2 * (radiusY + railPadY)).coerceAtMost(totalHeight - 16f * density)

            Box(
                modifier = Modifier
                    .size(
                        width = (tableWidth / density).dp,
                        height = (tableHeight / density).dp
                    )
                    .shadow(16.dp, RoundedCornerShape(100.dp), ambientColor = Color.Black, spotColor = Color.Black),
                contentAlignment = Alignment.Center
            ) {
                CasinoTableCanvas(
                    modifier = Modifier.fillMaxSize(),
                    orbitPhase = orbitProgress,
                    currentDealerIndex = resolvedCurrentDealerIndex,
                    nextDealerIndex = resolvedNextDealerIndex,
                    totalPlayers = players.size,
                    dealerGlowScale = dealerGlowScale
                )

                // Center Felt Watermark & Suits
                Column(
                    horizontalAlignment = Alignment.CenterHorizontally,
                    modifier = Modifier.padding(horizontal = 8.dp)
                ) {
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(5.dp)
                    ) {
                        Text("♠", color = Color(0xFFD4AF37).copy(alpha = 0.50f), fontSize = 10.sp, fontWeight = FontWeight.Bold)
                        Text("♥", color = Color(0xFFE57373).copy(alpha = 0.65f), fontSize = 10.sp, fontWeight = FontWeight.Bold)
                        Text("♦", color = Color(0xFFE57373).copy(alpha = 0.65f), fontSize = 10.sp, fontWeight = FontWeight.Bold)
                        Text("♣", color = Color(0xFFD4AF37).copy(alpha = 0.50f), fontSize = 10.sp, fontWeight = FontWeight.Bold)
                    }
                    Spacer(modifier = Modifier.height(1.dp))
                    Text(
                        text = "MARRIAGE",
                        style = TextStyle(
                            color = Color(0xFFE5C158).copy(alpha = 0.65f),
                            fontSize = 11.sp,
                            fontFamily = FontFamily.Serif,
                            fontWeight = FontWeight.Black,
                            letterSpacing = 2.5.sp,
                            shadow = Shadow(
                                color = Color(0x99000000),
                                offset = Offset(0f, 2f),
                                blurRadius = 3f
                            )
                        )
                    )
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        modifier = Modifier.padding(top = 1.dp)
                    ) {
                        Text(
                            text = "CLOCKWISE DEAL ↻",
                            color = Color(0xFFE5C158).copy(alpha = 0.40f),
                            fontSize = 7.5.sp,
                            fontWeight = FontWeight.Bold,
                            letterSpacing = 1.sp
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
                val isNextDealer = player.id == effectiveNextDealerId && !isCurrentDealer
                val isTopSeat = offsetY < (-15f * density)

                Box(
                    modifier = Modifier
                        .offset { IntOffset(offsetX.roundToInt(), offsetY.roundToInt()) }
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
                        onClick = { onPlayerClick?.invoke(player) }
                    )
                }
            }
        }
    }
}

/**
 * Draws the casino poker table felt, wood racetrack, padded leather bumper, and animated dealer rotation path.
 */
@Composable
private fun CasinoTableCanvas(
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

        // 1. Armrest (Padded Leather Rim)
        drawRoundRect(
            brush = Brush.verticalGradient(
                listOf(
                    Color(0xFF382218), // Top light highlight
                    Color(0xFF24150E), // Rich mahogany leather
                    Color(0xFF140B07)  // Deep shadow under rail
                )
            ),
            topLeft = Offset.Zero,
            size = Size(tableW, tableH),
            cornerRadius = CornerRadius(tableH / 2f, tableH / 2f)
        )

        // Armrest outer beveled rim highlight
        drawRoundRect(
            brush = Brush.verticalGradient(
                listOf(Color(0xFF5E3928), Color(0x22140B07))
            ),
            topLeft = Offset.Zero,
            size = Size(tableW, tableH),
            cornerRadius = CornerRadius(tableH / 2f, tableH / 2f),
            style = Stroke(width = 1.5.dp.toPx())
        )

        // 2. Concentric Brass Bead Inlay
        val brassInset = 4.dp.toPx()
        val brassW = (tableW - 2 * brassInset).coerceAtLeast(0f)
        val brassH = (tableH - 2 * brassInset).coerceAtLeast(0f)
        drawRoundRect(
            brush = Brush.linearGradient(
                listOf(
                    Color(0xFFFFF176),
                    Color(0xFFD4AF37),
                    Color(0xFF8D6E14),
                    Color(0xFFD4AF37)
                )
            ),
            topLeft = Offset(brassInset, brassInset),
            size = Size(brassW, brassH),
            cornerRadius = CornerRadius(brassH / 2f, brassH / 2f),
            style = Stroke(width = 1.dp.toPx())
        )

        // 3. Wooden Racetrack Inlay
        val woodInset = 6.dp.toPx()
        val woodW = (tableW - 2 * woodInset).coerceAtLeast(0f)
        val woodH = (tableH - 2 * woodInset).coerceAtLeast(0f)
        drawRoundRect(
            brush = Brush.radialGradient(
                listOf(Color(0xFF2E1911), Color(0xFF150A05)),
                center = center,
                radius = tableW * 0.5f
            ),
            topLeft = Offset(woodInset, woodInset),
            size = Size(woodW, woodH),
            cornerRadius = CornerRadius(woodH / 2f, woodH / 2f)
        )

        // 4. High-Roller Casino Emerald Felt
        val feltInset = 11.dp.toPx()
        val feltW = (tableW - 2 * feltInset).coerceAtLeast(0f)
        val feltH = (tableH - 2 * feltInset).coerceAtLeast(0f)
        drawRoundRect(
            brush = Brush.radialGradient(
                listOf(
                    Color(0xFF157641), // Center warm spotlight
                    Color(0xFF0C4A28), // Deep emerald felt
                    Color(0xFF062C18), // Shadowed felt
                    Color(0xFF02160C)  // Edge vignette
                ),
                center = center,
                radius = feltW * 0.55f
            ),
            topLeft = Offset(feltInset, feltInset),
            size = Size(feltW, feltH),
            cornerRadius = CornerRadius(feltH / 2f, feltH / 2f)
        )

        // Felt inner ambient shadow
        drawRoundRect(
            brush = Brush.radialGradient(
                listOf(Color.Transparent, Color(0x66000000)),
                center = center,
                radius = feltW * 0.52f
            ),
            topLeft = Offset(feltInset, feltInset),
            size = Size(feltW, feltH),
            cornerRadius = CornerRadius(feltH / 2f, feltH / 2f)
        )

        // 5. Golden Betting Rail & Rotation Orbit
        val betInset = feltInset + 10.dp.toPx()
        val betW = (tableW - 2 * betInset).coerceAtLeast(0f)
        val betH = (tableH - 2 * betInset).coerceAtLeast(0f)
        if (betW > 20f && betH > 20f) {
            // Outer golden betting line
            drawRoundRect(
                color = Color(0xFFD4AF37).copy(alpha = 0.38f),
                topLeft = Offset(betInset, betInset),
                size = Size(betW, betH),
                cornerRadius = CornerRadius(betH / 2f, betH / 2f),
                style = Stroke(width = 1.5.dp.toPx())
            )

            // Clockwise directional dash animation
            drawRoundRect(
                color = Color(0xFFD4AF37).copy(alpha = 0.22f),
                topLeft = Offset(betInset, betInset),
                size = Size(betW, betH),
                cornerRadius = CornerRadius(betH / 2f, betH / 2f),
                style = Stroke(
                    width = 1.dp.toPx(),
                    pathEffect = PathEffect.dashPathEffect(floatArrayOf(12f, 10f), orbitPhase * 22f)
                )
            )

            // Inner golden hairline
            val innerBetInset = betInset + 3.dp.toPx()
            val innerBetW = (tableW - 2 * innerBetInset).coerceAtLeast(0f)
            val innerBetH = (tableH - 2 * innerBetInset).coerceAtLeast(0f)
            if (innerBetW > 10f && innerBetH > 10f) {
                drawRoundRect(
                    color = Color(0xFFD4AF37).copy(alpha = 0.18f),
                    topLeft = Offset(innerBetInset, innerBetInset),
                    size = Size(innerBetW, innerBetH),
                    cornerRadius = CornerRadius(innerBetH / 2f, innerBetH / 2f),
                    style = Stroke(width = 0.75.dp.toPx())
                )
            }

            // 6. Dynamic Dealer Rotation Arc & Traveling Light Beam
            if (currentDealerIndex in 0 until totalPlayers && nextDealerIndex in 0 until totalPlayers && totalPlayers > 1) {
                val currentAngleRad = (-PI / 2.0) + (currentDealerIndex * 2.0 * PI / totalPlayers)
                val nextAngleRad = (-PI / 2.0) + (nextDealerIndex * 2.0 * PI / totalPlayers)
                var sweepRad = nextAngleRad - currentAngleRad
                while (sweepRad <= 0.0) sweepRad += 2.0 * PI

                val rBetX = betW / 2f
                val rBetY = betH / 2f

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

                // Spotlight on the felt beneath current dealer
                val dealerFeltX = center.x + (rBetX * 0.95f * cos(currentAngleRad)).toFloat()
                val dealerFeltY = center.y + (rBetY * 0.95f * sin(currentAngleRad)).toFloat()
                drawCircle(
                    brush = Brush.radialGradient(
                        listOf(
                            Color(0xFFFFD54F).copy(alpha = 0.30f * dealerGlowScale),
                            Color.Transparent
                        ),
                        center = Offset(dealerFeltX, dealerFeltY),
                        radius = 24.dp.toPx()
                    ),
                    radius = 24.dp.toPx(),
                    center = Offset(dealerFeltX, dealerFeltY)
                )
            }
        }
    }
}

/**
 * Authentic 3D Casino Tournament Dealer Puck:
 * - Ceramic ivory core with alternating midnight edge notches.
 * - 24k Gold inlay bezel ring.
 * - Recessed inner disc with drop shadow and bold serif "D".
 */
@Composable
fun CasinoDealerButton(
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

            // 1. Base ceramic puck (bone white / ivory)
            drawCircle(
                brush = Brush.radialGradient(
                    listOf(Color(0xFFFFFFFF), Color(0xFFEEEEEE), Color(0xFFD5D5D5)),
                    center = center,
                    radius = radius
                ),
                radius = radius,
                center = center
            )

            // 2. Alternating edge notches (casino tournament puck pattern)
            val numStripes = 12
            val stripeWidth = radius * 0.22f
            for (i in 0 until numStripes) {
                if (i % 2 == 1) {
                    drawArc(
                        color = Color(0xFF1B2332), // Deep midnight tournament notch
                        startAngle = i * (360f / numStripes),
                        sweepAngle = 360f / (numStripes * 2f),
                        useCenter = false,
                        style = Stroke(width = stripeWidth),
                        topLeft = Offset(center.x - radius + stripeWidth / 2f, center.y - radius + stripeWidth / 2f),
                        size = Size((radius - stripeWidth / 2f) * 2f, (radius - stripeWidth / 2f) * 2f)
                    )
                }
            }

            // 3. Concentric 24k Gold Inlay Ring
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
                radius = radius * 0.74f,
                center = center,
                style = Stroke(width = 1.2.dp.toPx())
            )

            // 4. Recessed inner disc
            drawCircle(
                brush = Brush.radialGradient(
                    listOf(Color(0xFFFFFFFF), Color(0xFFE8E8E8), Color(0xFFCECECE)),
                    center = center,
                    radius = radius * 0.70f
                ),
                radius = radius * 0.70f,
                center = center
            )
        }

        // 5. Embossed bold "D"
        Text(
            text = "D",
            fontFamily = FontFamily.Serif,
            fontWeight = FontWeight.Black,
            fontSize = 10.sp,
            color = Color(0xFF1A1A1A),
            modifier = Modifier.align(Alignment.Center)
        )
    }
}

/**
 * Platinum Next Dealer Chip ("›").
 */
@Composable
fun CasinoNextDealerButton(
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

            // Platinum disc
            drawCircle(
                brush = Brush.radialGradient(
                    listOf(Color(0xFFFFFFFF), Color(0xFFCFD8DC), Color(0xFF90A4AE)),
                    center = center,
                    radius = radius
                ),
                radius = radius,
                center = center
            )

            // Edge notches
            val numStripes = 8
            val stripeWidth = radius * 0.22f
            for (i in 0 until numStripes) {
                if (i % 2 == 1) {
                    drawArc(
                        color = Color(0xFF37474F),
                        startAngle = i * (360f / numStripes),
                        sweepAngle = 360f / (numStripes * 2f),
                        useCenter = false,
                        style = Stroke(width = stripeWidth),
                        topLeft = Offset(center.x - radius + stripeWidth / 2f, center.y - radius + stripeWidth / 2f),
                        size = Size((radius - stripeWidth / 2f) * 2f, (radius - stripeWidth / 2f) * 2f)
                    )
                }
            }

            // Inner ring
            drawCircle(
                color = Color(0xFF78909C),
                radius = radius * 0.72f,
                center = center,
                style = Stroke(width = 1.dp.toPx())
            )
        }

        Text(
            text = "›",
            fontWeight = FontWeight.Black,
            fontSize = 11.sp,
            color = Color(0xFF263238),
            modifier = Modifier.align(Alignment.Center)
        )
    }
}

@Composable
private fun FrostedNamePlaque(
    name: String,
    isCurrentDealer: Boolean,
    isNextDealer: Boolean
) {
    Box(
        modifier = Modifier
            .clip(RoundedCornerShape(8.dp))
            .background(Color(0xDD0D1612))
            .border(
                width = 0.75.dp,
                color = if (isCurrentDealer) Color(0xFFD4AF37).copy(alpha = 0.75f)
                else if (isNextDealer) Color(0xFF80DEEA).copy(alpha = 0.50f)
                else Color(0x33FFFFFF),
                shape = RoundedCornerShape(8.dp)
            )
            .padding(horizontal = 6.dp, vertical = 1.5.dp),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = name,
            color = if (isCurrentDealer) Color(0xFFFFD54F) else Color(0xFFE9EEF6),
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
    onClick: () -> Unit
) {
    Column(
        horizontalAlignment = Alignment.CenterHorizontally,
        modifier = Modifier
            .clickable(onClick = onClick)
            .padding(2.dp)
    ) {
        // When player is on the top half of the table, place name tag ABOVE the avatar
        // so the entire green felt and betting rail remain unobstructed.
        if (isTopSeat) {
            FrostedNamePlaque(
                name = player.name,
                isCurrentDealer = isCurrentDealer,
                isNextDealer = isNextDealer
            )
            Spacer(modifier = Modifier.height(3.dp))
        }

        Box(contentAlignment = Alignment.Center) {
            // Dealer animated halo
            if (isCurrentDealer) {
                Box(
                    modifier = Modifier
                        .size((48 * dealerGlowScale).dp)
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
                                    Color(0xFF80DEEA).copy(alpha = 0.30f),
                                    Color.Transparent
                                )
                            ),
                            shape = CircleShape
                        )
                )
            }

            // Avatar Anchor Container: precisely 32dp x 32dp
            Box(
                modifier = Modifier.size(32.dp),
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
                        listOf(Color(0xFFECEFF1), Color(0xFF90A4AE), Color(0xFFFFFFFF))
                    )
                    else -> Brush.linearGradient(
                        listOf(Color(0xFF546E7A), Color(0xFF37474F))
                    )
                }

                val borderWidth = if (isCurrentDealer) 2.5.dp else if (isNextDealer) 1.5.dp else 1.dp

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
                                        if (isCurrentDealer) Color(0xFF3E2723) else Color(0xFF263238),
                                        if (isCurrentDealer) Color(0xFF1B0000) else Color(0xFF0F171B)
                                    )
                                )
                            )
                            .border(borderWidth, borderBrush, CircleShape),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            text = player.name.take(1).uppercase(),
                            color = if (isCurrentDealer) Color(0xFFFFD54F) else Color(0xFFECEFF1),
                            fontWeight = FontWeight.Bold,
                            fontSize = 13.sp
                        )
                    }
                }

                // Miniature Brass Seat Number Token (Pinned to avatar's top-left)
                Box(
                    modifier = Modifier
                        .align(Alignment.TopStart)
                        .offset(x = (-4).dp, y = (-4).dp)
                        .size(14.dp)
                        .shadow(2.dp, CircleShape)
                        .background(
                            if (isCurrentDealer) Color(0xFF2E1C0C) else Color(0xFF1E262B),
                            shape = CircleShape
                        )
                        .border(
                            1.dp,
                            if (isCurrentDealer) Color(0xFFD4AF37) else Color(0xFF78909C),
                            CircleShape
                        ),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = "$seatNumber",
                        color = if (isCurrentDealer) Color(0xFFFFD54F) else Color(0xFFCFD8DC),
                        fontSize = 8.sp,
                        fontWeight = FontWeight.Black
                    )
                }

                // Casino Dealer Button / Next Chip (Pinned to avatar's bottom-right)
                if (isCurrentDealer) {
                    CasinoDealerButton(
                        size = 20.dp,
                        modifier = Modifier
                            .align(Alignment.BottomEnd)
                            .offset(x = 6.dp, y = 6.dp)
                    )
                } else if (isNextDealer) {
                    CasinoNextDealerButton(
                        size = 17.dp,
                        modifier = Modifier
                            .align(Alignment.BottomEnd)
                            .offset(x = 5.dp, y = 5.dp)
                    )
                }
            }
        }

        // When player is on the bottom half of the table, place name tag BELOW the avatar
        if (!isTopSeat) {
            Spacer(modifier = Modifier.height(3.dp))
            FrostedNamePlaque(
                name = player.name,
                isCurrentDealer = isCurrentDealer,
                isNextDealer = isNextDealer
            )
        }
    }
}
