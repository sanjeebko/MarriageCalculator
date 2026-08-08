package np.com.sanjeeb.marriagecalculator.data.remote

import android.app.NotificationManager
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.widget.Toast
import np.com.sanjeeb.marriagecalculator.data.repository.FriendRepository
import dagger.hilt.android.AndroidEntryPoint
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import javax.inject.Inject

@AndroidEntryPoint
class FriendRequestReceiver : BroadcastReceiver() {

    @Inject
    lateinit var friendRepository: FriendRepository

    override fun onReceive(context: Context, intent: Intent) {
        val friendshipId = intent.getStringExtra("friendshipId") ?: return
        val accept = intent.getBooleanExtra("accept", false)
        val notificationId = intent.getIntExtra("notificationId", -1)

        val notificationManager = context.getSystemService(Context.NOTIFICATION_SERVICE) as NotificationManager
        if (notificationId != -1) {
            notificationManager.cancel(notificationId)
        }

        // onReceive() must return quickly, but responding needs a network call. goAsync() keeps
        // the process alive for that call (~10s budget) instead of racing the OS reclaiming it
        // the instant onReceive returns - without this the request can be silently dropped.
        val pendingResult = goAsync()
        CoroutineScope(Dispatchers.IO).launch {
            try {
                friendRepository.respondFriendRequest(friendshipId, accept)
                withContext(Dispatchers.Main) {
                    val message = if (accept) "Friend request accepted" else "Friend request declined"
                    Toast.makeText(context, message, Toast.LENGTH_SHORT).show()
                }
            } catch (e: Exception) {
                withContext(Dispatchers.Main) {
                    Toast.makeText(context, "Failed to respond to request", Toast.LENGTH_SHORT).show()
                }
            } finally {
                pendingResult.finish()
            }
        }
    }
}
