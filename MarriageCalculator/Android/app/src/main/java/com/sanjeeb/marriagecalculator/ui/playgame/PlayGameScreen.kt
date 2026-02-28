package com.sanjeeb.marriagecalculator.ui.playgame

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.*
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

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun PlayGameScreen(
    gameSetId: Int,
    onAddRound: (Int) -> Unit,
    onViewScoreboard: () -> Unit,
    onBack: () -> Unit
) {
    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Text("Game", color = GoldAccent, fontFamily = FontFamily.Serif, fontWeight = FontWeight.Bold)
                },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, null, tint = GoldAccent)
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = TiharNightBlue)
            )
        },
        containerColor = Color.Transparent
    ) { padding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .background(Brush.verticalGradient(listOf(TiharNightBlue, Color(0xFF0D0D1A))))
                .padding(padding)
        ) {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(24.dp),
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.Center
            ) {
                // Add Round Button
                Button(
                    onClick = { onAddRound(0) },
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(72.dp),
                    shape = RoundedCornerShape(16.dp),
                    colors = ButtonDefaults.buttonColors(containerColor = DeepRedTika)
                ) {
                    Icon(Icons.Default.Add, null, tint = GoldAccent, modifier = Modifier.size(28.dp))
                    Spacer(modifier = Modifier.width(12.dp))
                    Text(
                        "Add Round",
                        color = GoldAccent,
                        fontSize = 20.sp,
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Serif
                    )
                }

                Spacer(modifier = Modifier.height(16.dp))

                // View Scoreboard Button
                OutlinedButton(
                    onClick = onViewScoreboard,
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(72.dp),
                    shape = RoundedCornerShape(16.dp),
                    border = ButtonDefaults.outlinedButtonBorder
                ) {
                    Icon(Icons.Default.Leaderboard, null, tint = GoldAccent, modifier = Modifier.size(28.dp))
                    Spacer(modifier = Modifier.width(12.dp))
                    Text(
                        "Scoreboard",
                        color = GoldAccent,
                        fontSize = 20.sp,
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Serif
                    )
                }
            }
        }
    }
}
