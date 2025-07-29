#!/bin/bash

# Build and run Marriage Calculator API with Docker (External Database)

echo "?? Marriage Calculator API - Docker Deployment"
echo "================================================"
echo "Using external database - no local SQL Server container"
echo ""

# Check if .env file exists, if not, create from template
if [ ! -f .env ]; then
    echo "Creating .env file from template..."
    cp .env.template .env
    echo "??  Please edit .env file with your external database configuration:"
    echo "   - MCDATABASE: Your database server address"
    echo "   - MCUSER: Your database username"
    echo "   - MCPASSWORD: Your database password"
    echo ""
    read -p "Press Enter to continue after editing .env file..."
fi

# Load environment variables from .env file
if [ -f .env ]; then
    export $(cat .env | grep -v '^#' | xargs)
fi

# Validate required environment variables
echo "Validating environment variables..."

if [ -z "$MCDATABASE" ]; then
    echo "? Error: MCDATABASE environment variable is required"
    echo "   Please set your external database server address"
    exit 1
fi

if [ -z "$MCUSER" ]; then
    echo "? Error: MCUSER environment variable is required"
    echo "   Please set your database username"
    exit 1
fi

if [ -z "$MCPASSWORD" ]; then
    echo "? Error: MCPASSWORD environment variable is required"
    echo "   Please set your database password"
    exit 1
fi

echo "? Environment variables validated"
echo ""

# Test database connectivity (optional)
echo "?? Database Configuration:"
echo "   Server: ${MCDATABASE}"
echo "   User: ${MCUSER}"
echo "   Password: [HIDDEN]"
echo ""

echo "?? Building Marriage Calculator API Docker image..."
docker-compose build

if [ $? -ne 0 ]; then
    echo "? Docker build failed!"
    exit 1
fi

echo "?? Starting Marriage Calculator API..."
docker-compose up -d

if [ $? -ne 0 ]; then
    echo "? Failed to start containers!"
    exit 1
fi

echo "? Waiting for API to be ready..."
sleep 20

echo "?? Checking API health..."
docker-compose ps

echo ""
echo "? Marriage Calculator API is running!"
echo "======================================"
echo "?? API URL: http://localhost:5000"
echo "?? Swagger UI: http://localhost:5000"
echo "?? HTTPS URL: https://localhost:5001 (if certificate is configured)"
echo ""
echo "???  Database: External (${MCDATABASE})"
echo "?? User: ${MCUSER}"
echo ""
echo "?? Management Commands:"
echo "   View logs: docker-compose logs -f"
echo "   Stop API: docker-compose down"
echo "   Restart: docker-compose restart"
echo ""
echo "?? Health Check: curl http://localhost:5000/api/database/info"