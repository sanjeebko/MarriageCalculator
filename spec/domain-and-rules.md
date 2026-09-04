# Marriage Card Game — Domain & Scoring Rules Specification

## 1. Game Equipment & Overview

- **Decks**: Exactly 3 standard 52-card French decks (156 cards total) shuffled together.
- **Printed Jokers (Manuk)**: Up to 3 wild jokers (optional house rule, default 3).
- **Players**: 2 to 6 active players.
- **Hand Size**: Exactly 21 cards dealt to each player.

---

## 2. Card Roles & Terminology

### 2.1 The Tiplu (Main Joker)
After the dealer distributes 21 cards to all players, a card is randomly drawn face-up from the center deck. This card is the **Tiplu**.
- Example: If the drawn card is 8 of Spades (8♠), then 8♠ is the **Tiplu**.
- Since 3 decks are in play, there are 3 copies of 8♠ in total. 1 is on the table, meaning a player can hold at most **2 Tiplu** in hand.

### 2.2 Poplu & Jhiplu (Adjacent Jokers)
- **Poplu (Card Above)**: Card of the same suit with rank immediately above the Tiplu.
  - If Tiplu = 8♠, Poplu = 9♠.
  - (If Tiplu = King, Poplu = Ace).
  - Maximum count in player hand: 3.
- **Jhiplu (Card Below)**: Card of the same suit with rank immediately below the Tiplu.
  - If Tiplu = 8♠, Jhiplu = 7♠.
  - (If Tiplu = Ace, Jhiplu = King).
  - Maximum count in player hand: 3.

### 2.3 Alter (Same Rank, Opposite Color)
Cards of the same numeric rank as the Tiplu, but in a suit of the opposite color:
- If Tiplu is Spades or Clubs (Black), Alters are Hearts & Diamonds (Red).
- Maximum count in player hand: 3.

### 2.4 Printed Jokers (Ordinary Jokers / Manuk)
Standard printed jokers included in decks. Functions as wild card for melds.
- Maximum count in player hand: 3.

---

## 3. Melds, Combinations & Game Status

### 3.1 Qualifying to "See" (Deke)
To earn the right to look at the Tiplu (the face-down maal card on the table) and meld wild jokers, a player must lay down **3 Pure Sequences** (pure runs of 3+ consecutive cards in the same suit, without using jokers as substitutes).
- **Seen Player**: A player who has shown 3 pure sequences before the game ends.
- **Unseen Player**: A player who has not shown 3 pure sequences. Pays highest penalties.

### 3.2 Dublee (Pairs Play)
An alternate strategy where the player collects identical matching pairs (e.g. 7♦ & 7♦ from separate decks):
- **Qualifying Dublee**: Must hold 7 identical pairs to see the joker.
- **Winning Dublee**: Must hold 8 identical pairs to declare a win.

---

## 4. Maal Value Specification

Points ("Maal") are awarded based on special cards and sets held by a player at the end of the hand. 
Crucially, **points follow a non-linear tier table based on the total count held**, not a simple per-card multiplier:

| Maal Item | Description | Tiers (1 held / 2 held / 3 held / 4 held) | Max Count Held |
| :--- | :--- | :--- | :--- |
| **Tiplu** | Main Joker | 3 pts (1) / 8 pts (2) | 2 (1 is on table) |
| **Poplu** | Rank above Tiplu, same suit | 2 pts (1) / 5 pts (2) / 10 pts (3) | 3 |
| **Jhiplu** | Rank below Tiplu, same suit | 2 pts (1) / 5 pts (2) / 10 pts (3) | 3 |
| **Alter** | Same rank as Tiplu, alternate color | 5 pts (1) / 15 pts (2) / 30 pts (3) | 3 |
| **Ordinary Joker** | Printed card joker | 5 pts (1) / 15 pts (2) / 30 pts (3) | 3 |
| **Marriage** | Jhiplu + Tiplu + Poplu sequence | 10 pts (1 set) / 25 pts (2 sets) | 2 |
| **Tunnela** | 3 identical cards (same suit & rank) | 5 pts (1) / 15 pts (2) / 30 pts (3) / 45 pts (4) | 4 |
| **Poplu/Jhiplu Tunnela** | 3 identical Poplu or Jhiplu | 10 pts (1) / 30 pts (2) / 45 pts (3) | 3 |
| **Alter Tunnela** | 3 identical Alter cards | Flat 35 pts | 1 |
| **Joker Tunnela** | 3 printed jokers | Flat 35 pts | 1 |

