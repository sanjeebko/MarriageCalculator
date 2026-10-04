package np.com.sanjeeb.marriagecalculator

import android.app.Application
import com.google.firebase.crashlytics.FirebaseCrashlytics
import dagger.hilt.android.HiltAndroidApp

@HiltAndroidApp
class MarriageCalculatorApp : Application() {

    override fun onCreate() {
        super.onCreate()
        // Report crashes from every build type, debug included (#144). Silencing
        // debug builds made "nothing in Crashlytics" ambiguous — it could mean no
        // crash, or simply that reporting was off, which is useless as a signal.
        //
        // Set explicitly rather than relying on the default: the flag is persisted
        // on the device between launches, so a device that ran an earlier build
        // with collection disabled would stay silent until something re-enables it.
        try {
            FirebaseCrashlytics.getInstance().setCrashlyticsCollectionEnabled(true)
        } catch (e: Exception) {
            // Firebase unavailable (GMS missing, mock runs) — crash reporting is
            // best-effort and must never take the app down on startup.
        }
    }
}
