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
    val context = LocalContext.current
    val coroutineScope = rememberCoroutineScope()
    val credentialManager = CredentialManager.create(context)

    var isRegisterMode by remember { mutableStateOf(false) }
    var loginInput by remember { mutableStateOf("") }
    var passwordInput by remember { mutableStateOf("") }
    var emailInput by remember { mutableStateOf("") }
    var otpCodeInput by remember { mutableStateOf("") }
    var regUsernameInput by remember { mutableStateOf("") }
    var regPasswordInput by remember { mutableStateOf("") }
    var regDisplayNameInput by remember { mutableStateOf("") }
    var codeSentMsg by remember { mutableStateOf<String?>(null) }

    LaunchedEffect(uiState) {
        if (uiState is LoginUiState.Success) {
            onLoginSuccess()
        } else if (uiState is LoginUiState.CodeSent) {
            codeSentMsg = (uiState as LoginUiState.CodeSent).message
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

        // Faint card background overlay
        Image(
            painter = painterResource(id = R.drawable.card_bg_pattern),
            contentDescription = null,
            contentScale = ContentScale.Crop,
            modifier = Modifier.fillMaxSize(),
            alpha = 0.05f
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
                    .scale(1.3f)
                    .padding(bottom = 20.dp)
            )

            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 24.dp),
                verticalArrangement = Arrangement.spacedBy(10.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                // Auth Mode Toggle Tabs (Sign In vs Register)
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.Center
                ) {
                    GlassButton(
                        onClick = { isRegisterMode = false },
                        text = "Sign In",
                        containerColor = if (!isRegisterMode) MetalGold else Color.White.copy(alpha = 0.1f),
                        textColor = if (!isRegisterMode) Color.Black else Color.White,
                        height = 40,
                        modifier = Modifier.weight(1f)
                    )
                    Spacer(Modifier.width(8.dp))
                    GlassButton(
                        onClick = { isRegisterMode = true },
                        text = "Register",
                        containerColor = if (isRegisterMode) MetalGold else Color.White.copy(alpha = 0.1f),
                        textColor = if (isRegisterMode) Color.Black else Color.White,
                        height = 40,
                        modifier = Modifier.weight(1f)
                    )
                }

                if (!isRegisterMode) {
                    // --- SIGN IN FORM ---
                    OutlinedTextField(
                        value = loginInput,
                        onValueChange = { loginInput = it },
                        label = { Text("Username or Email", color = MetalGold) },
                        placeholder = { Text("Enter username or email", color = Color.White.copy(alpha = 0.3f)) },
                        modifier = Modifier.fillMaxWidth(),
                        singleLine = true,
                        colors = OutlinedTextFieldDefaults.colors(
                            focusedContainerColor = Color(0xFF1C1C1C),
                            unfocusedContainerColor = Color(0xFF121212),
                            focusedBorderColor = MetalGold,
                            unfocusedBorderColor = Color.White.copy(alpha = 0.1f),
                            focusedLabelColor = MetalGold,
                            unfocusedTextColor = Color.White,
                            focusedTextColor = Color.White
                        ),
                        shape = RoundedCornerShape(12.dp)
                    )

                    OutlinedTextField(
                        value = passwordInput,
                        onValueChange = { passwordInput = it },
                        label = { Text("Password", color = MetalGold) },
                        placeholder = { Text("Enter password", color = Color.White.copy(alpha = 0.3f)) },
                        modifier = Modifier.fillMaxWidth(),
                        singleLine = true,
                        visualTransformation = androidx.compose.ui.text.input.PasswordVisualTransformation(),
                        colors = OutlinedTextFieldDefaults.colors(
                            focusedContainerColor = Color(0xFF1C1C1C),
                            unfocusedContainerColor = Color(0xFF121212),
                            focusedBorderColor = MetalGold,
                            unfocusedBorderColor = Color.White.copy(alpha = 0.1f),
                            focusedLabelColor = MetalGold,
                            unfocusedTextColor = Color.White,
                            focusedTextColor = Color.White
                        ),
                        shape = RoundedCornerShape(12.dp)
                    )

                    GlassButton(
                        onClick = { viewModel.loginWithEmailOrUsername(loginInput, passwordInput) },
                        text = "Sign In with Password",
                        containerColor = MetalGold,
                        textColor = Color.Black,
                        height = 48,
                        isLoading = uiState is LoginUiState.Loading
                    )
                } else {
                    // --- REGISTER FORM (OTP + User Creation) ---
                    OutlinedTextField(
                        value = emailInput,
                        onValueChange = { emailInput = it },
                        label = { Text("Email Address", color = MetalGold) },
                        placeholder = { Text("Enter your email", color = Color.White.copy(alpha = 0.3f)) },
                        modifier = Modifier.fillMaxWidth(),
                        singleLine = true,
                        colors = OutlinedTextFieldDefaults.colors(
                            focusedContainerColor = Color(0xFF1C1C1C),
                            unfocusedContainerColor = Color(0xFF121212),
                            focusedBorderColor = MetalGold,
                            unfocusedBorderColor = Color.White.copy(alpha = 0.1f),
                            focusedLabelColor = MetalGold,
                            unfocusedTextColor = Color.White,
                            focusedTextColor = Color.White
                        ),
                        shape = RoundedCornerShape(12.dp)
                    )

                    GlassButton(
                        onClick = { viewModel.sendVerificationCode(emailInput) },
                        text = "Send 6-Digit OTP Code",
                        containerColor = Color.White.copy(alpha = 0.2f),
                        textColor = Color.White,
                        height = 40,
                        isLoading = uiState is LoginUiState.Loading
                    )

                    if (codeSentMsg != null) {
                        Text(codeSentMsg!!, color = Color(0xFF4CAF50), fontSize = 12.sp, fontWeight = FontWeight.Bold)
                    }

                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        OutlinedTextField(
                            value = otpCodeInput,
                            onValueChange = { otpCodeInput = it },
                            label = { Text("6-Digit Code", color = MetalGold) },
                            modifier = Modifier.weight(1f),
                            singleLine = true,
                            colors = OutlinedTextFieldDefaults.colors(
                                focusedContainerColor = Color(0xFF1C1C1C),
                                unfocusedContainerColor = Color(0xFF121212),
                                focusedBorderColor = MetalGold,
                                unfocusedBorderColor = Color.White.copy(alpha = 0.1f),
                                focusedTextColor = Color.White,
                                unfocusedTextColor = Color.White
                            ),
                            shape = RoundedCornerShape(12.dp)
                        )

                        OutlinedTextField(
                            value = regUsernameInput,
                            onValueChange = { regUsernameInput = it },
                            label = { Text("Username", color = MetalGold) },
                            modifier = Modifier.weight(1f),
                            singleLine = true,
                            colors = OutlinedTextFieldDefaults.colors(
                                focusedContainerColor = Color(0xFF1C1C1C),
                                unfocusedContainerColor = Color(0xFF121212),
                                focusedBorderColor = MetalGold,
                                unfocusedBorderColor = Color.White.copy(alpha = 0.1f),
                                focusedTextColor = Color.White,
                                unfocusedTextColor = Color.White
                            ),
                            shape = RoundedCornerShape(12.dp)
                        )
                    }

                    OutlinedTextField(
                        value = regPasswordInput,
                        onValueChange = { regPasswordInput = it },
                        label = { Text("Password", color = MetalGold) },
                        modifier = Modifier.fillMaxWidth(),
                        singleLine = true,
                        visualTransformation = androidx.compose.ui.text.input.PasswordVisualTransformation(),
                        colors = OutlinedTextFieldDefaults.colors(
                            focusedContainerColor = Color(0xFF1C1C1C),
                            unfocusedContainerColor = Color(0xFF121212),
                            focusedBorderColor = MetalGold,
                            unfocusedBorderColor = Color.White.copy(alpha = 0.1f),
                            focusedTextColor = Color.White,
                            unfocusedTextColor = Color.White
                        ),
                        shape = RoundedCornerShape(12.dp)
                    )

                    GlassButton(
                        onClick = { viewModel.register(emailInput, otpCodeInput, regUsernameInput, regPasswordInput, regDisplayNameInput) },
                        text = "Complete Registration",
                        containerColor = MetalGold,
                        textColor = Color.Black,
                        height = 48,
                        isLoading = uiState is LoginUiState.Loading
                    )
                }

                Spacer(Modifier.height(4.dp))

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

                        val signInWithGoogleOption = GetSignInWithGoogleOption.Builder(serverClientId).build()
                        val request = GetCredentialRequest.Builder().addCredentialOption(signInWithGoogleOption).build()

                        val activity = context.findActivity()
                        if (activity != null) {
                            coroutineScope.launch {
                                try {
                                    val result = credentialManager.getCredential(context = activity, request = request)
                                    val credential = result.credential
                                    if (credential is CustomCredential && credential.type == GoogleIdTokenCredential.TYPE_GOOGLE_ID_TOKEN_CREDENTIAL) {
                                        val googleIdTokenCredential = GoogleIdTokenCredential.createFrom(credential.data)
                                        viewModel.loginWithGoogle(
                                            idToken = googleIdTokenCredential.idToken,
                                            displayName = googleIdTokenCredential.displayName,
                                            photoUrl = googleIdTokenCredential.profilePictureUri?.toString()
                                        )
                                    }
                                } catch (e: Exception) {
                                    Toast.makeText(context, "Google Sign-in error: ${e.message}", Toast.LENGTH_LONG).show()
                                }
                            }
                        }
                    },
                    text = "Continue with Google",
                    containerColor = Color.White.copy(alpha = 0.85f),
                    textColor = Color(0xFF333333),
                    height = 48,
                    isLoading = uiState is LoginUiState.Loading,
                    leadingIcon = {
                        Image(
                            painter = painterResource(id = R.drawable.ic_google_logo),
                            contentDescription = null,
                            modifier = Modifier
                                .size(20.dp)
                                .shadow(2.dp, shape = RoundedCornerShape(4.dp))
                        )
                    }
                )

                // Continue as Guest (Offline)
                GlassButton(
                    onClick = { viewModel.loginAsGuest() },
                    text = "Continue as Guest (Offline)",
                    containerColor = Color.White.copy(alpha = 0.12f),
                    textColor = Color.White,
                    height = 44,
                    isLoading = uiState is LoginUiState.Loading
                )

                if (uiState is LoginUiState.Error) {
                    Text(
                        text = (uiState as LoginUiState.Error).message,
                        color = Color(0xFFFF5252),
                        fontSize = 13.sp,
                        fontWeight = FontWeight.Bold,
                        textAlign = TextAlign.Center,
                        modifier = Modifier.padding(top = 2.dp)
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
