package com.sanjeeb.marriagecalculator.ui.dashboard

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.History
import androidx.compose.material.icons.filled.PlayArrow
import androidx.compose.material.icons.filled.WifiOff
import androidx.compose.material.icons.filled.People
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import com.sanjeeb.marriagecalculator.data.model.MarriageGameSet
import com.sanjeeb.marriagecalculator.ui.theme.DeepRedTika
import com.sanjeeb.marriagecalculator.ui.theme.GoldAccent
import com.sanjeeb.marriagecalculator.ui.theme.MarigoldOrange
import com.sanjeeb.marriagecalculator.ui.theme.TiharNightBlue
import com.sanjeeb.marriagecalculator.ui.components.MetallicButton
import com.sanjeeb.marriagecalculator.ui.components.MetallicRedFace
import com.sanjeeb.marriagecalculator.ui.components.MetallicRedRim

@Composable
fun DashboardScreen(
    onNewGame: () -> Unit,
    onResumeGame: (String) -> Unit,
    onFriends: () -> Unit,
    viewModel: DashboardViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

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
            Spacer(modifier = Modifier.height(4.dp))
            Text(
                text = "Marriage Card Game Calculator",
                color = Color.White.copy(alpha = 0.7f),
                fontSize = 14.sp
            )

            // Offline indicator
            if (uiState.isOfflineMode) {
                Spacer(modifier = Modifier.height(8.dp))
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Icon(
                        Icons.Default.WifiOff,
                        contentDescription = null,
                        tint = MarigoldOrange,
                        modifier = Modifier.size(14.dp)
                    )
                    Spacer(modifier = Modifier.width(4.dp))
                    Text(
                        text = "Offline Mode",
                        color = MarigoldOrange,
                        fontSize = 12.sp
                    )
                }
            }

            Spacer(modifier = Modifier.height(32.dp))

            // New Game Button
            MetallicButton(
                onClick = onNewGame,
                text = "New Game",
                rimColors = MetallicRedRim,
                faceColors = MetallicRedFace,
                textColor = GoldAccent,
                modifier = Modifier.height(72.dp),
                leadingIcon = {
                    Icon(
                        imageVector = Icons.Default.Add,
                        contentDescription = null,
                        tint = GoldAccent,
                        modifier = Modifier.size(28.dp)
                    )
                }
            )

            if (!uiState.isOfflineMode) {
                Spacer(modifier = Modifier.height(16.dp))
                MetallicButton(
                    onClick = onFriends,
                    text = "Friends & Social",
                    rimColors = listOf(GoldAccent, Color(0xFF6A5415)),
                    faceColors = listOf(Color(0xFFFFEA9F), Color(0xFFD4AF37)),
                    textColor = Color(0xFF1E1402),
                    modifier = Modifier.height(60.dp),
                    leadingIcon = {
                        Icon(
                            imageVector = Icons.Default.People,
                            contentDescription = null,
                            tint = Color(0xFF1E1402),
                            modifier = Modifier.size(24.dp)
                        )
                    }
                )
            }

            Spacer(modifier = Modifier.height(24.dp))

            // Active Games Section
            if (uiState.activeGames.isNotEmpty()) {
                Text(
                    text = "Active Games",
                    color = GoldAccent,
                    fontSize = 18.sp,
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Serif,
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(bottom = 12.dp)
                )

                LazyColumn(
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    items(uiState.activeGames) { game ->
                        ActiveGameCard(game = game, onResume = { onResumeGame(game.id) })
                    }
                }
            }

            if (uiState.isLoading) {
                Spacer(modifier = Modifier.height(24.dp))
                CircularProgressIndicator(color = GoldAccent)
            }
        }
    }
}

@Composable
private fun ActiveGameCard(game: MarriageGameSet, onResume: () -> Unit) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onResume() },
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(
            containerColor = Color.White.copy(alpha = 0.08f)
        ),
        border = BorderStroke(1.dp, GoldAccent.copy(alpha = 0.3f))
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = game.name,
                    color = Color.White,
                    fontSize = 16.sp,
                    fontWeight = FontWeight.Bold,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                Text(
                    text = "Last played: ${game.lastPlayed.take(10)}",
                    color = Color.White.copy(alpha = 0.5f),
                    fontSize = 12.sp
                )
            }
            Icon(
                Icons.Default.PlayArrow,
                contentDescription = "Resume",
                tint = GoldAccent,
                modifier = Modifier.size(32.dp)
            )
        }
    }
}
