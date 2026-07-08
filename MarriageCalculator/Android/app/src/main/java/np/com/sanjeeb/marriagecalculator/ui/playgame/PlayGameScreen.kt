package np.com.sanjeeb.marriagecalculator.ui.playgame

import androidx.compose.foundation.ScrollState
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
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
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import coil.compose.AsyncImage
import coil.request.ImageRequest
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.data.model.User
import np.com.sanjeeb.marriagecalculator.ui.gamesetup.PlayerMappingDialog
import np.com.sanjeeb.marriagecalculator.ui.gamesetup.RearrangeSeatsDialog
import np.com.sanjeeb.marriagecalculator.ui.scoreboard.RoundPlayerEntry
import np.com.sanjeeb.marriagecalculator.ui.theme.DeepRedTika
import np.com.sanjeeb.marriagecalculator.ui.theme.GoldAccent
import np.com.sanjeeb.marriagecalculator.ui.theme.MarigoldOrange
import np.com.sanjeeb.marriagecalculator.ui.theme.TiharNightBlue
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
    var selectedPlayerToMap by remember { mutableStateOf<Player?>(null) }
    var showTransferDialog by remember { mutableStateOf(false) }
    var showReorderDialog by remember { mutableStateOf(false) }
    var roundForDetails by remember { mutableStateOf<RoundItem?>(null) }

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
                actions = {
                    // Transfer host is disabled for now - the feature isn't fully designed yet.
                    if (uiState.isHost && uiState.isOnlineMode && !uiState.isSettled) {
                        IconButton(onClick = {}, enabled = false) {
                            Icon(Icons.Default.SwapHoriz, "Transfer Host (coming soon)", tint = GoldAccent.copy(alpha = 0.3f))
                        }
                    }
                    IconButton(onClick = onViewScoreboard) {
                        Icon(Icons.Default.Leaderboard, "Scoreboard", tint = GoldAccent)
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = TiharNightBlue)
            )
        },
        floatingActionButton = {
            if (!uiState.isSettled && uiState.isHost) {
                FloatingActionButton(
                    onClick = { onAddRound((uiState.rounds.size + 1).toString()) },
                    containerColor = DeepRedTika,
                    contentColor = GoldAccent
                ) {
                    Icon(Icons.Default.Add, contentDescription = "Add Round")
                }
            }
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
                // Error display
                uiState.error?.let { err ->
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 16.dp),
                        colors = CardDefaults.cardColors(containerColor = Color(0xFFFF5252).copy(alpha = 0.15f))
                    ) {
                        Row(
                            modifier = Modifier.padding(16.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Icon(Icons.Default.Error, contentDescription = null, tint = Color(0xFFFF5252))
                            Spacer(modifier = Modifier.width(12.dp))
                            Text(text = err, color = Color(0xFFFF8888), fontSize = 14.sp, modifier = Modifier.weight(1f))
                            IconButton(onClick = { viewModel.clearError() }) {
                                Icon(Icons.Default.Close, contentDescription = "Clear error", tint = Color.White)
                            }
                        }
                    }
                }

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

                // Reshuffle seating notice when round is complete
                val isDealingRoundComplete = uiState.players.isNotEmpty() &&
                        uiState.rounds.isNotEmpty() &&
                        (uiState.rounds.size % uiState.players.size == 0)

                if (isDealingRoundComplete && !uiState.isSettled && uiState.isHost) {
                    Card(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(bottom = 16.dp),
                        shape = RoundedCornerShape(12.dp),
                        colors = CardDefaults.cardColors(containerColor = MarigoldOrange.copy(alpha = 0.1f)),
                        border = androidx.compose.foundation.BorderStroke(1.dp, MarigoldOrange.copy(alpha = 0.3f))
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.weight(1f)) {
                                Icon(Icons.Default.Refresh, null, tint = MarigoldOrange, modifier = Modifier.size(20.dp))
                                Spacer(modifier = Modifier.width(8.dp))
                                Column {
                                    Text(
                                        text = "Dealing Round Complete!",
                                        color = Color.White,
                                        fontWeight = FontWeight.Bold,
                                        fontSize = 13.sp
                                    )
                                    Text(
                                        text = "Every player has dealt. Time to reshuffle seats?",
                                        color = Color.White.copy(alpha = 0.7f),
                                        fontSize = 11.sp
                                    )
                                }
                            }
                            TextButton(
                                onClick = { showReorderDialog = true },
                                contentPadding = PaddingValues(horizontal = 8.dp, vertical = 4.dp)
                            ) {
                                Text("Reshuffle", color = MarigoldOrange, fontWeight = FontWeight.Bold, fontSize = 12.sp)
                            }
                        }
                    }
                }

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
                            text = "No rounds played yet." + if (uiState.isHost) " Tap Add Round to start!" else "",
                            color = Color.White.copy(alpha = 0.4f),
                            fontSize = 14.sp
                        )
                    }
                } else {
                    CompactRoundsTable(
                        rounds = uiState.rounds.sortedByDescending { it.roundNumber },
                        players = uiState.players,
                        currencySymbol = currencySymbol(uiState.settings.currency.displayName()),
                        onRoundClick = { roundForDetails = it }
                    )
                }

                Spacer(modifier = Modifier.height(24.dp))

                // Leaderboard Title Row
                Row(
                    modifier = Modifier.fillMaxWidth().padding(bottom = 8.dp),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "STANDINGS",
                        color = GoldAccent,
                        fontSize = 12.sp,
                        fontWeight = FontWeight.Bold,
                        letterSpacing = 1.5.sp
                    )
                    if (uiState.isHost && !uiState.isSettled) {
                        Row(
                            modifier = Modifier
                                .clip(RoundedCornerShape(4.dp))
                                .clickable { showReorderDialog = true }
                                .padding(horizontal = 6.dp, vertical = 2.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Icon(Icons.Default.Reorder, null, tint = GoldAccent, modifier = Modifier.size(16.dp))
                            Spacer(modifier = Modifier.width(4.dp))
                            Text("Arrange Seats", color = GoldAccent, fontSize = 12.sp, fontWeight = FontWeight.Bold)
                        }
                    }
                }

                // Standings list
                val currencySymbolText = currencySymbol(uiState.settings.currency.displayName())
                uiState.players.forEach { standings ->
                    PlayerStandingsRow(
                        standings = standings,
                        isHost = uiState.isHost,
                        currentUserEmail = uiState.currentUserEmail,
                        currencySymbol = currencySymbolText,
                        onMapClick = { selectedPlayerToMap = standings.player },
                        onNudgeClick = {
                            viewModel.nudgePlayer(standings.player.id, gameSetId)
                        }
                    )
                    Spacer(modifier = Modifier.height(8.dp))
                }

                Spacer(modifier = Modifier.height(90.dp)) // clearance for the FAB
            }
        }
    }

    // Player Mapping Dialog
    selectedPlayerToMap?.let { player ->
        PlayerMappingDialog(
            player = player,
            friends = uiState.friendsList,
            onMap = { friend ->
                viewModel.mapPlayerToFriend(player.id, friend, gameSetId)
            },
            onDismiss = { selectedPlayerToMap = null }
        )
    }

    // Host Transfer Dialog
    if (showTransferDialog) {
        AlertDialog(
            onDismissRequest = { showTransferDialog = false },
            title = {
                Text(
                    text = "Transfer Game Host",
                    color = GoldAccent,
                    fontFamily = FontFamily.Serif,
                    fontWeight = FontWeight.Bold
                )
            },
            text = {
                Column {
                    Text(
                        text = "Select a registered player to transfer ownership of this game set. Once transferred, you will have read-only access.",
                        color = Color.White.copy(alpha = 0.7f),
                        fontSize = 14.sp,
                        modifier = Modifier.padding(bottom = 16.dp)
                    )
                    val otherPlayers = uiState.players.filter {
                        it.player.id != uiState.players.firstOrNull()?.player?.id &&
                        it.player.email.isNotBlank()
                    }

                    if (otherPlayers.isEmpty()) {
                        Text("No other registered players in this game to transfer ownership to.", color = Color.White.copy(alpha = 0.4f))
                    } else {
                        LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                            items(otherPlayers) { pStandings ->
                                Card(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .clickable {
                                            viewModel.transferHost(pStandings.player.id, gameSetId)
                                            showTransferDialog = false
                                        },
                                    colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.05f))
                                ) {
                                    Row(
                                        modifier = Modifier.padding(12.dp),
                                        verticalAlignment = Alignment.CenterVertically
                                    ) {
                                        Icon(Icons.Default.Person, null, tint = GoldAccent)
                                        Spacer(modifier = Modifier.width(12.dp))
                                        Column {
                                            Text(pStandings.player.name, color = Color.White, fontWeight = FontWeight.Bold)
                                            Text(pStandings.player.email, color = Color.White.copy(alpha = 0.5f), fontSize = 12.sp)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            confirmButton = {},
            dismissButton = {
                TextButton(onClick = { showTransferDialog = false }) {
                    Text("Cancel", color = GoldAccent)
                }
            },
            containerColor = TiharNightBlue,
            shape = RoundedCornerShape(16.dp),
            modifier = Modifier.border(1.dp, GoldAccent, RoundedCornerShape(16.dp))
        )
    }

    if (showReorderDialog) {
        val currentPlayers = uiState.players.map { it.player }
        RearrangeSeatsDialog(
            initialPlayers = currentPlayers,
            onSave = { reorderedList ->
                viewModel.reorderPlayers(reorderedList.map { it.id }, gameSetId)
            },
            onDismiss = { showReorderDialog = false }
        )
    }

    roundForDetails?.let { round ->
        RoundDetailsDialog(
            round = round,
            currencySymbol = currencySymbol(uiState.settings.currency.displayName()),
            onDismiss = { roundForDetails = null }
        )
    }
}

private fun currencySymbol(displayName: String): String =
    displayName.substringAfter("(", "").substringBefore(")").ifEmpty { displayName }

@Composable
private fun PlayerStandingsRow(
    standings: PlayerStandings,
    isHost: Boolean,
    currentUserEmail: String,
    currencySymbol: String,
    onMapClick: () -> Unit,
    onNudgeClick: () -> Unit
) {
    val scoreColor = when {
        standings.netPoints > 0 -> Color(0xFF4CAF50)
        standings.netPoints < 0 -> Color(0xFFFF5252)
        else -> Color.White
    }
    val isGuest = standings.player.email.isBlank()

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .then(
                if (isHost && isGuest) Modifier.clickable { onMapClick() } else Modifier
            ),
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
                    if (isGuest) {
                        Spacer(modifier = Modifier.width(6.dp))
                        Icon(
                            imageVector = Icons.Default.Link,
                            contentDescription = "Link Account",
                            tint = GoldAccent.copy(alpha = 0.7f),
                            modifier = Modifier.size(16.dp)
                        )
                    }
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

            // Nudge Button (if caller is host, player is registered user, and player is not the host themselves)
            if (isHost && standings.player.email.isNotBlank() && standings.player.email != currentUserEmail) {
                IconButton(
                    onClick = onNudgeClick,
                    modifier = Modifier.padding(end = 4.dp)
                ) {
                    Icon(
                        imageVector = Icons.Default.NotificationsActive,
                        contentDescription = "Nudge Player",
                        tint = GoldAccent,
                        modifier = Modifier.size(20.dp)
                    )
                }
            }

            // Net Points + Money
            Column(horizontalAlignment = Alignment.End) {
                Text(
                    text = "${standings.netPoints} pts",
                    color = scoreColor,
                    fontSize = 15.sp,
                    fontWeight = FontWeight.Bold
                )
                Text(
                    text = "${String.format("%.0f", standings.totalMoney)}$currencySymbol",
                    color = scoreColor.copy(alpha = 0.7f),
                    fontSize = 11.sp
                )
            }
        }
    }
}

private enum class RoundDisplayMode { MAAL, POINTS }

private const val ROUND_SEQ_COL_WIDTH_DP = 26
private const val ROUND_PLAYER_COL_WIDTH_DP = 52

/**
 * Compact grid: one row per round, one column per player. Top row shows Maal or Points
 * (switchable), bottom row always shows money won. Tap the round number to see the full
 * per-player breakdown in a popup.
 */
@Composable
private fun CompactRoundsTable(
    rounds: List<RoundItem>,
    players: List<PlayerStandings>,
    currencySymbol: String,
    onRoundClick: (RoundItem) -> Unit
) {
    var mode by remember { mutableStateOf(RoundDisplayMode.MAAL) }
    val scrollState = rememberScrollState()

    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(8.dp),
        colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.03f))
    ) {
        Column {
            // Maal / Points mode selector
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(vertical = 6.dp),
                horizontalArrangement = Arrangement.Center
            ) {
                ModeTab("Maal", mode == RoundDisplayMode.MAAL) { mode = RoundDisplayMode.MAAL }
                Spacer(modifier = Modifier.width(8.dp))
                ModeTab("Points", mode == RoundDisplayMode.POINTS) { mode = RoundDisplayMode.POINTS }
            }

            HorizontalDivider(color = Color.White.copy(alpha = 0.08f))

            // Header: player initials
            Row(
                modifier = Modifier.padding(vertical = 6.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Spacer(modifier = Modifier.width(ROUND_SEQ_COL_WIDTH_DP.dp))
                Row(modifier = Modifier.horizontalScroll(scrollState)) {
                    players.forEach { p ->
                        Box(
                            modifier = Modifier.width(ROUND_PLAYER_COL_WIDTH_DP.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            Text(
                                text = p.player.name.take(3).uppercase(),
                                color = GoldAccent.copy(alpha = 0.8f),
                                fontSize = 10.sp,
                                fontWeight = FontWeight.Bold
                            )
                        }
                    }
                }
            }

            HorizontalDivider(color = Color.White.copy(alpha = 0.08f))

            rounds.forEachIndexed { index, round ->
                val rowBackground = if (index % 2 == 0) Color.White.copy(alpha = 0.06f) else Color.Transparent
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .background(rowBackground)
                        .padding(vertical = 3.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Box(
                        modifier = Modifier
                            .width(ROUND_SEQ_COL_WIDTH_DP.dp)
                            .clickable { onRoundClick(round) },
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            text = "${round.roundNumber}",
                            color = GoldAccent,
                            fontSize = 12.sp,
                            fontWeight = FontWeight.Bold
                        )
                    }

                    Row(modifier = Modifier.horizontalScroll(scrollState)) {
                        players.forEach { p ->
                            val entry = round.playerEntries.find { it.playerId == p.player.id }
                            CompactRoundCell(entry, mode, currencySymbol, modifier = Modifier.width(ROUND_PLAYER_COL_WIDTH_DP.dp))
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun ModeTab(label: String, selected: Boolean, onClick: () -> Unit) {
    Text(
        text = label,
        color = if (selected) GoldAccent else Color.White.copy(alpha = 0.4f),
        fontSize = 12.sp,
        fontWeight = if (selected) FontWeight.Bold else FontWeight.Normal,
        modifier = Modifier
            .clip(RoundedCornerShape(6.dp))
            .then(if (selected) Modifier.background(GoldAccent.copy(alpha = 0.15f)) else Modifier)
            .clickable(onClick = onClick)
            .padding(horizontal = 12.dp, vertical = 4.dp)
    )
}

@Composable
private fun CompactRoundCell(entry: RoundPlayerEntry?, mode: RoundDisplayMode, currencySymbol: String, modifier: Modifier = Modifier) {
    val topValue = when (mode) {
        RoundDisplayMode.MAAL -> entry?.maal ?: 0
        RoundDisplayMode.POINTS -> entry?.score ?: 0
    }
    val topColor = when {
        mode == RoundDisplayMode.POINTS && topValue > 0 -> Color(0xFF4CAF50)
        mode == RoundDisplayMode.POINTS && topValue < 0 -> Color(0xFFFF5252)
        else -> Color.White.copy(alpha = 0.85f)
    }
    val money = entry?.money ?: 0.0
    val moneyColor = when {
        money > 0 -> Color(0xFF4CAF50)
        money < 0 -> Color(0xFFFF5252)
        else -> Color.White.copy(alpha = 0.5f)
    }
    val cellAlpha = if (entry?.isSeen == true) 1f else 0.5f

    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text(
            text = "$topValue",
            color = topColor.copy(alpha = cellAlpha),
            fontSize = 13.sp,
            fontWeight = FontWeight.Bold
        )
        Text(
            text = "${String.format("%.0f", money)}$currencySymbol",
            color = moneyColor.copy(alpha = cellAlpha),
            fontSize = 10.sp
        )
    }
}

@Composable
private fun RoundDetailsDialog(round: RoundItem, currencySymbol: String, onDismiss: () -> Unit) {
    AlertDialog(
        onDismissRequest = onDismiss,
        title = {
            Text(
                text = "Round ${round.roundNumber} Details",
                color = GoldAccent,
                fontFamily = FontFamily.Serif,
                fontWeight = FontWeight.Bold
            )
        },
        text = {
            Column {
                Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.padding(bottom = 12.dp)) {
                    Icon(Icons.Default.EmojiEvents, null, tint = MarigoldOrange, modifier = Modifier.size(16.dp))
                    Spacer(modifier = Modifier.width(6.dp))
                    Text("${round.winnerName} won  ·  Total Maal: ${round.totalMaal}", color = Color.White.copy(alpha = 0.8f), fontSize = 13.sp)
                }
                LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    items(round.playerEntries) { entry ->
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .clip(RoundedCornerShape(8.dp))
                                .background(Color.White.copy(alpha = 0.05f))
                                .padding(10.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Column(modifier = Modifier.weight(1f)) {
                                Text(entry.playerName, color = Color.White, fontWeight = FontWeight.Bold, fontSize = 13.sp)
                                Row(verticalAlignment = Alignment.CenterVertically) {
                                    Text(
                                        if (entry.isSeen) "Seen" else "Unseen",
                                        color = if (entry.isSeen) Color(0xFF4CAF50) else Color.White.copy(alpha = 0.4f),
                                        fontSize = 11.sp
                                    )
                                    if (entry.isDublee) {
                                        Spacer(modifier = Modifier.width(6.dp))
                                        Icon(Icons.Default.Diversity3, "Dublee", tint = DeepRedTika, modifier = Modifier.size(12.dp))
                                    }
                                    Spacer(modifier = Modifier.width(6.dp))
                                    Text("Maal: ${entry.maal}", color = Color.White.copy(alpha = 0.5f), fontSize = 11.sp)
                                }
                            }
                            Column(horizontalAlignment = Alignment.End) {
                                Text(
                                    text = "${entry.score} pts",
                                    color = if (entry.score > 0) Color(0xFF4CAF50) else if (entry.score < 0) Color(0xFFFF5252) else Color.White,
                                    fontWeight = FontWeight.Bold,
                                    fontSize = 14.sp
                                )
                                Text(
                                    text = "${String.format("%.1f", entry.money)}$currencySymbol",
                                    color = Color.White.copy(alpha = 0.6f),
                                    fontSize = 11.sp
                                )
                            }
                        }
                    }
                }
            }
        },
        confirmButton = {
            TextButton(onClick = onDismiss) {
                Text("Close", color = GoldAccent)
            }
        },
        containerColor = TiharNightBlue,
        shape = RoundedCornerShape(16.dp),
        modifier = Modifier.border(1.dp, GoldAccent, RoundedCornerShape(16.dp))
    )
}
