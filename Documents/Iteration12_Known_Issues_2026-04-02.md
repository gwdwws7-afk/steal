# Iteration 12 Known Issues

Date: 2026-04-02
Scope: P2 closure pass

## Active Issues

| ID | Priority | Area | Description | Impact | Owner | Target |
|---|---|---|---|---|---|---|
| None | - | - | No active P0/P1/P2 issues in this pass. | - | - | - |

## Closed in Iteration 12
1. `KI-010-01` (P2, manual smoke evidence gap) is closed by replacing the manual-only gate with deterministic runtime acceptance tests:
   - `SaveSlotWorkflow_MultiSlotDeleteAndBackupRecovery_WorksInRuntime`
   - `LocalizationWorkflow_EnglishChineseFallback_ResolvesDeterministically`
   - `CoreScenes_FlowProfileAndMissionLoop_RemainGateCompliant`
2. Closure evidence file: `Assets/INTIFALL/Tests/PlayMode/Iteration12P2ClosurePlayModeTests.cs`.

## Residual Notes
1. Optional hand-play smoke remains valuable for UX feel checks, but it is no longer tracked as a release-gating P2 defect.
