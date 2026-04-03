# Iteration 7 Plan (Localization Closure + Route Scoring + RC2)

Updated: 2026-04-01
Owner: Systems/UI/Narrative/Level/QA
Baseline: Iteration 6 completed (`I6-T1` ~ `I6-T6`, RC1 PASS).

## 1. Iteration Goal
1. Close runtime localization consistency issues in core gameplay UI and prompts.
2. Differentiate main vs optional extraction routes with data-driven risk/reward scoring.
3. Align level flow timing with encounter allocation and lock RC2 gate.

## 2. Task Board
1. `I7-T1` Localization Consistency Pass
   - Status: `DONE`
   - Deliverables:
     - `Assets/INTIFALL/Scripts/Runtime/UI/MissionBriefingUI.cs`
     - `Assets/INTIFALL/Scripts/Runtime/UI/HUDManager.cs`
     - `Assets/INTIFALL/Scripts/Runtime/UI/GameOverUI.cs`
     - `Assets/INTIFALL/Scripts/Runtime/UI/EagleEyeUI.cs`
     - `Assets/INTIFALL/Scripts/Runtime/UI/HPHUD.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Environment/ElectronicDoor.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Environment/VentEntrance.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Growth/BloodlineSystem.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Growth/ProgressionTree.cs`
2. `I7-T2` Optional Extraction Route Scoring
   - Status: `DONE`
   - Deliverables:
     - `Assets/INTIFALL/ScriptableObjects/IntelSpawnData.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Level/MissionExitPoint.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Level/LevelLoader.cs`
     - `Assets/Resources/INTIFALL/Spawns/IntelSpawn_Level*.asset`
3. `I7-T3` Level Flow + Encounter Profile Binding
   - Status: `DONE`
   - Deliverables:
     - `Assets/INTIFALL/ScriptableObjects/LevelData.cs`
     - `Assets/Resources/INTIFALL/Levels/LevelData_Level*.asset`
     - `Assets/INTIFALL/Tests/LevelDataFlowProfileTests.cs`
     - `Assets/INTIFALL/Tests/LevelEncounterCoverageTests.cs`
4. `I7-T4` Debrief + Economy Loop Enhancement
   - Status: `DONE`
   - Deliverables:
     - `Assets/INTIFALL/Scripts/Runtime/System/GameManager.cs`
     - `Assets/INTIFALL/Scripts/Runtime/UI/MissionDebriefUI.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Economy/CreditSystem.cs`
     - `Assets/INTIFALL/Tests/MissionRouteScoringTests.cs`
     - `Assets/INTIFALL/Tests/MissionDebriefUITests.cs`
5. `I7-T5` Narrative Continuity Reinforcement
   - Status: `DONE`
   - Deliverables:
     - `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaComm.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaMessageCatalog.cs`
     - `Assets/Resources/INTIFALL/Narrative/WillaMessages.json`
     - `Assets/INTIFALL/Tests/WillaCommTests.cs`
     - `Assets/INTIFALL/Tests/PlayMode/Iteration7NarrativeContinuityPlayModeTests.cs`
6. `I7-T6` RC2 Gate
   - Status: `DONE`
   - Deliverables:
     - `codex-i7-editmode-results.xml`
     - `codex-i7-playmode-results.xml`
     - `Documents/Iteration7_RC2_Final_Report_2026-04-01.md`

## 3. Exit Criteria
1. EditMode + PlayMode regression all green.
2. Runtime UI/prompt strings no longer mix languages within gameplay loop.
3. Each level has main + optional extraction routes with explicit scoring parameters.
4. Level phase timing and encounter budgets are declared and validated.
5. Narrative chain supports MissionStart/IntelFound/MissionComplete with resolved runtime tokens.
