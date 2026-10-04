package np.com.sanjeeb.marriagecalculator

import android.app.Application
import com.google.firebase.crashlytics.FirebaseCrashlytics
import dagger.hilt.android.HiltAndroidApp

@HiltAndroidApp
class MarriageCalculatorApp : Application() {

    override fun onCreate() {
        super.onCreate()
        // Report crashes from installed builds only — local debugging must not
        // pollute the dashboard. See #137.
        try {
            FirebaseCrashlytics.getInstance().setCrashlyticsCollectionEnabled(!BuildConfig.DEBUG)
        } catch (e: Exception) {
            // Firebase unavailable (GMS missing, mock runs) — crash reporting is
            // best-effort and must never take the app down on startup.
        }
    }
}
