# Iteration 1 Manual Smoke Run Log

Date: 2026-03-31 19:29:40
Executor: Codex (auto-prefill)
Unity: 6000.2.14f1
Branch/Commit: `master@60fb064`

## 1. Run Scope
Scenes:
1. `Assets/Scenes/Level01_Qhapaq_Passage.unity`
2. `Assets/Scenes/Level02_Temple_Complex.unity`
3. `Assets/Scenes/Level03_Underground_Labs.unity`
4. `Assets/Scenes/Level04_Qhipu_Core.unity`
5. `Assets/Scenes/Level05_General_Taki_Villa.unity`

Smoke cases:
1. `SMK-001 Scene Boot`
2. `SMK-002 Core Movement`
3. `SMK-003 Enemy Perception`
4. `SMK-004 Intel Collection`
5. `SMK-005 Exit Gating`
6. `SMK-006 Supply Point`
7. `SMK-007 Pause And HUD`
8. `SMK-008 Tool Hotkeys`
9. `SMK-009 Input Compatibility Guard`

## 2. Result Matrix
Use `PASS`, `FAIL`, or `BLOCKED`.

| Scene | SMK-001 | SMK-002 | SMK-003 | SMK-004 | SMK-005 | SMK-006 | SMK-007 | SMK-008 | SMK-009 | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| Level01_Qhapaq_Passage | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | Scene smoke + mission flow + pause/HUD/tool loop + movement/perception state-chain automation passed. |
| Level02_Temple_Complex | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | Scene smoke + mission flow + pause/HUD/tool loop + movement/perception state-chain automation passed. |
| Level03_Underground_Labs | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | Scene smoke + mission flow + pause/HUD/tool loop + movement/perception state-chain automation passed. |
| Level04_Qhipu_Core | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | Scene smoke + mission flow + pause/HUD/tool loop + movement/perception state-chain automation passed. |
| Level05_General_Taki_Villa | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | Scene smoke + mission flow + pause/HUD/tool loop + movement/perception state-chain automation passed. |

## 3. Defects Found
No confirmed manual defects in this auto-prefill run.

## 4. Summary
1. Overall status: `PASS`
2. Blocking defects count: `0`
3. Non-blocking defects count: `0`
4. Recommendation: Optional manual feel pass for `SMK-002`/`SMK-003` (movement smoothness and perception readability) before content lock.

## 5. Automated Evidence Used
1. EditMode regression: `246/246 PASS` (`codex-next-editmode-results.xml`)
2. PlayMode regression: `4/4 PASS` (`codex-next-playmode-results.xml`)
3. No input mismatch exception in PlayMode log:
   - checked `codex-next-playmode.log` for `InvalidOperationException: You are trying to read Input...` and found none.
4. Scene boot smoke for 5 core levels:
   - `codex-iteration2-scene-smoke-playmode-results.xml` (`5/5 PASS`)
5. Mission flow smoke for 5 core levels (intel collect + exit gating + supply presence):
   - `codex-i2t2-flow-playmode-results.xml` (`6/6 PASS`)
6. Pause/HUD/tool loop smoke for 5 core levels:
   - `codex-i2t2-ui-tools-playmode-results.xml` (`7/7 PASS`)
7. Full regression after I2-T2 UI/tool expansion:
   - EditMode: `255/255 PASS` (`codex-i2t2-full-smoke-editmode-results.xml`)
   - PlayMode: `8/8 PASS` (`codex-i2t2-full-smoke-playmode-results.xml`)
8. Movement/perception state-chain smoke for 5 core levels:
   - `codex-i2t2-full-smoke-playmode-results.xml` includes `Iteration2SceneMovementPerceptionPlayModeTests` pass.
