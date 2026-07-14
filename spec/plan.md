# Marriage Game Calculator - Android App Implementation Plan

## Problem Statement
Build a full-featured Android (Kotlin/Compose) app for the Marriage card game calculator, backed by the existing .NET API. The app must handle 2-6 players with efficient screen space usage, support offline/online modes, real-time score display, and integrate with the C# API hosted on Kubernetes. The Maui version is archived and replaced by this native Android app.

## Status: Phases 1-28 COMPLETE ✅ (except 25.9: Android invite-code UI, pending)
- 44 C# tests (12 Core + 32 API) + 74 Android unit tests all passing
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

## Phase 19: Delete Game / Round / Last Game (Complete)
Three levels of destructive edit, scoped down per user clarification: only the single most-recently-played game can be deleted (undo), any whole round can be deleted (cascades its games+scores, later rounds renumber down), and the entire game set can be deleted (cascades everything). All host-only, all gated on the game set not being settled, all require an in-app confirmation dialog first.
- [x] Step 19.1: **API** — `MarriageGameSetService.DeleteLastGameAsync(gameSetId, hostUserId)`: removes the highest-`Sequence` game in the highest-`Sequence` round; deletes the now-empty round too if that was its only game; un-marks `Completed` on the round if it had just been auto-completed by that game. `DeleteRoundAsync(gameSetId, roundId, hostUserId)`: deletes every game+score in the round, then decrements `Sequence` on every later round in the same game set so numbering stays contiguous. `DeleteGameSetAsync` fixed to actually cascade-delete every round/game/score first — previously it only deleted the `MarriageGameSet` document itself, orphaning everything else in Mongo (a latent bug, never exercised since the endpoint existed but had no Android caller). All three require `IsActive` (not settled) and host ownership, throwing `InvalidOperationException`/`UnauthorizedAccessException`/`KeyNotFoundException` mapped to 400/403/404 in the controller.
- [x] Step 19.2: **API** — New endpoints: `DELETE MarriageGameSets/{id}/games/last`, `DELETE MarriageGameSets/{id}/rounds/{roundId}`. Both added to `IMarriageGameSetService`/`MarriageGameSetsController`. 7 new controller tests (success, empty-round-removed, settled→400, non-host→403, round not-found→404). `dotnet test`: 44/44 (12 Core + 32 API).
- [x] Step 19.3: **Android online** — `deleteLastGame`/`deleteRound` added to `MarriageGameSetApiService` (Retrofit) and `GameSetRepository`; `deleteGameSet` repository wrapper added too (the Retrofit method already existed but had no repository caller, so it was dead code before this). **Bug found & fixed during live verification**: all three were first wired through the existing generic `safeApiCall<T>`, which treats a null response body as an error — but these endpoints can legitimately return `204 No Content` on success, which live-tested as a spurious "Empty response body" error even though the delete had actually succeeded server-side. Added a `safeUnitApiCall` that only checks `response.isSuccessful`, ignoring body presence, and switched all three delete calls to it.
- [x] Step 19.4: **Android offline (Room)** — Added `deleteLastGame`, `deleteRoundGames` (deletes the round's game rows + scores, then compacts every remaining game's `roundNumber` to stay contiguous 1..N by chronological order), and `deleteGameSet` (cascades round_scores → rounds → game_set_players → game_sets) to `OfflineGameRepository`, backed by new delete/renumber DAO queries on `RoundDao`, `RoundScoreDao`, `GameSetPlayerDao`, `GameSetDao`.
- [x] Step 19.5: **Android ViewModel** — `PlayGameViewModel.deleteLastGame`, `deleteRound`, `deleteGameSet` added, each branching online/offline like the existing `closeCurrentRound`; the first two reload via `loadGame()` after, the last takes an `onDeleted` callback (navigates back) since the game set no longer exists to reload.
- [x] Step 19.6: **Android UI** — `RoundBlock`'s header row gains an Undo icon (only on the latest round, only if it has games) and a Delete-round icon (any round with games), both host-only; `PlayGameScreen`'s `TopAppBar` gains a `MoreVert` overflow menu with "Delete Game" (host-only). All three funnel through a shared `ConfirmDeleteDialog` composable requiring an explicit tap before anything is deleted.
- [x] Step 19.7: Verify — `dotnet test` 44/44; `gradlew testDebugUnitTest`/`assembleDebug` green; rebuilt and redeployed the Docker API container; live end-to-end on the emulator against the real online account: deleted a middle round and confirmed later rounds renumbered down (5→4, 4→3) while earlier rounds were untouched; hit the `safeApiCall` 204-body bug on the very first live attempt, fixed it, and reverified deletion completes with no error and an immediate UI refresh; used "Undo Last Game" on a 1-game round and confirmed the round collapsed back to the blank-placeholder "no games yet" state; created a disposable throwaway game set and confirmed "Delete Game" removes it entirely from the Active Games list and navigates back (left the original `2026-07-08` test game's remaining history untouched by cancelling that confirmation instead of confirming it).
- **COMMIT**: "feat: add delete for last game, round, and whole game set"
- [x] Step 19.8: **Bug found & fixed post-release** — user reported that after deleting a game set, it stayed visible (and openable) on the Dashboard's Active Games list, and opening it threw an API error (404, since it really was gone server-side). Two independent root causes, both fixed:
  1. `DashboardViewModel` only loaded `activeGames` once in `init {}`. Since it's scoped to the Dashboard's nav back-stack entry (which lives for the whole session, not recreated by navigating to `PlayGameScreen` and back), the list never refreshed after any mutation made elsewhere. Fixed by adding a `DisposableEffect` + `LifecycleEventObserver` in `DashboardScreen` that calls `loadActiveGames()` on every `ON_RESUME`.
  2. Deeper cause: creating a game while online *also* writes a local Room "mirror" row (`OfflineGameRepository.createGameSetWithRemoteId`, used so the Dashboard has something to show before/without a full remote fetch). `DashboardViewModel`'s online/offline merge logic keeps any local row whose `remoteId` isn't in the freshly-fetched remote list, on the assumption that means "not yet synced" — but deleting the set online produces exactly the same signal (remoteId now absent from the remote list), so the stale local mirror got resurrected into the list every time, even with fix #1 applied. Fixed by adding `OfflineGameRepository.deleteGameSetByRemoteId` (looks up the local row by `remoteId`, cascades its deletion same as the offline path) and calling it from `PlayGameViewModel.deleteGameSet`'s online branch right after the remote delete succeeds.
  - Verified live: rebuilt, reinstalled, deleted a leftover orphaned entry from before the fix (confirming the fix cleans up *pre-existing* orphans too, not just future ones), then force-stopped and cold-relaunched the app to prove the entry does not reappear — it stayed gone, unlike before the fix.
- **COMMIT**: "fix: refresh dashboard on resume and clean up local mirror on game-set delete"
- [x] Step 19.9: **Bug found & fixed post-release** — user reported seeing "Round Complete! Every player has dealt" while "the last round has 0 games... all 0" on screen, and questioned how that satisfies "every player must deal in the same round." Root cause was UI clarity, not a data bug: `CompactRoundsTable` synthesizes a purely-cosmetic preview round card (0 games, blank placeholder row) whenever no real round is open, so the table always shows what the next round will look like. Sorted latest-first, that synthetic card sits directly under the "Round Complete!" banner - so it reads as if the completed round is the empty one, when the banner actually refers to the real (fully-dealt) round further down. Fixed two ways: the banner now names the round ("Round 2 Complete!" instead of generic "Round Complete!"), and a round header only says "in progress" once it has at least one real game — an empty synthetic round now says "not started" instead. Verified live against the real `2026-07-08` game (which was already sitting in exactly this state from Phase 19's round-deletion testing): banner now reads "Round 2 Complete!" and the empty card above it reads "Round 3 · not started."
- **COMMIT**: "fix: clarify which round completed vs the not-yet-started placeholder"

---

## Phase 20: Per-Round Seat Order + Round-Relative Dealer Rotation (Complete)
User feedback on the actual table rules: after a round completes, players reshuffle seats (via the app or by drawing cards manually) and the person who drew the lowest card - seated LAST in the list - deals the new round's first game. Two gaps vs. that: dealer rotation was computed from the *overall* game count (so a round closed early skewed who deals next round), and seat order was a single global list (a reshuffle silently rewrote every historical round's column order).
- [x] Step 20.1: **API** — `MarriageGameRound` gains `PlayerIds`: the game set's seat order snapshotted at round creation in `SubmitRoundAsync`, exposed through `MarriageGameRoundDto`. Legacy rounds have an empty list; clients fall back to the game set's current order for them.
- [x] Step 20.2: **Android (offline/Room)** — `RoundEntity` gains `seatOrder` (CSV of player ids at save time; DB v3→4, destructive). A logical round's seat order = its first game's snapshot. New `OfflineGameRepository.getOpenRoundState(gameSetId, playerCount)` returns games-in-open-round + that round's seat order, reusing the same bucket-chunking rule as display.
- [x] Step 20.3: **Android** — `RoundGroup` gains `seatOrder: List<Player>`; new shared `nextDealerFor(seatOrder, gamesInOpenRound)` = `seatOrder[(size - 1 + gamesInOpenRound) % size]` - i.e. the round's first game is dealt by the LAST seat, then the deal wraps to the top. Replaces the global `(totalGamesPlayed - 1 + size) % size` formula in both `PlayGameViewModel` branches AND `RoundInputViewModel.loadGameData` (which now derives the open round from the game set / Room instead of the passed game number, and lists players in the round's seat order). Standings' DEALER chip now matches by player id rather than list index.
- [x] Step 20.4: **Android UI** — `RoundBlock` renders `group.seatOrder` (falling back to the current order for legacy rounds), so each round keeps the columns it was played with; the not-started preview round uses the game set's current (possibly just-reshuffled) order.
- [x] Step 20.5: Verify — `dotnet test` 44/44, `gradlew testDebugUnitTest`/`assembleDebug` green, Docker API rebuilt+redeployed; live on the emulator: with Rounds 1-2 both closed early after 1 game (previously skewing the old formula to seat #2), the Round 3 preview's D badge correctly sat on the LAST seat; Arrange Seats dialog's DEALER chip tracked the last seat through a shuffle; RoundInput assigned the same dealer and listed players in the new order; after submitting a game, the next pending game's D badge wrapped to the FIRST seat; a second mid-flight reshuffle left the open Round 3's columns untouched (snapshot held) while legacy Rounds 1-2 re-rendered in the new order with per-player data still correct; the test game was then removed via Undo Last Game, restoring the "not started" preview under the newest order with D on its last seat.
- **COMMIT**: "feat: per-round seat order snapshots and round-relative dealer rotation"
- [x] Step 20.6: **Follow-up (user feedback)** — Two gaps in 20.1-20.5 for *legacy* data: (a) rounds created before snapshots existed still re-rendered on every reshuffle (their fallback is the live game-set order), and (b) games saved before dealer recording existed showed no D badge, so only recent games appeared flagged. Fixes: **freeze-on-reshuffle** — when the seat order actually changes, `UpdateGameSetAsync` (API) and `updateGameSetPlayerPositions` (Room) first stamp every still-unsnapshotted round/game with the *outgoing* order, so completed rounds lock to the seating they were played with and never move again; **derived dealer badges** — when a game has no stored dealer (blank/0), `PlayGameViewModel` derives it from the round's rotation rule (`nextDealerFor(seatOrder, seq - 1)`), so every game row shows its dealer circling one after another. **Bug found & fixed during live verification**: the first freeze attempt used a LINQ filter `PlayerIds.Count == 0`, which Mongo translates to `$size: 0` — that does NOT match legacy documents where the `PlayerIds` field is absent entirely, so nothing was stamped and rounds 1-2 followed the shuffle again. Replaced with an explicit `Exists(false) OR Size(0)` filter. Verified live: after redeploy, a reshuffle left Rounds 1-2 frozen at their outgoing order (PLA AAR APP SUS SAN) while the not-started Round 3 preview followed the new order (SAN SUS PLA AAR APP) with the D badge on the new last seat.
- **COMMIT**: "fix: freeze legacy rounds' seat order at reshuffle and derive missing dealer badges"
- [x] Step 20.7: **Follow-up (user feedback)** — "Each game data needs to be saved in database, rather than calculate again, or try to generate dynamically." The 20.6 freeze only persisted legacy history lazily (at the *next* reshuffle), and derived dealer badges lived only in the ViewModel. Replaced with an eager one-time persist on load: `MapToDtoAsync` stamps any round still missing `PlayerIds` with the game set's current order (same Exists/Size filter as 20.6) and `BuildRoundDtoAsync` computes-and-persists `DealerId` for any game missing one, using the round's stored rotation; offline mirrors this via `OfflineGameRepository.backfillRoundHistory` (blank `seatOrder` rows stamped, `dealerId = 0` rows persisted from each bucket's stored seat order) called from `PlayGameViewModel`'s offline load. The reshuffle-time freeze and ViewModel-side derivation remain as defense-in-depth, but after the first load everything renders from stored data. Verified live: opened the game (triggering the persist), reshuffled once more — Rounds 1-2 stayed locked at their stored order while only the not-started preview followed the new seating.
- **COMMIT**: "fix: persist seat-order and dealer history to the database on first load"
- [x] Step 20.8: **Follow-up (user feedback)** — "Seat order can be fixed for round, as the shuffle happens only after round is completed." Enforced the rule everywhere, not just via snapshots: the Standings "Arrange Seats" action is disabled (dimmed, label "Seats locked until round ends") while any round is in progress; `UpdateGameSetAsync` (API) rejects a player-order change with 400 while an open round exists (controller maps `InvalidOperationException` → BadRequest); `updateGameSetPlayerPositions` (Room) throws the same guard offline, surfaced through `reorderPlayers`' existing error handling. The Reshuffle banner already only appears after round completion, so between-rounds reshuffling remains one tap. Verified live: with no round open, "Arrange Seats" is active; after submitting a game (Round 3 in progress), it reads "Seats locked until round ends" and doesn't respond to taps; the standings DEALER chip correctly moved to the top seat (deal wrapped after the last seat dealt game 1); the test game was then removed via Undo Last Game, restoring the not-started preview.
- **COMMIT**: "feat: lock seat rearrangement while a round is in progress"

---

## Phase 21: Winner Cell Glass Highlight (Complete)
- [x] Step 21.1: Each submitted game's winner is immutable, so the winner's cell in the rounds table gets a permanent frosted-glass pill: rounded 8dp rectangle, translucent white→green→transparent vertical gradient fill, and a light-catching gradient border (`CompactRoundCell` gains `isWinner`, set from the stored `game.winnerId` - never the pending placeholder row). The details dialog's green winner glow is unchanged.
- [x] Step 21.2: Verify — build/tests green; live on the emulator: Round 1's pill sits on AAR (400p winner) and Round 2's on SUS - correctly following the *stored* winner rather than the biggest earner (PLA out-earned SUS via maal that game but didn't win it).
- **COMMIT**: "feat: frosted-glass highlight on each game's winner cell"
- [x] Step 21.3: **User feedback (screen compaction)** — Removed the "Round N Complete!" banner card entirely; reshuffling now lives as a small `Shuffle` icon right next to the not-started round's header ("Round 3 · not started ⤨"), which opens the same Arrange Seats dialog - it configures exactly that round's seating, and only appears when reshuffling is legal (host, no round in progress). Also removed the "ROUND" section title above the table. The table now starts directly under the top bar and all three rounds fit on one screen. Verified live: banner and title gone, icon present only on the not-started round, tapping it opens Arrange Seats with the DEALER chip on the last seat.
- **COMMIT**: "feat: replace reshuffle banner with header icon and drop ROUND title"
- [x] Step 21.4: **User feedback (currency display)** — Rates are entered in minor units for GBP/USD/AUD (pence/cents), but amounts should read in major units: 230p → £2.30, 230¢ → $2.30 (AUD: A$2.30). NPR/INR rates are whole rupees, displayed with their signs: ₨230 / ₹230 (matching the setup screen's "NPR (₨)" / "INR (₹)" labels). Implemented as `Currency.formatMoney(amount)` on the enum - the single source of money formatting - and rewired every display site to pass `Currency` instead of a symbol string: rounds-table cells/totals, Standings rows, round details dialog (PlayGameScreen), score preview boxes (RoundInputScreen), and player totals + Who-Owes-Whom settlement + round history (ScoreboardScreen); the old per-screen `currencySymbol()`/`formatMoney()` string helpers were deleted. Negatives format as -£2.30. Verified live on the GBP game: table shows £2.30/£0.30/-£2.90/£4.00, scoreboard shows £4.30 (43 pts × 10p) and settlements like "apple → Aariya Ojha £3.90".
- **COMMIT**: "feat: display money in major currency units with proper signs"

---

## Phase 22: Row-Tap Score Entry, Edit Previous Games, Frost Palette (Complete)
User feedback: the + FAB is redundant - the current game's blank row should BE the entry point; previous games must stay editable (with a warning that you're not editing the current game); and the gold text clashes with the glassmorphism design.
- [x] Step 22.1: **API** — `PUT MarriageGameSets/{id}/games/{gameId}` (`UpdateGameAsync`): re-scores an already-played game with corrected inputs via `ScoringEngine`; winner/seen/dublee/maal replaced, dealer and round position fixed; old score docs deleted and re-inserted; host-only, blocked when settled; 3 new controller tests (47 total: 12 Core + 35 API).
- [x] Step 22.2: **Android navigation** — `Screen.RoundInput` gains an optional `editGameId` query arg (`createEditRoute`); `PlayGameScreen` gains `onEditGame`.
- [x] Step 22.3: **Android RoundInput edit mode** — `RoundInputUiState.editGameId`; `loadForEdit` prefills players (in the game's stored seat order), seen/dublee/maal, winner, dealer from the stored game (online via game-set fetch, offline via new `getGameWithScores`), header reads "EDITING PREVIOUS GAME / Edit Game"; submit branches to `updateGame` (online PUT / offline `OfflineGameRepository.updateGame`, which replaces score rows and keeps dealer/seat/rounding fields).
- [x] Step 22.4: **Android game page** — FAB removed. The pending (current-game) row is highlighted with a frosted-glass pill and tapping it opens Add Score; tapping an already-played row shows "Modify Previous Game?" (message names the game number, warns scores will be recalculated) and on confirm opens the edit screen. The sequence-number cell still opens the details popup. Both taps host-only.
- [x] Step 22.5: **Colors** — new frost palette in theme (`FrostWhite` 0xFFE9EEF6, `FrostBlue` 0xFFA6BEE0): all gold accents inside the table, standings, and tooltip replaced (round titles, column initials, seq numbers, Σ, Maal/Points tabs, D badges, DEALER chip, icons); top bar and dialogs keep the festive gold branding.
- [x] Step 22.6: Verify — `dotnet test` 47/47, Android tests/assemble green, Docker API redeployed; live: pending row tap opened Add Score; previous-game tap showed the warning; Modify opened the prefilled Edit Game screen (Total Maal 19, +23 pts/+£2.30 preview matching stored values, dealer label on AAR); saving with identical inputs round-tripped the new PUT and reproduced identical table values - rescoring is consistent.
- **COMMIT**: "feat: tap-to-score rows, edit previous games, and frost glass palette"

---

## Phase 23: Selectable Color Themes (Complete)
Four designed themes - 2 dark, 2 light - selectable in-app; the choice is a purely device-local setting (SharedPreferences), deliberately NOT synced to the database or API.
- [x] Step 23.1: **Palette model** — `AppPalette` (backgroundTop/Bottom, surface, accent, accentAlt, cta, frostText, frostAccent, textPrimary, tint) + `AppThemeOption` enum with the 4 designs: **Tihar Night** (default; the existing festive dark: night blue, gold, deep red), **Midnight Frost** (dark: slate blues, ice-blue accent, teal secondary), **Marigold Day** (light: warm ivory, bronze-gold accent, deep red CTA), **Himalayan Mist** (light: cool pale blues, slate accents). The `tint` field is the glassmorphism base - white on dark themes, ink on light - so `tint.copy(alpha=…)` overlays keep frosted pills/borders visible in both modes.
- [x] Step 23.2: **Plumbing** — `LocalAppPalette` CompositionLocal + `AppTheme.palette` accessor; `MarriageCalculatorTheme(theme)` builds a matching Material3 dark/light color scheme, flips status-bar icon appearance for light themes, and provides the palette; `ThemePreference` (@Singleton, SharedPreferences `app_theme`) exposes a StateFlow; `MainActivity` collects it so switching re-themes the whole app instantly.
- [x] Step 23.3: **Picker UI** — "App Theme" drawer item on the Dashboard opens a dialog grouped DARK/LIGHT, each option showing 3 palette swatches + name + check; selection applies immediately (`DashboardViewModel.setTheme`).
- [x] Step 23.4: **Color routing** — all hardcoded theme colors in Dashboard, PlayGame, Scoreboard, RoundInput, GameSetup, and Friend screens rewired to `AppTheme.palette.*` (including `Color.White` text → `textPrimary` and `Color.White.copy(…)` glass overlays → `tint.copy(…)`); Login/Splash and the metallic branded buttons keep their fixed festive look. Fixed two stragglers found live: Dashboard's screen/drawer gradients and FriendScreen's gradient/textfield had hardcoded dark bottoms that broke light themes.
- [x] Step 23.5: Verify — build/tests green; live on the emulator: picker shows all 4 with swatches; Midnight Frost re-themed the app instantly; Marigold Day verified on the game page (glass pending-row pill, winner pills, and frost headers all legible on ivory); Himalayan Mist verified; force-stop + relaunch came back in the selected theme (persistence); restored Tihar Night as the active default.
- **COMMIT**: "feat: four selectable color themes with device-local persistence"

---

## Phase 24: Add Friends - Actionable Push Notifications (Complete)
User self-implemented the client-side "Add Friends" feature (actionable Accept/Decline push notifications, deep linking, friend search/share UI); review found 2 blocking backend gaps and 1 Android reliability gap, all fixed.
- [x] Step 24.1: **Backend FCM wiring** — `FriendshipService` previously never called `IFcmService` at all, so no push was ever triggered on a new/reopened friend request or on acceptance. Injected `IFcmService`, added `SendFriendRequestPushAsync`/`SendFriendAcceptedPushAsync` private helpers (look up the target's `FcmToken`, no-op if absent), wired them into all three `SendFriendRequestAsync` branches (new, reopened-rejected, mutual-auto-accept) and into `RespondFriendRequestAsync` on accept.
- [x] Step 24.2: **Data-only FCM payload** — added `IFcmService.SendDataMessageAsync(token, data)` alongside the existing `SendNotificationAsync`, building a `Message` with no `Notification` block. Required because a message with a `Notification` block is auto-rendered by the OS when the app is backgrounded/killed, bypassing the client's `onMessageReceived()` — which is where the Accept/Decline action buttons are built. `FRIEND_REQUEST`/`FRIEND_ACCEPTED` pushes now use the data-only path.
- [x] Step 24.3: **`FriendRequestReceiver.goAsync()`** — the receiver's `onReceive()` starts a coroutine to call the respond-friend-request API, but `onReceive()` returning early can let the OS reclaim the process before the network call finishes. Added `goAsync()`/`pendingResult.finish()` so the receiver's process lifetime extends to cover the async call.
- [x] Step 24.4: Verify — `dotnet build`/`dotnet test` (47/47 passing) and Android `testDebugUnitTest`/`assembleDebug` green; Docker API rebuilt/redeployed. Live on emulator: sent a real friend request (Aariya → Sanjeeb) — confirmed via direct Mongo query that `SendFriendRequestPushAsync` correctly no-ops when the receiver has no `FcmToken` (expected, not a bug). Planted a test `FcmToken` on the receiver's user document, re-sent the request, and confirmed via Docker logs that the backend attempted a real (non-mock) `FirebaseMessaging.SendAsync` call with the data-only payload, failing only because the planted token wasn't a real device token — proving the send path fires correctly end-to-end. Test data (dummy token, test friendship) cleaned up after verification.
- **Note (not fixed, out of scope)**: the friend-request delete endpoint appears to return an empty body that the Android client fails to parse, surfacing a spurious "Empty response body" error toast even though the delete succeeds server-side. Minor pre-existing client bug in the user's own implementation, flagged but not part of this fix.
- **COMMIT**: "fix: wire FCM push notifications for friend requests and harden background receiver"

---

## Phase 25: Private Friend Discovery — Invite Codes & Email Invites (API complete, Android pending)
Per requirement §4.4 "Private Friend Discovery": open user search is a privacy leak (partial-match harvesting of emails/names, and "User not found" errors let callers probe which emails are registered). Replaced by two private paths: a shareable 7-day multi-use invite code (redeeming = instant, auto-accepted friendship) and complete-email requests (pending request if registered, invitation email + claimable invite if not — identical response either way).
- [x] Step 25.1: **Core** — `FriendInviteCode` and `PendingEmailInvite` models; `Friendship.Source` (optional: "Code" | "Email"); DTOs (`InviteCodeDto`, `RedeemInviteCodeDto`, `RedeemInviteCodeResultDto`, `FriendRequestResultDto`, `ClaimInvitesResultDto`).
- [x] Step 25.2: **API data** — `friendInviteCodes` + `pendingEmailInvites` collections in `MongoDbContext`; startup `EnsureIndexesAsync` (unique code, TTL on both `ExpiresAt` fields, invitee-email lookup index) called from `InitializeDatabaseAsync`.
- [x] Step 25.3: **API repositories** — `IFriendInviteCodeRepository` + `IPendingEmailInviteRepository` (+ implementations).
- [x] Step 25.4: **API email** — `IEmailService` + `SmtpEmailService` (System.Net.Mail, config section `Email`; disabled no-op with warning when unconfigured). `App:DownloadUrl` config for the invite mail body. **Email:Host must be configured with an SMTP provider (e.g. Brevo/SendGrid free tier) before email invites actually send.**
- [x] Step 25.5: **API service** — `FriendInviteService`: get-or-create code (reuses active), redeem (rate-limited 5/10min via IMemoryCache, generic invalid/expired error, auto-accepted friendship, masked-email confirmation, FCM push `FRIEND_ADDED_VIA_CODE` to code owner), claim pending invites on login (invite → pending friendship + FRIEND_REQUEST push).
- [x] Step 25.6: **API service** — `FriendshipService.SendFriendRequestAsync` reworked: exact-email match only (display-name search removed), unknown email → stored `PendingEmailInvite` + invitation email, **identical generic response** whether or not the user exists; returns `FriendRequestResultDto`. `UserService.SearchUsersAsync` restricted to exact-email match (partial search removed — `GET Users/search` keeps its shape for old clients but no longer matches partially).
- [x] Step 25.7: **API endpoints** — `POST Friendships/invite-code`, `POST Friendships/invite-code/redeem`, `POST Friendships/claim-invites`; `POST Friendships/request` now returns `FriendRequestResultDto` (**breaking for the Android client** — see 25.9).
- [x] Step 25.8: **Tests + verify** — 7 new/updated controller tests (invite code, redeem incl. masked-email + BadRequest paths, claim count, anti-enumeration generic response, MaskEmail) in `FriendshipsAndPermissionsTests.cs`. Verified by full-source compile of Core, API, and API.Tests (Roslyn, all green). **Note**: the coding session's Linux sandbox had no NuGet access, so `dotnet build` + `dotnet test` must be re-run on the dev machine to confirm — expected green.
- [ ] Step 25.9: **Android (follow-up, not started)** — Friend screen: replace search-based UI with "My invite code" (show/share/copy) + "Enter code" + "Add by email"; handle `FriendRequestResultDto`; call `claim-invites` after login; handle new FCM type `FRIEND_ADDED_VIA_CODE`.

---

## Phase 26: Compact Round Input Screen (Complete, pending device verification)
User feedback with screenshot: the Round Input page's tall per-player cards (~180dp each: avatar header, toggle buttons, conditional Maal field, preview bar) forced scrolling with 4+ players. Replaced with a single tabular grid so all players fit on one screen — consistent with the compact-table language of Phases 15/16.
- [x] Step 26.1: `RoundInputScreen.kt` rewritten — `PlayerScoreCard` (card per player) replaced by one grid card: header row `PLAYER | 🏆 | SEEN | DUB | MAAL` + one zebra-striped row per player. Fixed column widths shared between header and rows; winner row tinted with accent.
- [x] Step 26.2: Row cells — small avatar (26dp) + name with "D" dealer badge; trophy tap-to-select winner; icon-checkboxes for Seen (locked for winner) / Dublee; compact `BasicTextField` Maal input (44×32dp) + mini calculator icon, both only active when Seen ("—" otherwise). Live points/money preview renders as a small colored line under the player name (no + prefix, per Phase 16 convention).
- [x] Step 26.3: Header compacted — round title and Total Maal chip share one line; ViewModel untouched (pure UI change), MaalCalculatorDialog/submit/discard flows unchanged.
- [x] Step 26.4: Verify — `./gradlew testDebugUnitTest` + `assembleDebug` green on the dev machine (run during Phase 28). Visual check on emulator during the Phase 28 walkthrough: all 5 players fit on one screen in the grid, Seen toggle reveals the Maal field + calculator icon, calculator dialog applies to the field and header chip, Discard & Return works. (Winner/dublee toggles not individually exercised.)

---

## Phase 27: Maal Scoring Correction — Progressive Alter/Manuk (Complete, pending device verification)
User correction of Phase 13's house-rule defaults: Alter and Manuk (printed joker) score in **progressive tiers**, not per card — 1 = 5, 2 = 15, 3 = 30 (max 3 of each exist). "3 alters that are also a tunnella = 35" needs no special case: it's 3 Alters (30) + 1 Tunnel (5).
- [x] Step 27.1: `MaalItem` gains `maxCount` + `progressive` flags; ALTER and MANUK defaults 1→5, capped at 3, progressive. `MaalCalculator.total` applies ×1/×3/×6 tier multipliers (scaling from the adjustable base value so house-rule edits stay proportional); `increment` clamps to the per-item max; new `itemPoints` helper.
- [x] Step 27.2: `MaalCalculatorDialog` shows the tier table ("1 = 5 · 2 = 15 · 3 = 30 pts") instead of "5 pts each" for progressive items.
- [x] Step 27.3: `MaalCalculatorTest` updated + new cases: tier values for Alter and Manuk, 3-alters+tunnel = 35, cap at 3 (stepper and stale persisted counts), custom-base scaling, updated defaults.
- [x] Step 27.4: Business rule recorded in `.agent/memory.md` §2.
- [x] Step 27.5: Verify — superseded by Phase 28 before device verification: the progressive-multiplier model was replaced by fixed per-item tier tables (which keep Alter/Manuk at 5/15/30). Tests/build green as part of Phase 28.

---

## Phase 28: Maal Calculator Validation — Fixed Tier Tables & Physical Max Counts (Complete)
User supplied the definitive scoring rules (3-deck game): every maal item scores in fixed tiers by count — not per-card multiples — and each item's count is capped at what can physically exist. Replaces Phase 27's progressive-multiplier model and Phase 13's adjustable per-card values with hard rules.
- [x] Step 28.1: **Model** — `MaalItem` rewritten around a `tiers: List<Int>` table where `tiers[n-1]` = total points for holding n; `maxCount` = tier count. Rules: Tiplu 3/8 max 2 (3rd tiplu is the table maal card); Poplu & Jhiplu 2/5/10 max 3; Marriage 10/25 max 2; **Tunnela** (renamed from "Tunnel", Nepali) 5/15/30/45 max 4 (more possible but vanishingly rare); new **Poplu/Jhiplu Tunnela** 10/30/45 max 3 (Tiplu tunnela can't exist); new **Alter Tunnela** flat 35; new **Joker Tunnela** flat 35 (user: same as alter tunnela); Alter and Manuk keep 5/15/30 max 3 (user: keep as-is).
- [x] Step 28.2: **Point values are fixed rules** — removed the "Adjust point values" toggle, the per-item `values` map, `defaultValues()`, and the value steppers from `MaalCalculatorDialog` (user: "it's fixed rule"). Each row now shows its tier table ("1 = 3 · 2 = 8 pts"); the + stepper visually disables at the item's max.
- [x] Step 28.3: **Tests** — `MaalCalculatorTest` rewritten: exact tier assertions for all 10 items, per-item caps (stepper increments and stale oversized counts both clamp), 99 total clamp, zero/negative-count safety, and an every-item invariant (`maxCount == tiers.size`, `points(max) == tiers.last()`).
- [x] Step 28.4: Verify — `testDebugUnitTest` + `assembleDebug` green; live on emulator: dialog shows all items with tier labels, 3 taps on Tiplu + stopped at 2 with the + button greyed out, total showed the tiered 8 (not linear 6), adding 2 Tunnela gave 23 (8+15), Apply wrote 23 to the player's Maal field and header chip; input then discarded to leave game data untouched.
- **COMMIT**: "feat: fixed Maal tier tables with physical max-count validation"
- [x] Step 28.5: **Points table popup** — info icon in the calculator's header opens a "Maal Points" reference dialog: rows = all 10 maal items (short names), columns ×1..×4 showing the tier totals, "–" beyond an item's max, with a "totals, not per card" footnote. Zebra-striped, scrolls if needed. Verified live on emulator.
- **COMMIT**: "feat: maal points reference table popup in calculator"

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
