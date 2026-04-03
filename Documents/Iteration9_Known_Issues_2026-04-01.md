# Iteration 9 Known Issues

Date: 2026-04-01
Scope: RC4 readiness

## Active Issues

| ID | Priority | Area | Description | Impact | Owner | Target |
|---|---|---|---|---|---|---|
| KI-009-01 | P2 | QA Process | Manual interactive smoke is still pending in this execution pass. | RC4 final sign-off cannot be considered complete until manual log is filled. | QA | Immediate next run |
| KI-009-02 | P2 | Localization Rollout | Localization service is integrated for enemy naming, but broader runtime UI keys are only partially migrated. | Mixed migration state may persist until full key adoption across UI/scripts. | Systems/UI | Iteration 10 |
| KI-009-03 | P3 | Save UX | Multi-slot save/recovery APIs exist, but menu-level slot selection UI is not yet exposed. | Users cannot select slots through full in-game UX yet. | UI/Core | Iteration 10 |

## Closed in Iteration 9
1. Single-slot-only save path is removed as a hard limitation (now multi-slot with backup recovery).
2. Localization now has centralized key table + resolver + tests.
3. Level pacing now has explicit window/pressure metadata and monotonicity checks.
4. Performance/stability stress gate is added to PlayMode regression.

## Gate Status
1. Automated RC4 readiness: `PASS`.
2. Manual smoke sign-off: `PENDING`.
