package com.sanjeeb.marriagecalculator.ui.theme

import androidx.compose.material.MaterialTheme
import androidx.compose.material.lightColors
import androidx.compose.runtime.Composable

private val LightColorPalette = lightColors(
    primary = DeepRedTika,
    primaryVariant = MarigoldOrange,
    secondary = GoldAccent
)

@Composable
fun MarriageCalculatorTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colors = LightColorPalette,
        typography = FestiveTypography,
        content = content
    )
}
