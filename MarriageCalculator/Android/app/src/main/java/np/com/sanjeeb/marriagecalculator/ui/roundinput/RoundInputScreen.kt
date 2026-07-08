package np.com.sanjeeb.marriagecalculator.ui.roundinput

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
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
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import coil.compose.AsyncImage
import coil.request.ImageRequest
import np.com.sanjeeb.marriagecalculator.ui.theme.DeepRedTika
import np.com.sanjeeb.marriagecalculator.ui.theme.GoldAccent
import java.io.File

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun RoundInputScreen(
    gameSetId: String,
    roundId: String,
    onScoreSubmitted: () -> Unit,
    onBack: () -> Unit,
    viewModel: RoundInputViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    val roundNumber = roundId.toIntOrNull() ?: 1
    var maalDialogPlayerId by remember { mutableStateOf<String?>(null) }

    LaunchedEffect(gameSetId) {
        viewModel.loadGameData(gameSetId, roundNumber)
    }

    LaunchedEffect(uiState.submitted) {
        if (uiState.submitted) onScoreSubmitted()
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, null, tint = GoldAccent)
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = Color.Transparent)
            )
        },
        containerColor = Color(0xFF151515)
    ) { padding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .background(Brush.verticalGradient(listOf(Color(0xFF2C2C2C), Color(0xFF121212))))
                .padding(padding)
        ) {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .verticalScroll(rememberScrollState())
                    .padding(horizontal = 16.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                // Header
                Text(
                    text = "CURRENT MATCH",
                    color = GoldAccent.copy(alpha = 0.8f),
                    fontSize = 11.sp,
                    fontWeight = FontWeight.Bold,
                    letterSpacing = 2.sp
                )
                Spacer(modifier = Modifier.height(4.dp))
                Text(
                    text = "Round $roundNumber",
                    color = Color.White,
                    fontSize = 32.sp,
                    fontFamily = FontFamily.Serif,
                    fontWeight = FontWeight.Bold
                )
                Spacer(modifier = Modifier.height(8.dp))
                
                // Total Maal Collected Display
                val totalMaal = uiState.playerStates.sumOf { if (it.seen) it.seenPoints else 0 }
                Card(
                    shape = RoundedCornerShape(8.dp),
                    colors = CardDefaults.cardColors(containerColor = GoldAccent.copy(alpha = 0.15f)),
                    border = androidx.compose.foundation.BorderStroke(1.dp, GoldAccent.copy(alpha = 0.4f)),
                    modifier = Modifier.padding(vertical = 4.dp)
                ) {
                    Row(
                        modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Icon(Icons.Default.Casino, null, tint = GoldAccent, modifier = Modifier.size(18.dp))
                        Spacer(modifier = Modifier.width(8.dp))
                        Text(
                            text = "Total Maal Collected: ",
                            color = Color.White.copy(alpha = 0.8f),
                            fontSize = 14.sp
                        )
                        Text(
                            text = "$totalMaal",
                            color = GoldAccent,
                            fontWeight = FontWeight.Bold,
                            fontSize = 16.sp
                        )
                    }
                }

                Spacer(modifier = Modifier.height(16.dp))
                Box(modifier = Modifier.width(40.dp).height(3.dp).background(GoldAccent))
                Spacer(modifier = Modifier.height(24.dp))

                // Player cards
                val currencySymbol = uiState.settings.currency.displayName()
                    .substringAfter("(", "").substringBefore(")").ifEmpty { "" }

                uiState.playerStates.forEach { playerState ->
                    PlayerScoreCard(
                        state = playerState,
                        showPreview = uiState.showPreview,
                        currencySymbol = currencySymbol,
                        onSelectWinner = { viewModel.setWinner(playerState.player.id) },
                        onToggleSeen = { viewModel.toggleSeen(playerState.player.id) },
                        onToggleDuply = { viewModel.toggleDuply(playerState.player.id) },
                        onMaalPointsChange = { viewModel.setSeenPoints(playerState.player.id, it) },
                        onOpenMaalCalculator = { maalDialogPlayerId = playerState.player.id }
                    )
                    Spacer(modifier = Modifier.height(16.dp))
                }

                Spacer(modifier = Modifier.height(8.dp))

                // Error
                uiState.error?.let {
                    Text(it, color = Color.Red, fontSize = 14.sp, modifier = Modifier.padding(bottom = 16.dp))
                }

                // Buttons
                Button(
                    onClick = { viewModel.submitRound() },
                    modifier = Modifier.fillMaxWidth().height(56.dp),
                    colors = ButtonDefaults.buttonColors(containerColor = Color(0xFFDCD451)),
                    shape = RoundedCornerShape(8.dp)
                ) {
                    Icon(Icons.Default.Calculate, contentDescription = null, tint = Color(0xFF3E2723))
                    Spacer(modifier = Modifier.width(8.dp))
                    Text("Save Round Results", color = Color(0xFF3E2723), fontWeight = FontWeight.Bold, fontSize = 16.sp)
                }

                Spacer(modifier = Modifier.height(12.dp))

                OutlinedButton(
                    onClick = { onBack() },
                    border = androidx.compose.foundation.BorderStroke(1.dp, Color.White.copy(alpha = 0.1f)),
                    modifier = Modifier.fillMaxWidth().height(56.dp),
                    shape = RoundedCornerShape(8.dp),
                    colors = ButtonDefaults.outlinedButtonColors(contentColor = Color.White)
                ) {
                    Text("Discard & Return")
                }

                Spacer(modifier = Modifier.height(32.dp))
            }

            // Maal calculator dialog (requirement §3.2 optional advanced calculator)
            maalDialogPlayerId?.let { playerId ->
                val playerState = uiState.playerStates.firstOrNull { it.player.id == playerId }
                if (playerState != null) {
                    MaalCalculatorDialog(
                        playerName = playerState.player.name,
                        initialCounts = uiState.maalCounts[playerId] ?: emptyMap(),
                        onApply = { counts, total ->
                            viewModel.applyMaalCounts(playerId, counts, total)
                        },
                        onDismiss = { maalDialogPlayerId = null }
                    )
                }
            }
        }
    }
}

