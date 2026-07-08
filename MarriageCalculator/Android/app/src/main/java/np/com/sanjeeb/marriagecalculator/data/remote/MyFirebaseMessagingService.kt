package np.com.sanjeeb.marriagecalculator.data.remote

import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.net.Uri
import android.os.Build
import android.util.Log
import androidx.core.app.NotificationCompat
import com.google.firebase.messaging.FirebaseMessagingService
import com.google.firebase.messaging.RemoteMessage
import np.com.sanjeeb.marriagecalculator.MainActivity
import np.com.sanjeeb.marriagecalculator.R
import np.com.sanjeeb.marriagecalculator.data.repository.SessionManager
import np.com.sanjeeb.marriagecalculator.data.repository.UserRepository
import dagger.hilt.android.AndroidEntryPoint
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import javax.inject.Inject

@AndroidEntryPoint
class MyFirebaseMessagingService : FirebaseMessagingService() {

    @Inject
    lateinit var userRepository: UserRepository

    @Inject
    lateinit var sessionManager: SessionManager

    override fun onNewToken(token: String) {
        super.onNewToken(token)
        Log.d("FCM_SERVICE", "Refreshed token: $token")
        
        // Store token in session manager
        sessionManager.saveFcmToken(token)

        // If user is already logged in, register token with API
        if (sessionManager.isLoggedIn()) {
            CoroutineScope(Dispatchers.IO).launch {
                userRepository.registerFcmToken(token)
            }
        }
    }

    override fun onMessageReceived(remoteMessage: RemoteMessage) {
        super.onMessageReceived(remoteMessage)
        Log.d("FCM_SERVICE", "From: ${remoteMessage.from}")

        // Check if message contains a notification payload
        val title = remoteMessage.notification?.title ?: remoteMessage.data["title"] ?: "Marriage Game Nudge"
        val body = remoteMessage.notification?.body ?: remoteMessage.data["body"] ?: "A player is waiting for you!"
        val gameSetId = remoteMessage.data["gameSetId"]

        sendNotification(title, body, gameSetId)
    }

    private fun sendNotification(title: String, body: String, gameSetId: String?) {
        val channelId = "marriage_nudges"
        val notificationId = System.currentTimeMillis().toInt()

        val intent = if (!gameSetId.isNullOrEmpty()) {
            // Launch app using the deep link URI
            Intent(Intent.ACTION_VIEW, Uri.parse("marriagecalculator://playgame/$gameSetId")).apply {
                setClass(applicationContext, MainActivity::class.java)
                flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TOP
            }
        } else {
            Intent(this, MainActivity::class.java).apply {
                flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TOP
            }
        }

        val pendingIntent = PendingIntent.getActivity(
            this,
            0,
            intent,
            PendingIntent.FLAG_ONE_SHOT or PendingIntent.FLAG_IMMUTABLE
        )

        // Using standard launcher icon as system push fallback
        val defaultIcon = applicationContext.resources.getIdentifier("app_icon", "drawable", packageName)
        
        val notificationBuilder = NotificationCompat.Builder(this, channelId)
            .setSmallIcon(if (defaultIcon != 0) defaultIcon else android.R.drawable.ic_dialog_info)
            .setContentTitle(title)
            .setContentText(body)
            .setAutoCancel(true)
            .setContentIntent(pendingIntent)
            .setPriority(NotificationCompat.PRIORITY_HIGH)

        val notificationManager = getSystemService(Context.NOTIFICATION_SERVICE) as NotificationManager

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel(
                channelId,
                "Marriage Game Nudges",
                NotificationManager.IMPORTANCE_HIGH
            ).apply {
                description = "Channels for game invites and nudge notifications"
            }
            notificationManager.createNotificationChannel(channel)
        }

        notificationManager.notify(notificationId, notificationBuilder.build())
    }
}
