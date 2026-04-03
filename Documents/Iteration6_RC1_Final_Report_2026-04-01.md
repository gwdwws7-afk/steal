# Iteration 6 RC1 Final Report

Date: 2026-04-01
Executor: Codex
Unity: 6000.2.14f1
Project: `C:\test\Steal`

## 1. Scope
Iteration 6 targets:
1. `I6-T1` Tool system data-driven closure.
2. `I6-T2` runtime text/mojibake cleanup.
3. `I6-T3` AI squad/search validation.
4. `I6-T4` level playability config reinforcement.
5. `I6-T5` balance pass + validators.
6. `I6-T6` RC1 gate.

## 2. Completion Status

| Task | Status | Evidence |
|---|---|---|
| `I6-T1` | DONE | Tool runtime mapping + ToolData assets updated |
| `I6-T2` | DONE | `SupplyPoint`, `HangingPoint`, tools text cleaned |
| `I6-T3` | DONE | `EnemySquadCoordinatorTests`, `Iteration6AISquadSearchPlayModeTests` |
| `I6-T4` | DONE | `LevelData` flow fields + `IntelSpawn` multi-exit topology |
| `I6-T5` | DONE | `ToolDataConfigurationTests`, `LevelDataFlowProfileTests`, `SpawnCoverageTests` |
| `I6-T6` | DONE | `codex-i6-rc1-editmode-results.xml`, `codex-i6-rc1-playmode-results.xml` |

## 3. Regression Results

| Suite | Total | Passed | Failed | Result File |
|---|---:|---:|---:|---|
| EditMode | 283 | 283 | 0 | `codex-i6-rc1-editmode-results.xml` |
| PlayMode | 11 | 11 | 0 | `codex-i6-rc1-playmode-results.xml` |

## 4. RC1 Smoke Coverage
Passed PlayMode smoke chains:
1. `CoreScenes_LoadAndSpawnLoopActors`
2. `CoreScenes_IntelAndExitFlow_WorksForSmoke`
3. `CoreScenes_MovementAndPerceptionChains_WorkForSmoke`
4. `CoreScenes_PauseHudAndToolLoop_WorksForSmoke`
5. `CoreScenes_NarrativeTriggerCoverage_IsAudited`
6. `CoreScenes_RuntimeIntegrityAndMissionCoverage_Pass`
7. `SquadAlertAndSearchLoop_RemainsStableWithoutOscillation`

Core scenes covered:
1. `Level01_Qhapaq_Passage`
2. `Level02_Temple_Complex`
3. `Level03_Underground_Labs`
4. `Level04_Qhipu_Core`
5. `Level05_General_Taki_Villa`

## 5. Deliverable Docs
1. `Documents/Iteration6_Plan.md`
2. `Documents/Iteration6_Tuning_Matrix_2026-04-01.md`
3. `Documents/Iteration6_Level_Playability_Audit_2026-04-01.md`
4. `Documents/Iteration6_Manual_Smoke_Run_Log_2026-04-01.md`
5. `Documents/Iteration6_Known_Issues_2026-04-01.md`

## 6. Sign-off
1. RC1 gate: PASS.
2. No active P0/P1 blockers detected in current automated evidence.
