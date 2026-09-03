package np.com.sanjeeb.marriagecalculator.ui.scoreboard

import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme

import android.app.Activity
import android.content.Context
import android.content.ContextWrapper
import android.content.pm.ActivityInfo
import androidx.compose.foundation.ScrollState
import androidx.compose.foundation.background
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
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
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import np.com.sanjeeb.marriagecalculator.data.model.Currency
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.ui.components.AppBackground

private fun Context.findActivity(): Activity? {
    var context = this
    while (context is ContextWrapper) {
        if (context is Activity) return context
        context = context.baseContext
    }
    return null
}

/**
 * Locks the host Activity to the given orientation while this composable is on screen,
 * restoring whatever orientation setting was active before it entered composition.
 */
@Composable
private fun LockScreenOrientation(orientation: Int) {
    val context = LocalContext.current
    DisposableEffect(orientation) {
        val activity = context.findActivity() ?: return@DisposableEffect onDispose {}
        val originalOrientation = activity.requestedOrientation
        activity.requestedOrientation = orientation
        onDispose {
            activity.requestedOrientation = originalOrientation
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ScoreboardScreen(
    gameSetId: String,
    onBack: () -> Unit,
    viewModel: ScoreboardViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(gameSetId) {
        viewModel.loadScoreboardData(gameSetId)
    }

    // The round history table has many player columns, so rotate to landscape while
    // it's showing to fit more of them on screen at once.
    if (uiState.showHistory) {
        LockScreenOrientation(ActivityInfo.SCREEN_ORIENTATION_SENSOR_LANDSCAPE)
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Text("Scoreboard", color = AppTheme.palette.accent, fontFamily = FontFamily.Serif, fontWeight = FontWeight.Bold)
                },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, null, tint = AppTheme.palette.accent)
                    }
                },
                actions = {
                    IconButton(onClick = { viewModel.toggleHistory() }) {
                        Icon(
                            if (uiState.showHistory) Icons.Default.TableChart else Icons.Default.History,
                            null,
                            tint = AppTheme.palette.accent
                        )
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = Color.Transparent)
            )
        },
        containerColor = Color.Transparent
    ) { padding ->
        AppBackground {
            Box(
                modifier = Modifier
                    .fillMaxSize()
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
}

@Composable
private fun ScoreboardView(uiState: ScoreboardUiState, onSettle: () -> Unit) {
    val sortedPlayers = uiState.players.sortedByDescending { it.totalPoints }
    val currency = uiState.settings.currency

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
                WhoOwesWhomSection(sortedPlayers, uiState.settings.pointRate, currency)
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
                    colors = ButtonDefaults.buttonColors(containerColor = AppTheme.palette.cta)
                ) {
                    Icon(Icons.Default.AccountBalance, null, tint = AppTheme.palette.accent)
                    Spacer(modifier = Modifier.width(8.dp))
                    Text("Settle & Freeze", color = AppTheme.palette.accent, fontWeight = FontWeight.Bold)
                }
            }
        }
    }
}

@Composable
private fun PlayerScoreRow(playerScore: PlayerTotalScore, rank: Int, currency: Currency) {
    val scoreColor = when {
        playerScore.totalPoints > 0 -> AppTheme.palette.numberPositive
        playerScore.totalPoints < 0 -> AppTheme.palette.numberNegative
        else -> AppTheme.palette.numberZero
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
            containerColor = if (rank == 1) AppTheme.palette.cta.copy(alpha = 0.3f) else AppTheme.palette.tint.copy(alpha = 0.05f)
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
                    .background(if (rank == 1) AppTheme.palette.accent else AppTheme.palette.tint.copy(alpha = 0.1f)),
                contentAlignment = Alignment.Center
            ) {
                if (medalEmoji.isNotEmpty()) {
                    Text(medalEmoji, fontSize = 18.sp)
                } else {
                    Text(
                        "#$rank",
                        color = AppTheme.palette.textPrimary,
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
                    color = AppTheme.palette.textPrimary,
                    fontSize = 16.sp,
                    fontWeight = FontWeight.Bold
                )
                Text(
                    "${playerScore.gamesWon}W / ${playerScore.gamesPlayed}G",
                    color = AppTheme.palette.tint.copy(alpha = 0.4f),
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
                    currency.formatMoney(playerScore.totalMoney),
                    color = scoreColor.copy(alpha = 0.7f),
                    fontSize = 12.sp
                )
            }
        }
    }
}

