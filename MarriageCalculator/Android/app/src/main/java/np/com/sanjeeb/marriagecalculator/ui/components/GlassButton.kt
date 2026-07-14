package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

@Composable
fun GlassButton(
    onClick: () -> Unit,
    text: String,
    modifier: Modifier = Modifier,
    containerColor: Color = Color.White.copy(alpha = 0.12f),
    borderColor: Color = Color.White.copy(alpha = 0.25f),
    textColor: Color = Color.White,
    height: Int = 44,
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
            .height(height.dp),
        colors = ButtonDefaults.buttonColors(
            containerColor = Color.Transparent,
            disabledContainerColor = Color.Transparent,
            contentColor = textColor,
            disabledContentColor = textColor.copy(alpha = 0.4f)
        ),
        contentPadding = PaddingValues(0.dp),
        shape = RoundedCornerShape(16.dp)
    ) {
        Box(
            modifier = Modifier
                .fillMaxSize()
                .background(
                    brush = Brush.verticalGradient(
                        colors = listOf(
                            // Clamp: a high-alpha container (e.g. 0.85) would otherwise overflow past 1
                            containerColor.copy(alpha = (containerColor.alpha * 1.5f).coerceAtMost(1f)),
                            containerColor
                        )
                    ),
                    shape = RoundedCornerShape(16.dp)
                )
                .border(
                    width = 1.dp,
                    brush = Brush.verticalGradient(
                        colors = listOf(
                            Color.White.copy(alpha = 0.25f),
                            Color.White.copy(alpha = 0.05f),
                            Color.Black.copy(alpha = 0.1f)
                        )
                    ),
                    shape = RoundedCornerShape(16.dp)
                )
                .clip(RoundedCornerShape(16.dp)),
            contentAlignment = Alignment.Center
        ) {
            // Inner subtle sheen
            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .background(
                        brush = Brush.linearGradient(
                            colors = listOf(
                                Color.White.copy(alpha = 0.05f),
                                Color.Transparent,
                                Color.Transparent,
                                Color.Black.copy(alpha = 0.05f)
                            )
                        )
                    )
            )

            if (isLoading) {
                CircularProgressIndicator(
                    color = textColor,
                    modifier = Modifier.size(20.dp),
                    strokeWidth = 2.dp
                )
            } else {
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.padding(horizontal = 16.dp)
                ) {
                    if (leadingIcon != null) {
                        leadingIcon()
                        Spacer(Modifier.width(10.dp))
                    }
                    Text(
                        text = text,
                        style = MaterialTheme.typography.titleMedium.copy(
                            fontWeight = FontWeight.ExtraBold,
                            fontSize = 14.sp,
                            letterSpacing = 0.2.sp
                        )
                    )
                }
            }
        }
    }
}
