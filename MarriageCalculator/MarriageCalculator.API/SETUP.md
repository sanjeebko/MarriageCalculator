# Quick Setup Guide - External Database Deployment

## ?? Quick Start (External Database)

This guide is for deploying the Marriage Calculator API using Docker with an **external hosted database**. No SQL Server container is created - the API connects to your existing database.

### 1. Copy Environment Template
```bash
cd MarriageCalculator.API
cp .env.template .env
```

### 2. Configure External Database
Open `.env` and update with your **existing database** configuration:
```env
MCDATABASE=your_external_database_server
MCUSER=your_database_username
MCPASSWORD=your_database_password
ASPNETCORE_ENVIRONMENT=Production
```

### 3. Run with Docker (Development)
```bash
# Make script executable (Linux/macOS)
chmod +x build-and-run.sh
./build-and-run.sh

# Or use PowerShell (Windows)
./build-and-run.ps1
```

### 4. Deploy to Production Server
```bash
# Set environment variables on server
export MCDATABASE=your_external_database_server
export MCUSER=your_database_username
export MCPASSWORD=your_database_password

# Deploy
chmod +x deploy-production.sh
./deploy-production.sh
```

### 5. Run for Development (No Docker)
Set environment variables in your IDE or terminal:

**Visual Studio:**
- Project Properties ? Debug ? Environment Variables
- Add: `MCDATABASE=your_server`, `MCUSER=your_username`, `MCPASSWORD=your_password`

**Command Line:**
```bash
# Linux/macOS
export MCDATABASE=your_external_database_server
export MCUSER=your_username
export MCPASSWORD=your_password
dotnet run

# Windows
set MCDATABASE=your_external_database_server
set MCUSER=your_username
set MCPASSWORD=your_password
dotnet run
```

## ??? Database Requirements

### External Database Setup
- **SQL Server** (any version supporting .NET 8)
- **Network accessible** from your Docker host
- **Database created**: `MarriageCalculator`
- **User permissions**: db_owner or sufficient create/read/write permissions

### Database Connection Examples
```env
# SQL Server on specific IP/hostname
MCDATABASE=your-sql-server.domain.com
MCUSER=your_db_user
MCPASSWORD=your_secure_password

# Azure SQL Database
MCDATABASE=yourserver.database.windows.net
MCUSER=your_azure_user@yourserver
MCPASSWORD=your_azure_password

# SQL Server with port
MCDATABASE=your-server.com,1433
MCUSER=your_user
MCPASSWORD=your_password
```

## ?? Docker Configuration

### Development (docker-compose.yml)
- **Port 5000**: HTTP API endpoint
- **Port 5001**: HTTPS API endpoint (if certificate configured)
- **External database**: No SQL Server container created
- **Health check**: Tests API and database connectivity

### Production (docker-compose.production.yml)
- **Port 80**: HTTP API endpoint
- **Port 443**: HTTPS API endpoint
- **Production environment**: Optimized logging and settings
- **External database**: Production database connection

## ?? Security Benefits

- ? No database container - uses your secure hosted database
- ? No hardcoded credentials in source code
- ? Environment-specific configuration
- ? .env files excluded from git
- ? Easy credential rotation
- ? Cloud deployment ready

## ?? Environment Support

| Environment | Configuration Method | Database |
|-------------|---------------------|----------|
| Development | `.env` file or IDE settings | External hosted |
| Docker | `docker-compose.yml` environment section | External hosted |
| Production | Server environment variables | External hosted |
| Azure | App Service Application Settings | Azure SQL or external |
| AWS | Elastic Beanstalk Environment Properties | RDS or external |

## ?? Important Notes

1. **External database required** - No SQL Server container is created
2. **All environment variables are REQUIRED** - no fallback values
3. **Database must be accessible** from Docker container network
4. **Firewall rules** may need configuration for database access
5. **SSL/TLS support** - Use TrustServerCertificate=true for development

## ?? Required Environment Variables

- **MCDATABASE**: Your external database server (e.g., "server.domain.com", "yourserver.database.windows.net")
- **MCUSER**: Database username with appropriate permissions
- **MCPASSWORD**: Database password

## ?? Common External Database Configurations

### Local Network SQL Server
```env
MCDATABASE=sql-server.local
MCUSER=api_user
MCPASSWORD=secure_password
```

### Azure SQL Database
```env
MCDATABASE=yourserver.database.windows.net
MCUSER=your_azure_user@yourserver
MCPASSWORD=your_azure_password
```

### AWS RDS SQL Server
```env
MCDATABASE=your-rds-instance.region.rds.amazonaws.com
MCUSER=admin
MCPASSWORD=your_rds_password
```

### On-Premise SQL Server
```env
MCDATABASE=sql-server.company.com
MCUSER=api_user
MCPASSWORD=secure_password
```

## ?? Health Monitoring

The API includes built-in health checks:
- **Database connectivity**: Tests connection to your external database
- **API health**: Verifies API endpoints are responding
- **Docker health**: Container-level health monitoring

**Health Check URL**: `http://localhost:5000/api/database/info`

## ?? Troubleshooting External Database

### Connection Issues
1. **Verify database server is accessible** from Docker network
2. **Check firewall rules** - ensure database port is open
3. **Test credentials** manually before Docker deployment
4. **Review connection string** format for your database type
5. **Check Docker logs**: `docker-compose logs -f`

### Common Solutions
- **SQL Server**: Enable TCP/IP and SQL Server Authentication
- **Azure SQL**: Configure firewall rules for your IP
- **Network issues**: Use IP address instead of hostname
- **SSL issues**: Add `TrustServerCertificate=true` for development