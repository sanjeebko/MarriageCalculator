package np.com.sanjeeb.marriagecalculator.ui.components

import android.content.Context
import android.content.Intent
import android.net.Uri
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.OpenInNew
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog
import androidx.compose.ui.window.DialogProperties
import np.com.sanjeeb.marriagecalculator.R
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme
import np.com.sanjeeb.marriagecalculator.ui.theme.AppThemeOption

data class DeveloperAppInfo(
    val title: String,
    val description: String,
    val category: String,
    val url: String,
    val iconVector: ImageVector? = null,
    val iconDrawableRes: Int? = null,
    val isCurrentApp: Boolean = false
)

/**
 * More Apps dialog displaying the developer's portfolio of apps and games
 * with theme-matching background and direct external links.
 */
@Composable
fun MoreAppsDialog(
    onDismiss: () -> Unit
) {
    val context = LocalContext.current
    val palette = AppTheme.palette

    // Theme-specific background artwork
    val bgDrawableId = when (palette) {
        AppThemeOption.BLACK_AND_WHITE.palette -> R.drawable.history_bg_monochrome
        AppThemeOption.TIHAR_NIGHT.palette -> R.drawable.history_bg_dashain
        AppThemeOption.HIGH_CONTRAST_DARK.palette -> R.drawable.history_bg_contrast
        AppThemeOption.MIDNIGHT_FROST.palette -> R.drawable.history_bg_frost
        AppThemeOption.MARIGOLD_DAY.palette -> R.drawable.history_bg_marigold
        AppThemeOption.HIMALAYAN_MIST.palette -> R.drawable.history_bg_mist
        else -> R.drawable.history_bg_frost
    }

    val apps = listOf(
        DeveloperAppInfo(
            title = "AAA Marriage Calculator",
            description = "Digital scorer and calculator for the 21-card Marriage game.",
            category = "Card Game Utility",
            url = "https://sanjeebojha.com.np/apps/marriage-calculator",
            iconDrawableRes = R.drawable.ic_moreapps_marriage,
            isCurrentApp = true
        ),
        DeveloperAppInfo(
            title = "Dice Roller",
            description = "3D physics-based multi-dice roller for tabletop and board games.",
            category = "Board Game Utility",
            url = "https://sanjeebojha.com.np/apps/dice-roller",
            iconDrawableRes = R.drawable.ic_moreapps_diceroller
        ),
        DeveloperAppInfo(
            title = "Word Duel",
            description = "Real-time competitive word puzzle challenge for Android.",
            category = "Word & Puzzle Game",
            url = "https://wordduel.sanjeebojha.com.np/android",
            iconDrawableRes = R.drawable.ic_moreapps_wordduel
        ),
        DeveloperAppInfo(
            title = "Five Words",
            description = "Daily 5-letter word guess puzzle on the Google Play Store.",
            category = "Play Store Game",
            url = "https://play.google.com/store/apps/details?id=com.sanjeebojha.fivewords&hl=en-US",
            iconDrawableRes = R.drawable.ic_moreapps_fivewords
        )
    )

    Dialog(
        onDismissRequest = onDismiss,
        properties = DialogProperties(usePlatformDefaultWidth = false)
    ) {
        Surface(
            modifier = Modifier
                .fillMaxWidth(0.92f)
                .fillMaxHeight(0.85f)
                .clip(RoundedCornerShape(24.dp)),
            shape = RoundedCornerShape(24.dp),
            border = BorderStroke(1.dp, palette.accent.copy(alpha = 0.35f)),
            color = palette.surface
        ) {
            Box(modifier = Modifier.fillMaxSize()) {
                // 1. Theme-Specific Background Image
                Image(
                    painter = painterResource(id = bgDrawableId),
                    contentDescription = null,
                    contentScale = ContentScale.Crop,
                    modifier = Modifier.fillMaxSize()
                )

                // 2. Translucent Frosted Gradient Scrim
                val scrimBrush = Brush.verticalGradient(
                    colors = if (palette.isDark) {
                        listOf(
                            palette.backgroundTop.copy(alpha = 0.90f),
                            palette.surface.copy(alpha = 0.85f),
                            palette.backgroundBottom.copy(alpha = 0.94f)
                        )
                    } else {
                        listOf(
                            palette.backgroundTop.copy(alpha = 0.92f),
                            palette.surface.copy(alpha = 0.88f),
                            palette.backgroundBottom.copy(alpha = 0.96f)
                        )
                    }
                )
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .background(scrimBrush)
                )

                // 3. Content
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(horizontal = 20.dp, vertical = 20.dp)
                ) {
                    // Header: Title & Close Button
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Icon(
                                imageVector = Icons.Default.Apps,
                                contentDescription = null,
                                tint = palette.accent,
                                modifier = Modifier.size(24.dp)
                            )
                            Spacer(modifier = Modifier.width(8.dp))
                            Text(
                                text = "More Apps",
                                color = palette.accent,
                                fontSize = 20.sp,
                                fontWeight = FontWeight.Bold,
                                fontFamily = FontFamily.Serif
                            )
                        }

                        IconButton(
                            onClick = onDismiss,
                            modifier = Modifier.size(32.dp)
                        ) {
                            Icon(
                                imageVector = Icons.Default.Close,
                                contentDescription = "Close",
                                tint = palette.accent,
                                modifier = Modifier.size(20.dp)
                            )
                        }
                    }

                    Text(
                        text = "Discover more apps and games by Sanjeeb Ojha",
                        color = palette.textPrimary.copy(alpha = 0.70f),
                        fontSize = 12.sp,
                        modifier = Modifier.padding(top = 2.dp, bottom = 14.dp)
                    )

                    // App Cards List
                    LazyColumn(
                        modifier = Modifier.weight(1f),
                        verticalArrangement = Arrangement.spacedBy(10.dp)
                    ) {
                        items(apps, key = { it.title }) { app ->
                            AppCardItem(app = app, context = context)
                        }
                    }

                    Spacer(modifier = Modifier.height(14.dp))

                    // Close Button
                    GlassButton(
                        onClick = onDismiss,
                        text = "Close",
                        containerColor = palette.cta.copy(alpha = 0.35f),
                        textColor = palette.accent,
                        height = 42,
                        modifier = Modifier.fillMaxWidth()
                    )
                }
            }
        }
    }
}

