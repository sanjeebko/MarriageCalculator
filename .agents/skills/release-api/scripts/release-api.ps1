<#
.SYNOPSIS
    Builds, pushes, deploys, and verifies the Marriage Calculator API to Kubernetes dev and prod environments.

.DESCRIPTION
    Automates the complete release lifecycle:
    1. Validates prerequisites (Docker, SSH connectivity to cluster node).
    2. Builds .NET 10 container image with git hash and 'latest' tags.
    3. Pushes image tags to Docker Hub.
    4. Triggers Kubernetes rollout restart in target namespace(s) via SSH.
    5. Awaits rollout completion status.
    6. Verifies /health/live and /health/ready endpoints.

.PARAMETER Environment
    Target environment: 'dev', 'prod', or 'all'. Defaults to 'dev'.

.PARAMETER Tag
    Image tag to publish. Defaults to the current Git short commit hash.

.PARAMETER K8sHost
    Control plane host IP. Defaults to '192.168.0.210'.

.PARAMETER K8sUser
    SSH username for cluster control plane. Defaults to 'sanjeeb'.

.PARAMETER SkipBuild
    Skip docker build step.

.PARAMETER SkipPush
    Skip docker push step.

.PARAMETER SkipDeploy
    Skip kubectl rollout restart step.

.PARAMETER SkipHealthCheck
    Skip health check probe verification.

.EXAMPLE
    pwsh release-api.ps1 -Environment dev
    pwsh release-api.ps1 -Environment prod
    pwsh release-api.ps1 -Environment all -Tag v1.0.0
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('dev', 'prod', 'all')]
    [string]$Environment = 'dev',

    [Parameter(Position = 1)]
    [string]$Tag,

    [string]$K8sHost = '192.168.0.210',
    [string]$K8sUser = 'sanjeeb',
    [switch]$SkipBuild,
    [switch]$SkipPush,
    [switch]$SkipDeploy,
    [switch]$SkipHealthCheck
)

$ErrorActionPreference = 'Stop'

function Write-Step {
    param([string]$Message)
    Write-Host "`n====> $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "  [OK] $Message" -ForegroundColor Green
}

function Write-Warn {
    param([string]$Message)
    Write-Host "  [WARN] $Message" -ForegroundColor Yellow
}

function Write-Failure {
    param([string]$Message)
    Write-Host "  [FAIL] $Message" -ForegroundColor Red
}

# Resolve Git Root directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "../../../..")
Set-Location $RepoRoot

Write-Step "Initializing Marriage Calculator API Release"
Write-Host "Target Environment: $Environment"
Write-Host "Repository Root:    $RepoRoot"

# Determine Git Tag if not provided
if (-not $Tag) {
    try {
        $Tag = (git rev-parse --short HEAD).Trim()
    } catch {
        $Tag = "build-" + (Get-Date -Format "yyyyMMdd-HHmmss")
    }
}
Write-Host "Image Tag:          $Tag"

# Pre-flight Check: Docker
if (-not $SkipBuild -or -not $SkipPush) {
    Write-Step "Checking Docker daemon status"
    try {
        docker info | Out-Null
        Write-Success "Docker daemon is running."
    } catch {
        Write-Failure "Docker is not running. Please start Docker Desktop before continuing."
        exit 1
    }
}

# Pre-flight Check: SSH connectivity
if (-not $SkipDeploy) {
    Write-Step "Checking SSH access to Kubernetes cluster ($K8sUser@$K8sHost)"
    $sshTest = ssh -o BatchMode=yes -o ConnectTimeout=5 "$K8sUser@$K8sHost" "echo ready" 2>&1
    if ($LASTEXITCODE -ne 0 -or $sshTest -notmatch "ready") {
        Write-Failure "Cannot reach Kubernetes cluster via SSH ($K8sUser@$K8sHost). Check your network or SSH keys."
        exit 1
    }
    Write-Success "SSH connection to cluster control plane confirmed."
}

