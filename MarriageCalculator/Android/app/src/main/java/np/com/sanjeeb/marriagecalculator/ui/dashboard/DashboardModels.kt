package np.com.sanjeeb.marriagecalculator.ui.dashboard

import np.com.sanjeeb.marriagecalculator.data.model.Player

data class EnrichedActiveGame(
    val id: String,
    val name: String,
    val lastPlayed: String,
    val players: List<Player>,
    val leaderName: String? = null,
    val leaderScoreText: String? = null,
    val roundStatusText: String = "Not started",
    val totalGamesPlayed: Int = 0,
    val isSettled: Boolean = false,
    val cardSuit: String = "♠"
)
