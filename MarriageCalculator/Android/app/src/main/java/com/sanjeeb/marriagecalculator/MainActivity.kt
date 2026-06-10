package com.sanjeeb.marriagecalculator

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.navigation.compose.rememberNavController
import com.sanjeeb.marriagecalculator.data.repository.SessionManager
import com.sanjeeb.marriagecalculator.navigation.MarriageNavGraph
import com.sanjeeb.marriagecalculator.ui.theme.MarriageCalculatorTheme
import dagger.hilt.android.AndroidEntryPoint
import javax.inject.Inject

@AndroidEntryPoint
class MainActivity : ComponentActivity() {

    @Inject
    lateinit var sessionManager: SessionManager

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MarriageCalculatorTheme {
                val navController = rememberNavController()
                MarriageNavGraph(
                    navController = navController,
                    sessionManager = sessionManager
                )
            }
        }
    }
}
