package com.sanjeeb.marriagecalculator.ui.dashboard

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.History
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.sanjeeb.marriagecalculator.ui.theme.DeepRedTika
import com.sanjeeb.marriagecalculator.ui.theme.GoldAccent
import com.sanjeeb.marriagecalculator.ui.theme.MarigoldOrange
import com.sanjeeb.marriagecalculator.ui.theme.TiharNightBlue

@Composable
fun DashboardScreen(
    onNewGame: () -> Unit,
    onResumeGame: (Int) -> Unit
) {
    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(
                Brush.verticalGradient(
                    colors = listOf(TiharNightBlue, Color(0xFF0D0D1A))
                )
            )
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(24.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Spacer(modifier = Modifier.height(48.dp))

            // Greeting
            Text(
                text = "नमस्ते!",
                color = GoldAccent,
                fontSize = 36.sp,
                fontWeight = FontWeight.Bold,
                fontFamily = FontFamily.Serif
            )
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = "Marriage Card Game Calculator",
                color = Color.White.copy(alpha = 0.7f),
                fontSize = 14.sp
            )

            Spacer(modifier = Modifier.height(48.dp))

            // New Game Button
            Button(
                onClick = onNewGame,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(72.dp),
                shape = RoundedCornerShape(16.dp),
                colors = ButtonDefaults.buttonColors(containerColor = DeepRedTika)
            ) {
                Icon(
                    imageVector = Icons.Default.Add,
                    contentDescription = null,
                    tint = GoldAccent,
                    modifier = Modifier.size(28.dp)
                )
                Spacer(modifier = Modifier.width(12.dp))
                Text(
                    text = "New Game",
                    color = GoldAccent,
                    fontSize = 20.sp,
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Serif
                )
            }

            Spacer(modifier = Modifier.height(16.dp))

            // Resume / History Button
            OutlinedButton(
                onClick = { /* TODO: show game list */ },
                modifier = Modifier
                    .fillMaxWidth()
                    .height(72.dp),
                shape = RoundedCornerShape(16.dp),
                border = BorderStroke(
                    width = 2.dp,
                    brush = Brush.linearGradient(listOf(GoldAccent, MarigoldOrange))
                )
            ) {
                Icon(
                    imageVector = Icons.Default.History,
                    contentDescription = null,
                    tint = GoldAccent,
                    modifier = Modifier.size(28.dp)
                )
                Spacer(modifier = Modifier.width(12.dp))
                Text(
                    text = "Resume Game",
                    color = GoldAccent,
                    fontSize = 20.sp,
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Serif
                )
            }
        }
    }
}
