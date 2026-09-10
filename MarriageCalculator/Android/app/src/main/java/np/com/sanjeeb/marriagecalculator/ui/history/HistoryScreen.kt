package np.com.sanjeeb.marriagecalculator.ui.history

import androidx.compose.animation.*
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.drawWithContent
import androidx.compose.ui.graphics.BlendMode
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.CompositingStrategy
import androidx.compose.ui.graphics.graphicsLayer
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import np.com.sanjeeb.marriagecalculator.data.local.ActivityLogEntity
import np.com.sanjeeb.marriagecalculator.data.local.ActivityType
import np.com.sanjeeb.marriagecalculator.ui.components.HistoryBackground
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

@OptIn(ExperimentalMaterial3Api::class, androidx.compose.foundation.ExperimentalFoundationApi::class)
@Composable
fun HistoryScreen(
    onBack: () -> Unit,
    viewModel: HistoryViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    val palette = AppTheme.palette
    val listState = rememberLazyListState()

    var showDatePicker by remember { mutableStateOf(false) }

    // Scroll to date target when jump requested
    LaunchedEffect(uiState.jumpToTargetIndex) {
        val target = uiState.jumpToTargetIndex ?: return@LaunchedEffect
        listState.animateScrollToItem(target)
        viewModel.onScrollHandled()
    }

    // Prefetch for virtual paging
    val shouldLoadMore by remember {
        derivedStateOf {
            val lastVisibleItem = listState.layoutInfo.visibleItemsInfo.lastOrNull() ?: return@derivedStateOf false
            lastVisibleItem.index >= listState.layoutInfo.totalItemsCount - 5
        }
    }
    LaunchedEffect(shouldLoadMore) {
        if (shouldLoadMore) {
            viewModel.loadMore()
        }
    }

    HistoryBackground {
        Scaffold(
            containerColor = Color.Transparent,
            topBar = {
                TopAppBar(
                    title = {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Text(
                                text = "History & Logs",
                                fontSize = 18.sp,
                                fontWeight = FontWeight.Bold,
                                color = palette.textPrimary
                            )
                            if (uiState.totalCount > 0) {
                                Spacer(modifier = Modifier.width(8.dp))
                                Surface(
                                    color = palette.surface.copy(alpha = 0.8f),
                                    shape = RoundedCornerShape(12.dp)
                                ) {
                                    Text(
                                        text = "${uiState.totalCount}",
                                        fontSize = 11.sp,
                                        fontWeight = FontWeight.Medium,
                                        color = palette.accent,
                                        modifier = Modifier.padding(horizontal = 7.dp, vertical = 2.dp)
                                    )
                                }
                            }
                        }
                    },
                    navigationIcon = {
                        IconButton(onClick = onBack) {
                            Icon(
                                imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                                contentDescription = "Back",
                                tint = palette.textPrimary,
                                modifier = Modifier.size(24.dp)
                            )
                        }
                    },
                    actions = {
                        // Small Calendar Date Jump button
                        IconButton(onClick = { showDatePicker = true }) {
                            Icon(
                                imageVector = Icons.Default.CalendarMonth,
                                contentDescription = "Jump to Date",
                                tint = palette.accent,
                                modifier = Modifier.size(22.dp)
                            )
                        }

                        // Refresh button
                        IconButton(onClick = { viewModel.refresh() }) {
                            Icon(
                                imageVector = Icons.Default.Refresh,
                                contentDescription = "Refresh",
                                tint = palette.textPrimary,
                                modifier = Modifier.size(22.dp)
                            )
                        }
                    },
                    colors = TopAppBarDefaults.topAppBarColors(containerColor = Color.Transparent)
                )
            }
        ) { paddingValues ->
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(paddingValues)
            ) {
                // Compact Filter Bar (Icon-first)
                CompactFilterBar(
                    selectedFilter = uiState.filter,
                    onFilterSelected = { viewModel.setFilter(it) }
                )

                // Virtualized Content with Fading Edge Shaders
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .graphicsLayer { compositingStrategy = CompositingStrategy.Offscreen }
                        .drawWithContent {
                            drawContent()
                            // Top smooth fade
                            drawRect(
                                brush = Brush.verticalGradient(
                                    colors = listOf(Color.Transparent, Color.Black),
                                    startY = 0f,
                                    endY = 40.dp.toPx()
                                ),
                                blendMode = BlendMode.DstIn
                            )
                            // Bottom smooth fade
                            drawRect(
                                brush = Brush.verticalGradient(
                                    colors = listOf(Color.Black, Color.Transparent),
                                    startY = size.height - 48.dp.toPx(),
                                    endY = size.height
                                ),
                                blendMode = BlendMode.DstIn
                            )
                        }
                ) {
                    if (uiState.isLoading && uiState.items.isEmpty()) {
                        Box(
                            modifier = Modifier.fillMaxSize(),
                            contentAlignment = Alignment.Center
                        ) {
                            CircularProgressIndicator(
                                color = palette.accent,
                                strokeWidth = 2.dp,
                                modifier = Modifier.size(28.dp)
                            )
                        }
                    } else if (uiState.items.isEmpty()) {
                        EmptyStateView(filter = uiState.filter)
                    } else {
                        LazyColumn(
                            state = listState,
                            contentPadding = PaddingValues(horizontal = 14.dp, vertical = 8.dp),
                            verticalArrangement = Arrangement.spacedBy(6.dp),
                            modifier = Modifier.fillMaxSize()
                        ) {
                            items(
                                items = uiState.items,
                                key = { item ->
                                    when (item) {
                                        is HistoryDisplayItem.DateSectionHeader -> "date_${item.dateKey}"
                                        is HistoryDisplayItem.LoginSummaryCard -> "login_${item.dateKey}"
                                        is HistoryDisplayItem.ActivityCard -> "log_${item.log.id}_${item.log.timestamp}"
                                    }
                                }
                            ) { item ->
                                Box(modifier = Modifier.animateItemPlacement()) {
                                    when (item) {
                                        is HistoryDisplayItem.DateSectionHeader -> {
                                            DateSectionHeaderView(header = item)
                                        }
                                        is HistoryDisplayItem.LoginSummaryCard -> {
                                            LoginSummaryCardView(
                                                card = item,
                                                onToggleExpand = { viewModel.toggleLoginExpanded(item.dateKey) }
                                            )
                                        }
                                        is HistoryDisplayItem.ActivityCard -> {
                                            ActivityCardView(log = item.log)
                                        }
                                    }
                                }
                            }

                            if (uiState.isPaging) {
                                item(key = "loading_more") {
                                    Box(
                                        modifier = Modifier
                                            .fillMaxWidth()
                                            .padding(12.dp),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        CircularProgressIndicator(
                                            color = palette.accent,
                                            strokeWidth = 2.dp,
                                            modifier = Modifier.size(20.dp)
                                        )
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // Material 3 Date Picker Dialog for Calendar Jump
    if (showDatePicker) {
        val datePickerState = rememberDatePickerState(
            initialSelectedDateMillis = System.currentTimeMillis()
        )
        DatePickerDialog(
            onDismissRequest = { showDatePicker = false },
            confirmButton = {
                TextButton(
                    onClick = {
                        val selected = datePickerState.selectedDateMillis
                        if (selected != null) {
                            viewModel.jumpToDate(selected)
                        }
                        showDatePicker = false
                    }
                ) {
                    Text("Jump", color = palette.accent, fontWeight = FontWeight.Bold)
                }
            },
            dismissButton = {
                TextButton(onClick = { showDatePicker = false }) {
                    Text("Cancel", color = palette.textPrimary.copy(alpha = 0.7f))
                }
            },
            colors = DatePickerDefaults.colors(
                containerColor = palette.surface
            )
        ) {
            DatePicker(
                state = datePickerState,
                colors = DatePickerDefaults.colors(
                    containerColor = palette.surface,
                    titleContentColor = palette.textPrimary,
                    headlineContentColor = palette.accent,
                    weekdayContentColor = palette.accentAlt,
                    subheadContentColor = palette.textPrimary,
                    selectedDayContainerColor = palette.accent,
                    selectedDayContentColor = palette.backgroundTop,
                    todayDateBorderColor = palette.accent,
                    todayContentColor = palette.accent
                )
            )
        }
    }
}

@Composable
private fun CompactFilterBar(
    selectedFilter: HistoryFilter,
    onFilterSelected: (HistoryFilter) -> Unit
) {
    val palette = AppTheme.palette
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(horizontal = 14.dp, vertical = 6.dp),
        horizontalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        HistoryFilter.entries.forEach { filter ->
            val isSelected = filter == selectedFilter
            val icon = when (filter) {
                HistoryFilter.ALL -> Icons.Default.AllInclusive
                HistoryFilter.GAMES -> Icons.Default.Casino
                HistoryFilter.PAYMENTS -> Icons.Default.Payments
                HistoryFilter.LOGINS -> Icons.Default.Person
            }

            val containerColor = if (isSelected) {
                palette.accent
            } else {
                palette.surface.copy(alpha = 0.88f)
            }
            val contentColor = if (isSelected) {
                if (palette.isDark) palette.backgroundBottom else Color.White
            } else {
                palette.textPrimary
            }
            val borderColor = if (isSelected) {
                palette.accent
            } else {
                if (palette.isDark) palette.tint.copy(alpha = 0.22f) else palette.tint.copy(alpha = 0.15f)
            }
            val iconTint = if (isSelected) {
                contentColor
            } else {
                if (palette.isDark) palette.accentAlt else palette.accent
            }

            Surface(
                color = containerColor,
                border = androidx.compose.foundation.BorderStroke(
                    width = if (isSelected) 1.5.dp else 1.dp,
                    color = borderColor
                ),
                shape = RoundedCornerShape(18.dp),
                shadowElevation = if (isSelected) 3.dp else 1.dp,
                modifier = Modifier
                    .weight(1f)
                    .clip(RoundedCornerShape(18.dp))
                    .clickable { onFilterSelected(filter) }
            ) {
                Row(
                    modifier = Modifier.padding(vertical = 7.dp),
                    horizontalArrangement = Arrangement.Center,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Icon(
                        imageVector = icon,
                        contentDescription = filter.label,
                        tint = iconTint,
                        modifier = Modifier.size(15.dp)
                    )
                    Spacer(modifier = Modifier.width(4.dp))
                    Text(
                        text = filter.label,
                        fontSize = 11.sp,
                        fontWeight = if (isSelected) FontWeight.Bold else FontWeight.Medium,
                        color = contentColor
                    )
                }
            }
        }
    }
}

@Composable
private fun DateSectionHeaderView(header: HistoryDisplayItem.DateSectionHeader) {
    val palette = AppTheme.palette
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(top = 10.dp, bottom = 4.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Icon(
            imageVector = Icons.Default.Event,
            contentDescription = null,
            tint = palette.accentAlt,
            modifier = Modifier.size(14.dp)
        )
        Spacer(modifier = Modifier.width(6.dp))
        Text(
            text = header.displayDate,
            fontSize = 12.sp,
            fontWeight = FontWeight.Bold,
            color = palette.accentAlt
        )
        Spacer(modifier = Modifier.width(8.dp))
        Box(
            modifier = Modifier
                .weight(1f)
                .height(0.8.dp)
                .background(palette.tint.copy(alpha = 0.12f))
        )
    }
}

@Composable
private fun LoginSummaryCardView(
    card: HistoryDisplayItem.LoginSummaryCard,
    onToggleExpand: () -> Unit
) {
    val palette = AppTheme.palette
    Surface(
        color = palette.cardSurface.copy(alpha = 0.86f),
        shape = RoundedCornerShape(10.dp),
        border = androidx.compose.foundation.BorderStroke(1.dp, palette.tint.copy(alpha = 0.16f)),
        modifier = Modifier
            .fillMaxWidth()
            .clip(RoundedCornerShape(10.dp))
            .clickable { onToggleExpand() }
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 12.dp, vertical = 9.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(
                imageVector = Icons.Default.Login,
                contentDescription = null,
                tint = palette.accent,
                modifier = Modifier.size(20.dp)
            )

            Spacer(modifier = Modifier.width(12.dp))

            Column(modifier = Modifier.weight(1f)) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Text(
                        text = if (card.count > 1) "Logged in ×${card.count}" else "Logged in",
                        fontSize = 13.sp,
                        fontWeight = FontWeight.SemiBold,
                        color = palette.textPrimary
                    )
                    Spacer(modifier = Modifier.width(6.dp))
                    Surface(
                        color = palette.surface,
                        shape = RoundedCornerShape(4.dp)
                    ) {
                        Text(
                            text = card.primaryUser,
                            fontSize = 10.sp,
                            color = palette.textPrimary.copy(alpha = 0.7f),
                            modifier = Modifier.padding(horizontal = 5.dp, vertical = 1.dp)
                        )
                    }
                }
                Text(
                    text = card.timeSpan,
                    fontSize = 11.sp,
                    color = palette.textPrimary.copy(alpha = 0.55f)
                )
            }

            // Expand Icon
            Icon(
                imageVector = if (card.isExpanded) Icons.Default.ExpandLess else Icons.Default.ExpandMore,
                contentDescription = if (card.isExpanded) "Collapse" else "Expand",
                tint = palette.textPrimary.copy(alpha = 0.4f),
                modifier = Modifier.size(18.dp)
            )
        }
    }
}

@Composable
private fun ActivityCardView(log: ActivityLogEntity) {
    val palette = AppTheme.palette
    val timeFormatted = remember(log.timestamp) {
        SimpleDateFormat("hh:mm a", Locale.getDefault()).format(Date(log.timestamp))
    }

    val (icon, iconTint, borderTint) = when (log.eventType) {
        ActivityType.GAME_CREATED.name -> Triple(Icons.Default.AddCircleOutline, palette.accent, palette.tint.copy(alpha = 0.20f))
        ActivityType.GAME_PLAYED.name -> Triple(Icons.Default.EmojiEvents, palette.accent, palette.tint.copy(alpha = 0.18f))
        ActivityType.PAYMENT_CLEARED.name -> Triple(Icons.Default.CheckCircle, palette.numberPositive, palette.numberPositive.copy(alpha = 0.40f))
        ActivityType.GAME_SETTLED.name -> Triple(Icons.Default.Lock, palette.frostAccent, palette.tint.copy(alpha = 0.20f))
        ActivityType.USER_LOGIN.name, ActivityType.APP_OPEN.name -> Triple(Icons.Default.AccountCircle, palette.textPrimary.copy(alpha = 0.85f), palette.tint.copy(alpha = 0.14f))
        else -> Triple(Icons.Default.Info, palette.textPrimary, palette.tint.copy(alpha = 0.16f))
    }

    Surface(
        color = palette.cardSurface.copy(alpha = 0.86f),
        shape = RoundedCornerShape(10.dp),
        border = androidx.compose.foundation.BorderStroke(1.dp, borderTint),
        modifier = Modifier.fillMaxWidth()
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 12.dp, vertical = 9.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(
                imageVector = icon,
                contentDescription = null,
                tint = iconTint,
                modifier = Modifier.size(20.dp)
            )

            Spacer(modifier = Modifier.width(12.dp))

            // Details
            Column(modifier = Modifier.weight(1f)) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = log.title,
                        fontSize = 13.sp,
                        fontWeight = FontWeight.SemiBold,
                        color = palette.textPrimary,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis
                    )
                    Text(
                        text = timeFormatted,
                        fontSize = 10.sp,
                        color = palette.textPrimary.copy(alpha = 0.45f)
                    )
                }

                Spacer(modifier = Modifier.height(2.dp))

                Text(
                    text = log.description,
                    fontSize = 11.sp,
                    color = palette.textPrimary.copy(alpha = 0.75f),
                    maxLines = 2,
                    overflow = TextOverflow.Ellipsis
                )

                // Optional participants pills for game creation
                if (log.eventType == ActivityType.GAME_CREATED.name && !log.metadata.isNullOrBlank()) {
                    val players = log.metadata.split(";").filter { it.isNotBlank() }
                    if (players.isNotEmpty()) {
                        Spacer(modifier = Modifier.height(4.dp))
                        Row(
                            horizontalArrangement = Arrangement.spacedBy(4.dp),
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            players.take(6).forEach { player ->
                                val initials = if (player.length >= 3) player.substring(0, 3).uppercase() else player.uppercase()
                                Surface(
                                    color = palette.surface,
                                    shape = RoundedCornerShape(4.dp)
                                ) {
                                    Text(
                                        text = initials,
                                        fontSize = 9.sp,
                                        fontWeight = FontWeight.Bold,
                                        color = palette.accent,
                                        modifier = Modifier.padding(horizontal = 4.dp, vertical = 1.dp)
                                    )
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

@Composable
private fun EmptyStateView(filter: HistoryFilter) {
    val palette = AppTheme.palette
    Box(
        modifier = Modifier
            .fillMaxSize()
            .padding(32.dp),
        contentAlignment = Alignment.Center
    ) {
        Column(
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Icon(
                imageVector = Icons.Default.History,
                contentDescription = null,
                tint = palette.tint.copy(alpha = 0.25f),
                modifier = Modifier.size(54.dp)
            )
            Spacer(modifier = Modifier.height(12.dp))
            Text(
                text = "No ${filter.label.lowercase()} records found",
                fontSize = 14.sp,
                fontWeight = FontWeight.Medium,
                color = palette.textPrimary.copy(alpha = 0.6f)
            )
            Spacer(modifier = Modifier.height(4.dp))
            Text(
                text = "New game sessions and activities will appear here.",
                fontSize = 12.sp,
                color = palette.textPrimary.copy(alpha = 0.4f)
            )
        }
    }
}
