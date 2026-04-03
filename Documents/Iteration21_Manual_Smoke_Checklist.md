# Iteration 21 Manual Smoke Checklist

Date: 2026-04-02  
Scope: `I21-T2` tool-parity and five-level playability follow-up

## 1. Test Environment
1. Unity: `6000.2.14f1`
2. Project: `C:\test\Steal`
3. Run mode: Editor Play (manual) + PlayMode smoke proxy (automation)

## 2. Tool Input Parity Cases
1. `1-4` numeric selection switches active tool correctly.
2. Mouse wheel cycles equipped tools and skips empty slots.
3. `ESC` clears active tool selection.
4. Right-button hold then release triggers cancel (no accidental tool fire).
5. Right-button short press/release uses active tool when legal.

## 3. Loadout Capacity Cases
1. Equipping tools up to total slot cost `4` succeeds.
2. Equipping beyond total slot cost `4` is blocked.
3. `2-slot` tools show occupancy feedback in HUD (slot label and capacity text).
4. Capacity-rejected equip displays user-facing reason text.

## 4. Five-Level Loop Cases (L01~L05)
1. Enter level, tool HUD present, capacity text updates after equipment changes.
2. Stealth/combat/tool usage loop remains stable after tool parity changes.
3. Mission extraction and debrief still complete without regressions.

## 5. Sign-off Rules
1. `PASS` requires all mandatory cases pass and no P0/P1/P2 blocker appears.
2. Any `FAIL` must include reproduction steps and severity.
