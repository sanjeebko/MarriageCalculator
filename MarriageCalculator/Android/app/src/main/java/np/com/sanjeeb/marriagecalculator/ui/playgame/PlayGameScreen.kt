package np.com.sanjeeb.marriagecalculator.ui.playgame

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.core.tween
import androidx.compose.animation.expandVertically
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.animation.scaleIn
import androidx.compose.animation.scaleOut
import androidx.compose.animation.shrinkVertically
import androidx.compose.foundation.ScrollState
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.automirrored.filled.Undo
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.layout.onGloballyPositioned
import androidx.compose.ui.layout.positionInRoot
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.IntOffset
import androidx.compose.ui.unit.IntRect
import androidx.compose.ui.unit.IntSize
import androidx.compose.ui.unit.LayoutDirection
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Popup
import androidx.compose.ui.window.PopupPositionProvider
import androidx.compose.ui.window.PopupProperties
import androidx.hilt.navigation.compose.hiltViewModel
import coil.compose.AsyncImage
import coil.request.ImageRequest
import kotlinx.coroutines.delay
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
    var gameForDetails by remember { mutableStateOf<GameEntry?>(null) }
    var tooltipAnchor by remember { mutableStateOf<PlayerTooltipAnchor?>(null) }
    var standingsExpanded by remember { mutableStateOf(false) }
    var showOverflowMenu by remember { mutableStateOf(false) }
    var showDeleteGameSetConfirm by remember { mutableStateOf(false) }
    var showDeleteLastGameConfirm by remember { mutableStateOf(false) }
    var roundPendingDeletion by remember { mutableStateOf<RoundGroup?>(null) }

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
                    if (uiState.isHost) {
                        Box {
                            IconButton(onClick = { showOverflowMenu = true }) {
                                Icon(Icons.Default.MoreVert, "More options", tint = GoldAccent)
                            }
                            DropdownMenu(expanded = showOverflowMenu, onDismissRequest = { showOverflowMenu = false }) {
                                DropdownMenuItem(
                                    text = { Text("Delete Game", color = Color(0xFFFF5252)) },
                                    leadingIcon = { Icon(Icons.Default.DeleteForever, null, tint = Color(0xFFFF5252)) },
                                    onClick = {
                                        showOverflowMenu = false
                                        showDeleteGameSetConfirm = true
                                    }
                                )
                            }
                        }
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = TiharNightBlue)
            )
        },
        floatingActionButton = {
            if (!uiState.isSettled && uiState.isHost) {
                FloatingActionButton(
                    onClick = { onAddRound((uiState.totalGamesPlayed + 1).toString()) },
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

                CompactRoundsTable(
                    roundGroups = uiState.roundGroups,
                    players = uiState.players,
                    nextDealerId = uiState.nextDealerId,
                    currencySymbol = currencySymbol(uiState.settings.currency.displayName()),
                    pointRate = uiState.settings.pointRate,
                    isHost = uiState.isHost && !uiState.isSettled,
                    onGameClick = { gameForDetails = it },
                    onPlayerHeaderClick = { player, position, size -> tooltipAnchor = PlayerTooltipAnchor(player, position, size) },
                    onCloseRound = { viewModel.closeCurrentRound(gameSetId) },
                    onDeleteLastGame = { showDeleteLastGameConfirm = true },
                    onDeleteRound = { round -> roundPendingDeletion = round },
                    onReshuffle = { showReorderDialog = true }
                )

                Spacer(modifier = Modifier.height(24.dp))

                // Standings - collapsed by default, the Round table above is the primary content
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .clip(RoundedCornerShape(6.dp))
                        .clickable { standingsExpanded = !standingsExpanded }
                        .padding(vertical = 8.dp),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Text(
                            text = "STANDINGS",
                            color = GoldAccent,
                            fontSize = 12.sp,
                            fontWeight = FontWeight.Bold,
                            letterSpacing = 1.5.sp
                        )
                        Spacer(modifier = Modifier.width(6.dp))
                        Icon(
                            imageVector = if (standingsExpanded) Icons.Default.ExpandLess else Icons.Default.ExpandMore,
                            contentDescription = null,
                            tint = GoldAccent.copy(alpha = 0.6f),
                            modifier = Modifier.size(18.dp)
                        )
                    }
                    if (standingsExpanded && uiState.isHost && !uiState.isSettled) {
                        // A round's seat order is fixed once it starts - reshuffling only
                        // happens between rounds, so disable this while a round is in progress.
                        val roundInProgress = uiState.roundGroups.any { !it.isCompleted && it.games.isNotEmpty() }
                        Row(
                            modifier = Modifier
                                .clip(RoundedCornerShape(4.dp))
                                .then(if (!roundInProgress) Modifier.clickable { showReorderDialog = true } else Modifier)
                                .padding(horizontal = 6.dp, vertical = 2.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            val tint = if (roundInProgress) GoldAccent.copy(alpha = 0.3f) else GoldAccent
                            Icon(Icons.Default.Reorder, null, tint = tint, modifier = Modifier.size(16.dp))
                            Spacer(modifier = Modifier.width(4.dp))
                            Text(
                                text = if (roundInProgress) "Seats locked until round ends" else "Arrange Seats",
                                color = tint,
                                fontSize = 12.sp,
                                fontWeight = FontWeight.Bold
                            )
                        }
                    }
                }

                AnimatedVisibility(
                    visible = standingsExpanded,
                    enter = expandVertically(animationSpec = tween(200)) + fadeIn(animationSpec = tween(200)),
                    exit = shrinkVertically(animationSpec = tween(150)) + fadeOut(animationSpec = tween(150))
                ) {
                    val currencySymbolText = currencySymbol(uiState.settings.currency.displayName())
                    Column(modifier = Modifier.padding(top = 4.dp)) {
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
                            Spacer(modifier = Modifier.height(4.dp))
                        }
                    }
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

    gameForDetails?.let { game ->
        RoundDetailsDialog(
            game = game,
            currencySymbol = currencySymbol(uiState.settings.currency.displayName()),
            onDismiss = { gameForDetails = null }
        )
    }

    tooltipAnchor?.let { anchor ->
        PlayerNameTooltip(anchor = anchor, onDismiss = { tooltipAnchor = null })
    }

    if (showDeleteLastGameConfirm) {
        ConfirmDeleteDialog(
            title = "Delete Last Game?",
            message = "This removes the most recently played game and its scores. This cannot be undone.",
            onConfirm = {
                viewModel.deleteLastGame(gameSetId)
                showDeleteLastGameConfirm = false
            },
            onDismiss = { showDeleteLastGameConfirm = false }
        )
    }

    roundPendingDeletion?.let { round ->
        ConfirmDeleteDialog(
            title = "Delete Round ${round.roundSequence}?",
            message = "This removes all ${round.games.size} game(s) and their scores from this round. Later rounds will renumber down. This cannot be undone.",
            onConfirm = {
                viewModel.deleteRound(gameSetId, round)
                roundPendingDeletion = null
            },
            onDismiss = { roundPendingDeletion = null }
        )
    }

    if (showDeleteGameSetConfirm) {
        ConfirmDeleteDialog(
            title = "Delete This Game?",
            message = "This permanently deletes the entire game - every round, game, and score. This cannot be undone.",
            onConfirm = {
                showDeleteGameSetConfirm = false
                viewModel.deleteGameSet(gameSetId, onDeleted = onBack)
            },
            onDismiss = { showDeleteGameSetConfirm = false }
        )
    }
}

@Composable
private fun ConfirmDeleteDialog(title: String, message: String, onConfirm: () -> Unit, onDismiss: () -> Unit) {
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text(title, color = GoldAccent, fontFamily = FontFamily.Serif, fontWeight = FontWeight.Bold) },
        text = { Text(message, color = Color.White.copy(alpha = 0.7f), fontSize = 14.sp) },
        confirmButton = {
            TextButton(onClick = onConfirm) {
                Text("Delete", color = Color(0xFFFF5252), fontWeight = FontWeight.Bold)
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text("Cancel", color = GoldAccent)
            }
        },
        containerColor = TiharNightBlue,
        shape = RoundedCornerShape(16.dp),
        modifier = Modifier.border(1.dp, GoldAccent, RoundedCornerShape(16.dp))
    )
}

