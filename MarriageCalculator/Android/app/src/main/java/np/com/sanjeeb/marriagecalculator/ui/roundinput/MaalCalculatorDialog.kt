package np.com.sanjeeb.marriagecalculator.ui.roundinput

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Remove
import androidx.compose.material.icons.outlined.Info
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog
import np.com.sanjeeb.marriagecalculator.data.model.MaalCalculator
import np.com.sanjeeb.marriagecalculator.data.model.MaalItem
import np.com.sanjeeb.marriagecalculator.ui.theme.DeepRedTika
import np.com.sanjeeb.marriagecalculator.ui.theme.GoldAccent
import np.com.sanjeeb.marriagecalculator.ui.theme.TiharNightBlue

/**
 * Advanced Maal calculator (requirement §3.2): count maal cards per type
 * and auto-sum the player's total Maal points. Point values are fixed
 * game rules (tiered by count) and each item's stepper stops at the
 * number of cards that can physically exist in a 3-deck game.
 */
@Composable
fun MaalCalculatorDialog(
    playerName: String,
    initialCounts: Map<MaalItem, Int>,
    onApply: (counts: Map<MaalItem, Int>, total: Int) -> Unit,
    onDismiss: () -> Unit
) {
    var counts by remember { mutableStateOf(initialCounts) }
    var showPointsTable by remember { mutableStateOf(false) }
    val total = MaalCalculator.total(counts)

    if (showPointsTable) {
        MaalPointsTableDialog(onDismiss = { showPointsTable = false })
    }

    Dialog(onDismissRequest = onDismiss) {
        Card(
            modifier = Modifier
                .fillMaxWidth()
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
                            text = "Maal Calculator",
                            color = GoldAccent,
                            fontFamily = FontFamily.Serif,
                            fontWeight = FontWeight.Bold,
                            fontSize = 18.sp
                        )
                        Text(
                            text = playerName,
                            color = Color.White.copy(alpha = 0.5f),
                            fontSize = 11.sp
                        )
                    }
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        IconButton(onClick = { showPointsTable = true }) {
                            Icon(
                                Icons.Outlined.Info,
                                contentDescription = "Show maal points table",
                                tint = GoldAccent.copy(alpha = 0.8f)
                            )
                        }
                        IconButton(onClick = onDismiss) {
                            Icon(Icons.Default.Close, contentDescription = "Close", tint = GoldAccent)
                        }
                    }
                }

                Spacer(modifier = Modifier.height(8.dp))

                Column(
                    modifier = Modifier
                        .heightIn(max = 400.dp)
                        .verticalScroll(rememberScrollState()),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    MaalItem.entries.forEach { item ->
                        MaalItemRow(
                            item = item,
                            count = counts[item] ?: 0,
                            onCountChange = { delta ->
                                counts = if (delta > 0) {
                                    MaalCalculator.increment(counts, item)
                                } else {
                                    MaalCalculator.decrement(counts, item)
                                }
                            }
                        )
                    }
                }

                Spacer(modifier = Modifier.height(12.dp))

                // Total display
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .background(GoldAccent.copy(alpha = 0.12f), RoundedCornerShape(8.dp))
                        .border(1.dp, GoldAccent.copy(alpha = 0.4f), RoundedCornerShape(8.dp))
                        .padding(horizontal = 16.dp, vertical = 10.dp),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "Total Maal",
                        color = Color.White.copy(alpha = 0.8f),
                        fontSize = 14.sp
                    )
                    Text(
                        text = total.toString(),
                        color = GoldAccent,
                        fontWeight = FontWeight.Bold,
                        fontSize = 20.sp
                    )
                }

                Spacer(modifier = Modifier.height(12.dp))

                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    OutlinedButton(
                        onClick = { counts = emptyMap() },
                        modifier = Modifier.weight(1f).heightIn(min = 48.dp),
                        colors = ButtonDefaults.outlinedButtonColors(contentColor = GoldAccent),
                        border = BorderStroke(1.dp, GoldAccent.copy(alpha = 0.5f)),
                        shape = RoundedCornerShape(8.dp)
                    ) {
                        Text("Reset", fontSize = 13.sp)
                    }
                    Button(
                        onClick = {
                            onApply(counts, total)
                            onDismiss()
                        },
                        modifier = Modifier.weight(1f).heightIn(min = 48.dp),
                        colors = ButtonDefaults.buttonColors(containerColor = DeepRedTika, contentColor = GoldAccent),
                        shape = RoundedCornerShape(8.dp)
                    ) {
                        Text("Apply", fontSize = 13.sp, fontWeight = FontWeight.Bold)
                    }
                }
            }
        }
    }
}

/** e.g. "1 = 3 · 2 = 8 pts", or just "35 pts" for single-tier items. */
private fun tierLabel(item: MaalItem): String =
    if (item.maxCount == 1) "${item.tiers[0]} pts"
    else item.tiers.mapIndexed { i, pts -> "${i + 1} = $pts" }.joinToString(" · ") + " pts"

