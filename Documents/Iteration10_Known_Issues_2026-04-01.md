# Iteration 10 Known Issues

Date: 2026-04-01
Scope: RC4.5 gate

## Active Issues

| ID | Priority | Area | Description | Impact | Owner | Target |
|---|---|---|---|---|---|---|
| KI-010-01 | P2 | Manual QA | Full hand-driven mission smoke remains pending by workflow choice. | Final release sign-off still needs manual execution evidence. | QA | Next acceptance pass |

## Closed in Iteration 10
1. Save slot operations are now accessible via menu logic and automated tests.
2. Persistent manager root warning path is addressed and verified in PlayMode.
3. Recovery from corrupted primary save is now PlayMode gate-covered.
4. KI-010-03 closed on 2026-04-01: `Assets/Scenes/MainMenu.unity` and `Assets/INTIFALL/Prefabs/UI/MainMenuRoot.prefab` now bind all save slot buttons/text fields to the new `MainMenuUI` serialized fields.
5. KI-010-02 closed on 2026-04-01: Runtime UI/environment prompt literals were migrated to localization keys (menu/tool/gameover/briefing/supply/door/vent/hang/intel/HUD surfaces), with key-presence tests added.

## Gate Status
RC4.5 automated gate remains `PASS`.
