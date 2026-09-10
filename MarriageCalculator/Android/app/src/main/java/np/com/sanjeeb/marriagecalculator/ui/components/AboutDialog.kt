package np.com.sanjeeb.marriagecalculator.ui.components

import android.content.Context
import android.content.Intent
import android.net.Uri
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material.icons.automirrored.filled.OpenInNew
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
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

/**
 * About Dialog displaying app branding, version info, game overview,
 * developer attribution, and links to Privacy Policy and Terms of Service.
 * Features a theme-specific background image matching the current active theme.
 */
@Composable
fun AboutDialog(
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

                // 2. Translucent Frosted Gradient Scrim for Readability
                val scrimBrush = Brush.verticalGradient(
                    colors = if (palette.isDark) {
                        listOf(
                            palette.backgroundTop.copy(alpha = 0.88f),
                            palette.surface.copy(alpha = 0.82f),
                            palette.backgroundBottom.copy(alpha = 0.92f)
                        )
                    } else {
                        listOf(
                            palette.backgroundTop.copy(alpha = 0.92f),
                            palette.surface.copy(alpha = 0.88f),
                            palette.backgroundBottom.copy(alpha = 0.95f)
                        )
                    }
                )
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .background(scrimBrush)
                )

                // 3. Dialog Content
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(horizontal = 20.dp, vertical = 20.dp)
                        .verticalScroll(rememberScrollState()),
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    // Close button top right
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.End
                    ) {
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

                    // App Icon
                    Surface(
                        modifier = Modifier
                            .size(72.dp)
                            .clip(RoundedCornerShape(18.dp)),
                        shape = RoundedCornerShape(18.dp),
                        border = BorderStroke(1.5.dp, palette.accent.copy(alpha = 0.5f)),
                        shadowElevation = 6.dp
                    ) {
                        Image(
                            painter = painterResource(id = R.drawable.app_icon),
                            contentDescription = "App Icon",
                            contentScale = ContentScale.Crop,
                            modifier = Modifier.fillMaxSize()
                        )
                    }

                    Spacer(modifier = Modifier.height(12.dp))

                    // Title
                    Text(
                        text = "AAA Marriage Calculator",
                        color = palette.accent,
                        fontSize = 20.sp,
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Serif,
                        textAlign = TextAlign.Center
                    )

                    Spacer(modifier = Modifier.height(4.dp))

                    // Version Tag
                    Surface(
                        color = palette.accent.copy(alpha = 0.15f),
                        shape = RoundedCornerShape(12.dp),
                        border = BorderStroke(0.8.dp, palette.accent.copy(alpha = 0.3f))
                    ) {
                        Text(
                            text = "v1.0.0 · Production Release",
                            color = palette.accent,
                            fontSize = 11.sp,
                            fontWeight = FontWeight.SemiBold,
                            modifier = Modifier.padding(horizontal = 10.dp, vertical = 3.dp)
                        )
                    }

                    Spacer(modifier = Modifier.height(6.dp))

                    // Subtitle
                    Text(
                        text = "Digital Scorer & Tracker for the 21-Card Marriage Game",
                        color = palette.textPrimary.copy(alpha = 0.75f),
                        fontSize = 12.sp,
                        textAlign = TextAlign.Center,
                        lineHeight = 16.sp
                    )

                    Spacer(modifier = Modifier.height(16.dp))

                    // Game Features Card
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        shape = RoundedCornerShape(14.dp),
                        colors = CardDefaults.cardColors(containerColor = palette.cardSurface.copy(alpha = 0.75f)),
                        border = BorderStroke(1.dp, palette.tint.copy(alpha = 0.15f))
                    ) {
                        Column(modifier = Modifier.padding(14.dp)) {
                            Text(
                                text = "KEY FEATURES",
                                color = palette.accent,
                                fontSize = 11.sp,
                                fontWeight = FontWeight.Bold,
                                letterSpacing = 1.2.sp
                            )
                            Spacer(modifier = Modifier.height(8.dp))

                            FeatureItem("Central Collection Scoring", "Winner collects penalties and distributes Maal.")
                            FeatureItem("Variations Supported", "Normal, Kidnap, and Murder game rules.")
                            FeatureItem("Comprehensive Maal Engine", "Tiplu, Poplu, Jhiplu, Alter, Jokers, and Tunnelas.")
                            FeatureItem("Dublee Rules", "+5 bonus for Dublee wins, zero penalty for seen losers.")
                            FeatureItem("Dealer Rotation & Seating", "Circular table arrangement and dealer tracking.")
                            FeatureItem("100% Offline Capable", "All games and histories stored safely on your device.")
                        }
                    }

                    Spacer(modifier = Modifier.height(16.dp))

                    // Legal Links Section
                    Text(
                        text = "LEGAL & POLICIES",
                        color = palette.accent,
                        fontSize = 11.sp,
                        fontWeight = FontWeight.Bold,
                        letterSpacing = 1.2.sp,
                        modifier = Modifier.fillMaxWidth()
                    )

                    Spacer(modifier = Modifier.height(8.dp))

                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(10.dp)
                    ) {
                        LegalLinkCard(
                            title = "Privacy Policy",
                            icon = Icons.Default.Security,
                            url = "https://sanjeebojha.com.np/privacy-policy/marriage-calculator",
                            modifier = Modifier.weight(1f),
                            context = context
                        )
                        LegalLinkCard(
                            title = "Terms of Service",
                            icon = Icons.Default.Description,
                            url = "https://sanjeebojha.com.np/terms/marriage-calculator",
                            modifier = Modifier.weight(1f),
                            context = context
                        )
                    }

                    Spacer(modifier = Modifier.height(18.dp))

                    // Developer Attribution
                    Column(
                        horizontalAlignment = Alignment.CenterHorizontally,
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text(
                            text = "Created by Sanjeeb Ojha",
                            color = palette.textPrimary.copy(alpha = 0.85f),
                            fontSize = 12.sp,
                            fontWeight = FontWeight.Medium
                        )
                        Spacer(modifier = Modifier.height(2.dp))
                        Text(
                            text = "Made with ❤️ in Nepal",
                            color = palette.accent,
                            fontSize = 11.sp,
                            fontWeight = FontWeight.Bold
                        )
                    }

                    Spacer(modifier = Modifier.height(14.dp))

                    // Close Button
                    GlassButton(
                        onClick = onDismiss,
                        text = "Close",
                        containerColor = palette.cta.copy(alpha = 0.35f),
                        textColor = palette.accent,
                        height = 42,
                        modifier = Modifier.fillMaxWidth(0.6f)
                    )
                }
            }
        }
    }
}

