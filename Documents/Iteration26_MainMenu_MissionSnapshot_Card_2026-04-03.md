## Iteration 26 - Main Menu Mission Snapshot Card (2026-04-03)

### Goal
- Persist the tool-window settlement telemetry into save snapshots.
- Surface latest mission outcome details on main menu for the active save slot.

### Runtime Changes
1. Save snapshot payload expanded in `SaveLoadManager.SaveData`:
   - `lastMissionToolRiskWindowAdjustment`
   - `lastMissionToolCooldownLoad`
   - `lastMissionRopeToolUses`
   - `lastMissionSmokeToolUses`
   - `lastMissionSoundBaitToolUses`
2. Mission snapshot write path updated:
   - `ApplyMissionSnapshot(...)` now copies the new mission fields from `MissionResult`.
3. Save normalize path updated:
   - new fields are clamped/sanitized during load/migration.
4. Main menu presentation updated:
   - `MainMenuUI` adds optional `activeSlotMissionSnapshotText` rich card output (4 lines).
   - if dedicated card text is not bound, active slot status gets a compact fallback line.

### Localization
- Added menu keys:
  - `menu.mission_snapshot.empty`
  - `menu.mission_snapshot.line1`
  - `menu.mission_snapshot.line2`
  - `menu.mission_snapshot.line3`
  - `menu.mission_snapshot.line4`
  - `menu.mission_snapshot.compact`

### Automated Coverage
1. `MainMenuSaveSlotTests`
   - card renders mission snapshot details.
   - empty state shown when snapshot missing.
   - compact fallback shown when dedicated card text is absent.
2. `SaveLoadManagerMigrationTests`
   - legacy/default normalization for new fields.
   - current-schema preservation for new fields.
3. `LocalizationServiceTests`
   - new mission snapshot keys are required.

### Verification
- EditMode: `370/370 PASS`
- PlayMode: `27/27 PASS`

### Status
- Mission snapshot persistence + main menu visibility is complete and regression-gated.
