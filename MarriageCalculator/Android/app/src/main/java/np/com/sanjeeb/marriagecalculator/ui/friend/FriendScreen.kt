package np.com.sanjeeb.marriagecalculator.ui.friend

import android.content.Context
import android.content.Intent
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
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
import np.com.sanjeeb.marriagecalculator.data.remote.InviteCodeDto
import np.com.sanjeeb.marriagecalculator.ui.components.AppBackground
import np.com.sanjeeb.marriagecalculator.ui.components.GlassButton
import androidx.compose.material3.TabRowDefaults.tabIndicatorOffset

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun FriendScreen(
    onBack: () -> Unit,
    viewModel: FriendViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    var selectedTabIndex by remember { mutableIntStateOf(0) }

    LaunchedEffect(Unit) {
        viewModel.loadData()
    }

    // Fetch my invite code when the Add Friends tab is first opened
    LaunchedEffect(selectedTabIndex) {
        if (selectedTabIndex == 2) viewModel.loadInviteCode()
    }

    AppBackground {
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
            val tabs = listOf("My Friends", "Requests", "Add Friends")
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

            // Success feedback (code redeemed / request sent)
            uiState.actionMessage?.let { msg ->
                Card(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(16.dp),
                    colors = CardDefaults.cardColors(containerColor = Color(0xFF4CAF50).copy(alpha = 0.15f))
                ) {
                    Row(
                        modifier = Modifier.padding(16.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Icon(Icons.Default.CheckCircle, contentDescription = null, tint = Color(0xFF4CAF50))
                        Spacer(modifier = Modifier.width(12.dp))
                        Text(text = msg, color = Color(0xFF81C784), fontSize = 14.sp, modifier = Modifier.weight(1f))
                        IconButton(onClick = { viewModel.clearActionMessage() }) {
                            Icon(Icons.Default.Close, contentDescription = "Dismiss", tint = AppTheme.palette.textPrimary)
                        }
                    }
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
                        2 -> AddFriendsTab(
                            inviteCode = uiState.inviteCode,
                            inviteCodeLoading = uiState.inviteCodeLoading,
                            redeemLoading = uiState.redeemLoading,
                            addEmailLoading = uiState.addEmailLoading,
                            onRedeemCode = { code -> viewModel.redeemInviteCode(code) },
                            onSendRequest = { email -> viewModel.sendFriendRequest(email) }
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

/**
 * Private friend discovery (requirement §4.4): no open user search.
 * Friends are added by sharing/redeeming an invite code, or by a
 * complete email address (the reply never reveals whether it's registered).
 */
@Composable
private fun AddFriendsTab(
    inviteCode: InviteCodeDto?,
    inviteCodeLoading: Boolean,
    redeemLoading: Boolean,
    addEmailLoading: Boolean,
    onRedeemCode: (String) -> Unit,
    onSendRequest: (String) -> Unit
) {
    val context = androidx.compose.ui.platform.LocalContext.current
    var codeInput by remember { mutableStateOf("") }
    var emailInput by remember { mutableStateOf("") }

    val fieldColors = OutlinedTextFieldDefaults.colors(
        focusedContainerColor = AppTheme.palette.tint.copy(alpha = 0.08f),
        unfocusedContainerColor = AppTheme.palette.tint.copy(alpha = 0.05f),
        focusedBorderColor = AppTheme.palette.accent,
        unfocusedBorderColor = AppTheme.palette.tint.copy(alpha = 0.1f),
        focusedLabelColor = AppTheme.palette.accent,
        unfocusedLabelColor = AppTheme.palette.tint.copy(alpha = 0.5f),
        focusedTextColor = AppTheme.palette.textPrimary,
        unfocusedTextColor = AppTheme.palette.textPrimary
    )

    Column(
        modifier = Modifier
            .fillMaxSize()
            .verticalScroll(rememberScrollState())
    ) {
        // ---- My invite code ----
        Card(
            colors = CardDefaults.cardColors(containerColor = AppTheme.palette.accent.copy(alpha = 0.1f)),
            shape = RoundedCornerShape(12.dp),
            modifier = Modifier.fillMaxWidth()
        ) {
            Column(modifier = Modifier.padding(16.dp)) {
                Text(
                    text = "MY INVITE CODE",
                    color = AppTheme.palette.accent,
                    fontSize = 12.sp,
                    fontWeight = FontWeight.Bold,
                    letterSpacing = 1.5.sp
                )
                Spacer(modifier = Modifier.height(8.dp))
                if (inviteCodeLoading) {
                    CircularProgressIndicator(
                        color = AppTheme.palette.accent,
                        modifier = Modifier.size(24.dp),
                        strokeWidth = 2.dp
                    )
                } else if (inviteCode != null) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Text(
                            text = inviteCode.code,
                            color = AppTheme.palette.textPrimary,
                            fontSize = 26.sp,
                            fontWeight = FontWeight.Bold,
                            fontFamily = FontFamily.Monospace,
                            letterSpacing = 3.sp,
                            modifier = Modifier.weight(1f)
                        )
                        IconButton(onClick = {
                            val clipboard = context.getSystemService(Context.CLIPBOARD_SERVICE) as android.content.ClipboardManager
                            clipboard.setPrimaryClip(android.content.ClipData.newPlainText("Invite code", inviteCode.code))
                            android.widget.Toast.makeText(context, "Code copied", android.widget.Toast.LENGTH_SHORT).show()
                        }) {
                            Icon(Icons.Default.ContentCopy, contentDescription = "Copy code", tint = AppTheme.palette.accent)
                        }
                        IconButton(onClick = {
                            val sendIntent = Intent().apply {
                                action = Intent.ACTION_SEND
                                putExtra(
                                    Intent.EXTRA_TEXT,
                                    "Add me on AAA Marriage Calculator! Open Friends → Add Friends and enter my invite code: ${inviteCode.code}"
                                )
                                type = "text/plain"
                            }
                            context.startActivity(Intent.createChooser(sendIntent, null))
                        }) {
                            Icon(Icons.Default.Share, contentDescription = "Share code", tint = AppTheme.palette.accent)
                        }
                    }
                    val expiryDate = inviteCode.expiresAt.take(10)
                    if (expiryDate.isNotEmpty()) {
                        Text(
                            text = "Anyone with this code becomes your friend instantly · valid until $expiryDate",
                            color = AppTheme.palette.tint.copy(alpha = 0.5f),
                            fontSize = 11.sp
                        )
                    }
                } else {
                    Text(
                        text = "Couldn't load your code. Pull back and retry.",
                        color = AppTheme.palette.tint.copy(alpha = 0.5f),
                        fontSize = 12.sp
                    )
                }
            }
        }

        Spacer(modifier = Modifier.height(24.dp))

        // ---- Redeem a friend's code ----
        Text(
            text = "Have a friend's code?",
            color = AppTheme.palette.accent,
            fontSize = 14.sp,
            fontWeight = FontWeight.Bold
        )
        Spacer(modifier = Modifier.height(8.dp))
        OutlinedTextField(
            value = codeInput,
            onValueChange = { codeInput = it.uppercase() },
            label = { Text("Enter invite code") },
            leadingIcon = { Icon(Icons.Default.Key, contentDescription = null, tint = AppTheme.palette.accent) },
            modifier = Modifier.fillMaxWidth(),
            singleLine = true,
            colors = fieldColors,
            shape = RoundedCornerShape(12.dp)
        )
        Spacer(modifier = Modifier.height(8.dp))
        GlassButton(
            onClick = {
                onRedeemCode(codeInput)
                codeInput = ""
            },
            text = "Add Friend by Code",
            containerColor = AppTheme.palette.cta.copy(alpha = 0.35f),
            textColor = AppTheme.palette.accent,
            height = 48,
            enabled = codeInput.trim().isNotEmpty(),
            isLoading = redeemLoading,
            leadingIcon = {
                Icon(Icons.Default.PersonAdd, null, tint = AppTheme.palette.accent, modifier = Modifier.size(18.dp))
            }
        )

        Spacer(modifier = Modifier.height(24.dp))

        // ---- Add by email ----
        Text(
            text = "Or add by email",
            color = AppTheme.palette.accent,
            fontSize = 14.sp,
            fontWeight = FontWeight.Bold
        )
        Spacer(modifier = Modifier.height(8.dp))
        OutlinedTextField(
            value = emailInput,
            onValueChange = { emailInput = it },
            label = { Text("Friend's full email address") },
            leadingIcon = { Icon(Icons.Default.Email, contentDescription = null, tint = AppTheme.palette.accent) },
            modifier = Modifier.fillMaxWidth(),
            singleLine = true,
            colors = fieldColors,
            shape = RoundedCornerShape(12.dp)
        )
        Spacer(modifier = Modifier.height(8.dp))
        GlassButton(
            onClick = {
                onSendRequest(emailInput)
                emailInput = ""
            },
            text = "Send Friend Request",
            containerColor = AppTheme.palette.tint.copy(alpha = 0.12f),
            textColor = AppTheme.palette.textPrimary,
            height = 48,
            enabled = emailInput.trim().isNotEmpty(),
            isLoading = addEmailLoading,
            leadingIcon = {
                Icon(Icons.Default.Send, null, tint = AppTheme.palette.accent, modifier = Modifier.size(16.dp))
            }
        )
        Text(
            text = "If they're not on the app yet, we'll email them an invitation — your request is waiting when they join.",
            color = AppTheme.palette.tint.copy(alpha = 0.4f),
            fontSize = 11.sp,
            modifier = Modifier.padding(top = 6.dp)
        )

        Spacer(modifier = Modifier.height(24.dp))
    }
}
