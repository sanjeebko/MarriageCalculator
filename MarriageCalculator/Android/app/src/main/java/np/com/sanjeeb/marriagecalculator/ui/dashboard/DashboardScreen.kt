package np.com.sanjeeb.marriagecalculator.ui.dashboard

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.layout.ColumnScope
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.material.icons.filled.Menu
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalLifecycleOwner
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.LifecycleEventObserver
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import coil.compose.AsyncImage
import coil.request.ImageRequest
import kotlinx.coroutines.launch
import np.com.sanjeeb.marriagecalculator.R
import np.com.sanjeeb.marriagecalculator.data.model.MarriageGameSet
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.data.model.UserCareerStats
import np.com.sanjeeb.marriagecalculator.ui.components.AppBackground
import np.com.sanjeeb.marriagecalculator.ui.components.GlassButton
import np.com.sanjeeb.marriagecalculator.ui.components.ThemePickerDialog
import np.com.sanjeeb.marriagecalculator.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DashboardScreen(
    onNewGame: () -> Unit,
    onResumeGame: (String) -> Unit,
    onFriends: () -> Unit,
    onLogout: () -> Unit,
    viewModel: DashboardViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    val currentTheme by viewModel.theme.collectAsState()
    val drawerState = rememberDrawerState(initialValue = DrawerValue.Closed)
    var showThemeDialog by remember { mutableStateOf(false) }

    // The ViewModel is scoped to this destination's nav back-stack entry, so it survives
    // navigating away and back (e.g. resuming or deleting a game) without being recreated -
    // reload on every resume so the list doesn't go stale after mutations made elsewhere.
    val lifecycleOwner = LocalLifecycleOwner.current
    DisposableEffect(lifecycleOwner) {
        val observer = LifecycleEventObserver { _, event ->
            if (event == Lifecycle.Event.ON_RESUME) {
                viewModel.loadActiveGames()
            }
        }
        lifecycleOwner.lifecycle.addObserver(observer)
        onDispose { lifecycleOwner.lifecycle.removeObserver(observer) }
    }
    val scope = rememberCoroutineScope()

    if (showThemeDialog) {
        ThemePickerDialog(
            current = currentTheme,
            onSelect = { viewModel.setTheme(it) },
            onDismiss = { showThemeDialog = false }
        )
    }

    ModalNavigationDrawer(
        drawerState = drawerState,
        drawerContent = {
            ModalDrawerSheet(
                drawerContainerColor = Color.Transparent,
                drawerShape = RoundedCornerShape(topEnd = 24.dp, bottomEnd = 24.dp),
                modifier = Modifier.width(300.dp)
            ) {
                // Glass container for the drawer content
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .background(
                            brush = Brush.linearGradient(
                                colors = listOf(
                                    AppTheme.palette.surface.copy(alpha = 0.97f),
                                    AppTheme.palette.backgroundBottom.copy(alpha = 0.95f)
                                )
                            )
                        )
                        .border(
                            width = 1.dp,
                            brush = Brush.horizontalGradient(
                                colors = listOf(AppTheme.palette.tint.copy(alpha = 0.2f), Color.Transparent)
                            ),
                            shape = RoundedCornerShape(topEnd = 24.dp, bottomEnd = 24.dp)
                        )
                ) {
                    // Glossy sheen layer
                    Box(
                        modifier = Modifier
                            .fillMaxSize()
                            .background(
                                brush = Brush.verticalGradient(
                                    colors = listOf(
                                        AppTheme.palette.tint.copy(alpha = 0.05f),
                                        Color.Transparent,
                                        Color.Black.copy(alpha = 0.05f)
                                    )
                                )
                            )
                    )
                    
                    Column(modifier = Modifier.fillMaxSize()) {
                        Spacer(Modifier.height(56.dp))
                        
                        val drawerItemColors = NavigationDrawerItemDefaults.colors(
                            unselectedContainerColor = AppTheme.palette.tint.copy(alpha = 0.05f),
                            selectedContainerColor = AppTheme.palette.tint.copy(alpha = 0.15f),
                            unselectedIconColor = AppTheme.palette.accent.copy(alpha = 0.9f),
                            unselectedTextColor = AppTheme.palette.textPrimary
                        )

                        val drawerItemModifier = Modifier
                            .padding(horizontal = 12.dp, vertical = 2.dp)
                            .height(44.dp)

                        NavigationDrawerItem(
                            label = { Text("New Game", fontSize = 14.sp, fontWeight = FontWeight.Medium) },
                            selected = false,
                            onClick = { scope.launch { drawerState.close() }; onNewGame() },
                            icon = { Icon(Icons.Default.Add, null, modifier = Modifier.size(18.dp)) },
                            colors = drawerItemColors,
                            modifier = drawerItemModifier,
                            shape = RoundedCornerShape(8.dp)
                        )
                        NavigationDrawerItem(
                            label = { Text("Friends & Social", fontSize = 14.sp, fontWeight = FontWeight.Medium) },
                            selected = false,
                            onClick = { scope.launch { drawerState.close() }; onFriends() },
                            icon = { Icon(Icons.Default.People, null, modifier = Modifier.size(18.dp)) },
                            colors = drawerItemColors,
                            modifier = drawerItemModifier,
                            shape = RoundedCornerShape(8.dp)
                        )
                        NavigationDrawerItem(
                            label = { Text("History", fontSize = 14.sp, fontWeight = FontWeight.Medium) },
                            selected = false,
                            onClick = { scope.launch { drawerState.close() } },
                            icon = { Icon(Icons.Default.History, null, modifier = Modifier.size(18.dp)) },
                            colors = drawerItemColors,
                            modifier = drawerItemModifier,
                            shape = RoundedCornerShape(8.dp)
                        )
                        NavigationDrawerItem(
                            label = { Text("App Theme", fontSize = 14.sp, fontWeight = FontWeight.Medium) },
                            selected = false,
                            onClick = { scope.launch { drawerState.close() }; showThemeDialog = true },
                            icon = { Icon(Icons.Default.Palette, null, modifier = Modifier.size(18.dp)) },
                            colors = drawerItemColors,
                            modifier = drawerItemModifier,
                            shape = RoundedCornerShape(8.dp)
                        )
                        NavigationDrawerItem(
                            label = { Text("About", fontSize = 14.sp, fontWeight = FontWeight.Medium) },
                            selected = false,
                            onClick = { scope.launch { drawerState.close() } },
                            icon = { Icon(Icons.Default.Info, null, modifier = Modifier.size(18.dp)) },
                            colors = drawerItemColors,
                            modifier = drawerItemModifier,
                            shape = RoundedCornerShape(8.dp)
                        )
                        NavigationDrawerItem(
                            label = { Text("More Apps", fontSize = 14.sp, fontWeight = FontWeight.Medium) },
                            selected = false,
                            onClick = { scope.launch { drawerState.close() } },
                            icon = { Icon(Icons.Default.Apps, null, modifier = Modifier.size(18.dp)) },
                            colors = drawerItemColors,
                            modifier = drawerItemModifier,
                            shape = RoundedCornerShape(8.dp)
                        )
                        
                        Spacer(Modifier.weight(1f))
                        
                        HorizontalDivider(
                            modifier = Modifier.padding(horizontal = 24.dp),
                            color = AppTheme.palette.tint.copy(alpha = 0.15f)
                        )
                        
                        NavigationDrawerItem(
                            label = { Text("Sign Out", color = Color(0xFFFF5252), fontSize = 14.sp, fontWeight = FontWeight.Bold) },
                            selected = false,
                            onClick = { scope.launch { drawerState.close() }; onLogout() },
                            icon = { Icon(Icons.AutoMirrored.Filled.ExitToApp, null, tint = Color(0xFFFF5252), modifier = Modifier.size(20.dp)) },
                            colors = drawerItemColors,
                            modifier = Modifier.padding(horizontal = 12.dp, vertical = 8.dp).height(44.dp),
                            shape = RoundedCornerShape(8.dp)
                        )
                        Spacer(Modifier.height(12.dp))
                    }
                }
            }
        }
    ) {
        Scaffold(
            topBar = {
                CenterAlignedTopAppBar(
                    title = {
                        uiState.user?.let { user ->
                            Row(
                                verticalAlignment = Alignment.CenterVertically,
                                modifier = Modifier.fillMaxWidth()
                            ) {
                                // Profile Image and Badge Container
                                Box(contentAlignment = Alignment.BottomEnd) {
                                    if (!user.photoUrl.isNullOrEmpty()) {
                                        AsyncImage(
                                            model = ImageRequest.Builder(LocalContext.current)
                                                .data(user.photoUrl)
                                                .crossfade(true)
                                                .build(),
                                            contentDescription = "Profile Picture",
                                            modifier = Modifier
                                                .size(36.dp)
                                                .clip(RoundedCornerShape(18.dp)),
                                            contentScale = ContentScale.Crop
                                        )
                                    } else {
                                        Icon(
                                            imageVector = Icons.Default.AccountCircle,
                                            contentDescription = null,
                                            tint = AppTheme.palette.accent,
                                            modifier = Modifier.size(36.dp)
                                        )
                                    }
                                    
                                    // Google Badge
                                    Surface(
                                        modifier = Modifier.size(12.dp).offset(x = 2.dp, y = 2.dp),
                                        shape = RoundedCornerShape(6.dp),
                                        color = AppTheme.palette.textPrimary
                                    ) {
                                        Image(
                                            painter = painterResource(id = R.drawable.ic_google_logo),
                                            contentDescription = null,
                                            modifier = Modifier.padding(2.dp)
                                        )
                                    }
                                }
                                
                                Spacer(Modifier.width(12.dp))
                                
                                Text(
                                    text = "नमस्ते, ${user.displayName}!",
                                    color = AppTheme.palette.accent,
                                    fontSize = 18.sp,
                                    fontWeight = FontWeight.Bold,
                                    fontFamily = FontFamily.Serif,
                                    maxLines = 1,
                                    overflow = TextOverflow.Ellipsis
                                )
                            }
                        }
                    },
                    navigationIcon = {
                        IconButton(onClick = { scope.launch { drawerState.open() } }) {
                            Icon(Icons.Default.Menu, contentDescription = "Menu", tint = AppTheme.palette.accent)
                        }
                    },
                    colors = TopAppBarDefaults.centerAlignedTopAppBarColors(
                        containerColor = Color.Transparent,
                        titleContentColor = AppTheme.palette.accent
                    )
                )
            },
            containerColor = Color.Transparent
        ) { padding ->
            AppBackground {
                LazyColumn(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(padding)
                        .padding(horizontal = 16.dp),
                    verticalArrangement = Arrangement.spacedBy(14.dp),
                    contentPadding = PaddingValues(top = 10.dp, bottom = 28.dp)
                ) {
                    // 1. Hero Career Stats Banner
                    item {
                        HeroStatsBanner(stats = uiState.careerStats)
                    }

                    // 2. Quick-Start Table Launcher (if 2+ recent players)
                    if (uiState.recentPlayers.size >= 2) {
                        item {
                            QuickStartLauncherCard(
                                players = uiState.recentPlayers,
                                isStarting = uiState.isQuickStarting,
                                onQuickStart = {
                                    viewModel.quickStartGame { newGameId ->
                                        onResumeGame(newGameId)
                                    }
                                }
                            )
                        }
                    }

                    // 3. Primary Actions (New Game / Friends) & Offline Mode
                    item {
                        Column {
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.spacedBy(12.dp)
                            ) {
                                GlassButton(
                                    onClick = onNewGame,
                                    text = "New Game",
                                    containerColor = AppTheme.palette.cta.copy(alpha = 0.35f),
                                    textColor = AppTheme.palette.accent,
                                    height = 44,
                                    modifier = Modifier.weight(1f),
                                    leadingIcon = {
                                        Icon(Icons.Default.Add, null, tint = AppTheme.palette.accent, modifier = Modifier.size(18.dp))
                                    }
                                )

                                if (!uiState.isOfflineMode) {
                                    GlassButton(
                                        onClick = onFriends,
                                        text = "Friends",
                                        containerColor = AppTheme.palette.tint.copy(alpha = 0.12f),
                                        textColor = AppTheme.palette.textPrimary,
                                        height = 44,
                                        modifier = Modifier.weight(1f),
                                        leadingIcon = {
                                            Icon(Icons.Default.People, null, tint = AppTheme.palette.accent, modifier = Modifier.size(18.dp))
                                        }
                                    )
                                }
                            }

                            if (uiState.isOfflineMode) {
                                Spacer(modifier = Modifier.height(6.dp))
                                Row(
                                    modifier = Modifier.fillMaxWidth(),
                                    horizontalArrangement = Arrangement.Center,
                                    verticalAlignment = Alignment.CenterVertically
                                ) {
                                    Icon(Icons.Default.WifiOff, null, tint = AppTheme.palette.accentAlt, modifier = Modifier.size(12.dp))
                                    Spacer(modifier = Modifier.width(4.dp))
                                    Text("Offline Mode", color = AppTheme.palette.accentAlt, fontSize = 10.sp)
                                }
                            }
                        }
                    }

                    // 4. Active Games Section
                    if (uiState.enrichedGames.isNotEmpty() || uiState.isLoading) {
                        item {
                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Row(verticalAlignment = Alignment.CenterVertically) {
                                    Text(
                                        text = "Active Games",
                                        color = AppTheme.palette.accent,
                                        fontSize = 16.sp,
                                        fontWeight = FontWeight.Bold,
                                        fontFamily = FontFamily.Serif
                                    )
                                    if (uiState.enrichedGames.isNotEmpty()) {
                                        Spacer(Modifier.width(6.dp))
                                        Text(
                                            text = "(${uiState.enrichedGames.size})",
                                            color = AppTheme.palette.tint.copy(alpha = 0.6f),
                                            fontSize = 13.sp,
                                            fontWeight = FontWeight.SemiBold
                                        )
                                    }
                                }
                                if (uiState.isLoading) {
                                    CircularProgressIndicator(
                                        modifier = Modifier.size(16.dp),
                                        strokeWidth = 2.dp,
                                        color = AppTheme.palette.accent
                                    )
                                }
                            }
                        }

                        items(uiState.enrichedGames, key = { it.id }) { game ->
                            EnrichedActiveGameCard(
                                game = game,
                                onResume = { onResumeGame(game.id) }
                            )
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun HeroStatsBanner(stats: UserCareerStats) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(16.dp),
        colors = CardDefaults.cardColors(containerColor = AppTheme.palette.tint.copy(alpha = 0.08f)),
        border = BorderStroke(1.dp, AppTheme.palette.accent.copy(alpha = 0.25f))
    ) {
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .background(
                    Brush.verticalGradient(
                        colors = listOf(
                            AppTheme.palette.tint.copy(alpha = 0.12f),
                            Color.Transparent,
                            AppTheme.palette.accent.copy(alpha = 0.05f)
                        )
                    )
                )
                .padding(horizontal = 14.dp, vertical = 12.dp)
        ) {
            Column {
                // Header with card suits
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "CAREER STATS",
                        color = AppTheme.palette.accent,
                        fontSize = 11.sp,
                        fontWeight = FontWeight.ExtraBold,
                        letterSpacing = 1.sp
                    )
                    Text(
                        text = "♠ ♥ ♦ ♣",
                        color = AppTheme.palette.accent.copy(alpha = 0.5f),
                        fontSize = 11.sp,
                        fontWeight = FontWeight.Bold,
                        letterSpacing = 2.sp
                    )
                }

                Spacer(Modifier.height(10.dp))

                // 4-column metrics
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    StatMetricColumn(
                        label = "GAMES",
                        value = "${stats.totalGames}",
                        valueColor = AppTheme.palette.textPrimary,
                        modifier = Modifier.weight(1f)
                    )
                    StatMetricColumn(
                        label = "WIN RATE",
                        value = "${stats.winRatePercent}%",
                        valueColor = AppTheme.palette.accent,
                        modifier = Modifier.weight(1f)
                    )
                    val pnlColor = when {
                        stats.isZero -> AppTheme.palette.numberZero
                        stats.isPositive -> AppTheme.palette.numberPositive
                        else -> AppTheme.palette.numberNegative
                    }
                    StatMetricColumn(
                        label = "NET P&L",
                        value = stats.netProfitLossFormatted,
                        valueColor = pnlColor,
                        modifier = Modifier.weight(1.2f)
                    )
                    StatMetricColumn(
                        label = "TOP MAAL",
                        value = if (stats.highestMaal > 0) "${stats.highestMaal} pts" else "—",
                        valueColor = AppTheme.palette.accentAlt,
                        modifier = Modifier.weight(1f)
                    )
                }
            }
        }
    }
}

