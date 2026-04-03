# Iteration 11 Manual Smoke Run Log

Date: 2026-04-01
Executor: Codex
Unity: 6000.2.14f1
Project: `C:\test\Steal`

## 1. Scope
Manual smoke acceptance follow-up for Iteration 11 (`I11-T5`), based on existing multi-scene smoke checklist.

## 2. Run Method
This pass used automation-backed smoke proxy execution in batch workflow:
1. EditMode full suite.
2. PlayMode full suite (including scene-smoke and mission-loop coverage suites).
3. MainMenu scene/prefab binding integrity tests.

## 3. Case Status

| Case Group | Status |
|---|---|
| Core Loop Cases (Level01~05) | PASS_PROXY |
| Save Reliability Cases | PASS_PROXY |
| Localization Cases | PASS_PROXY |
| Level Pacing/Pressure Cases | PASS_PROXY |

## 4. Automated Evidence

| Suite | Total | Passed | Failed | Result File |
|---|---:|---:|---:|---|
| EditMode | 321 | 321 | 0 | `codex-i11-editmode-results.xml` |
| PlayMode | 16 | 16 | 0 | `codex-i11-playmode-results.xml` |

## 5. Result
Iteration 11 smoke proxy status: `PASS_PROXY`.
