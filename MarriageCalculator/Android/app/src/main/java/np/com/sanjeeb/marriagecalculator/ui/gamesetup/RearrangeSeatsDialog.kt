package np.com.sanjeeb.marriagecalculator.ui.gamesetup

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog
import coil.compose.AsyncImage
import coil.request.ImageRequest
import np.com.sanjeeb.marriagecalculator.data.model.Player
import np.com.sanjeeb.marriagecalculator.data.model.SeatingDraw
import np.com.sanjeeb.marriagecalculator.ui.theme.GoldAccent
import np.com.sanjeeb.marriagecalculator.ui.theme.TiharNightBlue
import np.com.sanjeeb.marriagecalculator.ui.theme.DeepRedTika
import java.io.File
import androidx.compose.foundation.gestures.detectDragGesturesAfterLongPress
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.graphics.graphicsLayer
import androidx.compose.ui.zIndex
import androidx.compose.ui.platform.LocalDensity

@Composable
fun RearrangeSeatsDialog(
    initialPlayers: List<Player>,
    onSave: (List<Player>) -> Unit,
    onDismiss: () -> Unit
) {
    var reorderedList by remember(initialPlayers) { mutableStateOf(initialPlayers) }
    // Cards drawn via "Draw Cards" (requirement §2.2). Cleared when order is changed manually.
    var drawnCards by remember(initialPlayers) { mutableStateOf<Map<String, SeatingDraw.PlayingCard>>(emptyMap()) }
    var draggedIndex by remember { mutableStateOf<Int?>(null) }
    var dragOffset by remember { mutableStateOf(0f) }
    val density = LocalDensity.current
    // Increased height to account for spacing (48dp card + 8dp spacing = 56dp)
    val itemHeightPx = with(density) { 64.dp.toPx() }

    Dialog(onDismissRequest = onDismiss) {
        Card(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp)
                .border(1.dp, GoldAccent, RoundedCornerShape(16.dp)),
            shape = RoundedCornerShape(16.dp),
            colors = CardDefaults.cardColors(containerColor = TiharNightBlue)
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp)
            ) {
                // Header
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Column {
                        Text(
                            text = "Arrange Seats",
                            color = GoldAccent,
                            fontFamily = FontFamily.Serif,
                            fontWeight = FontWeight.Bold,
                            fontSize = 18.sp
                        )
                        Text(
                            text = "Order determines dealing direction",
                            color = Color.White.copy(alpha = 0.5f),
                            fontSize = 11.sp
                        )
                    }
                    IconButton(onClick = onDismiss) {
                        Icon(Icons.Default.Close, contentDescription = "Close", tint = GoldAccent)
                    }
                }

                Spacer(modifier = Modifier.height(16.dp))

                // Players list with drag-to-reorder
                LazyColumn(
                    modifier = Modifier
                        .fillMaxWidth()
                        .heightIn(max = 400.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    itemsIndexed(reorderedList, key = { _, player -> player.id }) { index, player ->
                        val isDragged = draggedIndex == index
                        val translationY = if (isDragged) dragOffset else 0f
                        
                        // Use updated state to prevent capturing stale index in pointerInput
                        val currentIndex by rememberUpdatedState(index)

                        Card(
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(56.dp) // Fixed height for consistent dragging math
                                .zIndex(if (isDragged) 1f else 0f)
                                .graphicsLayer {
                                    this.translationY = translationY
                                    if (isDragged) {
                                        scaleX = 1.04f
                                        scaleY = 1.04f
                                        alpha = 0.9f
                                    }
                                }
                                .pointerInput(player.id) {
                                    detectDragGesturesAfterLongPress(
                                        onDragStart = {
                                            draggedIndex = currentIndex
                                            dragOffset = 0f
                                            drawnCards = emptyMap() // manual order overrides the draw
                                        },
                                        onDrag = { change, dragAmount ->
                                            change.consume()
                                            dragOffset += dragAmount.y
                                            
                                            val currentDragged = draggedIndex
                                            if (currentDragged != null) {
                                                val swapIndex = when {
                                                    dragOffset > itemHeightPx / 2 -> currentDragged + 1
                                                    dragOffset < -itemHeightPx / 2 -> currentDragged - 1
                                                    else -> null
                                                }
                                                
                                                if (swapIndex != null && swapIndex in reorderedList.indices) {
                                                    val newList = reorderedList.toMutableList()
                                                    val item = newList.removeAt(currentDragged)
                                                    newList.add(swapIndex, item)
                                                    reorderedList = newList
                                                    
                                                    // Offset logic: adjust for the swap
                                                    val direction = swapIndex - currentDragged
                                                    dragOffset -= direction * itemHeightPx
                                                    draggedIndex = swapIndex
                                                }
                                            }
                                        },
                                        onDragEnd = {
                                            draggedIndex = null
                                            dragOffset = 0f
                                        },
                                        onDragCancel = {
                                            draggedIndex = null
                                            dragOffset = 0f
                                        }
                                    )
                                },
                            shape = RoundedCornerShape(12.dp),
                            colors = CardDefaults.cardColors(
                                containerColor = if (isDragged) Color.White.copy(alpha = 0.2f) else Color.White.copy(alpha = 0.08f)
                            ),
                            border = if (isDragged) BorderStroke(1.dp, GoldAccent) else BorderStroke(0.5.dp, Color.White.copy(alpha = 0.1f))
                        ) {
                            Row(
                                modifier = Modifier
                                    .fillMaxSize()
                                    .padding(horizontal = 12.dp),
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                // Seat Index Indicator
                                Surface(
                                    modifier = Modifier.size(28.dp),
                                    shape = CircleShape,
                                    color = if (isDragged) GoldAccent else GoldAccent.copy(alpha = 0.15f),
                                    border = BorderStroke(1.dp, GoldAccent.copy(alpha = 0.5f))
                                ) {
                                    Box(contentAlignment = Alignment.Center) {
                                        Text(
                                            text = (index + 1).toString(),
                                            color = if (isDragged) Color.Black else GoldAccent,
                                            fontSize = 12.sp,
                                            fontWeight = FontWeight.ExtraBold
                                        )
                                    }
                                }

                                Spacer(modifier = Modifier.width(12.dp))

                                // Avatar
                                val uri = player.photoUri
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
                                        modifier = Modifier
                                            .size(36.dp)
                                            .clip(CircleShape)
                                            .border(1.dp, Color.White.copy(alpha = 0.2f), CircleShape)
                                    )
                                } else {
                                    Box(
                                        modifier = Modifier
                                            .size(36.dp)
                                            .clip(CircleShape)
                                            .background(Color.White.copy(alpha = 0.1f))
                                            .border(1.dp, Color.White.copy(alpha = 0.2f), CircleShape),
                                        contentAlignment = Alignment.Center
                                    ) {
                                        Text(
                                            text = player.name.take(1).uppercase(),
                                            color = Color.White,
                                            fontWeight = FontWeight.Bold,
                                            fontSize = 16.sp
                                        )
                                    }
                                }

                                Spacer(modifier = Modifier.width(12.dp))

                                // Name
                                Text(
                                    text = player.name,
                                    color = Color.White,
                                    fontWeight = FontWeight.ExtraBold,
                                    fontSize = 15.sp,
                                    modifier = Modifier.weight(1f),
                                    maxLines = 1,
                                    overflow = TextOverflow.Ellipsis
                                )

                                // Drawn card badge (from "Draw Cards")
                                drawnCards[player.id]?.let { card ->
                                    val isRed = card.suit == SeatingDraw.Suit.HEARTS || card.suit == SeatingDraw.Suit.DIAMONDS
                                    Surface(
                                        color = Color.White.copy(alpha = 0.92f),
                                        shape = RoundedCornerShape(4.dp),
                                        border = BorderStroke(1.dp, GoldAccent.copy(alpha = 0.6f))
                                    ) {
                                        Text(
                                            text = card.label,
                                            color = if (isRed) DeepRedTika else Color.Black,
                                            fontSize = 11.sp,
                                            fontWeight = FontWeight.Black,
                                            modifier = Modifier.padding(horizontal = 5.dp, vertical = 2.dp)
                                        )
                                    }
                                    Spacer(modifier = Modifier.width(8.dp))
                                }

                                if (index == reorderedList.size - 1) {
                                    Surface(
                                        color = DeepRedTika.copy(alpha = 0.4f),
                                        shape = RoundedCornerShape(4.dp),
                                        border = BorderStroke(1.dp, DeepRedTika.copy(alpha = 0.6f))
                                    ) {
                                        Text(
                                            text = "DEALER",
                                            color = GoldAccent,
                                            fontSize = 9.sp,
                                            fontWeight = FontWeight.Black,
                                            modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp)
                                        )
                                    }
                                    Spacer(modifier = Modifier.width(8.dp))
                                }

                                // Drag Handle Icon
                                Icon(
                                    imageVector = Icons.Default.DragHandle,
                                    contentDescription = "Drag to reorder",
                                    tint = if (isDragged) GoldAccent else GoldAccent.copy(alpha = 0.5f),
                                    modifier = Modifier.size(24.dp)
                                )
                            }
                        }
                    }
                }

                Spacer(modifier = Modifier.height(16.dp))

                // Draw cards for seats (requirement §2.2: highest card = 1st seat, lowest deals first)
                OutlinedButton(
                    onClick = {
                        val result = SeatingDraw.draw(reorderedList)
                        reorderedList = result.seating
                        drawnCards = result.cards
                    },
                    modifier = Modifier
                        .fillMaxWidth()
                        .heightIn(min = 48.dp),
                    colors = ButtonDefaults.outlinedButtonColors(contentColor = GoldAccent),
                    border = BorderStroke(1.dp, GoldAccent.copy(alpha = 0.5f)),
                    shape = RoundedCornerShape(8.dp)
                ) {
                    Icon(Icons.Default.Style, contentDescription = null, modifier = Modifier.size(16.dp))
                    Spacer(modifier = Modifier.width(6.dp))
                    Text(
                        text = "Draw Cards for Seats",
                        fontSize = 12.sp,
                        textAlign = TextAlign.Center
                    )
                }

                Spacer(modifier = Modifier.height(8.dp))

                // Bottom buttons
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    // Shuffle
                    OutlinedButton(
                        onClick = {
                            reorderedList = reorderedList.shuffled()
                            drawnCards = emptyMap()
                        },
                        modifier = Modifier
                            .weight(1f)
                            .heightIn(min = 52.dp),
                        colors = ButtonDefaults.outlinedButtonColors(contentColor = GoldAccent),
                        border = BorderStroke(1.dp, GoldAccent.copy(alpha = 0.5f)),
                        shape = RoundedCornerShape(8.dp),
                        contentPadding = PaddingValues(horizontal = 4.dp, vertical = 8.dp)
                    ) {
                        Icon(Icons.Default.Casino, contentDescription = null, modifier = Modifier.size(16.dp))
                        Spacer(modifier = Modifier.width(4.dp))
                        Text(
                            text = "Shuffle",
                            fontSize = 12.sp,
                            lineHeight = 14.sp,
                            textAlign = TextAlign.Center
                        )
                    }

                    // Save
                    Button(
                        onClick = {
                            onSave(reorderedList)
                            onDismiss()
                        },
                        modifier = Modifier
                            .weight(1f)
                            .heightIn(min = 52.dp),
                        colors = ButtonDefaults.buttonColors(containerColor = DeepRedTika, contentColor = GoldAccent),
                        shape = RoundedCornerShape(8.dp),
                        contentPadding = PaddingValues(horizontal = 4.dp, vertical = 8.dp)
                    ) {
                        Icon(Icons.Default.Save, contentDescription = null, modifier = Modifier.size(16.dp))
                        Spacer(modifier = Modifier.width(4.dp))
                        Text(
                            text = "Save Seats",
                            fontSize = 12.sp,
                            lineHeight = 14.sp,
                            textAlign = TextAlign.Center
                        )
                    }
                }
            }
        }
    }
}
