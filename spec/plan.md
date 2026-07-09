# Marriage Game Calculator - Android App Implementation Plan

## Problem Statement
Build a full-featured Android (Kotlin/Compose) app for the Marriage card game calculator, backed by the existing .NET API. The app must handle 2-6 players with efficient screen space usage, support offline/online modes, real-time score display, and integrate with the C# API hosted on Kubernetes. The Maui version is archived and replaced by this native Android app.

## Status: Phases 1-18 COMPLETE ✅
- 37 C# tests (12 Core + 25 API) + 74 Android unit tests all passing
- Android APK builds successfully (`assembleDebug`)
- .NET API builds successfully
- **Docker**: Production-ready for API deployment.

## Approach
Iterative development in phases. Each step produces a buildable, testable commit. Use SignalR for real-time updates. Build scoring engine in C# Core, expose via API, consume in Android via Retrofit + OkHttp.

---

## Phase 1: Foundation & Navigation (Android)
- [x] Step 1.1: Create branch `android` from main
- [x] Step 1.2: Add dependencies (Retrofit, OkHttp, Gson, Navigation Compose, Hilt DI, Room for local storage, SignalR client)
- [x] Step 1.3: Set up navigation graph (Login → Dashboard → GameSetup → PlayGame → Scoreboard → RoundInput)
- [x] Step 1.4: Create data models mirroring C# Core models (Player, GameSettings, MarriageGame, MarriageGameScore, etc.)
- [x] Step 1.5: Create API service interfaces (Retrofit) for all endpoints
- [x] Step 1.6: Set up Hilt dependency injection module
- [x] Step 1.7: Write unit tests for data models
- **COMMIT**: "feat: Android navigation, DI, API client, and data models"

## Phase 2: Scoring Engine (C# Core + API)
- [x] Step 2.1: Implement Marriage game scoring engine in Core (Central Collection algorithm)
  - Handle Seen/Unseen/Dublee player states
  - Calculate Game Points (fixed penalties)
  - Calculate Maal Points (variable, player-to-player comparison)
  - Support Normal/Kidnap/Murder modes
  - Handle dynamic player count (2-6)
- [x] Step 2.2: Add scoring endpoint to API (POST /api/MarriageGames/calculate-score)
- [x] Step 2.3: Add SignalR hub for real-time score broadcasting
- [x] Step 2.4: Write comprehensive unit tests for scoring engine (all 3 modes, edge cases)
- **COMMIT**: "feat: Marriage game scoring engine with Normal/Kidnap/Murder modes"

## Phase 3: Dashboard & Game Setup Screen (Android)
- [x] Step 3.1: Build Dashboard screen (greeting, New Game, Join Game, History buttons)
- [x] Step 3.2: Build Game Setup screen
  - Player selection/creation (2-6 players) with circular seating arrangement
  - Currency selection (NPR, INR, GBP, USD, AUD priority)
  - Game settings (PointRate, SeenPenalty, UnseenPenalty, DubleeBonus, GameMode toggles)
  - Settings lock mechanism once game starts
- [x] Step 3.3: Create ViewModels for Dashboard and GameSetup
- [x] Step 3.4: Connect to API (create game set, save settings, add players)
- [x] Step 3.5: Write UI tests for game setup flow
- **COMMIT**: "feat: Dashboard and Game Setup screens with API integration"

## Phase 4: Round Input Screen (The Scorer)
- [x] Step 4.1: Build Round Input screen optimized for 6 players
  - Compact player cards showing name + status
  - Winner selection (tap to select)
  - Seen/Unseen/Dublee toggle per player
  - Maal numeric input per player
  - Dealer indicator
- [x] Step 4.2: Implement score calculation display (instant preview before submit)
- [x] Step 4.3: Handle round submission to API with validation
- [x] Step 4.4: Support editing previous rounds (mistake correction)
- [x] Step 4.5: Write tests for round input validation and score preview
- **COMMIT**: "feat: Round Input screen with score calculation and validation"

## Phase 5: Scoreboard & History
- [x] Step 5.1: Build Scoreboard screen (real-time balances for all players)
  - Compact layout for 6 players (grid/table layout)
  - Color-coded positive/negative scores
  - Money calculator (points × rate in selected currency)
  - "Who owes whom" clear display
