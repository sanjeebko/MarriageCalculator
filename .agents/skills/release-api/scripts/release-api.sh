#!/usr/bin/env bash
# ==============================================================================
# SYNOPSIS:
#   Automated release script for Marriage Calculator API across dev and prod namespaces.
#   - dev:  Builds :<sha> + :latest, pushes to Docker Hub, restarts dev deployment, checks health.
#   - prod: Verifies :<sha> exists on Docker Hub, promotes it to :stable (no rebuild),
#           restarts prod deployment, checks health.
#   - all:  Releases to dev, then promotes to prod.
#
# USAGE:
#   ./release-api.sh [dev|prod|all] [TAG]
#
# OPTIONS (environment variables):
#   SKIP_BUILD=1        Skip docker build
#   SKIP_PUSH=1         Skip docker push / promotion
#   SKIP_DEPLOY=1       Skip kubectl rollout restart
#   SKIP_HEALTHCHECK=1  Skip health probe checks
#   K8S_HOST=...        Kubernetes control plane IP (default: 192.168.0.210)
#   K8S_USER=...        SSH user (default: sanjeeb)
# ==============================================================================

set -euo pipefail

ENV="${1:-dev}"
TAG="${2:-}"
K8S_HOST="${K8S_HOST:-192.168.0.210}"
K8S_USER="${K8S_USER:-sanjeeb}"
IMAGE_REPO="sanjeebojha/marriagecalculatorapi"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../../../.." && pwd)"
cd "${REPO_ROOT}"

echo -e "\n====> Initializing Marriage Calculator API Release"
echo "Target Environment: ${ENV}"
echo "Repository Root:    ${REPO_ROOT}"

# Validate environment
if [[ "${ENV}" != "dev" && "${ENV}" != "prod" && "${ENV}" != "all" ]]; then
  echo "Error: Invalid environment '${ENV}'. Allowed values: dev, prod, all." >&2
  exit 1
fi

# Determine tag
if [[ -z "${TAG}" ]]; then
  TAG=$(git rev-parse --short HEAD 2>/dev/null || echo "build-$(date +%Y%m%d%H%M%S)")
fi
echo "Image Tag:          ${TAG}"

# Pre-flight check: Docker
if [[ "${SKIP_BUILD:-0}" != "1" || "${SKIP_PUSH:-0}" != "1" ]]; then
  echo -e "\n====> Checking Docker daemon status"
  if ! docker info >/dev/null 2>&1; then
    echo "Error: Docker daemon is not running." >&2
    exit 1
  fi
  echo "  [OK] Docker daemon is running."
fi

# Pre-flight check: SSH
if [[ "${SKIP_DEPLOY:-0}" != "1" ]]; then
  echo -e "\n====> Checking SSH access to Kubernetes cluster (${K8S_USER}@${K8S_HOST})"
  if ! ssh -o BatchMode=yes -o ConnectTimeout=5 "${K8S_USER}@${K8S_HOST}" "echo ready" >/dev/null 2>&1; then
    echo "Error: Cannot reach Kubernetes cluster via SSH (${K8S_USER}@${K8S_HOST})." >&2
    exit 1
  fi
  echo "  [OK] SSH connection confirmed."
fi

deploy_env() {
  local target_env="$1"
  echo -e "\n====> Deploying to '${target_env}' namespace"
  echo "Restarting deployment/marriagecalculatordeployment in namespace ${target_env}..."
  ssh "${K8S_USER}@${K8S_HOST}" "kubectl -n ${target_env} rollout restart deployment/marriagecalculatordeployment"
  
  echo "Waiting for rollout to complete..."
  ssh "${K8S_USER}@${K8S_HOST}" "kubectl -n ${target_env} rollout status deployment/marriagecalculatordeployment --timeout=150s"
  echo "  [OK] Deployment in namespace '${target_env}' rolled out successfully."
}

check_health() {
  local target_env="$1"
  echo -e "\nTesting health endpoints for environment: ${target_env}"
  if [[ "${target_env}" == "dev" ]]; then
    LIVE_URL="http://192.168.1.159/health/live"
    READY_URL="http://192.168.1.159/health/ready"
  else
    LIVE_URL="https://mcapi.sanjeebojha.com.np/health/live"
    READY_URL="https://mcapi.sanjeebojha.com.np/health/ready"
  fi

  if curl -fsSL --max-time 10 "${LIVE_URL}" >/dev/null 2>&1; then
    echo "  [OK] Liveness check passed (${LIVE_URL})"
  else
    echo "  [WARN] Liveness check failed or unreachable (${LIVE_URL})"
  fi

  if curl -fsSL --max-time 10 "${READY_URL}" >/dev/null 2>&1; then
    echo "  [OK] Readiness check passed (${READY_URL})"
  else
    echo "  [WARN] Readiness check failed or unreachable (${READY_URL})"
  fi
}