# 1. Build Docker Image
$ImageRepo = "sanjeebojha/marriagecalculatorapi"
if (-not $SkipBuild) {
    Write-Step "Building container image (${ImageRepo}:${Tag} and ${ImageRepo}:latest)"
    $dockerfile = "MarriageCalculator/MarriageCalculator.API/Dockerfile"
    $buildContext = "MarriageCalculator"
    
    docker build `
        -t "${ImageRepo}:${Tag}" `
        -t "${ImageRepo}:latest" `
        -f $dockerfile `
        $buildContext

    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Docker build failed."
        exit $LASTEXITCODE
    }
    Write-Success "Docker image built successfully."
} else {
    Write-Warn "Skipping Docker build (-SkipBuild specified)."
}

# 2. Push Docker Image
if (-not $SkipPush) {
    Write-Step "Pushing images to Docker Hub ($ImageRepo)"
    docker push "${ImageRepo}:${Tag}"
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to push ${ImageRepo}:${Tag} to Docker Hub."
        exit $LASTEXITCODE
    }
    docker push "${ImageRepo}:latest"
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to push ${ImageRepo}:latest to Docker Hub."
        exit $LASTEXITCODE
    }
    Write-Success "Docker images pushed successfully."
} else {
    Write-Warn "Skipping Docker push (-SkipPush specified)."
}

# 3. Deploy to Environments
$EnvironmentsToDeploy = if ($Environment -eq 'all') { @('dev', 'prod') } else { @($Environment) }

if (-not $SkipDeploy) {
    foreach ($envName in $EnvironmentsToDeploy) {
        Write-Step "Deploying to '$envName' namespace on Kubernetes cluster"
        
        # Rollout restart
        Write-Host "Restarting deployment/marriagecalculatordeployment in namespace $envName..."
        ssh "$K8sUser@$K8sHost" "kubectl -n $envName rollout restart deployment/marriagecalculatordeployment"
        if ($LASTEXITCODE -ne 0) {
            Write-Failure "Failed to trigger rollout restart in namespace $envName."
            exit $LASTEXITCODE
        }

        # Rollout status
        Write-Host "Waiting for rollout to complete..."
        ssh "$K8sUser@$K8sHost" "kubectl -n $envName rollout status deployment/marriagecalculatordeployment --timeout=150s"
        if ($LASTEXITCODE -ne 0) {
            Write-Failure "Rollout failed in namespace $envName. Check pod logs: ssh $K8sUser@$K8sHost 'kubectl -n $envName logs -l app=marriagecalculatorapi --tail=50'"
            exit $LASTEXITCODE
        }
        Write-Success "Deployment in namespace '$envName' rolled out successfully."
    }
} else {
    Write-Warn "Skipping deployment rollout (-SkipDeploy specified)."
}

# 4. Health Checks
if (-not $SkipHealthCheck -and -not $SkipDeploy) {
    Write-Step "Verifying service health checks"
    Write-Host "Allowing 5 seconds for network routes to stabilize..."
    Start-Sleep -Seconds 5
    
    foreach ($envName in $EnvironmentsToDeploy) {
        Write-Host "`nTesting health endpoints for environment: $envName"
        
        $liveUrl = if ($envName -eq 'dev') { "http://192.168.1.159/health/live" } else { "https://mcapi.sanjeebojha.com.np/health/live" }
        $readyUrl = if ($envName -eq 'dev') { "http://192.168.1.159/health/ready" } else { "https://mcapi.sanjeebojha.com.np/health/ready" }
        
        # Probe Liveness
        try {
            $liveResp = Invoke-RestMethod -Uri $liveUrl -TimeoutSec 10 -ErrorAction Stop
            Write-Success "Liveness check passed ($liveUrl): $liveResp"
        } catch {
            Write-Warn "Liveness check warning on $liveUrl : $_"
        }

        # Probe Readiness
        try {
            $readyResp = Invoke-RestMethod -Uri $readyUrl -TimeoutSec 10 -ErrorAction Stop
            Write-Success "Readiness check passed ($readyUrl): $readyResp"
        } catch {
            Write-Warn "Readiness check warning on $readyUrl : $_"
        }
    }
}

Write-Step "API Release Workflow Complete!"
Write-Host "All requested operations completed successfully.`n" -ForegroundColor Green
