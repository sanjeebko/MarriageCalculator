# Android UI/UX & Interaction Workflows Specification

## 1. Design Language: Festive Dashain & Tihar Theme

The UI embraces a rich, culturally immersive festive aesthetic celebrating South Asia's premier card-playing holidays (Dashain & Tihar).

### 1.1 Color Palette & Tokens
- **Background**: Deep Night Blue (`#0B101B`) or Rich Maroon.
- **Cards & Surfaces**: Glassmorphism translucent surfaces (`cardSurface.copy(alpha = 0.85f)`) with subtle golden/tint borders (`BorderStroke(1.dp, tint.copy(alpha = 0.12f))`).
- **Accent Primary**: Warm Golden Amber (`#FFC107` / `#E6A100`) evoking wealth and celebration.
- **Status Indicators**:
  - Positive Score / Winner: Bright Jade Green (`numberPositive`, `#00E676`).
  - Negative Score / Loser: Fiery Coral Red (`numberNegative`, `#FF5252`).
  - Zero / Neutral: Frost Muted Slate (`numberZero`, `#9E9E9E`).
- **Card Suit Accents**: Classical spade (♠), heart (♥), diamond (♦), and club (♣) motifs subtly integrated.

---

## 2. Screen Specifications

### 2.1 Login Screen (`LoginScreen.kt`)
- **First Screen on Launch**: Displays animated festive Rangoli branding.
- **Authentication Options**:
  1. Google Sign-In (Official Firebase OAuth flow).
  2. Quick Mock Sign-In (For local testing / QA).
  3. Continue as Guest (Offline mode, purely local Room storage).

### 2.2 Dashboard Screen (`DashboardScreen.kt`)
- **Career Stats Hero Card**:
  - Displays Total Matches, Games Played, Win Rate %, Total Net Earnings, and Personal Best Maal.
- **Quick-Start 1-Tap Launcher**:
  - Shows recent players chips. 1 tap creates a standard match without entering setup wizard.
- **Enriched Active Game Cards**:
  - Displays match name, current leader with crown (`👑 San (+₨240)`), active round status, suit emblem, and quick resume button.

### 2.3 Game Setup Screen (`GameSetupScreen.kt`)
- **Player Selection**: Add 2 to 6 players from friends or create local guest players.
- **Circular Seating Configuration**:
  - "Draw Cards" button simulates traditional card drawing (highest card gets 1st seat, lowest card deals 1st game).
- **Settings Configuration**: Currency (NPR, INR, GBP, USD, AUD), Point Rate, Seen Penalty, Unseen Penalty, Game Mode (Normal/Kidnap/Murder).

### 2.4 Play Game Screen (`PlayGameScreen.kt`)
The primary live table view during match play:
- **Visual Seating Ring (`VisualSeatingRing.kt`)**:
  - Authentic handcrafted Nepali carved wood table background asset (`nepali_wood_table.png`) featuring dark walnut/rosewood timber, intricate Newari floral and peacock border relief carvings, and an inlaid embossed brass mandala centerpiece.
  - Sized prominently to fill the card horizontally without excess whitespace or tiny visuals, with container height optimized to ~264dp.
  - Clean, unoccluded table surface: The trajectory arrow and traveling dot animation were eliminated, leaving the intricate Newari wood carving and brass mandala completely unobstructed.
  - Unified Player Seat Badge (`PlayerSeatBadge`): Combines enlarged profile images (38–42dp), centered circular seat number tokens (1..N), and player name text inside a single continuous stadium capsule pill with zero floating gaps.
  - Antique handcrafted embossed Nepali dealer coin (`NepaliDealerButton`, "D") and next dealer token (`NepaliNextDealerButton`, "›") pinned directly to the avatar container.
  - Rock-solid layout stability: Dealer halo animations utilize GPU-layer scaling (`graphicsLayer`) within a fixed footprint, completely preventing layout measurement thrashing, jitter, or vibration.
  - Dual-sided dealer smoke aura: The active dealer's unified badge features an ethereal, organic smoke effect wafting outward on both left and right sides via `Modifier.drawBehind` with smooth elliptical radial falloffs (`drawOval`) and animated harmonic breathing/drifting wisps (`smokePulse` and `smokeDrift`).
  - Dynamic Theme Adaptability: Badges, dealer aura smoke, dealer coins, seat number badges, and player names automatically adapt to the active app theme palette (`Tihar Night`, `High Contrast Dark`, `Midnight Frost`, `Marigold Day`, `Himalayan Mist`), displaying frosted surfaces and crisp typography across both light and dark modes.
- **Collapsible Previous Rounds**:
  - Previous rounds collapse into a single summary row showing player totals (`Σ`) to conserve screen space.
  - Expand/collapse toggles and "Expand All / Collapse All" button.
- **Adaptive Column Layout**:
  - Player columns automatically expand to fill 100% card width for 2, 3, or 4 players.
  - Maintains minimum 58dp column width and smooth horizontal scrolling for 5 and 6 players.
- **Pulsing FAB**:
  - Prominent "+ Record Game" floating button with breathing golden glow for fast thumb access.
- **Payment Cleared & Settle Up**:
  - Per-round "Payment Cleared ✓" toggles.
  - 1-Tap "Settle Up" cash settlement modal pairing debtors to creditors with copyable breakdown.
- **WhatsApp & Social Share Dialog**:
  - Exports a high-resolution 1080px branded match card bitmap.
- **Continue / Reopen Previous Round**:
  - Host can undo advancing to a new round if the new round has not yet started (0 games played).
  - Unstarted round header ("Round N+1 · not started") displays "Continue Round N" action with an undo icon.
  - Closed round header ("Round N") displays "Continue Round" action as long as Round N+1 has not yet started.
  - Reopening restores Round N to in-progress status, re-displays its blank pending game row, seamlessly preserves seating and dealer rotation, and removes the unstarted Round N+1 preview card.

### 2.5 Round Input Screen (`RoundInputScreen.kt`)
Optimized for rapid, low-friction score input between hands:
- **Winner Selection**: Tap to select winner with golden border glow, crown badge (`👑`), and haptic feedback.
- **Status Toggles**: Segmented or chip toggles for SEEN / UNSEEN / DUBLEE per player.
- **Quick Maal Presets**:
  - Single-tap preset chips (`+3`, `+5`, `+8`, `+10`, `Clear`) for rapid score entry.
  - Automatically marks player as SEEN.
- **Maal Calculator Dialog**:
  - Stepper modal supporting traditional combinations (Tiplu, Poplu, Jhiplu, Alter, Tunnela, Marriage).
