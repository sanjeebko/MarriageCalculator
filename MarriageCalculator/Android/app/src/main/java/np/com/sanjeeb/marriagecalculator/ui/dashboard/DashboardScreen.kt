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
import np.com.sanjeeb.marriagecalculator.ui.components.GlassButton
import np.com.sanjeeb.marriagecalculator.ui.components.MetallicButton
import np.com.sanjeeb.marriagecalculator.ui.components.MetallicRedFace
import np.com.sanjeeb.marriagecalculator.ui.components.MetallicRedRim
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
    val drawerState = rememberDrawerState(initialValue = DrawerValue.Closed)

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
                                    Color(0xFF1A1A2E).copy(alpha = 0.95f),
                                    Color(0xFF0D0D1A).copy(alpha = 0.9f)
                                )
                            )
                        )
                        .border(
                            width = 1.dp,
                            brush = Brush.horizontalGradient(
                                colors = listOf(Color.White.copy(alpha = 0.2f), Color.Transparent)
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
                                        Color.White.copy(alpha = 0.05f),
                                        Color.Transparent,
                                        Color.Black.copy(alpha = 0.05f)
                                    )
                                )
                            )
                    )
                    
                    Column(modifier = Modifier.fillMaxSize()) {
                        Spacer(Modifier.height(56.dp))
                        
                        val drawerItemColors = NavigationDrawerItemDefaults.colors(
                            unselectedContainerColor = Color.White.copy(alpha = 0.05f),
                            selectedContainerColor = Color.White.copy(alpha = 0.15f),
                            unselectedIconColor = GoldAccent.copy(alpha = 0.9f),
                            unselectedTextColor = Color.White
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
                            color = Color.White.copy(alpha = 0.15f)
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
                                            tint = GoldAccent,
                                            modifier = Modifier.size(36.dp)
                                        )
                                    }
                                    
                                    // Google Badge
                                    Surface(
                                        modifier = Modifier.size(12.dp).offset(x = 2.dp, y = 2.dp),
                                        shape = RoundedCornerShape(6.dp),
                                        color = Color.White
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
                                    color = GoldAccent,
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
                            Icon(Icons.Default.Menu, contentDescription = "Menu", tint = GoldAccent)
                        }
                    },
                    colors = TopAppBarDefaults.centerAlignedTopAppBarColors(
                        containerColor = TiharNightBlue,
                        titleContentColor = GoldAccent
                    )
                )
            },
            containerColor = Color.Transparent
        ) { padding ->
            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(padding)
                    .background(
                        Brush.verticalGradient(
                            colors = listOf(TiharNightBlue, Color(0xFF0D0D1A))
                        )
                    )
            ) {
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(horizontal = 16.dp),
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    // Small Spacer
                    Spacer(modifier = Modifier.height(16.dp))

                    // Medium Compact Buttons
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        GlassButton(
                            onClick = onNewGame,
                            text = "New Game",
                            containerColor = DeepRedTika.copy(alpha = 0.35f),
                            textColor = GoldAccent,
                            height = 44,
                            modifier = Modifier.weight(1f),
                            leadingIcon = {
                                Icon(Icons.Default.Add, null, tint = GoldAccent, modifier = Modifier.size(18.dp))
                            }
                        )

                        if (!uiState.isOfflineMode) {
                            GlassButton(
                                onClick = onFriends,
                                text = "Friends",
                                containerColor = Color.White.copy(alpha = 0.12f),
                                textColor = Color.White,
                                height = 44,
                                modifier = Modifier.weight(1f),
                                leadingIcon = {
                                    Icon(Icons.Default.People, null, tint = GoldAccent, modifier = Modifier.size(18.dp))
                                }
                            )
                        }
                    }

                    // Offline indicator (compact)
                    if (uiState.isOfflineMode) {
                        Spacer(modifier = Modifier.height(8.dp))
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Icon(Icons.Default.WifiOff, null, tint = MarigoldOrange, modifier = Modifier.size(12.dp))
                            Spacer(modifier = Modifier.width(4.dp))
                            Text("Offline Mode", color = MarigoldOrange, fontSize = 10.sp)
                        }
                    }

                    Spacer(modifier = Modifier.height(20.dp))

                    // Active Games Section
                    if (uiState.activeGames.isNotEmpty() || uiState.isLoading) {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Text(
                                text = "Active Games",
                                color = GoldAccent,
                                fontSize = 16.sp,
                                fontWeight = FontWeight.Bold,
                                fontFamily = FontFamily.Serif
                            )
                            if (uiState.isLoading) {
                                CircularProgressIndicator(
                                    modifier = Modifier.size(16.dp),
                                    strokeWidth = 2.dp,
                                    color = GoldAccent
                                )
                            }
                        }
                        
                        Spacer(modifier = Modifier.height(8.dp))

                        LazyColumn(
                            verticalArrangement = Arrangement.spacedBy(8.dp),
                            contentPadding = PaddingValues(bottom = 16.dp)
                        ) {
                            items(uiState.activeGames) { game ->
                                ActiveGameCardCompact(game = game, onResume = { onResumeGame(game.id) })
                            }
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun ActiveGameCardCompact(game: MarriageGameSet, onResume: () -> Unit) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onResume() },
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = Color.White.copy(alpha = 0.05f)),
        border = BorderStroke(1.dp, Color.White.copy(alpha = 0.1f))
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .background(
                    Brush.verticalGradient(
                        colors = listOf(Color.White.copy(alpha = 0.05f), Color.Transparent)
                    )
                )
                .padding(16.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = game.name,
                    color = Color.White,
                    fontSize = 15.sp,
                    fontWeight = FontWeight.ExtraBold,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                Spacer(Modifier.height(2.dp))
                Text(
                    text = "Last played: ${game.lastPlayed.take(10)}",
                    color = Color.White.copy(alpha = 0.5f),
                    fontSize = 11.sp,
                    fontWeight = FontWeight.Medium
                )
            }
            Surface(
                modifier = Modifier.size(32.dp),
                shape = RoundedCornerShape(16.dp),
                color = GoldAccent.copy(alpha = 0.15f),
                border = BorderStroke(1.dp, GoldAccent.copy(alpha = 0.3f))
            ) {
                Icon(
                    Icons.Default.PlayArrow,
                    contentDescription = "Resume",
                    tint = GoldAccent,
                    modifier = Modifier.padding(4.dp)
                )
            }
        }
    }
}
