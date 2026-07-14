package np.com.sanjeeb.marriagecalculator.ui.roundinput

import np.com.sanjeeb.marriagecalculator.ui.components.DealerBadge
import np.com.sanjeeb.marriagecalculator.ui.components.GlassButton
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.BasicTextField
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
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import np.com.sanjeeb.marriagecalculator.data.model.Currency
import coil.compose.AsyncImage
import coil.request.ImageRequest
import java.io.File

// Column widths for the compact score grid — header and rows must stay in sync.
private val WinnerColWidth = 36.dp
private val CheckColWidth = 40.dp
private val MaalColWidth = 88.dp

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun RoundInputScreen(
    gameSetId: String,
    roundId: String,
    editGameId: String? = null,
    onScoreSubmitted: () -> Unit,
    onBack: () -> Unit,
    viewModel: RoundInputViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    val roundNumber = roundId.toIntOrNull() ?: 1
    var maalDialogPlayerId by remember { mutableStateOf<String?>(null) }

    LaunchedEffect(gameSetId) {
        viewModel.loadGameData(gameSetId, roundNumber, editGameId)
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
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, null, tint = AppTheme.palette.accent)
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = Color.Transparent)
            )
        },
        containerColor = AppTheme.palette.backgroundBottom
    ) { padding ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .background(Brush.verticalGradient(listOf(AppTheme.palette.backgroundTop, AppTheme.palette.backgroundBottom)))
                .padding(padding)
        ) {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .verticalScroll(rememberScrollState())
                    .padding(horizontal = 12.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                // Compact header: title + total maal on one line
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.SpaceBetween,
                    modifier = Modifier.fillMaxWidth().padding(horizontal = 4.dp)
                ) {
                    Column {
                        Text(
                            text = if (uiState.editGameId != null) "EDITING PREVIOUS GAME" else "CURRENT MATCH",
                            color = AppTheme.palette.accent.copy(alpha = 0.8f),
                            fontSize = 10.sp,
                            fontWeight = FontWeight.Bold,
                            letterSpacing = 2.sp
                        )
                        Text(
                            text = when {
                                uiState.editGameId != null -> "Edit Game"
                                uiState.gameNumber != null -> "Round $roundNumber · Game ${uiState.gameNumber}"
                                else -> "Round $roundNumber"
                            },
                            color = AppTheme.palette.textPrimary,
                            fontSize = 24.sp,
                            fontFamily = FontFamily.Serif,
                            fontWeight = FontWeight.Bold
                        )
                    }

                    // Total Maal chip (includes the dublee winner's fixed +5 bonus)
                    val dubleeBonusApplied = uiState.playerStates.any { it.isWinner && it.duply } &&
                        uiState.settings.dublee
                    val totalMaal = uiState.playerStates.sumOf { if (it.seen) it.seenPoints else 0 } +
                        (if (dubleeBonusApplied) RoundInputViewModel.DUBLEE_WINNER_MAAL_BONUS else 0)
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        modifier = Modifier
                            .clip(RoundedCornerShape(8.dp))
                            .background(AppTheme.palette.accent.copy(alpha = 0.15f))
                            .border(1.dp, AppTheme.palette.accent.copy(alpha = 0.4f), RoundedCornerShape(8.dp))
                            .padding(horizontal = 10.dp, vertical = 6.dp)
                    ) {
                        Icon(Icons.Default.Casino, null, tint = AppTheme.palette.accent, modifier = Modifier.size(14.dp))
                        Spacer(modifier = Modifier.width(6.dp))
                        Text("Maal", color = AppTheme.palette.tint.copy(alpha = 0.8f), fontSize = 12.sp)
                        Spacer(modifier = Modifier.width(4.dp))
                        Text("$totalMaal", color = AppTheme.palette.accent, fontWeight = FontWeight.Bold, fontSize = 14.sp)
                    }
                }

                Spacer(modifier = Modifier.height(12.dp))

                // ---- Compact score grid ----
                Column(
                    modifier = Modifier
                        .fillMaxWidth()
                        .clip(RoundedCornerShape(12.dp))
                        .background(Color(0xFF1E1E1E))
                        .border(1.dp, AppTheme.palette.tint.copy(alpha = 0.08f), RoundedCornerShape(12.dp))
                ) {
                    ScoreGridHeader()

                    uiState.playerStates.forEachIndexed { index, playerState ->
                        PlayerScoreRow(
                            state = playerState,
                            striped = index % 2 == 1,
                            showPreview = uiState.showPreview,
                            currency = uiState.settings.currency,
                            onSelectWinner = { viewModel.setWinner(playerState.player.id) },
                            onToggleSeen = { viewModel.toggleSeen(playerState.player.id) },
                            onToggleDuply = { viewModel.toggleDuply(playerState.player.id) },
                            onMaalPointsChange = { viewModel.setSeenPoints(playerState.player.id, it) },
                            onOpenMaalCalculator = { maalDialogPlayerId = playerState.player.id }
                        )
                    }
                }

                // Dublee winner notice: the +5 is applied automatically, so say so
                if (uiState.playerStates.any { it.isWinner && it.duply } && uiState.settings.dublee) {
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(
                        text = "Dublee win: +${RoundInputViewModel.DUBLEE_WINNER_MAAL_BONUS} Maal added to the total Maal.",
                        color = AppTheme.palette.accent,
                        fontSize = 12.sp,
                        fontWeight = FontWeight.SemiBold
                    )
                }

                Spacer(modifier = Modifier.height(16.dp))

                // Error
                uiState.error?.let {
                    Text(it, color = Color.Red, fontSize = 14.sp, modifier = Modifier.padding(bottom = 12.dp))
                }

                // Buttons - shared GlassButton, same primary/secondary styling as the Dashboard
                GlassButton(
                    onClick = { viewModel.submitRound() },
                    text = "Save Round Results",
                    containerColor = AppTheme.palette.cta.copy(alpha = 0.35f),
                    textColor = AppTheme.palette.accent,
                    height = 52,
                    leadingIcon = {
                        Icon(Icons.Default.Calculate, null, tint = AppTheme.palette.accent, modifier = Modifier.size(18.dp))
                    }
                )

                Spacer(modifier = Modifier.height(10.dp))

                GlassButton(
                    onClick = { onBack() },
                    text = "Discard & Return",
                    containerColor = AppTheme.palette.tint.copy(alpha = 0.12f),
                    textColor = AppTheme.palette.textPrimary,
                    height = 48
                )

                Spacer(modifier = Modifier.height(24.dp))
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
private fun ScoreGridHeader() {
    Row(
        verticalAlignment = Alignment.CenterVertically,
        modifier = Modifier
            .fillMaxWidth()
            .background(AppTheme.palette.tint.copy(alpha = 0.06f))
            .padding(horizontal = 10.dp, vertical = 8.dp)
    ) {
        Text(
            text = "PLAYER",
            color = AppTheme.palette.frostAccent,
            fontSize = 10.sp,
            fontWeight = FontWeight.Bold,
            letterSpacing = 1.sp,
            modifier = Modifier.weight(1f)
        )
        HeaderCell(Icons.Default.EmojiEvents, "Winner", WinnerColWidth)
        HeaderText("SEEN", CheckColWidth)
        HeaderText("DUB", CheckColWidth)
        HeaderText("MAAL", MaalColWidth)
    }
}

