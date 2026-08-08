# API Deployment & Configuration Guide

This guide details how to build the Docker image, run the container locally with Docker Desktop, and deploy the **Marriage Calculator API** with **Firebase Cloud Messaging (FCM)** credentials to your Development and Production clusters (Docker & Kubernetes).

---

## 1. Build & Push API Docker Images

Use the following commands from the root directory of the repository to compile and publish the API docker image to your registry (`sanjeebojha/marriagecalculatorapi`):

```bash
# Navigate to the workspace root directory (where MarriageCalculator.sln is located)
cd /path/to/MarriageCalculator/

# 1. Build the Docker image (clean .NET 10 build stage)
docker build -f MarriageCalculator.API/Dockerfile -t sanjeebojha/marriagecalculatorapi:latest .

# 2. Tag image versions (e.g. latest, stable, specific release version)
docker tag sanjeebojha/marriagecalculatorapi:latest sanjeebojha/marriagecalculatorapi:stable
docker tag sanjeebojha/marriagecalculatorapi:latest sanjeebojha/marriagecalculatorapi:1.0.2

# 3. Push images to Docker Hub
docker push sanjeebojha/marriagecalculatorapi:latest
docker push sanjeebojha/marriagecalculatorapi:stable
docker push sanjeebojha/marriagecalculatorapi:1.0.2
```

---

## 2. Environment Configurations & Settings Keys

The API backend reads configuration parameters from `appsettings.json`, which can be overridden in container environments using standard double-underscore (`__`) environment variables:

| Configuration Key | Environment Variable Equivalent | Description | Default Value / Example |
| :--- | :--- | :--- | :--- |
| `MongoDbSettings:ConnectionString` | `MongoDbSettings__ConnectionString` | MongoDB connection URI | `mongodb://{MCUSER}:{MCPASSWORD}@192.168.0.229/MarriageCalculator` |
| `MongoDbSettings:DatabaseName` | `MongoDbSettings__DatabaseName` | MongoDB database name | `MarriageCalculator` |
| `Firebase:ProjectId` | `Firebase__ProjectId` | Firebase Console project ID | `marriagecalculator-197bd` |
| `Firebase:ServiceAccountKeyPath` | `Firebase__ServiceAccountKeyPath` | Path to service credentials JSON file | `firebase-adminsdk.json` *(resolves relative to app base)* |
| `Firebase:ServiceAccountKeyJson` | `Firebase__ServiceAccountKeyJson` | Raw JSON string of service credentials | *None (Optional override)* |

---

## 3. Docker Desktop: Local Development & Verification

When running locally inside Docker Desktop, you can choose between two methods to provide the Firebase Admin SDK credentials:

### Method A: Workspace Relative Mount (Recommended)
Save the key file as `MarriageCalculator/MarriageCalculator.API/firebase-adminsdk.json`. This path is ignored by git (via `.gitignore`), but mounted dynamically.

#### docker-compose.yml Configuration:
```yaml
services:
  marriagecalculator-api:
    image: sanjeebojha/marriagecalculatorapi:latest
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Firebase__ProjectId=marriagecalculator-197bd
    volumes:
      # Mounts the host key to the relative workspace app folder as read-only
      - ./MarriageCalculator/MarriageCalculator.API/firebase-adminsdk.json:/app/firebase-adminsdk.json:ro
```

### Method B: Environment Variable (JSON String)
Alternatively, pass the entire credentials JSON payload as an environment variable:
```yaml
services:
  marriagecalculator-api:
    image: sanjeebojha/marriagecalculatorapi:latest
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Firebase__ProjectId=marriagecalculator-197bd
      - Firebase__ServiceAccountKeyJson={"type": "service_account", "project_id": "marriagecalculator-197bd", ...}
```

---

## 4. Kubernetes Deployment (Dev & Prod Environments)

In Kubernetes clusters, **never** hardcode credentials in your container image. Instead, use Kubernetes Secrets.

