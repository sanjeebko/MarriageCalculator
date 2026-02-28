package com.sanjeeb.marriagecalculator.navigation

sealed class Screen(val route: String) {
    data object Login : Screen("login")
    data object Dashboard : Screen("dashboard")
    data object GameSetup : Screen("game_setup")
    data object PlayGame : Screen("play_game/{gameSetId}") {
        fun createRoute(gameSetId: Int) = "play_game/$gameSetId"
    }
    data object RoundInput : Screen("round_input/{gameSetId}/{roundId}") {
        fun createRoute(gameSetId: Int, roundId: Int) = "round_input/$gameSetId/$roundId"
    }
    data object Scoreboard : Screen("scoreboard/{gameSetId}") {
        fun createRoute(gameSetId: Int) = "scoreboard/$gameSetId"
    }
    data object RoundHistory : Screen("round_history/{gameSetId}") {
        fun createRoute(gameSetId: Int) = "round_history/$gameSetId"
    }
}
