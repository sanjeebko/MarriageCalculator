package np.com.sanjeeb.marriagecalculator.ui.splash

import androidx.compose.animation.core.*
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.draw.scale
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import kotlinx.coroutines.delay

import androidx.compose.foundation.Image
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.layout.ContentScale
import np.com.sanjeeb.marriagecalculator.R

@Composable
fun SplashScreen(onSplashComplete: () -> Unit) {
    var startAnimation by remember { mutableStateOf(false) }

    val logoScale by animateFloatAsState(
        targetValue = if (startAnimation) 1f else 0.3f,
        animationSpec = tween(800, easing = FastOutSlowInEasing),
        label = "logoScale"
    )
    val logoAlpha by animateFloatAsState(
        targetValue = if (startAnimation) 1f else 0f,
        animationSpec = tween(600),
        label = "logoAlpha"
    )
    val subtitleAlpha by animateFloatAsState(
        targetValue = if (startAnimation) 1f else 0f,
        animationSpec = tween(800, delayMillis = 800),
        label = "subtitleAlpha"
    )

    LaunchedEffect(Unit) {
        startAnimation = true
        delay(2200)
        onSplashComplete()
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(
                Brush.radialGradient(
                    colors = listOf(
                        Color(0xFF22252E).copy(alpha = 0.45f),
                        Color(0xFF0D0E12),
                        Color(0xFF050507)
                    ),
                    radius = 1200f
                )
            ),
        contentAlignment = Alignment.Center
    ) {
        // Faint card background overlay
        Image(
            painter = painterResource(id = R.drawable.card_bg_pattern),
            contentDescription = null,
            contentScale = ContentScale.Crop,
            modifier = Modifier.fillMaxSize(),
            alpha = 0.05f
        )

        Column(
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Image(
                painter = painterResource(id = R.drawable.marriage_logo_title),
                contentDescription = "Marriage Calculator",
                contentScale = ContentScale.FillWidth,
                modifier = Modifier
                    .fillMaxWidth()
                    .scale(1.5f * logoScale)
                    .alpha(logoAlpha)
            )

            Spacer(modifier = Modifier.height(12.dp))

            Text(
                "Track • Score • Win",
                modifier = Modifier.alpha(subtitleAlpha),
                color = Color.White.copy(alpha = 0.5f),
                fontSize = 14.sp,
                letterSpacing = 3.sp
            )
        }

        // Version at bottom
        Text(
            "v1.0",
            modifier = Modifier
                .align(Alignment.BottomCenter)
                .padding(bottom = 32.dp)
                .alpha(subtitleAlpha),
            color = Color.White.copy(alpha = 0.3f),
            fontSize = 12.sp
        )
    }
}
