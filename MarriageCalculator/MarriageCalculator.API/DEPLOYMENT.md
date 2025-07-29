```md
# Marriage Calculator API - Docker Deployment Guide

## ?? Overview

This guide covers deploying the Marriage Calculator API using Docker with an external database. The API is containerized while your database remains on your existing hosted infrastructure.

## ??? Architecture

```
???????????????????     ???????????????????????
?   Docker Host   ???????  External Database  ?
?                 ?     ?                     ?
? ??????????????? ?     ?   SQL Server        ?
? ? API Container? ?     ?   (Your hosted DB)  ?
? ?             ? ?     ?                     ?
? ? Port 5000   ? ?     ?   Port 1433         ?
? ? Port 5001   ? ?     ?                     ?
? ??????????????? ?     ???????????????????????
???????????????????
```

## ?? Quick Deployment

### Development Environment
```bash
# 1. Clone and navigate
cd MarriageCalculator.API

# 2. Configure database
cp .env.template .env
# Edit .env with your database details

# 3. Deploy
./build-and-run.sh
```

### Production Server
```bash
# 1. Set environment variables
export MCDATABASE=your_database_server
export MCUSER=your_database_user
export MCPASSWORD=your_database_password

# 2. Deploy
./deploy-production.sh
```

## ?? Configuration Files

### docker-compose.yml (Development)
- **Ports**: 5000 (HTTP), 5001 (HTTPS)
- **Environment**: Development/Testing
- **Health Check**: API and database connectivity

### docker-compose.production.yml (Production)
- **Ports**: 80 (HTTP), 443 (HTTPS)
- **Environment**: Production optimized
- **Security**: Enhanced configuration

## ?? Deployment Environments

### 1. Local Development
```bash
# Quick start with Docker
./build-and-run.sh

# Access API
curl http://localhost:5000/api/database/info
```

### 2. Linux Server Deployment
```bash
# Install Docker and Docker Compose
sudo apt update
sudo apt install docker.io docker-compose

# Set environment variables
export MCDATABASE=your_db_server
export MCUSER=your_db_user
export MCPASSWORD=your_db_password

# Deploy
./deploy-production.sh
```

### 3. Cloud Deployment (Azure VM/AWS EC2)
```bash
# Install Docker
curl -fsSL https://get.docker.com | sh

# Set environment variables (persistent)
echo "export MCDATABASE=your_db_server" >> ~/.bashrc
echo "export MCUSER=your_db_user" >> ~/.bashrc
echo "export MCPASSWORD=your_db_password" >> ~/.bashrc
source ~/.bashrc

# Deploy
./deploy-production.sh
```

### 4. Docker Swarm (Production Cluster)
```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.production.yml marriagecalculator
```

## ?? Security Configuration

### Environment Variables
```env
# Required for all environments
MCDATABASE=your_external_database_server
MCUSER=your_database_username
MCPASSWORD=your_secure_password

# Optional SSL configuration
CERT_PASSWORD=your_certificate_password
```

### Network Security
- **Database Access**: Ensure your database accepts connections from Docker host
- **Firewall Rules**: Open necessary ports (1433 for SQL Server)
- **SSL/TLS**: Configure for production environments

## ?? Monitoring and Health Checks

### Built-in Health Checks
- **API Health**: `http://localhost:5000/api/database/info`
- **Docker Health**: Automatic container health monitoring
- **Database Connectivity**: Real-time connection testing

### Monitoring Commands
```bash
# Check container status
docker-compose ps

# View logs
docker-compose logs -f

# Monitor resource usage
docker stats

# Health check
curl -f http://localhost:5000/api/database/info
```

## ?? Management Commands

### Development
```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# Restart services
docker-compose restart

# View logs
docker-compose logs -f

# Rebuild and restart
docker-compose up --build -d
```

### Production
```bash
# Deploy/Update
./deploy-production.sh

# Stop services
docker-compose -f docker-compose.production.yml down

# View logs
docker-compose -f docker-compose.production.yml logs -f

# Scale (if using swarm)
docker service scale marriagecalculator_api=3
```

## ?? Troubleshooting

### Common Issues

#### Database Connection Failed
```bash
# Check environment variables
env | grep MC

# Test database connectivity
telnet your_database_server 1433

# Check Docker logs
docker-compose logs marriagecalculator-api
```

#### Container Won't Start
```bash
# Check Docker status
systemctl status docker

# Verify ports are available
netstat -tulpn | grep :5000

# Check resource usage
df -h
free -m
```

#### API Not Responding
```bash
# Check container health
docker-compose ps

# View application logs
docker-compose logs -f marriagecalculator-api

# Test internal connectivity
docker exec -it marriagecalculator-api curl localhost:8080/api/database/info
```

## ?? Update and Maintenance

### Update API
```bash
# Pull latest code
git pull origin main

# Rebuild and deploy
./deploy-production.sh
```

### Backup Strategy
- **Database**: Use your existing database backup procedures
- **Configuration**: Backup .env files and docker-compose configurations
- **Container Data**: No persistent data in containers

### Performance Optimization
- **Resource Limits**: Configure memory and CPU limits in docker-compose
- **Connection Pooling**: Configured automatically in Entity Framework
- **Health Checks**: Monitor API performance and database connectivity

## ?? Support

### Logs Location
- **Application Logs**: `docker-compose logs marriagecalculator-api`
- **System Logs**: `/var/log/docker` (Linux)
- **Health Status**: `http://localhost:5000/api/database/info`

### Emergency Procedures
```bash
# Quick restart
docker-compose restart

# Emergency stop
docker-compose down

# Force rebuild
docker-compose build --no-cache
docker-compose up -d
```
```