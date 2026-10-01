<#
.SYNOPSIS
    Builds, pushes, deploys, and verifies the Marriage Calculator API to Kubernetes dev and prod environments.

.DESCRIPTION
    Automates the split-tag release lifecycle:
    - dev:  Builds .NET 10 container image with git hash and 'latest' tags, pushes to Docker Hub,
            restarts dev deployment, and verifies health.
    - prod: Verifies that the specified git hash tag exists on Docker Hub, promotes it to 'stable'
            (without rebuilding), restarts prod deployment, and verifies health.
    - all:  Performs dev release followed by prod promotion of the same tag.

.PARAMETER Environment
    Target environment: 'dev', 'prod', or 'all'. Defaults to 'dev'.

.PARAMETER Tag
    Image tag to publish/promote. Defaults to the current Git short commit hash.

.PARAMETER K8sHost
    Control plane host IP. Defaults to '192.168.0.210'.

.PARAMETER K8sUser
    SSH username for cluster control plane. Defaults to 'sanjeeb'.

.PARAMETER SkipBuild
    Skip docker build step (applies to dev).

.PARAMETER SkipPush
    Skip docker push/promotion step.

.PARAMETER SkipDeploy
    Skip kubectl rollout restart step.

.PARAMETER SkipHealthCheck
    Skip health check probe verification.

.EXAMPLE
    pwsh release-api.ps1 -Environment dev
    pwsh release-api.ps1 -Environment prod
    pwsh release-api.ps1 -Environment prod -Tag fd61036
    pwsh release-api.ps1 -Environment all
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

function Invoke-EnvironmentDeploy {
    param([string]$EnvName)

    Write-Step "Deploying to '$EnvName' namespace on Kubernetes cluster"
    Write-Host "Restarting deployment/marriagecalculatordeployment in namespace $EnvName..."
    ssh -n -o BatchMode=yes "$K8sUser@$K8sHost" "kubectl -n $EnvName rollout restart deployment/marriagecalculatordeployment"
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to trigger rollout restart in namespace $EnvName."
        exit $LASTEXITCODE
    }

    Write-Host "Waiting for rollout to complete..."
    ssh -n -o BatchMode=yes "$K8sUser@$K8sHost" "kubectl -n $EnvName rollout status deployment/marriagecalculatordeployment --timeout=150s"
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Rollout failed in namespace $EnvName. Check pod logs: ssh -n -o BatchMode=yes $K8sUser@$K8sHost 'kubectl -n $EnvName logs -l app=marriagecalculatorapi --tail=50'"
        exit $LASTEXITCODE
    }
    Write-Success "Deployment in namespace '$EnvName' rolled out successfully."
}

function Test-EnvironmentHealth {
    param([string]$EnvName)

    $liveUrl = if ($EnvName -eq 'dev') { "http://192.168.1.159/health/live" } else { "https://mcapi.sanjeebojha.com.np/health/live" }
    $readyUrl = if ($EnvName -eq 'dev') { "http://192.168.1.159/health/ready" } else { "https://mcapi.sanjeebojha.com.np/health/ready" }

    Write-Host "`nTesting health endpoints for environment: $EnvName"

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
    $sshTest = ssh -n -o BatchMode=yes -o ConnectTimeout=5 "$K8sUser@$K8sHost" "echo ready" 2>&1
    if ($LASTEXITCODE -ne 0 -or $sshTest -notmatch "ready") {
        Write-Failure "Cannot reach Kubernetes cluster via SSH ($K8sUser@$K8sHost). Check your network or SSH keys."
        exit 1
    }
    Write-Success "SSH connection to cluster control plane confirmed."
}

$ImageRepo = "sanjeebojha/marriagecalculatorapi"

# --- Dev Workflow ---
if ($Environment -eq 'dev' -or $Environment -eq 'all') {
    Write-Step "=== Processing DEV Release (Target: ${ImageRepo}:latest & ${ImageRepo}:${Tag}) ==="

    # 1. Build Docker Image
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
        Write-Step "Pushing dev images to Docker Hub ($ImageRepo)"
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

    # 3. Deploy Dev
    if (-not $SkipDeploy) {
        Invoke-EnvironmentDeploy -EnvName 'dev'
    } else {
        Write-Warn "Skipping dev deployment rollout (-SkipDeploy specified)."
    }

    # 4. Health Check Dev
    if (-not $SkipHealthCheck -and -not $SkipDeploy) {
        Write-Step "Verifying dev service health checks"
        Write-Host "Allowing 5 seconds for network routes to stabilize..."
        Start-Sleep -Seconds 5
        Test-EnvironmentHealth -EnvName 'dev'
    }
}

# --- Prod Promotion Workflow ---
if ($Environment -eq 'prod' -or $Environment -eq 'all') {
    Write-Step "=== Processing PROD Promotion (Target: ${ImageRepo}:stable from ${ImageRepo}:${Tag}) ==="
    Write-Host "Production runs the promoted ':stable' tag without rebuilding."

    # 1. Verify Image exists on Docker Hub
    Write-Step "Verifying ${ImageRepo}:${Tag} exists on Docker Hub"
    docker buildx imagetools inspect "${ImageRepo}:${Tag}" | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Image ${ImageRepo}:${Tag} not found on Docker Hub. Please release to dev first or specify an existing tag with -Tag."
        exit $LASTEXITCODE
    }
    Write-Success "Verified ${ImageRepo}:${Tag} is present on Docker Hub."

    # 2. Promote to :stable
    if (-not $SkipPush) {
        Write-Step "Promoting image: ${ImageRepo}:${Tag} -> ${ImageRepo}:stable"
        docker buildx imagetools create --prefer-index=false -t "${ImageRepo}:stable" "${ImageRepo}:${Tag}"
        if ($LASTEXITCODE -ne 0) {
            Write-Failure "Failed to promote ${ImageRepo}:${Tag} to ${ImageRepo}:stable on Docker Hub."
            exit $LASTEXITCODE
        }
        Write-Success "Promoted ${ImageRepo}:${Tag} to ${ImageRepo}:stable successfully."
    } else {
        Write-Warn "Skipping promotion push (-SkipPush specified)."
    }

    # 3. Deploy Prod
    if (-not $SkipDeploy) {
        Invoke-EnvironmentDeploy -EnvName 'prod'
    } else {
        Write-Warn "Skipping prod deployment rollout (-SkipDeploy specified)."
    }

    # 4. Health Check Prod
    if (-not $SkipHealthCheck -and -not $SkipDeploy) {
        Write-Step "Verifying prod service health checks"
        Write-Host "Allowing 5 seconds for network routes to stabilize..."
        Start-Sleep -Seconds 5
        Test-EnvironmentHealth -EnvName 'prod'
    }
}

Write-Step "API Release Workflow Complete!"
Write-Host "All requested operations completed successfully.`n" -ForegroundColor Green
