# Iteration 4 Plan (Vertical Slice Hardening)

Updated: 2026-04-01
Owner: Gameplay/Level/Narrative/QA
Baseline: Iteration 3 (`I3-T1` ~ `I3-T4`) completed and regression green.

## 1. Iteration Goal
Turn current placeholder-stable build into a production-facing vertical slice:
1. Replace placeholder-heavy scene/content links with source-of-truth assets where available.
2. Close remaining level validation gaps (reference integrity + mission path coverage).
3. Deliver a clear mission debrief and balancing pass for repeatable playtest quality.

## 2. Scope
### In Scope
1. Asset recovery completion (scenes/prefabs/scriptable object bindings).
2. Scene integrity auditing and automated validation expansion.
3. Mission debrief UX + runtime outcome presentation.
4. Gameplay tuning pass for stealth/combat/tool economy.
5. Iteration-level smoke and sign-off report.

### Out of Scope
1. Full final art replacement across all environments.
2. VO production and cinematic sequencing.
3. Platform publishing pipeline and store packaging.

## 3. Task Board
1. `I4-T1` Source Asset Replacement Pass
   - Status: `DONE`
   - Objective:
     - Replace placeholder scene/prefab/data assets with recovered source assets in priority order.
   - Planned Output:
     - `Documents/Asset_Recovery_Execution_Log_2026-04-01.md` update
     - `Assets/Scenes/Level01_Qhapaq_Passage.unity`
     - `Assets/Scenes/Level02_Temple_Complex.unity`
     - `Assets/Scenes/Level03_Underground_Labs.unity`
     - `Assets/Scenes/Level04_Qhipu_Core.unity`
     - `Assets/Scenes/Level05_General_Taki_Villa.unity`
2. `I4-T2` Scene Integrity Validator + Coverage Audit
   - Status: `DONE`
   - Objective:
     - Add editor/runtime validators for missing scripts, missing prefab refs, and critical component presence.
   - Planned Output:
     - `Assets/INTIFALL/Editor/*SceneIntegrity*.cs`
     - `Assets/INTIFALL/Tests/PlayMode/*SceneIntegrity*.cs`
     - `Documents/Iteration4_Scene_Integrity_Audit_2026-04-01.md`
3. `I4-T3` Mission Debrief Slice
   - Status: `DONE`
   - Objective:
     - Add post-mission summary UI showing rank/credits/intel/secondary outcome with narrative follow-up hook.
   - Planned Output:
     - `Assets/INTIFALL/Scripts/Runtime/UI/MissionDebriefUI.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Level/MissionExitPoint.cs` (integration)
     - `Assets/INTIFALL/Tests/*MissionDebrief*.cs`
4. `I4-T4` Gameplay Tuning Matrix Pass
   - Status: `DONE`
   - Objective:
     - Tune enemy perception timing, tool cooldown/ammo economy, and mission reward pacing for repeatable runs.
   - Planned Output:
     - `Assets/INTIFALL/Scripts/Runtime/AI/EnemyStateMachine.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Tools/ToolBase.cs`
     - `Assets/INTIFALL/Scripts/Runtime/Growth/LevelUpReward.cs`
     - `Documents/Iteration4_Tuning_Matrix_2026-04-01.md`
5. `I4-T5` Iteration 4 Final Smoke + Sign-off
   - Status: `DONE`
   - Objective:
     - Run full EditMode/PlayMode regression + 5-scene smoke and produce release-ready summary.
   - Planned Output:
     - `Documents/Iteration4_Final_Smoke_Report_2026-04-01.md`
     - `codex-i4-final-editmode-results.xml`
     - `codex-i4-final-playmode-results.xml`

## 4. Exit Criteria
1. Core level scenes (01~05) load with no missing script/component blockers.
2. Mission start -> intel collect -> exit -> debrief loop is verifiable in all 5 core scenes.
3. Narrative comm + mission outcome messaging remains valid after asset replacement.
4. EditMode + PlayMode regressions pass after each major task and at iteration close.
5. Iteration 4 final smoke report has no `BLOCKED` entries for core loop checks.

## 5. Recommended Execution Order
1. `I4-T1` (asset replacement baseline)
2. `I4-T2` (lock integrity guardrails)
3. `I4-T3` (player-facing debrief value)
4. `I4-T4` (tuning after systems stabilize)
5. `I4-T5` (final sign-off)
