# Refresh Token Implementation - Long-lived User Sessions

## Overview

The MarriageCalculator API now includes a comprehensive refresh token system that allows users to stay logged in for **7 days** without re-entering their credentials. This implements secure token rotation and provides a seamless user experience.

## Key Features

- ✅ **7-day refresh tokens** - Users stay logged in for a week
- ✅ **1-2 hour access tokens** - Short-lived for security
- ✅ **Automatic token rotation** - New tokens on every refresh
- ✅ **Token revocation** - Individual and bulk token invalidation
- ✅ **Secure token generation** - Cryptographically secure random tokens
- ✅ **Token cleanup** - Automatic expired token removal

## How It Works

### **1. Login Flow**
```
User Login → Access Token (1-2 hours) + Refresh Token (7 days)
```

### **2. Token Refresh Flow**
```
Refresh Token → New Access Token + New Refresh Token
                (Old refresh token automatically revoked)
```

### **3. Automatic Token Rotation**
- Each refresh operation generates new tokens
- Old refresh token is immediately revoked
- Prevents token reuse attacks

## Database Schema

### RefreshToken Table
```sql
CREATE TABLE RefreshToken (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId int NOT NULL,
    Token nvarchar(256) NOT NULL UNIQUE,
    ExpiresAt datetime2 NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive bit NOT NULL DEFAULT 1,
    RevokedAt datetime2 NULL,
    ReplacedByToken nvarchar(256) NULL,
    RevokedReason nvarchar(100) NULL,
    FOREIGN KEY (UserId) REFERENCES [User](Id) ON DELETE CASCADE
);
```

## API Endpoints

### **1. Login (Updated)**
`POST /api/UserAuth/login`

**Response now includes refresh token:**
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expires": "2025-01-14T14:00:00Z",
    "refreshToken": "Qi2LB7XvFNmlyWBd5EDWF+kqX7Ph5g2jQHXNvhPq...",
    "refreshTokenExpires": "2025-01-21T12:00:00Z",
    "user": {
      "id": 1,
      "displayName": "John Doe",
      "email": "john@example.com",
      "isEmailVerified": true,
      "isActive": true
    }
  }
}
```

### **2. Refresh Token**
`POST /api/UserAuth/refresh-token`

**Request:**
```json
{
  "refreshToken": "Qi2LB7XvFNmlyWBd5EDWF+kqX7Ph5g2jQHXNvhPq..."
}
```

**Response:**
```json
{
  "success": true,
  "message": "Tokens refreshed successfully.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expires": "2025-01-14T15:00:00Z",
    "refreshToken": "NEW_REFRESH_TOKEN_HERE...",
    "refreshTokenExpires": "2025-01-21T13:00:00Z"
  }
}
```

### **3. Revoke Token**
`POST /api/UserAuth/revoke-token`

**Request:**
```json
{
  "refreshToken": "Qi2LB7XvFNmlyWBd5EDWF+kqX7Ph5g2jQHXNvhPq..."
}
```

### **4. Revoke All Tokens** (Requires Authentication)
`POST /api/UserAuth/revoke-all-tokens`

Headers: `Authorization: Bearer <access-token>`

**Response:**
```json
{
  "success": true,
  "message": "All tokens revoked successfully."
}
```

## Client Implementation Examples

### **JavaScript/React Example**

```javascript
class TokenManager {
  constructor() {
    this.accessToken = localStorage.getItem('accessToken');
    this.refreshToken = localStorage.getItem('refreshToken');
  }

  // Store tokens after login
  setTokens(loginResponse) {
    this.accessToken = loginResponse.data.token;
    this.refreshToken = loginResponse.data.refreshToken;
    
    localStorage.setItem('accessToken', this.accessToken);
    localStorage.setItem('refreshToken', this.refreshToken);
    localStorage.setItem('tokenExpires', loginResponse.data.expires);
  }

  // Check if access token needs refresh
  async getValidAccessToken() {
    const expires = new Date(localStorage.getItem('tokenExpires'));
    const now = new Date();
    
    // Refresh if token expires in next 5 minutes
    if (expires.getTime() - now.getTime() < 5 * 60 * 1000) {
      await this.refreshAccessToken();
    }
    
    return this.accessToken;
  }

