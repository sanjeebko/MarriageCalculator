# Marriage Calculator Solution Architecture

This document serves as the architectural overview and design manual for the Marriage Calculator solution.

---

## 1. Solution Structure & Projects

The solution consists of three main projects, organized using clean architecture principles:

```
MarriageCalculator/
├── MarriageCalculator.Core/       # Domain Layer (POCO models & scoring logic)
├── MarriageCalculator.API/        # Application Layer (REST API & WebSockets)
└── Android/                       # Presentation Layer (Native Compose Client)
```

### 1.1 MarriageCalculator.Core (Shared Domain Library)
* **Role**: Central repository of domain models, rules, and calculation engines.
* **Tech Stack**: Pure .NET 8 Class Library.
* **Constraints**: Strictly **zero dependencies**. No databases, no UI frameworks, and no web components are imported. This keeps the scoring engine completely portable.
* **Contents**:
  * Domain models (`User`, `Player`, `GameSettings`, `MarriageGameSet`, etc.).
  * Central Collection scoring algorithm.

### 1.2 MarriageCalculator.API (Web API Backend)
* **Role**: Handles persistence, session sync, real-time broadcasts, and notification dispatch.
* **Tech Stack**: ASP.NET Core 8 with Controller-based REST routing.
* **Database**: **MongoDB** (Default server hosted on local network at `192.168.0.229`).
* **Real-time Sync**: **SignalR Hubs** to broadcast score updates concurrently to connected players.
* **Push Notifications**: **Firebase Cloud Messaging (FCM)** for offline invites and host nudges.

### 1.3 MarriageCalculator.Android (Native Mobile Client)
* **Role**: Native mobile client for player scoring inputs, scoreboard viewing, and setup.
* **Tech Stack**: Native Kotlin with **Jetpack Compose** for UI styling.
* **Local Storage**: **Room Database** for offline-first data caching and offline game support.
* **Network Client**: **Retrofit + OkHttp** with JWT/OAuth headers.
* **Deep Linking**: Handles `marriagecalculator://playgame/{gameSetId}` scheme to route users directly into an active game session from push notifications.

---

## 2. API Architecture Layers (Clean Architecture)

The backend is structured into decoupled layers, separating delivery mechanisms from business logic and database concerns:

```
[Client App] ──> [Controllers] ──> [Services] ──> [Repositories] ──> [MongoDB]
```

### 2.1 Controllers (`/Controllers`)
* **Role**: Handles HTTP requests, parses routing schemas, and outputs RESTful HTTP responses.
* **Responsibilities**:
  * Input model validation (`ModelState`).
  * Mapping endpoint targets using DTOs.
  * Standardized HTTP response codes.
  * Exposes routes: `/api/Users`, `/api/MarriageGameSets`, `/api/Friendships`, `/api/Scoring`.

### 2.2 Services (`/Services`)
* **Role**: Orchestrates business logic, applies domain validations, and converts models between DTOs and database entities.
* **Responsibilities**:
  * Authentication context validation.
  * FCM payload formatting and dispatching.
  * Encapsulates transactional business logic.

### 2.3 Repositories (`/Repositories`)
* **Role**: Abstract interface for reading and writing data, isolating the service layer from database-specific syntax.
* **Responsibilities**:
  * Connecting to MongoDB collections.
  * MongoDB queries, updates, and deletes.
  * Database transaction management.

### 2.4 DTOs (`/DTOs`)
* **Role**: Defines contracts for requesting or returning data over the network, decoupling database entities from public API footprints.

---

## 3. Key Design Patterns

### 3.1 Central Collection Scoring Algorithm
Scoring in the "Marriage" card game revolves around the **Central Collection** technique:
1. All losing players pay fixed game penalties to the winner (based on seen/unseen/dublee statuses).
2. The winner then distributes "Maal" payouts to all qualified seen players based on the difference of their held Maal points.
3. This is calculated dynamically on the server and returned to the client instantly.

### 3.2 Offline-First Sync Pattern
1. If the mobile app is in Guest mode or disconnected, game states are stored in the local **Room Database**.
2. Once connection is established, the local data is merged with the remote database through the API sync endpoints.

### 3.3 Real-time Scoreboard
* Every time a round input is submitted, the API recalculates the score and triggers a SignalR broadcast.
* Connected player clients receive the packet and update their Scoreboard grids instantly.