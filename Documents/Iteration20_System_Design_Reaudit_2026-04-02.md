# Iteration20 System Design Re-Audit (S1~S10)

Date: 2026-04-02  
Executor: Codex  
Unity Target: 6000.2.14f1  
Scope: Re-audit all systems against `Assets/Documents/System_Design_INTI_FALL.md`, `Assets/GDD_INTI_FALL.md`, runtime code, scene bindings, and runtime data assets.

## 1. Executive Verdict

1. Automated gates remain green (EditMode/PlayMode), but full design-to-runtime closure is not fully complete.
2. Current status should be treated as:
   1. `Automation/Regression Readiness`: PASS
   2. `Full GDD/SDD Runtime Completeness`: CONDITIONAL (has remaining gaps)
3. New blocker-class risk found in release-mode runtime path (see `P0-01`).

## 2. Key Findings

### P0 (Release Blocker)

| ID | Finding | Evidence | Impact | Required Fix |
|---|---|---|---|---|
| P0-01 | Core level scenes have empty `enemyPrefabs` / `intelPrefabs`, but strict runtime path disables placeholder fallback in non-editor release builds. | `Assets/Scenes/Level01_Qhapaq_Passage.unity:636-637` (same pattern in Level01~05), `Assets/INTIFALL/Scripts/Runtime/Level/LevelLoader.cs:166`, `:306`, `:528-538` | In release strict-runtime mode, enemy/intel spawn can fail, causing broken core loop. | Bind real prefabs for all 5 levels or explicitly allow strict fallback with release-safe strategy (preferred: bind prefabs). |

### P1 (High Priority Functional Gaps)

| ID | Finding | Evidence | Impact | Required Fix |
|---|---|---|---|---|
| P1-01 | Scene runtime uses `Assets/INTIFALL/ScriptableObjects` references (legacy variants), not the newer `Assets/Resources/INTIFALL` tuned profiles. | `Assets/Scenes/Level01_Qhapaq_Passage.unity:627-629` | Runtime behavior can diverge from audited/tuned data. | Unify to a single source of truth; migrate scene refs to resource-tuned assets (or regenerate INTIFALL mirror to match). |
| P1-02 | Runtime `INTIFALL` LevelData assets miss flow-profile fields now used by design audits (route/time/pressure profile), so runtime falls back to class defaults. | Missing fields in `Assets/INTIFALL/ScriptableObjects/Levels/LevelData_*.asset`; defaults in `Assets/INTIFALL/ScriptableObjects/LevelData.cs:15-32` | Level pacing/pressure progression can flatten in runtime despite resource-side tuning. | Backfill all new LevelData fields into runtime-bound assets for all 5 levels. |
| P1-03 | Runtime `INTIFALL` IntelSpawn assets still provide one exit per level without route/risk/multiplier fields, while resource-side assets define dual-route extraction. | `Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_*.asset` vs `Assets/Resources/INTIFALL/Spawns/IntelSpawn_*.asset` | Optional extraction loop and route-risk reward gradient may not be active in real scene flow. | Sync runtime-bound IntelSpawn assets to dual-exit structure (`main + optional`) including route fields. |
| P1-04 | Economy/growth systems are not wired into core scenes end-to-end. | Scene scan: `CreditSystem.cs`, `ArsenalUI.cs`, `ProgressionTree.cs`, `BloodlineSystem.cs`, `LevelUpReward.cs` attached in `0` core scenes; mission completion path uses `MissionExitPoint.cs:162,236` + `LevelFlowManager.cs:134-149` only | S7/S8 loop exists in code but is not fully active during normal gameplay progression path. | Add runtime ownership for economy/growth in flow (scene bootstrap + mission completion integration). |

### P2 (Medium Priority Design-Consistency Gaps)

| ID | Finding | Evidence | Impact | Required Fix |
|---|---|---|---|---|
| P2-01 | Tool switching/usage controls do not fully match design rules (number keys implemented, mouse wheel/cancel path not present). | `Assets/INTIFALL/Scripts/Runtime/Tools/ToolManager.cs:74-79` | Usability and design parity gap. | Add wheel-based selection and explicit cancel path. |
| P2-02 | Tool slot cost model (e.g., 2-slot tools) is not represented in current ToolData/ToolManager constraints. | `Assets/INTIFALL/ScriptableObjects/ToolData.cs` + `ToolManager.cs` (`maxToolSlots=4`, no slot-cost handling) | Build diversity/balance constraints are weaker than design intent. | Add slot cost metadata and enforce loadout capacity by total cost. |

## 3. System Completion Re-Estimate

| System | Completion (Estimate) | Re-audit Assessment |
|---|---:|---|
| S1 角色移动 | 80% | Walk/Sprint/Crouch/Roll/Rope/Vent/cover available; some design-fidelity details remain simplified. |
| S2 潜行 | 74% | Vision/hearing/shadow/comms chain present; part of priority logic and tuning depth still simplified. |
| S3 战斗 | 70% | CQC/backstab/sleep/alert-to-combat loop works; encounter-level combat behaviors remain lightweight. |
| S4 敌人 AI 状态机 | 79% | State machine + squad broadcast + sector search implemented and stable in tests. |
| S5 敌人感知 | 72% | Core module complete; per-enemy full parameter table is not fully data-wired end-to-end. |
| S6 工具系统 | 73% | Tool framework and major tools are present; slot-cost/design interaction rules partially missing. |
| S7 经济系统 | 58% | Credit/reward code exists; runtime scene wiring and meta-loop closure are incomplete. |
| S8 成长系统 | 54% | Progression/bloodline modules exist; mission-flow integration in core runtime is incomplete. |
| S9 叙事系统 | 82% | Message catalog and trigger chain are covered; content flow continuity is strong. |
| S10 关卡布局 | 68% | Whitebox/spawn coverage exists; runtime data source divergence reduces guaranteed design fidelity. |

## 4. Recommended Closure Order

1. `P0-01`: bind enemy/intel prefabs for Level01~05 and rerun strict-runtime verification.
2. `P1-01~P1-03`: unify runtime data source and remove INTIFALL/Resources divergence.
3. `P1-04`: wire S7/S8 runtime ownership and mission completion integration.
4. `P2-01~P2-02`: align tool controls and loadout constraints with design rules.

## 5. Conclusion

1. The project is regression-stable but not yet fully design-complete at runtime path.
2. Blocking gaps are now explicitly identified and should be tracked as active known issues until closed.
