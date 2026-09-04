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
  - Photorealistic casino poker table background asset (`casino_poker_table.png`) featuring dark espresso padded leather bumper, brass bead trim, walnut racetrack, and illuminated emerald felt with antialiased stadium masking.
  - Dynamic dealer rotation trajectory arc (`DealerRotationCanvas`) drawn along the betting rail with a traveling glowing gold comet pulse and clockwise directional chevrons pointing from the current dealer to the next dealer.
  - 3D ceramic tournament dealer button ("DEALER / D") with beveled depth and metallic gold border on the active dealer.
  - Platinum next dealer chip ("›") highlighting the upcoming dealer in rotation.
  - Rail-seated player pods (`PlayerSeatNode`) with metallic bezels (gold for dealer, platinum for next dealer, gunmetal for players), anchored brass seat tokens (1..N), and smart position-aware name plaques (top-aligned for top rim, bottom-aligned for bottom rim) to prevent felt occlusion.
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

### 2.5 Round Input Screen (`RoundInputScreen.kt`)
Optimized for rapid, low-friction score input between hands:
- **Winner Selection**: Tap to select winner with golden border glow, crown badge (`👑`), and haptic feedback.
- **Status Toggles**: Segmented or chip toggles for SEEN / UNSEEN / DUBLEE per player.
- **Quick Maal Presets**:
  - Single-tap preset chips (`+3`, `+5`, `+8`, `+10`, `Clear`) for rapid score entry.
  - Automatically marks player as SEEN.
- **Maal Calculator Dialog**:
  - Stepper modal supporting traditional combinations (Tiplu, Poplu, Jhiplu, Alter, Tunnela, Marriage).
