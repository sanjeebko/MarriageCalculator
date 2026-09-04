# Testing & Verification Specification

## 1. Verification Strategy & Quality Gates

Every code change must satisfy three tiers of automated verification before merging to `main`:

```
               /\
              /  \     Tier 3: End-to-End System Tests (Emulator UI & Live API)
             /----\
            /      \   Tier 2: Integration Tests (API Controllers, MongoDB Repos)
           /--------\
          /          \ Tier 1: Domain & Unit Tests (ScoringEngine, Math Helpers)
         +------------+
```

---

## 2. Invariant Verification Checklist

All scoring tests must assert the following mathematical properties:

1. **Zero-Sum Score Property**:
   $$\sum_{p \in \text{Players}} \text{Score}_p = 0$$
2. **Zero-Sum Cash Property**:
   $$\sum_{p \in \text{Players}} \text{MoneyWon}_p = 0.00$$
3. **Debt Minimization Consistency**:
   In `MatchShareHelper.computeSettlements`:
   $$\sum \text{Debts Settled} = \sum_{p, \text{Balance}_p < 0} -\text{Balance}_p = \sum_{p, \text{Balance}_p > 0} \text{Balance}_p$$
4. **Dublee Scoring Invariants**:
   - Dublee Winner Maal: Winner's total maal includes `+5` points above hand maal.
   - Seen Dublee Loser: Pays exactly `0` fixed penalty points (only Maal difference).
   - Unseen Dublee Loser: Pays full `UnseenPoint` penalty.

---

## 3. Automated Test Suites

### 3.1 C# Core & API Suites (`dotnet test`)
- `ScoringEngineTests`:
  - `CalculateScores_NormalMode_BalancedZeroSum`
  - `CalculateScores_KidnapMode_WinnerStealsUnseenMaal`
  - `CalculateScores_MurderMode_UnseenMaalVoided`
  - `CalculateScores_DubleeWinner_ReceivesFiveMaalBonus`
  - `CalculateScores_DubleeSeenLoser_ExemptFromSeenPenalty`
  - `CalculateScores_DubleeUnseenLoser_PaysUnseenPenalty`
- `MarriageGameSetsControllerTests`:
  - User authorization and tenancy isolation.
  - Round addition and state updates.

### 3.2 Android Unit Test Suites (`./gradlew testDebugUnitTest`)
- `MatchShareHelperTest`: Greedy debt resolution algorithm correctness.
- `VisualSeatingRingTest`: Polar coordinate calculation across 2, 3, 4, 5, 6 player counts.
- `MaalCalculatorTest`: Tiered point table evaluation and bounds clamping.
- `SeatingDrawTest`: Card drawing, tie-breaking by suit rank, dealer assignment.
- `RoundInputViewModelTest`: Score preview, quick presets, error state clearing.
- `DashboardStatsTest`: Career statistics aggregation and leader badge detection.

---

## 4. Standard Verification Commands

```powershell
# 1. Run all .NET unit and integration tests
dotnet test MarriageCalculator/MarriageCalculator.Tests/MarriageCalculator.Tests.csproj

# 2. Run all Android unit tests
cd MarriageCalculator/Android
./gradlew testDebugUnitTest

# 3. Assemble Android debug build
./gradlew assembleDebug

# 4. Verify API build
dotnet build MarriageCalculator/MarriageCalculator.API
```
