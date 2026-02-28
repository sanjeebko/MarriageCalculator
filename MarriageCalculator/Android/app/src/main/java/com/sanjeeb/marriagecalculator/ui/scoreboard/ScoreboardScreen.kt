package com.sanjeeb.marriagecalculator.ui.scoreboard

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier

@Composable
fun ScoreboardScreen(
    gameSetId: Int,
    onBack: () -> Unit
) {
    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center
    ) {
        Text("Scoreboard - Game #$gameSetId - Coming Soon")
    }
}
