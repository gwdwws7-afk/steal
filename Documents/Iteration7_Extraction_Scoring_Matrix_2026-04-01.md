# Iteration 7 Extraction Scoring Matrix

Date: 2026-04-01
Scope: Optional extraction route differentiation (`I7-T2`, `I7-T4`)

## 1. Runtime Scoring Model
Implemented in `GameManager.CalculateMissionResult(...)` with route parameters:
1. Base mission credits from `LevelUpReward.EvaluateCredits(...)`.
2. Route multiplier: `credits *= clamp(routeCreditMultiplier, 0.5, 2.0)`.
3. Optional route bonus: `+40` if not main route.
4. Route risk bonus: `+20 * routeRiskTier`.
5. Rank uplift: optional route with risk >= 2 and clean stealth grants `+1` rank score (capped at `S`).

## 2. Per-Level Route Configuration

| Level | Route | Type | Risk | Multiplier | Secondary Bonus | Intel Requirement |
|---|---|---|---:|---:|---:|---:|
| L01 | `dock_main` / Dock Extraction | Main | 0 | 1.00 | 0 | All intel |
| L01 | `service_walkway` / Service Walkway | Optional | 2 | 1.15 | +1 | 2 intel |
| L02 | `roof_lift` / Roof Lift | Main | 0 | 1.00 | 0 | All intel |
| L02 | `service_stairwell` / Service Stairwell | Optional | 2 | 1.20 | +1 | 2 intel |
| L03 | `underground_gate` / Underground Gate | Main | 0 | 1.00 | 0 | All intel |
| L03 | `coolant_tunnel` / Coolant Tunnel | Optional | 3 | 1.25 | +1 | 2 intel |
| L04 | `core_gate` / Core Gate | Main | 0 | 1.00 | 0 | All intel |
| L04 | `upper_ring` / Upper Ring Catwalk | Optional | 3 | 1.30 | +1 | 2 intel |
| L05 | `solar_bridge` / Solar Bridge | Main | 0 | 1.00 | 0 | All intel |
| L05 | `western_balcony` / Western Balcony | Optional | 3 | 1.35 | +1 | 2 intel |

## 3. Evidence
1. Data model:
   - `Assets/INTIFALL/ScriptableObjects/IntelSpawnData.cs`
   - `Assets/INTIFALL/Scripts/Runtime/Level/MissionExitPoint.cs`
   - `Assets/INTIFALL/Scripts/Runtime/Level/LevelLoader.cs`
2. Validation tests:
   - `Assets/INTIFALL/Tests/MissionRouteScoringTests.cs`
   - `Assets/INTIFALL/Tests/SpawnCoverageTests.cs`

## 4. Outcome
Main and optional extraction routes now produce deterministic, explainable score and credit differences through data-only tuning.
