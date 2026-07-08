package np.com.sanjeeb.marriagecalculator.ui.gamesetup

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.horizontalScroll
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
import androidx.compose.ui.graphics.PathEffect
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.hilt.navigation.compose.hiltViewModel
import coil.compose.AsyncImage
import coil.request.ImageRequest
import java.io.File
import java.io.FileOutputStream
import java.io.InputStream
import android.net.Uri
import np.com.sanjeeb.marriagecalculator.data.model.Currency
import np.com.sanjeeb.marriagecalculator.data.model.GameSettings
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.ui.theme.DeepRedTika
import np.com.sanjeeb.marriagecalculator.ui.theme.GoldAccent
import np.com.sanjeeb.marriagecalculator.ui.theme.MarigoldOrange
import np.com.sanjeeb.marriagecalculator.ui.theme.TiharNightBlue
import np.com.sanjeeb.marriagecalculator.ui.components.MetallicButton
import np.com.sanjeeb.marriagecalculator.ui.components.MetallicRedFace
import np.com.sanjeeb.marriagecalculator.ui.components.MetallicRedRim

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun GameSetupScreen(
    onGameCreated: (String) -> Unit,
    onBack: () -> Unit,
    viewModel: GameSetupViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    var showRearrangeSetupDialog by remember { mutableStateOf(false) }

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
                    onAddNewPlayer = viewModel::addNewPlayer,
                    currentUser = uiState.currentUser
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
                MetallicButton(
                    onClick = { showRearrangeSetupDialog = true },
                    text = "Start Game (${uiState.selectedPlayerIds.size} players)",
                    rimColors = MetallicRedRim,
                    faceColors = MetallicRedFace,
                    textColor = GoldAccent,
                    modifier = Modifier.height(56.dp),
                    enabled = uiState.selectedPlayerIds.size in 2..6,
                    isLoading = uiState.isLoading,
                    leadingIcon = {
                        Icon(Icons.Default.PlayArrow, null, tint = GoldAccent)
                    }
                )

                Spacer(modifier = Modifier.height(16.dp))
            }
        }
    }

    if (showRearrangeSetupDialog) {
        val selectedPlayers = viewModel.getSelectedPlayers()
        RearrangeSeatsDialog(
            initialPlayers = selectedPlayers,
            onSave = { reorderedList ->
                viewModel.createGame(reorderedList.map { it.id })
            },
            onDismiss = { showRearrangeSetupDialog = false }
        )
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun PlayerSelectionSection(
    allPlayers: List<Player>,
    selectedIds: Set<String>,
    onTogglePlayer: (String) -> Unit,
    onAddNewPlayer: (String, String?) -> Unit,
    currentUser: np.com.sanjeeb.marriagecalculator.data.model.User?
) {
    var showPlayerSheet by remember { mutableStateOf(false) }
    var showCreateSheet by remember { mutableStateOf(false) }

    val selectedPlayers = allPlayers.filter { it.id in selectedIds }

    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.SpaceBetween,
        verticalAlignment = Alignment.Bottom
    ) {
        Text(
            "Players",
            color = Color.White,
            fontSize = 20.sp,
            fontWeight = FontWeight.Bold,
            fontFamily = FontFamily.Serif
        )
        Text(
            "${selectedIds.size} / 6 JOINED",
            color = GoldAccent,
            fontWeight = FontWeight.Bold,
            fontSize = 10.sp
        )
    }
    
    Spacer(modifier = Modifier.height(12.dp))

    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        for (i in 0 until 6) {
            val emptyModifier = Modifier
                .weight(1f)
                .aspectRatio(1f)
                .padding(4.dp)
                .clip(RoundedCornerShape(8.dp))

            if (i < selectedPlayers.size) {
                PlayerSlot(player = selectedPlayers[i], modifier = emptyModifier) {
                    onTogglePlayer(selectedPlayers[i].id)
                }
            } else if (i == selectedPlayers.size) {
                Box(
                    modifier = emptyModifier
                        .clickable { showPlayerSheet = true },
                    contentAlignment = Alignment.Center
                ) {
                    androidx.compose.foundation.Canvas(modifier = Modifier.matchParentSize()) {
                        drawRoundRect(
                            color = Color.White.copy(alpha = 0.3f),
                            style = Stroke(width = 4f, pathEffect = PathEffect.dashPathEffect(floatArrayOf(10f, 10f), 0f)),
                            cornerRadius = androidx.compose.ui.geometry.CornerRadius(8.dp.toPx())
                        )
                    }
                    Icon(Icons.Default.Add, contentDescription = "Add Player", tint = Color.White)
                }
            } else {
                Box(modifier = emptyModifier.background(Color.White.copy(alpha = 0.05f), RoundedCornerShape(8.dp)))
            }
        }
    }

    if (showPlayerSheet) {
        ModalBottomSheet(onDismissRequest = { showPlayerSheet = false }, containerColor = TiharNightBlue) {
            PlayerSelectionSheetContent(
                allPlayers = allPlayers,
                selectedIds = selectedIds,
                onTogglePlayer = onTogglePlayer,
                onCreateNewClicked = {
                    showPlayerSheet = false
                    showCreateSheet = true
                },
                currentUser = currentUser
            )
        }
    }

    if (showCreateSheet) {
        ModalBottomSheet(onDismissRequest = { showCreateSheet = false }, containerColor = TiharNightBlue) {
            CreatePlayerSheetContent(
                onPlayerCreated = { name, uri ->
                    onAddNewPlayer(name, uri)
                    showCreateSheet = false
                }
            )
        }
    }
}

