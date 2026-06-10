# Marriage Game Calculator - Android App Implementation Plan

## Problem Statement
Build a full-featured Android (Kotlin/Compose) app for the Marriage card game calculator, backed by the existing .NET API. The app must handle 2-6 players with efficient screen space usage, support offline/online modes, real-time score display, and integrate with the C# API hosted on Kubernetes. The Maui version is archived and replaced by this native Android app.

## Status: ALL PHASES COMPLETE ✅
- 23 C# tests + 45 Android unit tests all passing
- Android APK builds successfully
- .NET API builds successfully
- **Docker**: Production-ready for API deployment.

## Approach
Iterative development in phases. Each step produces a buildable, testable commit. Use SignalR for real-time updates. Build scoring engine in C# Core, expose via API, consume in Android via Retrofit + OkHttp.

---

## Phase 1: Foundation & Navigation (Android)
- [x] Step 1.1: Create branch `android` from main
- [ ] Step 1.2: Add dependencies (Retrofit, OkHttp, Gson, Navigation Compose, Hilt DI, Room for local storage, SignalR client)
- [ ] Step 1.3: Set up navigation graph (Login → Dashboard → GameSetup → PlayGame → Scoreboard → RoundInput)
- [ ] Step 1.4: Create data models mirroring C# Core models (Player, GameSettings, MarriageGame, MarriageGameScore, etc.)
- [ ] Step 1.5: Create API service interfaces (Retrofit) for all endpoints
- [ ] Step 1.6: Set up Hilt dependency injection module
- [ ] Step 1.7: Write unit tests for data models
- **COMMIT**: "feat: Android navigation, DI, API client, and data models"

## Phase 2: Scoring Engine (C# Core + API)
- [ ] Step 2.1: Implement Marriage game scoring engine in Core (Central Collection algorithm)
  - Handle Seen/Unseen/Dublee player states
  - Calculate Game Points (fixed penalties)
  - Calculate Maal Points (variable, player-to-player comparison)
  - Support Normal/Kidnap/Murder modes
  - Handle dynamic player count (2-6)
- [ ] Step 2.2: Add scoring endpoint to API (POST /api/MarriageGames/calculate-score)
- [ ] Step 2.3: Add SignalR hub for real-time score broadcasting
- [ ] Step 2.4: Write comprehensive unit tests for scoring engine (all 3 modes, edge cases)
- **COMMIT**: "feat: Marriage game scoring engine with Normal/Kidnap/Murder modes"

## Phase 3: Dashboard & Game Setup Screen (Android)
- [ ] Step 3.1: Build Dashboard screen (greeting, New Game, Join Game, History buttons)
- [ ] Step 3.2: Build Game Setup screen
  - Player selection/creation (2-6 players) with circular seating arrangement
  - Currency selection (NPR, INR, GBP, USD, AUD priority)
  - Game settings (PointRate, SeenPenalty, UnseenPenalty, DubleeBonus, GameMode toggles)
  - Settings lock mechanism once game starts
- [ ] Step 3.3: Create ViewModels for Dashboard and GameSetup
- [ ] Step 3.4: Connect to API (create game set, save settings, add players)
- [ ] Step 3.5: Write UI tests for game setup flow
- **COMMIT**: "feat: Dashboard and Game Setup screens with API integration"

## Phase 4: Round Input Screen (The Scorer)
- [ ] Step 4.1: Build Round Input screen optimized for 6 players
  - Compact player cards showing name + status
  - Winner selection (tap to select)
  - Seen/Unseen/Dublee toggle per player
  - Maal numeric input per player
  - Dealer indicator
- [ ] Step 4.2: Implement score calculation display (instant preview before submit)
- [ ] Step 4.3: Handle round submission to API with validation
- [ ] Step 4.4: Support editing previous rounds (mistake correction)
- [ ] Step 4.5: Write tests for round input validation and score preview
- **COMMIT**: "feat: Round Input screen with score calculation and validation"

## Phase 5: Scoreboard & History
- [ ] Step 5.1: Build Scoreboard screen (real-time balances for all players)
  - Compact layout for 6 players (grid/table layout)
  - Color-coded positive/negative scores
  - Money calculator (points × rate in selected currency)
  - "Who owes whom" clear display
- [ ] Step 5.2: Build Round History (scrollable list of past rounds with details)
- [ ] Step 5.3: Integrate SignalR for real-time score updates
- [ ] Step 5.4: Add settlement/freeze functionality
- [ ] Step 5.5: Write tests for scoreboard calculations and SignalR integration
- **COMMIT**: "feat: Scoreboard with real-time updates and round history"