@Composable
private fun FeatureItem(title: String, description: String) {
    val palette = AppTheme.palette
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 3.dp),
        verticalAlignment = Alignment.Top
    ) {
        Icon(
            imageVector = Icons.Default.CheckCircle,
            contentDescription = null,
            tint = palette.accent,
            modifier = Modifier
                .size(14.dp)
                .padding(top = 2.dp)
        )
        Spacer(modifier = Modifier.width(8.dp))
        Column {
            Text(
                text = title,
                color = palette.textPrimary,
                fontSize = 12.sp,
                fontWeight = FontWeight.SemiBold
            )
            Text(
                text = description,
                color = palette.textPrimary.copy(alpha = 0.65f),
                fontSize = 10.sp,
                lineHeight = 13.sp
            )
        }
    }
}

@Composable
private fun LegalLinkCard(
    title: String,
    icon: androidx.compose.ui.graphics.vector.ImageVector,
    url: String,
    modifier: Modifier = Modifier,
    context: Context
) {
    val palette = AppTheme.palette
    Surface(
        modifier = modifier
            .clip(RoundedCornerShape(12.dp))
            .clickable {
                try {
                    val intent = Intent(Intent.ACTION_VIEW, Uri.parse(url))
                    context.startActivity(intent)
                } catch (_: Exception) { }
            },
        color = palette.cardSurface.copy(alpha = 0.85f),
        shape = RoundedCornerShape(12.dp),
        border = BorderStroke(1.dp, palette.accent.copy(alpha = 0.35f))
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 10.dp, vertical = 10.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.Center
        ) {
            Icon(
                imageVector = icon,
                contentDescription = null,
                tint = palette.accent,
                modifier = Modifier.size(16.dp)
            )
            Spacer(modifier = Modifier.width(6.dp))
            Text(
                text = title,
                color = palette.textPrimary,
                fontSize = 11.sp,
                fontWeight = FontWeight.SemiBold
            )
            Spacer(modifier = Modifier.width(4.dp))
            Icon(
                imageVector = Icons.AutoMirrored.Filled.OpenInNew,
                contentDescription = "Open in browser",
                tint = palette.accentAlt,
                modifier = Modifier.size(12.dp)
            )
        }
    }
}
