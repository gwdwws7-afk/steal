# Iteration 8 Economy Pressure Balance Audit

Date: 2026-04-01
Scope: Extraction reward rebalance (`I8-T2`)

## 1. Objective
Keep optional extraction routes rewarding, but remove the "always best" outcome when player behavior triggers high alert pressure.

## 2. Updated Runtime Surfaces
1. `Assets/INTIFALL/Scripts/Runtime/System/GameManager.cs`
2. `Assets/INTIFALL/Scripts/Runtime/Economy/CreditSystem.cs`
3. `Assets/INTIFALL/Scripts/Runtime/Growth/LevelUpReward.cs`

## 3. Baseline Reward Curve
`LevelUpReward.EvaluateCredits(...)` base credits:

| Rank | Base Credits |
|---|---:|
| S | 820 |
| A | 560 |
| B | 340 |
| C | 185 |
| D | 95 |

## 4. Pressure-Aware Adjustments
Applied in both mission-result path (`GameManager`) and reward helper path (`CreditSystem`):
1. Alert pressure multiplier:
   - `pressureFactor = clamp(1 - alerts * 0.06, 0.7, 1.0)`
2. Optional route discipline bonus:
   - `max(20, 70 - alerts * 15)`
3. Pressure penalty:
   - Alert overflow: `max(0, alerts - 1) * 25`
   - Tool overuse: `max(0, toolsUsed - 4) * 8`
   - Optional route + full alert extra penalty in mission result path.

## 5. Verification Matrix

| Scenario | Expected |
|---|---|
| Clean main route | Stable baseline reward |
| Clean optional route (higher risk/multiplier) | Higher than clean main route |
| High-pressure optional route | No longer guaranteed to beat clean main route |

Evidence:
1. `Assets/INTIFALL/Tests/MissionRouteScoringTests.cs`
   - `OptionalExit_WithHigherRiskAndMultiplier_YieldsHigherCredits`
   - `OptionalExit_UnderHighPressure_DoesNotDominateCleanMainRoute`
2. `Assets/INTIFALL/Tests/CreditSystemTests.cs`
   - `AddMissionReward_HighPressure_ReducesOptionalRouteReward`

## 6. Outcome
1. Optional extraction remains a high-skill/high-risk option.
2. Excessive alerts and tool spam now have explicit economic cost.
3. Reward differentiation better matches stealth-discipline intent.
