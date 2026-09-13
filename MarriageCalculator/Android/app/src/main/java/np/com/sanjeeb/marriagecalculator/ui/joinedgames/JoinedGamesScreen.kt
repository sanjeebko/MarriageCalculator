package np.com.sanjeeb.marriagecalculator.ui.joinedgames

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
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
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import np.com.sanjeeb.marriagecalculator.data.model.Currency
import np.com.sanjeeb.marriagecalculator.ui.components.AppBackground
import np.com.sanjeeb.marriagecalculator.ui.components.GlassButton
import np.com.sanjeeb.marriagecalculator.ui.dashboard.EnrichedActiveGame
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun JoinedGamesScreen(
    onOpenGame: (String) -> Unit,
    onBack: () -> Unit,
    viewModel: JoinedGamesViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    AppBackground(modifier = Modifier.fillMaxSize()) {
        Scaffold(
            topBar = {
                TopAppBar(
                    title = {
                        Column {
                            Text(
                                text = "Joined Games",
                                color = AppTheme.palette.accent,
                                fontFamily = FontFamily.Serif,
                                fontWeight = FontWeight.Bold,
                                fontSize = 20.sp
                            )
                            Text(
                                text = "Live matches you play in",
                                color = AppTheme.palette.tint.copy(alpha = 0.5f),
                                fontSize = 11.sp
                            )
                        }
                    },
                    navigationIcon = {
                        IconButton(onClick = onBack) {
                            Icon(
                                imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                                contentDescription = "Back",
                                tint = AppTheme.palette.accent
                            )
                        }
                    },
                    actions = {
                        IconButton(
                            onClick = { viewModel.loadJoinedGames() },
                            enabled = !uiState.isLoading
                        ) {
                            if (uiState.isLoading) {
                                CircularProgressIndicator(
                                    modifier = Modifier.size(18.dp),
                                    strokeWidth = 2.dp,
                                    color = AppTheme.palette.accent
                                )
                            } else {
                                Icon(
                                    imageVector = Icons.Default.Refresh,
                                    contentDescription = "Refresh",
                                    tint = AppTheme.palette.accent
                                )
                            }
                        }
                    },
                    colors = TopAppBarDefaults.topAppBarColors(containerColor = Color.Transparent)
                )
            },
            containerColor = Color.Transparent
        ) { paddingValues ->
            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(paddingValues)
                    .padding(horizontal = 16.dp),
                verticalArrangement = Arrangement.spacedBy(14.dp)
            ) {
                // 1. Participant Career Stats Hero Banner
                item {
                    ParticipantStatsBanner(stats = uiState.careerStats)
                }

                // 2. Read-Only Participant Explanation Card
                item {
                    ParticipantInfoCard()
                }

                // 3. Filter Row (Active Matches vs All Matches)
                item {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                            FilterChip(
                                selected = uiState.selectedFilter == JoinedGamesFilter.ACTIVE,
                                onClick = { viewModel.setFilter(JoinedGamesFilter.ACTIVE) },
                                label = {
                                    val activeCount = uiState.joinedGames.count { !it.isSettled }
                                    Text("Active ($activeCount)", fontSize = 12.sp)
                                },
                                colors = FilterChipDefaults.filterChipColors(
                                    selectedContainerColor = AppTheme.palette.accent.copy(alpha = 0.20f),
                                    selectedLabelColor = AppTheme.palette.accent,
                                    containerColor = AppTheme.palette.tint.copy(alpha = 0.05f),
                                    labelColor = AppTheme.palette.textPrimary.copy(alpha = 0.7f)
                                ),
                                border = FilterChipDefaults.filterChipBorder(
                                    borderColor = if (uiState.selectedFilter == JoinedGamesFilter.ACTIVE) AppTheme.palette.accent else AppTheme.palette.tint.copy(alpha = 0.15f),
                                    enabled = true,
                                    selected = uiState.selectedFilter == JoinedGamesFilter.ACTIVE
                                )
                            )

                            FilterChip(
                                selected = uiState.selectedFilter == JoinedGamesFilter.ALL,
                                onClick = { viewModel.setFilter(JoinedGamesFilter.ALL) },
                                label = {
                                    Text("All (${uiState.joinedGames.size})", fontSize = 12.sp)
                                },
                                colors = FilterChipDefaults.filterChipColors(
                                    selectedContainerColor = AppTheme.palette.accent.copy(alpha = 0.20f),
                                    selectedLabelColor = AppTheme.palette.accent,
                                    containerColor = AppTheme.palette.tint.copy(alpha = 0.05f),
                                    labelColor = AppTheme.palette.textPrimary.copy(alpha = 0.7f)
                                ),
                                border = FilterChipDefaults.filterChipBorder(
                                    borderColor = if (uiState.selectedFilter == JoinedGamesFilter.ALL) AppTheme.palette.accent else AppTheme.palette.tint.copy(alpha = 0.15f),
                                    enabled = true,
                                    selected = uiState.selectedFilter == JoinedGamesFilter.ALL
                                )
                            )
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

                // 4. Games List or Empty State
                if (uiState.filteredGames.isEmpty() && !uiState.isLoading) {
                    item {
                        EmptyJoinedGamesCard(
                            isFilteredActive = uiState.selectedFilter == JoinedGamesFilter.ACTIVE && uiState.joinedGames.isNotEmpty(),
                            onShowAll = { viewModel.setFilter(JoinedGamesFilter.ALL) }
                        )
                    }
                } else {
                    items(uiState.filteredGames, key = { it.id }) { game ->
                        JoinedGameCard(
                            game = game,
                            onOpen = { onOpenGame(game.id) }
                        )
                    }
                }

                item {
                    Spacer(modifier = Modifier.height(24.dp))
                }
            }
        }
    }
}

