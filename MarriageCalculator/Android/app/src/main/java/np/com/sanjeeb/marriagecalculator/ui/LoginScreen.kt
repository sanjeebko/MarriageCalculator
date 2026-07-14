package np.com.sanjeeb.marriagecalculator.ui

import android.app.Activity
import android.content.Context
import android.content.ContextWrapper
import android.widget.Toast
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
import androidx.compose.ui.platform.LocalContext
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
import androidx.credentials.CredentialManager
import androidx.credentials.GetCredentialRequest
import androidx.credentials.CustomCredential
import com.google.android.libraries.identity.googleid.GetSignInWithGoogleOption
import com.google.android.libraries.identity.googleid.GoogleIdTokenCredential
import androidx.hilt.navigation.compose.hiltViewModel
import np.com.sanjeeb.marriagecalculator.R
import np.com.sanjeeb.marriagecalculator.ui.components.GlassButton
import kotlinx.coroutines.launch

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
    val context = LocalContext.current
    val coroutineScope = rememberCoroutineScope()
    val credentialManager = CredentialManager.create(context)

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
                    .padding(bottom = 40.dp) // adjusted to make room for input
            )

            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 32.dp),
                verticalArrangement = Arrangement.spacedBy(14.dp),
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

                // Google Button
                GlassButton(
                    onClick = {
                        val webClientIdResId = context.resources.getIdentifier(
                            "default_web_client_id",
                            "string",
                            context.packageName
                        )
                        val serverClientId = if (webClientIdResId != 0) {
                            context.getString(webClientIdResId)
                        } else {
                            "721245084920-mockserverid.apps.googleusercontent.com"
                        }

                        Toast.makeText(context, "Sign-in start. Client: $serverClientId", Toast.LENGTH_SHORT).show()

                        val signInWithGoogleOption = GetSignInWithGoogleOption.Builder(serverClientId)
                            .build()

                        val request = GetCredentialRequest.Builder()
                            .addCredentialOption(signInWithGoogleOption)
                            .build()


                        val activity = context.findActivity()
                        if (activity != null) {
                            coroutineScope.launch {
                                try {
                                    val result = credentialManager.getCredential(
                                        context = activity,
                                        request = request
                                    )
                                    val credential = result.credential
                                    if (credential is CustomCredential && credential.type == GoogleIdTokenCredential.TYPE_GOOGLE_ID_TOKEN_CREDENTIAL) {
                                        val googleIdTokenCredential = GoogleIdTokenCredential.createFrom(credential.data)
                                        viewModel.loginWithGoogle(
                                            idToken = googleIdTokenCredential.idToken,
                                            displayName = googleIdTokenCredential.displayName,
                                            photoUrl = googleIdTokenCredential.profilePictureUri?.toString()
                                        )
                                    } else {
                                        Toast.makeText(context, "Unsupported auth credential", Toast.LENGTH_SHORT).show()
                                    }
                                } catch (e: Exception) {
                                    val errorMsg = e.message ?: ""
                                    val friendlyMsg = if (errorMsg.contains("28444")) {
                                        "Developer Console Setup Issue (28444). Please check if your SHA-1 fingerprint and Web Client ID are registered correctly."
                                    } else {
                                        "Sign-in error: $errorMsg"
                                    }
                                    Toast.makeText(context, friendlyMsg, Toast.LENGTH_LONG).show()
                                }
                            }
                        } else {
                            Toast.makeText(context, "Activity context not found", Toast.LENGTH_SHORT).show()
                        }
                    },
                    text = "Continue with Google",
                    containerColor = Color.White.copy(alpha = 0.85f),
                    textColor = Color(0xFF333333),
                    height = 64,
                    isLoading = uiState is LoginUiState.Loading,
                    leadingIcon = {
                        Image(
                            painter = painterResource(id = R.drawable.ic_google_logo),
                            contentDescription = null,
                            modifier = Modifier
                                .size(24.dp)
                                .shadow(2.dp, shape = RoundedCornerShape(4.dp))
                        )
                    }
                )

                // Developer Mock Sign-In
                GlassButton(
                    onClick = { viewModel.loginWithMockToken(username) },
                    text = "Developer Mock Sign-In",
                    containerColor = MetalGold.copy(alpha = 0.35f),
                    textColor = MetalGold,
                    height = 52,
                    isLoading = uiState is LoginUiState.Loading
                )

                // Continue as Guest (Offline)
                GlassButton(
                    onClick = { viewModel.loginAsGuest() },
                    text = "Continue as Guest (Offline)",
                    containerColor = Color.White.copy(alpha = 0.12f),
                    textColor = Color.White,
                    height = 52,
                    isLoading = uiState is LoginUiState.Loading
                )

                if (uiState is LoginUiState.Error) {
                    Text(
                        text = (uiState as LoginUiState.Error).message,
                        color = Color(0xFFFF5252),
                        fontSize = 14.sp,
                        fontWeight = FontWeight.Bold,
                        textAlign = TextAlign.Center,
                        modifier = Modifier.padding(top = 4.dp)
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

private fun Context.findActivity(): Activity? {
    var context = this
    while (context is ContextWrapper) {
        if (context is Activity) return context
        context = context.baseContext
    }
    return null
}
