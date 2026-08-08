package np.com.sanjeeb.marriagecalculator.data.repository

import android.content.Context
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import np.com.sanjeeb.marriagecalculator.ui.theme.AppThemeOption
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Device-local color theme selection. Deliberately NOT synced to the database or API - it is a
 * per-device display preference, stored in SharedPreferences only.
 */
@Singleton
class ThemePreference @Inject constructor(
    @ApplicationContext context: Context
) {
    private val prefs = context.getSharedPreferences("app_theme", Context.MODE_PRIVATE)

    private val _theme = MutableStateFlow(AppThemeOption.fromName(prefs.getString(KEY, null)))
    val theme: StateFlow<AppThemeOption> = _theme.asStateFlow()

    fun setTheme(option: AppThemeOption) {
        prefs.edit().putString(KEY, option.name).apply()
        _theme.value = option
    }

    private companion object {
        const val KEY = "selected_theme"
    }
}
