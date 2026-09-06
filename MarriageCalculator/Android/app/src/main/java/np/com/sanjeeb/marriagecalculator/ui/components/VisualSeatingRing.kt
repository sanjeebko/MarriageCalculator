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
import androidx.compose.foundation.layout.widthIn
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
            val radiusX = (tableWidth / 2f) + (26f * density)
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
                    .shadow(
                        16.dp,
                        RoundedCornerShape(80.dp),
                        ambientColor = if (AppTheme.palette.isDark) Color.Black else Color(0x33000000),
                        spotColor = if (AppTheme.palette.isDark) Color.Black else Color(0x22000000)
                    ),
                contentAlignment = Alignment.Center
            ) {
                Image(
                    painter = painterResource(id = R.drawable.nepali_wood_table),
                    contentDescription = "Nepali Carved Table",
                    contentScale = ContentScale.FillBounds,
                    modifier = Modifier.fillMaxSize()
                )
            }

            // 2. Players Ring seated cleanly OUTSIDE around the table as unified badges
            players.forEachIndexed { index, player ->
                val (offsetX, offsetY) = calculateSeatOffset(
                    index = index,
                    totalPlayers = players.size,
                    radiusX = radiusX,
                    radiusY = radiusY
                )

                val isCurrentDealer = player.id == currentDealerId
                val isNextDealer = player.id == effectiveNextDealerId && !isCurrentDealer

                Box(
                    modifier = Modifier
                        .offset { IntOffset(offsetX.roundToInt(), (offsetY + verticalCenterShift).roundToInt()) }
                        .zIndex(if (isCurrentDealer) 3f else if (isNextDealer) 2f else 1f),
                    contentAlignment = Alignment.Center
                ) {
                    PlayerSeatBadge(
                        player = player,
                        seatNumber = index + 1,
                        isCurrentDealer = isCurrentDealer,
                        isNextDealer = isNextDealer,
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
 * Authentic Handcrafted Embossed Nepali Dealer Coin:
 * - Handcrafted antique medallion disc with concentric engravings styled with active theme palette.
 * - Warm sun-wheel / beaded rim pattern.
 * - Recessed core and embossed bold "D".
 */
@Composable
fun NepaliDealerButton(
    modifier: Modifier = Modifier,
    size: androidx.compose.ui.unit.Dp = 20.dp
) {
    val pal = AppTheme.palette
    Box(
        modifier = modifier
            .size(size)
            .shadow(
                4.dp,
                CircleShape,
                ambientColor = if (pal.isDark) Color.Black else Color(0x33000000),
                spotColor = if (pal.isDark) Color.Black else Color(0x22000000)
            )
            .clip(CircleShape),
        contentAlignment = Alignment.Center
    ) {
        Canvas(modifier = Modifier.fillMaxSize()) {
            val center = Offset(this.size.width / 2f, this.size.height / 2f)
            val radius = this.size.minDimension / 2f

            // 1. Base theme-aware medallion disc
            drawCircle(
                brush = Brush.radialGradient(
                    listOf(
                        pal.accent,
                        pal.accentAlt,
                        if (pal.isDark) pal.surface else pal.cta
                    ),
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
                    color = if (pal.isDark) pal.surface else pal.cardSurface,
                    radius = beadRadius,
                    center = Offset(bx, by)
                )
            }

            // 3. Concentric theme bevel ring
            drawCircle(
                brush = Brush.sweepGradient(
                    listOf(
                        pal.accent,
                        pal.accentAlt,
                        pal.accent.copy(alpha = 0.8f),
                        pal.accentAlt,
                        pal.accent
                    ),
                    center = center
                ),
                radius = radius * 0.72f,
                center = center,
                style = Stroke(width = 1.2.dp.toPx())
            )

            // 4. Recessed inner core
            drawCircle(
                brush = Brush.radialGradient(
                    listOf(
                        pal.surface,
                        if (pal.isDark) pal.backgroundBottom else pal.cardSurface
                    ),
                    center = center,
                    radius = radius * 0.68f
                ),
                radius = radius * 0.68f,
                center = center
            )
        }

        // 5. Embossed bold "D" in theme accent
        Text(
            text = "D",
            fontFamily = FontFamily.Serif,
            fontWeight = FontWeight.Black,
            fontSize = 10.sp,
            color = pal.accent,
            modifier = Modifier.align(Alignment.Center)
        )
    }
}

/**
 * Next Dealer Medallion Coin ("›").
 */
@Composable
fun NepaliNextDealerButton(
    modifier: Modifier = Modifier,
    size: androidx.compose.ui.unit.Dp = 17.dp
) {
    val pal = AppTheme.palette
    Box(
        modifier = modifier
            .size(size)
            .shadow(
                3.dp,
                CircleShape,
                ambientColor = if (pal.isDark) Color.Black else Color(0x33000000),
                spotColor = if (pal.isDark) Color.Black else Color(0x22000000)
            )
            .clip(CircleShape),
        contentAlignment = Alignment.Center
    ) {
        Canvas(modifier = Modifier.fillMaxSize()) {
            val center = Offset(this.size.width / 2f, this.size.height / 2f)
            val radius = this.size.minDimension / 2f

            // Theme-aware disc
            drawCircle(
                brush = Brush.radialGradient(
                    listOf(
                        pal.accentAlt,
                        pal.surface,
                        if (pal.isDark) pal.backgroundBottom else pal.cardSurface
                    ),
                    center = center,
                    radius = radius
                ),
                radius = radius,
                center = center
            )

            // Inner ring
            drawCircle(
                color = pal.accentAlt.copy(alpha = 0.6f),
                radius = radius * 0.72f,
                center = center,
                style = Stroke(width = 1.dp.toPx())
            )
        }

        Text(
            text = "›",
            fontWeight = FontWeight.Black,
            fontSize = 11.sp,
            color = pal.accentAlt,
            modifier = Modifier.align(Alignment.Center)
        )
    }
}

/**
 * Unified Player Seat Badge:
 * Combines the enlarged profile image, seat number token, and player name into a single cohesive stadium badge.
 * Features dual-sided dealer smoke aura, theme-responsive medallions, and zero layout thrashing.
 */
@Composable
private fun PlayerSeatBadge(
    player: Player,
    seatNumber: Int,
    isCurrentDealer: Boolean,
    isNextDealer: Boolean,
    dealerGlowScale: Float,
    smokePulse: Float = 1f,
    smokeDrift: Float = 0f,
    onClick: () -> Unit
) {
    val pal = AppTheme.palette
    val badgeShape = RoundedCornerShape(22.dp)

    Box(contentAlignment = Alignment.Center) {
        Row(
            modifier = Modifier
                .drawBehind {
                    if (isCurrentDealer) {
                        val w = size.width
                        val h = size.height
                        val centerY = h / 2f
                        val pulse = smokePulse.coerceIn(0.25f, 1f)
                        val smokeLength = 26.dp.toPx()

                        val auraPrimary = pal.accent
                        val auraSecondary = pal.accentAlt

                        // 1. Dual-sided organic dealer aura plumes (elliptical falloff with zero flat edges)
                        // Left smoke plume (wafting outward to the left)
                        drawOval(
                            brush = Brush.radialGradient(
                                colors = listOf(
                                    auraPrimary.copy(alpha = 0.55f * pulse),
                                    auraSecondary.copy(alpha = 0.30f * pulse),
                                    auraPrimary.copy(alpha = 0.10f * pulse),
                                    Color.Transparent
                                ),
                                center = Offset(4.dp.toPx(), centerY),
                                radius = smokeLength
                            ),
                            topLeft = Offset(-smokeLength, centerY - (h * 0.85f)),
                            size = Size(smokeLength + 10.dp.toPx(), h * 1.7f)
                        )

                        // Right smoke plume (wafting outward to the right)
                        drawOval(
                            brush = Brush.radialGradient(
                                colors = listOf(
                                    auraPrimary.copy(alpha = 0.55f * pulse),
                                    auraSecondary.copy(alpha = 0.30f * pulse),
                                    auraPrimary.copy(alpha = 0.10f * pulse),
                                    Color.Transparent
                                ),
                                center = Offset(w - 4.dp.toPx(), centerY),
                                radius = smokeLength
                            ),
                            topLeft = Offset(w - 10.dp.toPx(), centerY - (h * 0.85f)),
                            size = Size(smokeLength + 10.dp.toPx(), h * 1.7f)
                        )

                        // 2. Billowing organic aura wisps / curls on both sides
                        val drift1 = sin(smokeDrift) * 3.dp.toPx()
                        val drift2 = cos(smokeDrift) * 2.5.dp.toPx()
                        val driftScale = 1f + (sin(smokeDrift * 1.5f) * 0.12f)

                        // LEFT SIDE AURA WISPS:
                        drawCircle(
                            brush = Brush.radialGradient(
                                colors = listOf(
                                    auraPrimary.copy(alpha = 0.50f * pulse),
                                    auraSecondary.copy(alpha = 0.18f * pulse),
                                    Color.Transparent
                                ),
                                center = Offset(-4.dp.toPx(), centerY + drift1),
                                radius = 13.dp.toPx() * driftScale
                            ),
                            radius = 13.dp.toPx() * driftScale,
                            center = Offset(-4.dp.toPx(), centerY + drift1)
                        )
                        drawCircle(
                            brush = Brush.radialGradient(
                                colors = listOf(
                                    auraSecondary.copy(alpha = 0.38f * pulse),
                                    Color.Transparent
                                ),
                                center = Offset(-14.dp.toPx(), centerY - drift2),
                                radius = 10.dp.toPx() * driftScale
                            ),
                            radius = 10.dp.toPx() * driftScale,
                            center = Offset(-14.dp.toPx(), centerY - drift2)
                        )

                        // RIGHT SIDE AURA WISPS:
                        drawCircle(
                            brush = Brush.radialGradient(
                                colors = listOf(
                                    auraPrimary.copy(alpha = 0.50f * pulse),
                                    auraSecondary.copy(alpha = 0.18f * pulse),
                                    Color.Transparent
                                ),
                                center = Offset(w + 4.dp.toPx(), centerY - drift1),
                                radius = 13.dp.toPx() * driftScale
                            ),
                            radius = 13.dp.toPx() * driftScale,
                            center = Offset(w + 4.dp.toPx(), centerY - drift1)
                        )
                        drawCircle(
                            brush = Brush.radialGradient(
                                colors = listOf(
                                    auraSecondary.copy(alpha = 0.38f * pulse),
                                    Color.Transparent
                                ),
                                center = Offset(w + 14.dp.toPx(), centerY + drift2),
                                radius = 10.dp.toPx() * driftScale
                            ),
                            radius = 10.dp.toPx() * driftScale,
                            center = Offset(w + 14.dp.toPx(), centerY + drift2)
                        )

                        // 3. Subtle ambient aura glow around the badge
                        drawRoundRect(
                            brush = Brush.radialGradient(
                                colors = listOf(
                                    auraPrimary.copy(alpha = 0.25f * pulse),
                                    Color.Transparent
                                ),
                                center = Offset(w / 2f, centerY),
                                radius = (w / 1.6f)
                            ),
                            topLeft = Offset(-6.dp.toPx(), -3.dp.toPx()),
                            size = Size(w + 12.dp.toPx(), h + 6.dp.toPx()),
                            cornerRadius = CornerRadius(24.dp.toPx())
                        )
                    }
                }
                .shadow(
                    6.dp,
                    badgeShape,
                    ambientColor = if (pal.isDark) Color.Black else Color(0x33000000),
                    spotColor = if (pal.isDark) Color.Black else Color(0x22000000)
                )
                .clip(badgeShape)
                .background(
                    if (pal.isDark) {
                        pal.surface.copy(alpha = 0.90f)
                    } else {
                        pal.surface.copy(alpha = 0.96f)
                    }
                )
                .border(
                    width = if (isCurrentDealer) 1.8.dp else if (isNextDealer) 1.4.dp else 1.dp,
                    brush = if (isCurrentDealer) Brush.sweepGradient(
                        listOf(
                            pal.accent,
                            pal.accentAlt,
                            pal.accent.copy(alpha = 0.75f),
                            pal.accentAlt,
                            pal.accent
                        )
                    ) else if (isNextDealer) Brush.linearGradient(
                        listOf(pal.accentAlt, pal.accent)
                    ) else if (pal.isDark) Brush.linearGradient(
                        listOf(pal.tint.copy(alpha = 0.22f), pal.tint.copy(alpha = 0.08f))
                    ) else Brush.linearGradient(
                        listOf(pal.accent.copy(alpha = 0.35f), pal.tint.copy(alpha = 0.15f))
                    ),
                    shape = badgeShape
                )
                .clickable(onClick = onClick)
                .padding(start = 2.dp, end = 9.dp, top = 2.dp, bottom = 2.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            // Profile image container: 42dp x 42dp (enlarged from 34dp)
            Box(
                modifier = Modifier.size(42.dp),
                contentAlignment = Alignment.Center
            ) {
                // Dealer animated warm halo scaled via graphicsLayer (GPU transform, zero layout re-measurement)
                if (isCurrentDealer) {
                    Box(
                        modifier = Modifier
                            .size(40.dp)
                            .graphicsLayer {
                                scaleX = dealerGlowScale
                                scaleY = dealerGlowScale
                            }
                            .background(
                                Brush.radialGradient(
                                    listOf(
                                        pal.accent.copy(alpha = 0.45f),
                                        pal.accentAlt.copy(alpha = 0.15f),
                                        Color.Transparent
                                    )
                                ),
                                shape = CircleShape
                            )
                    )
                } else if (isNextDealer) {
                    Box(
                        modifier = Modifier
                            .size(38.dp)
                            .background(
                                Brush.radialGradient(
                                    listOf(
                                        pal.accentAlt.copy(alpha = 0.25f),
                                        Color.Transparent
                                    )
                                ),
                                shape = CircleShape
                            )
                    )
                }

                // Avatar Photo or Initial: 38dp x 38dp
                Box(
                    modifier = Modifier.size(38.dp),
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
                        .shadow(3.dp, CircleShape)
                        .clip(CircleShape)

                    val borderBrush = when {
                        isCurrentDealer -> Brush.sweepGradient(
                            listOf(
                                pal.accent,
                                pal.accentAlt,
                                pal.accent.copy(alpha = 0.75f),
                                pal.accentAlt,
                                pal.accent
                            )
                        )
                        isNextDealer -> Brush.linearGradient(
                            listOf(pal.accentAlt, pal.accent)
                        )
                        else -> Brush.linearGradient(
                            if (pal.isDark) {
                                listOf(pal.tint.copy(alpha = 0.25f), pal.tint.copy(alpha = 0.10f))
                            } else {
                                listOf(pal.accent.copy(alpha = 0.35f), pal.tint.copy(alpha = 0.15f))
                            }
                        )
                    }

                    val borderWidth = if (isCurrentDealer) 2.dp else if (isNextDealer) 1.5.dp else 1.2.dp

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
                                            if (isCurrentDealer) {
                                                pal.accent.copy(alpha = if (pal.isDark) 0.40f else 0.28f)
                                            } else {
                                                if (pal.isDark) pal.surface else pal.cardSurface
                                            },
                                            if (isCurrentDealer) {
                                                pal.surface
                                            } else {
                                                if (pal.isDark) pal.backgroundBottom else pal.tint.copy(alpha = 0.08f)
                                            }
                                        )
                                    )
                                )
                                .border(borderWidth, borderBrush, CircleShape),
                            contentAlignment = Alignment.Center
                        ) {
                            Text(
                                text = player.name.take(1).uppercase(),
                                color = if (isCurrentDealer) pal.accent else pal.textPrimary,
                                fontWeight = FontWeight.Bold,
                                fontSize = 15.sp
                            )
                        }
                    }

                    // Antique Brass / Theme Dealer Coin / Next Coin pinned to avatar bottom-right
                    if (isCurrentDealer) {
                        NepaliDealerButton(
                            size = 18.dp,
                            modifier = Modifier
                                .align(Alignment.BottomEnd)
                                .offset(x = 3.dp, y = 3.dp)
                        )
                    } else if (isNextDealer) {
                        NepaliNextDealerButton(
                            size = 15.dp,
                            modifier = Modifier
                                .align(Alignment.BottomEnd)
                                .offset(x = 2.dp, y = 2.dp)
                        )
                    }
                }
            }

            Spacer(modifier = Modifier.width(5.dp))

            // Handcrafted Theme Circular Seat Number Token
            Box(
                modifier = Modifier
                    .size(17.dp)
                    .background(
                        if (isCurrentDealer) {
                            pal.accent.copy(alpha = if (pal.isDark) 0.35f else 0.25f)
                        } else {
                            if (pal.isDark) pal.surface else pal.tint.copy(alpha = 0.08f)
                        },
                        CircleShape
                    )
                    .border(
                        1.dp,
                        if (isCurrentDealer) pal.accent else pal.tint.copy(alpha = if (pal.isDark) 0.35f else 0.22f),
                        CircleShape
                    ),
                contentAlignment = Alignment.Center
            ) {
                Text(
                    text = "$seatNumber",
                    color = if (isCurrentDealer) pal.accent else pal.textPrimary,
                    fontSize = 9.5.sp,
                    fontWeight = FontWeight.Black,
                    style = TextStyle(
                        platformStyle = PlatformTextStyle(includeFontPadding = false),
                        textAlign = TextAlign.Center,
                        lineHeight = 17.sp
                    )
                )
            }

            Spacer(modifier = Modifier.width(4.dp))

            // Player Name Text
            Text(
                text = player.name,
                color = if (isCurrentDealer) pal.accent else pal.textPrimary,
                fontSize = 11.sp,
                fontWeight = if (isCurrentDealer) FontWeight.Bold else FontWeight.SemiBold,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis,
                modifier = Modifier.widthIn(max = 68.dp)
            )
        }
    }
}
