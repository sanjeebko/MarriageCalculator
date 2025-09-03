# Email Testing API Documentation

## Overview

The `EmailTestController` provides endpoints for testing email functionality **with authentication required**. These endpoints are designed for authorized testing and monitoring purposes.

## Important Notes

?? **AUTHENTICATION REQUIRED**: All endpoints (except health check) require valid JWT authentication.

?? **Monitoring**: The health endpoint remains public for Kubernetes monitoring.

## Prerequisites

Before using these endpoints, ensure:

1. **Authentication**: Obtain a valid JWT token through the authentication endpoints
2. **Environment Variables**: Configure the following environment variables:
   - `MCSMTP` - SMTP server (e.g., smtp.zoho.eu)
   - `MCMAILUSERNAME` - Email username/address
   - `MCMAILPASSWORD` - Email password or app-specific password

## Available Endpoints

### 1. Health Check (Public)

**GET** `/api/EmailTest/health`

?? **No authentication required** - Public endpoint for Kubernetes monitoring.

Returns the current API health status.

**Response Example:**
```json
{
  "success": true,
  "message": "API is healthy",
  "data": {
    "status": "Healthy",
    "timestamp": "2025-07-31T23:45:00.000Z",
    "environment": "Production",
    "version": "1.0.0",
    "podName": "marriagecalculatorapi-xyz123",
    "isKubernetes": true
  }
}
```

### 2. Test SMTP Connection (Authenticated)

**GET** `/api/EmailTest/test-smtp-connection`

?? **Authentication required** - Tests SMTP server connection without sending an email.

**Headers Required:**
```
Authorization: Bearer <your-jwt-token>
```

**Response Example:**
```json
{
  "success": true,
  "message": "SMTP connection test successful",
  "data": {
    "smtpServer": "smtp.zoho.eu",
    "port": 587,
    "useSsl": true,
    "fromEmail": "noreply@sanjeebojha.com.np",
    "testTimestamp": "2025-07-31T23:45:00.000Z",
    "connectionSuccessful": true,
    "errorMessage": "",
    "responseTime": 150.5
  }
}
```

### 3. Test Verification Email (Authenticated)

**POST** `/api/EmailTest/send-verification`

?? **Authentication required** - Sends a test verification email with a randomly generated 5-digit code.

**Headers Required:**
```
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "email": "test@example.com",
  "displayName": "Test User"
}
```

**Response Example:**
```json
{
  "success": true,
  "message": "Test verification email sent successfully to test@example.com. Verification code: 12345"
}
```

## Testing with cURL

### 1. Get Authentication Token First
```bash
# Login to get JWT token
curl -X POST "https://mcapi.sanjeebojha.com.np/api/UserAuth/login" \
     -H "Content-Type: application/json" \
     -d '{"email":"your-email@example.com"}'

# Use the token from the response for subsequent requests
```

### 2. Health Check (No Auth Required)
```bash
curl -X GET "https://mcapi.sanjeebojha.com.np/api/EmailTest/health" \
     -H "accept: application/json"
```

### 3. Test SMTP Connection (Auth Required)
```bash
curl -X GET "https://mcapi.sanjeebojha.com.np/api/EmailTest/test-smtp-connection" \
     -H "accept: application/json" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 4. Send Test Verification Email (Auth Required)
```bash
curl -X POST "https://mcapi.sanjeebojha.com.np/api/EmailTest/send-verification" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -d '{"email":"test@example.com","displayName":"Test User"}'
```

## Testing with Swagger UI

1. Navigate to your API Swagger UI: `https://mcapi.sanjeebojha.com.np/swagger`
2. **Authenticate first:**
   - Click the **"Authorize"** button at the top
   - Enter your JWT token in the format: `Bearer YOUR_JWT_TOKEN`
   - Click **"Authorize"**
3. Look for the **"Email Testing"** section
4. Expand any endpoint you want to test
5. Click **"Try it out"**
6. Fill in the request body
7. Click **"Execute"**

## Security Features

### Authentication Requirements
- All email testing endpoints require valid JWT authentication
- Health check endpoint remains public for monitoring purposes
- 401 Unauthorized responses for unauthenticated requests

### Authorization Headers
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Error Responses

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### 400 Bad Request
```json
{
  "success": false,
  "message": "Invalid email address provided."
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Failed to send verification email. Check server logs for details."
}
```

## Production Deployment

? **Security Implemented:**
- Authentication required for sensitive operations
- Health check available for monitoring
- Proper error handling and logging
- No sensitive data exposure

?? **Additional Security Recommendations:**
- Implement rate limiting for email endpoints
- Use professional email services like SendGrid or AWS SES
- Add email queuing for better performance and reliability
- Monitor and log all email operations

## Troubleshooting

### Common Issues:

1. **401 Unauthorized**
   - Ensure you have a valid JWT token
   - Check token expiration
   - Verify Authorization header format

2. **"SMTP configuration missing"**
   - Check that all environment variables are set correctly
   - Restart the API server after setting environment variables

3. **"Authentication failed"**
   - Use app-specific passwords for Gmail/Outlook
   - Check that the username/password combination is correct

4. **"Connection refused"**
   - Verify SMTP server address and port
   - Check firewall settings and network policies

### Logging

Check the API server logs for detailed error information when emails fail to send. The EmailService logs all email operations with appropriate log levels.