# Iteration 6 Level Playability Config Audit

Date: 2026-04-01
Executor: Codex
Scope: `I6-T4` (`LevelData` + `IntelSpawnData`)

## 1. Route Design Fields (LevelData)

| Level | Main Routes | Stealth Routes | Designed Minutes | Standard Time (s) | Time Limit (s) |
|---|---:|---:|---:|---:|---:|
| Level01_Qhapaq_Passage | 2 | 1 | 12 | 720 | 900 |
| Level02_Temple_Complex | 2 | 1 | 15 | 900 | 1080 |
| Level03_Underground_Labs | 2 | 1 | 18 | 1080 | 1260 |
| Level04_Qhipu_Core | 2 | 1 | 20 | 1200 | 1440 |
| Level05_General_Taki_Villa | 3 | 1 | 22 | 1320 | 1560 |

## 2. Exit Topology (IntelSpawnData)
Each level now includes:
1. Main exit (`requiresAllIntel = true`).
2. Secondary tactical exit (`requiresAllIntel = false`).

## 3. Loader Integration
`LevelLoader.SpawnExitPoint()` now respects per-exit intel requirements instead of forcing all exits to require full intel.

## 4. Validation Coverage
1. `SpawnCoverageTests` validates intel/supply/exit/vent minimum coverage.
2. `LevelDataFlowProfileTests` validates route-count and time-budget constraints.
