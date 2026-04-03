# Iteration 9 Manual Smoke Checklist

Date: 2026-04-01
Scope: RC4 manual acceptance (`I9-T1`)

## 1. Test Environment
1. Unity: `6000.2.14f1`
2. Project: `C:\test\Steal`
3. Build/Run target: Editor Play (windowed, input enabled)

## 2. Core Loop Cases (Level01~05)
1. Enter level -> player can move/crouch/roll/rope without input exceptions.
2. Intel collection increments objective counter and updates UI.
3. Exit remains locked until intel requirement reached.
4. Mission complete debrief displays rank/credits/route data.

## 3. Save Reliability Cases
1. Save in slot 0, return menu, load slot 0 -> progress restored.
2. Save in slot 1 with different progress -> loading slot 0 remains unchanged.
3. Delete slot N -> both primary and backup for slot N are gone.
4. Corrupt primary key manually, keep backup valid -> load recovers from backup.

## 4. Localization Cases
1. Enemy name display resolves from key in English mode.
2. Enemy name display resolves from key in Chinese mode.
3. Missing key falls back to configured display fields (no empty string).

## 5. Level Pacing/Pressure Cases
1. Level pressure feel increases from L01 to L05 (alert persistence, supply scarcity).
2. Completion time lands inside configured per-level window.
3. No route becomes impossible due tuning changes.

## 6. Sign-off
1. PASS if all required cases pass and no P0/P1 defects appear.
2. Any FAIL must include reproduction steps and blocking severity.
