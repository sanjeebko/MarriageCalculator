# Marriage Card Game Calculator - Requirements

## 1. Project Overview
**App Name**: Marriage Game Calculator
**Platform**: Android (Native)
**Type**: Utility / Scorekeeper
**Description**: A digital scorekeeper for the "Marriage" card game (popular in Nepal, India, and Bhutan). It eliminates the need for manual calculations and pen-and-paper tracking by automatically calculating the exchange of points/money between players after every round.

## 2. The Game: "Marriage" (Rules & Context)

### 2.1 Game Setup & Equipment
*   **Players**: Supports 2 to 6 players.
*   **Decks**: Utilizes 3 standard decks of 52 cards mixed together.
*   **Special Cards**: Option to add up to 3 extra "Jokers" (Manuk) which have special value/points.
*   **Cards Handled**: Each player is dealt 21 cards.

### 2.2 Seating & Dealing (The "Round" Logic)
*   **Seating Arrangement (First Round)**:
    *   Determined by drawing cards.
    *   **Highest Card**: Chooses 1st Seat.
    *   **2nd Highest**: Sits to the Right of 1st.
    *   **Lowest Card**: Sits to the Left of 1st (Last Seat).
    *   *Arrangement is Circular*.
*   **Round Structure**: A "Round" consists of exactly **N Games**, where N is the number of players.
    *   Each player gets exactly one chance to be the Dealer per round.
    *   **First Dealer**: The "Lowest Card" holder (from seating phase) deals the first game.
*   **Dealing Mechanics**:
    *   Dealer distributes cards one-by-one, starting from the player to their **Right**.
    *   Each player receives 21 cards.
    *   **Tiplu/Joker Selection**: After dealing, the dealer shows the top card of the remaining deck to determine the "Main Joker" (Tiplu).
    *   **Turn Order**: Gameplay begins with the player to the **Right** of the dealer.

### 2.3 Winning Conditions
*   **Objective**: Arrange hand into valid sets (Sequences, Trials, or Dublees).
*   **Winning Paths**:
    1.  **Sequence Play (Tunnel)**:
        *   Player must first show **3 Pure Sequences** (e.g., 2-3-4 of Spades) to "See" the joker.
        *   To Win: Complete the rest of the hand into valid sequences/trials.
    2.  **Dublee Play (Pairs)**:
        *   Player collects pairs of identical cards (e.g., 7-Diamonds & 7-Diamonds).
        *   To "See": Must show **7 Pairs**.
        *   To Win: Must collect **8 Pairs** total.

### 2.4 Terminology
*   **Maal (Points)**: Bonus points derived from specific card combinations (Tunnel, Sequence, Tiplu, Poplu, etc.).
*   **Seen (Deke)**: A player who has successfully formed the initial 3 pure sequences and has "seen" the Joker/Tiplu.
*   **Unseen (Nadeke)**: A player who has not yet qualified to see the Joker. They pay the highest penalty.
*   **Dublee**: A special way of winning/playing where the player holds paired cards. (Optional rule).
*   **Winner**: Information about who finished the game first.

### 2.3 Scoring Mechanics
The calculator handles the complex "give-and-take" logic:
1.  **Game Points (Fixed)**: A fixed penalty paid by all losers to the winner.
    *   *Unseen Player*: Pays fixed penalty (e.g., 10 pts) + Maal Value.
    *   *Seen Player*: Pays fixed penalty (e.g., 3 pts) + difference in Maal.
2.  **Maal Points (Variable)**:
    *   Players compare their total "Maal" points with every other "Seen" player.
    *   If Player A has 10 Maal and Player B has 5 Maal, Player B owes Player A 5 points.
    *   Unseen players pay the full Maal value to the winner and other valid claimants.

### 2.4 Game Variations
*   **Normal**: The standard scoring method. "Unseen" players **ARE** allowed to count their Maal points. Even though they didn't qualify to "see", the Maal cards in their hand still reduce the total points they owe to the Winner/Seen players.
*   **Kidnap**: A high-stakes mode. The Winner "kidnaps" (steals) the Maal cards/points of the "Unseen" player(s). The Unseen player's Maal points are added to the Winner's Maal total, significantly increasing the points the loser must pay.
*   **Murder**: The "Unseen" player's Maal points are "murdered" (voided/zeroed). The loser cannot claim any points for the Maal in their hand, but the Winner does *not* get to add them to their own score. The Winner only scores based on their own hand.

## 3. App Feature Requirements

### 3.1 Game Setup Screen
*   **Add/Select Players**: Ability to create player profiles or select from a history list.
*   **Currency Support**:
    *   **Type**: Real Cash (No Chips).
    *   **Priority List**: NPR, INR, GBP, USD, AUD (Top of list).
    *   **Other**: Allow generic currency selection.
