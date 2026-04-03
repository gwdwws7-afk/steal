# Iteration31~35 Execution Report

Date: 2026-04-03  
Executor: Codex  
Project: `C:\test\Steal`  
Unity: `6000.2.14f1`

## Scope

This batch closed the previously planned I31~I35 items in one pass:

1. I31 Terminal interaction chain productization.
2. I32 AI linkage deepening after terminal and sound events.
3. I33 Tool + economy second-pass tuning (asset + scoring guardrails).
4. I34 Narrative content enrichment (terminal document catalog + scripted triggers).
5. I35 Full regression gate and progress documentation.

## Delivered Changes

### I31 - Terminal chain productization

1. `TerminalInteractable` now supports:
   1. start/progress/cancel/complete event chain.
   2. level-driven tuning overrides (`ApplyLevelTuning`).
   3. optional summary + scripted trigger fallback payload at configure-time.
2. `LevelLoader` now routes terminal intel with description/trigger payload into terminal runtime config.
3. `LevelData` terminal tuning fields are active in runtime wiring.
4. `HUDManager` terminal hack feedback now uses localized keys:
   1. `hud.terminal.hack_progress`
   2. `hud.terminal.hack_complete`
   3. `hud.terminal.hack_cancelled`

### I32 - AI linkage deepening

1. `EnemyStateMachine` now consumes `TerminalAlertSuppressedEvent` by radius/position and transitions alerted states into controlled `Searching` instead of hard reset.
2. `EnemyController.InvestigateSound(...)` now emits low-priority squad propagation so nearby squadmates react coherently.

### I33 - Tool/economy second-pass tuning

1. Settlement logic reinforcement in `GameManager.EvaluateToolRiskWindowAdjustment(...)`:
   1. diverse core-tool usage bonus.
   2. dominant single-tool ratio penalty.
   3. rope-heavy high-risk penalty.
2. Tool asset tuning updates:
   1. `ToolData_Rope`: cooldown `5`, maxAmmo `2`, energyCost `6`.
   2. `ToolData_SmokeBomb`: duration `8`.
   3. `ToolData_SoundBait`: cooldown `8`, maxAmmo `3`.
   4. `ToolData_EMP`: cooldown `22`, energyCost `16`.
   5. `ToolData_TimedNoise`: cooldown `15`, maxAmmo `4`.
3. Added baseline guard test in `ToolDataConfigurationTests` to prevent drift from I33 tuning envelope.

### I34 - Narrative content enrichment

1. Added terminal narrative catalog system:
   1. runtime loader: `TerminalDocumentCatalog`.
   2. resource data: `Assets/Resources/INTIFALL/Narrative/TerminalDocuments.json`.
2. `NarrativeManager.ReadTerminal(...)` now:
   1. emits `TerminalDocumentReadEvent` with title/summary/advancedTrigger.
   2. emits scripted narrative triggers (`ENarrativeEventType.ScriptedTrigger`) from catalog/fallback.
3. `IntelPickup` now accepts scripted trigger payload and emits scripted narrative events on collection.
4. `WillaComm` now parses scripted tokens and maps them to:
   1. warning
   2. story reveal
   3. betrayal
5. Catalog content coverage: all 5 levels terminal IDs included (20 entries total), with at least one advanced trigger terminal per level.

### I35 - Regression and closure evidence

1. Full EditMode gate:
   1. `384/384 PASS`
   2. result: `codex-i31-i35-final-editmode-results.xml`
   3. log: `codex-i31-i35-final-editmode.log`
2. Full PlayMode gate:
   1. `27/27 PASS`
   2. result: `codex-i31-i35-final-playmode-results.xml`
   3. log: `codex-i31-i35-final-playmode.log`

## New / Updated Guard Tests

1. `Assets/INTIFALL/Tests/TerminalDocumentCatalogTests.cs`
2. `Assets/INTIFALL/Tests/TerminalInteractableTests.cs`
3. `Assets/INTIFALL/Tests/WillaCommTests.cs`
4. `Assets/INTIFALL/Tests/ToolDataConfigurationTests.cs`
5. Existing updated suites retained:
   1. `EnemyStateMachineTests`
   2. `EnemySquadCoordinatorTests`
   3. `ToolRiskWindowScoringTests`

## Current Status

1. I31: DONE
2. I32: DONE
3. I33: DONE
4. I34: DONE
5. I35: DONE

Overall: PASS (automation green, content and trigger chain closed for this batch).
