# RC Known Issues

Date: 2026-04-02

## Active Issues

| ID | Priority | Area | Description | Impact | Plan |
|---|---|---|---|---|---|
| None | - | - | No active blocker or parity issue after I22-T4 freeze/handoff closure. | - | Continue routine regression gates and manual sign-off policy check. |

## Closed In P0 -> P1 Closure Pass (2026-04-02)
| ID | Priority | Area | Closure Summary | Evidence |
|---|---|---|---|---|
| KI-020-01 | P0 | Runtime Spawn | `LevelLoader` now supports release-safe runtime fallback spawn when scene prefab arrays are empty; enemy/intel clones are explicitly activated and discoverable in runtime/test scans. | `Assets/INTIFALL/Scripts/Runtime/Level/LevelLoader.cs`, `codex-p0p1-fix-playmode-rerun-2026-04-02.xml` (`21/21 PASS`) |
| KI-020-02 | P1 | Data Consistency | Runtime load now auto-resolves scene data from `Resources/INTIFALL` and prefers resource-tuned profiles by scene name. | `Assets/INTIFALL/Scripts/Runtime/Level/LevelLoader.cs`, `codex-p0p1-fix-editmode-rerun-2026-04-02.xml` (`341/341 PASS`) |
| KI-020-03 | P1 | Progression Loop | Mission completion now applies progression/economy integration and runtime bootstrap ensures required meta systems exist in core scene flow. | `Assets/INTIFALL/Scripts/Runtime/Level/LevelLoader.cs`, `Assets/INTIFALL/Scripts/Runtime/Level/MissionExitPoint.cs`, EditMode/PlayMode rerun all green |

## Closed In P2 Closure Pass (2026-04-02)
| ID | Priority | Area | Closure Summary | Evidence |
|---|---|---|---|---|
| KI-020-04 | P2 | Tool Design Parity | `ToolManager` now supports mouse-wheel selection, cancel-by-ESC and right-button hold-release cancel path, and enforces loadout slot-cost capacity (`4` total slots with multi-slot tools). `ToolData` and runtime assets were updated with `slotCost`, including multiple `2-slot` tools. | `Assets/INTIFALL/Scripts/Runtime/Tools/ToolManager.cs`, `Assets/INTIFALL/ScriptableObjects/ToolData.cs`, `Assets/INTIFALL/ScriptableObjects/Tools/*.asset`, `codex-p2-editmode-rerun-2026-04-02.xml` (`347/347 PASS`), `codex-p2-playmode-results-2026-04-02.xml` (`21/21 PASS`) |

## Historical Note
1. Prior RC-prep closure snapshot is superseded by this document state.
2. Current active blocker classes:
   1. `P0`: `0`
   2. `P1`: `0`
   3. `P2`: `0`

## Re-Audit Note
1. `2026-04-02` full design re-audit supersedes the previous "no active issue" snapshot.
2. Evidence and detailed assessment:
   1. `Documents/Iteration20_System_Design_Reaudit_2026-04-02.md`

## Post-Package Recheck (`I22-T3`)
1. Stability gate rerun:
   1. `codex-i22-t3-stability-editmode-results.xml` (`351/351 PASS`)
   2. `codex-i22-t3-stability-playmode-results.xml` (`21/21 PASS`)
2. New blocker classes introduced after rerun:
   1. `P0`: `0`
   2. `P1`: `0`
   3. `P2`: `0`
