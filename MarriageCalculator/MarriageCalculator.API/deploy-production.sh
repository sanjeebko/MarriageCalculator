#!/bin/bash

# Production deployment script for Marriage Calculator API
# For use on production servers with external database

set -e

echo "?? Marriage Calculator API - Production Deployment"
echo "=================================================="
echo ""

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
echo "?? Checking prerequisites..."

if ! command_exists docker; then
    echo "? Docker is not installed. Please install Docker first."
    exit 1
fi

if ! command_exists docker-compose; then
    echo "? Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

echo "? Prerequisites met"
echo ""

# Validate environment variables
echo "?? Validating environment variables..."

if [ -z "$MCDATABASE" ]; then
    echo "? Error: MCDATABASE environment variable is required"
    echo "   Set it with: export MCDATABASE=your_database_server"
    exit 1
fi

if [ -z "$MCUSER" ]; then
    echo "? Error: MCUSER environment variable is required"
    echo "   Set it with: export MCUSER=your_database_user"
    exit 1
fi

if [ -z "$MCPASSWORD" ]; then
    echo "? Error: MCPASSWORD environment variable is required"
    echo "   Set it with: export MCPASSWORD=your_database_password"
    exit 1
fi

echo "? Environment variables validated"
echo ""

# Display configuration (without sensitive data)
echo "?? Production Configuration:"
echo "   Database Server: ${MCDATABASE}"
echo "   Database User: ${MCUSER}"
echo "   Environment: Production"
echo "   SSL/HTTPS: Handled by Cloudflare"
echo ""

# Build and deploy
echo "?? Building production image..."
docker-compose -f docker-compose.production.yml build --no-cache

if [ $? -ne 0 ]; then
    echo "? Build failed!"
    exit 1
fi

echo "?? Stopping existing containers..."
docker-compose -f docker-compose.production.yml down

echo "?? Starting production deployment..."
docker-compose -f docker-compose.production.yml up -d

if [ $? -ne 0 ]; then
    echo "? Deployment failed!"
    exit 1
fi

echo "? Waiting for API to initialize..."
sleep 30

# Health check
echo "?? Performing health check..."
HEALTH_URL="http://localhost/api/database/info"

for i in {1..5}; do
    if curl -f -s "$HEALTH_URL" > /dev/null; then
        echo "? Health check passed!"
        break
    else
        echo "? Attempt $i/5 - Waiting for API..."
        sleep 10
    fi
    
    if [ $i -eq 5 ]; then
        echo "? Health check failed after 5 attempts"
        echo "?? Check logs with: docker-compose -f docker-compose.production.yml logs"
        exit 1
    fi
done

echo ""
echo "? Marriage Calculator API deployed successfully!"
echo "=============================================="
echo "?? API URL: http://localhost"
echo "?? HTTPS: Handled by Cloudflare (external)"
echo "?? Swagger UI: http://localhost"
echo "???  Database: External (${MCDATABASE})"
echo ""
echo "?? Management Commands:"
echo "   View logs: docker-compose -f docker-compose.production.yml logs -f"
echo "   Stop API: docker-compose -f docker-compose.production.yml down"
echo "   Restart: docker-compose -f docker-compose.production.yml restart"
echo "   Update: ./deploy-production.sh"
echo ""
echo "?? Health Check: curl http://localhost/api/database/info"
echo ""
echo "??  Note: SSL/HTTPS is handled by Cloudflare - container serves HTTP only"