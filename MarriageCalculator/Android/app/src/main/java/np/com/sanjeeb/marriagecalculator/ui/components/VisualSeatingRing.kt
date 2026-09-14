package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.animation.scaleIn
import androidx.compose.animation.scaleOut
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
import androidx.compose.foundation.interaction.MutableInteractionSource
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
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.SwapHoriz
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
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

    var selectedPlayerId by remember { mutableStateOf<String?>(null) }

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
            // Sized with comfortable outer margin to prevent avatar clipping on the perimeter
            val availableWidthDp = totalWidth / density
            val targetTableWidthDp = (availableWidthDp - 96f).coerceIn(210f, 250f)
            val tableWidth = targetTableWidthDp * density
            val tableHeight = tableWidth / 1.6238f

            // Player orbit radii: positioned cleanly and comfortably OUTSIDE the carved wood rim
            val radiusX = (tableWidth / 2f) + (22f * density)
            val radiusY = (tableHeight / 2f) + (24f * density)
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
                    modifier = Modifier
                        .fillMaxSize()
                        .clickable(
                            interactionSource = remember { MutableInteractionSource() },
                            indication = null
                        ) {
                            selectedPlayerId = null
                        }
                )
            }

            // 2. Players Ring seated cleanly OUTSIDE around the table as compact circular nodes
            players.forEachIndexed { index, player ->
                val (offsetX, offsetY) = calculateSeatOffset(
                    index = index,
                    totalPlayers = players.size,
                    radiusX = radiusX,
                    radiusY = radiusY
                )

                val isCurrentDealer = player.id == currentDealerId
                val isNextDealer = player.id == effectiveNextDealerId && !isCurrentDealer
                val isSelected = player.id == selectedPlayerId

                Box(
                    modifier = Modifier
                        .offset { IntOffset(offsetX.roundToInt(), (offsetY + verticalCenterShift).roundToInt()) }
                        .zIndex(if (isSelected) 5f else if (isCurrentDealer) 3f else if (isNextDealer) 2f else 1f),
                    contentAlignment = Alignment.Center
                ) {
                    PlayerSeatCircle(
                        player = player,
                        seatNumber = index + 1,
                        isCurrentDealer = isCurrentDealer,
                        isNextDealer = isNextDealer,
                        isSelected = isSelected,
                        dealerGlowScale = dealerGlowScale,
                        onClick = {
                            selectedPlayerId = if (selectedPlayerId == player.id) null else player.id
                            onPlayerClick?.invoke(player)
                        }
                    )
                }
            }

            // 3. Floating info panel in the center of the board when an avatar is tapped
            val selectedIndex = players.indexOfFirst { it.id == selectedPlayerId }
            val selectedPlayer = if (selectedIndex >= 0) players[selectedIndex] else null

            Column(
                modifier = Modifier
                    .offset { IntOffset(0, verticalCenterShift.roundToInt()) }
                    .zIndex(10f),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                AnimatedVisibility(
                    visible = selectedPlayer != null,
                    enter = fadeIn(animationSpec = tween(200)) + scaleIn(initialScale = 0.85f, animationSpec = tween(200)),
                    exit = fadeOut(animationSpec = tween(150)) + scaleOut(targetScale = 0.85f, animationSpec = tween(150))
                ) {
                    if (selectedPlayer != null) {
                        CenterPlayerInfoPanel(
                            player = selectedPlayer,
                            seatNumber = selectedIndex + 1,
                            isCurrentDealer = selectedPlayer.id == currentDealerId,
                            isNextDealer = selectedPlayer.id == effectiveNextDealerId && selectedPlayer.id != currentDealerId,
                            onDismiss = { selectedPlayerId = null }
                        )
                    }
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
 * Compact circular player avatar node on the table perimeter.
 * Shows only the profile photo (or initial) with a circular border.
 * The seat/order number token is pinned over the top-start corner,
 * and the dealer button is pinned over the bottom-end corner.
 */
@Composable
private fun PlayerSeatCircle(
    player: Player,
    seatNumber: Int,
    isCurrentDealer: Boolean,
    isNextDealer: Boolean,
    isSelected: Boolean,
    dealerGlowScale: Float,
    onClick: () -> Unit
) {
    val pal = AppTheme.palette

    Box(
        modifier = Modifier
            .size(50.dp)
            .clickable(
                interactionSource = remember { MutableInteractionSource() },
                indication = null,
                onClick = onClick
            ),
        contentAlignment = Alignment.Center
    ) {
        // 1. Dealer Animated Pulsing Halo (when current dealer)
        if (isCurrentDealer) {
            Box(
                modifier = Modifier
                    .size(48.dp)
                    .graphicsLayer {
                        scaleX = dealerGlowScale
                        scaleY = dealerGlowScale
                    }
                    .background(
                        Brush.radialGradient(
                            listOf(
                                pal.accent.copy(alpha = 0.50f),
                                pal.accentAlt.copy(alpha = 0.20f),
                                Color.Transparent
                            )
                        ),
                        shape = CircleShape
                    )
            )
        } else if (isNextDealer) {
            Box(
                modifier = Modifier
                    .size(44.dp)
                    .background(
                        Brush.radialGradient(
                            listOf(
                                pal.accentAlt.copy(alpha = 0.30f),
                                Color.Transparent
                            )
                        ),
                        shape = CircleShape
                    )
            )
        }

        // 2. Selected Player Outer Ring (active focus)
        if (isSelected) {
            Box(
                modifier = Modifier
                    .size(47.dp)
                    .border(
                        width = 2.dp,
                        brush = Brush.sweepGradient(
                            listOf(
                                pal.accent,
                                pal.accentAlt,
                                pal.accent
                            )
                        ),
                        shape = CircleShape
                    )
            )
        }

        // 3. Avatar Container (Photo or Initial): 40dp x 40dp
        Box(
            modifier = Modifier.size(40.dp),
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
                isSelected -> Brush.sweepGradient(
                    listOf(
                        pal.accent,
                        pal.accentAlt,
                        pal.accent
                    )
                )
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
                        listOf(pal.tint.copy(alpha = 0.40f), pal.tint.copy(alpha = 0.18f))
                    } else {
                        listOf(pal.accent.copy(alpha = 0.45f), pal.tint.copy(alpha = 0.20f))
                    }
                )
            }

            val borderWidth = if (isSelected) 2.2.dp else if (isCurrentDealer) 2.dp else if (isNextDealer) 1.5.dp else 1.2.dp

            if (model != null) {
                AsyncImage(
                    model = ImageRequest.Builder(LocalContext.current)
                        .data(model)
                        .crossfade(true)
                        .build(),
                    contentDescription = player.name,
                    contentScale = ContentScale.Crop,
                    modifier = avatarModifier.border(borderWidth, borderBrush, CircleShape)
                )
            } else {
                Box(
                    modifier = avatarModifier
                        .background(
                            Brush.radialGradient(
                                listOf(
                                    if (isCurrentDealer || isSelected) {
                                        pal.accent.copy(alpha = if (pal.isDark) 0.40f else 0.28f)
                                    } else {
                                        if (pal.isDark) pal.surface else pal.cardSurface
                                    },
                                    if (isCurrentDealer || isSelected) {
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
                        color = if (isCurrentDealer || isSelected) pal.accent else pal.textPrimary,
                        fontWeight = FontWeight.Bold,
                        fontSize = 16.sp
                    )
                }
            }
        }

        // 4. Circular Seat/Order Number Token pinned to top-start over profile avatar
        Box(
            modifier = Modifier
                .size(17.dp)
                .align(Alignment.TopStart)
                .offset(x = 1.dp, y = 1.dp)
                .shadow(2.dp, CircleShape)
                .background(
                    if (isCurrentDealer || isSelected) {
                        pal.accent.copy(alpha = if (pal.isDark) 0.85f else 0.80f)
                    } else {
                        if (pal.isDark) pal.surface.copy(alpha = 0.95f) else pal.cardSurface
                    },
                    CircleShape
                )
                .border(
                    1.dp,
                    if (isCurrentDealer || isSelected) pal.accent else pal.tint.copy(alpha = if (pal.isDark) 0.50f else 0.35f),
                    CircleShape
                ),
            contentAlignment = Alignment.Center
        ) {
            Text(
                text = "$seatNumber",
                color = if (isCurrentDealer || isSelected) {
                    if (pal.isDark) Color.Black else Color.White
                } else {
                    pal.textPrimary
                },
                fontSize = 9.5.sp,
                fontWeight = FontWeight.Black,
                style = TextStyle(
                    platformStyle = PlatformTextStyle(includeFontPadding = false),
                    textAlign = TextAlign.Center,
                    lineHeight = 17.sp
                )
            )
        }

        // 5. Dealer Button / Next Dealer Coin pinned to bottom-end over profile avatar
        if (isCurrentDealer) {
            NepaliDealerButton(
                size = 17.dp,
                modifier = Modifier
                    .align(Alignment.BottomEnd)
                    .offset(x = (-1).dp, y = (-1).dp)
            )
        } else if (isNextDealer) {
            NepaliNextDealerButton(
                size = 15.dp,
                modifier = Modifier
                    .align(Alignment.BottomEnd)
                    .offset(x = (-1).dp, y = (-1).dp)
            )
        }
    }
}

/**
 * Center floating information card displayed when a player's circular avatar is tapped.
 * Displays the player's full name, seat order, email/account info, and dealer status.
 */
@Composable
private fun CenterPlayerInfoPanel(
    player: Player,
    seatNumber: Int,
    isCurrentDealer: Boolean,
    isNextDealer: Boolean,
    onDismiss: () -> Unit
) {
    val pal = AppTheme.palette
    val panelShape = RoundedCornerShape(14.dp)

    Box(
        modifier = Modifier
            .widthIn(min = 140.dp, max = 200.dp)
            .shadow(
                elevation = 10.dp,
                shape = panelShape,
                ambientColor = if (pal.isDark) Color.Black else Color(0x40000000),
                spotColor = if (pal.isDark) Color.Black else Color(0x30000000)
            )
            .clip(panelShape)
            .background(
                if (pal.isDark) pal.surface.copy(alpha = 0.95f) else pal.surface.copy(alpha = 0.98f)
            )
            .border(
                width = 1.5.dp,
                brush = Brush.linearGradient(
                    listOf(
                        pal.accent.copy(alpha = 0.70f),
                        pal.accentAlt.copy(alpha = 0.40f)
                    )
                ),
                shape = panelShape
            )
            .padding(horizontal = 12.dp, vertical = 10.dp)
    ) {
        Column(
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.spacedBy(4.dp)
        ) {
            // Header: Seat pill on the left, Close icon on the right
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Box(
                    modifier = Modifier
                        .background(pal.accent.copy(alpha = 0.15f), RoundedCornerShape(6.dp))
                        .border(0.8.dp, pal.accent.copy(alpha = 0.4f), RoundedCornerShape(6.dp))
                        .padding(horizontal = 6.dp, vertical = 2.dp)
                ) {
                    Text(
                        text = "Seat #$seatNumber",
                        fontSize = 10.sp,
                        fontWeight = FontWeight.Bold,
                        color = pal.accent
                    )
                }

                Box(
                    modifier = Modifier
                        .size(20.dp)
                        .clip(CircleShape)
                        .background(pal.tint.copy(alpha = 0.10f))
                        .clickable(onClick = onDismiss),
                    contentAlignment = Alignment.Center
                ) {
                    Icon(
                        imageVector = Icons.Filled.Close,
                        contentDescription = "Close",
                        tint = pal.textPrimary.copy(alpha = 0.6f),
                        modifier = Modifier.size(13.dp)
                    )
                }
            }

            // Player Full Name
            Text(
                text = player.name,
                fontWeight = FontWeight.Bold,
                fontSize = 14.sp,
                color = pal.textPrimary,
                textAlign = TextAlign.Center,
                maxLines = 2,
                overflow = TextOverflow.Ellipsis
            )

            // Email (if available)
            if (!player.email.isNullOrBlank()) {
                Text(
                    text = player.email,
                    fontSize = 10.5.sp,
                    color = pal.textPrimary.copy(alpha = 0.65f),
                    textAlign = TextAlign.Center,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
            }

            // Dealer Status Badge
            if (isCurrentDealer) {
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(4.dp),
                    modifier = Modifier
                        .background(pal.accent.copy(alpha = 0.18f), RoundedCornerShape(10.dp))
                        .border(1.dp, pal.accent.copy(alpha = 0.5f), RoundedCornerShape(10.dp))
                        .padding(horizontal = 8.dp, vertical = 2.dp)
                ) {
                    NepaliDealerButton(size = 14.dp)
                    Text(
                        text = "Dealer",
                        fontSize = 11.sp,
                        fontWeight = FontWeight.Bold,
                        color = pal.accent
                    )
                }
            } else if (isNextDealer) {
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(4.dp),
                    modifier = Modifier
                        .background(pal.accentAlt.copy(alpha = 0.15f), RoundedCornerShape(10.dp))
                        .border(1.dp, pal.accentAlt.copy(alpha = 0.4f), RoundedCornerShape(10.dp))
                        .padding(horizontal = 8.dp, vertical = 2.dp)
                ) {
                    NepaliNextDealerButton(size = 13.dp)
                    Text(
                        text = "Next Dealer",
                        fontSize = 11.sp,
                        fontWeight = FontWeight.SemiBold,
                        color = pal.accentAlt
                    )
                }
            }
        }
    }
}