@Composable
private fun ParticipantStatsBanner(stats: JoinedGamesCareerStats) {
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
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "PARTICIPANT CAREER STATS",
                        color = AppTheme.palette.accent,
                        fontSize = 11.sp,
                        fontWeight = FontWeight.ExtraBold,
                        letterSpacing = 1.sp
                    )
                    Text(
                        text = "♠ ♥ ♦ ♣",
                        color = AppTheme.palette.accentAlt,
                        fontSize = 12.sp
                    )
                }

                Spacer(Modifier.height(10.dp))

                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    StatTile(label = "Matches", value = "${stats.totalMatchesJoined}", modifier = Modifier.weight(1f))
                    StatTile(label = "Games", value = "${stats.totalGamesPlayed}", modifier = Modifier.weight(1f))
                    StatTile(label = "Wins", value = "${stats.totalWins}", modifier = Modifier.weight(1f))

                    val moneyColor = when {
                        stats.netMoney > 0 -> AppTheme.palette.numberPositive
                        stats.netMoney < 0 -> AppTheme.palette.numberNegative
                        else -> AppTheme.palette.numberZero
                    }
                    val moneyPrefix = if (stats.netMoney > 0) "+" else ""
                    StatTile(
                        label = "Net P/L",
                        value = "$moneyPrefix${stats.currency.formatMoney(stats.netMoney)}",
                        valueColor = moneyColor,
                        modifier = Modifier.weight(1.3f)
                    )
                }
            }
        }
    }
}

@Composable
private fun StatTile(
    label: String,
    value: String,
    valueColor: Color = AppTheme.palette.textPrimary,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text(
            text = value,
            color = valueColor,
            fontSize = 15.sp,
            fontWeight = FontWeight.Bold,
            maxLines = 1,
            overflow = TextOverflow.Ellipsis
        )
        Spacer(Modifier.height(2.dp))
        Text(
            text = label,
            color = AppTheme.palette.tint.copy(alpha = 0.55f),
            fontSize = 10.sp,
            fontWeight = FontWeight.Medium
        )
    }
}

