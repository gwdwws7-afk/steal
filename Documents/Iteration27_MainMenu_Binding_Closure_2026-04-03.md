## Iteration 27 - MainMenu Snapshot Binding Closure (2026-04-03)

### Goal
- Close the remaining UI binding gap for `activeSlotMissionSnapshotText`.
- Prevent regression by promoting this binding to a scene-level required field check.

### Changes
1. `MainMenuRoot.prefab`
   - Added `MissionSnapshotText` UI text node under `MainPanel`.
   - Bound `MainMenuUI.activeSlotMissionSnapshotText` to the new text component.
2. `MainMenuSceneBindingTests`
   - Added strict assertion for `activeSlotMissionSnapshotText` binding.

### Verification
- EditMode: `370/370 PASS`
- PlayMode: `27/27 PASS`

### Status
- KI-010-03 style unbound snapshot-text risk is now closed in prefab + scene test gate.
