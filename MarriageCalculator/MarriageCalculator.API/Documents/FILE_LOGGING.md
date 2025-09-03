# File Logging System Documentation

## Overview

The MarriageCalculator API now uses **Serilog** for comprehensive logging to both console and files. The logging system automatically creates daily log files with detailed information about API operations, errors, and system events.

## Log File Location and Naming

### **?? Log Directory**
```
MarriageCalculator.API/
??? Logs/
    ??? log_2025-01-30.log
    ??? log_2025-01-31.log
    ??? log_2025-02-01.log
    ??? log_[current-date].log
```

### **?? File Naming Convention**
- **Format**: `log_YYYY-MM-DD.log`
- **Examples**: 
  - `log_2025-01-30.log`
  - `log_2025-02-01.log`
  - `log_2025-12-25.log`

### **?? Rolling Behavior**
- **New file created**: Every day at midnight
- **File size limit**: 10 MB per file (50 MB in development)
- **Retention**: 30 days (7 days in development)
- **Automatic cleanup**: Old files deleted automatically

## Log Entry Format

### **?? Standard Format**
```
[2025-01-30 14:32:15.123 +05:00 INF] MarriageCalculator.API.Controllers.EmailTestController: Testing verification email to: test@example.com
[2025-01-30 14:32:16.456 +05:00 ERR] MarriageCalculator.API.Services.EmailService: SMTP configuration missing. Required environment variables: MCSMTP, MCMAILUSERNAME, MCMAILPASSWORD
```

### **??? Log Level Indicators**
- **DBG** - Debug (Development only)
- **INF** - Information
- **WRN** - Warning
- **ERR** - Error
- **FTL** - Fatal

## What Gets Logged

### **?? HTTP Requests**
```
[2025-01-30 14:30:45.123 +05:00 INF] HTTP POST /api/EmailTest/send-verification responded 500 in 1234.5678 ms
```

### **?? Authentication Events**
```
[2025-01-30 14:31:22.456 +05:00 INF] MarriageCalculator.API.Controllers.UserAuthController: User login attempt for email: user@example.com
[2025-01-30 14:31:23.789 +05:00 WRN] MarriageCalculator.API.Services.UserAuthService: Login failed - email not verified for: user@example.com
```

### **?? Email Operations**
```
[2025-01-30 14:32:10.123 +05:00 INF] MarriageCalculator.API.Services.EmailService: Sending email - To: test@example.com, Subject: Verify Your Email Address
[2025-01-30 14:32:15.456 +05:00 INF] MarriageCalculator.API.Services.EmailService: Email sent successfully to test@example.com
[2025-01-30 14:32:20.789 +05:00 ERR] MarriageCalculator.API.Services.EmailService: Failed to send email to test@example.com
System.Net.Mail.SmtpException: Authentication failed
```

### **??? Database Operations**
```
[2025-01-30 14:25:30.123 +05:00 INF] MarriageCalculator.API.Data.MarriageCalculatorDbContext: Executed DbCommand (25ms) [Parameters=[@p0='?' (Size = 100), @p1='?' (Size = 255)], CommandType='Text', CommandTimeout='60']
INSERT INTO [User] ([DisplayName], [Email]) VALUES (@p0, @p1)
```

### **?? Errors and Exceptions**
```
[2025-01-30 14:35:45.123 +05:00 ERR] MarriageCalculator.API.Controllers.EmailTestController: Error sending test verification email to test@example.com
System.ArgumentException: Invalid email configuration
   at MarriageCalculator.API.Services.EmailService.SendEmailAsync(String toEmail, String subject, String htmlBody)
   at MarriageCalculator.API.Controllers.EmailTestController.SendTestVerificationEmail(EmailTestRequest request)
```

## Log Levels and Configuration

### **??? Production (appsettings.json)**
- **Default Level**: Information
- **Microsoft**: Warning
- **EntityFramework**: Information
- **File Retention**: 30 days
- **File Size Limit**: 10 MB

