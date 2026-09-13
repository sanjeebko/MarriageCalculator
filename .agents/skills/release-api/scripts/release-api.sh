#!/usr/bin/env bash
# ==============================================================================
# SYNOPSIS:
#   Automated release script for Marriage Calculator API across dev and prod namespaces.
#
# USAGE:
#   ./release-api.sh [dev|prod|all] [TAG]
#
# OPTIONS (environment variables):
#   SKIP_BUILD=1        Skip docker build
#   SKIP_PUSH=1         Skip docker push
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
  echo -e "\n====> Pushing images to Docker Hub"
  docker push "${IMAGE_REPO}:${TAG}"
  docker push "${IMAGE_REPO}:latest"
  echo "  [OK] Docker images pushed successfully."
else
  echo "  [WARN] Skipping Docker push (SKIP_PUSH=1)."
fi

# Determine deployment targets
TARGET_ENVS=()
if [[ "${ENV}" == "all" ]]; then
  TARGET_ENVS=("dev" "prod")
else
  TARGET_ENVS=("${ENV}")
fi

# 3. Deploy
if [[ "${SKIP_DEPLOY:-0}" != "1" ]]; then
  for target_env in "${TARGET_ENVS[@]}"; do
    echo -e "\n====> Deploying to '${target_env}' namespace"
    echo "Restarting deployment/marriagecalculatordeployment in namespace ${target_env}..."
    ssh "${K8S_USER}@${K8S_HOST}" "kubectl -n ${target_env} rollout restart deployment/marriagecalculatordeployment"
    
    echo "Waiting for rollout to complete..."
    ssh "${K8S_USER}@${K8S_HOST}" "kubectl -n ${target_env} rollout status deployment/marriagecalculatordeployment --timeout=150s"
    echo "  [OK] Deployment in namespace '${target_env}' rolled out successfully."
  done
else
  echo "  [WARN] Skipping deployment rollout (SKIP_DEPLOY=1)."
fi

# 4. Health Checks
if [[ "${SKIP_HEALTHCHECK:-0}" != "1" && "${SKIP_DEPLOY:-0}" != "1" ]]; then
  echo -e "\n====> Verifying service health checks"
  echo "Allowing 5 seconds for network routes to stabilize..."
  sleep 5
  for target_env in "${TARGET_ENVS[@]}"; do
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
  done
fi

echo -e "\n====> API Release Workflow Complete!\n"
