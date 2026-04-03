# Iteration 7 Manual Smoke Run Log

Date: 2026-04-01
Executor: Codex
Unity: 6000.2.14f1
Project: `C:\test\Steal`

## 1. Scope
I7 smoke confirmation for:
1. Runtime localization readability.
2. Optional extraction route scoring chain.
3. Narrative continuity with extended mission-complete tokens.

## 2. Run Method
This iteration used PlayMode smoke harness execution (not a hand-driven interactive pass in Editor UI).

## 3. Smoke Cases
| Case | Result |
|---|---|
| `PlayerController_CanTickAFrame_WithoutInputExceptions` | PASS |
| `LevelLoader_LoadLevel_SpawnsDefaultLoopActors` | PASS |
| `MissionExit_StaysLockedUntilIntelCollected` | PASS |
| `CoreScenes_LoadAndSpawnLoopActors` | PASS |
| `CoreScenes_IntelAndExitFlow_WorksForSmoke` | PASS |
| `CoreScenes_MovementAndPerceptionChains_WorkForSmoke` | PASS |
| `CoreScenes_PauseHudAndToolLoop_WorksForSmoke` | PASS |
| `CoreScenes_NarrativeTriggerCoverage_IsAudited` | PASS |
| `CoreScenes_RuntimeIntegrityAndMissionCoverage_Pass` | PASS |
| `SquadAlertAndSearchLoop_RemainsStableWithoutOscillation` | PASS |
| `CoreScenes_NarrativeChain_ResolvesExtendedOutcomeTokens` | PASS |
| `GameLoop_TicksAtLeastOneFrame` | PASS |

## 4. Result
Smoke gate status: PASS.
