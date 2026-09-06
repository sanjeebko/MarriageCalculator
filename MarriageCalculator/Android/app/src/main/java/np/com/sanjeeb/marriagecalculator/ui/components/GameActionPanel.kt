package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.core.tween
import androidx.compose.animation.expandVertically
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.animation.shrinkVertically
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme

/**
 * Reusable, theme-aware collapsible action panel for the game screen (Issue #51).
 * Displays a compact minimized bar by default to preserve vertical and horizontal screen real estate.
 * When expanded, reveals all secondary game action icons with clear text labels and comfortable touch targets.
 */
@Composable
fun GameActionPanel(
    expanded: Boolean,
    onDismiss: () -> Unit,
    onSettleUpClick: () -> Unit,
    onScoreboardClick: () -> Unit,
    onShareClick: () -> Unit,
    onThemeClick: () -> Unit,
    isHost: Boolean,
    isOnlineMode: Boolean,
    isSettled: Boolean,
    onTransferHostClick: () -> Unit,
    onDeleteGameClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    AnimatedVisibility(
        visible = expanded,
        enter = expandVertically(animationSpec = tween(220)) + fadeIn(animationSpec = tween(220)),
        exit = shrinkVertically(animationSpec = tween(180)) + fadeOut(animationSpec = tween(180)),
        modifier = modifier
    ) {
        Card(
            modifier = Modifier.fillMaxWidth(),
            shape = RoundedCornerShape(14.dp),
            colors = CardDefaults.cardColors(containerColor = AppTheme.palette.cardSurface),
            border = BorderStroke(1.dp, AppTheme.palette.tint.copy(alpha = 0.15f))
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 12.dp, vertical = 8.dp)
            ) {
                // Header Row with Title and Close Button
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(bottom = 6.dp),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Icon(
                            imageVector = Icons.Default.Widgets,
                            contentDescription = null,
                            tint = AppTheme.palette.accent,
                            modifier = Modifier.size(16.dp)
                        )
                        Spacer(modifier = Modifier.width(6.dp))
                        Text(
                            text = "GAME ACTIONS",
                            color = AppTheme.palette.frostAccent,
                            fontSize = 11.sp,
                            fontWeight = FontWeight.Bold,
                            letterSpacing = 1.2.sp
                        )
                    }

                    IconButton(
                        onClick = onDismiss,
                        modifier = Modifier.size(24.dp)
                    ) {
                        Icon(
                            imageVector = Icons.Default.Close,
                            contentDescription = "Close actions",
                            tint = AppTheme.palette.frostAccent.copy(alpha = 0.7f),
                            modifier = Modifier.size(16.dp)
                        )
                    }
                }

                // Action Tiles Row
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 4.dp)
                        .horizontalScroll(rememberScrollState()),
                    horizontalArrangement = Arrangement.SpaceEvenly,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    // Settle Up
                    GameActionTile(
                        icon = Icons.Default.Payments,
                        label = "Settle",
                        tint = AppTheme.palette.accent,
                        onClick = {
                            onDismiss()
                            onSettleUpClick()
                        }
                    )

                    // Scoreboard
                    GameActionTile(
                        icon = Icons.Default.Leaderboard,
                        label = "Scores",
                        tint = AppTheme.palette.accent,
                        onClick = {
                            onDismiss()
                            onScoreboardClick()
                        }
                    )

                    // Share Match Summary
                    GameActionTile(
                        icon = Icons.Default.Share,
                        label = "Share",
                        tint = AppTheme.palette.accent,
                        onClick = {
                            onDismiss()
                            onShareClick()
                        }
                    )

                    // App Theme
                    GameActionTile(
                        icon = Icons.Default.Palette,
                        label = "Theme",
                        tint = AppTheme.palette.accent,
                        onClick = {
                            onDismiss()
                            onThemeClick()
                        }
                    )

                    // Transfer Host (if host & online & not settled)
                    if (isHost && isOnlineMode && !isSettled) {
                        GameActionTile(
                            icon = Icons.Default.SwapHoriz,
                            label = "Host",
                            tint = AppTheme.palette.accent.copy(alpha = 0.4f),
                            enabled = false,
                            subtitle = "Soon",
                            onClick = onTransferHostClick
                        )
                    }

                    // Delete Game (if host)
                    if (isHost) {
                        GameActionTile(
                            icon = Icons.Default.DeleteForever,
                            label = "Delete",
                            tint = Color(0xFFFF5252),
                            isDestructive = true,
                            onClick = {
                                onDismiss()
                                onDeleteGameClick()
                            }
                        )
                    }
                }
            }
        }
    }
}

@Composable
private fun GameActionTile(
    icon: ImageVector,
    label: String,
    tint: Color,
    onClick: () -> Unit,
    enabled: Boolean = true,
    subtitle: String? = null,
    isDestructive: Boolean = false
) {
    Column(
        horizontalAlignment = Alignment.CenterHorizontally,
        modifier = Modifier
            .clip(RoundedCornerShape(10.dp))
            .then(if (enabled) Modifier.clickable(onClick = onClick) else Modifier)
            .padding(horizontal = 4.dp, vertical = 2.dp)
    ) {
        Box(
            modifier = Modifier
                .size(42.dp)
                .clip(CircleShape)
                .background(
                    if (isDestructive) Color(0xFFFF5252).copy(alpha = 0.12f)
                    else AppTheme.palette.tint.copy(alpha = 0.10f)
                )
                .border(
                    BorderStroke(
                        1.dp,
                        if (isDestructive) Color(0xFFFF5252).copy(alpha = 0.4f)
                        else AppTheme.palette.tint.copy(alpha = 0.25f)
                    ),
                    CircleShape
                ),
            contentAlignment = Alignment.Center
        ) {
            Icon(
                imageVector = icon,
                contentDescription = label,
                tint = tint,
                modifier = Modifier.size(20.dp)
            )
        }

        Spacer(modifier = Modifier.height(4.dp))

        Text(
            text = label,
            color = if (isDestructive) Color(0xFFFF5252)
            else if (!enabled) AppTheme.palette.textPrimary.copy(alpha = 0.4f)
            else AppTheme.palette.textPrimary,
            fontSize = 11.sp,
            fontWeight = FontWeight.Medium,
            textAlign = TextAlign.Center,
            maxLines = 1
        )

        if (subtitle != null) {
            Text(
                text = subtitle,
                color = AppTheme.palette.textPrimary.copy(alpha = 0.4f),
                fontSize = 9.sp,
                textAlign = TextAlign.Center
            )
        }
    }
}
