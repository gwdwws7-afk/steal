# Iteration 21 Tool/Economy Tuning Matrix

Date: 2026-04-02  
Scope: `I21-T4` second-pass tuning after slot-cost model activation

## 1. Tuning Objective
1. Keep `2-slot` heavy tools viable under 4-capacity constraint.
2. Avoid collapsing to a single optimal low-cost loadout.
3. Preserve S/A/B/C mission economy gradient already validated in prior passes.

## 2. Tool Data Adjustments
| Tool | slotCost | Before | After | Rationale |
|---|---:|---|---|---|
| EMP | 2 | cooldown `26`, ammo `1` | cooldown `24`, ammo `2` | Reduce over-punishment from 2-slot occupation; keep electronic-control identity. |
| Drone | 2 | cooldown `42` | cooldown `38` | Increase strategic uptime for high-investment slot footprint. |
| DroneInterference | 2 | cooldown `36` | cooldown `33` | Keep parity with Drone branch while preserving distinct behavior. |
| Rope | 2 | cooldown `5`, duration `8` | cooldown `4`, duration `10` | Ensure traversal tool remains a meaningful path-enabler despite 2-slot cost. |
| WallBreak | 2 | cooldown `9` | cooldown `8` | Offset occupancy cost with slightly faster loop cadence. |
| WallBreaker | 2 | cooldown `9` | cooldown `8` | Keep duplicate profile aligned to avoid hidden data drift. |

## 3. Consistency Checks
1. `slotCost` is present for all runtime tool assets.
2. Allowed cost range remains `1~2` (`ToolDataConfigurationTests`).
3. At least one `2-slot` tool exists (multi-slot rule actively exercised).

## 4. Verification Evidence
1. EditMode: `351/351 PASS` (`codex-i21-editmode-results-2026-04-02.xml`)
2. PlayMode: `21/21 PASS` (`codex-i21-playmode-results-2026-04-02.xml`)