*Note: A Tiplu Tunnela cannot exist because one Tiplu is always exposed on the table as the indicator.*

---

## 5. Scoring Algorithm: Central Collection

The Marriage Calculator implements the canonical **Central Collection** algorithm centered around the Winner.

```
Total Round Flow:
1. Apply Dublee Winner Bonus (+5 Maal to Winner if winner played Dublee).
2. Apply Game Mode transformations (Kidnap / Murder / Normal).
3. Winner collects fixed penalties from all losers.
4. Distribute Maal:
   a. Unseen losers pay full Maal difference to each Seen player and Winner.
   b. Each pair of Seen players (including Winner) exchange Maal difference.
5. Multiply Point totals by PointRate to calculate cash MoneyWon.
```

### 5.1 Fixed Game Penalties (`CollectPenalties`)
- **Unseen Loser**: Pays `UnseenPoint` (Default: 10 pts) to Winner.
- **Seen Loser**: Pays `SeenPoint` (Default: 3 pts) to Winner.
- **Exemption Rule (Dublee Loser)**: If a loser has seen the joker (`Seen = true`) AND played Dublee (`Dublee = true`), they pay **0 pts penalty** (exempt from `SeenPoint`). An unseen Dublee loser still pays `UnseenPoint`.

### 5.2 Game Mode Variations (`ApplyGameMode`)
The mode dictates how an Unseen player's Maal points are treated:

1. **Normal Mode**:
   - Unseen players keep their Maal points.
   - Their Maal acts as a buffer: it directly reduces the amount of points they owe to the Winner and Seen players (`diff = seen.Maal - unseen.Maal`).
2. **Kidnap Mode**:
   - The Winner "kidnaps" (steals) all Maal points from every Unseen player.
   - Each Unseen player's Maal is transferred to the Winner: `winner.Maal += unseen.Maal; unseen.Maal = 0;`.
   - The Unseen players now owe full penalties plus the Winner's augmented Maal.
3. **Murder Mode**:
   - The Unseen players' Maal points are murdered (voided/zeroed).
   - `unseen.Maal = 0;`.
   - The Winner does NOT receive these points. The loser cannot use their Maal to mitigate their loss.

### 5.3 Dublee Winner Rule
- If the Winner won with Dublee (`winner.Dublee = true`), they receive a flat **+5 Maal bonus** (`DubleeWinnerMaalBonus = 5`) added to their raw Maal before mode adjustment and distribution.
- This bonus flows through Maal exchange with all players and is reflected in `TotalMaal`.

### 5.4 Pairwise Maal Exchange (`DistributeMaal`)
1. **Unseen to Seen Exchange**:
   For each unseen player $U$ and each seen player $S$ (including winner):
   $$	ext{diff} = S.	ext{Maal} - U.	ext{Maal}$$
   $$S.	ext{Score} \leftarrow S.	ext{Score} + 	ext{diff}$$
   $$U.	ext{Score} \leftarrow U.	ext{Score} - 	ext{diff}$$

2. **Seen to Seen Exchange**:
   For every unique pair of seen players $(S_i, S_j)$ where $i < j$:
   $$	ext{diff} = S_i.	ext{Maal} - S_j.	ext{Maal}$$
   $$S_i.	ext{Score} \leftarrow S_i.	ext{Score} + 	ext{diff}$$
   $$S_j.	ext{Score} \leftarrow S_j.	ext{Score} - 	ext{diff}$$

### 5.5 Invariant Proof: Zero-Sum Game
The game score is strictly zero-sum across all participating players:
$$\sum_{i=1}^N 	ext{Score}_i = 0$$
$$\sum_{i=1}^N 	ext{MoneyWon}_i = \sum_{i=1}^N (	ext{Score}_i 	imes 	ext{PointRate}) = 0.00$$
Even if a Winner collects fixed penalties, if opponents hold significantly more Maal than the winner, the Winner's final net score can be negative.
