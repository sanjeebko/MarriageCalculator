
package com.sanjeeb.marriagecalculator.ui

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.scale
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.ColorFilter
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.withStyle
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.sanjeeb.marriagecalculator.R
import com.sanjeeb.marriagecalculator.ui.components.MetallicButton

// Metallic Noir Color Palette
val MetalGold = Color(0xFFD4AF37)
val SilverTop = Color(0xFFF2F2F2)
val SilverBottom = Color(0xFF909090)
val SilverGlow = Color(0xFFFFFFFF)

val BlueTop = Color(0xFF0088FF)
val BlueBottom = Color(0xFF003399)
val BlueGlow = Color(0xFF00FFFF)

@Composable
fun LoginScreen(onGoogleLogin: () -> Unit, onGuestLogin: () -> Unit) {
    Box(
        modifier = Modifier.fillMaxSize()
    ) {
        // Metallic Background
        Image(
            painter = painterResource(id = R.drawable.login_bg_metal),
            contentDescription = null,
            contentScale = ContentScale.Crop,
            modifier = Modifier.fillMaxSize()
        )

        Column(
            modifier = Modifier
                .fillMaxSize(), // Removed 32.dp padding here to allow logo to grow
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            // Main Logo
            Image(
                painter = painterResource(id = R.drawable.marriage_logo_title),
                contentDescription = "Marriage Calculator",
                contentScale = ContentScale.FillWidth,
                modifier = Modifier
                    .fillMaxWidth()
                    .scale(1.5f)
                    .padding(bottom = 100.dp) // increased to compensate for scale
            )

            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 32.dp), // Re-apply padding for buttons
                verticalArrangement = Arrangement.spacedBy(24.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                // Google Button (Polished Silver Bezel)
                MetallicButton(
                    onClick = onGoogleLogin,
                    text = "Continue with Google",
                    rimColors = listOf(Color(0xFFFFFFFF), Color(0xFF606060)),
                    faceColors = listOf(Color(0xFFFFFFFF), Color(0xFFDDDDDD)),
                    textColor = Color(0xFF333333),
                    modifier = Modifier.height(72.dp),
                    leadingIcon = {
                        Image(
                            painter = painterResource(id = R.drawable.ic_google_logo),
                            contentDescription = null,
                            modifier = Modifier
                                .size(28.dp)
                                .shadow(4.dp, shape = RoundedCornerShape(4.dp))
                        )
                    }
                )

                // Guest Button (Electric Blue Bezel)
                /* TODO: Uncomment later when guest login is needed
                MetallicButton(
                    onClick = onGuestLogin,
                    text = "Play as Guest",
                    rimColors = listOf(Color(0xFF88CCFF), Color(0xFF003366)),
                    faceColors = listOf(Color(0xFF0077EE), Color(0xFF0044AA)),
                    textColor = Color(0xFFE6E6E6),
                    modifier = Modifier.height(72.dp),
                    leadingIcon = {
                        Image(
                            painter = painterResource(id = R.drawable.ic_guest),
                            contentDescription = null,
                            modifier = Modifier
                                .size(28.dp)
                                .shadow(4.dp, shape = RoundedCornerShape(4.dp))
                        )
                    }
                )
                */

                Spacer(Modifier.height(8.dp))
                
                // Terms Text
                Text(
                    text = buildAnnotatedString {
                        append("By continuing, you agree to our ")
                        withStyle(style = SpanStyle(color = MetalGold)) {
                            append("Terms of Service")
                        }
                        append(" and ")
                        withStyle(style = SpanStyle(color = MetalGold)) {
                            append("Privacy Policy")
                        }
                    },
                    color = Color.White.copy(alpha = 0.6f),
                    fontSize = 11.sp,
                    textAlign = TextAlign.Center,
                    lineHeight = 16.sp,
                    modifier = Modifier.padding(horizontal = 16.dp)
                )
            }
        }

        // Footer
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .align(Alignment.BottomCenter)
                .padding(bottom = 32.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Image(
                painter = painterResource(id = R.drawable.ic_mountain),
                contentDescription = null,
                modifier = Modifier.height(32.dp),
                colorFilter = ColorFilter.tint(MetalGold)
            )
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = buildAnnotatedString {
                    append("MADE WITH ")
                    withStyle(style = SpanStyle(color = Color.Red)) {
                        append("❤")
                    }
                    append(" FROM NEPAL")
                },
                color = MetalGold,
                style = MaterialTheme.typography.labelMedium.copy(
                    fontFamily = FontFamily.Serif,
                    fontWeight = FontWeight.Bold,
                    letterSpacing = 1.sp
                )
            )
        }
    }
}

@Preview(showBackground = true)
@Composable
fun LoginScreenPreview() {
    LoginScreen(onGoogleLogin = {}, onGuestLogin = {})
}
