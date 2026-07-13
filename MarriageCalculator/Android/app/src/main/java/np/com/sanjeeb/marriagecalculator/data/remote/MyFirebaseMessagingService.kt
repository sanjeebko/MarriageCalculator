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

        val type = remoteMessage.data["type"]
        val title = remoteMessage.notification?.title ?: remoteMessage.data["title"] ?: "Marriage Game"
        val body = remoteMessage.notification?.body ?: remoteMessage.data["body"] ?: ""

        when (type) {
            "FRIEND_REQUEST" -> {
                val friendshipId = remoteMessage.data["friendshipId"]
                val requesterName = remoteMessage.data["requesterName"] ?: "Someone"
                sendFriendRequestNotification(friendshipId, requesterName)
            }
            "FRIEND_ACCEPTED" -> {
                val requesterName = remoteMessage.data["requesterName"] ?: "Someone"
                sendNotification("Friend Request Accepted", "$requesterName is now your friend!", null)
            }
            else -> {
                val gameSetId = remoteMessage.data["gameSetId"]
                sendNotification(title, body, gameSetId)
            }
        }
    }

    private fun sendFriendRequestNotification(friendshipId: String?, requesterName: String) {
        if (friendshipId == null) return

        val channelId = "friend_requests"
        val notificationId = System.currentTimeMillis().toInt()

        // Intent to open the Friends screen
        val contentIntent = Intent(Intent.ACTION_VIEW, Uri.parse("marriagecalculator://friends")).apply {
            setClass(applicationContext, MainActivity::class.java)
            flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TOP
        }

        val contentPendingIntent = PendingIntent.getActivity(
            this,
            0,
            contentIntent,
            PendingIntent.FLAG_ONE_SHOT or PendingIntent.FLAG_IMMUTABLE
        )

        // Action for Accepting
        val acceptIntent = Intent(this, FriendRequestReceiver::class.java).apply {
            putExtra("friendshipId", friendshipId)
            putExtra("accept", true)
            putExtra("notificationId", notificationId)
        }
        val acceptPendingIntent = PendingIntent.getBroadcast(
            this,
            notificationId + 1,
            acceptIntent,
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE
        )

        // Action for Declining
        val declineIntent = Intent(this, FriendRequestReceiver::class.java).apply {
            putExtra("friendshipId", friendshipId)
            putExtra("accept", false)
            putExtra("notificationId", notificationId)
        }
        val declinePendingIntent = PendingIntent.getBroadcast(
            this,
            notificationId + 2,
            declineIntent,
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE
        )

        val defaultIcon = applicationContext.resources.getIdentifier("app_icon", "drawable", packageName)

        val notificationBuilder = NotificationCompat.Builder(this, channelId)
            .setSmallIcon(if (defaultIcon != 0) defaultIcon else android.R.drawable.ic_dialog_info)
            .setContentTitle("Friend Request")
            .setContentText("$requesterName sent you a friend request.")
            .setAutoCancel(true)
            .setContentIntent(contentPendingIntent)
            .setPriority(NotificationCompat.PRIORITY_HIGH)
            .addAction(android.R.drawable.ic_input_add, "Accept", acceptPendingIntent)
            .addAction(android.R.drawable.ic_delete, "Decline", declinePendingIntent)

        val notificationManager = getSystemService(Context.NOTIFICATION_SERVICE) as NotificationManager

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel(
                channelId,
                "Friend Requests",
                NotificationManager.IMPORTANCE_HIGH
            ).apply {
                description = "Notifications for incoming friend requests"
            }
            notificationManager.createNotificationChannel(channel)
        }

        notificationManager.notify(notificationId, notificationBuilder.build())
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
