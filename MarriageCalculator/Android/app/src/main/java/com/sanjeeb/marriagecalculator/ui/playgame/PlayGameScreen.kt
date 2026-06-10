package com.sanjeeb.marriagecalculator.ui.playgame

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import coil.compose.AsyncImage
import coil.request.ImageRequest
import com.sanjeeb.marriagecalculator.ui.components.MetallicButton
import com.sanjeeb.marriagecalculator.ui.components.MetallicGoldFace
import com.sanjeeb.marriagecalculator.ui.components.MetallicGoldRim
import com.sanjeeb.marriagecalculator.ui.components.MetallicRedFace
import com.sanjeeb.marriagecalculator.ui.components.MetallicRedRim
import com.sanjeeb.marriagecalculator.ui.theme.DeepRedTika
import com.sanjeeb.marriagecalculator.ui.theme.GoldAccent
import com.sanjeeb.marriagecalculator.ui.theme.MarigoldOrange
import com.sanjeeb.marriagecalculator.ui.theme.TiharNightBlue
import java.io.File

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun PlayGameScreen(
    gameSetId: String,
    onAddRound: (String) -> Unit,
    onViewScoreboard: () -> Unit,
    onBack: () -> Unit,
    viewModel: PlayGameViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(gameSetId) {
        viewModel.loadGame(gameSetId)
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Text(
                        text = uiState.gameName,
                        color = GoldAccent,
                        fontFamily = FontFamily.Serif,
                        fontWeight = FontWeight.Bold,
                        fontSize = 20.sp
                    )
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
                    .verticalScroll(rememberScrollState())
                    .padding(horizontal = 16.dp, vertical = 12.dp)
            ) {
                // Next Dealer Banner
                if (uiState.nextDealerName.isNotEmpty() && !uiState.isSettled) {
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 16.dp),
                        shape = RoundedCornerShape(12.dp),
                        colors = CardDefaults.cardColors(containerColor = GoldAccent.copy(alpha = 0.1f)),
                        border = androidx.compose.foundation.BorderStroke(1.dp, GoldAccent.copy(alpha = 0.3f))
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Icon(Icons.Default.Casino, null, tint = GoldAccent, modifier = Modifier.size(20.dp))
                            Spacer(modifier = Modifier.width(8.dp))
                            Text(
                                text = "Next Dealer: ",
                                color = Color.White.copy(alpha = 0.7f),
                                fontSize = 14.sp
                            )
                            Text(
                                text = uiState.nextDealerName,
                                color = GoldAccent,
                                fontWeight = FontWeight.Bold,
                                fontSize = 14.sp
                            )
                        }
                    }
                }

                // Leaderboard Title
                Text(
                    text = "STANDINGS",
                    color = GoldAccent,
                    fontSize = 12.sp,
                    fontWeight = FontWeight.Bold,
                    letterSpacing = 1.5.sp,
                    modifier = Modifier.padding(bottom = 8.dp)
                )

                // Standings list
                uiState.players.forEach { standings ->
                    PlayerStandingsRow(standings)
                    Spacer(modifier = Modifier.height(8.dp))
                }

                Spacer(modifier = Modifier.height(24.dp))

                // Rounds Section Title
                Text(
                    text = "ROUNDS PLAYED",
                    color = GoldAccent,
                    fontSize = 12.sp,
                    fontWeight = FontWeight.Bold,
                    letterSpacing = 1.5.sp,
                    modifier = Modifier.padding(bottom = 8.dp)
                )

                if (uiState.rounds.isEmpty()) {
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(vertical = 32.dp),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            text = "No rounds played yet. Tap Add Round to start!",
                            color = Color.White.copy(alpha = 0.4f),
                            fontSize = 14.sp
                        )
                    }
                } else {
                    uiState.rounds.forEach { round ->
                        RoundItemRow(round)
                        Spacer(modifier = Modifier.height(8.dp))
                    }
                }

                Spacer(modifier = Modifier.height(100.dp)) // padding for the bottom buttons
            }

            // Bottom Buttons fixed
            Box(
                modifier = Modifier
                    .align(Alignment.BottomCenter)
                    .fillMaxWidth()
                    .background(
                        Brush.verticalGradient(
                            listOf(Color.Transparent, Color(0xFF0D0D1A).copy(alpha = 0.95f))
                        )
                    )
                    .padding(16.dp)
            ) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    if (!uiState.isSettled) {
                        // Add Round Button
                        MetallicButton(
                            onClick = { onAddRound((uiState.rounds.size + 1).toString()) },
                            text = "Add Round",
                            rimColors = MetallicRedRim,
                            faceColors = MetallicRedFace,
                            textColor = GoldAccent,
                            modifier = Modifier
                                .weight(1f)
                                .height(56.dp),
                            leadingIcon = {
                                Icon(Icons.Default.Add, null, tint = GoldAccent, modifier = Modifier.size(20.dp))
                            }
                        )
                    }

                    // Scoreboard Button
                    MetallicButton(
                        onClick = onViewScoreboard,
                        text = if (uiState.isSettled) "Final Scoreboard" else "Scoreboard",
                        rimColors = MetallicGoldRim,
                        faceColors = MetallicGoldFace,
                        textColor = GoldAccent,
                        modifier = Modifier
                            .weight(1f)
                            .height(56.dp),
                        leadingIcon = {
                            Icon(Icons.Default.Leaderboard, null, tint = GoldAccent, modifier = Modifier.size(20.dp))
                        }
                    )
                }
            }
        }
    }
}

