# Iteration 5 Plan (Gameplay Depth + RC0 Gate)

Updated: 2026-04-01
Owner: Gameplay/AI/Systems/Narrative/QA
Baseline: Iteration 4 completed (`I4-T1` ~ `I4-T5`).

## 1. Iteration Goal
1. Raise level playability quality from whitebox-ready to behavior-complete stealth loop.
2. Increase AI readability and counterplay under detection pressure.
3. Complete narrative chain per level and lock a release-candidate gate (`RC0`).

## 2. Task Board
1. `I5-T1` Level Playability / Whitebox Closure
   - Status: `DONE`
   - Evidence: `Documents/Iteration5_Whitebox_Rebuild_Report_2026-04-01.md`
2. `I5-T2` AI Behavior Depth
   - Status: `DONE`
   - Deliverables:
     - `Assets/INTIFALL/Scripts/Runtime/AI/EnemySquadCoordinator.cs`
     - `Assets/INTIFALL/Scripts/Runtime/AI/EnemyController.cs`
     - `Assets/INTIFALL/Scripts/Runtime/AI/EnemyStateMachine.cs`
3. `I5-T3` Tool + Economy Rebalance (Pass 2)
   - Status: `DONE`
   - Deliverables:
     - `Assets/INTIFALL/Scripts/Runtime/Tools/*.cs` (cooldown/ammo/range rebalance)
     - `Assets/INTIFALL/Scripts/Runtime/Economy/CreditSystem.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Growth/LevelUpReward.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Economy/SupplyPoint.cs`
4. `I5-T4` Narrative Content Fill
   - Status: `DONE`
   - Deliverables:
     - `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaMessageCatalog.cs`
     - `Assets/Resources/INTIFALL/Narrative/WillaMessages.json`
     - `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaComm.cs`
5. `I5-T5` RC0 Gate
   - Status: `DONE`
   - Deliverables:
     - `codex-i5-rc0-editmode-results.xml`
     - `codex-i5-rc0-playmode-results.xml`
     - `Documents/Iteration5_RC0_Final_Report_2026-04-01.md`
     - `Documents/Iteration5_Manual_Smoke_Run_Log_2026-04-01.md`
     - `Documents/Iteration5_Known_Issues_2026-04-01.md`

## 3. Exit Criteria
1. EditMode + PlayMode regression all green.
2. 5 core scenes smoke matrix all PASS.
3. Narrative chain has MissionStart/IntelFound/MissionComplete for Level01~05.
4. No P0/P1 blockers in known issues list.
