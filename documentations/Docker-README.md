# MarriageCalculator API - Docker Deployment

## ?? Simple Docker Build & Push

### Prerequisites

- Docker installed and running
- Docker Hub account (sanjeebojha)
- Access to your external database
- Cloudflare account for SSL (recommended)

### Build Commands

Open the root folder where solution is located:

```bash
# Navigate to solution root (where MarriageCalculator.sln would be)
cd /path/to/MarriageCalculator/
```

### For MarriageCalculator API:

```bash
# Build Docker image (clean .NET 8 build)
docker build -f MarriageCalculator.API/Dockerfile -t sanjeebojha/marriagecalculatorapi:1.0.1 .

# Tag as latest and stable
docker tag sanjeebojha/marriagecalculatorapi:1.0.1 sanjeebojha/marriagecalculatorapi:latest
docker tag sanjeebojha/marriagecalculatorapi:1.0.1 sanjeebojha/marriagecalculatorapi:stable

# Push to Docker Hub
docker push sanjeebojha/marriagecalculatorapi:1.0.1
docker push sanjeebojha/marriagecalculatorapi:latest
docker push sanjeebojha/marriagecalculatorapi:stable
```

## ?? Running the Container

### Environment Variables Required:

```bash
export MCDATABASE=your_database_server
export MCUSER=your_database_username
export MCPASSWORD=your_database_password
export ASPNETCORE_ENVIRONMENT=Production
```

### Run Container:

```bash
# Development
docker run -d \
  -p 5000:8080 \
  -e MCDATABASE=$MCDATABASE \
  -e MCUSER=$MCUSER \
  -e MCPASSWORD=$MCPASSWORD \
  -e ASPNETCORE_ENVIRONMENT=Development \
  --name marriagecalculator-api \
  sanjeebojha/marriagecalculatorapi:latest

# Production (HTTP only - SSL handled by Cloudflare)
docker run -d \
  -p 80:8080 \
  -e MCDATABASE=$MCDATABASE \
  -e MCUSER=$MCUSER \
  -e MCPASSWORD=$MCPASSWORD \
  -e ASPNETCORE_ENVIRONMENT=Production \
  --name marriagecalculator-api \
  sanjeebojha/marriagecalculatorapi:latest
```

## ?? Verify Deployment

```bash
# Check container status
docker ps

# Check logs
docker logs marriagecalculator-api

# Health check
curl http://localhost:5000/api/database/info  # Development
curl http://localhost/api/database/info       # Production (direct)
curl https://yourdomain.com/api/database/info # Production (via Cloudflare)
```

## ?? Stop/Remove Container

```bash
# Stop container
docker stop marriagecalculator-api

# Remove container
docker rm marriagecalculator-api
```

## ?? Docker Images

- **Repository**: `sanjeebojha/marriagecalculatorapi`
- **Tags**: `1.0.1`, `latest`, `stable`
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:8.0`
- **Build Image**: `mcr.microsoft.com/dotnet/sdk:8.0` (standard .NET 8)
- **Exposed Ports**: `8080` (HTTP only)

## ??? Database Connection

The API connects to your **external database** - no database container required.

**Required Environment Variables:**

- `MCDATABASE` - Database server address
- `MCUSER` - Database username
- `MCPASSWORD` - Database password
- `ASPNETCORE_ENVIRONMENT` - Application environment (Development/Production)

## ?? Build Notes - ? SIMPLIFIED

- **Clean .NET 8 Build**: No MAUI workload installation required
- **Fast Build Time**: Standard .NET 8 SDK with no additional dependencies
- **Small Image Size**: Optimized without unnecessary workloads
- **Simple Dependencies**: Pure .NET 8 Core library
- **HTTP Only**: SSL/HTTPS handled by Cloudflare

## ?? SSL/HTTPS Configuration

### ?? **Cloudflare SSL (Recommended)**

- ? **Automatic SSL**: Cloudflare handles all SSL certificates
- ? **Zero Configuration**: No certificate setup needed
- ? **Better Performance**: No SSL overhead on container
- ? **DDoS Protection**: Built-in security features
- ? **Global CDN**: Improved performance worldwide

### **Production Architecture**

```
[Users] ? [Cloudflare SSL] ? [Your Server:80] ? [Container:8080]
 HTTPS       SSL Termination      HTTP           HTTP
```

## ?? Quick Commands Reference

```bash
# Build (clean and fast)
docker build -f MarriageCalculator.API/Dockerfile -t sanjeebojha/marriagecalculatorapi:1.0.1 .

# Tag
docker tag sanjeebojha/marriagecalculatorapi:1.0.1 sanjeebojha/marriagecalculatorapi:latest

# Push
docker push sanjeebojha/marriagecalculatorapi:1.0.1
docker push sanjeebojha/marriagecalculatorapi:latest

# Run Production (HTTP only)
docker run -d -p 80:8080 -e MCDATABASE=your_server -e MCUSER=your_user -e MCPASSWORD=your_password -e ASPNETCORE_ENVIRONMENT=Production sanjeebojha/marriagecalculatorapi:latest
```

## ?? Access Points

### **Production (via Cloudflare)**

- **API**: https://yourdomain.com
- **Swagger UI**: https://yourdomain.com/swagger
- **Health Check**: https://yourdomain.com/api/database/info

### **Development**

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/api/database/info

### **Direct Server Access**

- **API**: http://your-server-ip
- **Health Check**: http://your-server-ip/api/database/info

## ?? Performance Improvements

### Benefits of Cloudflare SSL

- ? **No SSL overhead**: Container serves HTTP only
- ? **Faster startup**: No certificate loading
- ? **Simpler configuration**: No SSL environment variables
- ? **Automatic renewal**: Cloudflare manages certificates
- ? **Global performance**: CDN and edge locations

## ?? Troubleshooting

### Build Issues

- **Clean .NET 8 build**: No MAUI workload dependencies
- **Fast builds**: Standard SDK without complex workloads

### Runtime Issues

- **Database Connection**: Verify environment variables
- **Port Access**: Ensure port 80 is available
- **Cloudflare**: Check DNS and proxy settings
- **Container Logs**: `docker logs marriagecalculator-api`

### Cloudflare Setup

- **DNS**: A record pointing to your server
- **Proxy**: Orange cloud enabled
- **SSL Mode**: "Flexible" or "Full"
- **HTTPS Redirect**: Enabled

The Docker deployment is optimized for Cloudflare SSL with simplified configuration! ??
