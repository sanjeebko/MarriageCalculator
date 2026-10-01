---
name: release-api
description: >-
  Build, containerize, push, deploy, and verify the Marriage Calculator .NET 10 API across Kubernetes dev and prod environments. Use whenever releasing API updates, redeploying containers, or verifying backend cluster health.
---

# Marriage Calculator API Release Skill

Runbook and automated tooling for building, containerizing, pushing, and deploying the **Marriage Calculator .NET 10 Web API** to Kubernetes (`dev` and `prod` namespaces) on the home cluster (`192.168.0.210`).

---

## Environments & Architecture Reference

| Component | `dev` Environment | `prod` Environment |
| :--- | :--- | :--- |
| **Kubernetes Namespace** | `dev` | `prod` |
| **Deployment Name** | `marriagecalculatordeployment` | `marriagecalculatordeployment` |
| **Container Image** | `sanjeebojha/marriagecalculatorapi:latest` | `sanjeebojha/marriagecalculatorapi:stable` |
| **Database** | MongoDB `192.168.0.229:27017` (`marriagecalculator_dev`) | MongoDB `192.168.0.229:27017` (`marriagecalculator_prod`) |
| **MetalLB LoadBalancer** | `192.168.1.159:80` | `192.168.1.55:80` |
| **Public Domain** | N/A (Internal LAN only) | `https://mcapi.sanjeebojha.com.np/api/` |
| **Health Check Endpoint** | `http://192.168.1.159/health/ready` | `https://mcapi.sanjeebojha.com.np/health/ready` |
| **Cluster Node** | `k8s-control` (`192.168.0.210`) | `k8s-control` (`192.168.0.210`) |

### Split Tagging Scheme

| Tag | Meaning | Pulled By | Workflow |
| :--- | :--- | :--- | :--- |
| `:<git-sha>` | Immutable build, one per commit | Rollback reference | Built and published on dev release |
| `:latest` | Newest build from `main` | **dev** | Built & published on dev release |
| `:stable` | Image promoted to production after passing dev | **prod** | Retagged from `:<git-sha>` without rebuild |

---

## 1. Quick Automated Release

Run the automated release script from the repository root:

### PowerShell (Windows / pwsh)
```powershell
# 1. Release to dev (builds :<sha> + :latest, pushes, restarts dev, verifies health)
pwsh ./.agents/skills/release-api/scripts/release-api.ps1 -Environment dev

# 2. Promote to prod (promotes :<sha> -> :stable without rebuild, restarts prod, verifies health)
pwsh ./.agents/skills/release-api/scripts/release-api.ps1 -Environment prod -Tag <sha>

# Or release to both dev and prod sequentially
pwsh ./.agents/skills/release-api/scripts/release-api.ps1 -Environment all
```

### Bash (Linux / macOS / WSL)
```bash
# 1. Release to dev
./.agents/skills/release-api/scripts/release-api.sh dev

# 2. Promote to prod
./.agents/skills/release-api/scripts/release-api.sh prod <sha>

# Or release to both dev and prod sequentially
./.agents/skills/release-api/scripts/release-api.sh all
```

### Supported Script Parameters
- `-Environment <dev | prod | all>`: Target namespace (default: `dev`).
- `-Tag <string>`: Docker image tag to build or promote (defaults to current git short hash).
- `-SkipBuild`: Skips `docker build` step (applies to dev).
- `-SkipPush`: Skips `docker push` / promotion step.
- `-SkipDeploy`: Skips triggering `kubectl rollout restart`.
- `-SkipHealthCheck`: Skips health endpoint verification.

---

## 2. Step-by-Step Manual Workflow

Follow these steps if performing the release manually or investigating an intermediate step:

### Step 1: Pre-Flight Checks
1. Ensure Docker Desktop / daemon is running:
   ```bash
   docker info
   ```
