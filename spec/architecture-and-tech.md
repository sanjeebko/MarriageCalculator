# System Architecture & Technology Stack Specification

## 1. Architectural Blueprint

The Marriage Calculator solution is structured using Clean Architecture with distinct concerns separated across backend microservices, core domain libraries, and native client applications.

```
+-------------------------------------------------------------------------+
|                              Android Client                             |
|          (Kotlin 2.0 / Jetpack Compose / MVVM / Room / Hilt / OkHttp)    |
+-------------------------------------------------------------------------+
       | REST (JSON)                    | SignalR (WebSockets)
       v                                v
+-------------------------------------------------------------------------+
|                           MarriageCalculator.API                        |
|              (ASP.NET Core 10 / Controllers / SignalR Hubs)             |
+-------------------------------------------------------------------------+
       |                                | Pure C# References
       v                                v
+-----------------------------+  +----------------------------------------+
|      MongoDB Database       |  |        MarriageCalculator.Core         |
| (Collections: Users, Sets)  |  |    (POCOs, ScoringEngine, Zero-Dep)    |
+-----------------------------+  +----------------------------------------+
```

---

## 2. Component Specifications

### 2.1 `MarriageCalculator.Core`
- **Platform**: .NET 10 Class Library.
- **Constraints**: 0 external runtime dependencies (Pure C#). Technology-agnostic.
- **Responsibilities**:
  - Domain models: `MarriageGame`, `MarriageGameSet`, `MarriageGameScore`, `GameSettings`, `Player`, `User`.
  - Enums: `Currency`, `FoulPointBonusType`, `SeatDirection`.
  - Scoring service: `ScoringEngine` executing `CalculateScores()` and `ValidateZeroSum()`.

### 2.2 `MarriageCalculator.API`
- **Platform**: ASP.NET Core 10 Web API.
- **Database Engine**: MongoDB (Driver 2.x/3.x) targeting `192.168.0.229:27017`.
- **Authentication**: JWT validation for Firebase Auth (Google Sign-In) and Development Mock Auth.
- **Real-Time Layer**: SignalR Hub (`/gamehub`) broadcasting live game score mutations to connected clients.
- **Containerization**: Docker Compose (`deploy-api.bat`, production-ready Linux container).

### 2.3 `MarriageCalculator.Android`
- **Language & Runtime**: Kotlin 2.0+, Min SDK 26, Target SDK 34.
- **UI Toolkit**: 100% Jetpack Compose (Material3 with custom festive design tokens).
- **Dependency Injection**: Dagger Hilt (`@HiltAndroidApp`, `@AndroidEntryPoint`, `@HiltViewModel`).
- **Local Persistence**: Room SQLite database with full offline capability.
- **Networking**: Retrofit 2 + OkHttp 4 + Gson + SignalR Java Client.
- **Image Generation**: Android 2D Canvas API for high-resolution 1080px shareable result graphics (`MatchShareHelper`).

---

## 3. Data Persistence & Schemas

### 3.1 MongoDB Backend Schemas

#### Collection: `Users`
```json
{
  "_id": "ObjectId",
  "userId": "string (unique index)",
  "displayName": "string",
  "email": "string",
  "createdAt": "ISODate"
}
```

#### Collection: `GameSettings`
```json
{
  "_id": "ObjectId",
  "userId": "string (owner)",
  "murder": true,
  "kidnap": false,
  "seenPoint": 3,
  "unseenPoint": 10,
  "pointRate": 10.0,
  "currency": 0, // NPR_Rupee
  "dublee": true,
  "dubleePointLess": true,
  "dubleePointBonus": 0,
  "foulPoint": 15,
  "foulPointBonus": 0,
  "audio": true
}
```

#### Collection: `MarriageGameSets`
```json
{
  "_id": "ObjectId",
  "gameSetId": "string (UUID)",
  "hostUserId": "string",
  "name": "string",
  "createdAt": "ISODate",
  "isCompleted": false,
  "settings": { /* GameSettings */ },
  "players": [
    { "playerId": "string", "name": "string", "seatOrder": 1, "userId": "string?" }
  ],
  "games": [
    {
      "gameId": "string",
      "gameSequence": 1,
      "dealerId": "string",
      "winnerId": "string",
      "totalMaal": 25,
      "scores": {
        "playerId_1": {
          "seen": true,
          "duply": false,
          "winner": true,
          "maal": 15,
          "score": 42,
          "moneyWon": 420.0
        }
      }
    }
  ]
}
```

### 3.2 Android Room SQLite Schemas

- **`game_sets`**: Mirrors remote `MarriageGameSet` with synchronization status (`isSynced`, `remoteId`).
- **`players`**: Local dummy players and cached friend profiles.
- **`game_entries`**: Individual round entries with player score mappings.
- **`user_preferences`**: Local user settings and cached credentials (DataStore/SharedPreferences).

---

## 4. Multi-Tenant User Isolation & Permissions

1. **Host-Centric Authority**:
   - Every `MarriageGameSet` has exactly one `HostUserId`.
   - Only the host can add games, edit previous rounds, reshuffle seats, or settle the game.
   - Participants have real-time read-only access to scores, standings, and the seating ring.
2. **Data Filtering**:
   - Every database query from API controllers enforces `Where(x => x.UserId == currentUserId || x.HostUserId == currentUserId)`.
   - Prevents cross-tenant data leakage during concurrent multiplayer sessions.