### **?? Development (appsettings.Development.json)**
- **Default Level**: Debug
- **All Components**: More verbose logging
- **EmailService**: Debug level for troubleshooting
- **File Retention**: 7 days
- **File Size Limit**: 50 MB

## Finding Specific Information

### **?? Email Issues**
Search for:
```
EmailService
SMTP configuration missing
Failed to send email
Authentication failed
```

### **?? Authentication Problems**
Search for:
```
UserAuthController
Login failed
Invalid credentials
Email not verified
JWT
```

### **?? API Requests**
Search for:
```
HTTP POST
HTTP GET
responded 500
responded 400
```

### **?? Database Issues**
Search for:
```
MarriageCalculatorDbContext
DbCommand
Database initialization
Connection failed
```

## Accessing Log Files

### **?? File Location**
The log files are created in the `Logs` folder within the API application directory:
```
G:\workspace\MarriageCalculator\13\MarriageCalculator\MarriageCalculator\MarriageCalculator.API\Logs\
```

### **?? Reading Logs**
You can open log files with:
- **Notepad++** (with syntax highlighting)
- **Visual Studio Code**
- **Any text editor**
- **PowerShell**: `Get-Content "Logs\log_2025-01-30.log" -Tail 50`
- **Command Line**: `tail -f Logs/log_2025-01-30.log` (on Linux/Mac)

### **?? Searching Logs**
```powershell
# Find all email-related errors
Select-String -Path "Logs\*.log" -Pattern "EmailService.*ERR"

# Find authentication issues
Select-String -Path "Logs\*.log" -Pattern "Login failed"

# Find HTTP 500 errors
Select-String -Path "Logs\*.log" -Pattern "responded 500"
```

## Log File Management

### **?? Automatic Cleanup**
- Files older than retention period are automatically deleted
- No manual intervention required
- Configurable retention periods

### **?? Backup Considerations**
- Log files contain sensitive information
- Consider log aggregation services for production
- Regular backup of log files recommended

### **?? Log Analysis Tools**
For production environments, consider:
- **Seq** - Structured log analysis
- **ELK Stack** (Elasticsearch, Logstash, Kibana)
- **Splunk** - Enterprise log management
- **Azure Log Analytics** - Cloud-based analysis

## Troubleshooting Email Issues

With the new logging system, when you get an email error, follow these steps:

### **Step 1: Check Latest Log File**
```powershell
# Get today's log file
$logFile = "Logs\log_$(Get-Date -Format 'yyyy-MM-dd').log"
Get-Content $logFile -Tail 20
```

### **Step 2: Search for Email Errors**
```powershell
# Find all email-related errors in today's log
Select-String -Path $logFile -Pattern "EmailService.*ERR"
```

### **Step 3: Check SMTP Configuration**
Look for this specific error:
```
SMTP configuration missing. Required environment variables: MCSMTP, MCMAILUSERNAME, MCMAILPASSWORD
```

### **Step 4: Check Authentication Errors**
Look for:
```
Authentication failed
The SMTP server requires a secure connection
```

## Configuration Examples

### **Basic File Logging (Production)**
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log_.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

### **Enhanced Logging (Development)**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "MarriageCalculator.API.Services.EmailService": "Debug"
      }
    }
  }
}
```

## Benefits of File Logging

### **? Persistent Storage**
- Logs survive application restarts
- Historical data for troubleshooting
- Audit trail for compliance

### **?? Better Debugging**
- Detailed error messages with stack traces
- Request/response correlation
- Performance metrics

### **?? Easy Troubleshooting**
- Search across multiple days
- Pattern recognition for recurring issues
- Detailed email delivery tracking

### **?? Monitoring and Alerts**
- Log file monitoring tools
- Automated error detection
- Performance trend analysis

---

**Now you can easily track and debug email issues and other API problems using the comprehensive file logging system!** ???