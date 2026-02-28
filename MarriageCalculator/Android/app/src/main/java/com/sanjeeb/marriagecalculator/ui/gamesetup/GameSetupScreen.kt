package com.sanjeeb.marriagecalculator.ui.gamesetup

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
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import com.sanjeeb.marriagecalculator.data.model.Currency
import com.sanjeeb.marriagecalculator.data.model.GameSettings
import com.sanjeeb.marriagecalculator.data.model.Player
import com.sanjeeb.marriagecalculator.ui.theme.DeepRedTika
import com.sanjeeb.marriagecalculator.ui.theme.GoldAccent
import com.sanjeeb.marriagecalculator.ui.theme.MarigoldOrange
import com.sanjeeb.marriagecalculator.ui.theme.TiharNightBlue

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun GameSetupScreen(
    onGameCreated: (Int) -> Unit,
    onBack: () -> Unit,
    viewModel: GameSetupViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    // Navigate when game is created
    LaunchedEffect(uiState.createdGameSetId) {
        uiState.createdGameSetId?.let { onGameCreated(it) }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Text(
                        "New Game",
                        color = GoldAccent,
                        fontFamily = FontFamily.Serif,
                        fontWeight = FontWeight.Bold
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
                    .padding(16.dp)
            ) {
                // Game Name
                OutlinedTextField(
                    value = uiState.gameName,
                    onValueChange = { viewModel.setGameName(it) },
                    label = { Text("Game Name (optional)", color = GoldAccent.copy(alpha = 0.7f)) },
                    modifier = Modifier.fillMaxWidth(),
                    colors = OutlinedTextFieldDefaults.colors(
                        focusedTextColor = Color.White,
                        unfocusedTextColor = Color.White,
                        focusedBorderColor = GoldAccent,
                        unfocusedBorderColor = GoldAccent.copy(alpha = 0.3f)
                    ),
                    singleLine = true
                )

                Spacer(modifier = Modifier.height(20.dp))

                // Players Section
                PlayerSelectionSection(
                    allPlayers = viewModel.getAllPlayers(),
                    selectedIds = uiState.selectedPlayerIds,
                    onTogglePlayer = viewModel::togglePlayerSelection,
                    showAddPlayer = uiState.showAddPlayer,
                    newPlayerName = uiState.newPlayerName,
                    onNewPlayerNameChange = viewModel::setNewPlayerName,
                    onToggleAddPlayer = viewModel::toggleShowAddPlayer,
                    onAddPlayer = viewModel::addLocalPlayer
                )

                Spacer(modifier = Modifier.height(20.dp))

                // Settings Section
                SettingsSection(
                    settings = uiState.settings,
                    onSettingsChange = viewModel::updateSettings
                )

                Spacer(modifier = Modifier.height(24.dp))

                // Error
                uiState.error?.let {
                    Text(
                        text = it,
                        color = Color.Red,
                        fontSize = 14.sp,
                        modifier = Modifier.padding(bottom = 8.dp)
                    )
                }

                // Start Game Button
                Button(
                    onClick = { viewModel.createGame() },
                    modifier = Modifier
                        .fillMaxWidth()
                        .height(56.dp),
                    shape = RoundedCornerShape(16.dp),
                    colors = ButtonDefaults.buttonColors(containerColor = DeepRedTika),
                    enabled = uiState.selectedPlayerIds.size in 2..6 && !uiState.isLoading
                ) {
                    if (uiState.isLoading) {
                        CircularProgressIndicator(color = GoldAccent, modifier = Modifier.size(24.dp))
                    } else {
                        Icon(Icons.Default.PlayArrow, null, tint = GoldAccent)
                        Spacer(modifier = Modifier.width(8.dp))
                        Text(
                            "Start Game (${uiState.selectedPlayerIds.size} players)",
                            color = GoldAccent,
                            fontSize = 18.sp,
                            fontWeight = FontWeight.Bold,
                            fontFamily = FontFamily.Serif
                        )
                    }
                }

                Spacer(modifier = Modifier.height(16.dp))
            }
        }
    }
}