@Composable
fun PlayerSlot(player: Player, modifier: Modifier, onClick: () -> Unit) {
    Box(
        modifier = modifier
            .background(Color.White.copy(alpha = 0.1f), RoundedCornerShape(8.dp))
            .clickable { onClick() }
    ) {
        if (!player.photoUri.isNullOrEmpty()) {
            val model = if (player.photoUri.startsWith("android.resource") || player.photoUri.startsWith("http")) {
                player.photoUri
            } else {
                File(player.photoUri)
            }
            AsyncImage(
                model = ImageRequest.Builder(LocalContext.current)
                    .data(model)
                    .crossfade(true)
                    .build(),
                contentDescription = "Player photo",
                contentScale = ContentScale.Crop,
                modifier = Modifier.fillMaxSize().clip(RoundedCornerShape(8.dp))
            )
        } else {
             Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                 Text(player.name.take(1).uppercase(), color = Color.White, fontSize = 18.sp, fontWeight = FontWeight.Bold)
             }
        }
        
        Box(
            modifier = Modifier
                .align(Alignment.BottomCenter)
                .fillMaxWidth()
                .background(Brush.verticalGradient(listOf(Color.Transparent, Color.Black.copy(alpha = 0.8f))))
                .padding(vertical = 4.dp),
            contentAlignment = Alignment.Center
        ) {
            Text(player.name, color = Color.White, fontSize = 9.sp, maxLines = 1)
        }
    }
}

