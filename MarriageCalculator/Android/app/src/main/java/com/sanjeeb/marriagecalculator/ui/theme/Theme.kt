package com.sanjeeb.marriagecalculator.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.runtime.Composable

private val DarkColorScheme = darkColorScheme(
    primary = DeepRedTika,
    secondary = MarigoldOrange,
    tertiary = GoldAccent,
    background = TiharNightBlue,
    surface = TiharNightBlue,
    onPrimary = GoldAccent,
    onSecondary = GoldAccent,
    onBackground = GoldAccent,
    onSurface = GoldAccent
)

@Composable
fun MarriageCalculatorTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = DarkColorScheme,
        typography = FestiveTypography,
        content = content
    )
}
