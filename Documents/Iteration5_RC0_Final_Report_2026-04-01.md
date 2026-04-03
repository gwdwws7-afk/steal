# Iteration 5 RC0 Final Report

Date: 2026-04-01  
Executor: Codex  
Unity: 6000.2.14f1  
Project: `C:\test\Steal`

## 1. Scope
Iteration 5 execution targets:
1. `I5-T2` AI behavior depth (patrol-alert-search linkage, squad communication, partition search).
2. `I5-T3` Tool and economy second balancing pass.
3. `I5-T4` Narrative content completion for 5 levels.
4. `I5-T5` RC0 gate (EditMode + PlayMode + 5-level smoke + known issues list).

## 2. Task Completion

| Task | Status | Evidence |
|---|---|---|
| `I5-T2` | DONE | `Assets/INTIFALL/Scripts/Runtime/AI/EnemySquadCoordinator.cs`, `Assets/INTIFALL/Scripts/Runtime/AI/EnemyController.cs` |
| `I5-T3` | DONE | `Assets/INTIFALL/Scripts/Runtime/Tools/*.cs`, `Assets/INTIFALL/Scripts/Runtime/Economy/CreditSystem.cs`, `Assets/INTIFALL/Scripts/Runtime/Growth/LevelUpReward.cs` |
| `I5-T4` | DONE | `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaMessageCatalog.cs`, `Assets/Resources/INTIFALL/Narrative/WillaMessages.json`, `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaComm.cs` |
| `I5-T5` | DONE | `codex-i5-rc0-editmode-results.xml`, `codex-i5-rc0-playmode-results.xml`, known issues list |

## 3. Regression Results

| Suite | Total | Passed | Failed | Result File |
|---|---:|---:|---:|---|
| EditMode | 274 | 274 | 0 | `codex-i5-rc0-editmode-results.xml` |
| PlayMode | 10 | 10 | 0 | `codex-i5-rc0-playmode-results.xml` |

## 4. RC0 Smoke Coverage
PlayMode smoke cases passed:
1. `CoreScenes_LoadAndSpawnLoopActors`
2. `CoreScenes_IntelAndExitFlow_WorksForSmoke`
3. `CoreScenes_MovementAndPerceptionChains_WorkForSmoke`
4. `CoreScenes_PauseHudAndToolLoop_WorksForSmoke`
5. `CoreScenes_NarrativeTriggerCoverage_IsAudited`
6. `CoreScenes_RuntimeIntegrityAndMissionCoverage_Pass`

Core scenes covered:
1. `Level01_Qhapaq_Passage`
2. `Level02_Temple_Complex`
3. `Level03_Underground_Labs`
4. `Level04_Qhipu_Core`
5. `Level05_General_Taki_Villa`

## 5. I5 Technical Highlights
1. AI squad coordinator added with shared alert wave and deterministic partition search target generation.
2. Enemy controller upgraded to:
   - register/unregister squad members,
   - broadcast high/low priority alerts,
   - move through anti-jitter partition search steps after losing target.
3. Tool pass rebalanced cooldown/ammo/range trade-offs (Smoke/Flash/SleepDart/SoundBait/TimedNoise/EMP/WallBreaker/Rope).
4. Economy pass increased S/A/B/C reward separation and tuned bonus composition.
5. Narrative chain now includes explicit `MissionStart`, `IntelFound`, `MissionComplete` entries for all level indices `0..4` in both default + resource catalog paths.

## 6. Known Issues
See `Documents/Iteration5_Known_Issues_2026-04-01.md`.

## 7. Sign-off
1. RC0 gate is PASS.
2. No P0/P1 blockers in current automated regression + smoke coverage.
3. Ready for next iteration content polish / balancing telemetry pass.