2. Verify SSH access to Kubernetes control plane:
   ```bash
   ssh sanjeeb@192.168.0.210 "kubectl get nodes"
   ```

### Step 2: Build & Push Container Image for Dev
Build from the root of the repository using `MarriageCalculator/` as the build context:
```bash
# Obtain current git commit hash
GIT_TAG=$(git rev-parse --short HEAD)

# Build image with git commit tag and latest
docker build \
  -t sanjeebojha/marriagecalculatorapi:$GIT_TAG \
  -t sanjeebojha/marriagecalculatorapi:latest \
  -f MarriageCalculator/MarriageCalculator.API/Dockerfile \
  MarriageCalculator

# Push to Docker Hub
docker push sanjeebojha/marriagecalculatorapi:$GIT_TAG
docker push sanjeebojha/marriagecalculatorapi:latest
```

### Step 3: Deploy to Dev
```bash
ssh sanjeeb@192.168.0.210 "kubectl -n dev rollout restart deployment/marriagecalculatordeployment"
ssh sanjeeb@192.168.0.210 "kubectl -n dev rollout status deployment/marriagecalculatordeployment --timeout=150s"
```

### Step 4: Promote Image to Production (`:stable`)
Once verified on dev, promote the immutable git hash tag to `:stable` without rebuilding:
```bash
docker buildx imagetools create --prefer-index=false \
  -t sanjeebojha/marriagecalculatorapi:stable \
  sanjeebojha/marriagecalculatorapi:$GIT_TAG
```

### Step 5: Trigger Kubernetes Rollout Restart for Prod
```bash
ssh sanjeeb@192.168.0.210 "kubectl -n prod rollout restart deployment/marriagecalculatordeployment"
ssh sanjeeb@192.168.0.210 "kubectl -n prod rollout status deployment/marriagecalculatordeployment --timeout=150s"
```

### Step 6: Verify Service Health
1. **Liveness Probe**:
   - Dev: `curl -f -s http://192.168.1.159/health/live`
   - Prod: `curl -f -s https://mcapi.sanjeebojha.com.np/health/live`
2. **Readiness Probe** (Verifies MongoDB database connectivity):
   - Dev: `curl -f -s http://192.168.1.159/health/ready`
   - Prod: `curl -f -s https://mcapi.sanjeebojha.com.np/health/ready`
3. **API Endpoint Verification**:
   - Dev: `curl -s -o /dev/null -w "%{http_code}" http://192.168.1.159/api/MarriageGames`
   - Prod: `curl -s -o /dev/null -w "%{http_code}" https://mcapi.sanjeebojha.com.np/api/MarriageGames`
   *(Note: Returns 401 Unauthorized when unauthenticated, confirming endpoint is active and secured).*

---

## 3. Rollback & Troubleshooting

If a rollout fails or the health probe fails to report healthy:

1. **Production Rollback**:
   - Re-point `:stable` to the previous known good commit tag:
     ```bash
     docker buildx imagetools create --prefer-index=false \
       -t sanjeebojha/marriagecalculatorapi:stable \
       sanjeebojha/marriagecalculatorapi:<previous-sha>
     ssh sanjeeb@192.168.0.210 "kubectl -n prod rollout restart deployment/marriagecalculatordeployment"
     ```
   - *Or* roll back immediately in Kubernetes:
     ```bash
     ssh sanjeeb@192.168.0.210 "kubectl -n prod rollout undo deployment/marriagecalculatordeployment"
     ```

2. **Development Rollback**:
   ```bash
   ssh sanjeeb@192.168.0.210 "kubectl -n dev rollout undo deployment/marriagecalculatordeployment"
   ```

3. **View Pod Logs**:
   ```bash
   ssh sanjeeb@192.168.0.210 "kubectl -n <dev|prod> logs -l app=marriagecalculatorapi -c webapi --tail=100"
   ```

4. Consult the [Troubleshooting Guide](./references/troubleshooting.md) for detailed error resolutions.
