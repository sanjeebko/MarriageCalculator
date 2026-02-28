package com.sanjeeb.marriagecalculator.ui.roundinput

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier

@Composable
fun RoundInputScreen(
    gameSetId: Int,
    roundId: Int,
    onScoreSubmitted: () -> Unit,
    onBack: () -> Unit
) {
    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center
    ) {
        Text("Round Input - Game #$gameSetId, Round #$roundId - Coming Soon")
    }
}