data class PlayerTooltipAnchor(val player: Player, val position: Offset, val size: IntSize)

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
        shape = RoundedCornerShape(6.dp),
        colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.05f))
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 8.dp, vertical = 6.dp),
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
                        .size(26.dp)
                        .clip(RoundedCornerShape(5.dp))
                )
            } else {
                Box(
                    modifier = Modifier
                        .size(26.dp)
                        .clip(RoundedCornerShape(5.dp))
                        .background(Color.DarkGray),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = standings.player.name.take(1).uppercase(),
                        color = Color.White,
                        fontWeight = FontWeight.Bold,
                        fontSize = 12.sp
                    )
                }
            }

            Spacer(modifier = Modifier.width(8.dp))

            // Player Name + Dealer Indicator
            Column(modifier = Modifier.weight(1f)) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Text(
                        text = standings.player.name,
                        color = Color.White,
                        fontSize = 13.sp,
                        fontWeight = FontWeight.Bold
                    )
                    if (isGuest) {
                        Spacer(modifier = Modifier.width(4.dp))
                        Icon(
                            imageVector = Icons.Default.Link,
                            contentDescription = "Link Account",
                            tint = GoldAccent.copy(alpha = 0.7f),
                            modifier = Modifier.size(13.dp)
                        )
                    }
                    if (standings.isNextDealer) {
                        Spacer(modifier = Modifier.width(4.dp))
                        Box(
                            modifier = Modifier
                                .clip(RoundedCornerShape(4.dp))
                                .background(GoldAccent.copy(alpha = 0.2f))
                                .border(0.5.dp, GoldAccent, RoundedCornerShape(4.dp))
                                .padding(horizontal = 4.dp, vertical = 1.dp)
                        ) {
                            Text(
                                text = "DEALER",
                                color = GoldAccent,
                                fontSize = 7.sp,
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
                    modifier = Modifier.size(28.dp)
                ) {
                    Icon(
                        imageVector = Icons.Default.NotificationsActive,
                        contentDescription = "Nudge Player",
                        tint = GoldAccent,
                        modifier = Modifier.size(16.dp)
                    )
                }
                Spacer(modifier = Modifier.width(2.dp))
            }

            // Net Points + Money
            Column(horizontalAlignment = Alignment.End) {
                Text(
                    text = "${standings.netPoints} pts",
                    color = scoreColor,
                    fontSize = 13.sp,
                    fontWeight = FontWeight.Bold
                )
                Text(
                    text = "${String.format("%.0f", standings.totalMoney)}$currencySymbol",
                    color = scoreColor.copy(alpha = 0.7f),
                    fontSize = 10.sp
                )
            }
        }
    }
}

