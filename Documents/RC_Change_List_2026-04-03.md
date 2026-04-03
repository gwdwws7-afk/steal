# RC Change List (2026-04-03)

Project: `C:\test\Steal`

## 1. Runtime Systems

1. Terminal chain and interaction events:
   1. `Assets/INTIFALL/Scripts/Runtime/Environment/TerminalInteractable.cs`
   2. `Assets/INTIFALL/Scripts/Runtime/Level/LevelLoader.cs`
   3. `Assets/INTIFALL/Scripts/Runtime/UI/HUDManager.cs`
2. AI response and squad linkage:
   1. `Assets/INTIFALL/Scripts/Runtime/AI/EnemyStateMachine.cs`
   2. `Assets/INTIFALL/Scripts/Runtime/AI/EnemyController.cs`
3. Narrative closure:
   1. `Assets/INTIFALL/Scripts/Runtime/Narrative/NarrativeManager.cs`
   2. `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaComm.cs`
   3. `Assets/INTIFALL/Scripts/Runtime/Narrative/IntelPickup.cs`
   4. `Assets/INTIFALL/Scripts/Runtime/Narrative/TerminalDocumentCatalog.cs`
4. Economy and tool-window scoring:
   1. `Assets/INTIFALL/Scripts/Runtime/System/GameManager.cs`
5. Input/interaction wiring:
   1. `Assets/INTIFALL/Scripts/Runtime/Environment/ElectronicDoor.cs`

## 2. Data / Assets

1. Level and spawn tuning:
   1. `Assets/INTIFALL/ScriptableObjects/LevelData.cs`
   2. `Assets/INTIFALL/ScriptableObjects/Levels/LevelData_Level01_Qhapaq_Passage.asset`
   3. `Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_Level01_Qhapaq_Passage.asset`
   4. `Assets/Resources/INTIFALL/Levels/LevelData_Level01_Qhapaq_Passage.asset`
   5. `Assets/Resources/INTIFALL/Spawns/IntelSpawn_Level01_Qhapaq_Passage.asset`
2. Tool tuning second pass:
   1. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_Rope.asset`
   2. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_SmokeBomb.asset`
   3. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_SoundBait.asset`
   4. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_EMP.asset`
   5. `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_TimedNoise.asset`
3. Narrative catalog and localization:
   1. `Assets/Resources/INTIFALL/Narrative/TerminalDocuments.json`
   2. `Assets/Resources/INTIFALL/Localization/LocalizationTable.json`

## 3. Automated Tests

1. New:
   1. `Assets/INTIFALL/Tests/TerminalInteractableTests.cs`
   2. `Assets/INTIFALL/Tests/TerminalDocumentCatalogTests.cs`
2. Updated:
   1. `Assets/INTIFALL/Tests/EnemyStateMachineTests.cs`
   2. `Assets/INTIFALL/Tests/EnemySquadCoordinatorTests.cs`
   3. `Assets/INTIFALL/Tests/EnvironmentTests.cs`
   4. `Assets/INTIFALL/Tests/PlayMode/Iteration2SceneMissionFlowPlayModeTests.cs`
   5. `Assets/INTIFALL/Tests/ToolRiskWindowScoringTests.cs`
   6. `Assets/INTIFALL/Tests/ToolDataConfigurationTests.cs`
   7. `Assets/INTIFALL/Tests/WillaCommTests.cs`

## 4. Design / Reports

1. `Assets/GDD_INTI_FALL.md`
2. `Documents/Iteration30_System_Design_Reaudit_2026-04-03.md`
3. `Documents/Iteration31_to_35_Execution_Report_2026-04-03.md`