@Composable
private fun PlayerSelectionSection(
    allPlayers: List<Player>,
    selectedIds: Set<Int>,
    onTogglePlayer: (Int) -> Unit,
    showAddPlayer: Boolean,
    newPlayerName: String,
    onNewPlayerNameChange: (String) -> Unit,
    onToggleAddPlayer: () -> Unit,
    onAddPlayer: () -> Unit
) {
    Text(
        "Players (${selectedIds.size}/6)",
        color = GoldAccent,
        fontSize = 18.sp,
        fontWeight = FontWeight.Bold,
        fontFamily = FontFamily.Serif
    )
    Spacer(modifier = Modifier.height(8.dp))

    // Player grid - compact 3-column for up to 6 players
    LazyVerticalGrid(
        columns = GridCells.Fixed(3),
        modifier = Modifier.heightIn(max = 300.dp),
        horizontalArrangement = Arrangement.spacedBy(8.dp),
        verticalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        items(allPlayers) { player ->
            PlayerChip(
                player = player,
                isSelected = selectedIds.contains(player.id),
                onClick = { onTogglePlayer(player.id) }
            )
        }
    }

    Spacer(modifier = Modifier.height(8.dp))

    // Add player
    if (showAddPlayer) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically
        ) {
            OutlinedTextField(
                value = newPlayerName,
                onValueChange = onNewPlayerNameChange,
                label = { Text("Player Name", color = GoldAccent.copy(alpha = 0.7f)) },
                modifier = Modifier.weight(1f),
                colors = OutlinedTextFieldDefaults.colors(
                    focusedTextColor = Color.White,
                    unfocusedTextColor = Color.White,
                    focusedBorderColor = GoldAccent,
                    unfocusedBorderColor = GoldAccent.copy(alpha = 0.3f)
                ),
                singleLine = true
            )
            Spacer(modifier = Modifier.width(8.dp))
            IconButton(onClick = onAddPlayer) {
                Icon(Icons.Default.Check, null, tint = GoldAccent)
            }
        }
    } else {
        TextButton(onClick = onToggleAddPlayer) {
            Icon(Icons.Default.PersonAdd, null, tint = GoldAccent, modifier = Modifier.size(18.dp))
            Spacer(modifier = Modifier.width(4.dp))
            Text("Add Player", color = GoldAccent)
        }
    }
}

