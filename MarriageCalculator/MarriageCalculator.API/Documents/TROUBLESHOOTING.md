# SQL Server Connection Troubleshooting Guide

## Current Error Analysis
```
SqlException: Cannot open database "MarriageCalculatorDB" requested by the login. The login failed.
Login failed for user 'mcuser'.
```

## Root Causes and Solutions

### 1. **Database Does Not Exist**
**Problem**: The database `MarriageCalculatorDB` might not exist on the SQL Server.

**Solution**:
```sql
-- Connect to SQL Server as administrator and run:
CREATE DATABASE [MarriageCalculatorDB]
```

### 2. **User/Login Does Not Exist**
**Problem**: The login `mcuser` might not exist on the SQL Server.

**Solution**: Run the `DatabaseSetup.sql` script provided in the Scripts folder.

### 3. **Permission Issues**
**Problem**: User exists but doesn't have access to the database.

**Solution**:
```sql
USE [MarriageCalculatorDB]
ALTER ROLE [db_owner] ADD MEMBER [mcuser]
```

## Step-by-Step Resolution

### Step 1: Verify SQL Server Connectivity
Test if you can connect to the SQL Server at all:

```bash
# Using sqlcmd (if available)
sqlcmd -S 192.168.0.214 -U sa -P [sa_password]

# Or use SQL Server Management Studio (SSMS)
# Server: 192.168.0.214
# Authentication: SQL Server Authentication
# Login: sa (or another admin account)
```

### Step 2: Check SQL Server Configuration
Ensure SQL Server is configured to accept connections:

1. **SQL Server Configuration Manager**:
   - Enable TCP/IP protocol
   - Ensure SQL Server is listening on port 1433
   - Restart SQL Server service

2. **SQL Server Authentication**:
   - Must be set to "SQL Server and Windows Authentication mode"
   - Check in SSMS: Server Properties ? Security

### Step 3: Run Database Setup Script
Execute the `DatabaseSetup.sql` script as a SQL Server administrator:

```sql
-- This script will:
-- 1. Create the MarriageCalculatorDB database
-- 2. Create the mcuser login
-- 3. Create the mcuser database user
-- 4. Grant necessary permissions
```

### Step 4: Test the Connection
Use the API endpoint to test connectivity:

```bash
GET /api/database/test-connection
```

### Step 5: Alternative Connection Strings
If the current connection fails, try these alternatives:

#### Option 1: Windows Authentication (if applicable)
```json
{
  "DefaultConnection": "Server=192.168.0.214;Database=MarriageCalculatorDB;Integrated Security=true;TrustServerCertificate=true;"
}
```

#### Option 2: Different Port
```json
{
  "DefaultConnection": "Server=192.168.0.214,1433;Database=MarriageCalculatorDB;User Id=mcuser;Password=Scorpions18;TrustServerCertificate=true;"
}
```

#### Option 3: Named Instance
```json
{
  "DefaultConnection": "Server=192.168.0.214\\SQLEXPRESS;Database=MarriageCalculatorDB;User Id=mcuser;Password=Scorpions18;TrustServerCertificate=true;"
}
```

## Enhanced Error Logging

The updated `Program.cs` now includes detailed error logging that will help identify the specific issue:

- **Error 2**: Connection timeout
- **Error 18456**: Login failed
- **Error 1225**: Database access denied

## Manual Database Creation (Alternative)

If automated creation fails, manually create the database:

### Using SSMS:
1. Connect to SQL Server as administrator
2. Right-click "Databases" ? "New Database"
3. Name: `MarriageCalculatorDB`
4. Click OK

### Create Login:
1. Expand "Security" ? "Logins"
2. Right-click ? "New Login"
3. Login name: `mcuser`
4. Authentication: SQL Server authentication
5. Password: `Scorpions18`
6. Default database: `MarriageCalculatorDB`

### Grant Permissions:
1. Right-click the `mcuser` login ? "Properties"
2. Go to "User Mapping"
3. Check `MarriageCalculatorDB`
4. Select `db_owner` role
5. Click OK

## Testing Commands

### Test Basic Connectivity:
```bash
telnet 192.168.0.214 1433
```

### Test with Entity Framework:
```bash
dotnet ef database update
```

### Test API Connection:
```bash
curl https://localhost:7000/api/database/test-connection
```

## Common Firewall Issues

Ensure these ports are open:
- **1433**: Default SQL Server port
- **1434**: SQL Server Browser (for named instances)

## Configuration Verification

After setup, verify these settings in SQL Server:

1. **SQL Server Authentication**: Enabled
2. **TCP/IP Protocol**: Enabled
3. **SQL Server Browser Service**: Running (if using named instances)
4. **Windows Firewall**: Allows SQL Server traffic

## Next Steps

1. Run the `DatabaseSetup.sql` script
2. Test the connection using the API endpoint
3. Check the enhanced error logs in the application
4. If issues persist, verify SQL Server configuration and network connectivity

The enhanced error handling in the updated code will provide more specific information about what's failing, making it easier to diagnose and fix the issue.