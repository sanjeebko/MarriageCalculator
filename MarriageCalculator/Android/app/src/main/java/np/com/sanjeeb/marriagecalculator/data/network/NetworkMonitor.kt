package np.com.sanjeeb.marriagecalculator.data.network

import android.content.Context
import android.net.ConnectivityManager
import android.net.Network
import android.net.NetworkCapabilities
import android.net.NetworkRequest
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.channels.awaitClose
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.callbackFlow
import kotlinx.coroutines.flow.distinctUntilChanged
import javax.inject.Inject
import javax.inject.Singleton

interface NetworkMonitor {
    val isOnline: Flow<Boolean>
}

@Singleton
class ConnectivityNetworkMonitor @Inject constructor(
    @param:ApplicationContext private val context: Context
) : NetworkMonitor {
    private val connectivityManager =
        context.getSystemService(Context.CONNECTIVITY_SERVICE) as? ConnectivityManager

    override val isOnline: Flow<Boolean> = callbackFlow {
        val callback = object : ConnectivityManager.NetworkCallback() {
            private val networks = mutableSetOf<Network>()

            override fun onAvailable(network: Network) {
                networks += network
                trySend(true)
            }

            override fun onLost(network: Network) {
                networks -= network
                trySend(networks.isNotEmpty())
            }

            override fun onCapabilitiesChanged(network: Network, capabilities: NetworkCapabilities) {
                val hasInternet = capabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET) &&
                        capabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_VALIDATED)
                if (hasInternet) {
                    networks += network
                } else {
                    networks -= network
                }
                trySend(networks.isNotEmpty())
            }
        }

        val request = NetworkRequest.Builder()
            .addCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET)
            .build()

        try {
            connectivityManager?.registerNetworkCallback(request, callback)
        } catch (e: Exception) {
            trySend(true)
        }

        val currentNetwork = connectivityManager?.activeNetwork
        val caps = connectivityManager?.getNetworkCapabilities(currentNetwork)
        val isInitiallyOnline = caps?.hasCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET) == true &&
                caps.hasCapability(NetworkCapabilities.NET_CAPABILITY_VALIDATED)
        trySend(isInitiallyOnline)

        awaitClose {
            try {
                connectivityManager?.unregisterNetworkCallback(callback)
            } catch (e: Exception) {
                // Ignored
            }
        }
    }.distinctUntilChanged()
}