- [x] Step 5.2: Build Round History (scrollable list of past rounds with details)
- [x] Step 5.3: Integrate SignalR for real-time score updates
- [x] Step 5.4: Add settlement/freeze functionality
- [x] Step 5.5: Write tests for scoreboard calculations and SignalR integration
- **COMMIT**: "feat: Scoreboard with real-time updates and round history"

## Phase 6: Local Storage & Offline Mode
- [x] Step 6.1: Set up Room database with entities mirroring API models
- [x] Step 6.2: Implement offline-first pattern (local save → sync when online)
- [x] Step 6.3: Guest mode (no auth, local dummy players)
- [x] Step 6.4: Data sync logic — online mode writes directly to the API (remoteId + synced flag stored locally); offline/guest mode is local-only via Room. No background reconnect/merge queue exists yet (see Phase 14 candidate below).
- [x] Step 6.5: Write tests for offline storage and sync
- **COMMIT**: "feat: Offline mode with Room database and sync"

## Phase 7: Player Session Management
- [x] Step 7.1: Join/Leave between games (not during a hand)
- [x] Step 7.2: Session settlement (freeze scores, fresh slate option)
- [x] Step 7.3: Player seating management (circular arrangement visualization)
- [x] Step 7.4: Write tests for session management
- **COMMIT**: "feat: Player session management with join/leave and settlement"

## Phase 8: Authentication & Social
- [x] Step 8.1: Google Sign-In (Firebase Auth) integration
- [x] Step 8.2: Guest/Offline vs Online mode toggle
- [x] Step 8.3: Friend system (send/accept invitations)
- [x] Step 8.4: Dummy-to-Real player mapping
- [x] Step 8.5: Game ownership & permissions (owner=full access, participants=read-only)
- [x] Step 8.6: Write tests for auth flows
- **COMMIT**: "feat: Authentication, friend system, and game permissions"

## Phase 9: Notifications & Real-time
- [x] Step 9.1: FCM integration for push notifications
- [x] Step 9.2: "Nudge" feature (owner can nudge players)
- [x] Step 9.3: Deep link handling (notification → specific game page)
- [x] Step 9.4: Write tests for notification handling
- **COMMIT**: "feat: Push notifications with nudge and deep linking"

## Phase 10: Polish & Branding
- [x] Step 10.1: Create app icon (glossy, premium, 3D - playing cards + cultural motifs)
- [x] Step 10.2: Splash screen with animation (Rangoli + Diyas theme)
- [x] Step 10.3: UI polish pass (consistent festive theme across all screens)
- [x] Step 10.4: Screen space optimization audit for 6-player display
- [x] Step 10.5: Performance testing and optimization
- **COMMIT**: "feat: Branding, splash screen, and UI polish"

## Phase 11: API Enhancements for Kubernetes
- [x] Step 11.1: Add health check endpoints for k8s probes
- [x] Step 11.2: Configure SignalR for scaled deployment (Redis backplane if needed)
- [x] Step 11.3: API rate limiting and security headers
- [x] Step 11.4: Update Docker compose for k8s readiness
- **COMMIT**: "feat: API Kubernetes-ready with health checks and SignalR scaling"

## Phase 12: User Entity & Multi-User Support Refactoring (Complete)
- [x] Step 12.1: Define `User` model in `MarriageCalculator.Core` (UserId, DisplayName, Email, CreatedAt).
- [x] Step 12.2: Implement authentication middleware/services in `MarriageCalculator.API` (Firebase token validation / custom OAuth headers for QA testing).
- [x] Step 12.3: Modify `GameSettings` and `MarriageGameSet` models in Core and MongoDB schemas to include owner identifiers (`UserId` and `HostUserId`).
- [x] Step 12.4: Update API controllers and repositories to filter all CRUD operations by the caller's authenticated `UserId` (including full API unit tests).
- [x] Step 12.5: Build Android Login screen as the initial landing phase, integrating auth calls to the backend and storing token credentials locally.
- [x] Step 12.6: Update Android Retrofit client to append authentication headers to all outbound requests.
- [x] Step 12.7: Write concurrent load tests and unit tests ensuring multiple users can operate simultaneously without cross-talk.
- **COMMIT**: "refactor: User entity migration, settings/games user linking, and multi-user login support"

