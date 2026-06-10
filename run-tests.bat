@echo off
echo ===================================================
echo   RUNNING BACKEND UNIT & ISOLATION TESTS (C#)
echo ===================================================
dotnet test MarriageCalculator/MarriageCalculator.sln
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] C# backend tests failed!
    exit /b %ERRORLEVEL%
)

echo.
echo ===================================================
echo   RUNNING FRONTEND UNIT TESTS (ANDROID KOTLIN)
echo ===================================================
cd MarriageCalculator\Android
call gradlew.bat testDebugUnitTest
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Android tests failed!
    cd ..\..
    exit /b %ERRORLEVEL%
)
cd ..\..

echo.
echo ===================================================
echo   ALL TESTS PASSED SUCCESSFULLY!
echo ===================================================
