# Iteration 8 Runtime Stability Audit

Date: 2026-04-01
Scope: Subscription/squad-registry stability (`I8-T4`)

## 1. Objective
Prevent hidden runtime accumulation issues across repeated scene loops, especially duplicate event callbacks and stale enemy-squad entries.

## 2. Updated Runtime Surfaces
1. `Assets/INTIFALL/Scripts/Runtime/System/EventBus.cs`
2. `Assets/INTIFALL/Scripts/Runtime/AI/EnemySquadCoordinator.cs`

## 3. Stability Guardrails
1. Event bus subscriptions are now idempotent per handler.
2. Unsubscribe removes empty event slots.
3. Test/diagnostic helpers added:
   - `EventBus.GetSubscriberCount<T>()`
   - `EventBus.ClearAllSubscribers()`
4. Enemy squad coordinator now purges invalid entries on register/unregister/broadcast paths.
5. Coordinator test helper added:
   - `EnemySquadCoordinator.ResetForTests()`

## 4. Validation Evidence
1. `Assets/INTIFALL/Tests/EventBusTests.cs`
   - Duplicate subscribe does not duplicate callback invocation.
2. `Assets/INTIFALL/Tests/EnemySquadCoordinatorTests.cs`
   - Invalid entry purge after object destruction.
3. `Assets/INTIFALL/Tests/PlayMode/Iteration8SceneStabilityPlayModeTests.cs`
   - Two-pass loop across Level01~05.
   - Verifies no manager accumulation and subscriber counts stay bounded.

## 5. Automated Scene Stability Result
PlayMode case:
1. `CoreScenes_TwoPassLoop_DoesNotAccumulateRuntimeManagersOrSubscribers` -> PASS.

## 6. Outcome
1. Repeated core-scene cycling remains stable in automated evidence.
2. Event and squad static-state accumulation risk reduced.
