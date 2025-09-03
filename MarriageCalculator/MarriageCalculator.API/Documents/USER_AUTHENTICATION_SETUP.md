# User Authentication System - Setup and Usage Guide

## Overview

The MarriageCalculator API now includes a comprehensive user authentication system with the following features:

- **User Registration** with email verification
- **JWT-based Authentication** for secure API access
- **Password Security** with strength validation and secure hashing
- **Email Verification** with 5-digit codes (2-hour expiration)
- **Token Management** with logout and blacklisting
- **Separate User System** (distinct from Players)

## Architecture

### Models
- **User**: Main user account entity (separate from Player)
- **UserEmailVerification**: Email verification codes with expiration

### Controllers
- **UserRegistrationController**: `/api/UserRegistration`
  - `POST /register` - Register new user
  - `POST /verify-email` - Verify email with code
  - `POST /resend-verification` - Resend verification code

- **UserAuthController**: `/api/UserAuth`
  - `POST /login` - User login with JWT token
  - `POST /logout` - Logout and blacklist token
  - `GET /me` - Get current user info
  - `POST /validate-token` - Validate JWT token

### Services
- **UserAuthService**: Main authentication business logic
- **JwtService**: JWT token generation and validation
- **PasswordService**: Password hashing and validation
- **EmailService**: Email sending (currently logs to console)

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!@#",
    "Issuer": "MarriageCalculator.API",
    "Audience": "MarriageCalculator.Users",
    "ExpirationMinutes": 60
  },
  "Email": {
    "FromAddress": "noreply@marriagecalculator.com",
    "FromName": "Marriage Calculator"
  }
}
```

### Environment Variables (Production)
- `MCDATABASE`: Database server
- `MCUSER`: Database username
- `MCPASSWORD`: Database password

## Password Requirements

Passwords must meet the following criteria:
- **Minimum 8 characters** long
- **At least 1 capital letter** (A-Z)
- **At least 1 number** (0-9) **OR** **1 symbol** (!@#$%^&*()_+-=[]{}|;':\",./<>?)

## API Usage Examples

### 1. User Registration

```http
POST /api/UserRegistration/register
Content-Type: application/json

{
  "displayName": "John Doe",
  "email": "john.doe@example.com",
  "password": "MySecure123!"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "User registered successfully. Please check your email for verification code.",
  "data": {
    "id": 1,
    "displayName": "John Doe",
    "email": "john.doe@example.com",
    "isEmailVerified": false,
    "createdAt": "2025-01-14T10:30:00Z",
    "lastLoginAt": null,
    "isActive": true
  }
}
```

### 2. Email Verification

```http
POST /api/UserRegistration/verify-email
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "verificationCode": "12345"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Email verified successfully."
}
```

### 3. User Login

```http
POST /api/UserAuth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "MySecure123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expires": "2025-01-14T11:30:00Z",
    "user": {
      "id": 1,
      "displayName": "John Doe",
      "email": "john.doe@example.com",
      "isEmailVerified": true,
      "createdAt": "2025-01-14T10:30:00Z",
      "lastLoginAt": "2025-01-14T10:32:00Z",
      "isActive": true
    }
  }
}
```

### 4. Accessing Protected Endpoints

```http
GET /api/UserAuth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 5. User Logout

```http
POST /api/UserAuth/logout
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Database Schema

### User Table
```sql
CREATE TABLE [User] (
    Id int IDENTITY(1,1) PRIMARY KEY,
    DisplayName nvarchar(100) NOT NULL,
    Email nvarchar(255) NOT NULL UNIQUE,
    PasswordHash nvarchar(max) NOT NULL,
    Salt nvarchar(max) NOT NULL,
    IsEmailVerified bit NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt datetime2 NULL,
    IsActive bit NOT NULL DEFAULT 1
);
```

### UserEmailVerification Table
```sql
CREATE TABLE [UserEmailVerification] (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId int NOT NULL,
    VerificationCode nvarchar(5) NOT NULL,
    ExpiresAt datetime2 NOT NULL,
    IsUsed bit NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UsedAt datetime2 NULL,
    FOREIGN KEY (UserId) REFERENCES [User](Id) ON DELETE CASCADE
);
```

## Security Features

1. **Password Hashing**: PBKDF2 with SHA-256, 100,000 iterations
2. **Salt**: Unique 256-bit cryptographic salt per password
3. **JWT Security**: HS256 signing with configurable expiration
4. **Token Blacklisting**: Logout invalidates tokens
5. **Email Verification**: Required before login
6. **Rate Limiting**: Consider implementing for production

## Development vs Production

### Development
- Longer JWT expiration (120 minutes)
- HTTP allowed for JWT
- Console email logging
- More detailed logging

### Production
- Standard JWT expiration (60 minutes)
- HTTPS required for JWT
- Implement actual email service (SendGrid, AWS SES, etc.)
- Secure JWT secret key (use environment variables)

## Email Service Integration

Currently, emails are logged to the console. For production:

1. **SendGrid Integration:**
```csharp
// In EmailService.cs, replace logging with:
var client = new SendGridClient(_configuration["SendGrid:ApiKey"]);
var from = new EmailAddress(_configuration["Email:FromAddress"], "Marriage Calculator");
var to = new EmailAddress(email, displayName);
var msg = MailHelper.CreateSingleEmail(from, to, "Verify Your Email Address", emailBody, emailBody);
var response = await client.SendEmailAsync(msg);
return response.IsSuccessStatusCode;
```

2. **AWS SES Integration:**
```csharp
// Add AWS SDK and configure SES client
var sesClient = new AmazonSimpleEmailServiceClient();
var sendRequest = new SendEmailRequest
{
    Source = _configuration["Email:FromAddress"],
    Destination = new Destination { ToAddresses = { email } },
    Message = new Message
    {
        Subject = new Content("Verify Your Email Address"),
        Body = new Body { Text = new Content(emailBody) }
    }
};
await sesClient.SendEmailAsync(sendRequest);
```

## Error Handling

The API returns consistent error responses:

```json
{
  "success": false,
  "message": "Descriptive error message"
}
```

Common HTTP status codes:
- **200**: Success
- **201**: Created (registration)
- **400**: Bad Request (validation errors)
- **401**: Unauthorized (login failures)
- **404**: Not Found
- **500**: Internal Server Error

## Testing with Swagger

1. Start the API
2. Navigate to the Swagger UI (root URL in development)
3. Register a new user
4. Check console logs for verification code (since email is logged)
5. Verify email with the code
6. Login to get JWT token
7. Use "Authorize" button in Swagger to set JWT token
8. Test protected endpoints

## Next Steps

1. **Implement Password Reset**: Add forgot password functionality
2. **Role-Based Authorization**: Add user roles and permissions
3. **Account Management**: Update user profile, change password
4. **Session Management**: Advanced token management, refresh tokens
5. **Audit Logging**: Track authentication events
6. **Rate Limiting**: Prevent brute force attacks
7. **Email Templates**: Professional HTML email templates
8. **Two-Factor Authentication**: SMS or TOTP-based 2FA

## Migration from Existing System

The new User system is completely separate from the existing Player system:
- **Users**: Authentication accounts for API access
- **Players**: Game participants (remain unchanged)

This allows flexibility in how users and players are associated in your application logic. 