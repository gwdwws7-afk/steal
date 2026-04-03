# Iteration 8 Localization Data Audit

Date: 2026-04-01
Scope: Data-layer localization closure (`I8-T1`)

## 1. Objective
Close the remaining localization gap from Iteration 7 by moving enemy naming in data definitions from single-field defaults to bilingual fields plus a stable localization key.

## 2. Data Model Update
Updated file:
1. `Assets/INTIFALL/ScriptableObjects/EnemyTypeData.cs`

Added fields:
1. `displayNameEnglish`
2. `displayNameChinese`
3. `localizationKey`
4. `GetDisplayName(SystemLanguage language)`

Behavior:
1. `displayName` now defaults to `displayNameEnglish`.
2. Chinese language requests resolve to `displayNameChinese` when present.
3. Fallback order: Chinese -> English -> legacy `displayName`.

## 3. Default Enemy Identity Table

| EnemyType | English | Chinese | Localization Key |
|---|---|---|---|
| Normal | Guard | 普通士兵 | `enemy.guard` |
| Reinforced | Reinforced Guard | 强化士兵 | `enemy.reinforced_guard` |
| Heavy | Heavy Guard | 重型兵 | `enemy.heavy_guard` |
| Quipucamayoc | Quipucamayoc | 祭司 | `enemy.quipucamayoc` |
| Saqueos | Saqueos | 噬掠者 | `enemy.saqueos` |

## 4. Validation Evidence
1. `Assets/INTIFALL/Tests/EnemyTypeDataLocalizationTests.cs`
   - English primary display verification.
   - Language-specific resolver verification.
   - Localization-key coverage across all enemy types.
2. `Assets/INTIFALL/Tests/DataLayerLocalizationConsistencyTests.cs`
   - Replacement-character guard (`U+FFFD`) for ScriptableObject definitions.

## 5. Outcome
1. Iteration 7 known issue `KI-007-01` is closed.
2. Enemy naming data is now localization-ready and no longer tied to a single language default.