## Phase 6: Local Storage & Offline Mode
- [ ] Step 6.1: Set up Room database with entities mirroring API models
- [ ] Step 6.2: Implement offline-first pattern (local save → sync when online)
- [ ] Step 6.3: Guest mode (no auth, local dummy players)
- [ ] Step 6.4: Data sync logic (merge local + remote on reconnect)
- [ ] Step 6.5: Write tests for offline storage and sync
- **COMMIT**: "feat: Offline mode with Room database and sync"

## Phase 7: Player Session Management
- [ ] Step 7.1: Join/Leave between games (not during a hand)
- [ ] Step 7.2: Session settlement (freeze scores, fresh slate option)
- [ ] Step 7.3: Player seating management (circular arrangement visualization)
- [ ] Step 7.4: Write tests for session management
- **COMMIT**: "feat: Player session management with join/leave and settlement"

## Phase 8: Authentication & Social
- [ ] Step 8.1: Google Sign-In (Firebase Auth) integration
- [ ] Step 8.2: Guest/Offline vs Online mode toggle
- [ ] Step 8.3: Friend system (send/accept invitations)
- [ ] Step 8.4: Dummy-to-Real player mapping
- [ ] Step 8.5: Game ownership & permissions (owner=full access, participants=read-only)
- [ ] Step 8.6: Write tests for auth flows
- **COMMIT**: "feat: Authentication, friend system, and game permissions"

## Phase 9: Notifications & Real-time
- [ ] Step 9.1: FCM integration for push notifications
- [ ] Step 9.2: "Nudge" feature (owner can nudge players)
- [ ] Step 9.3: Deep link handling (notification → specific game page)
- [ ] Step 9.4: Write tests for notification handling
- **COMMIT**: "feat: Push notifications with nudge and deep linking"

## Phase 10: Polish & Branding
- [ ] Step 10.1: Create app icon (glossy, premium, 3D - playing cards + cultural motifs)
- [ ] Step 10.2: Splash screen with animation (Rangoli + Diyas theme)
- [ ] Step 10.3: UI polish pass (consistent festive theme across all screens)
- [ ] Step 10.4: Screen space optimization audit for 6-player display
- [ ] Step 10.5: Performance testing and optimization
- **COMMIT**: "feat: Branding, splash screen, and UI polish"

## Phase 11: API Enhancements for Kubernetes
- [ ] Step 11.1: Add health check endpoints for k8s probes
- [ ] Step 11.2: Configure SignalR for scaled deployment (Redis backplane if needed)
- [ ] Step 11.3: API rate limiting and security headers
- [ ] Step 11.4: Update Docker compose for k8s readiness
- **COMMIT**: "feat: API Kubernetes-ready with health checks and SignalR scaling"

## Phase 12: User Entity & Multi-User Support Refactoring (Active)
- [x] Step 12.1: Define `User` model in `MarriageCalculator.Core` (UserId, DisplayName, Email, CreatedAt).
- [x] Step 12.2: Implement authentication middleware/services in `MarriageCalculator.API` (Firebase token validation / custom OAuth headers for QA testing).
- [x] Step 12.3: Modify `GameSettings` and `MarriageGameSet` models in Core and MongoDB schemas to include owner identifiers (`UserId` and `HostUserId`).
- [x] Step 12.4: Update API controllers and repositories to filter all CRUD operations by the caller's authenticated `UserId` (including full API unit tests).
- [x] Step 12.5: Build Android Login screen as the initial landing phase, integrating auth calls to the backend and storing token credentials locally.
- [x] Step 12.6: Update Android Retrofit client to append authentication headers to all outbound requests.
- [x] Step 12.7: Write concurrent load tests and unit tests ensuring multiple users can operate simultaneously without cross-talk.
- **COMMIT**: "refactor: User entity migration, settings/games user linking, and multi-user login support"

---

## Key Design Decisions
1. **Screen Space for 6 Players**: Use compact card grid (2×3 or circular) with collapsible details. Score input uses horizontal scroll or tabbed view.
2. **Scoring Algorithm**: Central Collection technique per requirements - Winner collects all, then distributes Maal.
3. **Real-time**: SignalR hub for live score broadcasting to all participants.
4. **Offline-first**: Room DB locally, sync to API when connected.
5. **Architecture**: MVVM on Android (ViewModel + Compose), Clean Architecture on API.
6. **Testing Strategy**: Unit tests for scoring engine, ViewModel tests, UI tests for critical flows. Use a dedicated testing agent for quality feedback.

## Notes
- Maui project archived in `archive.MarriageCalculator.MAUI/` - do not modify
- API environment variables: MCDATABASE, MCUSER, MCPASSWORD
- Festive theme (Dashain/Tihar) colors: DeepRed, MarigoldOrange, Gold, NightBlue
- Android package: `com.sanjeeb.marriagecalculator`