  // Refresh access token
  async refreshAccessToken() {
    try {
      const response = await fetch('/api/UserAuth/refresh-token', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: this.refreshToken })
      });

      if (response.ok) {
        const data = await response.json();
        this.setTokens(data);
        return true;
      } else {
        // Refresh failed, redirect to login
        this.logout();
        return false;
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
      this.logout();
      return false;
    }
  }

  // Logout and clean up
  async logout() {
    if (this.refreshToken) {
      // Revoke refresh token
      await fetch('/api/UserAuth/revoke-token', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: this.refreshToken })
      });
    }
    
    // Clear local storage
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenExpires');
    
    // Redirect to login
    window.location.href = '/login';
  }

  // Logout from all devices
  async logoutAllDevices() {
    await fetch('/api/UserAuth/revoke-all-tokens', {
      method: 'POST',
      headers: { 
        'Authorization': `Bearer ${this.accessToken}`,
        'Content-Type': 'application/json'
      }
    });
    
    this.logout();
  }
}

// HTTP Interceptor for automatic token refresh
const api = axios.create();

api.interceptors.request.use(async (config) => {
  const token = await tokenManager.getValidAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      const refreshed = await tokenManager.refreshAccessToken();
      if (refreshed) {
        // Retry the original request
        return api.request(error.config);
      }
    }
    return Promise.reject(error);
  }
);
```

### **Mobile App Example (React Native)**

```javascript
import AsyncStorage from '@react-native-async-storage/async-storage';

class MobileTokenManager {
  async storeTokens(loginResponse) {
    await AsyncStorage.multiSet([
      ['accessToken', loginResponse.data.token],
      ['refreshToken', loginResponse.data.refreshToken],
      ['tokenExpires', loginResponse.data.expires]
    ]);
  }

  async getValidAccessToken() {
    const [accessToken, expires] = await AsyncStorage.multiGet([
      'accessToken', 'tokenExpires'
    ]);
    
    if (new Date(expires[1]) - new Date() < 5 * 60 * 1000) {
      await this.refreshAccessToken();
      const newToken = await AsyncStorage.getItem('accessToken');
      return newToken;
    }
    
    return accessToken[1];
  }

  async logout() {
    const refreshToken = await AsyncStorage.getItem('refreshToken');
    
    if (refreshToken) {
      await fetch('/api/UserAuth/revoke-token', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken })
      });
    }
    
    await AsyncStorage.multiRemove([
      'accessToken', 'refreshToken', 'tokenExpires'
    ]);
  }
}
```

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!@#",
    "Issuer": "MarriageCalculator.API",
    "Audience": "MarriageCalculator.Users",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### appsettings.Development.json
```json
{
  "Jwt": {
    "SecretKey": "YourDevelopmentSecretKeyThatShouldBeAtLeast32CharactersLong!@#DEV",
    "Issuer": "MarriageCalculator.API.Development", 
    "Audience": "MarriageCalculator.Users.Development",
    "ExpirationMinutes": 120,
    "RefreshTokenExpirationDays": 7
  }
}
```

## Security Features

### **1. Token Rotation**
- New refresh token generated on every use
- Old refresh token immediately revoked
- Prevents token reuse attacks

### **2. Secure Token Generation**
- 64-byte cryptographically secure random tokens
- Base64 encoded for safe transmission
- Unique constraint in database

### **3. Token Validation**
- Checks expiration time
- Validates active status
- Ensures not revoked

### **4. Cleanup and Maintenance**
- Automatic expired token cleanup
- Cascade delete when user is deleted
- Proper token lifecycle management

## Testing the Implementation

### **1. Login and Get Tokens**
```bash
curl -X POST http://localhost:7294/api/UserAuth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

### **2. Use Access Token**
```bash
curl -X GET http://localhost:7294/api/Players \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### **3. Refresh Tokens**
```bash
curl -X POST http://localhost:7294/api/UserAuth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

### **4. Revoke Token**
```bash
curl -X POST http://localhost:7294/api/UserAuth/revoke-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

## Benefits

1. **✅ Improved UX** - Users stay logged in for 7 days
2. **✅ Enhanced Security** - Short-lived access tokens
3. **✅ Token Rotation** - Automatic security through rotation
4. **✅ Flexible Control** - Individual and bulk token revocation
5. **✅ Scalable** - Supports multiple devices per user
6. **✅ Audit Trail** - Token creation, usage, and revocation tracking

## Migration for Existing Clients

1. **Update login handling** to store both tokens
2. **Implement token refresh logic** before API calls
3. **Handle token refresh failures** with re-authentication
4. **Update logout** to revoke refresh tokens

Your users can now enjoy seamless 7-day sessions with enhanced security! 🎉 