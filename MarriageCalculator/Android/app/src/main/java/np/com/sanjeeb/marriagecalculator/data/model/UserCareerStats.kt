package np.com.sanjeeb.marriagecalculator.data.model

data class UserCareerStats(
    val totalGames: Int = 0,
    val winRatePercent: Int = 0,
    val netProfitLoss: Double = 0.0,
    val netProfitLossFormatted: String = "₨0",
    val isPositive: Boolean = true,
    val isZero: Boolean = true,
    val highestMaal: Int = 0
)
