# Iteration 3 Plan (Narrative Loop)

Updated: 2026-04-01
Owner: Narrative/UI/Systems
Baseline: Iteration 2 scene smoke and mission-loop automation are green.

## 1. Iteration Goal
Raise narrative system from framework-only to playable mission feedback loop:
1. Mission start, intel pickup, and mission completion can automatically trigger comms.
2. Runtime comms do not silently drop when multiple events fire in short windows.
3. Narrative loop remains regression-safe under current EditMode/PlayMode suite.

## 2. Scope
### In Scope
1. Event-driven Willa comm auto triggers.
2. Comm request queueing while a message is on screen.
3. Test coverage for new Willa comm runtime behavior.

### Out of Scope
1. Full narrative text rewrite/localization pass.
2. Cinematic sequencing and voice-over content production.
3. Final mission debrief UI art polish.

## 3. Task Board
1. `I3-T1` Willa comm event bridge and queue reliability
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaComm.cs`
     - `Assets/INTIFALL/Tests/WillaCommTests.cs`
     - `codex-i3t1-editmode-results.xml`
     - `codex-i3t1-playmode-results.xml`
2. `I3-T2` Narrative content data cleanup (message encoding/authoring pipeline)
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaMessageCatalog.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaComm.cs`
     - `Assets/Resources/INTIFALL/Narrative/WillaMessages.json`
     - `Assets/INTIFALL/Tests/WillaMessageCatalogTests.cs`
     - `codex-i3t2-editmode-results.xml`
     - `codex-i3t2-playmode-results.xml`
3. `I3-T3` Mission outcome comm enrichment (rank/intel deltas in messaging)
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaComm.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Narrative/WillaMessageCatalog.cs`
     - `Assets/Resources/INTIFALL/Narrative/WillaMessages.json`
     - `Assets/INTIFALL/Tests/WillaCommTests.cs`
     - `codex-i3t3-editmode-results.xml`
     - `codex-i3t3-playmode-results.xml`
4. `I3-T4` Scene narrative trigger audit (level-by-level content coverage)
   - Status: `DONE`
   - Output:
     - `Assets/INTIFALL/Scripts/Runtime/Level/LevelLoader.cs`
     - `Assets/INTIFALL/Tests/PlayMode/Iteration3SceneNarrativeAuditPlayModeTests.cs`
     - `Documents/Iteration3_Narrative_Trigger_Audit_2026-04-01.md`
     - `codex-i3t4-playmode-results.xml`
     - `codex-i3t4-editmode-results.xml`

## 4. Exit Criteria
1. Willa comm can auto-react to level load, first intel pickup, and mission outcome.
2. Concurrent trigger bursts preserve message order via queueing.
3. EditMode + PlayMode regressions pass after narrative changes.