### Step 1: Create the Kubernetes Secret
Choose one of the following methods to load the secret into the cluster:

#### Option 1: Create secret from the file (recommended for file mount)
```bash
kubectl create secret generic firebase-adminsdk-secret \
  --from-file=firebase-adminsdk.json=path/to/firebase-adminsdk.json \
  -n mc-namespace
```

#### Option 2: Create secret containing the JSON string (recommended for environment variables)
```bash
kubectl create secret generic firebase-adminsdk-secret \
  --from-literal=credentials-json='{"type": "service_account", "project_id": "marriagecalculator-197bd", ...}' \
  -n mc-namespace
```

---

### Step 2: Inject the Secret into Pods

#### Mounting Secret as a File Volume (Matching Option 1)
This mounts the secret JSON file inside the container at `/app/secrets/firebase-adminsdk.json` and tells the API where to find it.

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: marriagecalculator-api
  namespace: mc-namespace
  labels:
    app: marriagecalculator-api
spec:
  replicas: 2
  selector:
    matchLabels:
      app: marriagecalculator-api
  template:
    metadata:
      labels:
        app: marriagecalculator-api
    spec:
      containers:
      - name: api
        image: sanjeebojha/marriagecalculatorapi:stable
        ports:
        - containerPort: 8080
        env:
        # DB Credentials
        - name: MCDATABASE
          value: "192.168.0.229"
        - name: MCUSER
          value: "database-username"
        - name: MCPASSWORD
          value: "database-password"
        # Firebase Project ID
        - name: Firebase__ProjectId
          value: "marriagecalculator-197bd"
        # Overrides Path configuration to point to mounted volume path
        - name: Firebase__ServiceAccountKeyPath
          value: "/app/secrets/firebase-adminsdk.json"
        volumeMounts:
        - name: firebase-volume
          mountPath: /app/secrets
          readOnly: true
      volumes:
      - name: firebase-volume
        secret:
          secretName: firebase-adminsdk-secret
```

#### Injecting Secret directly as Environment Variable (Matching Option 2)
No volume mounts are required. The JSON string is fed directly into memory.

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: marriagecalculator-api
  namespace: mc-namespace
spec:
  replicas: 2
  selector:
    matchLabels:
      app: marriagecalculator-api
  template:
    metadata:
      labels:
        app: marriagecalculator-api
    spec:
      containers:
      - name: api
        image: sanjeebojha/marriagecalculatorapi:stable
        ports:
        - containerPort: 8080
        env:
        # DB Credentials
        - name: MCDATABASE
          value: "192.168.0.229"
        - name: MCUSER
          value: "database-username"
        - name: MCPASSWORD
          value: "database-password"
        # Firebase Configurations
        - name: Firebase__ProjectId
          value: "marriagecalculator-197bd"
        # Inject the raw JSON content directly from Kubernetes Secrets
        - name: Firebase__ServiceAccountKeyJson
          valueFrom:
            secretKeyRef:
              name: firebase-credentials
              key: credentials-json
```

---

## 5. Verification Check

You can verify that the containerized API initialized the Firebase Admin SDK successfully by checking the container logs:

* **Production (Firebase Enabled)**:
  ```text
  info: MarriageCalculator.API.Services.FcmService[0]
        FirebaseApp initialized successfully using service account JSON credentials.
  ```
  *or*
  ```text
  info: MarriageCalculator.API.Services.FcmService[0]
        FirebaseApp initialized successfully using service account key file at: /app/secrets/firebase-adminsdk.json
  ```

* **Mock Fallback (If credentials missing or misconfigured)**:
  ```text
  warn: MarriageCalculator.API.Services.FcmService[0]
        Firebase configurations (ProjectId, ServiceAccountKeyPath, or ServiceAccountKeyJson) are not configured. FCM notifications will run in Mock (logging-only) mode.
  ```
  *(Mock mode prevents the API container from crashing, enabling localized endpoints and sandbox testing without failing container health checks).*