@Composable
private fun StatMetricColumn(
    label: String,
    value: String,
    valueColor: Color,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text(
            text = label,
            fontSize = 9.sp,
            fontWeight = FontWeight.Bold,
            color = AppTheme.palette.tint.copy(alpha = 0.6f),
            letterSpacing = 0.5.sp
        )
        Spacer(Modifier.height(3.dp))
        Text(
            text = value,
            fontSize = 14.sp,
            fontWeight = FontWeight.ExtraBold,
            color = valueColor,
            maxLines = 1,
            overflow = TextOverflow.Ellipsis
        )
    }
}

@Composable
private fun QuickStartLauncherCard(
    players: List<Player>,
    isStarting: Boolean,
    onQuickStart: () -> Unit
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(14.dp),
        colors = CardDefaults.cardColors(containerColor = AppTheme.palette.cardSurface),
        border = BorderStroke(1.dp, AppTheme.palette.accent.copy(alpha = 0.25f))
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .background(
                    Brush.verticalGradient(
                        colors = listOf(
                            AppTheme.palette.tint.copy(alpha = 0.08f),
                            Color.Transparent
                        )
                    )
                )
                .padding(12.dp)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Icon(
                        imageVector = Icons.Default.Bolt,
                        contentDescription = null,
                        tint = AppTheme.palette.accent,
                        modifier = Modifier.size(16.dp)
                    )
                    Spacer(Modifier.width(4.dp))
                    Text(
                        text = "Quick Start Table",
                        color = AppTheme.palette.accent,
                        fontSize = 13.sp,
                        fontWeight = FontWeight.Bold
                    )
                }
                Text(
                    text = "${players.size} players ready",
                    color = AppTheme.palette.tint.copy(alpha = 0.6f),
                    fontSize = 11.sp
                )
            }

            Spacer(Modifier.height(8.dp))

            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Row(
                    horizontalArrangement = Arrangement.spacedBy((-6).dp),
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.weight(1f)
                ) {
                    players.take(5).forEachIndexed { idx, player ->
                        val avatarColor = getAvatarColor(idx)
                        Surface(
                            shape = CircleShape,
                            color = avatarColor.copy(alpha = 0.9f),
                            border = BorderStroke(1.5.dp, AppTheme.palette.surface),
                            modifier = Modifier.size(28.dp)
                        ) {
                            Box(contentAlignment = Alignment.Center) {
                                Text(
                                    text = player.name.take(1).uppercase(),
                                    color = Color.White,
                                    fontSize = 12.sp,
                                    fontWeight = FontWeight.Bold
                                )
                            }
                        }
                    }
                    Spacer(Modifier.width(10.dp))
                    Text(
                        text = players.take(3).joinToString(", ") { it.name.substringBefore(" ") },
                        color = AppTheme.palette.textPrimary,
                        fontSize = 12.sp,
                        fontWeight = FontWeight.Medium,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis
                    )
                }

                Button(
                    onClick = onQuickStart,
                    enabled = !isStarting,
                    colors = ButtonDefaults.buttonColors(containerColor = AppTheme.palette.accent),
                    shape = RoundedCornerShape(8.dp),
                    contentPadding = PaddingValues(horizontal = 14.dp, vertical = 6.dp),
                    modifier = Modifier.height(34.dp)
                ) {
                    if (isStarting) {
                        CircularProgressIndicator(
                            modifier = Modifier.size(14.dp),
                            strokeWidth = 2.dp,
                            color = AppTheme.palette.surface
                        )
                    } else {
                        Icon(
                            Icons.Default.PlayArrow,
                            contentDescription = null,
                            tint = AppTheme.palette.surface,
                            modifier = Modifier.size(16.dp)
                        )
                        Spacer(Modifier.width(4.dp))
                        Text(
                            text = "Play",
                            color = AppTheme.palette.surface,
                            fontSize = 12.sp,
                            fontWeight = FontWeight.Bold
                        )
                    }
                }
            }
        }
    }
}

