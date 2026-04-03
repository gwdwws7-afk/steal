# Iteration 22 Manual Smoke Checklist

Date: 2026-04-02  
Scope: `I22-T1` pre-release human acceptance (tool parity + five-level playable loop)

## 1. Environment
1. Unity: `6000.2.14f1`
2. Project: `C:\test\Steal`
3. Preferred mode: Editor Play with visible Game view and input focus.

## 2. Tool Input Parity (Must Pass)
1. Numeric key `1-4` selects equipped tools correctly.
2. Mouse wheel cycles tools (forward/backward), skipping empty slots.
3. `ESC` clears current active tool selection.
4. Right-button short press/release uses active tool once.
5. Right-button hold then release cancels pending use (no unwanted fire).

## 3. Loadout Capacity + Feedback (Must Pass)
1. Total slot cost up to `4` equips successfully.
2. Equip request over total slot cost `4` is blocked.
3. HUD shows slot occupancy markers (`[2]` style) for multi-slot tools.
4. HUD capacity line updates (`used/max`, `remaining`).
5. Over-capacity attempt shows rejection reason text.

## 4. Five-Level Loop (Must Pass)
1. Level01~05 all load and begin with no input exceptions.
2. Tool loop (select/use/cancel) remains stable during stealth and combat.
3. Intel pickup -> exit unlock -> debrief chain remains valid.
4. Menu/pause/save-load path remains usable across the run.

## 5. Sign-off
1. `PASS` requires all Must Pass items green and no new P0/P1/P2 defects.
2. Any `FAIL` must include reproducible steps and severity.
