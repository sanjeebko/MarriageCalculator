package com.sanjeeb.marriagecalculator.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.navArgument
import com.sanjeeb.marriagecalculator.ui.LoginScreen
import com.sanjeeb.marriagecalculator.ui.dashboard.DashboardScreen
import com.sanjeeb.marriagecalculator.ui.gamesetup.GameSetupScreen
import com.sanjeeb.marriagecalculator.ui.playgame.PlayGameScreen
import com.sanjeeb.marriagecalculator.ui.roundinput.RoundInputScreen
import com.sanjeeb.marriagecalculator.ui.scoreboard.ScoreboardScreen
import com.sanjeeb.marriagecalculator.ui.splash.SplashScreen

@Composable
fun MarriageNavGraph(navController: NavHostController) {
    NavHost(
        navController = navController,
        startDestination = Screen.Splash.route
    ) {
        composable(Screen.Splash.route) {
            SplashScreen(
                onSplashComplete = {
                    navController.navigate(Screen.Login.route) {
                        popUpTo(Screen.Splash.route) { inclusive = true }
                    }
                }
            )
        }

        composable(Screen.Login.route) {
            LoginScreen(
                onGoogleLogin = {
                    navController.navigate(Screen.Dashboard.route) {
                        popUpTo(Screen.Login.route) { inclusive = true }
                    }
                },
                onGuestLogin = {
                    navController.navigate(Screen.Dashboard.route) {
                        popUpTo(Screen.Login.route) { inclusive = true }
                    }
                }
            )
        }

        composable(Screen.Dashboard.route) {
            DashboardScreen(
                onNewGame = { navController.navigate(Screen.GameSetup.route) },
                onResumeGame = { gameSetId ->
                    navController.navigate(Screen.PlayGame.createRoute(gameSetId))
                }
            )
        }

        composable(Screen.GameSetup.route) {
            GameSetupScreen(
                onGameCreated = { gameSetId ->
                    navController.navigate(Screen.PlayGame.createRoute(gameSetId)) {
                        popUpTo(Screen.Dashboard.route)
                    }
                },
                onBack = { navController.popBackStack() }
            )
        }

        composable(
            route = Screen.PlayGame.route,
            arguments = listOf(navArgument("gameSetId") { type = NavType.StringType })
        ) { backStackEntry ->
            val gameSetId = backStackEntry.arguments?.getString("gameSetId") ?: return@composable
            PlayGameScreen(
                gameSetId = gameSetId,
                onAddRound = { roundId ->
                    navController.navigate(Screen.RoundInput.createRoute(gameSetId, roundId))
                },
                onViewScoreboard = {
                    navController.navigate(Screen.Scoreboard.createRoute(gameSetId))
                },
                onBack = { navController.popBackStack() }
            )
        }

        composable(
            route = Screen.RoundInput.route,
            arguments = listOf(
                navArgument("gameSetId") { type = NavType.StringType },
                navArgument("roundId") { type = NavType.StringType }
            )
        ) { backStackEntry ->
            val gameSetId = backStackEntry.arguments?.getString("gameSetId") ?: return@composable
            val roundId = backStackEntry.arguments?.getString("roundId") ?: return@composable
            RoundInputScreen(
                gameSetId = gameSetId,
                roundId = roundId,
                onScoreSubmitted = { navController.popBackStack() },
                onBack = { navController.popBackStack() }
            )
        }

        composable(
            route = Screen.Scoreboard.route,
            arguments = listOf(navArgument("gameSetId") { type = NavType.StringType })
        ) { backStackEntry ->
            val gameSetId = backStackEntry.arguments?.getString("gameSetId") ?: return@composable
            ScoreboardScreen(
                gameSetId = gameSetId,
                onBack = { navController.popBackStack() }
            )
        }
    }
}