# --- Dev Workflow ---
if [[ "${ENV}" == "dev" || "${ENV}" == "all" ]]; then
  echo -e "\n====> === Processing DEV Release (Target: ${IMAGE_REPO}:latest & ${IMAGE_REPO}:${TAG}) ==="

  # 1. Build
  if [[ "${SKIP_BUILD:-0}" != "1" ]]; then
    echo -e "\n====> Building container image (${IMAGE_REPO}:${TAG} and ${IMAGE_REPO}:latest)"
    docker build \
      -t "${IMAGE_REPO}:${TAG}" \
      -t "${IMAGE_REPO}:latest" \
      -f MarriageCalculator/MarriageCalculator.API/Dockerfile \
      MarriageCalculator
    echo "  [OK] Docker image built successfully."
  else
    echo "  [WARN] Skipping Docker build (SKIP_BUILD=1)."
  fi

  # 2. Push
  if [[ "${SKIP_PUSH:-0}" != "1" ]]; then
    echo -e "\n====> Pushing dev images to Docker Hub"
    docker push "${IMAGE_REPO}:${TAG}"
    docker push "${IMAGE_REPO}:latest"
    echo "  [OK] Docker images pushed successfully."
  else
    echo "  [WARN] Skipping Docker push (SKIP_PUSH=1)."
  fi

  # 3. Deploy Dev
  if [[ "${SKIP_DEPLOY:-0}" != "1" ]]; then
    deploy_env "dev"
  else
    echo "  [WARN] Skipping dev deployment rollout (SKIP_DEPLOY=1)."
  fi

  # 4. Health Check Dev
  if [[ "${SKIP_HEALTHCHECK:-0}" != "1" && "${SKIP_DEPLOY:-0}" != "1" ]]; then
    echo -e "\n====> Verifying dev service health checks"
    echo "Allowing 5 seconds for network routes to stabilize..."
    sleep 5
    check_health "dev"
  fi
fi

# --- Prod Promotion Workflow ---
if [[ "${ENV}" == "prod" || "${ENV}" == "all" ]]; then
  echo -e "\n====> === Processing PROD Promotion (Target: ${IMAGE_REPO}:stable from ${IMAGE_REPO}:${TAG}) ==="
  echo "Production runs the promoted ':stable' tag without rebuilding."

  # 1. Verify Image exists on Docker Hub
  echo -e "\n====> Verifying ${IMAGE_REPO}:${TAG} exists on Docker Hub"
  if ! docker buildx imagetools inspect "${IMAGE_REPO}:${TAG}" >/dev/null 2>&1; then
    echo "Error: Image ${IMAGE_REPO}:${TAG} not found on Docker Hub. Please release to dev first or specify an existing tag." >&2
    exit 1
  fi
  echo "  [OK] Verified ${IMAGE_REPO}:${TAG} is present on Docker Hub."

  # 2. Promote to :stable
  if [[ "${SKIP_PUSH:-0}" != "1" ]]; then
    echo -e "\n====> Promoting image: ${IMAGE_REPO}:${TAG} -> ${IMAGE_REPO}:stable"
    docker buildx imagetools create --prefer-index=false -t "${IMAGE_REPO}:stable" "${IMAGE_REPO}:${TAG}"
    echo "  [OK] Promoted ${IMAGE_REPO}:${TAG} to ${IMAGE_REPO}:stable successfully."
  else
    echo "  [WARN] Skipping promotion push (SKIP_PUSH=1)."
  fi

  # 3. Deploy Prod
  if [[ "${SKIP_DEPLOY:-0}" != "1" ]]; then
    deploy_env "prod"
  else
    echo "  [WARN] Skipping prod deployment rollout (SKIP_DEPLOY=1)."
  fi

  # 4. Health Check Prod
  if [[ "${SKIP_HEALTHCHECK:-0}" != "1" && "${SKIP_DEPLOY:-0}" != "1" ]]; then
    echo -e "\n====> Verifying prod service health checks"
    echo "Allowing 5 seconds for network routes to stabilize..."
    sleep 5
    check_health "prod"
  fi
fi

echo -e "\n====> API Release Workflow Complete!\n"
