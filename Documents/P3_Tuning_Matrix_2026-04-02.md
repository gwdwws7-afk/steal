# P3 Tuning Matrix

Date: 2026-04-02  
Scope: `P3-T3` (AI rhythm, tool economy, reward banding)

## 1. AI Rhythm Injection
Updated runtime to apply level pressure profile into spawned enemies:
1. `Assets/INTIFALL/Scripts/Runtime/AI/EnemyStateMachine.cs`
2. `Assets/INTIFALL/Scripts/Runtime/AI/EnemyController.cs`
3. `Assets/INTIFALL/Scripts/Runtime/Level/LevelLoader.cs`

Applied profile (derived from `LevelData`):
1. Alert/search/full-alert timers map to level design fields.
2. Alert drop delay and suspicious window scale by `patrolPressureTier`.
3. Detection pulse interval and squad broadcast cadence tighten with higher pressure tier.

Validation:
1. `Assets/INTIFALL/Tests/PlayMode/Iteration13AITuningPlayModeTests.cs`

## 2. Tool Economy Retune
Adjusted `ToolData` runtime stats to avoid one-tool dominance and strengthen role tradeoffs:
1. Drone: `range 18`, `cooldown 42`, `ammo 1`
2. DroneInterference: `range 17`, `cooldown 36`, `ammo 1`
3. EMP: `range 7`, `cooldown 26`, `ammo 1`
4. FlashBang: `range 10`, `cooldown 18`, `ammo 2`
5. Rope: `range 12`, `cooldown 5`, `ammo 3`
6. SleepDart: `range 16`, `cooldown 12`, `ammo 2`
7. SmokeBomb: `range 8`, `cooldown 14`, `ammo 3`
8. SoundBait: `range 9`, `cooldown 7`, `ammo 4`
9. TimedNoise: `range 20`, `cooldown 17`, `ammo 3`
10. WallBreak/WallBreaker: `range 2.5`, `cooldown 9`, `ammo 2`

Validation:
1. `Assets/INTIFALL/Tests/ToolDataConfigurationTests.cs` adds non-dominance guard.

## 3. Reward Banding Adjustment
Updated mission reward shaping to widen S/A/B/C payout separation:
1. `Assets/INTIFALL/Scripts/Runtime/System/GameManager.cs`
2. Added rank-band offset in final credits:
   - S: `+120`
   - A: `+60`
   - B: `+0`
   - C: `-55`
   - D: `-120`

Validation:
1. `Assets/INTIFALL/Tests/MissionRewardBandingTests.cs`
2. `Assets/INTIFALL/Tests/MissionRouteScoringTests.cs` remains green (optional route no longer always dominant under pressure).

## 4. Outcome
`P3-T3` completed with automated regression coverage and no new P0/P1/P2 regressions.