@Composable
private fun PlayerScoreCard(
    state: PlayerRoundState,
    showPreview: Boolean,
    currencySymbol: String,
    onSelectWinner: () -> Unit,
    onToggleSeen: () -> Unit,
    onToggleDuply: () -> Unit,
    onMaalPointsChange: (Int) -> Unit,
    onOpenMaalCalculator: () -> Unit
) {
    val isDealer = state.isDealer
    val isWinner = state.isWinner
    val highlightBorder = isWinner || isDealer

    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = Color(0xFF1E1E1E)),
        border = androidx.compose.foundation.BorderStroke(1.dp, if (isWinner) GoldAccent else if (isDealer) Color.White.copy(alpha = 0.2f) else Color.White.copy(alpha = 0.05f))
    ) {
        Row(modifier = Modifier.fillMaxWidth().height(IntrinsicSize.Min)) {
            // Left border indicator
            if (isWinner) {
                Box(modifier = Modifier.fillMaxHeight().width(4.dp).background(GoldAccent))
            } else if (isDealer) {
                Box(modifier = Modifier.fillMaxHeight().width(4.dp).background(Color.White.copy(alpha = 0.4f)))
            }
            
            Column(modifier = Modifier.padding(16.dp).fillMaxWidth()) {
                // Header row
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.SpaceBetween,
                    modifier = Modifier.fillMaxWidth()
                ) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        // Avatar
                        val uri = state.player.photoUri
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
                                modifier = Modifier.size(48.dp).clip(RoundedCornerShape(8.dp))
                            )
                        } else {
                            Box(
                                modifier = Modifier.size(48.dp).clip(RoundedCornerShape(8.dp)).background(Color.DarkGray),
                                contentAlignment = Alignment.Center
                            ) {
                                Text(state.player.name.take(1).uppercase(), color = Color.White, fontWeight = FontWeight.Bold, fontSize = 20.sp)
                            }
                        }

                        Spacer(modifier = Modifier.width(16.dp))

                        Column {
                            Text(
                                text = state.player.name,
                                color = Color.White,
                                fontSize = 18.sp,
                                fontFamily = FontFamily.Serif,
                                fontWeight = FontWeight.Bold
                            )
                            Text(
                                text = if (isDealer) "DEALER" else "PLAYER",
                                color = Color.White.copy(alpha = 0.5f),
                                fontSize = 10.sp,
                                letterSpacing = 1.sp
                            )
                        }
                    }

                    // Winner Trophy/Crown Trigger
                    IconButton(onClick = onSelectWinner) {
                        Icon(
                            imageVector = Icons.Default.EmojiEvents,
                            contentDescription = "Select Winner",
                            tint = if (isWinner) GoldAccent else Color.White.copy(alpha = 0.15f),
                            modifier = Modifier.size(28.dp)
                        )
                    }
                }

                Spacer(modifier = Modifier.height(16.dp))

                // Toggles Row
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    // Seen Toggle
                    Row(
                        modifier = Modifier
                            .weight(1f)
                            .clip(RoundedCornerShape(8.dp))
                            .background(if (state.seen) GoldAccent.copy(alpha = 0.1f) else Color(0xFF252525))
                            .border(0.5.dp, if (state.seen) GoldAccent.copy(alpha = 0.3f) else Color.Transparent, RoundedCornerShape(8.dp))
                            .clickable(enabled = !isWinner) { onToggleSeen() } // Winner is always seen
                            .padding(horizontal = 12.dp, vertical = 10.dp),
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        Text(
                            text = "Seen Joker?",
                            color = if (state.seen) GoldAccent else Color.White.copy(alpha = 0.7f),
                            fontSize = 12.sp,
                            fontWeight = if (state.seen) FontWeight.Bold else FontWeight.Normal
                        )
                        Icon(
                            imageVector = if (state.seen) Icons.Default.CheckBox else Icons.Default.CheckBoxOutlineBlank,
                            contentDescription = "Seen status",
                            tint = if (state.seen) GoldAccent else Color.White.copy(alpha = 0.2f),
                            modifier = Modifier.size(16.dp)
                        )
                    }

                    // Dublee (Doublee Hand) Toggle
                    Row(
                        modifier = Modifier
                            .weight(1f)
                            .clip(RoundedCornerShape(8.dp))
                            .background(if (state.duply) DeepRedTika.copy(alpha = 0.1f) else Color(0xFF252525))
                            .border(0.5.dp, if (state.duply) DeepRedTika.copy(alpha = 0.3f) else Color.Transparent, RoundedCornerShape(8.dp))
                            .clickable { onToggleDuply() }
                            .padding(horizontal = 12.dp, vertical = 10.dp),
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        Text(
                            text = "Dublee?",
                            color = if (state.duply) DeepRedTika else Color.White.copy(alpha = 0.7f),
                            fontSize = 12.sp,
                            fontWeight = if (state.duply) FontWeight.Bold else FontWeight.Normal
                        )
                        Icon(
                            imageVector = if (state.duply) Icons.Default.CheckBox else Icons.Default.CheckBoxOutlineBlank,
                            contentDescription = "Dublee status",
                            tint = if (state.duply) DeepRedTika else Color.White.copy(alpha = 0.2f),
                            modifier = Modifier.size(16.dp)
                        )
                    }
                }

                // Maal points input (only shown when Seen is true)
                if (state.seen) {
                    Spacer(modifier = Modifier.height(16.dp))
                    Column(modifier = Modifier.fillMaxWidth()) {
                        Text(
                            text = "MAAL (0 - 99)",
                            color = GoldAccent.copy(alpha = 0.8f),
                            fontSize = 10.sp,
                            fontWeight = FontWeight.Bold,
                            letterSpacing = 0.5.sp
                        )
                        Spacer(modifier = Modifier.height(6.dp))
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            OutlinedTextField(
                                value = if (state.seenPoints == 0) "" else state.seenPoints.toString(),
                                onValueChange = {
                                    val typed = it.toIntOrNull() ?: 0
                                    onMaalPointsChange(typed)
                                },
                                modifier = Modifier.weight(1f).height(50.dp),
                                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                                colors = OutlinedTextFieldDefaults.colors(
                                    focusedContainerColor = Color(0xFF2A2A2A),
                                    unfocusedContainerColor = Color(0xFF2A2A2A),
                                    focusedBorderColor = GoldAccent.copy(alpha = 0.5f),
                                    unfocusedBorderColor = Color.Transparent,
                                    focusedTextColor = Color.White,
                                    unfocusedTextColor = Color.White
                                ),
                                placeholder = { Text("Enter Maal points", color = Color.White.copy(alpha = 0.2f), fontSize = 13.sp) },
                                singleLine = true,
                                shape = RoundedCornerShape(8.dp)
                            )
                            Spacer(modifier = Modifier.width(8.dp))
                            // Open the advanced Maal calculator
                            IconButton(
                                onClick = onOpenMaalCalculator,
                                modifier = Modifier
                                    .size(50.dp)
                                    .clip(RoundedCornerShape(8.dp))
                                    .background(GoldAccent.copy(alpha = 0.12f))
                                    .border(1.dp, GoldAccent.copy(alpha = 0.4f), RoundedCornerShape(8.dp))
                            ) {
                                Icon(
                                    imageVector = Icons.Default.Calculate,
                                    contentDescription = "Open Maal calculator",
                                    tint = GoldAccent,
                                    modifier = Modifier.size(24.dp)
                                )
                            }
                        }
                    }
                }

                // Live preview of this round's computed points/money, once a winner is picked
                if (showPreview) {
                    Spacer(modifier = Modifier.height(12.dp))
                    val scoreColor = when {
                        state.previewScore > 0 -> Color(0xFF4CAF50)
                        state.previewScore < 0 -> Color(0xFFFF5252)
                        else -> Color.White.copy(alpha = 0.6f)
                    }
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .clip(RoundedCornerShape(8.dp))
                            .background(scoreColor.copy(alpha = 0.08f))
                            .padding(horizontal = 12.dp, vertical = 8.dp),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Text(
                            text = "Points: ${if (state.previewScore > 0) "+" else ""}${state.previewScore}",
                            color = scoreColor,
                            fontSize = 13.sp,
                            fontWeight = FontWeight.Bold
                        )
                        Text(
                            text = "Money: ${if (state.previewMoney > 0) "+" else ""}${String.format("%.1f", state.previewMoney)}$currencySymbol",
                            color = scoreColor,
                            fontSize = 13.sp,
                            fontWeight = FontWeight.Bold
                        )
                    }
                }
            }
        }
    }
}
