# Iteration 8 Save Migration Audit

Date: 2026-04-01
Scope: Save compatibility and schema migration (`I8-T3`)

## 1. Objective
Ensure older save payloads can be loaded safely after mission-result schema expansion introduced in Iteration 7/8.

## 2. Runtime Changes
Updated file:
1. `Assets/INTIFALL/Scripts/Runtime/Core/SaveLoadManager.cs`

Key additions:
1. `CurrentSaveSchemaVersion = 2`
2. `DeserializeWithMigration(...)`
3. Schema normalization and migration helpers.
4. `SaveMigrationAppliedEvent` for runtime observability.
5. Mission snapshot fields persisted into save data:
   - Rank/rankScore/credits/intel/secondary progress.
   - Route id/label/risk/multiplier.
   - Tool and alert pressure counters.

## 3. Migration Rules
1. Missing/legacy `schemaVersion` is interpreted as schema `1`.
2. Legacy invalid level index (for example `currentLevel <= 0`) is normalized to valid range.
3. Newly introduced mission snapshot fields receive safe defaults.
4. All numeric fields are clamped to non-negative or bounded ranges before use.

## 4. Validation Evidence
1. `Assets/INTIFALL/Tests/SaveLoadManagerMigrationTests.cs`
   - Legacy schema upgrades to current version.
   - Current schema mission snapshot remains intact.
   - Invalid JSON returns `null` without crash.
2. Regression suites:
   - EditMode PASS (`302/302`)
   - PlayMode PASS (`13/13`)

## 5. Outcome
1. Save load path now has explicit forward-compatibility handling.
2. Iteration 8 mission metadata can persist and round-trip safely.
3. Migration execution is externally observable through event publication.