@Composable
private fun HeaderCell(icon: androidx.compose.ui.graphics.vector.ImageVector, description: String, width: androidx.compose.ui.unit.Dp) {
    Box(modifier = Modifier.width(width), contentAlignment = Alignment.Center) {
        Icon(icon, description, tint = AppTheme.palette.frostAccent, modifier = Modifier.size(14.dp))
    }
}

@Composable
private fun HeaderText(text: String, width: androidx.compose.ui.unit.Dp) {
    Text(
        text = text,
        color = AppTheme.palette.frostAccent,
        fontSize = 10.sp,
        fontWeight = FontWeight.Bold,
        letterSpacing = 1.sp,
        textAlign = TextAlign.Center,
        modifier = Modifier.width(width)
    )
}

/// One compact grid row per player: name (+D badge, preview line) | 🏆 | seen | dublee | maal.
@Composable
private fun PlayerScoreRow(
    state: PlayerRoundState,
    striped: Boolean,
    showPreview: Boolean,
    currency: Currency,
    onSelectWinner: () -> Unit,
    onToggleSeen: () -> Unit,
    onToggleDuply: () -> Unit,
    onMaalPointsChange: (Int) -> Unit,
    onOpenMaalCalculator: () -> Unit
) {
    val isWinner = state.isWinner
    val rowBackground = when {
        isWinner -> AppTheme.palette.accent.copy(alpha = 0.08f)
        striped -> AppTheme.palette.tint.copy(alpha = 0.03f)
        else -> Color.Transparent
    }

    Row(
        verticalAlignment = Alignment.CenterVertically,
        modifier = Modifier
            .fillMaxWidth()
            .background(rowBackground)
            .padding(horizontal = 10.dp, vertical = 6.dp)
    ) {
        // Player: small avatar + name + dealer badge (+ preview line underneath)
        Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier.weight(1f)
        ) {
            PlayerAvatar(state)
            Spacer(modifier = Modifier.width(8.dp))
            Column {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Text(
                        text = state.player.name,
                        color = AppTheme.palette.textPrimary,
                        fontSize = 13.sp,
                        fontWeight = if (isWinner) FontWeight.Bold else FontWeight.Medium,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis,
                        modifier = Modifier.weight(1f, fill = false)
                    )
                    if (state.isDealer) {
                        Spacer(modifier = Modifier.width(4.dp))
                        DealerBadge(size = 14.dp)
                    }
                }
                // Live preview of this game's points/money, once a winner is picked
                if (showPreview) {
                    val scoreColor = when {
                        state.previewScore > 0 -> Color(0xFF4CAF50)
                        state.previewScore < 0 -> Color(0xFFFF5252)
                        else -> AppTheme.palette.tint.copy(alpha = 0.6f)
                    }
                    Text(
                        text = "${state.previewScore} · ${currency.formatMoney(state.previewMoney)}",
                        color = scoreColor,
                        fontSize = 11.sp,
                        fontWeight = FontWeight.Bold,
                        maxLines = 1
                    )
                }
            }
        }

        // Winner trophy toggle
        Box(modifier = Modifier.width(WinnerColWidth), contentAlignment = Alignment.Center) {
            Icon(
                imageVector = Icons.Default.EmojiEvents,
                contentDescription = "Select winner",
                tint = if (isWinner) AppTheme.palette.accent else AppTheme.palette.tint.copy(alpha = 0.15f),
                modifier = Modifier
                    .size(30.dp)
                    .clip(RoundedCornerShape(6.dp))
                    .clickable { onSelectWinner() }
                    .padding(4.dp)
            )
        }

        // Seen checkbox (winner is always seen — locked)
        GridCheckbox(
            checked = state.seen,
            enabled = !isWinner,
            activeColor = AppTheme.palette.accent,
            description = "Seen joker",
            width = CheckColWidth,
            onToggle = onToggleSeen
        )

        // Dublee checkbox
        GridCheckbox(
            checked = state.duply,
            enabled = true,
            activeColor = AppTheme.palette.cta,
            description = "Dublee",
            width = CheckColWidth,
            onToggle = onToggleDuply
        )

        // Maal input + calculator (active only when seen)
        Row(
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.Center,
            modifier = Modifier.width(MaalColWidth)
        ) {
            if (state.seen) {
                BasicTextField(
                    value = if (state.seenPoints == 0) "" else state.seenPoints.toString(),
                    onValueChange = { onMaalPointsChange(it.toIntOrNull() ?: 0) },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true,
                    textStyle = TextStyle(
                        color = AppTheme.palette.textPrimary,
                        fontSize = 13.sp,
                        fontWeight = FontWeight.Bold,
                        textAlign = TextAlign.Center
                    ),
                    cursorBrush = SolidColor(AppTheme.palette.accent),
                    decorationBox = { inner ->
                        Box(
                            contentAlignment = Alignment.Center,
                            modifier = Modifier
                                .size(width = 44.dp, height = 32.dp)
                                .clip(RoundedCornerShape(6.dp))
                                .background(Color(0xFF2A2A2A))
                                .border(1.dp, AppTheme.palette.accent.copy(alpha = 0.35f), RoundedCornerShape(6.dp))
                        ) {
                            if (state.seenPoints == 0) {
                                Text("0", color = AppTheme.palette.tint.copy(alpha = 0.25f), fontSize = 13.sp)
                            }
                            inner()
                        }
                    }
                )
                Spacer(modifier = Modifier.width(4.dp))
                Icon(
                    imageVector = Icons.Default.Calculate,
                    contentDescription = "Open Maal calculator",
                    tint = AppTheme.palette.accent,
                    modifier = Modifier
                        .size(28.dp)
                        .clip(RoundedCornerShape(6.dp))
                        .background(AppTheme.palette.accent.copy(alpha = 0.12f))
                        .clickable { onOpenMaalCalculator() }
                        .padding(5.dp)
                )
            } else {
                Text("—", color = AppTheme.palette.tint.copy(alpha = 0.2f), fontSize = 13.sp)
            }
        }
    }
}