@Composable
private fun PlayerStandingsRow(standings: PlayerStandings) {
    val scoreColor = when {
        standings.netPoints > 0 -> Color(0xFF4CAF50)
        standings.netPoints < 0 -> Color(0xFFFF5252)
        else -> Color.White
    }

    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(8.dp),
        colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.05f))
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(10.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            // Avatar
            val uri = standings.player.photoUri
            val model = if (uri != null && (uri.startsWith("android.resource") || uri.startsWith("http"))) {
                uri
            } else if (uri != null) {
                File(uri)
            } else null

            if (model != null) {
                AsyncImage(
                    model = ImageRequest.Builder(LocalContext.current)
                        .data(model)
                        .crossfade(true)
                        .build(),
                    contentDescription = null,
                    contentScale = ContentScale.Crop,
                    modifier = Modifier
                        .size(36.dp)
                        .clip(RoundedCornerShape(6.dp))
                )
            } else {
                Box(
                    modifier = Modifier
                        .size(36.dp)
                        .clip(RoundedCornerShape(6.dp))
                        .background(Color.DarkGray),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = standings.player.name.take(1).uppercase(),
                        color = Color.White,
                        fontWeight = FontWeight.Bold,
                        fontSize = 16.sp
                    )
                }
            }

            Spacer(modifier = Modifier.width(12.dp))

            // Player Name + Dealer Indicator
            Column(modifier = Modifier.weight(1f)) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Text(
                        text = standings.player.name,
                        color = Color.White,
                        fontSize = 15.sp,
                        fontWeight = FontWeight.Bold
                    )
                    if (standings.isNextDealer) {
                        Spacer(modifier = Modifier.width(6.dp))
                        Box(
                            modifier = Modifier
                                .clip(RoundedCornerShape(4.dp))
                                .background(GoldAccent.copy(alpha = 0.2f))
                                .border(0.5.dp, GoldAccent, RoundedCornerShape(4.dp))
                                .padding(horizontal = 4.dp, vertical = 2.dp)
                        ) {
                            Text(
                                text = "DEALER",
                                color = GoldAccent,
                                fontSize = 8.sp,
                                fontWeight = FontWeight.Bold
                            )
                        }
                    }
                }
            }

            // Net Points
            Text(
                text = "${if (standings.netPoints > 0) "+" else ""}${standings.netPoints} pts",
                color = scoreColor,
                fontSize = 15.sp,
                fontWeight = FontWeight.Bold
            )
        }
    }
}

@Composable
private fun RoundItemRow(round: RoundItem) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(8.dp),
        colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.03f))
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Column {
                Text(
                    text = "Round ${round.roundNumber}",
                    color = Color.White,
                    fontWeight = FontWeight.Bold,
                    fontSize = 14.sp
                )
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Icon(
                        imageVector = Icons.Default.EmojiEvents,
                        contentDescription = null,
                        tint = MarigoldOrange,
                        modifier = Modifier.size(14.dp)
                    )
                    Spacer(modifier = Modifier.width(4.dp))
                    Text(
                        text = round.winnerName,
                        color = Color.White.copy(alpha = 0.6f),
                        fontSize = 12.sp
                    )
                }
            }

            Column(horizontalAlignment = Alignment.End) {
                Text(
                    text = "${if (round.winnerScore > 0) "+" else ""}${round.winnerScore} pts",
                    color = Color(0xFF4CAF50),
                    fontWeight = FontWeight.Bold,
                    fontSize = 14.sp
                )
                Text(
                    text = "Total Maal: ${round.totalMaal}",
                    color = Color.White.copy(alpha = 0.4f),
                    fontSize = 11.sp
                )
            }
        }
    }
}
