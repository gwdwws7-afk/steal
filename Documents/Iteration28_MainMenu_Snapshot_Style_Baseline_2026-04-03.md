## Iteration 28 - MainMenu Snapshot Style Baseline (2026-04-03)

### Goal
- Improve `MissionSnapshotText` readability in main menu.
- Add a durable automated style baseline gate to prevent accidental UI drift.

### Changes
1. `MainMenuRoot.prefab`
   - `MissionSnapshotText` layout tuned:
     - anchored Y: `236`
     - size: `980 x 136`
   - text style tuned:
     - `fontSize = 19`
     - `alignment = UpperLeft`
     - `lineSpacing = 1.15`
     - `raycastTarget = false`
2. `MainMenuSceneBindingTests`
   - new test: `MainMenuScene_MissionSnapshotText_UsesReadableBaselineStyle`
   - validates binding + style baseline (layout, alignment, wrapping, line spacing, raycast).

### Verification
- EditMode: `371/371 PASS`
- PlayMode: `27/27 PASS`

### Status
- Main menu snapshot card now has explicit visual baseline guard in automated tests.