@Composable
private fun ParticipantInfoCard() {
    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        color = AppTheme.palette.cardSurface.copy(alpha = 0.6f),
        border = BorderStroke(1.dp, AppTheme.palette.tint.copy(alpha = 0.15f))
    ) {
        Row(
            modifier = Modifier.padding(12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(
                modifier = Modifier
                    .size(36.dp)
                    .clip(CircleShape)
                    .background(AppTheme.palette.accent.copy(alpha = 0.15f)),
                contentAlignment = Alignment.Center
            ) {
                Icon(
                    imageVector = Icons.Default.Visibility,
                    contentDescription = null,
                    tint = AppTheme.palette.accent,
                    modifier = Modifier.size(20.dp)
                )
            }
            Spacer(modifier = Modifier.width(12.dp))
            Column {
                Text(
                    text = "Live Read-Only View",
                    color = AppTheme.palette.accent,
                    fontSize = 13.sp,
                    fontWeight = FontWeight.Bold
                )
                Spacer(modifier = Modifier.height(2.dp))
                Text(
                    text = "Scores, seating, and rounds update in real-time as the host records games. You cannot edit scores or record rounds.",
                    color = AppTheme.palette.tint.copy(alpha = 0.65f),
                    fontSize = 11.sp,
                    lineHeight = 15.sp
                )
            }
        }
    }
}

@Composable
private fun JoinedGameCard(
    game: EnrichedActiveGame,
    onOpen: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onOpen() },
        shape = RoundedCornerShape(14.dp),
        colors = CardDefaults.cardColors(containerColor = AppTheme.palette.cardSurface),
        border = BorderStroke(1.dp, AppTheme.palette.tint.copy(alpha = 0.18f))
    ) {
        Column(modifier = Modifier.padding(14.dp)) {
            // Top Row: Card Suit + Game Name + Host info + Status Badge
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.weight(1f)
                ) {
                    Text(
                        text = game.cardSuit,
                        color = if (game.cardSuit in listOf("♥", "♦")) Color(0xFFE53935) else AppTheme.palette.accent,
                        fontSize = 18.sp,
                        fontWeight = FontWeight.Bold
                    )
                    Spacer(Modifier.width(8.dp))
                    Column {
                        Text(
                            text = game.name,
                            color = AppTheme.palette.textPrimary,
                            fontWeight = FontWeight.Bold,
                            fontSize = 15.sp,
                            maxLines = 1,
                            overflow = TextOverflow.Ellipsis
                        )
                        Spacer(Modifier.height(1.dp))
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Icon(
                                imageVector = Icons.Default.Person,
                                contentDescription = null,
                                tint = AppTheme.palette.accent,
                                modifier = Modifier.size(11.dp)
                            )
                            Spacer(Modifier.width(3.dp))
                            Text(
                                text = "Host: ${game.hostUserName ?: "Host"}",
                                color = AppTheme.palette.accent,
                                fontSize = 11.sp,
                                fontWeight = FontWeight.Medium
                            )
                            Spacer(Modifier.width(6.dp))
                            Text(
                                text = "• ${game.lastPlayed}",
                                color = AppTheme.palette.tint.copy(alpha = 0.45f),
                                fontSize = 10.sp
                            )
                        }
                    }
                }

                // Read Only Badge + Status
                Column(horizontalAlignment = Alignment.End) {
                    Surface(
                        shape = RoundedCornerShape(6.dp),
                        color = AppTheme.palette.accent.copy(alpha = 0.15f),
                        border = BorderStroke(0.5.dp, AppTheme.palette.accent.copy(alpha = 0.5f))
                    ) {
                        Row(
                            modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Icon(
                                imageVector = Icons.Default.Visibility,
                                contentDescription = null,
                                tint = AppTheme.palette.accent,
                                modifier = Modifier.size(10.dp)
                            )
                            Spacer(Modifier.width(3.dp))
                            Text(
                                text = "Read Only",
                                color = AppTheme.palette.accent,
                                fontSize = 10.sp,
                                fontWeight = FontWeight.Bold
                            )
                        }
                    }
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
                    color = AppTheme.palette.tint.copy(alpha = 0.10f),
                    border = BorderStroke(0.5.dp, AppTheme.palette.tint.copy(alpha = 0.20f))
                ) {
                    Text(
                        text = "${game.roundStatusText} (${game.totalGamesPlayed} games)",
                        color = AppTheme.palette.frostAccent,
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

            // Your standing highlight
            if (game.myMoney != null) {
                Spacer(Modifier.height(8.dp))
                val standingColor = when {
                    game.myMoney > 0 -> AppTheme.palette.numberPositive
                    game.myMoney < 0 -> AppTheme.palette.numberNegative
                    else -> AppTheme.palette.numberZero
                }
                val prefix = if (game.myMoney > 0) "+" else ""
                val rankText = game.myRank?.let { "Rank #$it" } ?: ""

                Surface(
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(8.dp),
                    color = standingColor.copy(alpha = 0.10f),
                    border = BorderStroke(0.5.dp, standingColor.copy(alpha = 0.35f))
                ) {
                    Row(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(horizontal = 10.dp, vertical = 6.dp),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Text(
                                text = "Your Score:",
                                color = AppTheme.palette.textPrimary.copy(alpha = 0.8f),
                                fontSize = 12.sp,
                                fontWeight = FontWeight.Medium
                            )
                            Spacer(Modifier.width(6.dp))
                            Text(
                                text = "$prefix${Currency.NPR_Rupee.formatMoney(game.myMoney)} (${game.myNetPoints ?: 0} pts)",
                                color = standingColor,
                                fontSize = 12.sp,
                                fontWeight = FontWeight.Bold
                            )
                        }

                        if (rankText.isNotEmpty()) {
                            Text(
                                text = rankText,
                                color = AppTheme.palette.accent,
                                fontSize = 11.sp,
                                fontWeight = FontWeight.Bold
                            )
                        }
                    }
                }
            }

            // Player Avatar Bubbles & Action Button
            Spacer(Modifier.height(10.dp))
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                // Players bubble
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
                            modifier = Modifier.size(22.dp)
                        ) {
                            Box(contentAlignment = Alignment.Center) {
                                Text(
                                    text = p.name.take(1).uppercase(),
                                    color = Color.White,
                                    fontSize = 10.sp,
                                    fontWeight = FontWeight.Bold
                                )
                            }
                        }
                    }
                    Spacer(Modifier.width(8.dp))
                    Text(
                        text = "${game.players.size} players",
                        color = AppTheme.palette.tint.copy(alpha = 0.6f),
                        fontSize = 11.sp
                    )
                }

                // View Score Button
                GlassButton(
                    onClick = onOpen,
                    text = "View Scores",
                    containerColor = AppTheme.palette.accent.copy(alpha = 0.15f),
                    textColor = AppTheme.palette.accent,
                    height = 32,
                    leadingIcon = {
                        Icon(
                            imageVector = Icons.Default.Visibility,
                            contentDescription = null,
                            tint = AppTheme.palette.accent,
                            modifier = Modifier.size(14.dp)
                        )
                    }
                )
            }
        }
    }
}

