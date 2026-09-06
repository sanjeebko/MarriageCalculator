package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.animation.animateColorAsState
import androidx.compose.animation.core.RepeatMode
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.infiniteRepeatable
import androidx.compose.animation.core.rememberInfiniteTransition
import androidx.compose.animation.core.tween
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CloudDone
import androidx.compose.material.icons.filled.CloudOff
import androidx.compose.material.icons.filled.CloudUpload
import androidx.compose.material.icons.filled.Sync
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.graphicsLayer
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.ViewModel
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.StateFlow
import np.com.sanjeeb.marriagecalculator.data.sync.SyncManager
import np.com.sanjeeb.marriagecalculator.data.sync.SyncStatus
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme
import javax.inject.Inject

@HiltViewModel
class SyncViewModel @Inject constructor(
    private val syncManager: SyncManager
) : ViewModel() {
    val syncStatus: StateFlow<SyncStatus> = syncManager.syncStatus
    fun triggerSync() = syncManager.triggerSync()
}

@Composable
fun SyncStatusIndicator(
    modifier: Modifier = Modifier,
    viewModel: SyncViewModel = hiltViewModel()
) {
    val syncStatus by viewModel.syncStatus.collectAsState()
    var showDialog by remember { mutableStateOf(false) }

    // Color animation
    val targetColor = when (syncStatus) {
        is SyncStatus.Synced -> Color(0xFF00E676) // Bright vibrant emerald green
        is SyncStatus.Syncing -> AppTheme.palette.accent
        else -> Color(0xFF9E9E9E) // Gray for offline or pending local-only sync
    }
    val animatedColor by animateColorAsState(targetValue = targetColor, label = "syncColor")

    // Rotation for syncing
    val infiniteTransition = rememberInfiniteTransition(label = "syncSpin")
    val rotation by infiniteTransition.animateFloat(
        initialValue = 0f,
        targetValue = 360f,
        animationSpec = infiniteRepeatable(
            animation = tween(durationMillis = 1000),
            repeatMode = RepeatMode.Restart
        ),
        label = "syncRotation"
    )

    Box(
        modifier = modifier
            .clip(CircleShape)
            .clickable { showDialog = true }
            .padding(6.dp),
        contentAlignment = Alignment.Center
    ) {
        val icon = when (syncStatus) {
            is SyncStatus.Synced -> Icons.Default.CloudDone
            is SyncStatus.Syncing -> Icons.Default.Sync
            is SyncStatus.PendingSync -> Icons.Default.CloudUpload
            is SyncStatus.Offline -> Icons.Default.CloudOff
        }

        Icon(
            imageVector = icon,
            contentDescription = when (syncStatus) {
                is SyncStatus.Synced -> "Synced with online database"
                is SyncStatus.Syncing -> "Syncing data..."
                is SyncStatus.PendingSync -> "Pending sync"
                is SyncStatus.Offline -> "Offline (local storage)"
            },
            tint = animatedColor,
            modifier = Modifier
                .size(24.dp)
                .graphicsLayer {
                    if (syncStatus is SyncStatus.Syncing) {
                        rotationZ = rotation
                    }
                }
        )
    }

    if (showDialog) {
        SyncStatusDetailDialog(
            status = syncStatus,
            onDismiss = { showDialog = false },
            onSyncNow = {
                viewModel.triggerSync()
                showDialog = false
            }
        )
    }
}

@Composable
fun SyncStatusDetailDialog(
    status: SyncStatus,
    onDismiss: () -> Unit,
    onSyncNow: () -> Unit
) {
    Dialog(onDismissRequest = onDismiss) {
        Surface(
            shape = RoundedCornerShape(20.dp),
            color = AppTheme.palette.surface,
            border = androidx.compose.foundation.BorderStroke(
                1.dp,
                Brush.linearGradient(
                    listOf(
                        AppTheme.palette.accent.copy(alpha = 0.5f),
                        AppTheme.palette.accentAlt.copy(alpha = 0.2f)
                    )
                )
            ),
            shadowElevation = 8.dp,
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp)
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(24.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                val iconColor = when (status) {
                    is SyncStatus.Synced -> Color(0xFF00E676)
                    is SyncStatus.Syncing -> AppTheme.palette.accent
                    else -> Color(0xFF9E9E9E)
                }

                Box(
                    modifier = Modifier
                        .size(56.dp)
                        .clip(CircleShape)
                        .background(iconColor.copy(alpha = 0.15f))
                        .border(1.dp, iconColor.copy(alpha = 0.4f), CircleShape),
                    contentAlignment = Alignment.Center
                ) {
                    Icon(
                        imageVector = when (status) {
                            is SyncStatus.Synced -> Icons.Default.CloudDone
                            is SyncStatus.Syncing -> Icons.Default.Sync
                            is SyncStatus.PendingSync -> Icons.Default.CloudUpload
                            is SyncStatus.Offline -> Icons.Default.CloudOff
                        },
                        contentDescription = null,
                        tint = iconColor,
                        modifier = Modifier.size(32.dp)
                    )
                }

                Spacer(modifier = Modifier.height(16.dp))

                Text(
                    text = when (status) {
                        is SyncStatus.Synced -> "Online & Synced"
                        is SyncStatus.Syncing -> "Syncing with Cloud..."
                        is SyncStatus.PendingSync -> "Local Only (Pending Sync)"
                        is SyncStatus.Offline -> "Offline Mode"
                    },
                    fontSize = 18.sp,
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Serif,
                    color = iconColor
                )

                Spacer(modifier = Modifier.height(8.dp))

                Text(
                    text = when (status) {
                        is SyncStatus.Synced ->
                            "All scores and game sets are synchronized with the online database. Your data is safely backed up."
                        is SyncStatus.Syncing ->
                            "Currently uploading offline game records to the cloud database..."
                        is SyncStatus.PendingSync ->
                            "${(status as SyncStatus.PendingSync).pendingCount} record(s) written to local database only. They will automatically sync when connected to the server."
                        is SyncStatus.Offline ->
                            "Internet is currently not available. Scores are written safely to the local database and will automatically sync once online."
                    },
                    fontSize = 13.sp,
                    color = AppTheme.palette.textPrimary.copy(alpha = 0.75f),
                    textAlign = androidx.compose.ui.text.style.TextAlign.Center,
                    lineHeight = 18.sp
                )

                Spacer(modifier = Modifier.height(20.dp))

                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.End
                ) {
                    if (status !is SyncStatus.Synced && status !is SyncStatus.Syncing) {
                        OutlinedButton(
                            onClick = onSyncNow,
                            colors = ButtonDefaults.outlinedButtonColors(
                                contentColor = AppTheme.palette.accent
                            ),
                            border = androidx.compose.foundation.BorderStroke(1.dp, AppTheme.palette.accent.copy(alpha = 0.5f)),
                            shape = RoundedCornerShape(10.dp)
                        ) {
                            Text("Sync Now", fontSize = 13.sp)
                        }
                        Spacer(modifier = Modifier.width(8.dp))
                    }

                    Button(
                        onClick = onDismiss,
                        colors = ButtonDefaults.buttonColors(
                            containerColor = AppTheme.palette.accent,
                            contentColor = AppTheme.palette.surface
                        ),
                        shape = RoundedCornerShape(10.dp)
                    ) {
                        Text("OK", fontSize = 13.sp, fontWeight = FontWeight.SemiBold)
                    }
                }
            }
        }
    }
}
