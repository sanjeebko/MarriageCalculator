package np.com.sanjeeb.marriagecalculator.ui.components

import android.widget.Toast
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.AccountBalance
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.ContentCopy
import androidx.compose.material.icons.filled.Payments
import androidx.compose.material.icons.filled.Share
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalClipboardManager
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.AnnotatedString
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.window.Dialog
import androidx.compose.ui.window.DialogProperties
import np.com.sanjeeb.marriagecalculator.data.model.Currency
import np.com.sanjeeb.marriagecalculator.ui.share.MatchShareHelper
import np.com.sanjeeb.marriagecalculator.ui.share.SettlementTransfer
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme

/**
 * 1-Tap 'Settle Up' Cash Settlement Matrix (Issue #30):
 * Calculates the minimal peer-to-peer cash/eSewa/bank transfers required to settle the game
 * and provides 1-tap copy/share and optional Settle & Freeze.
 */
@Composable
fun SettleUpDialog(
    matchName: String,
    balances: List<Pair<String, Double>>,
    currency: Currency,
    isSettled: Boolean = false,
    canSettleAndFreeze: Boolean = false,
    onSettleAndFreeze: (() -> Unit)? = null,
    onDismiss: () -> Unit
) {
    val context = LocalContext.current
    val clipboardManager = LocalClipboardManager.current
    val settlements: List<SettlementTransfer> = MatchShareHelper.computeSettlements(balances)
    val totalTransferred = settlements.sumOf { it.amount }

    Dialog(
        onDismissRequest = onDismiss,
        properties = DialogProperties(usePlatformDefaultWidth = false)
    ) {
        Card(
            modifier = Modifier
                .fillMaxWidth(0.92f)
                .padding(vertical = 24.dp),
            shape = RoundedCornerShape(18.dp),
            colors = CardDefaults.cardColors(containerColor = AppTheme.palette.cardSurface),
            border = BorderStroke(1.5.dp, AppTheme.palette.accent.copy(alpha = 0.6f))
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(20.dp)
            ) {
                // Header
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Box(
                            modifier = Modifier
                                .size(36.dp)
                                .clip(CircleShape)
                                .background(AppTheme.palette.accent.copy(alpha = 0.15f)),
                            contentAlignment = Alignment.Center
                        ) {
                            Icon(
                                imageVector = Icons.Default.Payments,
                                contentDescription = null,
                                tint = AppTheme.palette.accent,
                                modifier = Modifier.size(20.dp)
                            )
                        }
                        Spacer(modifier = Modifier.width(10.dp))
                        Column {
                            Text(
                                text = "Settle Up Matrix",
                                color = AppTheme.palette.accent,
                                fontSize = 18.sp,
                                fontFamily = FontFamily.Serif,
                                fontWeight = FontWeight.Bold
                            )
                            Text(
                                text = "Minimum cash & digital transfers",
                                color = AppTheme.palette.tint.copy(alpha = 0.6f),
                                fontSize = 11.sp
                            )
                        }
                    }
                    IconButton(onClick = onDismiss, modifier = Modifier.size(30.dp)) {
                        Icon(Icons.Default.Close, contentDescription = "Close", tint = AppTheme.palette.tint)
                    }
                }

                Spacer(modifier = Modifier.height(14.dp))

                // Total Settling Volume Chip
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .clip(RoundedCornerShape(8.dp))
                        .background(AppTheme.palette.tint.copy(alpha = 0.05f))
                        .border(1.dp, AppTheme.palette.tint.copy(alpha = 0.1f), RoundedCornerShape(8.dp))
                        .padding(horizontal = 12.dp, vertical = 8.dp),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "Total Settlement Volume",
                        color = AppTheme.palette.tint.copy(alpha = 0.7f),
                        fontSize = 12.sp,
                        fontWeight = FontWeight.Medium
                    )
                    Text(
                        text = currency.formatMoney(totalTransferred),
                        color = AppTheme.palette.accent,
                        fontSize = 14.sp,
                        fontWeight = FontWeight.Bold
                    )
                }

                Spacer(modifier = Modifier.height(12.dp))

                // Scrollable List of Direct Transfers
                Column(
                    modifier = Modifier
                        .weight(1f, fill = false)
                        .heightIn(max = 340.dp)
                        .verticalScroll(rememberScrollState())
                ) {
                    if (settlements.isEmpty()) {
                        Box(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(vertical = 24.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            Text(
                                text = "All balances are settled! 🎉\nNo transfers required.",
                                color = AppTheme.palette.tint.copy(alpha = 0.6f),
                                textAlign = TextAlign.Center,
                                fontSize = 13.sp
                            )
                        }
                    } else {
                        settlements.forEachIndexed { index, transfer ->
                            Card(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(vertical = 4.dp),
                                shape = RoundedCornerShape(10.dp),
                                colors = CardDefaults.cardColors(containerColor = AppTheme.palette.tint.copy(alpha = 0.06f)),
                                border = BorderStroke(1.dp, AppTheme.palette.tint.copy(alpha = 0.12f))
                            ) {
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(horizontal = 12.dp, vertical = 10.dp),
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.SpaceBetween
                                ) {
                                    // Payer -> Recipient Flow
                                    Column(modifier = Modifier.weight(1f)) {
                                        Row(verticalAlignment = Alignment.CenterVertically) {
                                            Text(
                                                text = transfer.fromPlayer,
                                                color = Color(0xFFFCA5A5), // Soft red/coral for payer
                                                fontWeight = FontWeight.Bold,
                                                fontSize = 14.sp
                                            )
                                            Text(
                                                text = " ➔ pays ➔ ",
                                                color = AppTheme.palette.tint.copy(alpha = 0.5f),
                                                fontSize = 11.sp
                                            )
                                            Text(
                                                text = transfer.toPlayer,
                                                color = Color(0xFF86EFAC), // Soft green for receiver
                                                fontWeight = FontWeight.Bold,
                                                fontSize = 14.sp
                                            )
                                        }
                                        Spacer(modifier = Modifier.height(2.dp))
                                        Text(
                                            text = "eSewa / Khalti / Cash / Bank",
                                            color = AppTheme.palette.tint.copy(alpha = 0.4f),
                                            fontSize = 9.sp
                                        )
                                    }

                                    // Amount
                                    Text(
                                        text = currency.formatMoney(transfer.amount),
                                        color = AppTheme.palette.accent,
                                        fontSize = 15.sp,
                                        fontWeight = FontWeight.Bold
                                    )
                                }
                            }
                        }
                    }
                }

                Spacer(modifier = Modifier.height(14.dp))

                // Action Buttons
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    // Copy to Clipboard Button
                    OutlinedButton(
                        onClick = {
                            val text = buildSettlementCopyText(matchName, settlements, currency)
                            clipboardManager.setText(AnnotatedString(text))
                            Toast.makeText(context, "Settlement copied to clipboard!", Toast.LENGTH_SHORT).show()
                        },
                        modifier = Modifier.weight(1f),
                        shape = RoundedCornerShape(10.dp),
                        colors = ButtonDefaults.outlinedButtonColors(contentColor = AppTheme.palette.accent),
                        border = BorderStroke(1.dp, AppTheme.palette.accent.copy(alpha = 0.5f))
                    ) {
                        Icon(Icons.Default.ContentCopy, null, modifier = Modifier.size(16.dp))
                        Spacer(modifier = Modifier.width(6.dp))
                        Text("Copy", fontSize = 12.sp, fontWeight = FontWeight.Bold)
                    }

                    // Settle & Freeze (if game is still active and caller is host)
                    if (canSettleAndFreeze && !isSettled && onSettleAndFreeze != null) {
                        Button(
                            onClick = {
                                onSettleAndFreeze()
                                onDismiss()
                            },
                            modifier = Modifier.weight(1f),
                            shape = RoundedCornerShape(10.dp),
                            colors = ButtonDefaults.buttonColors(
                                containerColor = AppTheme.palette.cta,
                                contentColor = AppTheme.palette.accent
                            )
                        ) {
                            Icon(Icons.Default.AccountBalance, null, modifier = Modifier.size(16.dp))
                            Spacer(modifier = Modifier.width(6.dp))
                            Text("Freeze Game", fontSize = 12.sp, fontWeight = FontWeight.Bold)
                        }
                    }
                }
            }
        }
    }
}

private fun buildSettlementCopyText(
    matchName: String,
    settlements: List<SettlementTransfer>,
    currency: Currency
): String {
    val sb = StringBuilder()
    sb.appendLine("💳 *Marriage Calculator - Settle Up Matrix*")
    sb.appendLine("Match: $matchName")
    sb.appendLine()
    if (settlements.isEmpty()) {
        sb.appendLine("All balances are even! No transfers needed.")
    } else {
        sb.appendLine("Recommended transfers (minimum peer-to-peer):")
        settlements.forEach { t ->
            sb.appendLine("• *${t.fromPlayer}* pays *${t.toPlayer}*: ${currency.formatMoney(t.amount)}")
        }
    }
    sb.appendLine()
    sb.appendLine("Settled via eSewa / Cash / Bank transfer ✨")
    return sb.toString()
}
