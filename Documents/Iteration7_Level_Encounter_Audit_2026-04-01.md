# Iteration 7 Level Encounter Audit

Date: 2026-04-01
Scope: Level playability profile binding (`I7-T3`)

## 1. Objective
Bind each level's designed completion time to explicit phase timing and encounter budget allocation.

## 2. Level Profiles

| Level | Designed Minutes | Infiltration | Objective | Extraction | Enemy Budget (I/O/E) | Total |
|---|---:|---:|---:|---:|---:|---:|
| L01 Qhapaq Passage | 12 | 4 | 5 | 3 | 2 / 3 / 1 | 6 |
| L02 Temple Complex | 15 | 5 | 6 | 4 | 2 / 3 / 2 | 7 |
| L03 Underground Labs | 18 | 6 | 7 | 5 | 2 / 4 / 2 | 8 |
| L04 Qhipu Core | 20 | 6 | 8 | 6 | 3 / 5 / 3 | 11 |
| L05 General Taki Villa | 22 | 7 | 9 | 6 | 3 / 5 / 3 | 11 |

## 3. Validation Rules
1. `infiltrationMinutes + objectiveMinutes + extractionMinutes == designedCompletionMinutes`.
2. `infiltrationEnemyBudget + objectiveEnemyBudget + extractionEnemyBudget == totalEnemyCount`.
3. Patrol route count in enemy spawn assets is not below planned route requirements.

## 4. Evidence
1. Data model and assets:
   - `Assets/INTIFALL/ScriptableObjects/LevelData.cs`
   - `Assets/Resources/INTIFALL/Levels/LevelData_Level*.asset`
2. Validation tests:
   - `Assets/INTIFALL/Tests/LevelDataFlowProfileTests.cs`
   - `Assets/INTIFALL/Tests/LevelEncounterCoverageTests.cs`

## 5. Outcome
Design time and encounter density are now explicitly represented per level and validated in EditMode regression.
