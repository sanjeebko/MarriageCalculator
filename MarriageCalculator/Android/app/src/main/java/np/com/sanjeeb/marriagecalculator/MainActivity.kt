package np.com.sanjeeb.marriagecalculator

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.lifecycle.lifecycleScope
import androidx.navigation.compose.rememberNavController
import com.google.firebase.messaging.FirebaseMessaging
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import np.com.sanjeeb.marriagecalculator.data.repository.UserRepository
import np.com.sanjeeb.marriagecalculator.navigation.MarriageNavGraph
import np.com.sanjeeb.marriagecalculator.ui.theme.MarriageCalculatorTheme
import dagger.hilt.android.AndroidEntryPoint
import kotlinx.coroutines.launch
import javax.inject.Inject

@AndroidEntryPoint
class MainActivity : ComponentActivity() {

    @Inject
    lateinit var sessionManager: SessionManager

    @Inject
    lateinit var userRepository: UserRepository

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        fetchAndRegisterFcmToken()

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

    private fun fetchAndRegisterFcmToken() {
        try {
            FirebaseMessaging.getInstance().token.addOnCompleteListener { task ->
                if (task.isSuccessful) {
                    val token = task.result
                    if (token != null) {
                        sessionManager.saveFcmToken(token)
                        if (sessionManager.isLoggedIn()) {
                            lifecycleScope.launch {
                                userRepository.registerFcmToken(token)
                            }
                        }
                    }
                }
            }
        } catch (e: Exception) {
            // Firebase not initialized or GMS missing (e.g. mock runs)
        }
    }
}
