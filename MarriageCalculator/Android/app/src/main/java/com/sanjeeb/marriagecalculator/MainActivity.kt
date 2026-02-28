package com.sanjeeb.marriagecalculator

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import com.sanjeeb.marriagecalculator.ui.LoginScreen
import com.sanjeeb.marriagecalculator.ui.theme.MarriageCalculatorTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MarriageCalculatorTheme {
                LoginScreen(onGoogleLogin = {}, onGuestLogin = {})
            }
        }
    }
}