*   **Game Settings**:
    *   **Point Rate (Per Point Value)**: Currency value of 1 point (e.g., Rs. 5).
    *   **Seen Penalty**: Points charged for just playing (e.g., 3 pts).
    *   **Unseen Penalty**: Points charged for not qualifying (e.g., 10 pts).
    *   **Dublee Bonus**: Extra points for dublee win (e.g., 5 pts).
    *   **Game Mode**: Toggle controls for **Kidnap** or **Murder** modes (can be set per round or game).
    *   **Constraints**: Core settings (Game Mode, Point Rate guidelines) are **LOCKED** once the session starts. Only minor adjustments (if allowed) can be made mid-game.

### 2.5 Advanced Scoring Algorithm (Central Collection)
*   **Concept**: Calculations follow a "Central Collection" technique centered on the Winner.
*   **Flow**:
    1.  **Collection**: The Winner initially "collects" all applicable penalties/points from all losers (Game Points + Unseen Penalties).
    2.  **Distribution**: The Winner then "pays out" Maal points to *all* players (including themselves and losers) based on their held Maal.
    3.  **Net Result**:
        *   Winner's Final Score = (Total Collected) - (Total Maal Payouts).
        *   *Note*: A Winner can have a **negative** score/money result if the opponents hold significantly more Maal than the winner collected.
*   **Dynamic Player Count**: Scoring logic automatically adjusts based on the number of active players in the current round.

### 2.7 Player Session Management
*   **Join/Leave**: Players can join or leave the session between Games (not during a hand).
*   **Session Settlement**:
    *   Users can "Freeze" and settle the money/scores after *any* game.
    *   A new "Money Collection" session can start immediately after settlement with a fresh slate, while keeping player seating/roster intact.

## 3. App Feature Requirements

### 3.2 Round Input Screen (The Scorer)
*   **Select Winner**: Who won the hand?
*   **Player Status**: Mark each player as "Seen", "Unseen", or "Dublee".
*   **Maal Input**:
    *   Simple numeric input for each player's total Maal (e.g., Player A: 15, Player B: 25).
    *   (Optional Advanced) Calculator for Maal: Select cards (Tiplu, Sequences) to auto-sum Maal.

### 3.3 Scoreboard / History
*   **Current Scores**: Real-time balance of each player (e.g., Player A: +500, Player B: -200).
*   **Round History**: detailed scrollable list of past rounds.
*   **Money Calculator**: Total Won/Lost in currency (Total Points * Point Rate).

### 3.4 Data Persistence
*   **Local**: Game state saved locally so the app can be closed and reopened without losing the game.
*   **Cloud (Future)**: Sync scores to MongoDB for long-term history (via .NET API).

## 4. User Experience Goals
*   **Speed**: Inputting scores must be faster than calculating on paper.
*   **Clarity**: Clearly show who owes whom immediately.
*   **Mistake Correction**: Ability to edit previous rounds if a mistake was made.

## 4. Authentication & Connectivity (New)

### 4.1 Users & Authentication
*   **Provider**: Firebase Authentication (Google Sign-In).
*   **Modes**:
    *   **Guest/Offline Mode**:
        *   User plays locally.
        *   Can create "Dummy Players" (local-only, simple names, no email required).
    *   **Online Mode**:
        *   Requires Sign-In.
        *   Enables Cloud persistence and Multiplayer features.

### 4.2 Friends & Social
*   **Friend System**:
    *   Users can send invitiations to other users.
    *   The other user must install the app and accept the request to become "Friends".
*   **Player Mapping (Dummy-to-Real)**:
    *   A host can start a game with "Dummy Players" (e.g., "Player 2").
    *   **Linking**: The host can later map a "Dummy Player" to a "Real Friend's Account".
    *   **Constraint**: The Real User must be a "Friend" first.
    *   **Auto-Update**: Once linked, the Dummy Player's display name automatically updates to the Real User's profile name.

### 4.3 Game Ownership & Permissions
*   **Single Owner**: A contest (Set of Games) has exactly **ONE** Owner at a time.
*   **Permissions**:
    *   **Owner/Host**: Full access to Add Rounds, Edit Scores, Modify Settings.
    *   **Participants**: Read-only access to Game Calculation and History (Real-time view).
*   **Transfer**: ownership can be transferred from the current Owner to another player in the game.

### 4.4 Notifications (FCM)
*   **Infrastructure**: Firebase Cloud Messaging (FCM).
*   **Feature: Nudge**:
    *   **Action**: The Game Owner can "Nudge" players to join/resume the game.
    *   **Behavior**: Sends an offline push notification to the target player(s).
    *   **Deep Link**: Clicking the notification automatically launches the app and navigates directly to the specific Game Page.

## 5. Branding & Assets
*   **App Icon**:
    *   Style: Glossy, Premium, 3D.
    *   Elements: Playing cards, coins/chips, cultural motifs (Nepal/South Asia).
*   **Splash Screen**:
    *   First screen on launch.
    *   Branding logo with subtle animation.
    *   Transitions smoothly to the Login/Dashboard.