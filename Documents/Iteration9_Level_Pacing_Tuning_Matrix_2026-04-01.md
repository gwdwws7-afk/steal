# Iteration 9 Level Pacing Tuning Matrix

Date: 2026-04-01
Scope: `I9-T4`

## 1. Data Model Extension
Updated:
1. `Assets/INTIFALL/ScriptableObjects/LevelData.cs`

Added fields:
1. `completionWindowMinMinutes`
2. `completionWindowMaxMinutes`
3. `patrolPressureTier` (`1~5`)
4. `enemyDensityTargetPerMinute`

## 2. Level Matrix (Level01~05)

| Level | Designed Min | Completion Window | Pressure Tier | Enemy Density Target | Supply Points | Full Alert Duration |
|---|---:|---|---:|---:|---:|---:|
| L01 | 12 | 11~14 | 1 | 0.50 | 5 | 26 |
| L02 | 15 | 13~17 | 2 | 0.47 | 5 | 30 |
| L03 | 18 | 16~20 | 3 | 0.45 | 4 | 33 |
| L04 | 20 | 18~23 | 4 | 0.55 | 4 | 36 |
| L05 | 22 | 20~25 | 5 | 0.50 | 3 | 40 |

## 3. Tuning Intent
1. Explicit completion windows make pacing expectations testable.
2. Pressure tier now scales monotonically with level index.
3. Supply points trend downward as level pressure increases.
4. Full-alert persistence increases toward late-game missions.

## 4. Validation Updates
Updated:
1. `Assets/INTIFALL/Tests/LevelDataFlowProfileTests.cs`

New checks:
1. Designed completion time must stay inside completion window.
2. Enemy density target must remain close to actual density.
3. Pressure curve monotonicity:
   - `patrolPressureTier` non-decreasing
   - `fullAlertDuration` non-decreasing
   - `supplyPointCount` non-increasing

## 5. Outcome
Level pacing parameters are now explicit, auditable, and regression-guarded.
