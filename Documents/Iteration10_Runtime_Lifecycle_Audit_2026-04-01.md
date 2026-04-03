# Iteration 10 Runtime Lifecycle Audit

Date: 2026-04-01
Scope: `I10-T3`

## 1. Objective
Remove `DontDestroyOnLoad only works for root GameObjects` warning from persistent manager lifecycle.

## 2. Runtime Changes
Updated:
1. `Assets/INTIFALL/Scripts/Runtime/System/GameManager.cs`
2. `Assets/INTIFALL/Scripts/Runtime/Audio/AudioManager.cs`

Both managers now detach from parent (`transform.SetParent(null)`) before `DontDestroyOnLoad(...)`.

## 3. Validation
1. PlayMode regression output no longer includes the known root warning string.
2. Dedicated PlayMode gate added in `Iteration10PersistenceAndRecoveryPlayModeTests` for root-detach behavior.

## 4. Outcome
Persistent manager lifecycle is now root-safe and warning noise is reduced in CI evidence.