@Composable
private fun PlayerChip(player: Player, isSelected: Boolean, onClick: () -> Unit) {
    val bgColor = if (isSelected) DeepRedTika else Color.White.copy(alpha = 0.05f)
    val borderColor = if (isSelected) GoldAccent else Color.White.copy(alpha = 0.2f)

    Column(
        modifier = Modifier
            .clip(RoundedCornerShape(12.dp))
            .background(bgColor)
            .border(1.dp, borderColor, RoundedCornerShape(12.dp))
            .clickable { onClick() }
            .padding(12.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        // Avatar
        Box(
            modifier = Modifier
                .size(40.dp)
                .clip(CircleShape)
                .background(
                    if (isSelected) GoldAccent else Color.White.copy(alpha = 0.1f)
                ),
            contentAlignment = Alignment.Center
        ) {
            Text(
                text = player.name.take(1).uppercase(),
                color = if (isSelected) DeepRedTika else Color.White,
                fontWeight = FontWeight.Bold,
                fontSize = 18.sp
            )
        }
        Spacer(modifier = Modifier.height(4.dp))
        Text(
            text = player.name,
            color = if (isSelected) GoldAccent else Color.White.copy(alpha = 0.7f),
            fontSize = 12.sp,
            textAlign = TextAlign.Center,
            maxLines = 1
        )
    }
}

@Composable
private fun SettingsSection(settings: GameSettings, onSettingsChange: (GameSettings) -> Unit) {
    var expanded by remember { mutableStateOf(false) }

    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.05f))
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .clickable { expanded = !expanded },
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Text(
                    "Game Settings",
                    color = GoldAccent,
                    fontSize = 18.sp,
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Serif
                )
                Icon(
                    if (expanded) Icons.Default.ExpandLess else Icons.Default.ExpandMore,
                    null,
                    tint = GoldAccent
                )
            }

            if (expanded) {
                Spacer(modifier = Modifier.height(16.dp))

                // Game Mode
                Text("Game Mode", color = Color.White.copy(alpha = 0.7f), fontSize = 14.sp)
                Spacer(modifier = Modifier.height(4.dp))
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    GameModeChip("Murder", settings.murder) {
                        onSettingsChange(settings.copy(murder = true, kidnap = false))
                    }
                    GameModeChip("Kidnap", settings.kidnap) {
                        onSettingsChange(settings.copy(murder = false, kidnap = true))
                    }
                    GameModeChip("Normal", !settings.murder && !settings.kidnap) {
                        onSettingsChange(settings.copy(murder = false, kidnap = false))
                    }
                }

                Spacer(modifier = Modifier.height(12.dp))

                // Point Settings Row
                Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    SettingField("Seen Pts", settings.seenPoint.toString(), Modifier.weight(1f)) { value ->
                        value.toIntOrNull()?.let { onSettingsChange(settings.copy(seenPoint = it)) }
                    }
                    SettingField("Unseen Pts", settings.unseenPoint.toString(), Modifier.weight(1f)) { value ->
                        value.toIntOrNull()?.let { onSettingsChange(settings.copy(unseenPoint = it)) }
                    }
                    SettingField("Point Rate", settings.pointRate.toString(), Modifier.weight(1f)) { value ->
                        value.toDoubleOrNull()?.let { onSettingsChange(settings.copy(pointRate = it)) }
                    }
                }

                Spacer(modifier = Modifier.height(12.dp))

                // Currency
                Text("Currency", color = Color.White.copy(alpha = 0.7f), fontSize = 14.sp)
                Spacer(modifier = Modifier.height(4.dp))
                Row(horizontalArrangement = Arrangement.spacedBy(4.dp)) {
                    Currency.entries.forEach { currency ->
                        FilterChip(
                            selected = settings.currency == currency,
                            onClick = { onSettingsChange(settings.copy(currency = currency)) },
                            label = { Text(currency.displayName(), fontSize = 11.sp) },
                            colors = FilterChipDefaults.filterChipColors(
                                selectedContainerColor = DeepRedTika,
                                selectedLabelColor = GoldAccent
                            ),
                            modifier = Modifier.height(32.dp)
                        )
                    }
                }

                Spacer(modifier = Modifier.height(12.dp))

                // Dublee toggle
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    Text("Dublee", color = Color.White.copy(alpha = 0.7f))
                    Switch(
                        checked = settings.dublee,
                        onCheckedChange = { onSettingsChange(settings.copy(dublee = it)) },
                        colors = SwitchDefaults.colors(checkedTrackColor = DeepRedTika, checkedThumbColor = GoldAccent)
                    )
                }
            }
        }
    }
}

@Composable
private fun GameModeChip(label: String, isSelected: Boolean, onClick: () -> Unit) {
    FilterChip(
        selected = isSelected,
        onClick = onClick,
        label = { Text(label, fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Normal) },
        colors = FilterChipDefaults.filterChipColors(
            selectedContainerColor = DeepRedTika,
            selectedLabelColor = GoldAccent,
            labelColor = Color.White.copy(alpha = 0.6f)
        )
    )
}

@Composable
private fun SettingField(label: String, value: String, modifier: Modifier = Modifier, onValueChange: (String) -> Unit) {
    OutlinedTextField(
        value = value,
        onValueChange = onValueChange,
        label = { Text(label, color = GoldAccent.copy(alpha = 0.7f), fontSize = 10.sp) },
        modifier = modifier,
        colors = OutlinedTextFieldDefaults.colors(
            focusedTextColor = Color.White,
            unfocusedTextColor = Color.White,
            focusedBorderColor = GoldAccent,
            unfocusedBorderColor = GoldAccent.copy(alpha = 0.3f)
        ),
        singleLine = true
    )
}
