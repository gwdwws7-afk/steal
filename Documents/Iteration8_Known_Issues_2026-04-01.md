# Iteration 8 Known Issues

Date: 2026-04-01
Scope: RC3 automated gate (`I8-T5`)

## Active Issues

| ID | Priority | Area | Description | Impact | Owner | Target |
|---|---|---|---|---|---|---|
| KI-008-01 | P3 | QA Process | Manual interactive smoke was skipped by explicit request for this iteration. | Residual UX risk for interactions only verifiable by hand-play. | QA | Next manual acceptance window |
| KI-008-02 | P2 | Save Operations | Save system is now schema-migrated but still single-slot (`PlayerPrefs`) with no built-in backup slot. | Corruption-recovery options are limited for field failures. | Core Systems | Post-RC3 hardening |
| KI-008-03 | P2 | Localization Pipeline | Data-layer bilingual fields and keys are present for enemy type data, but no centralized runtime table resolver is enforced project-wide yet. | Future content growth may require repeated per-system localization wiring. | Localization/Tools | Next localization infrastructure pass |

## Closed in Iteration 8
1. `KI-007-01` (EnemyTypeData Chinese-first default) is closed with bilingual fields and resolver.
2. Optional-route reward now has pressure penalties and no longer dominates in high-alert cases.
3. Save payload compatibility now includes schema versioning and migration.
4. Event subscription and enemy squad static-state accumulation risks were reduced with dedicated tests.

## Gate Status
RC3 automated gate remains `PASS` with current known-issue profile.
