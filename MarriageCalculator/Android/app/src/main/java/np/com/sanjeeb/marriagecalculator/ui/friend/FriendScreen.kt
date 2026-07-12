package np.com.sanjeeb.marriagecalculator.ui.friend

import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import np.com.sanjeeb.marriagecalculator.data.model.User
import np.com.sanjeeb.marriagecalculator.data.remote.FriendshipDto
import np.com.sanjeeb.marriagecalculator.ui.components.MetallicButton
import androidx.compose.material3.TabRowDefaults.tabIndicatorOffset

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun FriendScreen(
    onBack: () -> Unit,
    viewModel: FriendViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    var selectedTabIndex by remember { mutableIntStateOf(0) }
    var searchQuery by remember { mutableStateOf("") }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(
                Brush.verticalGradient(
                    colors = listOf(AppTheme.palette.backgroundTop, AppTheme.palette.backgroundBottom)
                )
            )
    ) {
        Column(modifier = Modifier.fillMaxSize()) {
            // Top Bar
            TopAppBar(
                title = {
                    Text(
                        text = "Friends & Social",
                        color = AppTheme.palette.accent,
                        fontFamily = FontFamily.Serif,
                        fontWeight = FontWeight.Bold
                    )
                },
                navigationIcon = {
                    IconButton(onClick = onBack) {
                        Icon(
                            imageVector = Icons.Default.ArrowBack,
                            contentDescription = "Back",
                            tint = AppTheme.palette.accent
                        )
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = Color.Transparent
                )
            )

            // Tabs
            val tabs = listOf("My Friends", "Requests", "Find Users")
            TabRow(
                selectedTabIndex = selectedTabIndex,
                containerColor = AppTheme.palette.tint.copy(alpha = 0.03f),
                contentColor = AppTheme.palette.accent,
                indicator = { tabPositions ->
                    TabRowDefaults.SecondaryIndicator(
                        Modifier.tabIndicatorOffset(tabPositions[selectedTabIndex]),
                        color = AppTheme.palette.accent
                    )
                }
            ) {
                tabs.forEachIndexed { index, title ->
                    val badgeCount = if (index == 1) uiState.pendingReceived.size else 0
                    Tab(
                        selected = selectedTabIndex == index,
                        onClick = { selectedTabIndex = index },
                        text = {
                            Row(verticalAlignment = Alignment.CenterVertically) {
                                Text(
                                    text = title,
                                    color = if (selectedTabIndex == index) AppTheme.palette.accent else AppTheme.palette.tint.copy(alpha = 0.6f),
                                    fontWeight = if (selectedTabIndex == index) FontWeight.Bold else FontWeight.Normal
                                )
                                if (badgeCount > 0) {
                                    Spacer(modifier = Modifier.width(6.dp))
                                    Badge(containerColor = AppTheme.palette.accentAlt) {
                                        Text(text = badgeCount.toString(), color = AppTheme.palette.textPrimary)
                                    }
                                }
                            }
                        }
                    )
                }
            }

            // Error display
            uiState.error?.let { err ->
                Card(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(16.dp),
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
                            Icon(Icons.Default.Close, contentDescription = "Clear error", tint = AppTheme.palette.textPrimary)
                        }
                    }
                }
            }

            // Tab Content
            Box(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth()
                    .padding(16.dp)
            ) {
                if (uiState.isLoading && selectedTabIndex != 2) {
                    CircularProgressIndicator(
                        color = AppTheme.palette.accent,
                        modifier = Modifier.align(Alignment.Center)
                    )
                } else {
                    when (selectedTabIndex) {
                        0 -> FriendsTab(uiState.friends, onRemoveFriend = { viewModel.removeFriend(it.userId) })
                        1 -> RequestsTab(
                            received = uiState.pendingReceived,
                            sent = uiState.pendingSent,
                            onRespond = { id, accept -> viewModel.respondToRequest(id, accept) },
                            onCancel = { id -> viewModel.removeFriend(id) }
                        )
                        2 -> SearchTab(
                            query = searchQuery,
                            onQueryChange = {
                                searchQuery = it
                                viewModel.searchUsers(it)
                            },
                            loading = uiState.searchLoading,
                            results = uiState.searchResults,
                            error = uiState.searchError,
                            sentList = uiState.pendingSent,
                            friendsList = uiState.friends,
                            onSendRequest = { email ->
                                viewModel.sendFriendRequest(email) {
                                    searchQuery = ""
                                    selectedTabIndex = 1
                                }
                            }
                        )
                    }
                }
            }
        }
    }
}

