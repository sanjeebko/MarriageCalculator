
package com.sanjeeb.marriagecalculator.ui.theme

import androidx.compose.material.Typography
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.sp
import com.sanjeeb.marriagecalculator.ui.theme.GoldAccent

val FestiveTypography = Typography(
 h4 = TextStyle(
 fontFamily = FontFamily.Serif,
 fontWeight = FontWeight.Bold,
 fontSize = 34.sp,
 color = GoldAccent
 ),
 button = TextStyle(
 fontFamily = FontFamily.SansSerif,
 fontWeight = FontWeight.Medium,
 fontSize = 14.sp,
 letterSpacing = 1.25.sp
 )
)
