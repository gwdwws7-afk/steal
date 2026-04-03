# Iteration 9 Performance Gate Report

Date: 2026-04-01
Scope: `I9-T5`

## 1. Objective
Add a repeatable PlayMode stress gate that can detect runtime accumulation and major performance drift across repeated core-scene loops.

## 2. Added Test
1. `Assets/INTIFALL/Tests/PlayMode/Iteration9PerformanceGatePlayModeTests.cs`

Case:
1. `CoreScenes_StressLoop_StaysWithinRuntimeBudget`

Behavior:
1. Runs three passes over Level01~05.
2. Collects frame-time samples in each scene.
3. Asserts:
   - no manager accumulation
   - bounded event subscriber counts
   - bounded enemy squad registry growth
   - frame-time and GC thresholds remain within configured budget

## 3. Gate Results
Automated suites:
1. `codex-i9-editmode-results.xml` -> `312/312 PASS`
2. `codex-i9-playmode-results.xml` -> `14/14 PASS`

## 4. Outcome
1. Iteration 9 now has an explicit stress-oriented PlayMode gate in addition to functional smoke.
2. Stability/performance regressions in scene-loop behavior are less likely to slip through CI.
