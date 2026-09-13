# Marriage Calculator API Deployment Troubleshooting Runbook

This guide contains operational diagnostic steps, known error signatures, and remediation procedures for the Marriage Calculator Web API on the Kubernetes home cluster (`192.168.0.210`).

---

## 1. Quick Diagnostics

Connect to `k8s-control` (`192.168.0.210`) via SSH:

```bash
ssh sanjeeb@192.168.0.210
```

### Check Pod Status
```bash
# Check dev pods
kubectl -n dev get pods -l app=marriagecalculatorapi -o wide

# Check prod pods
kubectl -n prod get pods -l app=marriagecalculatorapi -o wide
```

### View Real-time Pod Logs
```bash
# dev logs
kubectl -n dev logs -l app=marriagecalculatorapi -c webapi --tail=100 -f

# prod logs
kubectl -n prod logs -l app=marriagecalculatorapi -c webapi --tail=100 -f
```

### Check Namespace Events (for scheduling/pull issues)
```bash
kubectl -n <dev|prod> get events --sort-by='.lastTimestamp'
```

---

## 2. Immediate Rollback

If a newly deployed container fails or causes downtime, roll back immediately:

```bash
# Roll back to previous revision
kubectl -n <dev|prod> rollout undo deployment/marriagecalculatordeployment

# View rollout history
kubectl -n <dev|prod> rollout history deployment/marriagecalculatordeployment

# Roll back to specific revision
kubectl -n <dev|prod> rollout undo deployment/marriagecalculatordeployment --to-revision=<REV_NUMBER>
```

---

## 3. Common Error Signatures and Fixes

### Error 1: `System.InvalidOperationException: DatabaseName is not set in environment variable MCDATABASENAME`
- **Cause**: The .NET 10 `Program.cs` requires `MCDATABASENAME` to be explicitly defined.
- **Remedy**:
  Ensure `MCDATABASENAME` is declared under `env:` in `deployments.yaml`:
  - For `dev`: `value: "marriagecalculator_dev"`
  - For `prod`: `value: "marriagecalculator_prod"`

### Error 2: `MongoTimeoutException` or Connection Timeout to `192.168.0.229:27017`
- **Cause 1: Wrong Host**:
  - Check `MCDATABASE` in `deployments.yaml`. It must be `192.168.0.229:27017` (MongoDB), not `192.168.0.214` (MSSQL).
- **Cause 2: Egress Network Policy Blocking TCP 27017**:
  - The `marriage-calculator-egress-policy` NetworkPolicy restricts outbound pod traffic.
  - Port 27017 must be included under `egress.ports`:
    ```yaml
    - to: []
      ports:
      - protocol: TCP
        port: 27017 # MongoDB
    ```
- **Verification from inside the pod**:
  ```bash
  POD=$(kubectl -n <dev|prod> get pod -l app=marriagecalculatorapi -o jsonpath='{.items[0].metadata.name}')
  kubectl -n <dev|prod> exec -it "$POD" -c webapi -- curl -v telnet://192.168.0.229:27017
  ```

### Error 3: Readiness Probe Failure (`HTTP 503 Service Unavailable` on `/health/ready`)
- **Cause**: The API's `/health/ready` check executes a ping against MongoDB. If the database is unreachable or credentials fail, it returns HTTP 503.
- **Check Mongo Auth**:
  - `dev`: `mcuserdev` / `Scorpions18` on auth database `marriagecalculator_dev`
  - `prod`: `mcuserprod` / `Scorpions18` on auth database `marriagecalculator_prod`

### Error 4: `ImagePullBackOff` or `ErrImagePull`
- **Cause**: Docker Hub rate limiting, network issue, or typo in the image tag.
- **Remedy**:
  - Confirm image exists on Docker Hub:
    `docker pull sanjeebojha/marriagecalculatorapi:<tag>`
  - If using `:latest`, verify `docker push sanjeebojha/marriagecalculatorapi:latest` completed successfully.

---

## 4. Required Environment Variables Reference

| Variable | Description | Example (Dev) | Example (Prod) |
| :--- | :--- | :--- | :--- |
| `MCDATABASE` | MongoDB host and port | `192.168.0.229:27017` | `192.168.0.229:27017` |
| `MCDATABASENAME` | Mongo database name | `marriagecalculator_dev` | `marriagecalculator_prod` |
| `MCUSER` | MongoDB auth username | `mcuserdev` | `mcuserprod` |
| `MCPASSWORD` | MongoDB auth password | `Scorpions18` | `Scorpions18` |
| `ASPNETCORE_ENVIRONMENT`| ASP.NET environment | `Development` | `Production` |
| `MCSMTP` | SMTP server for emails | `smtp.zoho.eu` | `smtp.zoho.eu` |
| `MCMAILUSERNAME` | SMTP sender account | `noreply@sanjeebojha.com.np` | `noreply@sanjeebojha.com.np` |
| `MCMAILPASSWORD` | SMTP account password | *(from secrets)* | *(from secrets)* |
