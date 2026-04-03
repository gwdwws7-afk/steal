# Iteration 1 Manual Smoke Checklist

Updated: 2026-03-31
Project root: `C:\test\Steal`
Unity version: `6000.2.14f1`

## 0. Purpose
Validate the Iteration 1 playable loop in graybox scenes:
1. Infiltrate.
2. Collect intel.
3. Exit mission.

This checklist is for manual execution in Unity Editor Play Mode.

## 1. Preconditions
1. Project opens without compile errors.
2. Input handler is compatible:
   - `ProjectSettings/ProjectSettings.asset` has `activeInputHandler: 2` (Both).
3. Scene bootstrap already applied:
   - `INTIFALL_Runtime` exists in each level scene.
4. Quick automated sanity is green before manual run:
```powershell
.\run-regression-tests.ps1 -ProjectPath "C:\test\Steal" -UnityExe "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -TestPlatform EditMode
.\run-regression-tests.ps1 -ProjectPath "C:\test\Steal" -UnityExe "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -TestPlatform PlayMode
```

## 2. Scenes In Scope
Run the same smoke sequence in this order:
1. `Assets/Scenes/Level01_Qhapaq_Passage.unity`
2. `Assets/Scenes/Level02_Temple_Complex.unity`
3. `Assets/Scenes/Level03_Underground_Labs.unity`
4. `Assets/Scenes/Level04_Qhipu_Core.unity`
5. `Assets/Scenes/Level05_General_Taki_Villa.unity`

## 3. Smoke Cases (Per Scene)
Use IDs below in your execution log.

### SMK-001 Scene Boot
Steps:
1. Open scene.
2. Press Play.
Expected:
1. No red errors in Console.
2. A player object is present and controllable.
3. Enemies, intel pickups, supply point, and exit point are visible/spawned.

### SMK-002 Core Movement
Steps:
1. Move with `W/A/S/D`.
2. Hold `LeftShift` to sprint.
3. Hold `C` to crouch.
4. Press `Space` to roll.
Expected:
1. Character moves in all directions.
2. Sprint state changes speed.
3. Crouch state changes movement behavior.
4. Roll triggers once per key press.

### SMK-003 Enemy Perception
Steps:
1. Enter enemy vision range in front cone.
2. Break line of sight and move away.
Expected:
1. Enemy transitions to detection/alert behavior.
2. Enemy can lose player and return/search based on state machine logic.

### SMK-004 Intel Collection
Steps:
1. Collect one intel pickup.
2. Collect remaining intel pickups.
Expected:
1. Pickup disappears after collection.
2. Intel collection events are emitted (no errors).
3. All scene intel can be collected.

### SMK-005 Exit Gating
Steps:
1. Before full intel collection, enter exit trigger.
2. After collecting all intel, enter exit trigger again.
Expected:
1. Exit does not complete mission before requirement is met.
2. Exit completes mission when requirement is met.
3. Level flow proceeds to next level (or configured fallback) without errors.

### SMK-006 Supply Point
Steps:
1. Interact with or trigger supply point behavior.
2. Retry during cooldown window.
Expected:
1. Supply point applies configured effect.
2. Cooldown behavior is enforced (no repeated free trigger in cooldown window).

### SMK-007 Pause And HUD
Steps:
1. Press `Esc` to pause/resume.
2. Press `H` to toggle HUD.
Expected:
1. Pause menu/game pause state toggles correctly.
2. HUD visibility toggles without errors.

### SMK-008 Tool Hotkeys
Steps:
1. Press `1/2/3/4` to switch tool slot.
2. Press `Q` to use active tool.
Expected:
1. Active slot changes correctly when slot has tool.
2. Tool use path executes without exceptions.

### SMK-009 Input Compatibility Guard
Steps:
1. During all above steps, watch Console.
Expected:
1. No `InvalidOperationException` about `UnityEngine.Input` and Input System mismatch.

## 4. Exit Criteria
Iteration 1 manual smoke is PASS only if:
1. All `SMK-001` to `SMK-009` pass in all 5 scenes.
2. No blocking console errors remain unresolved.
3. Any non-blocking warnings are logged with owner and follow-up date.

## 5. Failure Triage Rules
1. P0:
   - Crash, hard lock, cannot enter Play Mode, cannot complete mission loop.
2. P1:
   - Core loop broken but workaround exists (for example exit gating incorrect).
3. P2:
   - UX/feedback issue that does not block loop completion.

For each failure capture:
1. Scene path.
2. Smoke case ID.
3. Repro steps (numbered).
4. Console snippet.
5. Screenshot or short clip path.

## 6. Recommended Run Cadence
1. Mandatory before merging gameplay or input changes.
2. Mandatory after scene/spawn data batch edits.
3. At least once daily during Iteration 1 stabilization.