@Composable
private fun EmptyJoinedGamesCard(
    isFilteredActive: Boolean,
    onShowAll: () -> Unit
) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(16.dp),
        colors = CardDefaults.cardColors(containerColor = AppTheme.palette.cardSurface.copy(alpha = 0.6f)),
        border = BorderStroke(1.dp, AppTheme.palette.tint.copy(alpha = 0.15f))
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(32.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Box(
                modifier = Modifier
                    .size(56.dp)
                    .clip(CircleShape)
                    .background(AppTheme.palette.tint.copy(alpha = 0.1f)),
                contentAlignment = Alignment.Center
            ) {
                Icon(
                    imageVector = Icons.Default.SportsEsports,
                    contentDescription = null,
                    tint = AppTheme.palette.accent.copy(alpha = 0.7f),
                    modifier = Modifier.size(32.dp)
                )
            }

            Spacer(Modifier.height(16.dp))

            Text(
                text = if (isFilteredActive) "No Active Joined Games" else "No Joined Games Yet",
                color = AppTheme.palette.textPrimary,
                fontSize = 16.sp,
                fontWeight = FontWeight.Bold
            )

            Spacer(Modifier.height(6.dp))

            Text(
                text = if (isFilteredActive) {
                    "You have no currently active games hosted by others. Check all games to view completed history."
                } else {
                    "When another player creates a game and includes your account or registered email as a player, their matches will automatically show up here for live viewing."
                },
                color = AppTheme.palette.tint.copy(alpha = 0.6f),
                fontSize = 12.sp,
                textAlign = androidx.compose.ui.text.style.TextAlign.Center,
                lineHeight = 17.sp
            )

            if (isFilteredActive) {
                Spacer(Modifier.height(16.dp))
                GlassButton(
                    onClick = onShowAll,
                    text = "Show All Joined Games",
                    containerColor = AppTheme.palette.accent.copy(alpha = 0.15f),
                    textColor = AppTheme.palette.accent,
                    height = 36
                )
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