@Composable
private fun WhoOwesWhomSection(players: List<PlayerTotalScore>, pointRate: Double, currency: Currency) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = AppTheme.palette.tint.copy(alpha = 0.05f))
    ) {
        Column(modifier = Modifier.padding(12.dp)) {
            Text(
                "💰 Settlement Summary",
                color = AppTheme.palette.accent,
                fontSize = 16.sp,
                fontWeight = FontWeight.Bold,
                fontFamily = FontFamily.Serif
            )
            Spacer(modifier = Modifier.height(8.dp))

            // Greedy matching algorithm for settlement:
            val receivers = players.filter { it.totalMoney > 0 }
                .map { it.player.name to it.totalMoney }
                .sortedByDescending { it.second }
                .toMutableList()

            val givers = players.filter { it.totalMoney < 0 }
                .map { it.player.name to -it.totalMoney }
                .sortedByDescending { it.second }
                .toMutableList()

            if (receivers.isEmpty() || givers.isEmpty()) {
                Text("All settled!", color = AppTheme.palette.tint.copy(alpha = 0.5f))
                return@Column
            }

            var printedAny = false
            var giverIdx = 0
            var receiverIdx = 0

            while (giverIdx < givers.size && receiverIdx < receivers.size) {
                val (giverName, giverAmt) = givers[giverIdx]
                val (receiverName, receiverAmt) = receivers[receiverIdx]

                val amount = kotlin.math.min(giverAmt, receiverAmt)
                if (amount > 0.01) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(vertical = 2.dp),
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        Text(
                            "$giverName → $receiverName",
                            color = AppTheme.palette.tint.copy(alpha = 0.7f),
                            fontSize = 13.sp
                        )
                        Text(
                            currency.formatMoney(amount),
                            color = AppTheme.palette.accent,
                            fontSize = 13.sp,
                            fontWeight = FontWeight.Bold
                        )
                    }
                    printedAny = true
                }

                givers[giverIdx] = giverName to (giverAmt - amount)
                receivers[receiverIdx] = receiverName to (receiverAmt - amount)

                if (givers[giverIdx].second < 0.01) {
                    giverIdx++
                }
                if (receivers[receiverIdx].second < 0.01) {
                    receiverIdx++
                }
            }

            if (!printedAny) {
                Text("All settled!", color = AppTheme.palette.tint.copy(alpha = 0.5f))
            }
        }
    }
}

// Alternating festive backgrounds for each round block (cycled), matching the
// Dashain/Tihar palette used elsewhere in the app.
private val ROUND_BLOCK_COLORS = listOf(
    Color(0xFF1B3B2E), // deep green
    Color(0xFF3B3416), // deep gold/olive
    Color(0xFF3B2416)  // deep marigold
)

private const val LABEL_COL_WIDTH_DP = 68
private const val PLAYER_COL_WIDTH_DP = 84
private const val TOTAL_COL_WIDTH_DP = 84
private const val CELL_ROW_HEIGHT_DP = 22

@Composable
private fun TableCell(text: String, widthDp: Int, color: Color, bold: Boolean = false) {
    Box(
        modifier = Modifier
            .width(widthDp.dp)
            .height(CELL_ROW_HEIGHT_DP.dp),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text,
            color = color,
            fontSize = 12.sp,
            fontWeight = if (bold) FontWeight.Bold else FontWeight.Normal,
            textAlign = TextAlign.Center,
            maxLines = 1
        )
    }
}

/**
 * Full spreadsheet-style breakdown: one block per round with Seen/Dublee/Maal/Points/Money
 * sub-rows per player, a Total Maal column, and a grand-total row at the bottom.
 */
@Composable
private fun RoundHistoryView(uiState: ScoreboardUiState) {
    if (uiState.rounds.isEmpty()) {
        Box(
            modifier = Modifier.fillMaxSize(),
            contentAlignment = Alignment.Center
        ) {
            Text("No rounds played yet", color = AppTheme.palette.tint.copy(alpha = 0.5f))
        }
        return
    }

    val horizontalScrollState = rememberScrollState()
    val players = uiState.players.map { it.player }
    val currency = uiState.settings.currency

    Column(modifier = Modifier.fillMaxSize().padding(vertical = 12.dp)) {
        Text(
            "Rate: ${uiState.settings.pointRate} ${currency.displayName().substringAfter("(", "").substringBefore(")")} / point",
            color = AppTheme.palette.tint.copy(alpha = 0.5f),
            fontSize = 11.sp,
            modifier = Modifier.padding(horizontal = 12.dp, vertical = 4.dp)
        )

        // Header row: player names + Total Maal, pinned above the scrolling round list
        // but scrolled horizontally in sync with the round blocks below.
        Row(
            modifier = Modifier
                .horizontalScroll(horizontalScrollState)
                .padding(horizontal = 12.dp)
        ) {
            TableCell("", LABEL_COL_WIDTH_DP, Color.Transparent)
            players.forEach { p ->
                TableCell(p.name, PLAYER_COL_WIDTH_DP, AppTheme.palette.accent, bold = true)
            }
            TableCell("Total\nMaal", TOTAL_COL_WIDTH_DP, AppTheme.palette.accent, bold = true)
        }
        HorizontalDivider(color = AppTheme.palette.tint.copy(alpha = 0.15f), modifier = Modifier.padding(top = 4.dp))

        LazyColumn(
            modifier = Modifier.fillMaxSize(),
            contentPadding = PaddingValues(vertical = 8.dp)
        ) {
            items(uiState.rounds) { round ->
                RoundBlock(round, players, horizontalScrollState, currency)
                Spacer(modifier = Modifier.height(8.dp))
            }

            item {
                TotalRow(uiState.players, players, horizontalScrollState, currency)
            }
        }
    }
}

