package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.text.PlatformTextStyle
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.LineHeightStyle
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme

/**
 * Small circular "D" marker shown next to the dealer's name/score.
 * Disables Android's default font padding and centers the line height —
 * at badge sizes (12-14dp) the padding otherwise pushes the glyph
 * visibly below the circle's center.
 */
@Composable
fun DealerBadge(size: Dp = 14.dp) {
    Box(
        modifier = Modifier
            .size(size)
            .clip(CircleShape)
            .background(AppTheme.palette.frostAccent.copy(alpha = 0.22f)),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = "D",
            style = TextStyle(
                color = AppTheme.palette.frostAccent,
                fontSize = if (size < 14.dp) 8.sp else 9.sp,
                fontWeight = FontWeight.Bold,
                platformStyle = PlatformTextStyle(includeFontPadding = false),
                lineHeightStyle = LineHeightStyle(
                    alignment = LineHeightStyle.Alignment.Center,
                    trim = LineHeightStyle.Trim.Both
                )
            )
        )
    }
}
