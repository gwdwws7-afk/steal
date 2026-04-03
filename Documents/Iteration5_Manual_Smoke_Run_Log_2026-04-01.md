# Iteration 5 Manual Smoke Run Log

Date: 2026-04-01
Executor: Codex (auto-prefill)
Unity: 6000.2.14f1
Project: `C:\test\Steal`

## 1. Scope
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
9. `SMK-009 Narrative Chain`

## 2. Result Matrix
Use `PASS`, `FAIL`, or `BLOCKED`.

| Scene | SMK-001 | SMK-002 | SMK-003 | SMK-004 | SMK-005 | SMK-006 | SMK-007 | SMK-008 | SMK-009 | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| Level01_Qhapaq_Passage | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | Automated smoke + narrative audit + mission flow all green. |
| Level02_Temple_Complex | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | Automated smoke + narrative audit + mission flow all green. |
| Level03_Underground_Labs | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | Automated smoke + narrative audit + mission flow all green. |
| Level04_Qhipu_Core | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | Automated smoke + narrative audit + mission flow all green. |
| Level05_General_Taki_Villa | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | PASS | Automated smoke + narrative audit + mission flow all green. |

## 3. Defects Found
No blocking defects observed in RC0 run.

## 4. Summary
1. Overall status: `PASS`
2. Blocking defects count: `0`
3. Non-blocking defects count: `2` (see known issues list)
4. Recommendation: proceed to next iteration with telemetry-driven balancing.

## 5. Automated Evidence Used
1. EditMode regression: `274/274 PASS` (`codex-i5-rc0-editmode-results.xml`)
2. PlayMode regression: `10/10 PASS` (`codex-i5-rc0-playmode-results.xml`)
3. 5-scene movement/perception/mission/narrative/runtime-integrity smoke: PASS (PlayMode suite).
