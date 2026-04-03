# Iteration24 HangingPoint Patrol-Link Windows

Date: 2026-04-03  
Executor: Codex  
Unity: 6000.2.14f1  
Project: `C:\test\Steal`

## 1. Goal

Add an automated patrol-link gate so hanging points remain tactically connected to patrol pressure while preserving counterplay windows.

## 2. Patrol-Link Gate

1. New PlayMode test:
   1. `Assets/INTIFALL/Tests/PlayMode/Iteration24HangingPointPatrolLinkPlayModeTests.cs`
   2. Case: `CoreScenes_HangingPointPatrolLinkWindows_AreValid`

2. Per-hanging-point constraints:
   1. nearest patrol distance `>= 4m` (avoid instant unavoidable death points)
   2. nearest patrol distance `<= 16m` (avoid tactically disconnected points)

3. Per-level constraints:
   1. at least one high-pressure hanging point (`nearest patrol <= 8m`)
   2. at least one counterplay-window hanging point (`6m <= nearest patrol <= 14m`)

## 3. Snapshot (Post-Tune)

| Scene | Nearest Patrol Dist Range |
|---|---|
| Level01_Qhapaq_Passage | 4.47 ~ 6.00 |
| Level02_Temple_Complex | 6.40 ~ 13.04 |
| Level03_Underground_Labs | 6.93 ~ 9.38 |
| Level04_Qhipu_Core | 6.32 ~ 13.75 |
| Level05_General_Taki_Villa | 4.90 ~ 11.50 |

## 4. Verification

1. EditMode full regression:
   1. Result: `364/364 PASS`
   2. Evidence: `codex-i24-patrol-full-editmode-results.xml`

2. PlayMode full regression:
   1. Result: `27/27 PASS`
   2. Evidence: `codex-i24-patrol-full-playmode-results.xml`

## 5. Outcome

1. Hanging points now have automated patrol-link window constraints in addition to risk-reward constraints.
2. Level02/Level05 micro-tunes remain compatible with patrol-link gate.
3. Current automation baseline is fully green.
