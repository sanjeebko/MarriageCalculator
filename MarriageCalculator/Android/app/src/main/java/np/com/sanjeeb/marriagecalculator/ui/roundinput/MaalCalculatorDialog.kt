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
 * and auto-sum the player's total Maal points.
 */
@Composable
fun MaalCalculatorDialog(
    playerName: String,
    initialCounts: Map<MaalItem, Int>,
    onApply: (counts: Map<MaalItem, Int>, total: Int) -> Unit,
    onDismiss: () -> Unit
) {
    var counts by remember { mutableStateOf(initialCounts) }
    var values by remember { mutableStateOf(MaalItem.defaultValues()) }
    var editValues by remember { mutableStateOf(false) }
    val total = MaalCalculator.total(counts, values)

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
                    IconButton(onClick = onDismiss) {
                        Icon(Icons.Default.Close, contentDescription = "Close", tint = GoldAccent)
                    }
                }

                Spacer(modifier = Modifier.height(8.dp))

                // Point-value edit toggle
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "Adjust point values",
                        color = Color.White.copy(alpha = 0.6f),
                        fontSize = 12.sp
                    )
                    Switch(
                        checked = editValues,
                        onCheckedChange = { editValues = it },
                        colors = SwitchDefaults.colors(
                            checkedThumbColor = GoldAccent,
                            checkedTrackColor = GoldAccent.copy(alpha = 0.3f)
                        )
                    )
                }

                Spacer(modifier = Modifier.height(8.dp))

                Column(
                    modifier = Modifier
                        .heightIn(max = 360.dp)
                        .verticalScroll(rememberScrollState()),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    MaalItem.entries.forEach { item ->
                        MaalItemRow(
                            item = item,
                            count = counts[item] ?: 0,
                            value = values[item] ?: item.defaultValue,
                            editValues = editValues,
                            onCountChange = { delta ->
                                counts = if (delta > 0) {
                                    MaalCalculator.increment(counts, item)
                                } else {
                                    MaalCalculator.decrement(counts, item)
                                }
                            },
                            onValueChange = { delta ->
                                val current = values[item] ?: item.defaultValue
                                values = values + (item to (current + delta).coerceIn(0, 20))
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

@Composable
private fun MaalItemRow(
    item: MaalItem,
    count: Int,
    value: Int,
    editValues: Boolean,
    onCountChange: (Int) -> Unit,
    onValueChange: (Int) -> Unit
) {
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
            if (editValues) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    StepperButton("−") { onValueChange(-1) }
                    Text(
                        text = "$value pt${if (value == 1) "" else "s"}",
                        color = GoldAccent.copy(alpha = 0.9f),
                        fontSize = 11.sp,
                        modifier = Modifier.padding(horizontal = 6.dp)
                    )
                    StepperButton("+") { onValueChange(1) }
                }
            } else {
                Text(
                    text = "$value pt${if (value == 1) "" else "s"} each",
                    color = Color.White.copy(alpha = 0.4f),
                    fontSize = 11.sp
                )
            }
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
                modifier = Modifier
                    .size(32.dp)
                    .background(GoldAccent.copy(alpha = 0.15f), CircleShape)
            ) {
                Icon(
                    Icons.Default.Add,
                    contentDescription = "Increase ${item.displayName}",
                    tint = GoldAccent,
                    modifier = Modifier.size(16.dp)
                )
            }
        }
    }
}

@Composable
private fun StepperButton(label: String, onClick: () -> Unit) {
    Surface(
        onClick = onClick,
        shape = CircleShape,
        color = Color.White.copy(alpha = 0.1f),
        modifier = Modifier.size(20.dp)
    ) {
        Box(contentAlignment = Alignment.Center) {
            Text(label, color = GoldAccent, fontSize = 12.sp, fontWeight = FontWeight.Bold)
        }
    }
}