@Composable
private fun RoundBlock(
    round: RoundSummary,
    players: List<Player>,
    scrollState: ScrollState,
    currency: Currency
) {
    val blockColor = ROUND_BLOCK_COLORS[(round.roundNumber - 1).mod(ROUND_BLOCK_COLORS.size)]

    Column(
        modifier = Modifier
            .fillMaxWidth()
            .background(blockColor)
            .padding(vertical = 6.dp)
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 12.dp),
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            Text(
                "Round ${round.roundNumber}",
                color = AppTheme.palette.accent,
                fontWeight = FontWeight.Bold,
                fontSize = 13.sp
            )
            Text("👑 ${round.winnerName}", color = AppTheme.palette.accentAlt, fontSize = 12.sp)
        }

        Row(
            modifier = Modifier
                .horizontalScroll(scrollState)
                .padding(horizontal = 12.dp, vertical = 4.dp)
        ) {
            Column {
                TableCell("Seen", LABEL_COL_WIDTH_DP, AppTheme.palette.tint.copy(alpha = 0.6f))
                TableCell("Dublee", LABEL_COL_WIDTH_DP, AppTheme.palette.tint.copy(alpha = 0.6f))
                TableCell("Maal", LABEL_COL_WIDTH_DP, AppTheme.palette.tint.copy(alpha = 0.6f))
                TableCell("Points", LABEL_COL_WIDTH_DP, AppTheme.palette.tint.copy(alpha = 0.6f))
                TableCell("Money", LABEL_COL_WIDTH_DP, AppTheme.palette.tint.copy(alpha = 0.6f))
            }
            players.forEach { p ->
                val entry = round.playerEntries.find { it.playerId == p.id }
                Column {
                    TableCell(
                        if (entry?.isSeen == true) "Yes" else "No",
                        PLAYER_COL_WIDTH_DP,
                        if (entry?.isSeen == true) AppTheme.palette.numberPositive else AppTheme.palette.tint.copy(alpha = 0.5f)
                    )
                    TableCell(
                        if (entry?.isDublee == true) "Yes" else "-",
                        PLAYER_COL_WIDTH_DP,
                        AppTheme.palette.tint.copy(alpha = 0.7f)
                    )
                    TableCell("${entry?.maal ?: 0}", PLAYER_COL_WIDTH_DP, AppTheme.palette.textPrimary)
                    val score = entry?.score ?: 0
                    TableCell(
                        "${if (score > 0) "+" else ""}$score",
                        PLAYER_COL_WIDTH_DP,
                        if (score > 0) AppTheme.palette.numberPositive else if (score < 0) AppTheme.palette.numberNegative else AppTheme.palette.textPrimary,
                        bold = true
                    )
                    TableCell(
                        currency.formatMoney(entry?.money ?: 0.0),
                        PLAYER_COL_WIDTH_DP,
                        AppTheme.palette.tint.copy(alpha = 0.8f)
                    )
                }
            }
            Column {
                repeat(2) { TableCell("", TOTAL_COL_WIDTH_DP, Color.Transparent) }
                TableCell("${round.totalMaal}", TOTAL_COL_WIDTH_DP, AppTheme.palette.accent, bold = true)
                repeat(2) { TableCell("", TOTAL_COL_WIDTH_DP, Color.Transparent) }
            }
        }
    }
}

@Composable
private fun TotalRow(
    playerTotals: List<PlayerTotalScore>,
    players: List<Player>,
    scrollState: ScrollState,
    currency: Currency
) {
    HorizontalDivider(color = AppTheme.palette.tint.copy(alpha = 0.2f), modifier = Modifier.padding(horizontal = 12.dp, vertical = 4.dp))
    Row(
        modifier = Modifier
            .horizontalScroll(scrollState)
            .padding(horizontal = 12.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        TableCell("Total", LABEL_COL_WIDTH_DP, AppTheme.palette.accent, bold = true)
        players.forEach { p ->
            val total = playerTotals.find { it.player.id == p.id }?.totalMoney ?: 0.0
            TableCell(
                currency.formatMoney(total),
                PLAYER_COL_WIDTH_DP,
                if (total > 0) AppTheme.palette.numberPositive else if (total < 0) AppTheme.palette.numberNegative else AppTheme.palette.textPrimary,
                bold = true
            )
        }
        TableCell("", TOTAL_COL_WIDTH_DP, Color.Transparent)
    }
}
