# Iteration20 P2 Closure Report

Date: 2026-04-02  
Executor: Codex  
Unity: 6000.2.14f1

## Scope
Close `KI-020-04` (`P2`, Tool Design Parity):
1. Tool switching parity (`1-4 + mouse wheel`).
2. Cancel path parity (`ESC` and right-button hold-release cancel).
3. Loadout slot-cost model (`4` total capacity with multi-slot tools).

## Implemented Changes
1. Input compatibility:
   1. Added `InputCompat.GetKeyUp(KeyCode)` for release-driven interactions.
2. Tool manager behavior:
   1. Added wheel-based tool cycling (`SelectNextTool` / `SelectPreviousTool`).
   2. Added cancel action (`CancelActiveToolSelection`) and hold-release cancel timing.
   3. Added slot-cost accounting (`equippedToolSlotCosts`, total/remaining capacity helpers).
   4. Added equip guard (`CanEquipTool`) to block over-capacity loadouts.
3. Tool data model:
   1. Added `ToolData.slotCost` (`Min(1)`, default `1`).
4. Runtime tool assets:
   1. Added `slotCost` to all runtime tool assets.
   2. Marked multi-slot heavy tools (`Drone`, `DroneInterference`, `EMP`, `Rope`, `WallBreak`, `WallBreaker`) as `2-slot`.

## Files
1. `Assets/INTIFALL/Scripts/Runtime/Input/InputCompat.cs`
2. `Assets/INTIFALL/Scripts/Runtime/Tools/ToolManager.cs`
3. `Assets/INTIFALL/ScriptableObjects/ToolData.cs`
4. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_Drone.asset`
5. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_DroneInterference.asset`
6. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_EMP.asset`
7. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_FlashBang.asset`
8. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_Rope.asset`
9. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_SleepDart.asset`
10. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_SmokeBomb.asset`
11. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_SoundBait.asset`
12. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_TimedNoise.asset`
13. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_WallBreak.asset`
14. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_WallBreaker.asset`
15. `Assets/INTIFALL/Tests/InputCompatTests.cs`
16. `Assets/INTIFALL/Tests/ToolManagerTests.cs`
17. `Assets/INTIFALL/Tests/ToolDataConfigurationTests.cs`
18. `Documents/RC_Known_Issues_2026-04-02.md`
19. `Documents/Iteration14_to_20_Completion_Report_2026-04-02.md`

## Verification
1. EditMode:
   1. Initial run: `346/347 PASS`, `1` test fix-up required (`ToolManagerTests` expectation issue).
   2. Rerun: `347/347 PASS` (`codex-p2-editmode-rerun-2026-04-02.xml`).
2. PlayMode:
   1. `21/21 PASS` (`codex-p2-playmode-results-2026-04-02.xml`).

## Closure Decision
1. `KI-020-04`: `CLOSED`
2. Active blocker count snapshot:
   1. `P0`: `0`
   2. `P1`: `0`
   3. `P2`: `0`