@Composable
private fun FriendsTab(
    friends: List<User>,
    onRemoveFriend: (User) -> Unit
) {
    if (friends.isEmpty()) {
        Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                Icon(
                    imageVector = Icons.Default.People,
                    contentDescription = null,
                    tint = AppTheme.palette.tint.copy(alpha = 0.2f),
                    modifier = Modifier.size(64.dp)
                )
                Spacer(modifier = Modifier.height(16.dp))
                Text(text = "No friends added yet.", color = AppTheme.palette.tint.copy(alpha = 0.4f))
            }
        }
    } else {
        LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
            items(friends) { friend ->
                Card(
                    colors = CardDefaults.cardColors(containerColor = AppTheme.palette.tint.copy(alpha = 0.06f)),
                    shape = RoundedCornerShape(12.dp)
                ) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(16.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        // Avatar placeholder
                        Box(
                            modifier = Modifier
                                .size(40.dp)
                                .background(AppTheme.palette.accent.copy(alpha = 0.1f), RoundedCornerShape(20.dp)),
                            contentAlignment = Alignment.Center
                        ) {
                            Text(
                                text = friend.displayName.take(1).uppercase(),
                                color = AppTheme.palette.accent,
                                fontWeight = FontWeight.Bold,
                                fontSize = 18.sp
                            )
                        }
                        Spacer(modifier = Modifier.width(16.dp))
                        Column(modifier = Modifier.weight(1f)) {
                            Text(
                                text = friend.displayName,
                                color = AppTheme.palette.textPrimary,
                                fontWeight = FontWeight.Bold,
                                fontSize = 16.sp,
                                maxLines = 1,
                                overflow = TextOverflow.Ellipsis
                            )
                            Text(
                                text = friend.email,
                                color = AppTheme.palette.tint.copy(alpha = 0.5f),
                                fontSize = 12.sp,
                                maxLines = 1,
                                overflow = TextOverflow.Ellipsis
                            )
                        }
                        IconButton(onClick = { onRemoveFriend(friend) }) {
                            Icon(
                                imageVector = Icons.Default.PersonRemove,
                                contentDescription = "Remove friend",
                                tint = Color(0xFFFF5252)
                            )
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun RequestsTab(
    received: List<FriendshipDto>,
    sent: List<FriendshipDto>,
    onRespond: (String, Boolean) -> Unit,
    onCancel: (String) -> Unit
) {
    LazyColumn(
        verticalArrangement = Arrangement.spacedBy(16.dp),
        modifier = Modifier.fillMaxSize()
    ) {
        if (received.isNotEmpty()) {
            item {
                Text(
                    text = "Received Requests",
                    color = AppTheme.palette.accent,
                    fontSize = 16.sp,
                    fontWeight = FontWeight.Bold
                )
            }
            items(received) { req ->
                Card(
                    colors = CardDefaults.cardColors(containerColor = AppTheme.palette.tint.copy(alpha = 0.08f)),
                    shape = RoundedCornerShape(12.dp)
                ) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(12.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Column(modifier = Modifier.weight(1f)) {
                            Text(text = req.requesterName, color = AppTheme.palette.textPrimary, fontWeight = FontWeight.Bold)
                            Text(text = req.requesterEmail, color = AppTheme.palette.tint.copy(alpha = 0.5f), fontSize = 12.sp)
                        }
                        IconButton(onClick = { onRespond(req.id, true) }) {
                            Icon(Icons.Default.Check, contentDescription = "Accept", tint = Color.Green)
                        }
                        IconButton(onClick = { onRespond(req.id, false) }) {
                            Icon(Icons.Default.Close, contentDescription = "Decline", tint = Color(0xFFFF5252))
                        }
                    }
                }
            }
        }

        if (sent.isNotEmpty()) {
            item {
                Text(
                    text = "Sent Requests",
                    color = AppTheme.palette.accent,
                    fontSize = 16.sp,
                    fontWeight = FontWeight.Bold
                )
            }
            items(sent) { req ->
                Card(
                    colors = CardDefaults.cardColors(containerColor = AppTheme.palette.tint.copy(alpha = 0.04f)),
                    shape = RoundedCornerShape(12.dp)
                ) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(12.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Column(modifier = Modifier.weight(1f)) {
                            Text(text = req.receiverName, color = AppTheme.palette.textPrimary, fontWeight = FontWeight.Bold)
                            Text(text = req.receiverEmail, color = AppTheme.palette.tint.copy(alpha = 0.5f), fontSize = 12.sp)
                        }
                        IconButton(onClick = { onCancel(req.id) }) {
                            Icon(Icons.Default.Delete, contentDescription = "Cancel request", tint = Color.LightGray)
                        }
                    }
                }
            }
        }

        if (received.isEmpty() && sent.isEmpty()) {
            item {
                Box(
                    modifier = Modifier
                        .fillParentMaxSize()
                        .padding(bottom = 64.dp),
                    contentAlignment = Alignment.Center
                ) {
                    Text(text = "No pending invitations.", color = AppTheme.palette.tint.copy(alpha = 0.4f))
                }
            }
        }
    }
}

@Composable
private fun SearchTab(
    query: String,
    onQueryChange: (String) -> Unit,
    loading: Boolean,
    results: List<User>,
    error: String?,
    sentList: List<FriendshipDto>,
    friendsList: List<User>,
    onSendRequest: (String) -> Unit
) {
    Column(modifier = Modifier.fillMaxSize()) {
        OutlinedTextField(
            value = query,
            onValueChange = onQueryChange,
            label = { Text("Search users", color = AppTheme.palette.accent) },
            placeholder = { Text("Enter email or display name", color = AppTheme.palette.tint.copy(alpha = 0.3f)) },
            leadingIcon = { Icon(Icons.Default.Search, contentDescription = null, tint = AppTheme.palette.accent) },
            modifier = Modifier.fillMaxWidth(),
            singleLine = true,
            colors = OutlinedTextFieldDefaults.colors(
                focusedContainerColor = Color(0xFF1C1C1C),
                unfocusedContainerColor = AppTheme.palette.tint.copy(alpha = 0.08f),
                focusedBorderColor = AppTheme.palette.accent,
                unfocusedBorderColor = AppTheme.palette.tint.copy(alpha = 0.1f),
                focusedLabelColor = AppTheme.palette.accent,
                unfocusedLabelColor = AppTheme.palette.tint.copy(alpha = 0.5f),
                focusedTextColor = AppTheme.palette.textPrimary,
                unfocusedTextColor = AppTheme.palette.textPrimary
            ),
            shape = RoundedCornerShape(12.dp)
        )

        Spacer(modifier = Modifier.height(16.dp))

        if (loading) {
            Box(modifier = Modifier.weight(1f).fillMaxWidth(), contentAlignment = Alignment.Center) {
                CircularProgressIndicator(color = AppTheme.palette.accent)
            }
        } else if (error != null) {
            Box(modifier = Modifier.weight(1f).fillMaxWidth(), contentAlignment = Alignment.Center) {
                Text(text = error, color = Color(0xFFFF5252))
            }
        } else if (results.isEmpty() && query.trim().isNotEmpty()) {
            Box(modifier = Modifier.weight(1f).fillMaxWidth(), contentAlignment = Alignment.Center) {
                Text(text = "No users found matching search.", color = AppTheme.palette.tint.copy(alpha = 0.4f))
            }
        } else {
            LazyColumn(
                modifier = Modifier.weight(1f),
                verticalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                items(results) { user ->
                    val alreadyFriends = friendsList.any { it.userId == user.userId }
                    val alreadySent = sentList.any { it.receiverUserId == user.userId }

                    Card(
                        colors = CardDefaults.cardColors(containerColor = AppTheme.palette.tint.copy(alpha = 0.06f)),
                        shape = RoundedCornerShape(12.dp)
                    ) {
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(12.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Column(modifier = Modifier.weight(1f)) {
                                Text(text = user.displayName, color = AppTheme.palette.textPrimary, fontWeight = FontWeight.Bold)
                                Text(text = user.email, color = AppTheme.palette.tint.copy(alpha = 0.5f), fontSize = 12.sp)
                            }

                            when {
                                alreadyFriends -> {
                                    Text(text = "Friends", color = Color.Green, fontSize = 14.sp, fontWeight = FontWeight.Bold)
                                }
                                alreadySent -> {
                                    Text(text = "Pending", color = AppTheme.palette.accentAlt, fontSize = 14.sp)
                                }
                                else -> {
                                    IconButton(onClick = { onSendRequest(user.email) }) {
                                        Icon(Icons.Default.PersonAdd, contentDescription = "Add friend", tint = AppTheme.palette.accent)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
