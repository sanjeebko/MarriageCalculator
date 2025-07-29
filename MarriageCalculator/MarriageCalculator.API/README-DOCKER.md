```md
# Marriage Calculator API - Docker Deployment

## ?? Quick Start

Deploy the Marriage Calculator API using Docker with your existing hosted database in 3 simple steps:

### 1. Configure Database Connection
```bash
cp .env.template .env
# Edit .env with your database details
```

### 2. Deploy with Docker
```bash
# Development
./build-and-run.sh

# Production
./deploy-production.sh
```

### 3. Access Your API
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000
- **Health Check**: http://localhost:5000/api/database/info

## ?? What's Included

### ? API-Only Deployment
- **No SQL Server container** - uses your existing database
- **Lightweight deployment** - only the API is containerized
- **External database connection** - connects to your hosted database

### ? Environment Configurations
- **Development**: `docker-compose.yml` (ports 5000/5001)
- **Production**: `docker-compose.production.yml` (ports 80/443)
- **Environment variables**: Full configuration through `.env` file

### ? Security Features
- **No hardcoded credentials** - all via environment variables
- **Non-root container** - runs with restricted user permissions
- **Health monitoring** - automatic API and database health checks

## ??? Database Requirements

Your existing database must be:
- **SQL Server** (any version supporting .NET 8)
- **Network accessible** from Docker host
- **Database named**: `MarriageCalculator`
- **User with permissions**: db_owner or create/read/write access

## ?? Required Environment Variables

```env
MCDATABASE=your_database_server    # e.g., 192.168.0.214 or server.domain.com
MCUSER=your_database_username      # Database user with appropriate permissions
MCPASSWORD=your_database_password  # Secure password for database access
```

## ?? Files Overview

| File | Purpose |
|------|---------|
| `Dockerfile` | Optimized API container configuration |
| `docker-compose.yml` | Development deployment |
| `docker-compose.production.yml` | Production deployment |
| `.env.template` | Environment variables template |
| `build-and-run.sh/.ps1` | Development deployment scripts |
| `deploy-production.sh` | Production deployment script |
| `DEPLOYMENT.md` | Comprehensive deployment guide |

## ?? Deployment Commands

### Development
```bash
# Linux/macOS
./build-and-run.sh

# Windows PowerShell
./build-and-run.ps1
```

### Production
```bash
# Set environment variables first
export MCDATABASE=your_server
export MCUSER=your_user
export MCPASSWORD=your_password

# Deploy
./deploy-production.sh
```

## ?? Health Monitoring

The API includes comprehensive health checks:
- **Container health**: Docker-level monitoring
- **API health**: Application responsiveness
- **Database connectivity**: Real-time connection testing

**Health endpoint**: `GET /api/database/info`

## ?? Quick Troubleshooting

### Check API Status
```bash
docker-compose ps
curl http://localhost:5000/api/database/info
```

### View Logs
```bash
docker-compose logs -f marriagecalculator-api
```

### Common Issues
- **Database connection**: Verify `MCDATABASE`, `MCUSER`, `MCPASSWORD`
- **Port conflicts**: Change ports in docker-compose.yml if needed
- **Firewall**: Ensure database server accepts connections from Docker host

## ?? Benefits

? **Simplified Deployment** - No database setup required  
? **Secure Configuration** - Environment-based credentials  
? **Production Ready** - Optimized for server deployment  
? **Easy Updates** - Simple rebuild and redeploy process  
? **Monitoring Built-in** - Health checks and logging included  

Your existing database remains untouched - only the API runs in Docker!
```