## Phase 13: Gameplay Aids & Hardening (Complete)
Covers requirement §3.2 "(Optional Advanced) Calculator for Maal" and §2.2 seating/dealing automation, plus codebase cleanup.
- [x] Step 13.1: Cleanup — remove stray duplicate package `np.com.marriage.calculator` (3 leftover files: LoginScreen.kt, theme/Color.kt, theme/Type.kt)
- [x] Step 13.2: Maal Calculator engine (pure Kotlin, `data/model/MaalCalculator.kt`)
  - Maal item types: Tiplu, Poplu, Jhiplu, Alter, Marriage, Tunnel, Manuk (Joker)
  - Configurable per-item point values with common defaults (Tiplu 3, Poplu 2, Jhiplu 2, Alter 1, Marriage 10, Tunnel 5, Manuk 1)
  - Auto-sum counts × values → total Maal
- [x] Step 13.3: Maal Calculator dialog in Round Input screen
  - Calculator icon next to the Maal field opens a stepper dialog per maal item
  - Total auto-fills the player's Maal input; counts preserved per player while on screen
- [x] Step 13.4: Seating draw engine (pure Kotlin, `data/model/SeatingDraw.kt`)
  - Each player draws a distinct card from a single 52-card deck
  - Highest card → 1st seat, descending order; ties broken by suit (Spades > Hearts > Diamonds > Clubs)
  - Lowest card → last seat = first dealer (per requirement §2.2)
- [x] Step 13.5: "Draw Cards" action in Rearrange Seats dialog
  - Draws for all players, shows each player's card, reorders seating automatically
- [x] Step 13.6: Unit tests for Maal calculator and seating draw (MaalCalculatorTest.kt, SeatingDrawTest.kt)
- [x] Step 13.7: Verify — `./gradlew testDebugUnitTest` (74 tests, BUILD SUCCESSFUL) and `assembleDebug` (BUILD SUCCESSFUL); `dotnet test` (30 tests, all passing)
- **COMMIT**: "feat: Maal calculator, seating card draw, and duplicate-package cleanup"

