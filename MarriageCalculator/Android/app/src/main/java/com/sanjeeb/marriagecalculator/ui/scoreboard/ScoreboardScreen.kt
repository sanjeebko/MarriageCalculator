package com.sanjeeb.marriagecalculator.ui.scoreboard

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import com.sanjeeb.marriagecalculator.ui.theme.DeepRedTika
import com.sanjeeb.marriagecalculator.ui.theme.GoldAccent
import com.sanjeeb.marriagecalculator.ui.theme.MarigoldOrange
import com.sanjeeb.marriagecalculator.ui.theme.TiharNightBlue

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ScoreboardScreen(
    gameSetId: Int,
    onBack: () -> Unit,
    viewModel: ScoreboardViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Text("Scoreboard", color = GoldAccent, fontFamily = FontFamily.Serif, fontWeight = FontWeight.Bold)
                },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, null, tint = GoldAccent)
                    }
                },
                actions = {
                    IconButton(onClick = { viewModel.toggleHistory() }) {
                        Icon(
                            if (uiState.showHistory) Icons.Default.TableChart else Icons.Default.History,
                            null,
                            tint = GoldAccent
                        )
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
            if (uiState.showHistory) {
                RoundHistoryView(uiState)
            } else {
                ScoreboardView(uiState, viewModel::settleGame)
            }
        }
    }
}

@Composable
private fun ScoreboardView(uiState: ScoreboardUiState, onSettle: () -> Unit) {
    val sortedPlayers = uiState.players.sortedByDescending { it.totalPoints }
    val currency = uiState.settings.currency.displayName()

    LazyColumn(
        modifier = Modifier
            .fillMaxSize()
            .padding(12.dp),
        verticalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        // Player Score Cards - compact for 6 players
        items(sortedPlayers) { playerScore ->
            val rank = sortedPlayers.indexOf(playerScore) + 1
            PlayerScoreRow(playerScore, rank, currency)
        }

        // Settlement section
        item {
            Spacer(modifier = Modifier.height(16.dp))

            // Who Owes Whom section
            if (sortedPlayers.any { it.totalPoints != 0 }) {
                WhoOwesWhomSection(sortedPlayers, uiState.settings.pointRate)
            }

            Spacer(modifier = Modifier.height(16.dp))

            // Settle button
            if (!uiState.isSettled) {
                Button(
                    onClick = onSettle,
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(48.dp),
                    shape = RoundedCornerShape(12.dp),
                    colors = ButtonDefaults.buttonColors(containerColor = DeepRedTika)
                ) {
                    Icon(Icons.Default.AccountBalance, null, tint = GoldAccent)
                    Spacer(modifier = Modifier.width(8.dp))
                    Text("Settle & Freeze", color = GoldAccent, fontWeight = FontWeight.Bold)
                }
            }
        }
    }
}

@Composable
private fun PlayerScoreRow(playerScore: PlayerTotalScore, rank: Int, currency: String) {
    val scoreColor = when {
        playerScore.totalPoints > 0 -> Color(0xFF4CAF50)
        playerScore.totalPoints < 0 -> Color(0xFFFF5252)
        else -> Color.White.copy(alpha = 0.5f)
    }
    val medalEmoji = when (rank) {
        1 -> "🥇"
        2 -> "🥈"
        3 -> "🥉"
        else -> ""
    }

    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(
            containerColor = if (rank == 1) DeepRedTika.copy(alpha = 0.3f) else Color.White.copy(alpha = 0.05f)
        )
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            // Rank + Avatar
            Box(
                modifier = Modifier
                    .size(36.dp)
                    .clip(CircleShape)
                    .background(if (rank == 1) GoldAccent else Color.White.copy(alpha = 0.1f)),
                contentAlignment = Alignment.Center
            ) {
                if (medalEmoji.isNotEmpty()) {
                    Text(medalEmoji, fontSize = 18.sp)
                } else {
                    Text(
                        "#$rank",
                        color = Color.White,
                        fontSize = 14.sp,
                        fontWeight = FontWeight.Bold
                    )
                }
            }

            Spacer(modifier = Modifier.width(10.dp))

            // Name + stats
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    playerScore.player.name,
                    color = Color.White,
                    fontSize = 16.sp,
                    fontWeight = FontWeight.Bold
                )
                Text(
                    "${playerScore.gamesWon}W / ${playerScore.gamesPlayed}G",
                    color = Color.White.copy(alpha = 0.4f),
                    fontSize = 11.sp
                )
            }

            // Score + Money
            Column(horizontalAlignment = Alignment.End) {
                Text(
                    "${if (playerScore.totalPoints > 0) "+" else ""}${playerScore.totalPoints} pts",
                    color = scoreColor,
                    fontSize = 18.sp,
                    fontWeight = FontWeight.Bold
                )
                Text(
                    "${String.format("%.0f", playerScore.totalMoney)} $currency",
                    color = scoreColor.copy(alpha = 0.7f),
                    fontSize = 12.sp
                )
            }
        }
    }
}

