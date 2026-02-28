package com.sanjeeb.marriagecalculator.ui.splash

import androidx.compose.animation.core.*
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.scale
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.sanjeeb.marriagecalculator.ui.theme.DeepRedTika
import com.sanjeeb.marriagecalculator.ui.theme.GoldAccent
import com.sanjeeb.marriagecalculator.ui.theme.MarigoldOrange
import com.sanjeeb.marriagecalculator.ui.theme.TiharNightBlue
import kotlinx.coroutines.delay

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
    val titleAlpha by animateFloatAsState(
        targetValue = if (startAnimation) 1f else 0f,
        animationSpec = tween(800, delayMillis = 400),
        label = "titleAlpha"
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
                        DeepRedTika.copy(alpha = 0.3f),
                        TiharNightBlue,
                        Color(0xFF0D0D1A)
                    ),
                    radius = 1200f
                )
            ),
        contentAlignment = Alignment.Center
    ) {
        Column(
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            // Card icon circle
            Box(
                modifier = Modifier
                    .size(120.dp)
                    .scale(logoScale)
                    .alpha(logoAlpha)
                    .clip(CircleShape)
                    .background(
                        Brush.linearGradient(listOf(DeepRedTika, MarigoldOrange))
                    ),
                contentAlignment = Alignment.Center
            ) {
                Text(
                    "♠♥\n♦♣",
                    color = GoldAccent,
                    fontSize = 32.sp,
                    fontWeight = FontWeight.Bold,
                    textAlign = TextAlign.Center,
                    lineHeight = 36.sp
                )
            }

            Spacer(modifier = Modifier.height(24.dp))

            // App title
            Text(
                "Marriage",
                modifier = Modifier.alpha(titleAlpha),
                color = GoldAccent,
                fontSize = 36.sp,
                fontWeight = FontWeight.Bold,
                fontFamily = FontFamily.Serif,
                letterSpacing = 2.sp
            )
            Text(
                "Calculator",
                modifier = Modifier.alpha(titleAlpha),
                color = MarigoldOrange,
                fontSize = 28.sp,
                fontWeight = FontWeight.Light,
                fontFamily = FontFamily.Serif,
                letterSpacing = 4.sp
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
