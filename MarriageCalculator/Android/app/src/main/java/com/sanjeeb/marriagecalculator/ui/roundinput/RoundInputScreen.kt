package com.sanjeeb.marriagecalculator.ui.roundinput

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
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
import androidx.compose.ui.text.input.KeyboardType
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
fun RoundInputScreen(
    gameSetId: Int,
    roundId: Int,
    onScoreSubmitted: () -> Unit,
    onBack: () -> Unit,
    viewModel: RoundInputViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(uiState.submitted) {
        if (uiState.submitted) onScoreSubmitted()
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Text("Round Scorer", color = GoldAccent, fontFamily = FontFamily.Serif, fontWeight = FontWeight.Bold)
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
                    .padding(12.dp)
            ) {
                // Instruction
                Text(
                    "Tap a player to set as Winner",
                    color = Color.White.copy(alpha = 0.5f),
                    fontSize = 12.sp,
                    modifier = Modifier.padding(bottom = 8.dp)
                )

                // Player cards grid - 2 columns for up to 6 players
                LazyVerticalGrid(
                    columns = GridCells.Fixed(2),
                    modifier = Modifier.heightIn(max = 600.dp),
                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    items(uiState.playerStates) { playerState ->
                        PlayerScoreCard(
                            state = playerState,
                            onSelectWinner = { viewModel.setWinner(playerState.player.id) },
                            onToggleSeen = { viewModel.toggleSeen(playerState.player.id) },
                            onToggleDuply = { viewModel.toggleDuply(playerState.player.id) },
                            onMaalChange = { viewModel.setMaal(playerState.player.id, it) }
                        )
                    }
                }

                Spacer(modifier = Modifier.height(12.dp))

                // Score Preview
                if (uiState.showPreview && uiState.winnerId != null) {
                    ScorePreviewSection(uiState.playerStates, uiState.settings.pointRate)
                    Spacer(modifier = Modifier.height(12.dp))
                }

                // Error
                uiState.error?.let {
                    Text(it, color = Color.Red, fontSize = 14.sp, modifier = Modifier.padding(bottom = 8.dp))
                }

                // Submit Button
                Button(
                    onClick = { viewModel.submitRound() },
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(52.dp),
                    shape = RoundedCornerShape(16.dp),
                    colors = ButtonDefaults.buttonColors(containerColor = DeepRedTika),
                    enabled = uiState.winnerId != null && !uiState.isLoading
                ) {
                    Icon(Icons.Default.Check, null, tint = GoldAccent)
                    Spacer(modifier = Modifier.width(8.dp))
                    Text("Submit Round", color = GoldAccent, fontWeight = FontWeight.Bold, fontSize = 16.sp)
                }
            }
        }
    }
}