private enum class RoundDisplayMode { MAAL, POINTS }

private const val ROUND_SEQ_COL_WIDTH_DP = 26
private const val ROUND_PLAYER_COL_WIDTH_DP = 52

private fun blankGameEntry(players: List<Player>, dealerId: String, sequenceInRound: Int): GameEntry {
    return GameEntry(
        gameId = "pending",
        gameSequenceInRound = sequenceInRound,
        dealerId = dealerId,
        winnerId = "",
        winnerName = "",
        totalMaal = 0,
        playerEntries = players.map { p ->
            RoundPlayerEntry(
                playerId = p.id,
                playerName = p.name,
                isSeen = false,
                isDublee = false,
                isWinner = false,
                maal = 0,
                score = 0,
                money = 0.0
            )
        }
    )
}

/**
 * A round (up to N games, N = player count, one deal per player) is laid out as a Column of
 * self-contained [RoundBlock]s - each one owns its own header row, game rows, total row, and
 * horizontal scroll, so it works like a repeater: every round is an independent unit that could
 * show a different player/seat order after a reshuffle. Only the Maal/Points mode toggle is
 * shared across all rounds, since it's a display preference rather than round data.
 */
@Composable
private fun CompactRoundsTable(
    roundGroups: List<RoundGroup>,
    players: List<PlayerStandings>,
    nextDealerId: String,
    currencySymbol: String,
    pointRate: Double,
    isHost: Boolean,
    onGameClick: (GameEntry) -> Unit,
    onPlayerHeaderClick: (Player, Offset, IntSize) -> Unit,
    onCloseRound: () -> Unit,
    onDeleteLastGame: () -> Unit,
    onDeleteRound: (RoundGroup) -> Unit,
    onReshuffle: () -> Unit
) {
    var mode by remember { mutableStateOf(RoundDisplayMode.MAAL) }

    val hasOpenRound = roundGroups.any { !it.isCompleted }
    val currentSeatOrder = players.map { it.player }
    val displayGroups = remember(roundGroups, players) {
        val groups = roundGroups.toMutableList()
        if (!hasOpenRound) {
            // Preview of the upcoming round - it uses the game set's current (possibly just
            // reshuffled) seat order, unlike the historical rounds above which keep their own.
            val nextSeq = (groups.maxOfOrNull { it.roundSequence } ?: 0) + 1
            groups.add(RoundGroup(roundId = "", roundSequence = nextSeq, isCompleted = false, seatOrder = currentSeatOrder))
        }
        groups.sortedByDescending { it.roundSequence }
    }

    Column {
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

        displayGroups.forEachIndexed { index, group ->
            RoundBlock(
                group = group,
                players = group.seatOrder.ifEmpty { currentSeatOrder },
                nextDealerId = nextDealerId,
                mode = mode,
                currencySymbol = currencySymbol,
                pointRate = pointRate,
                isHost = isHost,
                isLatestRound = index == 0,
                onGameClick = onGameClick,
                onPlayerHeaderClick = onPlayerHeaderClick,
                onCloseRound = onCloseRound,
                onDeleteLastGame = onDeleteLastGame,
                onDeleteRound = { onDeleteRound(group) },
                onReshuffle = onReshuffle
            )
            Spacer(modifier = Modifier.height(10.dp))
        }
    }
}

