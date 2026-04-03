# Iteration 9 Plan (Reliability + Localization Infra + RC4 Readiness)

Updated: 2026-04-01
Owner: Core/Systems/Level/QA
Baseline: Iteration 8 completed (`I8-T1` ~ `I8-T5`, RC3 automated PASS).

## 1. Iteration Goal
1. Improve save reliability from single-slot to multi-slot with backup recovery.
2. Introduce a centralized localization infrastructure for key-based lookup.
3. Tighten level pacing metadata with explicit completion windows and pressure curve.
4. Add runtime stress gate for performance/stability regression.
5. Prepare RC4 readiness package (manual smoke still required for final sign-off).

## 2. Task Board
1. `I9-T1` Manual Smoke Acceptance Pack
   - Status: `IN_PROGRESS`
   - Output:
     - `Documents/Iteration9_Manual_Smoke_Checklist.md`
     - `Documents/Iteration9_Manual_Smoke_Run_Log_2026-04-01.md`
2. `I9-T2` Save Reliability Hardening
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/Core/SaveLoadManager.cs`
     - `Assets/INTIFALL/Tests/SaveLoadManagerReliabilityTests.cs`
3. `I9-T3` Localization Infrastructure
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/System/LocalizationService.cs`
     - `Assets/Resources/INTIFALL/Localization/LocalizationTable.json`
     - `Assets/INTIFALL/ScriptableObjects/EnemyTypeData.cs`
     - `Assets/INTIFALL/Tests/LocalizationServiceTests.cs`
4. `I9-T4` Level Pacing/Density Pass
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/ScriptableObjects/LevelData.cs`
     - `Assets/Resources/INTIFALL/Levels/LevelData_Level*.asset`
     - `Assets/INTIFALL/Tests/LevelDataFlowProfileTests.cs`
5. `I9-T5` Performance/Stability Gate
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Tests/PlayMode/Iteration9PerformanceGatePlayModeTests.cs`
     - `codex-i9-editmode-results.xml`
     - `codex-i9-playmode-results.xml`
6. `I9-T6` RC4 Readiness Report
   - Status: `DONE`
   - Output:
     - `Documents/Iteration9_RC4_Readiness_Report_2026-04-01.md`

## 3. Exit Criteria
1. EditMode + PlayMode regression all green.
2. Save load path supports slot isolation and corrupted-primary backup fallback.
3. Localization service provides key-based language resolution with tests.
4. Level pacing metadata exposes completion windows and pressure progression checks.
5. RC4 readiness report is complete; manual smoke status explicitly tracked.
