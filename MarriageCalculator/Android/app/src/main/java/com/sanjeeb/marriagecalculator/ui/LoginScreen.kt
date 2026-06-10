package com.sanjeeb.marriagecalculator.ui

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.*
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
import androidx.hilt.navigation.compose.hiltViewModel
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
fun LoginScreen(
    onLoginSuccess: () -> Unit,
    viewModel: LoginViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()
    var username by remember { mutableStateOf("sanjeeb") }

    LaunchedEffect(uiState) {
        if (uiState is LoginUiState.Success) {
            onLoginSuccess()
        }
    }

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
            modifier = Modifier.fillMaxSize(),
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
                    .padding(bottom = 60.dp) // adjusted to make room for input
            )

            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 32.dp),
                verticalArrangement = Arrangement.spacedBy(20.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                // Username Input Field (Glossy dark container, Gold highlight)
                OutlinedTextField(
                    value = username,
                    onValueChange = { username = it },
                    label = { Text("Test Username", color = MetalGold) },
                    placeholder = { Text("Enter mock username", color = Color.White.copy(alpha = 0.3f)) },
                    modifier = Modifier.fillMaxWidth(),
                    singleLine = true,
                    colors = OutlinedTextFieldDefaults.colors(
                        focusedContainerColor = Color(0xFF1C1C1C),
                        unfocusedContainerColor = Color(0xFF121212),
                        focusedBorderColor = MetalGold,
                        unfocusedBorderColor = Color.White.copy(alpha = 0.1f),
                        focusedLabelColor = MetalGold,
                        unfocusedLabelColor = Color.White.copy(alpha = 0.5f),
                        focusedTextColor = Color.White,
                        unfocusedTextColor = Color.White
                    ),
                    shape = RoundedCornerShape(12.dp)
                )

                // Google Button (Polished Silver Bezel)
                MetallicButton(
                    onClick = { viewModel.loginWithMockToken(username) },
                    text = "Continue with Google",
                    rimColors = listOf(Color(0xFFFFFFFF), Color(0xFF606060)),
                    faceColors = listOf(Color(0xFFFFFFFF), Color(0xFFDDDDDD)),
                    textColor = Color(0xFF333333),
                    modifier = Modifier.height(72.dp),
                    isLoading = uiState is LoginUiState.Loading,
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

                if (uiState is LoginUiState.Error) {
                    Text(
                        text = (uiState as LoginUiState.Error).message,
                        color = Color(0xFFFF5252),
                        fontSize = 14.sp,
                        fontWeight = FontWeight.Bold,
                        textAlign = TextAlign.Center,
                        modifier = Modifier.padding(top = 8.dp)
                    )
                }

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
