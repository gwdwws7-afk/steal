# Iteration 10 Localization Rollout Audit

Date: 2026-04-01
Scope: `I10-T2`

## 1. Objective
Move selected runtime text surfaces from hardcoded literals to key-based localization backed by `LocalizationService`.

## 2. Runtime Changes
Updated:
1. `Assets/INTIFALL/Scripts/Runtime/UI/HUDManager.cs`
2. `Assets/INTIFALL/Scripts/Runtime/UI/MissionDebriefUI.cs`
3. `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaComm.cs`
4. `Assets/INTIFALL/Scripts/Runtime/UI/MainMenuUI.cs`

Updated table:
1. `Assets/Resources/INTIFALL/Localization/LocalizationTable.json`

## 3. Key Additions (Representative)
1. HUD:
   - `hud.primary.collect_extract`
   - `hud.secondary.undetected`
   - `hud.primary.complete`
2. Debrief:
   - `debrief.title`
   - `debrief.line.*`
   - `debrief.stealth.*`
3. Menu:
   - `menu.new_game`
   - `menu.active_slot`
   - `menu.slot.*`
   - `menu.level.locked`
4. Narrative comm:
   - `willa.speaker.secure`
   - `willa.damage.*`

## 4. Validation
Used:
1. Existing `LocalizationServiceTests` + runtime regression suites.
2. Existing narrative/debrief tests with forced English language override to keep deterministic assertions.

## 5. Compatibility Note
Runtime scripts remain free of direct CJK literals to preserve `LocalizationConsistencyTests` gate; translated strings are table-driven.

## 6. Outcome
Localization rollout now covers menu/debrief/HUD/willa core loop paths while preserving existing regression constraints.
