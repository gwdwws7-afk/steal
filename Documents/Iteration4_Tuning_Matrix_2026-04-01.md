# Iteration 4 Tuning Matrix

Date: 2026-04-01  
Executor: Codex  
Scope: `I4-T4` (`EnemyStateMachine`, `ToolBase`, `LevelUpReward`)

## 1. Tuning Goals
1. Reduce abrupt AI state oscillation during stealth break/restore windows.
2. Stabilize tool economy behavior (no negative ammo, deterministic cooldown semantics).
3. Rebalance mission payout pacing to better differentiate run quality.

## 2. Parameter Matrix

| System | Parameter | Before | After | Intent |
|---|---|---:|---:|---|
| `EnemyStateMachine` | `suspiciousDuration` | `2.5` | `2.2` | Slightly faster initial suspicion resolution. |
| `EnemyStateMachine` | `searchDuration` | `6.0` | `7.2` | Longer search tail to support stealth reposition loops. |
| `EnemyStateMachine` | `alertDuration` | `4.0` | `3.5` | Faster escalation/decay cycle under active detection. |
| `EnemyStateMachine` | `alertDropToSearchDelay` | `1.25` | `1.1` | Shorten alert-to-search drop when LOS is broken. |
| `EnemyStateMachine` | `fullAlertDropToSearchDelay` | `2.0` | `2.6` | Keep full-alert pressure longer before downshifting. |
| `EnemyStateMachine` | `minimumStateDuration` | `N/A` | `0.2` | New safety floor to avoid invalid zero/negative timing configs. |
| `ToolBase` | Ammo consume rule | Always `-1` on use | Consume only when tool is limited-ammo | Remove unlimited-tool depletion bug. |
| `ToolBase` | Cooldown start rule | Always set cooldown flag | Set cooldown only when `cooldown > 0` | Avoid false cooldown lock on instant-use tools. |
| `ToolBase` | `CooldownProgress` | Raw ratio (`_currentCooldown/cooldown`) | Clamped ratio with zero-cooldown guard | Prevent divide/invalid progress edge cases. |
| `ToolBase` | Reload cap | `maxAmmo` only | `max(maxAmmo, ammo)` | Keep compatibility with legacy configs. |
| `LevelUpReward` | Rank formula | Static 4-branch thresholds | Expanded objective/intel/time-aware thresholds | Better mid-tier differentiation (`B/C`) for partial completion. |
| `LevelUpReward` | Credit base (`D/C/B/A/S`) | `50/100/200/350/500` | `70/140/240/370/520` | Smoother progression and reduced jump spikes. |
| `LevelUpReward` | Bonus mix | `zeroKill +150`, `noDamage +200`, fixed time bonus | `zeroKill +120`, `noDamage +130`, tiered time bonus (`+45/+90`) | Keep stealth bonuses strong but less binary. |

## 3. Functional Safeguards Added
1. `EnemyStateMachine` now normalizes timing in `Awake` and `OnValidate`.
2. `ToolBase` now treats unlimited-ammo tools as non-depleting and non-reloadable.
3. `LevelUpReward` rewritten in ASCII/clean format to remove corrupted string literals and improve maintainability.

## 4. Validation Evidence
1. `Assets/INTIFALL/Tests/ToolBaseTests.cs` includes `Use_UnlimitedAmmo_DoesNotGoNegative` and all tool-base tests pass.
2. `Assets/INTIFALL/Tests/LevelUpRewardTests.cs` pass with updated scoring/credit behavior.
3. Full EditMode regression: `codex-i4-final-editmode-results.xml` (`267/267 PASS`).

## 5. Notes
1. This pass focused on system-level stability and reward pacing, not final content-authoring balance values per level.
2. Further micro-tuning should be driven by playtest telemetry (alert dwell time, tool usage frequency, mission payout distribution).