/** Reference popup: full maal-vs-points table (rows = items, columns = count held). */
@Composable
private fun MaalPointsTableDialog(onDismiss: () -> Unit) {
    val maxColumns = MaalItem.entries.maxOf { it.maxCount }
    val countColumnWidth = 44.dp

    Dialog(onDismissRequest = onDismiss) {
        Card(
            modifier = Modifier
                .fillMaxWidth()
                .border(1.dp, GoldAccent, RoundedCornerShape(16.dp)),
            shape = RoundedCornerShape(16.dp),
            colors = CardDefaults.cardColors(containerColor = TiharNightBlue)
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp)
            ) {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "Maal Points",
                        color = GoldAccent,
                        fontFamily = FontFamily.Serif,
                        fontWeight = FontWeight.Bold,
                        fontSize = 18.sp
                    )
                    IconButton(onClick = onDismiss) {
                        Icon(Icons.Default.Close, contentDescription = "Close", tint = GoldAccent)
                    }
                }

                Spacer(modifier = Modifier.height(4.dp))

                // Header: MAAL | 1 | 2 | 3 | 4 (count held)
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(horizontal = 8.dp, vertical = 6.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "MAAL",
                        color = GoldAccent.copy(alpha = 0.8f),
                        fontSize = 11.sp,
                        fontWeight = FontWeight.Bold,
                        modifier = Modifier.weight(1f)
                    )
                    (1..maxColumns).forEach { n ->
                        Text(
                            text = "×$n",
                            color = GoldAccent.copy(alpha = 0.8f),
                            fontSize = 11.sp,
                            fontWeight = FontWeight.Bold,
                            modifier = Modifier.width(countColumnWidth),
                            textAlign = androidx.compose.ui.text.style.TextAlign.Center
                        )
                    }
                }

                Column(
                    modifier = Modifier
                        .heightIn(max = 480.dp)
                        .verticalScroll(rememberScrollState())
                ) {
                    MaalItem.entries.forEachIndexed { index, item ->
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .background(
                                    if (index % 2 == 0) Color.White.copy(alpha = 0.05f) else Color.Transparent,
                                    RoundedCornerShape(6.dp)
                                )
                                .padding(horizontal = 8.dp, vertical = 8.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            // Short name (strip the parenthetical description to keep rows on one line)
                            Text(
                                text = item.displayName.substringBefore(" ("),
                                color = Color.White,
                                fontSize = 12.sp,
                                fontWeight = FontWeight.SemiBold,
                                modifier = Modifier.weight(1f)
                            )
                            (1..maxColumns).forEach { n ->
                                Text(
                                    text = if (n <= item.maxCount) item.points(n).toString() else "–",
                                    color = if (n <= item.maxCount) GoldAccent else Color.White.copy(alpha = 0.25f),
                                    fontSize = 12.sp,
                                    fontWeight = if (n <= item.maxCount) FontWeight.Bold else FontWeight.Normal,
                                    modifier = Modifier.width(countColumnWidth),
                                    textAlign = androidx.compose.ui.text.style.TextAlign.Center
                                )
                            }
                        }
                    }
                }

                Spacer(modifier = Modifier.height(8.dp))

                Text(
                    text = "Points are totals for the count held, not per card.",
                    color = Color.White.copy(alpha = 0.45f),
                    fontSize = 10.sp
                )
            }
        }
    }
}

@Composable
private fun MaalItemRow(
    item: MaalItem,
    count: Int,
    onCountChange: (Int) -> Unit
) {
    val atMax = count >= item.maxCount
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .background(Color.White.copy(alpha = 0.06f), RoundedCornerShape(10.dp))
            .padding(horizontal = 12.dp, vertical = 8.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Column(modifier = Modifier.weight(1f)) {
            Text(
                text = item.displayName,
                color = Color.White,
                fontSize = 13.sp,
                fontWeight = FontWeight.SemiBold
            )
            Text(
                text = tierLabel(item),
                color = Color.White.copy(alpha = 0.4f),
                fontSize = 11.sp
            )
        }

        // Count stepper
        Row(verticalAlignment = Alignment.CenterVertically) {
            IconButton(
                onClick = { onCountChange(-1) },
                enabled = count > 0,
                modifier = Modifier
                    .size(32.dp)
                    .background(Color.White.copy(alpha = 0.08f), CircleShape)
            ) {
                Icon(
                    Icons.Default.Remove,
                    contentDescription = "Decrease ${item.displayName}",
                    tint = if (count > 0) GoldAccent else Color.White.copy(alpha = 0.2f),
                    modifier = Modifier.size(16.dp)
                )
            }
            Text(
                text = count.toString(),
                color = if (count > 0) GoldAccent else Color.White.copy(alpha = 0.4f),
                fontWeight = FontWeight.Bold,
                fontSize = 16.sp,
                modifier = Modifier.widthIn(min = 32.dp),
                textAlign = androidx.compose.ui.text.style.TextAlign.Center
            )
            IconButton(
                onClick = { onCountChange(1) },
                enabled = !atMax,
                modifier = Modifier
                    .size(32.dp)
                    .background(
                        if (atMax) Color.White.copy(alpha = 0.08f) else GoldAccent.copy(alpha = 0.15f),
                        CircleShape
                    )
            ) {
                Icon(
                    Icons.Default.Add,
                    contentDescription = "Increase ${item.displayName}",
                    tint = if (atMax) Color.White.copy(alpha = 0.2f) else GoldAccent,
                    modifier = Modifier.size(16.dp)
                )
            }
        }
    }
}
