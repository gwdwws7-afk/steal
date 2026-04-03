# Iteration 21 Manual Smoke Run Log

Date: 2026-04-02  
Executor: Codex  
Unity: 6000.2.14f1  
Project: `C:\test\Steal`

## 1. Scope
Follow-up smoke for `I21-T2` after P2 closure:
1. Tool input parity (`1-4`, wheel, cancel paths).
2. Loadout slot-cost enforcement and HUD feedback.
3. Five-level mission loop sanity after tool-system changes.

## 2. Run Method
This pass used automation-backed smoke proxy execution:
1. Full EditMode suite (`ToolManagerTests`, `ToolHUDTests`, `ToolDataConfigurationTests`, integration suites).
2. Full PlayMode suite (scene smoke, mission flow, movement/perception, UI/tools chain).
3. Stability gate reruns (`run-i19-stability-gate.ps1`, `run-rc-final-gate.ps1`) with I21 prefixes.

## 3. Case Status
| Case Group | Status |
|---|---|
| Tool Input Parity Cases | PASS_PROXY |
| Loadout Capacity Cases | PASS_PROXY |
| Five-Level Loop Cases (L01~L05) | PASS_PROXY |

## 4. Automated Evidence
| Suite | Total | Passed | Failed | Result File |
|---|---:|---:|---:|---|
| EditMode | 351 | 351 | 0 | `codex-i21-editmode-results-2026-04-02.xml` |
| PlayMode | 21 | 21 | 0 | `codex-i21-playmode-results-2026-04-02.xml` |
| I19 Stability Gate (EditMode) | 351 | 351 | 0 | `codex-i21-stability-post-editmode-results.xml` |
| I19 Stability Gate (PlayMode) | 21 | 21 | 0 | `codex-i21-stability-post-playmode-results.xml` |
| RC Final Gate (EditMode) | 351 | 351 | 0 | `codex-i21-rcfinal-post-editmode-results.xml` |
| RC Final Gate (PlayMode) | 21 | 21 | 0 | `codex-i21-rcfinal-post-playmode-results.xml` |

## 5. Result
`I21-T2` smoke proxy status: `PASS_PROXY`.

## 6. Manual Note
Physical hand-play confirmation in Editor/Game view is not executed in this batch-only pass.  
If release governance requires strict manual UX sign-off, run one dedicated human play-through and append findings.
