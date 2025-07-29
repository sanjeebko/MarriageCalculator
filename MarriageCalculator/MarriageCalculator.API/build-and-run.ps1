# Build and run Marriage Calculator API with Docker (External Database)

Write-Host "?? Marriage Calculator API - Docker Deployment" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host "Using external database - no local SQL Server container" -ForegroundColor Yellow
Write-Host ""

# Check if .env file exists, if not, create from template
if (-not (Test-Path .env)) {
    Write-Host "Creating .env file from template..." -ForegroundColor Yellow
    Copy-Item .env.template .env
    Write-Host "??  Please edit .env file with your external database configuration:" -ForegroundColor Yellow
    Write-Host "   - MCDATABASE: Your database server address" -ForegroundColor Cyan
    Write-Host "   - MCUSER: Your database username" -ForegroundColor Cyan
    Write-Host "   - MCPASSWORD: Your database password" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "Press Enter to continue after editing .env file"
}

# Load environment variables from .env file
if (Test-Path .env) {
    Get-Content .env | ForEach-Object {
        if ($_ -match "^([^#][^=]+)=(.*)$") {
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2])
        }
    }
}

# Validate required environment variables
Write-Host "Validating environment variables..." -ForegroundColor Green

if (-not $env:MCDATABASE) {
    Write-Host "? Error: MCDATABASE environment variable is required" -ForegroundColor Red
    Write-Host "   Please set your external database server address" -ForegroundColor Yellow
    exit 1
}

if (-not $env:MCUSER) {
    Write-Host "? Error: MCUSER environment variable is required" -ForegroundColor Red
    Write-Host "   Please set your database username" -ForegroundColor Yellow
    exit 1
}

if (-not $env:MCPASSWORD) {
    Write-Host "? Error: MCPASSWORD environment variable is required" -ForegroundColor Red
    Write-Host "   Please set your database password" -ForegroundColor Yellow
    exit 1
}

Write-Host "? Environment variables validated" -ForegroundColor Green
Write-Host ""

# Display database configuration
Write-Host "?? Database Configuration:" -ForegroundColor Yellow
Write-Host "   Server: $env:MCDATABASE" -ForegroundColor Cyan
Write-Host "   User: $env:MCUSER" -ForegroundColor Cyan
Write-Host "   Password: [HIDDEN]" -ForegroundColor Cyan
Write-Host ""

Write-Host "?? Building Marriage Calculator API Docker image..." -ForegroundColor Green
docker-compose build

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Docker build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "?? Starting Marriage Calculator API..." -ForegroundColor Green
docker-compose up -d

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Failed to start containers!" -ForegroundColor Red
    exit 1
}

Write-Host "? Waiting for API to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 20

Write-Host "?? Checking API health..." -ForegroundColor Green
docker-compose ps

Write-Host ""
Write-Host "? Marriage Calculator API is running!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host "?? API URL: http://localhost:5000" -ForegroundColor Cyan
Write-Host "?? Swagger UI: http://localhost:5000" -ForegroundColor Cyan
Write-Host "?? HTTPS URL: https://localhost:5001 (if certificate is configured)" -ForegroundColor Cyan
Write-Host ""
Write-Host "???  Database: External ($env:MCDATABASE)" -ForegroundColor Yellow
Write-Host "?? User: $env:MCUSER" -ForegroundColor Yellow
Write-Host ""
Write-Host "?? Management Commands:" -ForegroundColor Yellow
Write-Host "   View logs: docker-compose logs -f" -ForegroundColor Cyan
Write-Host "   Stop API: docker-compose down" -ForegroundColor Cyan
Write-Host "   Restart: docker-compose restart" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Health Check: curl http://localhost:5000/api/database/info" -ForegroundColor Magenta