@Composable
private fun WhoOwesWhomSection(players: List<PlayerTotalScore>, pointRate: Double) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.05f))
    ) {
        Column(modifier = Modifier.padding(12.dp)) {
            Text(
                "💰 Settlement Summary",
                color = GoldAccent,
                fontSize = 16.sp,
                fontWeight = FontWeight.Bold,
                fontFamily = FontFamily.Serif
            )
            Spacer(modifier = Modifier.height(8.dp))

            // Simple settlement: losers pay winners
            val winners = players.filter { it.totalPoints > 0 }.sortedByDescending { it.totalPoints }
            val losers = players.filter { it.totalPoints < 0 }.sortedBy { it.totalPoints }

            if (winners.isEmpty() || losers.isEmpty()) {
                Text("All settled!", color = Color.White.copy(alpha = 0.5f))
                return@Column
            }

            for (loser in losers) {
                for (winner in winners) {
                    val amount = kotlin.math.abs(loser.totalMoney)
                    if (amount > 0) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(vertical = 2.dp),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Text(
                                "${loser.player.name} → ${winner.player.name}",
                                color = Color.White.copy(alpha = 0.7f),
                                fontSize = 13.sp
                            )
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun RoundHistoryView(uiState: ScoreboardUiState) {
    LazyColumn(
        modifier = Modifier
            .fillMaxSize()
            .padding(12.dp),
        verticalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        if (uiState.rounds.isEmpty()) {
            item {
                Box(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(48.dp),
                    contentAlignment = Alignment.Center
                ) {
                    Text("No rounds played yet", color = Color.White.copy(alpha = 0.5f))
                }
            }
        }

        items(uiState.rounds.reversed()) { round ->
            Card(
                modifier = Modifier.fillMaxWidth(),
                shape = RoundedCornerShape(12.dp),
                colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.05f))
            ) {
                Column(modifier = Modifier.padding(12.dp)) {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        Text(
                            "Round ${round.roundNumber}",
                            color = GoldAccent,
                            fontWeight = FontWeight.Bold,
                            fontSize = 14.sp
                        )
                        Text(
                            "👑 ${round.winnerName}",
                            color = MarigoldOrange,
                            fontSize = 13.sp
                        )
                    }
                    Spacer(modifier = Modifier.height(6.dp))

                    // Scores for this round
                    round.scores.forEach { (playerId, score) ->
                        val playerName = uiState.players.find { it.player.id == playerId }?.player?.name ?: "Player $playerId"
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Text(playerName, color = Color.White.copy(alpha = 0.7f), fontSize = 12.sp)
                            Text(
                                "${if (score > 0) "+" else ""}$score",
                                color = if (score > 0) Color(0xFF4CAF50) else Color(0xFFFF5252),
                                fontSize = 12.sp,
                                fontWeight = FontWeight.Bold
                            )
                        }
                    }
                }
            }
        }
    }
}
