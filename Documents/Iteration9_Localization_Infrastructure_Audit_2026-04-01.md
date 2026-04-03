# Iteration 9 Localization Infrastructure Audit

Date: 2026-04-01
Scope: `I9-T3`

## 1. Objective
Move localization from per-class fallback strings to a centralized key-driven resolver that supports language selection and testable key coverage.

## 2. Runtime Changes
Added:
1. `Assets/INTIFALL/Scripts/Runtime/System/LocalizationService.cs`
2. `Assets/Resources/INTIFALL/Localization/LocalizationTable.json`

Updated:
1. `Assets/INTIFALL/ScriptableObjects/EnemyTypeData.cs`

## 3. Localization Service Features
1. Resource-backed localization table loading.
2. Key-based lookup with language override support.
3. Fallback chain:
   - localized value by key
   - language-specific fallback field
   - key itself
4. Test hooks:
   - `Reload()`
   - `ResetForTests()`
   - `LoadedKeyCount`
   - `HasKey(...)`

## 4. Table Coverage in This Pass
1. Enemy identity keys (`enemy.*`) for all enemy types.
2. Selected HUD/route keys prepared for broader runtime adoption.

## 5. Validation
Added:
1. `Assets/INTIFALL/Tests/LocalizationServiceTests.cs`

Covered assertions:
1. Localization table loads successfully.
2. Key resolution returns language-specific values.
3. Missing keys return deterministic language fallback.
4. All `EnemyTypeData` localization keys exist in the localization table.

## 6. Outcome
1. Core localization infra is now centralized and reusable.
2. Enemy naming path now resolves through key-based localization first.
3. Project has a maintainable foundation for expanding localization coverage.
