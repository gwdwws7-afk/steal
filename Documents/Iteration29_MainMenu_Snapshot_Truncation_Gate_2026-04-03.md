## Iteration 29 - MainMenu Snapshot Truncation Gate (2026-04-03)

### Goal
- Prevent oversized mission snapshot text from breaking main menu readability.
- Enforce deterministic truncation behavior with automated tests.

### Runtime Changes
1. `MainMenuUI`
   - added snapshot truncation policy constants:
     - `MissionSnapshotMaxLines = 4`
     - `MissionSnapshotLineMaxCharacters = 96`
     - `MissionSnapshotCompactMaxCharacters = 48`
   - added helpers:
     - `BuildMissionSnapshotBlock(...)`
     - `EllipsizeSingleLine(...)`
   - applied truncation to:
     - dedicated snapshot card (`activeSlotMissionSnapshotText`)
     - compact fallback line in slot status text

### Automated Coverage
1. `MainMenuSaveSlotTests`
   - new test:
     - `RefreshMenuState_WithLongRouteLabel_ClampsSnapshotToFourEllipsizedLines`
   - validates:
     - output remains 4 lines
     - long route line ends with `...`
     - each line respects 96-char budget

### Verification
- EditMode: `372/372 PASS`
- PlayMode: `27/27 PASS`

### Status
- Snapshot long-text overflow risk is now closed with runtime guard + regression test.
