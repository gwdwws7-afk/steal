## Iteration 25 - Debrief Tool Window Visibility (2026-04-03)

### Scope
- Expose tool cooldown/risk-window settlement details to mission debrief UI.
- Keep settlement telemetry machine-testable and regression-gated.

### Runtime Changes
1. `GameManager` mission result payload now carries tool-window settlement telemetry:
   - `ToolRiskWindowAdjustment`
   - `ToolCooldownLoad`
   - `RopeToolUses`
   - `SmokeToolUses`
   - `SoundBaitToolUses`
2. `MissionOutcomeEvaluatedEvent` now forwards the same telemetry from `MissionResult`.
3. `MissionDebriefUI` summary now includes:
   - tool-window score line (`+/-N` with qualitative assessment)
   - tool-mix line (rope/smoke/bait usage and cooldown load)
4. Localization table updated with debrief keys:
   - `debrief.line.tool_window`
   - `debrief.line.tool_mix`
   - `debrief.tool_window.positive`
   - `debrief.tool_window.neutral`
   - `debrief.tool_window.negative`

### Automated Coverage
1. `MissionDebriefUITests`
   - verifies new tool-window/tool-mix lines are rendered in summary.
2. `ToolRiskWindowScoringTests`
   - verifies high-risk balanced plan has better `ToolRiskWindowAdjustment` than baseline/spam.
   - verifies medium-risk smoke+bait has better adjustment than rope-only.
3. `MissionExitPointTests`
   - verifies expanded mission outcome event telemetry fields are present.

### Verification
- EditMode: `367/367 PASS`
- PlayMode: `27/27 PASS`
- Runner:
  - `run-regression-tests.ps1 -TestPlatform EditMode`
  - `run-regression-tests.ps1 -TestPlatform PlayMode`

### Status
- Debrief visibility for tool cooldown/risk-window settlement is now complete and regression-gated.
