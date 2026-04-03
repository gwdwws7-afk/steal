using System;
using System.Collections.Generic;
using UnityEngine;

namespace INTIFALL.System
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _eventTable = new();
        private static readonly object _lock = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
                return;

            lock (_lock)
            {
                Type eventType = typeof(T);
                if (_eventTable.TryGetValue(eventType, out Delegate existing))
                {
                    // Keep subscriptions idempotent per handler to avoid duplicate callbacks after repeated OnEnable cycles.
                    Delegate merged = Delegate.Combine(Delegate.Remove(existing, handler), handler);
                    _eventTable[eventType] = merged;
                }
                else
                {
                    _eventTable[eventType] = handler;
                }
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null)
                return;

            lock (_lock)
            {
                Type eventType = typeof(T);
                if (_eventTable.TryGetValue(eventType, out Delegate existing))
                {
                    Delegate next = Delegate.Remove(existing, handler);
                    if (next == null)
                        _eventTable.Remove(eventType);
                    else
                        _eventTable[eventType] = next;
                }
            }
        }

        public static void Publish<T>(T eventData)
        {
            Type eventType = typeof(T);
            Delegate handlers;
            lock (_lock)
            {
                if (!_eventTable.TryGetValue(eventType, out handlers))
                    return;
            }
            (handlers as Action<T>)?.Invoke(eventData);
        }

        public static int GetSubscriberCount<T>()
        {
            lock (_lock)
            {
                if (_eventTable.TryGetValue(typeof(T), out Delegate handlers))
                    return handlers?.GetInvocationList().Length ?? 0;

                return 0;
            }
        }

        public static void ClearAllSubscribers()
        {
            lock (_lock)
            {
                _eventTable.Clear();
            }
        }
    }

    public struct PlayerMovedEvent
    {
        public Vector3 position;
        public Vector3 velocity;
        public bool isSprinting;
        public bool isCrouching;
    }

    public struct PlayerEnteredCoverEvent
    {
        public Vector3 coverPosition;
    }

    public struct AlertStateChangedEvent
    {
        public int enemyId;
        public EAlertState newState;
    }

    public enum EAlertState
    {
        Unaware,
        Suspicious,
        Searching,
        Alert,
        FullAlert
    }
}
