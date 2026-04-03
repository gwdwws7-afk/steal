# Iteration 22 Manual Smoke Run Log

Date: 2026-04-02  
Executor: Codex  
Unity: 6000.2.14f1  
Project: `C:\test\Steal`

## 1. Scope
Execution start for `I22-T1`:
1. Tool input parity validation.
2. Loadout capacity and HUD feedback validation.
3. Five-level loop sanity before final hand-play sign-off.

## 2. Execution Method
Current pass is automation-backed evidence collection (`PASS_PROXY`) to start I22:
1. EditMode regression run.
2. PlayMode regression run.
3. Existing targeted tests include:
   1. `ToolManagerTests`
   2. `ToolHUDTests`
   3. `ToolDataConfigurationTests`
   4. Scene/movement/mission/UI PlayMode smoke suites.

## 3. Automated Results (I22 Prefix)
| Suite | Total | Passed | Failed | Result File |
|---|---:|---:|---:|---|
| EditMode | 351 | 351 | 0 | `codex-i22-t1-editmode-results-2026-04-02.xml` |
| PlayMode | 21 | 21 | 0 | `codex-i22-t1-playmode-results-2026-04-02.xml` |

## 4. Case Status Snapshot
| Case Group | Status | Note |
|---|---|---|
| Tool Input Parity | PASS_PROXY | Covered by `ToolManagerTests` + PlayMode UI/tools smoke. |
| Loadout Capacity + Feedback | PASS_PROXY | Covered by `ToolManagerTests` + `ToolHUDTests` + asset config checks. |
| Five-Level Loop | PASS_PROXY | Covered by Level01~05 PlayMode smoke suites. |
| Physical hand-play UX confirmation | PENDING_MANUAL | Requires direct interactive play session in Editor/Game view. |

## 5. Interim Verdict
`I22-T1` execution has started and proxy gate is `PASS_PROXY`.  
Blocking defects detected in this pass: `0`.

## 6. Follow-up Required for Full Manual Sign-off
1. Run one dedicated hand-play pass against `Iteration22_Manual_Smoke_Checklist.md`.
2. Append manual observations/screenshots/repro steps to this log.
