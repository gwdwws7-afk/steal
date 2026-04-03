# Iteration 7 Known Issues

Date: 2026-04-01
Scope: RC2 gate (`I7-T6`)

## Active Issues

| ID | Priority | Area | Description | Impact | Owner | Target |
|---|---|---|---|---|---|---|
| KI-007-01 | P2 | Localization Data | `EnemyTypeData` helper defaults still use Chinese labels while runtime UI flow is now English-first. | Cosmetic consistency in non-runtime data editors only. | UI/Localization | I8 data-table localization pass |
| KI-007-02 | P2 | Economy Tuning | Optional extraction credit model is deterministic but still rule-based; telemetry-driven balancing not yet applied. | Gameplay balance refinement opportunity, no blocker. | Economy Design | I8 balancing pass |
| KI-007-03 | P3 | QA Process | RC2 smoke evidence is automated PlayMode harness; no full hand-driven mission replay was executed in this pass. | Residual UX risk for purely manual interactions. | QA | I8 manual acceptance sweep |

## Closed in Iteration 7
1. Runtime mixed-language/mojibake issues in gameplay scripts are removed.
2. Optional extraction routes now have explicit risk/reward parameters and scoring impact.
3. Mission debrief now includes extraction, pressure, and tool-usage context.
4. Narrative mission-complete tokens now support route and pressure fields.

## Gate Status
RC2 remains `PASS` with current known-issue profile.
