# Environment Variables Configuration

## Overview
The MarriageCalculator.API uses environment variables for all database configuration to enhance security and deployment flexibility. **All environment variables are required - no fallback values are provided.**

## Required Environment Variables

### Database Configuration
- **MCDATABASE**: Database server address (e.g., "your-server.domain.com", "localhost", "server-ip-address")
- **MCUSER**: Database username
- **MCPASSWORD**: Database password

## Setting Environment Variables

### Windows (Development)
```cmd
# Command Prompt
set MCDATABASE=your_database_server
set MCUSER=your_username
set MCPASSWORD=your_password

# PowerShell
$env:MCDATABASE="your_database_server"
$env:MCUSER="your_username"
$env:MCPASSWORD="your_password"
```

### Linux/macOS (Development)
```bash
export MCDATABASE=your_database_server
export MCUSER=your_username
export MCPASSWORD=your_password
```

### Visual Studio (Development)
1. Right-click on the API project
2. Select "Properties"
3. Go to "Debug" ? "General"
4. Click "Open debug launch profiles UI"
5. Add environment variables:
   - Name: `MCDATABASE`, Value: `your_database_server`
   - Name: `MCUSER`, Value: `your_username`
   - Name: `MCPASSWORD`, Value: `your_password`

### Docker Deployment
Environment variables are configured in `docker-compose.yml`:

```yaml
environment:
  - MCDATABASE=your_database_server
  - MCUSER=your_database_user
  - MCPASSWORD=your_database_password
```

### Production Server
```bash
# Add to system environment or application configuration
export MCDATABASE=your_database_server
export MCUSER=production_username
export MCPASSWORD=production_password

# Or use systemd service file
Environment=MCDATABASE=your_database_server
Environment=MCUSER=production_username
Environment=MCPASSWORD=production_password
```

### Azure App Service
1. Go to Azure Portal
2. Navigate to your App Service
3. Go to "Configuration" ? "Application settings"
4. Add new application settings:
   - `MCDATABASE`: your_database_server
   - `MCUSER`: your_username
   - `MCPASSWORD`: your_password

### AWS Elastic Beanstalk
1. Go to AWS Console
2. Navigate to your Elastic Beanstalk environment
3. Go to "Configuration" ? "Software"
4. Add environment properties:
   - `MCDATABASE`: your_database_server
   - `MCUSER`: your_username
   - `MCPASSWORD`: your_password

## Configuration Files

### appsettings.json
Contains connection string template with placeholders:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server={MCDATABASE};Database=MarriageCalculator;User Id={MCUSER};Password={MCPASSWORD};TrustServerCertificate=true;..."
  }
}
```

### .env.template
Copy this to `.env` and update with your values:
```env
MCDATABASE=your_database_server
MCUSER=your_username
MCPASSWORD=your_password
```

## Security Best Practices

1. **Never commit credentials to source control**
2. **Use different credentials for each environment**
3. **Rotate passwords regularly**
4. **Use strong passwords**
5. **Limit database user permissions**

## No Fallback Values
?? **Important**: This application does NOT provide fallback values for environment variables. All three variables (MCDATABASE, MCUSER, MCPASSWORD) must be explicitly set, or the application will fail to start with a clear error message.

## Verification
Check if environment variables are properly set:

```csharp
var server = Environment.GetEnvironmentVariable("MCDATABASE");
var user = Environment.GetEnvironmentVariable("MCUSER");
var password = Environment.GetEnvironmentVariable("MCPASSWORD");
Console.WriteLine($"Using database server: {server}, user: {user}");
```

## Common Environment Variable Patterns

### Development
```env
MCDATABASE=dev-sql-server.local
MCUSER=dev_user
MCPASSWORD=development_password
```

### Docker (Local Testing)
```env
MCDATABASE=localhost
MCUSER=test_user
MCPASSWORD=test_password
```

### Production (Example)
```env
MCDATABASE=prod-sql-server.yourdomain.com
MCUSER=api_user
MCPASSWORD=SecureProductionPassword123!
```

## Troubleshooting

### Missing Environment Variables
If any required environment variable is not set, the application will throw a clear error:
- `InvalidOperationException: MCDATABASE environment variable is required but not set.`
- `InvalidOperationException: MCUSER environment variable is required but not set.`
- `InvalidOperationException: MCPASSWORD environment variable is required but not set.`

### Connection Failures
- Verify all environment variables are set correctly
- Check database server accessibility
- Review connection string format
- Ensure database server allows connections from your IP

### Docker Issues
- Ensure all variables are defined in docker-compose.yml
- Rebuild containers after changing environment variables
- Check container logs for connection errors
- Verify database connectivity from Docker host