@Composable
fun PlayerSelectionSheetContent(
    allPlayers: List<Player>,
    selectedIds: Set<String>,
    onTogglePlayer: (String) -> Unit,
    onCreateNewClicked: () -> Unit,
    currentUser: np.com.sanjeeb.marriagecalculator.data.model.User?
) {
    Column(modifier = Modifier.padding(16.dp).fillMaxWidth().heightIn(min = 300.dp)) {
        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
            Text("Select Player", fontSize = 20.sp, fontWeight = FontWeight.Bold, color = GoldAccent, fontFamily = FontFamily.Serif)
            TextButton(onClick = onCreateNewClicked) {
                Icon(Icons.Default.Add, null, tint = GoldAccent, modifier = Modifier.size(16.dp))
                Spacer(modifier = Modifier.width(4.dp))
                Text("New Player", color = GoldAccent)
            }
        }
        Spacer(modifier = Modifier.height(16.dp))

        if (currentUser != null && currentUser.email.isNotEmpty()) {
            val isMeSelected = selectedIds.any { id ->
                val p = allPlayers.find { it.id == id }
                p?.email?.equals(currentUser.email, ignoreCase = true) == true
            }
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .clickable {
                        val mePlayer = allPlayers.find { it.email.equals(currentUser.email, ignoreCase = true) }
                        mePlayer?.let { onTogglePlayer(it.id) }
                    }
                    .padding(vertical = 8.dp),
                border = BorderStroke(1.dp, if (isMeSelected) GoldAccent else Color.White.copy(alpha = 0.2f)),
                colors = CardDefaults.cardColors(
                    containerColor = if (isMeSelected) DeepRedTika.copy(alpha = 0.2f) else Color.White.copy(alpha = 0.05f)
                )
            ) {
                Row(
                    modifier = Modifier.padding(12.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Box(
                        modifier = Modifier
                            .size(40.dp)
                            .clip(CircleShape)
                            .background(Color.White.copy(alpha = 0.1f)),
                        contentAlignment = Alignment.Center
                    ) {
                        if (!currentUser.photoUrl.isNullOrEmpty()) {
                            AsyncImage(
                                model = currentUser.photoUrl,
                                contentDescription = null,
                                contentScale = ContentScale.Crop,
                                modifier = Modifier.fillMaxSize()
                            )
                        } else {
                            Text(
                                text = currentUser.displayName.take(1).uppercase(),
                                color = Color.White,
                                fontWeight = FontWeight.Bold,
                                fontSize = 18.sp
                            )
                        }
                    }
                    Spacer(modifier = Modifier.width(12.dp))
                    Column(modifier = Modifier.weight(1f)) {
                        Text(currentUser.displayName, color = Color.White, fontWeight = FontWeight.Bold, fontSize = 14.sp)
                        Text("You (${currentUser.email})", color = Color.White.copy(alpha = 0.5f), fontSize = 12.sp)
                    }
                    if (isMeSelected) {
                        Icon(Icons.Default.CheckCircle, contentDescription = "Selected", tint = GoldAccent)
                    } else {
                        Text("Add Me", color = GoldAccent, fontWeight = FontWeight.Bold, fontSize = 12.sp)
                    }
                }
            }
            Spacer(modifier = Modifier.height(8.dp))
        }
        
        val gridPlayers = remember(allPlayers, currentUser) {
            if (currentUser != null && currentUser.email.isNotEmpty()) {
                allPlayers.filter { !it.email.equals(currentUser.email, ignoreCase = true) }
            } else {
                allPlayers
            }
        }

        LazyVerticalGrid(
            columns = GridCells.Fixed(3),
            modifier = Modifier.weight(1f, fill = false),
            horizontalArrangement = Arrangement.spacedBy(8.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            items(gridPlayers) { player ->
                val isSelected = selectedIds.contains(player.id)
                val bgColor = if (isSelected) DeepRedTika else Color.White.copy(alpha = 0.05f)
                val borderColor = if (isSelected) GoldAccent else Color.White.copy(alpha = 0.2f)

                Column(
                    modifier = Modifier
                        .clip(RoundedCornerShape(12.dp))
                        .background(bgColor)
                        .border(1.dp, borderColor, RoundedCornerShape(12.dp))
                        .clickable { onTogglePlayer(player.id) }
                        .padding(12.dp),
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    Box(
                        modifier = Modifier
                            .size(40.dp)
                            .clip(CircleShape)
                            .background(if (isSelected) GoldAccent else Color.White.copy(alpha = 0.1f)),
                        contentAlignment = Alignment.Center
                    ) {
                        if (!player.photoUri.isNullOrEmpty()) {
                            AsyncImage(
                                model = if (player.photoUri.startsWith("android") || player.photoUri.startsWith("http")) player.photoUri else File(player.photoUri),
                                contentDescription = null,
                                contentScale = ContentScale.Crop,
                                modifier = Modifier.fillMaxSize()
                            )
                        } else {
                            Text(
                                text = player.name.take(1).uppercase(),
                                color = if (isSelected) DeepRedTika else Color.White,
                                fontWeight = FontWeight.Bold,
                                fontSize = 18.sp
                            )
                        }
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
        }
        Spacer(modifier = Modifier.height(24.dp))
    }
}

@Composable
fun CreatePlayerSheetContent(onPlayerCreated: (String, String?) -> Unit) {
    var name by remember { mutableStateOf("") }
    var selectedPhotoUri by remember { mutableStateOf<String?>(null) }
    val context = LocalContext.current

    val galleryLauncher = rememberLauncherForActivityResult(ActivityResultContracts.GetContent()) { uri: Uri? ->
        uri?.let {
            val savedUri = copyUriToInternalStorage(context, it)
            selectedPhotoUri = savedUri
        }
    }

    Column(modifier = Modifier.padding(16.dp).fillMaxWidth().padding(bottom = 32.dp)) {
        Text("Create New Player", fontSize = 20.sp, fontWeight = FontWeight.Bold, color = GoldAccent, fontFamily = FontFamily.Serif)
        Spacer(modifier = Modifier.height(16.dp))

        OutlinedTextField(
            value = name,
            onValueChange = { name = it },
            label = { Text("Player Name", color = GoldAccent.copy(alpha = 0.7f)) },
            modifier = Modifier.fillMaxWidth(),
            colors = OutlinedTextFieldDefaults.colors(
                focusedTextColor = Color.White,
                unfocusedTextColor = Color.White,
                focusedBorderColor = GoldAccent,
                unfocusedBorderColor = GoldAccent.copy(alpha = 0.3f)
            ),
            singleLine = true
        )
        
        Spacer(modifier = Modifier.height(24.dp))
        Text("Select Photo", color = Color.White.copy(alpha = 0.7f), fontSize = 14.sp)
        Spacer(modifier = Modifier.height(8.dp))
        
        Row(horizontalArrangement = Arrangement.spacedBy(16.dp), modifier = Modifier.horizontalScroll(rememberScrollState())) {
            listOf("avatar_1", "avatar_2", "avatar_3").forEach { avatarName ->
                val isSelected = selectedPhotoUri?.contains(avatarName) == true
                Box(modifier = Modifier.size(64.dp).clip(RoundedCornerShape(8.dp)).clickable { 
                    selectedPhotoUri = "android.resource://${context.packageName}/drawable/$avatarName" 
                }.border(if (isSelected) 2.dp else 0.dp, GoldAccent, RoundedCornerShape(8.dp))) {
                    AsyncImage(model = "android.resource://${context.packageName}/drawable/$avatarName", contentDescription = null, modifier = Modifier.fillMaxSize())
                }
            }

            Box(modifier = Modifier.size(64.dp).clip(RoundedCornerShape(8.dp)).background(Color.White.copy(alpha = 0.1f)).clickable { 
                galleryLauncher.launch("image/*") 
            }, contentAlignment = Alignment.Center) {
                Icon(Icons.Default.PhotoLibrary, "Gallery", tint = GoldAccent)
            }
        }
        
        Spacer(modifier = Modifier.height(32.dp))
        
        Button(
            onClick = { onPlayerCreated(name, selectedPhotoUri) },
            modifier = Modifier.fillMaxWidth().height(50.dp),
            enabled = name.isNotBlank(),
            colors = ButtonDefaults.buttonColors(containerColor = DeepRedTika, contentColor = GoldAccent)
        ) {
            Text("Save & Add Player")
        }
    }
}

fun copyUriToInternalStorage(context: android.content.Context, uri: Uri): String {
    val inputStream = context.contentResolver.openInputStream(uri) ?: return ""
    val file = File(context.filesDir, "player_${System.currentTimeMillis()}.jpg")
    val outputStream = FileOutputStream(file)
    inputStream.copyTo(outputStream)
    inputStream.close()
    outputStream.close()
    return file.absolutePath
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
