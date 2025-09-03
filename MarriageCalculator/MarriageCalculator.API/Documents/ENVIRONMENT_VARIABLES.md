# Environment Variables Configuration

## Overview
The MarriageCalculator.API uses environment variables for all database and email configuration to enhance security and deployment flexibility. **All environment variables are required - no fallback values are provided.**

## Required Environment Variables

### Database Configuration
- **MCDATABASE**: Database server address (e.g., "your-server.domain.com", "localhost", "server-ip-address")
- **MCUSER**: Database username
- **MCPASSWORD**: Database password

### Email Configuration (SMTP)
- **MCSMTP**: SMTP server address (e.g., "smtp.gmail.com", "smtp.outlook.com")
- **MCMAILUSERNAME**: Email address used for sending emails
- **MCMAILPASSWORD**: Password for the email account (use app-specific password for Gmail/Outlook)

## Setting Environment Variables

### Windows (Development)
```cmd
# Command Prompt - Database
set MCDATABASE=your_database_server
set MCUSER=your_username
set MCPASSWORD=your_password

# Command Prompt - Email
set MCSMTP=smtp.gmail.com
set MCMAILUSERNAME=your_email@gmail.com
set MCMAILPASSWORD=your_app_password

# PowerShell - Database
$env:MCDATABASE="your_database_server"
$env:MCUSER="your_username"
$env:MCPASSWORD="your_password"

# PowerShell - Email
$env:MCSMTP="smtp.gmail.com"
$env:MCMAILUSERNAME="your_email@gmail.com"
$env:MCMAILPASSWORD="your_app_password"
```

### Linux/macOS (Development)
```bash
# Database
export MCDATABASE=your_database_server
export MCUSER=your_username
export MCPASSWORD=your_password

# Email
export MCSMTP=smtp.gmail.com
export MCMAILUSERNAME=your_email@gmail.com
export MCMAILPASSWORD=your_app_password
```

### Visual Studio (Development)
1. Right-click on the API project
2. Select "Properties"
3. Go to "Debug" ? "General"
4. Click "Open debug launch profiles UI"
5. Add environment variables:
   - **Database:**
     - Name: `MCDATABASE`, Value: `your_database_server`
     - Name: `MCUSER`, Value: `your_username`
     - Name: `MCPASSWORD`, Value: `your_password`
   - **Email:**
     - Name: `MCSMTP`, Value: `smtp.gmail.com`
     - Name: `MCMAILUSERNAME`, Value: `your_email@gmail.com`
     - Name: `MCMAILPASSWORD`, Value: `your_app_password`

### Docker Deployment
Environment variables are configured in `docker-compose.yml`:

```yaml
environment:
  # Database
  - MCDATABASE=your_database_server
  - MCUSER=your_database_user
  - MCPASSWORD=your_database_password
  # Email
  - MCSMTP=smtp.gmail.com
  - MCMAILUSERNAME=your_email@gmail.com
  - MCMAILPASSWORD=your_app_password
```

### Production Server
```bash
# Database
export MCDATABASE=your_database_server
export MCUSER=production_username
export MCPASSWORD=production_password

# Email
export MCSMTP=smtp.gmail.com
export MCMAILUSERNAME=production_email@domain.com
export MCMAILPASSWORD=production_app_password

# Or use systemd service file
Environment=MCDATABASE=your_database_server
Environment=MCUSER=production_username
Environment=MCPASSWORD=production_password
Environment=MCSMTP=smtp.gmail.com
Environment=MCMAILUSERNAME=production_email@domain.com
Environment=MCMAILPASSWORD=production_app_password
```

### Azure App Service
1. Go to Azure Portal
2. Navigate to your App Service
3. Go to "Configuration" ? "Application settings"
4. Add new application settings:
   - **Database:**
     - `MCDATABASE`: your_database_server
     - `MCUSER`: your_username
     - `MCPASSWORD`: your_password
   - **Email:**
     - `MCSMTP`: smtp.gmail.com
     - `MCMAILUSERNAME`: your_email@domain.com
     - `MCMAILPASSWORD`: your_app_password

### AWS Elastic Beanstalk
1. Go to AWS Console
2. Navigate to your Elastic Beanstalk environment
3. Go to "Configuration" ? "Software"
4. Add environment properties:
   - **Database:**
     - `MCDATABASE`: your_database_server
     - `MCUSER`: your_username
     - `MCPASSWORD`: your_password
   - **Email:**
     - `MCSMTP`: smtp.gmail.com
     - `MCMAILUSERNAME`: your_email@domain.com
     - `MCMAILPASSWORD`: your_app_password

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
# Database Configuration
MCDATABASE=your_database_server
MCUSER=your_username
MCPASSWORD=your_password