@Composable
private fun PlayerScoreCard(
    state: PlayerRoundState,
    onSelectWinner: () -> Unit,
    onToggleSeen: () -> Unit,
    onToggleDuply: () -> Unit,
    onMaalChange: (Int) -> Unit
) {
    val borderColor = when {
        state.isWinner -> GoldAccent
        state.seen -> MarigoldOrange.copy(alpha = 0.6f)
        else -> Color.White.copy(alpha = 0.15f)
    }
    val bgColor = when {
        state.isWinner -> DeepRedTika.copy(alpha = 0.6f)
        state.seen -> Color.White.copy(alpha = 0.08f)
        else -> Color.White.copy(alpha = 0.03f)
    }

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onSelectWinner() },
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = bgColor),
        border = androidx.compose.foundation.BorderStroke(
            width = if (state.isWinner) 2.dp else 1.dp,
            color = borderColor
        )
    ) {
        Column(modifier = Modifier.padding(10.dp)) {
            // Header row: name + winner crown
            Row(
                verticalAlignment = Alignment.CenterVertically,
                modifier = Modifier.fillMaxWidth()
            ) {
                // Avatar
                Box(
                    modifier = Modifier
                        .size(28.dp)
                        .clip(CircleShape)
                        .background(if (state.isWinner) GoldAccent else Color.White.copy(alpha = 0.1f)),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        state.player.name.take(1).uppercase(),
                        color = if (state.isWinner) DeepRedTika else Color.White,
                        fontSize = 14.sp,
                        fontWeight = FontWeight.Bold
                    )
                }
                Spacer(modifier = Modifier.width(6.dp))
                Text(
                    state.player.name,
                    color = Color.White,
                    fontSize = 14.sp,
                    fontWeight = FontWeight.Bold,
                    maxLines = 1,
                    modifier = Modifier.weight(1f)
                )
                if (state.isWinner) {
                    Text("👑", fontSize = 16.sp)
                }
                if (state.isDealer) {
                    Text("🃏", fontSize = 14.sp)
                }
            }

            Spacer(modifier = Modifier.height(6.dp))

            // Status toggles row
            Row(
                horizontalArrangement = Arrangement.spacedBy(4.dp),
                modifier = Modifier.fillMaxWidth()
            ) {
                StatusChip(
                    label = if (state.seen) "Seen" else "Unseen",
                    isActive = state.seen,
                    onClick = onToggleSeen,
                    activeColor = MarigoldOrange,
                    enabled = !state.isWinner,
                    modifier = Modifier.weight(1f)
                )
                StatusChip(
                    label = "Dublee",
                    isActive = state.duply,
                    onClick = onToggleDuply,
                    activeColor = Color(0xFF9C27B0),
                    modifier = Modifier.weight(1f)
                )
            }

            Spacer(modifier = Modifier.height(6.dp))

            // Maal input
            Row(verticalAlignment = Alignment.CenterVertically) {
                Text("Maal:", color = Color.White.copy(alpha = 0.6f), fontSize = 12.sp)
                Spacer(modifier = Modifier.width(4.dp))
                // Minus button
                IconButton(
                    onClick = { onMaalChange(state.maal - 1) },
                    modifier = Modifier.size(28.dp)
                ) {
                    Icon(Icons.Default.Remove, null, tint = GoldAccent, modifier = Modifier.size(16.dp))
                }
                OutlinedTextField(
                    value = if (state.maal == 0) "" else state.maal.toString(),
                    onValueChange = { onMaalChange(it.toIntOrNull() ?: 0) },
                    modifier = Modifier
                        .weight(1f)
                        .height(40.dp),
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    colors = OutlinedTextFieldDefaults.colors(
                        focusedTextColor = Color.White,
                        unfocusedTextColor = Color.White,
                        focusedBorderColor = GoldAccent.copy(alpha = 0.5f),
                        unfocusedBorderColor = Color.White.copy(alpha = 0.15f)
                    ),
                    textStyle = androidx.compose.ui.text.TextStyle(
                        textAlign = TextAlign.Center,
                        fontSize = 14.sp
                    ),
                    singleLine = true
                )
                // Plus button
                IconButton(
                    onClick = { onMaalChange(state.maal + 1) },
                    modifier = Modifier.size(28.dp)
                ) {
                    Icon(Icons.Default.Add, null, tint = GoldAccent, modifier = Modifier.size(16.dp))
                }
            }

            // Preview score
            if (state.previewScore != 0) {
                Spacer(modifier = Modifier.height(4.dp))
                Text(
                    text = "${if (state.previewScore > 0) "+" else ""}${state.previewScore} pts",
                    color = if (state.previewScore > 0) Color(0xFF4CAF50) else Color(0xFFFF5252),
                    fontSize = 13.sp,
                    fontWeight = FontWeight.Bold,
                    modifier = Modifier.fillMaxWidth(),
                    textAlign = TextAlign.End
                )
            }
        }
    }
}

@Composable
private fun StatusChip(
    label: String,
    isActive: Boolean,
    onClick: () -> Unit,
    activeColor: Color,
    modifier: Modifier = Modifier,
    enabled: Boolean = true
) {
    val bg = if (isActive) activeColor.copy(alpha = 0.3f) else Color.Transparent
    val border = if (isActive) activeColor else Color.White.copy(alpha = 0.2f)

    Box(
        modifier = modifier
            .clip(RoundedCornerShape(6.dp))
            .background(bg)
            .border(1.dp, border, RoundedCornerShape(6.dp))
            .clickable(enabled = enabled) { onClick() }
            .padding(horizontal = 6.dp, vertical = 4.dp),
        contentAlignment = Alignment.Center
    ) {
        Text(
            label,
            color = if (isActive) activeColor else Color.White.copy(alpha = 0.5f),
            fontSize = 11.sp,
            fontWeight = if (isActive) FontWeight.Bold else FontWeight.Normal
        )
    }
}

@Composable
private fun ScorePreviewSection(playerStates: List<PlayerRoundState>, pointRate: Double) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.05f))
    ) {
        Column(modifier = Modifier.padding(12.dp)) {
            Text(
                "Score Preview",
                color = GoldAccent,
                fontSize = 16.sp,
                fontWeight = FontWeight.Bold,
                fontFamily = FontFamily.Serif
            )
            Spacer(modifier = Modifier.height(8.dp))
            playerStates.forEach { ps ->
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 2.dp),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    Row {
                        if (ps.isWinner) Text("👑 ", fontSize = 12.sp)
                        Text(ps.player.name, color = Color.White, fontSize = 13.sp)
                    }
                    Row {
                        Text(
                            "${if (ps.previewScore > 0) "+" else ""}${ps.previewScore}",
                            color = if (ps.previewScore > 0) Color(0xFF4CAF50) else Color(0xFFFF5252),
                            fontSize = 13.sp,
                            fontWeight = FontWeight.Bold
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                        Text(
                            "(${String.format("%.0f", ps.previewMoney)})",
                            color = Color.White.copy(alpha = 0.5f),
                            fontSize = 12.sp
                        )
                    }
                }
            }
        }
    }
}
