package np.com.sanjeeb.marriagecalculator.navigation

sealed class Screen(val route: String) {
    data object Splash : Screen("splash")
    data object Login : Screen("login")
    data object Dashboard : Screen("dashboard")
    data object GameSetup : Screen("game_setup")
    data object PlayGame : Screen("play_game/{gameSetId}") {
        fun createRoute(gameSetId: String) = "play_game/$gameSetId"
    }
    data object RoundInput : Screen("round_input/{gameSetId}/{roundId}") {
        fun createRoute(gameSetId: String, roundId: String) = "round_input/$gameSetId/$roundId"
    }
    data object Scoreboard : Screen("scoreboard/{gameSetId}") {
        fun createRoute(gameSetId: String) = "scoreboard/$gameSetId"
    }
    data object RoundHistory : Screen("round_history/{gameSetId}") {
        fun createRoute(gameSetId: String) = "round_history/$gameSetId"
    }
    data object Friend : Screen("friend")
}