@Composable
private fun GridCheckbox(
    checked: Boolean,
    enabled: Boolean,
    activeColor: Color,
    description: String,
    width: androidx.compose.ui.unit.Dp,
    onToggle: () -> Unit
) {
    Box(modifier = Modifier.width(width), contentAlignment = Alignment.Center) {
        Icon(
            imageVector = if (checked) Icons.Default.CheckBox else Icons.Default.CheckBoxOutlineBlank,
            contentDescription = description,
            tint = when {
                checked -> activeColor
                enabled -> AppTheme.palette.tint.copy(alpha = 0.25f)
                else -> AppTheme.palette.tint.copy(alpha = 0.1f)
            },
            modifier = Modifier
                .size(30.dp)
                .clip(RoundedCornerShape(6.dp))
                .clickable(enabled = enabled) { onToggle() }
                .padding(5.dp)
        )
    }
}

@Composable
private fun PlayerAvatar(state: PlayerRoundState) {
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
            modifier = Modifier.size(26.dp).clip(RoundedCornerShape(6.dp))
        )
    } else {
        Box(
            modifier = Modifier
                .size(26.dp)
                .clip(RoundedCornerShape(6.dp))
                .background(AppTheme.palette.tint.copy(alpha = 0.25f)),
            contentAlignment = Alignment.Center
        ) {
            Text(
                state.player.name.take(1).uppercase(),
                color = AppTheme.palette.textPrimary,
                fontWeight = FontWeight.Bold,
                fontSize = 12.sp
            )
        }
    }
}
