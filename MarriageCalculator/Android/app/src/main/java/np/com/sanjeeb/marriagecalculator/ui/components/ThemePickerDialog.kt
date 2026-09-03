package np.com.sanjeeb.marriagecalculator.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import np.com.sanjeeb.marriagecalculator.ui.theme.AppTheme
import np.com.sanjeeb.marriagecalculator.ui.theme.AppThemeOption

/**
 * Reusable modal dialog for choosing the app-wide color theme.
 * Displays swatches and lets the user pick any dark or light theme option.
 */
@Composable
fun ThemePickerDialog(
    current: AppThemeOption,
    onSelect: (AppThemeOption) -> Unit,
    onDismiss: () -> Unit
) {
    val pal = AppTheme.palette
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("App Theme", color = pal.accent, fontFamily = FontFamily.Serif, fontWeight = FontWeight.Bold) },
        text = {
            Column {
                listOf(
                    "Dark" to AppThemeOption.entries.filter { it.palette.isDark },
                    "Light" to AppThemeOption.entries.filter { !it.palette.isDark }
                ).forEach { (label, options) ->
                    Text(
                        text = label.uppercase(),
                        color = pal.textPrimary.copy(alpha = 0.5f),
                        fontSize = 11.sp,
                        fontWeight = FontWeight.Bold,
                        letterSpacing = 1.5.sp,
                        modifier = Modifier.padding(top = 8.dp, bottom = 4.dp)
                    )
                    options.forEach { option ->
                        val selected = option == current
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .clip(RoundedCornerShape(10.dp))
                                .background(if (selected) pal.tint.copy(alpha = 0.12f) else Color.Transparent)
                                .border(
                                    1.dp,
                                    if (selected) pal.accent.copy(alpha = 0.6f) else pal.tint.copy(alpha = 0.12f),
                                    RoundedCornerShape(10.dp)
                                )
                                .clickable { onSelect(option) }
                                .padding(horizontal = 10.dp, vertical = 8.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            // Mini palette preview: background, accent, cta swatches
                            Row {
                                listOf(
                                    option.palette.backgroundTop,
                                    option.palette.accent,
                                    option.palette.cta
                                ).forEach { swatch ->
                                    Box(
                                        modifier = Modifier
                                            .padding(end = 3.dp)
                                            .size(16.dp)
                                            .clip(CircleShape)
                                            .background(swatch)
                                            .border(0.5.dp, pal.tint.copy(alpha = 0.3f), CircleShape)
                                    )
                                }
                            }
                            Spacer(Modifier.width(10.dp))
                            Text(
                                text = option.displayName,
                                color = pal.textPrimary,
                                fontSize = 14.sp,
                                fontWeight = if (selected) FontWeight.Bold else FontWeight.Medium,
                                modifier = Modifier.weight(1f)
                            )
                            if (selected) {
                                Icon(Icons.Default.Check, null, tint = pal.accent, modifier = Modifier.size(18.dp))
                            }
                        }
                        Spacer(Modifier.height(6.dp))
                    }
                }
            }
        },
        confirmButton = {
            TextButton(onClick = onDismiss) {
                Text("Done", color = pal.accent, fontWeight = FontWeight.Bold)
            }
        },
        containerColor = pal.surface,
        shape = RoundedCornerShape(16.dp),
        modifier = Modifier.border(1.dp, pal.accent.copy(alpha = 0.5f), RoundedCornerShape(16.dp))
    )
}
