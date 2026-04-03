# Iteration 22 Release Notes

Date: 2026-04-02  
Version Tag: `RC-I22`  
Unity: 6000.2.14f1

## 1. Highlights
1. Tool-system parity is complete for release path:
   1. Numeric (`1-4`) and mouse-wheel selection.
   2. `ESC` cancel and right-button hold/release cancel behavior.
   3. Slot-cost loadout enforcement with multi-slot tools.
2. ToolHUD now provides capacity-oriented player feedback:
   1. Slot labels indicate multi-slot occupancy (for example `[2]`).
   2. Live loadout capacity text (`used/max`, `remaining`).
   3. Equip rejection reason text when capacity is exceeded.
3. Heavy tool balancing pass under slot-cost constraints:
   1. EMP/Drone/Rope/WallBreak cadence and/or ammo adjusted to preserve viable strategy diversity.

## 2. Quality Snapshot
1. I22 targeted regression:
   1. EditMode: `351 / 351 PASS` (`codex-i22-t1-editmode-results-2026-04-02.xml`)
   2. PlayMode: `21 / 21 PASS` (`codex-i22-t1-playmode-results-2026-04-02.xml`)
2. I22 one-click RC final gate:
   1. EditMode: `351 / 351 PASS` (`codex-i22-rcfinal-editmode-results.xml`)
   2. PlayMode: `21 / 21 PASS` (`codex-i22-rcfinal-playmode-results.xml`)
3. I22-T3 post-package stability gate:
   1. EditMode: `351 / 351 PASS` (`codex-i22-t3-stability-editmode-results.xml`)
   2. PlayMode: `21 / 21 PASS` (`codex-i22-t3-stability-playmode-results.xml`)
4. I22-T4 release freeze:
   1. Artifact manifest generated: `Documents/Iteration22_Release_Artifacts_Manifest_2026-04-02.csv`
   2. Freeze/handoff closure report: `Documents/Iteration22_T4_Release_Freeze_Handoff_Report_2026-04-02.md`

## 3. Known Issues
1. Active blocker defects: none (`P0/P1/P2 = 0`).
2. Human hand-play sign-off remains process-dependent:
   1. Current automated/proxy smoke status is `PASS_PROXY`.
   2. If governance requires strict manual hand-play sign-off, append a human-run pass to `Iteration22_Manual_Smoke_Run_Log_2026-04-02.md`.
