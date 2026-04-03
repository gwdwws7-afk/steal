# Iteration 8 Plan (Data Localization + Save Migration + RC3)

Updated: 2026-04-01
Owner: Systems/AI/Economy/Core/QA
Baseline: Iteration 7 completed (`I7-T1` ~ `I7-T6`, RC2 PASS).

## 1. Iteration Goal
1. Close data-layer localization gaps that remained after runtime localization passes.
2. Rebalance extraction reward model so optional routes are high-reward only under controlled pressure.
3. Add save schema migration to keep old saves compatible with newer mission-result fields.
4. Harden runtime stability (event subscription hygiene and enemy squad registry cleanup).
5. Lock RC3 gate with automated regression evidence, while skipping manual smoke per request.

## 2. Task Board
1. `I8-T1` Data-Layer Localization Closure
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/ScriptableObjects/EnemyTypeData.cs`
     - `Assets/INTIFALL/Tests/EnemyTypeDataLocalizationTests.cs`
     - `Assets/INTIFALL/Tests/DataLayerLocalizationConsistencyTests.cs`
2. `I8-T2` Economy Pressure Rebalance
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/System/GameManager.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Economy/CreditSystem.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Growth/LevelUpReward.cs`
     - `Assets/INTIFALL/Tests/MissionRouteScoringTests.cs`
     - `Assets/INTIFALL/Tests/CreditSystemTests.cs`
3. `I8-T3` Save Compatibility and Migration
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/Core/SaveLoadManager.cs`
     - `Assets/INTIFALL/Tests/SaveLoadManagerMigrationTests.cs`
4. `I8-T4` Runtime Stability Guardrails
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/System/EventBus.cs`
     - `Assets/INTIFALL/Scripts/Runtime/AI/EnemySquadCoordinator.cs`
     - `Assets/INTIFALL/Tests/EventBusTests.cs`
     - `Assets/INTIFALL/Tests/EnemySquadCoordinatorTests.cs`
     - `Assets/INTIFALL/Tests/PlayMode/Iteration8SceneStabilityPlayModeTests.cs`
5. `I8-T5` RC3 Automated Gate
   - Status: `DONE`
   - Output:
     - `codex-i8-final-editmode-results.xml`
     - `codex-i8-final-playmode-results.xml`
     - `Documents/Iteration8_RC3_Final_Report_2026-04-01.md`

## 3. Exit Criteria
1. EditMode + PlayMode all green.
2. Enemy type data has explicit English/Chinese labels and localization keys.
3. High-pressure optional extraction no longer dominates clean main-route rewards.
4. Legacy save payload can migrate into current schema without load failure.
5. Two-pass core-scene loop does not accumulate manager instances or subscriber counts.
