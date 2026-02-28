package com.sanjeeb.marriagecalculator.ui.playgame

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier

@Composable
fun PlayGameScreen(
    gameSetId: Int,
    onAddRound: (Int) -> Unit,
    onViewScoreboard: () -> Unit,
    onBack: () -> Unit
) {
    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center
    ) {
        Text("Play Game #$gameSetId - Coming Soon")
    }
}