@Composable
private fun AppCardItem(
    app: DeveloperAppInfo,
    context: Context
) {
    val palette = AppTheme.palette

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clip(RoundedCornerShape(14.dp))
            .clickable {
                try {
                    val intent = Intent(Intent.ACTION_VIEW, Uri.parse(app.url))
                    context.startActivity(intent)
                } catch (_: Exception) { }
            },
        shape = RoundedCornerShape(14.dp),
        colors = CardDefaults.cardColors(
            containerColor = palette.cardSurface.copy(alpha = 0.85f)
        ),
        border = BorderStroke(
            1.dp,
            if (app.isCurrentApp) palette.accent.copy(alpha = 0.40f)
            else palette.tint.copy(alpha = 0.18f)
        )
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            // App Icon
            Surface(
                modifier = Modifier
                    .size(46.dp)
                    .clip(RoundedCornerShape(10.dp)),
                shape = RoundedCornerShape(10.dp),
                color = palette.surface,
                border = BorderStroke(1.dp, palette.accent.copy(alpha = 0.25f))
            ) {
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    if (app.iconDrawableRes != null) {
                        Image(
                            painter = painterResource(id = app.iconDrawableRes),
                            contentDescription = app.title,
                            contentScale = ContentScale.Crop,
                            modifier = Modifier.fillMaxSize()
                        )
                    } else if (app.iconVector != null) {
                        Icon(
                            imageVector = app.iconVector,
                            contentDescription = app.title,
                            tint = palette.accent,
                            modifier = Modifier.size(24.dp)
                        )
                    }
                }
            }

            Spacer(modifier = Modifier.width(12.dp))

            // Details
            Column(modifier = Modifier.weight(1f)) {
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(6.dp)
                ) {
                    Text(
                        text = app.title,
                        color = palette.textPrimary,
                        fontSize = 14.sp,
                        fontWeight = FontWeight.Bold
                    )
                    if (app.isCurrentApp) {
                        Surface(
                            color = palette.accent.copy(alpha = 0.18f),
                            shape = RoundedCornerShape(4.dp)
                        ) {
                            Text(
                                text = "Current",
                                color = palette.accent,
                                fontSize = 9.sp,
                                fontWeight = FontWeight.Bold,
                                modifier = Modifier.padding(horizontal = 4.dp, vertical = 1.dp)
                            )
                        }
                    }
                }

                Spacer(modifier = Modifier.height(2.dp))

                Text(
                    text = app.description,
                    color = palette.textPrimary.copy(alpha = 0.70f),
                    fontSize = 11.sp,
                    lineHeight = 14.sp
                )

                Spacer(modifier = Modifier.height(4.dp))

                Text(
                    text = app.category,
                    color = palette.accentAlt,
                    fontSize = 10.sp,
                    fontWeight = FontWeight.Medium
                )
            }

            Spacer(modifier = Modifier.width(8.dp))

            // External link button
            Surface(
                modifier = Modifier
                    .size(34.dp)
                    .clip(RoundedCornerShape(8.dp)),
                shape = RoundedCornerShape(8.dp),
                color = palette.accent.copy(alpha = 0.12f),
                border = BorderStroke(0.8.dp, palette.accent.copy(alpha = 0.3f))
            ) {
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    Icon(
                        imageVector = Icons.AutoMirrored.Filled.OpenInNew,
                        contentDescription = "Open ${app.title}",
                        tint = palette.accent,
                        modifier = Modifier.size(16.dp)
                    )
                }
            }
        }
    }
}