@Composable
private fun EnrichedActiveGameCard(
    game: EnrichedActiveGame,
    onResume: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onResume() },
        shape = RoundedCornerShape(14.dp),
        colors = CardDefaults.cardColors(containerColor = AppTheme.palette.tint.copy(alpha = 0.07f)),
        border = BorderStroke(1.dp, AppTheme.palette.tint.copy(alpha = 0.15f))
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .background(
                    Brush.verticalGradient(
                        colors = listOf(
                            AppTheme.palette.tint.copy(alpha = 0.08f),
                            Color.Transparent
                        )
                    )
                )
                .padding(14.dp)
        ) {
            // Header: Suit Emblem + Game Name + Date + Resume Button
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.weight(1f)
                ) {
                    // Card suit emblem
                    Surface(
                        shape = RoundedCornerShape(6.dp),
                        color = AppTheme.palette.accent.copy(alpha = 0.15f),
                        border = BorderStroke(1.dp, AppTheme.palette.accent.copy(alpha = 0.3f)),
                        modifier = Modifier.size(24.dp)
                    ) {
                        Box(contentAlignment = Alignment.Center) {
                            Text(
                                text = game.cardSuit,
                                color = if (game.cardSuit in listOf("♥", "♦")) Color(0xFFFF5252) else AppTheme.palette.accent,
                                fontSize = 13.sp,
                                fontWeight = FontWeight.Bold
                            )
                        }
                    }

                    Spacer(Modifier.width(8.dp))

                    Column {
                        Text(
                            text = game.name,
                            color = AppTheme.palette.textPrimary,
                            fontSize = 15.sp,
                            fontWeight = FontWeight.ExtraBold,
                            maxLines = 1,
                            overflow = TextOverflow.Ellipsis
                        )
                        Text(
                            text = "Last played: ${game.lastPlayed}",
                            color = AppTheme.palette.tint.copy(alpha = 0.6f),
                            fontSize = 10.sp
                        )
                    }
                }

                // Resume action
                Surface(
                    modifier = Modifier.size(32.dp),
                    shape = RoundedCornerShape(16.dp),
                    color = AppTheme.palette.accent.copy(alpha = 0.18f),
                    border = BorderStroke(1.dp, AppTheme.palette.accent.copy(alpha = 0.4f))
                ) {
                    Icon(
                        Icons.Default.PlayArrow,
                        contentDescription = "Resume",
                        tint = AppTheme.palette.accent,
                        modifier = Modifier.padding(6.dp)
                    )
                }
            }

            Spacer(Modifier.height(10.dp))

            // Round status & Leader badge row
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Surface(
                    shape = RoundedCornerShape(6.dp),
                    color = AppTheme.palette.tint.copy(alpha = 0.12f),
                    border = BorderStroke(0.5.dp, AppTheme.palette.tint.copy(alpha = 0.25f))
                ) {
                    Text(
                        text = game.roundStatusText,
                        color = AppTheme.palette.accentAlt,
                        fontSize = 11.sp,
                        fontWeight = FontWeight.SemiBold,
                        modifier = Modifier.padding(horizontal = 8.dp, vertical = 3.dp)
                    )
                }

                if (!game.leaderName.isNullOrEmpty() && !game.leaderScoreText.isNullOrEmpty()) {
                    Surface(
                        shape = RoundedCornerShape(6.dp),
                        color = Color(0xFFFFD700).copy(alpha = 0.15f),
                        border = BorderStroke(0.5.dp, Color(0xFFFFD700).copy(alpha = 0.35f))
                    ) {
                        Text(
                            text = "👑 ${game.leaderName} (${game.leaderScoreText})",
                            color = Color(0xFFFFD700),
                            fontSize = 11.sp,
                            fontWeight = FontWeight.Bold,
                            modifier = Modifier.padding(horizontal = 8.dp, vertical = 3.dp)
                        )
                    }
                }
            }

            // Player Avatar Bubbles
            if (game.players.isNotEmpty()) {
                Spacer(Modifier.height(10.dp))
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy((-4).dp)
                ) {
                    game.players.take(6).forEachIndexed { idx, p ->
                        val color = getAvatarColor(idx)
                        Surface(
                            shape = CircleShape,
                            color = color.copy(alpha = 0.85f),
                            border = BorderStroke(1.5.dp, AppTheme.palette.surface),
                            modifier = Modifier.size(24.dp)
                        ) {
                            Box(contentAlignment = Alignment.Center) {
                                Text(
                                    text = p.name.take(1).uppercase(),
                                    color = Color.White,
                                    fontSize = 11.sp,
                                    fontWeight = FontWeight.Bold
                                )
                            }
                        }
                    }
                    Spacer(Modifier.width(8.dp))
                    Text(
                        text = game.players.joinToString(" · ") { it.name.substringBefore(" ") },
                        color = AppTheme.palette.tint.copy(alpha = 0.6f),
                        fontSize = 11.sp,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis
                    )
                }
            }
        }
    }
}

private fun getAvatarColor(index: Int): Color {
    val colors = listOf(
        Color(0xFFE53935), // Crimson
        Color(0xFF1E88E5), // Blue
        Color(0xFF43A047), // Green
        Color(0xFFFB8C00), // Orange
        Color(0xFF8E24AA), // Purple
        Color(0xFF00ACC1)  // Teal
    )
    return colors[index % colors.size]
}
