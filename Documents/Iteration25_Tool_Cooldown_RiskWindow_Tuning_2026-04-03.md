# Iteration25 Tool Cooldown Risk-Window Tuning

Date: 2026-04-03  
Executor: Codex  
Unity: 6000.2.14f1  
Project: `C:\test\Steal`

## 1. Goal

Link tool usage rhythm to route-risk settlement so Rope/Smoke/SoundBait produce differentiated mission rewards by risk tier, while preventing low-quality tool spam from dominating settlement.

## 2. Runtime Changes

1. Tool telemetry expansion:
   1. `ToolUsedEvent` now carries `cooldownSeconds`.
   2. Source: `ToolBase.Use()` publishes current tool cooldown as telemetry.

2. GameManager risk-window settlement:
   1. Added mission-level tool telemetry accumulation:
      1. `_toolCooldownLoad`
      2. `_ropeToolUses`
      3. `_smokeToolUses`
      4. `_soundBaitToolUses`
   2. Added `EvaluateToolRiskWindowAdjustment(...)` and integrated it into credit settlement path.
   3. Windows by risk tier:
      1. Risk 1: SoundBait-oriented compact rhythm preferred.
      2. Risk 2: Smoke + SoundBait coordinated control window preferred.
      3. Risk 3: Rope + Smoke high-risk execution window preferred.
   4. Added penalties for low-cooldown spam / overuse to preserve non-single-optimal behavior.

## 3. Test Coverage Added

1. New test file:
   1. `Assets/INTIFALL/Tests/ToolRiskWindowScoringTests.cs`
2. New cases:
   1. `HighRisk_BalancedRopeSmokeSoundWindow_OutperformsNoToolPlan`
   2. `HighRisk_LowCooldownSpam_IsPenalizedVersusBalancedWindow`
   3. `MediumRisk_SmokeAndBaitWindow_OutperformsRopeOnlyPlan`

## 4. Verification

1. EditMode full regression:
   1. Result: `367/367 PASS`
   2. Evidence: `codex-i25-full-editmode-results.xml`

2. PlayMode full regression:
   1. Result: `27/27 PASS`
   2. Evidence: `codex-i25-full-playmode-results.xml`

## 5. Outcome

1. Route-risk settlement now differentiates tool mix quality rather than counting only raw tool usage.
2. High-risk tactical profiles gain clear reward upside, while spammy low-rhythm usage is controlled.
3. Full automation baseline remains green.
