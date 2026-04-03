# Iteration 11 Plan (MainMenu Guardrails + Localization Closure + RC5 Gate)

Updated: 2026-04-01
Owner: UI/Core/Systems/QA
Baseline: Iteration 10 RC4.5 automated gate PASS.

## 1. Iteration Goal
1. Add hard guardrails for MainMenu scene/prefab binding integrity.
2. Close remaining runtime localization gaps on lower-frequency UI/prompts.
3. Improve save-slot UX feedback and continue-fail behavior.
4. Run RC5 automated gate and produce acceptance documentation.

## 2. Task Board
1. `I11-T1` MainMenu Binding Guardrails
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Tests/MainMenuSceneBindingTests.cs`
2. `I11-T2` Runtime Localization Closure
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/UI/GameOverUI.cs`
     - `Assets/INTIFALL/Scripts/Runtime/UI/MissionBriefingUI.cs`
     - `Assets/INTIFALL/Scripts/Runtime/UI/EagleEyeUI.cs`
     - `Assets/INTIFALL/Scripts/Runtime/UI/HPHUD.cs`
     - `Assets/INTIFALL/Scripts/Runtime/UI/ToolHUD.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Economy/ArsenalUI.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Economy/SupplyPoint.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Environment/ElectronicDoor.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Environment/VentEntrance.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Environment/HangingPoint.cs`
     - `Assets/Resources/INTIFALL/Localization/LocalizationTable.json`
     - `Assets/INTIFALL/Tests/LocalizationServiceTests.cs`
3. `I11-T3` MainMenu Save Slot UX Polish
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/UI/MainMenuUI.cs`
     - `Assets/INTIFALL/Editor/MainMenuScenePrefabBinder.cs`
     - `Assets/INTIFALL/Tests/MainMenuSaveSlotTests.cs`
     - `Assets/Scenes/MainMenu.unity`
     - `Assets/INTIFALL/Prefabs/UI/MainMenuRoot.prefab`
4. `I11-T4` RC5 Automated Gate
   - Status: `DONE`
   - Output:
     - `codex-i11-editmode-results.xml`
     - `codex-i11-playmode-results.xml`
5. `I11-T5` Acceptance Documentation
   - Status: `DONE`
   - Output:
     - `Documents/Iteration11_Manual_Smoke_Run_Log_2026-04-01.md`
     - `Documents/Iteration11_RC5_Report_2026-04-01.md`

## 3. Exit Criteria
1. MainMenu scene has complete serialized binding coverage for save-slot widgets and references.
2. Continue no longer silently starts a new run when active save is corrupted/unloadable.
3. Runtime prompt text surfaces route through localization keys with table coverage.
4. EditMode + PlayMode suites are fully green.
