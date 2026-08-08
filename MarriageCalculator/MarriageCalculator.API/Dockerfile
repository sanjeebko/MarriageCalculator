# Use the official ASP.NET Core runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for dependency restoration
COPY ["MarriageCalculator.API/MarriageCalculator.API.csproj", "MarriageCalculator.API/"]
COPY ["MarriageCalculator.Core/MarriageCalculator.Core.csproj", "MarriageCalculator.Core/"]

# Restore dependencies
RUN dotnet restore "MarriageCalculator.API/MarriageCalculator.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/MarriageCalculator.API"
RUN dotnet build "MarriageCalculator.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MarriageCalculator.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage - create the runtime image
FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Health check for Kubernetes probes
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health/ready || exit 1

ENTRYPOINT ["dotnet", "MarriageCalculator.API.dll"]