# Project Brief & Prompt: Marriage Game Calculator Landing Page Website

Use the specifications below to generate a modern, premium, and visually stunning landing page for the **Marriage Game Calculator** mobile application.

---

## 1. Core Brand & Visual Aesthetic
*   **Design Theme**: **Dashain & Tihar Festival** (Vibrant, Culturally Rich, Festive South Asian atmosphere).
*   **Colors**:
    *   **Primary Background**: Deep Night Blue or Rich Maroon.
    *   **Primary Accents**: Gold (Wealth/Prosperity) & Marigold Orange.
    *   **Highlights**: Fresh Green (Jamara) & Deep Red (Tika).
*   **Styling**: Modern premium look using glassmorphism (frosted glass panels with glowing borders), 3D/glossy buttons, smooth transitions, and subtle floating animations.
*   **Cultural Motifs**: Mandalas (for circular seating representations), Diyos (lamps), kites, and Dhaka fabric borders.
*   **App Logo Asset Path**: 
    `f:\workspace\games\MC\MarriageCalculator\MarriageCalculator\Android\app\src\main\res\drawable\marriage_logo_title.png`
*   **Festive Background Image Asset Path**: 
    `f:\workspace\games\MC\MarriageCalculator\.agent\temp\website_bg.png`

---

## 2. Game Overview & Rules (For Content Sections)
Include a clean "Rules Cheatsheet" or "How it Works" section explaining the game of **Marriage** (popular in Nepal, India, and Bhutan):

*   **Setup**: 2 to 6 players, using 3 standard decks of 52 cards + up to 3 special Manuk (Joker) cards. Each player is dealt 21 cards.
*   **The Seating Rotation**: Circular seating arrangement (Mandala) determined dynamically by drawing cards.
*   **Seen (Deke)**: A player who has shown 3 Pure Sequences (Tunnels/Sequences) to qualify to see the main Joker (Tiplu). They pay a low penalty upon losing.
*   **Unseen (Nadeke)**: A player who hasn't qualified to see the Joker yet. They pay a higher penalty.
*   **Dublee (Pairs)**: A special alternative play mode where the player collects matching card pairs (7 pairs to see, 8 to win).

---

## 3. Key App Features to Highlight
1.  **Advanced Scoring Engine (Central Collection)**:
    *   Eliminates manual paper math. The winner initially collects penalties from all losers (fixed game points), and then redistributes variable Maal (points) payouts to everyone based on card combinations.
    *   Supports positive and negative net cash totals for the winner.
2.  **Flexible Cash Tracking**:
    *   Supports real cash currencies with priority on NPR, INR, GBP, USD, and AUD.
    *   Configurable settings: Point Rate (e.g., $1 per point), Seen Penalty, Unseen Penalty, and Dublee Bonus.
3.  **Social Connectivity & FCM Nudges**:
    *   Start games with local "Dummy Players" and easily map/link them to real friends' accounts later.
    *   One-tap "Nudge" button to send Firebase Cloud Messages (push notifications) to offline players to invite them to join or resume a session.
4.  **Three Game Variations (Toggleable)**:
    *   **Normal Mode**: Unseen players still count the Maal in their hand to reduce what they owe.
    *   **Kidnap Mode**: High stakes. The winner steals the Maal of all Unseen players.
    *   **Murder Mode**: Unseen players have their Maal voided (set to 0), so it does not reduce their penalties, nor does the winner steal them.

---

## 4. Website Page Sections & Layout Guide

### 1. Hero Section
*   **Visual**: A frosted-glass container with the main logo (`marriage_logo_title.png`) positioned prominently.
*   **Headline**: *Ditch the Pen and Paper. Master the Marriage Card Score.*
*   **Subheadline**: *A premium, fast, and culturally inspired digital scorer for the popular South Asian Marriage card game.*
*   **Call to Action**: Two glossy metallic buttons: "Download Android App" (with Google Play / APK icons) and "Try the Live Calculator Demo".

### 2. Interactive Scoring Simulator (The "Hook")
Implement a simplified client-side calculator matching the app's **Central Collection** logic so visitors can interact with it.
*   **Inputs**:
    *   Game Mode Dropdown: `Normal`, `Kidnap`, `Murder`.
    *   Point Rate Slider/Input: e.g., Rs. 5 per point.
    *   Player inputs (for 3 or 4 sample players):
        *   Mark status: `Winner`, `Seen`, `Unseen`.
        *   Input Maal Points (number fields).
*   **Output Box**: Real-time display showing the exact net currency won/lost by each player after clicking "Calculate".

### 3. Detailed Features Grid
Showcase card components with hover effects and glassmorphism styling highlighting:
*   *Real Cash Settlements* (Freeze and settle at any round).
*   *Circular Seating representation* (Mandala dealer rotation helper).
*   *Instant Error Correction* (Edit past rounds easily).
*   *Offline Guest Mode* (Play offline locally, sync later).

### 4. Game Variations Comparison
A beautiful comparisons table or tabbed panel explaining **Normal**, **Kidnap**, and **Murder** modes with high-contrast badge icons.

### 5. Call to Action (Footer Banner)
*   A premium banner featuring a cultural motif (like a golden Diyo lamp or Himalayan mountains silhouette).
*   Download link buttons.
*   Footer text matching the mobile app: `"MADE WITH ❤ FROM NEPAL"` in elegant gold/red fonts.