## Phase 14: Round History Table + Online-Mode Round Submission (Complete)
Triggered by a user request to see round-by-round Seen/Dublee/Maal/Points/Money in a spreadsheet-style table on the Scoreboard. Live testing on the emulator (an online, Google-authenticated account) surfaced a pre-existing, more serious bug: **Add Round and Scoreboard were completely non-functional for any online (non-guest) user.** `RoundInputViewModel` and `ScoreboardViewModel` only ever read/wrote local Room storage; for a remote MongoDB-backed game set (non-numeric ID) they silently no-opped. Worse, the API itself had no endpoint to persist a round's per-player scores at all — `POST MarriageGames` required a `MarriageGameRoundId` that nothing could create, and had no `Scores` field. Fixing the requested feature required fixing this first.
- [x] Step 14.1: **API** — `SubmitRoundDto`/`RoundPlayerInputDto` (`MarriageCalculator.Core/DTOs/ApiDtos.cs`) and `POST MarriageGameSets/{id}/rounds` (`MarriageGameSetsController.cs`, `MarriageGameSetService.SubmitRoundAsync`). Atomically creates the `MarriageGameRound` + its one `MarriageGame`, computes every player's score **server-side** via the existing `ScoringEngine` (never trusts a client-submitted score), and persists `MarriageGameScore` docs. Host-only (403 otherwise).
- [x] Step 14.2: **API** — `MarriageGameDto.MarriageGameScores` (keyed by playerId) added and populated in `MarriageGameSetService.MapToDtoAsync`, so `GET MarriageGameSets/{id}` now exposes per-player Seen/Dublee/Maal/Score, not just the summed `TotalScore`.
- [x] Step 14.3: **Android** — `RoundInputViewModel` branches online (remote `GameSetRepository`) vs offline (`OfflineGameRepository`) for both loading players/settings and submitting a round, mirroring the pattern already used in `PlayGameViewModel`.
- [x] Step 14.4: **Android** — `ScoreboardViewModel` gets the same online/offline branch for `loadScoreboardData`, building `RoundSummary`/`PlayerTotalScore` from the enriched `MarriageGameSet` DTO when online.
- [x] Step 14.5: **Android** — Round Input screen now shows a live Points/Money preview per player (was computed in `RoundInputViewModel.calculatePreview()` but never rendered).
- [x] Step 14.6: **Android** — New spreadsheet-style Round History Table (`ScoreboardScreen.kt`, replacing the old flat `RoundHistoryView`): one color-cycled block per round with Seen/Dublee/Maal/Points/Money sub-rows per player + Total Maal column, synced horizontal scroll (shared `ScrollState`) across header/rounds/total row, and a bottom Total row (money summed per player, zero-sum).
- [x] Step 14.7: Verify — unit tests updated (`ScoreboardViewModelTest`, new `ControllersTests` cases for submit-round success/forbidden/bad-request) and passing; `dotnet test` 35/35; `gradlew testDebugUnitTest`/`assembleDebug` both green; **live end-to-end verification on the emulator** against the real online account and a rebuilt Docker API container — submitted a real round, confirmed it appears correctly in both Standings and the new table (zero-sum: +40/-10/-10/-10/-10), confirmed horizontal scroll sync.
- **COMMIT**: "feat: round history table, online-mode round submission, and live score preview"
- **NOTE**: A test round (Round 1, winner Aariya Ojha) was submitted to the "2026-07-08" game set during live verification and is now real data in the shared dev database — left in place rather than guessing at cleanup semantics; flag to the user.
- [x] Step 14.8: **Android** — Round History Table locks the screen to landscape while showing (`ScoreboardScreen.kt` `LockScreenOrientation`, `MainActivity` gets `android:configChanges` so rotation doesn't recreate the Activity), fitting more player columns before horizontal scroll kicks in. Reverts to the prior orientation when leaving the table. Verified live: 5 players + Total Maal fit with zero scrolling in landscape.
- **COMMIT**: "feat: lock round history table to landscape for more player columns"

## Phase 15: Compact Rounds Grid on the Game Page (Complete)
The Scoreboard's spreadsheet-style table (Phase 14) is intentionally verbose (explicit Seen/Dublee/Maal/Points/Money rows) for a dedicated history view. The main game page ("Rounds Played" section of `PlayGameScreen`) needed a denser, icon-driven summary instead: one compact two-line cell per player per round (🏆 winner icon + 👥 dublee icon + points on top, money below; no row labels), with a per-round info icon opening a popup with the full breakdown.
- [x] Step 15.1: `RoundPlayerEntry` (shared model in `ui/scoreboard/ScoreboardViewModel.kt`) gains an `isWinner` field — needed a real signal for the trophy icon instead of the incorrect `score > 0` proxy (a seen non-winning player can still net a positive score).
- [x] Step 15.2: `PlayGameViewModel.RoundItem` gains `playerEntries: List<RoundPlayerEntry>` and `PlayGameUiState` gains `settings: GameSettings`, populated in both the online (`MarriageGameScore.winner`/`.duply`/`.seen`) and offline (`RoundScoreEntity.isWinner`/`.isDublee`/`.isSeen`) load paths, mirroring Phase 14's `ScoreboardViewModel` pattern.
- [x] Step 15.3: `CompactRoundsTable` + `CompactRoundCell` (`PlayGameScreen.kt`) replace the old flat `RoundItemRow` list — synced horizontal scroll (shared `ScrollState`) across a player-avatar header row and one row per round; unseen/non-winning cells are dimmed via alpha rather than a text label.
- [x] Step 15.4: `RoundDetailsDialog` — tapping the small info icon under a round's "R{n}" label opens a popup with the full per-player Seen/Dublee/Maal/Points/Money breakdown for that round.
- [x] Step 15.5: Verify — `dotnet test`/`gradlew testDebugUnitTest`/`assembleDebug` all green; live end-to-end on the emulator confirmed the compact grid renders correctly (trophy + points top row, money bottom row) and the info-icon popup opens with correct data.
- **COMMIT**: "feat: compact icon-driven rounds grid on the game page with detail popup"
- **RESOLVED**: The "Round 2" non-zero-sum concern noted above was a false alarm — a later screenshot of the same round's details popup (Phase 16) showed the true value was +10 (Sushma, winner), not +1; the earlier compact-grid screenshot had truncated the trailing digit at the screen edge. +3-7+23-29+10 = 0, correctly zero-sum. No scoring engine bug.

## Phase 16: Game Page UI Refinements (Complete)
A round of detailed UX feedback on `PlayGameScreen` tightening up Phase 15's compact grid and replacing the custom bottom action bar with standard Material components.
- [x] Step 16.1: Rounds sorted latest-first (`uiState.rounds.sortedByDescending { it.roundNumber }` at the call site, ViewModel state itself stays ascending since dealer-index math depends on count not order).
- [x] Step 16.2: Section order swapped — Rounds Played now above Standings.
- [x] Step 16.3: Round row's leading column is just the plain sequence number (no "R" prefix), and the number itself is the tap target for the details popup — the separate info icon is gone.
- [x] Step 16.4: Player column headers are first-3-letters text (e.g. "AAR"), no circle/avatar background.
- [x] Step 16.5: Positive values show as plain green text with no `+` prefix (negative still shows `-` naturally); applied across the compact grid, the details popup, and the Standings row.
- [x] Step 16.6: Cell content model changed — top row now shows Maal or Points (switchable via a "Maal"/"Points" tab control above the table, default Maal), bottom row always shows money; winner/dublee icons dropped from the compact cells (still visible in the details popup).
- [x] Step 16.7: Alternating row background (zebra striping) on the rounds table.
- [x] Step 16.8: `PlayerStandings` gains `totalMoney`, computed from `netPoints * settings.pointRate` in both online/offline load paths; Standings rows now show points + money stacked.
- [x] Step 16.9: Bottom action bar (`MetallicButton` "Add Round" + "Scoreboard") replaced with a standard Material3 `FloatingActionButton` (Add Round) and a `TopAppBar` icon button (Scoreboard, `Icons.Default.Leaderboard`).
- [x] Step 16.10: Transfer Host icon kept visible but disabled (`enabled = false`, dimmed tint) — feature isn't fully designed yet.
- [x] Step 16.11: Verify — `dotnet test`/`gradlew testDebugUnitTest`/`assembleDebug` all green; live end-to-end on the emulator confirmed every item above (latest-round-first ordering, section swap, tap-to-open on the number itself, 3-letter headers, no `+` signs, Maal/Points toggle switching correctly, visible zebra striping, Standings money column, FAB + top-bar Scoreboard icon navigating correctly, Transfer Host icon inert when tapped).
- **COMMIT**: "feat: redesign game page rounds table and replace bottom bar with Material FAB"

## Phase 17: Round Details Winner Highlight + Compact Standings (Complete)
- [x] Step 17.1: In `RoundDetailsDialog`, the winner's row gets a green glow (`Modifier.shadow` with green `ambientColor`/`spotColor`, a light green background tint, and a green border) plus a small trophy icon next to their name.
- [x] Step 17.2: `PlayerStandingsRow` made compact — smaller avatar (36dp → 26dp), tighter card padding, smaller fonts, reduced inter-row spacing (8dp → 4dp).
- [x] Step 17.3: Verify — `dotnet test` 35/35; `gradlew testDebugUnitTest`/`assembleDebug` green; live on the emulator confirmed the winner's row in the details popup has a visible green glow/border/icon and Standings visibly fits more rows on screen.
- **COMMIT**: "feat: highlight round-details winner and compact standings rows"

## Phase 18: Real Round = N Games Hierarchy (Complete)
Everything through Phase 17 treated one "Add Round" submission as both a round and a game (1:1), a simplification from Phase 14. This phase implements the actual card-game rule from requirement §2.2: a round consists of up to N games (N = player count), one deal per player; the round completes once everyone has dealt, or can be closed early.
- [x] Step 18.1: **API** — `MarriageGameSetService.SubmitRoundAsync` now appends a new `MarriageGame` to the latest still-open `MarriageGameRound` (creating a new round only if none is open), auto-marking the round `Completed` once `games.count == player count`. Extracted `BuildRoundDtoAsync` (shared by `MapToDtoAsync` and the new round-submission/close paths) to avoid duplicating the round→games→scores DTO-building logic.
- [x] Step 18.2: **API** — New `POST MarriageGameSets/{id}/rounds/{roundId}/close` (+ `CloseRoundAsync` service method) ends a round early (fewer than N games) so the next submitted game starts a fresh round. Host-only.
- [x] Step 18.3: **Android (offline/Room)** — `RoundEntity` (still one row = one game, name kept for schema continuity) gains `dealerId` and `closesRound` columns (DB version 2→3, destructive migration — acceptable pre-release). `OfflineGameRepository` gains `closeCurrentRound`. Logical-round grouping is derived at read time by chunking games into buckets of `playerCount`, closing a bucket early if a game has `closesRound = true` — mirrors the server's explicit `Completed` flag without needing a separate local "round" table.
- [x] Step 18.4: **Android** — `PlayGameViewModel` reworked around `RoundGroup` (a round: sequence, completed flag, its `GameEntry` list, total score per player) instead of a flat per-game list, for both online and offline load paths. `closeCurrentRound()` added, calling the new API/repository method then reloading.
- [x] Step 18.5: **Android UI** (`PlayGameScreen.kt`, full rewrite) —
  - Next Dealer banner removed from view (value still computed in `PlayGameUiState.nextDealerId`/`nextDealerName` for later use)
  - "ROUNDS PLAYED" renamed to "ROUND"
  - Every open round shows a synthetic blank placeholder game row (all-default values) at the top, purely UI-computed — not persisted until real data is submitted via the FAB
  - Small "D" badge on the dealing player's cell, per game
  - Per-round Total row (money summed across that round's games)
  - "Close Round" text action next to an in-progress round's header (host-only, hidden if the round has zero games yet)
  - Standings moved below the Round table, collapsed by default, expands on tap with an animated chevron
  - Tapping a player's 3-letter table header opens an animated (fade+scale) popup with their full name, email, and circular photo
- [x] Step 18.6: **Bug found & fixed during verification** — `PlayGameViewModel`'s new `nextDealerIndex` formula (`totalGamesPlayed % size`) didn't match `RoundInputViewModel`'s pre-existing dealer formula (`(roundNumber - 2 + size) % size`, where `roundNumber = totalGamesPlayed + 1`) — they were off by one, so the blank row's dealer badge showed a different player than who `RoundInputViewModel` would actually assign as dealer. Fixed `PlayGameViewModel` to use the algebraically equivalent `(totalGamesPlayed - 1 + size) % size`.
- [x] Step 18.7: Verify — `dotnet test` 37/37 (added `CloseRound` controller tests); `gradlew testDebugUnitTest`/`assembleDebug` green (74 tests); full live walkthrough on the emulator against the real online account and a rebuilt Docker API + fresh (destructively-migrated) local DB: legacy 1-game "rounds" from before this phase displayed correctly as individual completed rounds; added a game to auto-continue a round; verified the dealer badge on the blank row matched `RoundInputViewModel`'s actual assignment after the fix; completed a round via 5th game and confirmed a fresh round auto-started; tested "Close Round" on a 1-game round and confirmed it closed immediately with a new round appearing above it; confirmed Standings collapse/expand animation and the player-name popup (with photo/email) both work.
- **COMMIT**: "feat: real Round-contains-N-Games hierarchy with close-round support"
- [x] Step 18.8: **Follow-up** — `CompactRoundsTable` refactored so each round is a fully self-contained `RoundBlock` (own header row, own game rows, own total row, own horizontal `ScrollState`) instead of one shared header for all rounds. Anticipates reshuffles changing seating between rounds, and reads as a true repeater: each round is an independent unit. Only the Maal/Points mode toggle stays shared/global (a display preference, not round data). Verified live: each round now renders as its own card with matching header/rows/total.
- [x] Step 18.9: **Follow-up** — Player-name popup from 18.5 replaced a full-screen `Dialog` (black scrim covering the whole app) with a lightweight, anchored web-style tooltip. `RoundBlock`'s header `Box` now captures its own screen position/size via `onGloballyPositioned`/`positionInRoot`; a new `PlayerTooltipAnchor(player, position, size)` carries that to a `Popup` (not `Dialog`) using a custom `TooltipAbovePositionProvider` that centers the tooltip above the tapped cell (flips below if there isn't room), with `PopupProperties(focusable = false, dismissOnClickOutside = true)` for scrim-free outside-tap dismissal and a `LaunchedEffect(anchor) { delay(3000); onDismiss() }` for auto-dismiss. Verified live on the emulator (via combined adb tap+screenshot commands with explicit delays to catch the transient state): tooltip renders directly above the tapped 3-letter header without dimming/covering the rest of the screen, a tap outside closes it immediately, and it disappears on its own after 3 seconds.

---

## Backlog / Future Candidates (not started)
- **True offline reconnect sync**: requirement §3.4 calls out cloud sync as "(Future)". Today, online mode writes straight to the API and offline/guest mode is local-only Room storage (Phase 6.4) — there's no queued-write/merge engine that reconciles a game played offline once connectivity returns. Would need an outbox table + WorkManager sync job + conflict resolution rule (e.g. last-write-wins vs. host-wins) if pursued.

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
