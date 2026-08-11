package np.com.sanjeeb.marriagecalculator.data.model

data class SendVerificationCodeRequest(
    val email: String
)

data class SendVerificationCodeResult(
    val success: Boolean,
    val message: String
)

data class RegisterUserRequest(
    val email: String,
    val verificationCode: String,
    val username: String,
    val password: String,
    val displayName: String
)

data class LoginRequest(
    val usernameOrEmail: String,
    val password: String
)

data class AuthTokenResult(
    val token: String,
    val userId: String,
    val username: String,
    val email: String,
    val displayName: String,
    val expiresAt: String
)

data class TogglePaymentClearedRequest(
    val paymentCleared: Boolean
)