/** One self-contained round: its own header row, game rows, and total row - a repeatable unit. Renders the seat order this round was played with. */
@Composable
private fun RoundBlock(
    group: RoundGroup,
    players: List<Player>,
    nextDealerId: String,
    mode: RoundDisplayMode,
    currencySymbol: String,
    pointRate: Double,
    isHost: Boolean,
    isLatestRound: Boolean,
    onGameClick: (GameEntry) -> Unit,
    onPlayerHeaderClick: (Player, Offset, IntSize) -> Unit,
    onCloseRound: () -> Unit,
    onDeleteLastGame: () -> Unit,
    onDeleteRound: () -> Unit,
    onReshuffle: () -> Unit
) {
    val scrollState = rememberScrollState()

    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(8.dp),
        colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.03f))
    ) {
        Column {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 8.dp, vertical = 6.dp),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Text(
                        text = "Round ${group.roundSequence}" + when {
                            group.isCompleted -> ""
                            group.games.isEmpty() -> " · not started"
                            else -> " · in progress"
                        },
                        color = GoldAccent,
                        fontSize = 12.sp,
                        fontWeight = FontWeight.Bold
                    )
                    // Reshuffling only happens between rounds, so the icon lives on the round
                    // that hasn't started yet - it configures that round's seating.
                    if (isHost && !group.isCompleted && group.games.isEmpty()) {
                        IconButton(onClick = onReshuffle, modifier = Modifier.size(24.dp)) {
                            Icon(
                                imageVector = Icons.Default.Shuffle,
                                contentDescription = "Reshuffle seats",
                                tint = MarigoldOrange,
                                modifier = Modifier.size(14.dp)
                            )
                        }
                    }
                }
                Row(verticalAlignment = Alignment.CenterVertically) {
                    if (isHost && !group.isCompleted && group.games.isNotEmpty()) {
                        Text(
                            text = "Close Round",
                            color = DeepRedTika,
                            fontSize = 10.sp,
                            fontWeight = FontWeight.Bold,
                            modifier = Modifier
                                .clip(RoundedCornerShape(4.dp))
                                .clickable(onClick = onCloseRound)
                                .padding(horizontal = 6.dp, vertical = 2.dp)
                        )
                    }
                    if (isHost && isLatestRound && group.games.isNotEmpty()) {
                        IconButton(onClick = onDeleteLastGame, modifier = Modifier.size(24.dp)) {
                            Icon(
                                imageVector = Icons.AutoMirrored.Filled.Undo,
                                contentDescription = "Undo last game",
                                tint = GoldAccent.copy(alpha = 0.7f),
                                modifier = Modifier.size(14.dp)
                            )
                        }
                    }
                    if (isHost && group.games.isNotEmpty()) {
                        IconButton(onClick = onDeleteRound, modifier = Modifier.size(24.dp)) {
                            Icon(
                                imageVector = Icons.Default.DeleteOutline,
                                contentDescription = "Delete round",
                                tint = Color(0xFFFF5252).copy(alpha = 0.7f),
                                modifier = Modifier.size(14.dp)
                            )
                        }
                    }
                }
            }

            HorizontalDivider(color = Color.White.copy(alpha = 0.08f))

            // Header: player initials, tappable for a tooltip with the full name
            Row(
                modifier = Modifier.padding(vertical = 6.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Spacer(modifier = Modifier.width(ROUND_SEQ_COL_WIDTH_DP.dp))
                Row(modifier = Modifier.horizontalScroll(scrollState)) {
                    players.forEach { p ->
                        var headerPosition by remember { mutableStateOf(Offset.Zero) }
                        var headerSize by remember { mutableStateOf(IntSize.Zero) }
                        Box(
                            modifier = Modifier
                                .width(ROUND_PLAYER_COL_WIDTH_DP.dp)
                                .onGloballyPositioned { coords ->
                                    headerPosition = coords.positionInRoot()
                                    headerSize = coords.size
                                }
                                .clickable { onPlayerHeaderClick(p, headerPosition, headerSize) },
                            contentAlignment = Alignment.Center
                        ) {
                            Text(
                                text = p.name.take(3).uppercase(),
                                color = GoldAccent.copy(alpha = 0.8f),
                                fontSize = 10.sp,
                                fontWeight = FontWeight.Bold
                            )
                        }
                    }
                }
            }

            HorizontalDivider(color = Color.White.copy(alpha = 0.08f))

            val pendingSeq = group.games.size + 1
            val rowsAscending = if (!group.isCompleted) {
                group.games + blankGameEntry(players, nextDealerId, pendingSeq)
            } else group.games
            val rowsDisplay = rowsAscending.sortedByDescending { it.gameSequenceInRound }

            rowsDisplay.forEachIndexed { index, game ->
                val isPending = game.gameId == "pending"
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
                            .then(if (!isPending) Modifier.clickable { onGameClick(game) } else Modifier),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            text = "${game.gameSequenceInRound}",
                            color = if (isPending) Color.White.copy(alpha = 0.3f) else GoldAccent,
                            fontSize = 12.sp,
                            fontWeight = FontWeight.Bold
                        )
                    }

                    Row(modifier = Modifier.horizontalScroll(scrollState)) {
                        players.forEach { p ->
                            val entry = game.playerEntries.find { it.playerId == p.id }
                            CompactRoundCell(
                                entry = entry,
                                mode = mode,
                                currencySymbol = currencySymbol,
                                isDealer = game.dealerId == p.id,
                                isWinner = !isPending && game.winnerId == p.id,
                                modifier = Modifier.width(ROUND_PLAYER_COL_WIDTH_DP.dp)
                            )
                        }
                    }
                }
            }

            if (group.games.isNotEmpty()) {
                HorizontalDivider(color = Color.White.copy(alpha = 0.1f), modifier = Modifier.padding(horizontal = 8.dp))
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 4.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Box(modifier = Modifier.width(ROUND_SEQ_COL_WIDTH_DP.dp), contentAlignment = Alignment.Center) {
                        Text("Σ", color = GoldAccent.copy(alpha = 0.6f), fontSize = 12.sp, fontWeight = FontWeight.Bold)
                    }
                    Row(modifier = Modifier.horizontalScroll(scrollState)) {
                        players.forEach { p ->
                            val points = group.totalScoreByPlayer[p.id] ?: 0
                            val money = points * pointRate
                            Box(modifier = Modifier.width(ROUND_PLAYER_COL_WIDTH_DP.dp), contentAlignment = Alignment.Center) {
                                Text(
                                    text = "${String.format("%.0f", money)}$currencySymbol",
                                    color = when {
                                        money > 0 -> Color(0xFF4CAF50)
                                        money < 0 -> Color(0xFFFF5252)
                                        else -> Color.White.copy(alpha = 0.6f)
                                    },
                                    fontSize = 11.sp,
                                    fontWeight = FontWeight.Bold
                                )
                            }
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
private fun CompactRoundCell(
    entry: RoundPlayerEntry?,
    mode: RoundDisplayMode,
    currencySymbol: String,
    isDealer: Boolean,
    isWinner: Boolean = false,
    modifier: Modifier = Modifier
) {
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

    // The winner is fixed once a game is submitted - mark their cell with a frosted-glass pill:
    // rounded rectangle, translucent gradient fill, and a light-catching gradient border.
    val winnerGlass = if (isWinner) {
        Modifier
            .padding(horizontal = 2.dp)
            .clip(RoundedCornerShape(8.dp))
            .background(
                Brush.verticalGradient(
                    listOf(
                        Color.White.copy(alpha = 0.22f),
                        Color(0xFF4CAF50).copy(alpha = 0.10f),
                        Color.White.copy(alpha = 0.04f)
                    )
                )
            )
            .border(
                width = 1.dp,
                brush = Brush.verticalGradient(
                    listOf(
                        Color.White.copy(alpha = 0.50f),
                        Color(0xFF4CAF50).copy(alpha = 0.25f),
                        Color.White.copy(alpha = 0.06f)
                    )
                ),
                shape = RoundedCornerShape(8.dp)
            )
            .padding(vertical = 2.dp)
    } else Modifier

    Column(
        modifier = modifier.then(winnerGlass),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Row(verticalAlignment = Alignment.CenterVertically) {
            Text(
                text = "$topValue",
                color = topColor.copy(alpha = cellAlpha),
                fontSize = 13.sp,
                fontWeight = FontWeight.Bold
            )
            if (isDealer) {
                Spacer(modifier = Modifier.width(3.dp))
                Box(
                    modifier = Modifier
                        .size(12.dp)
                        .clip(CircleShape)
                        .background(GoldAccent.copy(alpha = 0.25f)),
                    contentAlignment = Alignment.Center
                ) {
                    Text("D", color = GoldAccent, fontSize = 8.sp, fontWeight = FontWeight.Bold)
                }
            }
        }
        Text(
            text = "${String.format("%.0f", money)}$currencySymbol",
            color = moneyColor.copy(alpha = cellAlpha),
            fontSize = 10.sp
        )
    }
}

@Composable
private fun RoundDetailsDialog(game: GameEntry, currencySymbol: String, onDismiss: () -> Unit) {
    val dealerName = game.playerEntries.find { it.playerId == game.dealerId }?.playerName
    AlertDialog(
        onDismissRequest = onDismiss,
        title = {
            Text(
                text = "Game ${game.gameSequenceInRound} Details",
                color = GoldAccent,
                fontFamily = FontFamily.Serif,
                fontWeight = FontWeight.Bold
            )
        },
        text = {
            Column {
                Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.padding(bottom = 4.dp)) {
                    Icon(Icons.Default.EmojiEvents, null, tint = MarigoldOrange, modifier = Modifier.size(16.dp))
                    Spacer(modifier = Modifier.width(6.dp))
                    Text("${game.winnerName} won  ·  Total Maal: ${game.totalMaal}", color = Color.White.copy(alpha = 0.8f), fontSize = 13.sp)
                }
                if (!dealerName.isNullOrBlank()) {
                    Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.padding(bottom = 12.dp)) {
                        Icon(Icons.Default.Casino, null, tint = GoldAccent.copy(alpha = 0.7f), modifier = Modifier.size(14.dp))
                        Spacer(modifier = Modifier.width(6.dp))
                        Text("Dealer: $dealerName", color = Color.White.copy(alpha = 0.6f), fontSize = 12.sp)
                    }
                } else {
                    Spacer(modifier = Modifier.height(8.dp))
                }
                LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                    items(game.playerEntries) { entry ->
                        val winnerGlow = Color(0xFF4CAF50)
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .then(
                                    if (entry.isWinner) {
                                        Modifier.shadow(
                                            elevation = 10.dp,
                                            shape = RoundedCornerShape(8.dp),
                                            ambientColor = winnerGlow,
                                            spotColor = winnerGlow
                                        )
                                    } else Modifier
                                )
                                .clip(RoundedCornerShape(8.dp))
                                .background(if (entry.isWinner) winnerGlow.copy(alpha = 0.15f) else Color.White.copy(alpha = 0.05f))
                                .then(
                                    if (entry.isWinner) {
                                        Modifier.border(1.5.dp, winnerGlow.copy(alpha = 0.7f), RoundedCornerShape(8.dp))
                                    } else Modifier
                                )
                                .padding(10.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Column(modifier = Modifier.weight(1f)) {
                                Row(verticalAlignment = Alignment.CenterVertically) {
                                    Text(entry.playerName, color = Color.White, fontWeight = FontWeight.Bold, fontSize = 13.sp)
                                    if (entry.isWinner) {
                                        Spacer(modifier = Modifier.width(4.dp))
                                        Icon(
                                            imageVector = Icons.Default.EmojiEvents,
                                            contentDescription = "Winner",
                                            tint = MarigoldOrange,
                                            modifier = Modifier.size(14.dp)
                                        )
                                    }
                                    if (entry.playerId == game.dealerId) {
                                        Spacer(modifier = Modifier.width(4.dp))
                                        Box(
                                            modifier = Modifier
                                                .size(14.dp)
                                                .clip(CircleShape)
                                                .background(GoldAccent.copy(alpha = 0.25f)),
                                            contentAlignment = Alignment.Center
                                        ) {
                                            Text("D", color = GoldAccent, fontSize = 8.sp, fontWeight = FontWeight.Bold)
                                        }
                                    }
                                }
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

/** Positions a tooltip just above the tapped header cell, flipping below if there isn't room. */
private class TooltipAbovePositionProvider(
    private val anchorPosition: Offset,
    private val anchorSize: IntSize,
    private val verticalGapPx: Int
) : PopupPositionProvider {
    override fun calculatePosition(
        anchorBounds: IntRect,
        windowSize: IntSize,
        layoutDirection: LayoutDirection,
        popupContentSize: IntSize
    ): IntOffset {
        val anchorCenterX = anchorPosition.x + anchorSize.width / 2f
        val x = (anchorCenterX - popupContentSize.width / 2f).toInt()
            .coerceIn(8, (windowSize.width - popupContentSize.width - 8).coerceAtLeast(8))
        var y = (anchorPosition.y - popupContentSize.height - verticalGapPx).toInt()
        if (y < 0) {
            // Not enough room above - show below the header instead.
            y = (anchorPosition.y + anchorSize.height + verticalGapPx).toInt()
        }
        return IntOffset(x, y)
    }
}

/**
 * Compact web-style tooltip: appears just above the tapped player-name header (or below it if
 * there's no room), dismisses on outside tap or automatically after 3 seconds. Does not cover
 * the rest of the screen.
 */
@Composable
private fun PlayerNameTooltip(anchor: PlayerTooltipAnchor, onDismiss: () -> Unit) {
    val gapPx = with(LocalDensity.current) { 8.dp.roundToPx() }

    LaunchedEffect(anchor) {
        delay(3000)
        onDismiss()
    }

    Popup(
        popupPositionProvider = remember(anchor) {
            TooltipAbovePositionProvider(anchor.position, anchor.size, gapPx)
        },
        onDismissRequest = onDismiss,
        properties = PopupProperties(focusable = false, dismissOnClickOutside = true)
    ) {
        var visible by remember { mutableStateOf(false) }
        LaunchedEffect(Unit) { visible = true }

        AnimatedVisibility(
            visible = visible,
            enter = fadeIn(animationSpec = tween(160)) + scaleIn(initialScale = 0.85f, animationSpec = tween(160)),
            exit = fadeOut(animationSpec = tween(120)) + scaleOut(targetScale = 0.85f, animationSpec = tween(120))
        ) {
            Card(
                shape = RoundedCornerShape(12.dp),
                colors = CardDefaults.cardColors(containerColor = TiharNightBlue),
                border = androidx.compose.foundation.BorderStroke(1.dp, GoldAccent.copy(alpha = 0.5f)),
                elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
            ) {
                Row(
                    modifier = Modifier.padding(horizontal = 12.dp, vertical = 8.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    val uri = anchor.player.photoUri
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
                                .size(32.dp)
                                .clip(CircleShape)
                                .border(1.dp, GoldAccent, CircleShape)
                        )
                    } else {
                        Box(
                            modifier = Modifier
                                .size(32.dp)
                                .clip(CircleShape)
                                .background(GoldAccent.copy(alpha = 0.2f))
                                .border(1.dp, GoldAccent, CircleShape),
                            contentAlignment = Alignment.Center
                        ) {
                            Text(anchor.player.name.take(1).uppercase(), color = GoldAccent, fontSize = 13.sp, fontWeight = FontWeight.Bold)
                        }
                    }
                    Spacer(modifier = Modifier.width(8.dp))
                    Column {
                        Text(
                            text = anchor.player.name,
                            color = Color.White,
                            fontSize = 13.sp,
                            fontWeight = FontWeight.Bold,
                            fontFamily = FontFamily.Serif
                        )
                        if (anchor.player.email.isNotBlank()) {
                            Text(anchor.player.email, color = Color.White.copy(alpha = 0.5f), fontSize = 10.sp)
                        }
                    }
                }
            }
        }
    }
}
