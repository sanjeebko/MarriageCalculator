package np.com.sanjeeb.marriagecalculator.ui.theme

import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertTrue
import org.junit.Test

class AppThemeTest {

    @Test
    fun `BLACK_AND_WHITE is default theme on fallback`() {
        val defaultTheme = AppThemeOption.fromName(null)
        assertEquals(AppThemeOption.BLACK_AND_WHITE, defaultTheme)
    }

    @Test
    fun `BLACK_AND_WHITE is resolved from name`() {
        val theme = AppThemeOption.fromName("BLACK_AND_WHITE")
        assertEquals(AppThemeOption.BLACK_AND_WHITE, theme)
        assertEquals("Black & White", theme.displayName)
    }

    @Test
    fun `BLACK_AND_WHITE is dark theme`() {
        val palette = AppThemeOption.BLACK_AND_WHITE.palette
        assertTrue(palette.isDark)
    }

    @Test
    fun `all theme options have valid palettes`() {
        AppThemeOption.entries.forEach { option ->
            assertNotNull(option.displayName)
            assertNotNull(option.palette)
            assertNotNull(option.palette.backgroundTop)
            assertNotNull(option.palette.backgroundBottom)
            assertNotNull(option.palette.surface)
            assertNotNull(option.palette.accent)
            assertNotNull(option.palette.accentAlt)
            assertNotNull(option.palette.cta)
            assertNotNull(option.palette.frostText)
            assertNotNull(option.palette.frostAccent)
            assertNotNull(option.palette.textPrimary)
            assertNotNull(option.palette.tint)
            assertNotNull(option.palette.cardSurface)
        }
    }
}