# Email Configuration
MCSMTP=smtp.gmail.com
MCMAILUSERNAME=your_email@gmail.com
MCMAILPASSWORD=your_app_password
```

## Common SMTP Configurations

### Gmail
```env
MCSMTP=smtp.gmail.com
MCMAILUSERNAME=your_email@gmail.com
MCMAILPASSWORD=your_app_specific_password
```
**Note:** Use App-Specific Passwords for Gmail accounts with 2FA enabled.

### Outlook/Hotmail
```env
MCSMTP=smtp.live.com
MCMAILUSERNAME=your_email@outlook.com
MCMAILPASSWORD=your_password
```

### Yahoo Mail
```env
MCSMTP=smtp.mail.yahoo.com
MCMAILUSERNAME=your_email@yahoo.com
MCMAILPASSWORD=your_app_password
```

### Custom SMTP
```env
MCSMTP=mail.yourdomain.com
MCMAILUSERNAME=noreply@yourdomain.com
MCMAILPASSWORD=your_smtp_password
```

## Security Best Practices

1. **Never commit credentials to source control**
2. **Use different credentials for each environment**
3. **Rotate passwords regularly**
4. **Use strong passwords**
5. **Limit database user permissions**
6. **Use App-Specific Passwords for email providers that support 2FA**
7. **Consider using dedicated email service accounts**

## No Fallback Values
?? **Important**: This application does NOT provide fallback values for environment variables. All variables (database and email) must be explicitly set, or the application will fail to start or send emails with clear error messages.

## Verification
Check if environment variables are properly set:

```csharp
// Database
var server = Environment.GetEnvironmentVariable("MCDATABASE");
var user = Environment.GetEnvironmentVariable("MCUSER");
var password = Environment.GetEnvironmentVariable("MCPASSWORD");

// Email
var smtpServer = Environment.GetEnvironmentVariable("MCSMTP");
var emailUser = Environment.GetEnvironmentVariable("MCMAILUSERNAME");
var emailPassword = Environment.GetEnvironmentVariable("MCMAILPASSWORD");

Console.WriteLine($"Database: {server}, Email SMTP: {smtpServer}");
```

## Common Environment Variable Patterns

### Development
```env
# Database
MCDATABASE=dev-sql-server.local
MCUSER=dev_user
MCPASSWORD=development_password

# Email
MCSMTP=smtp.gmail.com
MCMAILUSERNAME=dev.marriagecalc@gmail.com
MCMAILPASSWORD=dev_app_password
```

### Docker (Local Testing)
```env
# Database
MCDATABASE=localhost
MCUSER=test_user
MCPASSWORD=test_password

# Email
MCSMTP=smtp.gmail.com
MCMAILUSERNAME=test.marriagecalc@gmail.com
MCMAILPASSWORD=test_app_password
```

### Production (Example)
```env
# Database
MCDATABASE=prod-sql-server.yourdomain.com
MCUSER=api_user
MCPASSWORD=SecureProductionPassword123!

# Email
MCSMTP=smtp.yourdomain.com
MCMAILUSERNAME=noreply@yourdomain.com
MCMAILPASSWORD=SecureEmailPassword456!
```

## Troubleshooting

### Missing Environment Variables
If any required environment variable is not set, the application will throw clear errors:
- **Database:** `InvalidOperationException: MCDATABASE/MCUSER/MCPASSWORD environment variable is required but not set.`
- **Email:** Error logged when attempting to send emails: "SMTP configuration missing. Required environment variables: MCSMTP, MCMAILUSERNAME, MCMAILPASSWORD"

### Connection Failures
**Database:**
- Verify all environment variables are set correctly
- Check database server accessibility
- Review connection string format
- Ensure database server allows connections from your IP

**Email:**
- Verify SMTP server settings
- Check if email provider requires App-Specific Passwords
- Ensure port 587 is not blocked by firewall
- Verify SSL/TLS settings with your email provider

### Email-Specific Issues
1. **Gmail:** Enable "Less secure app access" or use App-Specific Passwords
2. **Outlook:** May require OAuth2 for production use
3. **Corporate Email:** Check with IT department for SMTP settings
4. **Port Issues:** Some networks block port 587; try port 25 or 465

### Docker Issues
- Ensure all variables are defined in docker-compose.yml
- Rebuild containers after changing environment variables
- Check container logs for connection errors
- Verify network connectivity from Docker host