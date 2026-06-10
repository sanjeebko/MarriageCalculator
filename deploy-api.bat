@echo off
echo ===================================================
echo   STOPPING EXISTING MC CONTAINERS AND RELEASING PORTS
echo ===================================================
echo Stopping and removing old mc-api container if running...
docker stop mc-api >nul 2>&1
docker rm mc-api >nul 2>&1

echo Stopping and removing new marriagecalculator-api if running...
docker stop marriagecalculator-api >nul 2>&1
docker rm marriagecalculator-api >nul 2>&1

echo Running docker compose down...
docker compose down >nul 2>&1

echo ===================================================
echo   DEPLOYING NEW API CONTAINER LOCALLY
echo ===================================================
docker compose up -d

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to launch docker compose!
    exit /b %ERRORLEVEL%
)

echo.
echo ===================================================
echo   API SUCCESSFULLY DEPLOYED
echo ===================================================
echo The API is now running at: http://localhost:5000
echo.
echo Please look for your Wi-Fi IPv4 address below to configure your Android app:
echo.
ipconfig | findstr /i "ipv4"
echo ===================================================
