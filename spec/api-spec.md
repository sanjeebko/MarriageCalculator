# API & Real-Time Protocol Specification

## 1. Authentication & Security

All API endpoints (except public health checks and OAuth callback) require an Authorization header:
```http
Authorization: Bearer <Firebase_ID_Token_Or_Mock_Token>
```
The API extracts the caller's identity:
- `UserId`: Unique user ID.
- `Email`: Verified email address.
- `DisplayName`: Caller's display name.

---

## 2. REST Endpoints

### 2.1 Authentication & Profile
- `POST /api/auth/verify-token`
  - Validates client token, creates user profile in MongoDB if first login, returns user entity.
- `GET /api/auth/me`
  - Returns current user profile, total game statistics, and active settings.

### 2.2 Game Settings
- `GET /api/GameSettings`
  - Returns default game settings for the calling user.
- `POST /api/GameSettings`
  - Creates or updates custom default game settings for the user.

### 2.3 Game Sets (`/api/MarriageGameSets`)
- `GET /api/MarriageGameSets`
  - Lists all active and recent game sets hosted by or including the calling user.
- `GET /api/MarriageGameSets/{id}`
  - Retrieves full game set with all completed rounds, player standings, and seat orders.
- `POST /api/MarriageGameSets`
  - Creates a new game set. Parameters: `Name`, `PlayerIds`, `Settings`.
- `POST /api/MarriageGameSets/{id}/games`
  - Adds a new completed game to the open round.
- `PUT /api/MarriageGameSets/{id}/games/{gameId}`
  - Updates/edits a previously submitted game (mistake correction).
- `DELETE /api/MarriageGameSets/{id}/games/last`
  - Undoes the most recent game in the round.
- `POST /api/MarriageGameSets/{id}/close-round`
  - Closes the active round and advances the sequence.
- `POST /api/MarriageGameSets/{id}/transfer-host`
  - Transfers `HostUserId` ownership to another registered participant.

### 2.4 Score Calculation
- `POST /api/MarriageGames/calculate-score`
  - Stateless calculator endpoint. Takes a `MarriageGame` and `GameSettings`, executes `ScoringEngine.CalculateScores()`, and returns calculated scores with `ValidateZeroSum()` verification.

### 2.5 Friends & Private Social Discovery
To respect user privacy, open partial user search is forbidden. Discovery uses two private channels:

1. **7-Day Invite Code**:
   - `POST /api/friends/invite-code`
     - Generates/returns a 6-character alphanumeric code (excluding ambiguous chars 0, O, 1, I, L) valid for 7 days.
   - `POST /api/friends/redeem-code`
     - Body: `{ "code": "X9K2P4" }`.
     - Automatically accepts friendship immediately without code owner confirmation (code = consent).
     - Response masks owner's email (e.g. `s***@g***.com`). Rate limited to 5 attempts per 10 minutes.
2. **Complete Email Invitation**:
   - `POST /api/friends/invite-by-email`
     - Body: `{ "email": "friend@example.com" }`.
     - If registered: creates pending request.
     - If not registered: queues claimable invite and sends invitation email.
     - Anti-enumeration: returns identical response regardless of whether email exists.

---

## 3. SignalR Real-Time Hub Protocol

- **Hub Endpoint**: `/gamehub`
- **Client Join Group**: `JoinGameSet(gameSetId)`
- **Client Leave Group**: `LeaveGameSet(gameSetId)`

### Hub Broadcast Events:
- `GameAdded(gameSetId, gameEntry)`: Broadcast to table whenever host enters a game.
- `GameUpdated(gameSetId, gameEntry)`: Broadcast when a round is edited.
- `RoundClosed(gameSetId, roundSequence)`: Triggered when round ends.
- `PaymentClearedToggled(gameSetId, roundSequence, isCleared)`: Broadcast payment clearing state.
- `HostTransferred(gameSetId, newHostUserId)`: Real-time UI transition for host privileges.
- `NudgeReceived(gameSetId, hostName)`: Triggers client vibration and attention cue.
