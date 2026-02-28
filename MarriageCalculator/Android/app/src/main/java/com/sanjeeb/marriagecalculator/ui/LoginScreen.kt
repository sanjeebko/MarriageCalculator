
package com.sanjeeb.marriagecalculator.ui

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.alpha
import androidx.compose.ui.draw.scale
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.graphics.Brush
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
                    rimColors = listOf(Color(0xFFFFFFFF), Color(0xFF606060)), // Highlights and Shadows
                    faceColors = listOf(Color(0xFFFFFFFF), Color(0xFFDDDDDD)),
                    text = "Continue with Google",
                    textColor = Color(0xFF333333),
                    iconResId = R.drawable.ic_google_logo
                )

                // Guest Button (Electric Blue Bezel)
                MetallicButton(
                    onClick = onGuestLogin,
                    rimColors = listOf(Color(0xFF88CCFF), Color(0xFF003366)),
                    faceColors = listOf(Color(0xFF0077EE), Color(0xFF0044AA)), // Reverted to Electric Blue
                    text = "Play as Guest",
                    textColor = Color(0xFFE6E6E6), // Light Gray (~10% Black / 90% White)
                    iconResId = R.drawable.ic_guest
                )

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

@Composable
fun MetallicButton(
    onClick: () -> Unit,
    rimColors: List<Color>,
    faceColors: List<Color>, // Expecting top, mid-light, mid-dark, bottom for simple horizon or just pass list
    text: String,
    textColor: Color,
    iconResId: Int?
) {
    Button(
        onClick = onClick,
        modifier = Modifier
            .fillMaxWidth()
            .height(72.dp) // Thicker button
            .shadow(
                elevation = 12.dp,
                shape = RoundedCornerShape(16.dp),
                spotColor = Color.Black
            ),
        colors = ButtonDefaults.buttonColors(containerColor = Color.Transparent),
        contentPadding = PaddingValues(0.dp),
        shape = RoundedCornerShape(16.dp)
    ) {
        // 1. Thick Outer Bezel (The Rim)
        Box(
            modifier = Modifier
                .fillMaxSize()
                .background(
                    brush = Brush.linearGradient(
                        colors = rimColors,
                        start = androidx.compose.ui.geometry.Offset(0f, 0f),
                        end = androidx.compose.ui.geometry.Offset(0f, Float.POSITIVE_INFINITY)
                    )
                )
                .padding(4.dp), // Thicker Rim
            contentAlignment = Alignment.Center
        ) {
            // 2. Inner Face with Complex Gradient (Horizon Effect)
            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .background(
                        brush = Brush.verticalGradient(
                            colors = faceColors 
                        ),
                        shape = RoundedCornerShape(12.dp)
                    )
                    .border(
                        width = 1.dp,
                        brush = Brush.verticalGradient(
                            colors = listOf(
                                Color.White.copy(alpha = 0.5f), 
                                Color.Transparent
                            )
                        ),
                        shape = RoundedCornerShape(12.dp)
                    ),
                contentAlignment = Alignment.Center
            ) {
                // Horizontal Sheen Overlay (10% opacity)
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .alpha(0.1f)
                        .background(
                            brush = Brush.horizontalGradient(
                                colorStops = arrayOf(
                                    0.0f to Color.Black,
                                    0.25f to Color.White,
                                    0.35f to Color.Black,// Averaged 15-35 range for smoothness
                                    0.65f to Color.Black,
                                    0.85f to Color.White,
                                    1.0f to Color.Black
                                )
                            ),
                            shape = RoundedCornerShape(12.dp)
                        )
                )
                // Horizontal Sheen Overlay (10% opacity)
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .alpha(0.1f)
                        .background(
                            brush = Brush.horizontalGradient(
                                colorStops = arrayOf(
                                    0.0f to Color.Black,
                                    0.25f to Color.White, // Averaged 15-35 range for smoothness
                                    0.35f to Color.Black,
                                    0.80f to Color.White,
                                    1.0f to Color.Black
                                )
                            ),
                            shape = RoundedCornerShape(12.dp)
                        )
                )
                
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .alpha(0.05f)
                        .background(
                            brush = Brush.verticalGradient(
                                colorStops = arrayOf(
                                    0.0f to Color.Black,
                                    0.15f to Color.White,
                                    0.25f to Color.White,
                                    0.35f to Color.Black,// Averaged 15-35 range for smoothness
                                    0.75f to Color.Black,
                                    0.85f to Color.Gray,
                                    0.90f to Color.Black,
                                    1.0f to Color.Black
                                )
                            ),
                            shape = RoundedCornerShape(12.dp)
                        )
                )
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .alpha(0.05f)
                        .background(
                            brush = Brush.verticalGradient(
                                colorStops = arrayOf(
                                    0.0f to Color.Black,
                                    0.15f to Color.White, // Averaged 15-35 range for smoothness
                                    0.25f to Color.Gray,

                                    0.75f to Color.Black,
                                    0.80f to Color.Gray,
                                    1.0f to Color.Black
                                )
                            ),
                            shape = RoundedCornerShape(12.dp)
                        )
                )

                // 3. Floating Content (Text & Icon)
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    modifier = Modifier.padding(horizontal = 16.dp)
                ) {
                    if (iconResId != null) {
                        Image(
                            painter = painterResource(id = iconResId),
                            contentDescription = null,
                            modifier = Modifier
                                .size(28.dp)
                                .shadow(4.dp, shape =  RoundedCornerShape(4.dp)) // Slight shadow for icon
                        )
                        Spacer(Modifier.width(16.dp))
                    }
                    
                    // Text with White Emboss effect
                    Box {
                         // 1. White Highlight (Emboss) - Bottom reflection
                         Text(
                            text = text,
                            color = Color.White.copy(alpha = 0.6f),
                            style = MaterialTheme.typography.titleMedium.copy(
                                fontWeight = FontWeight.Bold,
                                fontSize = 18.sp
                            ),
                             modifier = Modifier.padding(top = 1.2.dp) // Shift down for emboss
                        )
                        // 2. Main Text
                        Text(
                            text = text,
                            color = textColor,
                            style = MaterialTheme.typography.titleMedium.copy(
                                fontWeight = FontWeight.Bold,
                                fontSize = 18.sp
                            )
                        )
                    }
                }
            }
        }
    }
}

@Preview(showBackground = true)
@Composable
fun LoginScreenPreview() {
    LoginScreen(onGoogleLogin = {}, onGuestLogin = {})
}
