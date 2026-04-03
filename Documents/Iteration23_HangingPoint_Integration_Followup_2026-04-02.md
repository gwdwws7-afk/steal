# Iteration23 HangingPoint Integration Follow-up

Date: 2026-04-02  
Executor: Codex  
Unity: 6000.2.14f1  
Project: `C:\test\Steal`

## 1. Purpose

Follow-up hardening after rope runtime closure to remove dual-control movement risk between `HangingPoint` and `PlayerController`.

## 2. Changes

1. `HangingPoint` integration alignment:
   1. Removed direct per-frame player translation from `UpdatePlayerHanging` (movement authority now fully in `PlayerController`).
   2. Added guard in `Update` to release occupied state if player/controller is missing.
   3. Added guard in `Update` to release occupied state when player has already detached rope externally (timeout/manual detach).
   4. Updated `Detach` to support `detachFromController` flag for clean release without duplicate detach calls.

2. Test coverage:
   1. Added `EnvironmentTests.HangingPoint_PlayerDetachedExternally_ReleasesOccupiedState` to prevent occupancy lock regressions.

## 3. Verification

1. EditMode full regression:
   1. Result: `364/364 PASS`
   2. Evidence: `codex-i23-next-full-editmode-results.xml`

2. PlayMode full regression:
   1. Result: `24/24 PASS`
   2. Evidence: `codex-i23-next-full-playmode-results.xml`

## 4. Outcome

1. Rope movement authority is now single-source (`PlayerController`) during hanging.
2. Hanging point occupancy no longer risks stale lock after external detach.
3. No regression introduced in full automated suite.
