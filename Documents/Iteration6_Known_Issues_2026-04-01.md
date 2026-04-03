# Iteration 6 Known Issues

Date: 2026-04-01
Scope: RC1 gate (`I6-T6`)

## Active Issues

| ID | Priority | Area | Description | Impact | Owner | Target |
|---|---|---|---|---|---|---|
| KI-006-01 | P2 | Localization | Runtime UI still mixes English and Chinese strings across non-critical menus (not mojibake, but style is inconsistent). | Cosmetic consistency issue only. | UI/Localization | I7 localization pass |
| KI-006-02 | P2 | Optional Exit Design | Secondary exits are now data-enabled (`requiresAllIntel = false`), but encounter scripting/score weighting for optional extraction routes is still basic. | Usable but not yet fully differentiated in mission scoring. | Level Design | I7 mission-logic refinement |

## Closed in Iteration 6
1. Tool runtime tuning no longer depends on script constants only.
2. AI alert null-reference issue in playmode path is resolved.
3. Level route/time planning fields are now present and validated.

## Gate Status
RC1 remains `PASS` with current known-issue profile.
