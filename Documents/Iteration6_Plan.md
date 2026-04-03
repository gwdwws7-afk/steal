# Iteration 6 Plan (Data-Driven Stabilization + RC1)

Updated: 2026-04-01
Owner: Systems/AI/Level/Narrative/QA
Baseline: Iteration 5 completed (`I5-T1` ~ `I5-T5`).

## 1. Iteration Goal
1. Convert core tuning surfaces from script-hardcoded values to data-driven assets.
2. Raise AI and level-loop reliability with stronger regression coverage.
3. Close RC1 gate with fully repeatable automated evidence.

## 2. Task Board
1. `I6-T1` Tool System Data-Driven Closure
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/Tools/ToolBase.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Tools/ToolManager.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Tools/*.cs`
     - `Assets/INTIFALL/ScriptableObjects/Tools/*.asset`
2. `I6-T2` Text/Localization Cleanup
   - Status: `DONE`
   - Output:
     - Removed mojibake strings in runtime tool/supply/hanging prompts.
3. `I6-T3` AI Depth Validation Pass
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Tests/EnemySquadCoordinatorTests.cs`
     - `Assets/INTIFALL/Tests/PlayMode/Iteration6AISquadSearchPlayModeTests.cs`
4. `I6-T4` Level Playability Config Pass
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/ScriptableObjects/LevelData.cs`
     - `Assets/Resources/INTIFALL/Levels/LevelData_*.asset`
     - `Assets/Resources/INTIFALL/Spawns/IntelSpawn_*.asset`
     - `Assets/INTIFALL/Scripts/Runtime/Level/LevelLoader.cs`
5. `I6-T5` Balance Pass 3 + Validators
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Tests/ToolDataConfigurationTests.cs`
     - `Assets/INTIFALL/Tests/LevelDataFlowProfileTests.cs`
     - `Assets/INTIFALL/Tests/SpawnCoverageTests.cs`
6. `I6-T6` RC1 Gate
   - Status: `DONE`
   - Output:
     - `codex-i6-rc1-editmode-results.xml`
     - `codex-i6-rc1-playmode-results.xml`
     - `Documents/Iteration6_RC1_Final_Report_2026-04-01.md`

## 3. Exit Criteria
1. EditMode + PlayMode all green.
2. Tool balancing can be modified through ToolData assets.
3. Level route/time design fields are present and validated for Level01~05.
4. AI squad alert/search chain has dedicated regression coverage.
