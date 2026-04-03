# RC Change List

Date: 2026-04-02  
Scope: `RC-T1` candidate freeze input

## 1. Included Functional Changes
1. I14 core conflict closure: FullAlert migration fix + strict runtime placeholder gate + mission settlement unification.
2. I15 AI depth closure: patrol/search timing profile + squad communication broadcast + deterministic search sectoring.
3. I16 tool/economy closure: cooldown/ammo/range retune + route reward multipliers + S/A/B/C gradient reinforcement.
4. I17 growth/narrative closure: progression/save compatibility + five-level narrative trigger chain + localization consistency hardening.
5. I18 level playability closure: level flow profile/encounter coverage assets for Level01~05.
6. I19 stability closure: event subscription leak guard and automated stability gate script.
7. I20 release closure: RC freeze rerun and final sign-off report refresh.
8. I21 tool parity closure: wheel/cancel input parity + slot-cost loadout enforcement + ToolHUD capacity/reject feedback.
9. I21 tuning closure: heavy-tool cadence rebalance under multi-slot constraints.
10. I21 gate refresh: stability gate + RC final gate rerun after P2 closure.
11. I22 manual-smoke kickoff: dedicated checklist + run-log scaffold with I22-prefixed regression evidence.
12. I22 release package closure: RC final gate rerun (`codex-i22-rcfinal`) + release notes + package checklist + closure report.
13. I22-T3 post-package stability closure: stability gate rerun (`codex-i22-t3-stability`) + dedicated closure report.
14. I22-T4 release freeze/handoff closure: immutable artifact manifest (`SHA256`) + final handoff report for downstream iteration continuity.

## 2. Added/Updated Test Gates
1. `EnemyControllerTuningTests`
2. `EnemyStateMachineTests`
3. `EnemySquadCoordinatorTests`
4. `MissionRewardBandingTests`
5. `MissionRouteScoringTests`
6. `MissionExitPointTests`
7. `SecondaryObjectiveTrackerTests`
8. `Iteration19_StabilityGateTests`
9. `SpawnCoverageTests`
10. `LevelEncounterCoverageTests`
11. `LevelDataFlowProfileTests`
12. `SaveLoadManagerMigrationTests`
13. `SaveLoadManagerReliabilityTests`
14. `NarrativeManagerTests`
15. `WillaCommTests`
16. `ToolHUDTests`
17. `ToolManagerTests` (expanded with slot-cost + rejection event coverage)
18. `ToolDataConfigurationTests` (slot-cost completeness and 2-slot presence checks)
19. `InputCompatTests` (`GetKeyUp` safe fallback coverage)
20. I22 one-click RC final rerun (`codex-i22-rcfinal-editmode-results.xml`, `codex-i22-rcfinal-playmode-results.xml`)
21. I22-T3 stability rerun (`codex-i22-t3-stability-editmode-results.xml`, `codex-i22-t3-stability-playmode-results.xml`)

## 3. RC Freeze Rule
From this point, only blocker fixes are allowed before release sign-off:
1. P0/P1 correctness blockers.
2. Regression blockers found by RC automated gate.
3. Build-breaking blockers.
