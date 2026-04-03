# Iteration 8 Manual Smoke Run Log

Date: 2026-04-01
Executor: Codex
Unity: 6000.2.14f1
Project: `C:\test\Steal`

## 1. Scope
Iteration 8 smoke confirmation for:
1. Data-layer localization closure.
2. Pressure-aware extraction reward behavior.
3. Save migration compatibility.
4. Runtime stability across repeated scene loops.

## 2. Run Method
Manual interactive smoke was intentionally skipped per user request in this iteration.
Automated PlayMode smoke/regression evidence is used as release gate for this pass.

## 3. Manual Case Status

| Case Type | Result |
|---|---|
| Interactive hand-driven mission replay | SKIPPED_BY_REQUEST |

## 4. Automated Coverage Used Instead

| Case | Result |
|---|---|
| `PlayerController_CanTickAFrame_WithoutInputExceptions` | PASS |
| `LevelLoader_LoadLevel_SpawnsDefaultLoopActors` | PASS |
| `MissionExit_StaysLockedUntilIntelCollected` | PASS |
| `CoreScenes_IntelAndExitFlow_WorksForSmoke` | PASS |
| `CoreScenes_MovementAndPerceptionChains_WorkForSmoke` | PASS |
| `CoreScenes_LoadAndSpawnLoopActors` | PASS |
| `CoreScenes_PauseHudAndToolLoop_WorksForSmoke` | PASS |
| `CoreScenes_NarrativeTriggerCoverage_IsAudited` | PASS |
| `CoreScenes_RuntimeIntegrityAndMissionCoverage_Pass` | PASS |
| `SquadAlertAndSearchLoop_RemainsStableWithoutOscillation` | PASS |
| `CoreScenes_NarrativeChain_ResolvesExtendedOutcomeTokens` | PASS |
| `CoreScenes_TwoPassLoop_DoesNotAccumulateRuntimeManagersOrSubscribers` | PASS |
| `GameLoop_TicksAtLeastOneFrame` | PASS |

## 5. Result
Iteration 8 smoke gate status (automated): PASS.
