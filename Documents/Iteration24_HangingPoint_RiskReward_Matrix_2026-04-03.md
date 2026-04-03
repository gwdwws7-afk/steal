# Iteration24 HangingPoint Risk-Reward Matrix

Date: 2026-04-03  
Executor: Codex  
Unity: 6000.2.14f1  
Project: `C:\test\Steal`

## 1. Goal

Establish and enforce an automated tactical gate for hanging-point placement quality, linking hanging-point usefulness to enemy pressure and objective relevance.

## 2. Micro-Tune Applied (Final)

1. `Level02_Temple_Complex` first pass:
   1. `Hang_04` moved `(-24, 18, 4)` -> `(-21.359, 17.472, 5.321)` (~3m toward nearest objective).
2. `Level05_General_Taki_Villa` first pass:
   1. `Hang_01` moved `(-18, 18, 18)` -> `(-15.644, 15.786, 15.644)` (~4m toward nearest objective).
3. `Level02_Temple_Complex` second pass (risk uplift):
   1. `Hang_03` moved `(-24, 13, -4)` -> `(-21.94, 11.626, -3.657)` (~2.5m toward nearest enemy corridor).
   2. `Hang_04` moved `(-21.359, 17.472, 5.321)` -> `(-19.21, 16.27, 5.752)` (~2.5m toward nearest enemy corridor).

## 3. Gate Thresholds

1. Risk tier by nearest enemy spawn distance:
   1. High: `<= 8m` (tier 3)
   2. Medium: `<= 14m` (tier 2)
   3. Low: `<= 22m` (tier 1)
   4. None: `> 22m` (tier 0)

2. Reward tier by nearest objective distance (`intel + supply + exit`):
   1. High: `<= 7m` (tier 3)
   2. Medium: `<= 13m` (tier 2)
   3. Low: `<= 20m` (tier 1)
   4. None: `> 20m` (tier 0)

3. Per-level pass conditions:
   1. At least one hanging point with `riskTier >= 2`.
   2. At least one hanging point with `rewardTier >= 2`.
   3. No dead point where `enemyDist > 35m` and `objectiveDist > 30m` simultaneously.

## 4. Automated Guard

1. PlayMode test file:
   1. `Assets/INTIFALL/Tests/PlayMode/Iteration24HangingPointRiskRewardPlayModeTests.cs`
2. Guard case:
   1. `CoreScenes_HangingPointRiskRewardThresholds_Pass`
3. Per-point metrics logged with `[I24]` tag for traceability.

## 5. Matrix Snapshot (Post-Tune Final)

| Scene | Hanging Points | `riskTier>=2` | `rewardTier>=2` | Min Enemy Dist | Max Enemy Dist | Min Objective Dist | Max Objective Dist |
|---|---:|---:|---:|---:|---:|---:|---:|
| Level01_Qhapaq_Passage | 3 | 3 | 3 | 4.47 | 6.00 | 4.07 | 7.97 |
| Level02_Temple_Complex | 4 | 4 | 4 | 6.40 | 13.04 | 6.34 | 9.26 |
| Level03_Underground_Labs | 3 | 3 | 3 | 6.93 | 9.38 | 7.67 | 10.45 |
| Level04_Qhipu_Core | 4 | 4 | 4 | 6.32 | 13.75 | 4.69 | 10.02 |
| Level05_General_Taki_Villa | 4 | 4 | 4 | 4.90 | 11.50 | 5.62 | 12.98 |

## 6. Verification

1. EditMode full regression:
   1. Result: `364/364 PASS`
   2. Evidence: `codex-i24-next2-full-editmode-results.xml`

2. PlayMode full regression:
   1. Result: `26/26 PASS`
   2. Evidence: `codex-i24-next2-full-playmode-results.xml`

## 7. Outcome

1. Five levels now all satisfy medium-or-higher tactical value across hanging points (`risk>=2`, `reward>=2`).
2. Automated tactical gate remains stable and green.
3. No regression introduced.
