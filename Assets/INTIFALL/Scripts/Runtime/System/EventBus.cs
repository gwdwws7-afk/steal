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
            lock (_lock)
            {
                Type eventType = typeof(T);
                if (_eventTable.TryGetValue(eventType, out Delegate existing))
                {
                    _eventTable[eventType] = Delegate.Combine(existing, handler);
                }
                else
                {
                    _eventTable[eventType] = handler;
                }
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            lock (_lock)
            {
                Type eventType = typeof(T);
                if (_eventTable.TryGetValue(eventType, out Delegate existing))
                {
                    _eventTable[eventType] = Delegate.Remove(existing, handler);
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
