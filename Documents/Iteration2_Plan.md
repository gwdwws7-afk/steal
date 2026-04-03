# Iteration 2 Plan (Kickoff)

Updated: 2026-03-31
Owner: Gameplay/Systems
Baseline: Iteration 1 runtime loop available, automated tests green.

## 1. Iteration Goal
Raise mission-level playability from graybox loop to verifiable gameplay slice:
1. Objective feedback chain becomes visible and reactive in HUD.
2. AI alert/perception behavior gets tighter tuning and mission pressure.
3. Level validation expands from compile/test to scene-level smoke evidence.

## 2. Scope
### In Scope
1. Objective/HUD runtime integration.
2. Mission intel + exit gating usability pass.
3. Input stability and no-exception guarantee.
4. Regression test expansion for new runtime behavior.

### Out of Scope
1. Final art/audio replacement.
2. Full narrative content authoring.
3. Production-grade enemy behavior trees beyond current state machine.

## 3. Task Board
1. `I2-T1` Objective HUD event sync
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/UI/HUDManager.cs`
     - `Assets/INTIFALL/Scripts/Runtime/UI/EagleEyeUI.cs`
     - `Assets/INTIFALL/Tests/EagleEyeUITests.cs`
2. `I2-T2` Scene-by-scene smoke execution (`SMK-001` to `SMK-009`)
   - Status: `DONE`
   - Output:
     - `Documents/Iteration1_Manual_Smoke_Run_Log_2026-03-31.md` update
     - `Assets/INTIFALL/Tests/PlayMode/Iteration2SceneSmokePlayModeTests.cs`
     - `Assets/INTIFALL/Tests/PlayMode/Iteration2SceneMissionFlowPlayModeTests.cs`
     - `Assets/INTIFALL/Tests/PlayMode/Iteration2SceneUIPauseAndToolsPlayModeTests.cs`
     - `Assets/INTIFALL/Tests/PlayMode/Iteration2SceneMovementPerceptionPlayModeTests.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Level/LevelLoader.cs` (runtime UI bootstrap for missing pause/HUD hooks)
3. `I2-T3` Enemy pressure tuning pass (alert/suspicious/search timing and transition feel)
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/AI/EnemyStateMachine.cs`
     - `Assets/INTIFALL/Scripts/Runtime/AI/EnemyController.cs`
     - `Assets/INTIFALL/Tests/EnemyStateMachineTests.cs`
4. `I2-T4` Mission result/rank bridge to runtime objective outcome
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/System/GameManager.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Level/MissionExitPoint.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Growth/LevelUpReward.cs`
     - `Assets/INTIFALL/Tests/GameManagerTests.cs`
     - `Assets/INTIFALL/Tests/LevelUpRewardTests.cs`

## 4. Exit Criteria
1. Objective text and intel count always match runtime mission state.
2. No input-system mismatch exception in PlayMode logs.
3. EditMode + PlayMode regression pass.
4. Manual smoke report has no `BLOCKED` entries for 5 core scenes.
