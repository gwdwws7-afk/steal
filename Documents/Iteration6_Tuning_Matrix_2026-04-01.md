# Iteration 6 Tuning Matrix

Date: 2026-04-01
Executor: Codex
Scope: `I6-T1` + `I6-T5`

## 1. Tool Data-Driven Mapping

| Tool | Range | Cooldown | Duration | MaxAmmo | Notes |
|---|---:|---:|---:|---:|---|
| SmokeBomb | 6.0 | 16.0 | 7.0 | 3 | Vision denial baseline. |
| FlashBang | 9.0 | 24.0 | 3.0 | 2 | Higher commitment burst CC. |
| SleepDart | 18.0 | 4.0 | 16.0 | 5 | Precision non-lethal option. |
| SoundBait | 7.0 | 8.0 | 2.5 | 4 | Fast attention redirection. |
| TimedNoise | 18.0 | 22.0 | 4.0 | 2 | Delayed lure for route sync. |
| EMP | 6.0 | 32.0 | 10.0 | 1 | High-impact scarce disruptor. |
| WallBreaker | 2.4 | 7.0 | 2.6 | 2 | Environmental breach utility. |
| Rope | 12.0 | 6.0 | 8.0 | 3 | Mobility and traversal support. |
| DroneInterference | 15.0 | 50.0 | 24.0 | 1 | Long-duration tactical lure. |

## 2. Runtime Integration
1. `ToolManager` now routes `ToolData` into `ToolBase.ApplyToolData()`.
2. Tool-specific parameters (radius/effect duration/travel distance) now consume mapped config values.
3. `ToolData` asset tests enforce non-zero operational stats and readable naming.

## 3. Supply Economy Adjustment
1. Supply point now uses configurable incremental tool refill (`toolAmmoRefillAmount`) instead of full refill by default.
2. Optional cooldown reset at supply is explicit (`resetToolCooldownOnSupply`) and disabled by default.

## 4. Expected Effect
1. Balance changes can be performed with data edits instead of script edits.
2. Tool choice diversity improves by clearer cooldown/ammo opportunity cost.
3. Regression risk is reduced by asset-level validator tests.
