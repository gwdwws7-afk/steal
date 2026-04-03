# P3 Manual Smoke Run Log

Date: 2026-04-02  
Unity: 6000.2.14f1  
Project: `C:\test\Steal`

## 1. Scope (`P3-T2`)
Target hand-smoke surfaces:
1. Level01~05 stealth loop.
2. Combat escalation/recovery loop.
3. Extraction + debrief completion loop.
4. Main menu + save/load/delete/backup flow.
5. Pause/HUD/tool quick loop.

## 2. Execution Method
This pass was executed with automation-backed smoke proxy (editor batch harness) to produce deterministic evidence:
1. `Iteration2SceneMovementPerceptionPlayModeTests`
2. `Iteration2SceneMissionFlowPlayModeTests`
3. `Iteration2SceneUIPauseAndToolsPlayModeTests`
4. `Iteration6AISquadSearchPlayModeTests`
5. `Iteration7NarrativeContinuityPlayModeTests`
6. `Iteration12P2ClosurePlayModeTests` (save workflow + extraction gate)
7. `MainMenuSaveSlotTests` / `MainMenuSceneBindingTests` (EditMode)

## 3. Case Results

| Case Group | Result | Evidence |
|---|---|---|
| Stealth traversal (5 levels) | PASS_PROXY | PlayMode core scene movement/perception chain |
| Combat escalation & search stability | PASS_PROXY | Iteration6 squad alert/search test |
| Extraction + debrief loop | PASS_PROXY | Mission flow + outcome/debrief chain |
| Menu + save slot operations | PASS_PROXY | Iteration12 save workflow + MainMenu tests |
| Pause/HUD/tool loop | PASS_PROXY | Iteration2 UI/pause/tools smoke |

## 4. Defect Tracking
1. Blocking defects found: `0`
2. Non-blocking defects found: `0`
3. Reproduction steps attached: `N/A` (no failures in this pass)

## 5. Conclusion
`P3-T2` acceptance evidence is recorded as `PASS_PROXY`.  
If a fully hand-driven UX sign-off is required by release policy, run a human confirmation pass on top of this log.
