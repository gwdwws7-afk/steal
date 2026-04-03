# Iteration 10 Plan (Save Slot UX + Localization Rollout + Gate Hardening)

Updated: 2026-04-01
Owner: UI/Core/Systems/QA
Baseline: Iteration 9 completed (automated RC4 readiness PASS, manual pending).

## 1. Iteration Goal
1. Complete save-slot UX closure in main menu (slot select/load/delete/backup restore).
2. Expand localization from infrastructure to high-frequency runtime surfaces.
3. Remove known lifecycle warning from persistent managers.
4. Add PlayMode gate for persistence-root and backup recovery behavior.

## 2. Task Board
1. `I10-T1` Save Slot UI Closure
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/UI/MainMenuUI.cs`
     - `Assets/INTIFALL/Tests/MainMenuSaveSlotTests.cs`
2. `I10-T2` Localization Rollout (Runtime Surfaces)
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/UI/HUDManager.cs`
     - `Assets/INTIFALL/Scripts/Runtime/UI/MissionDebriefUI.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaComm.cs`
     - `Assets/Resources/INTIFALL/Localization/LocalizationTable.json`
3. `I10-T3` Lifecycle Warning Cleanup
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/System/GameManager.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Audio/AudioManager.cs`
4. `I10-T4` Automated Gate Enhancements
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Tests/PlayMode/Iteration10PersistenceAndRecoveryPlayModeTests.cs`
     - `codex-i10c-editmode-results.xml`
     - `codex-i10c-playmode-results.xml`
5. `I10-T5` RC4.5 Packaging Report
   - Status: `DONE`
   - Output:
     - `Documents/Iteration10_RC45_Report_2026-04-01.md`

## 3. Exit Criteria
1. Save slot behavior is operable through UI hooks.
2. Localization keys drive runtime text on selected core surfaces.
3. `DontDestroyOnLoad` root warning is eliminated in automated evidence.
4. EditMode + PlayMode gates remain fully green after rollout.
