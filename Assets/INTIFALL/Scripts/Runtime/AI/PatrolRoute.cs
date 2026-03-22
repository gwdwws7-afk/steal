using UnityEngine;
using System.Collections.Generic;

namespace INTIFALL.AI
{
    public class PatrolRoute : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private List<Transform> waypoints = new();
        [SerializeField] private float waitTime = 2f;

        [Header("Settings")]
        [SerializeField] private bool loop = true;
        [SerializeField] private float arrivalThreshold = 1f;

        private int _currentIndex;
        private float _waitTimer;
        private bool _isWaiting;

        public int CurrentIndex => _currentIndex;

        public Vector3 GetCurrentWaypoint()
        {
            if (waypoints.Count == 0) return transform.position;
            return waypoints[_currentIndex].position;
        }

        public void AdvanceToNext()
        {
            if (waypoints.Count == 0) return;

            if (_isWaiting)
            {
                _isWaiting = false;
                _waitTimer = 0f;
            }

            if (loop)
            {
                _currentIndex = (_currentIndex + 1) % waypoints.Count;
            }
            else
            {
                if (_currentIndex < waypoints.Count - 1)
                {
                    _currentIndex++;
                }
                else
                {
                    _currentIndex = 0;
                }
            }

            _isWaiting = true;
            _waitTimer = waitTime;
        }

        public void Update()
        {
            if (!_isWaiting) return;

            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
            }
        }

        public bool HasWaypoints()
        {
            return waypoints.Count > 0;
        }

        public bool IsWaiting()
        {
            return _isWaiting;
        }

        public float GetWaitProgress()
        {
            if (!_isWaiting) return 0f;
            return 1f - (_waitTimer / waitTime);
        }

        public void Reset()
        {
            _currentIndex = 0;
            _isWaiting = false;
            _waitTimer = 0f;
        }

        public void SetWaypoint(int index, Transform waypoint)
        {
            if (index >= 0 && index < waypoints.Count)
            {
                waypoints[index] = waypoint;
            }
        }

        public void AddWaypoint(Transform waypoint)
        {
            waypoints.Add(waypoint);
        }

        public void RemoveWaypoint(int index)
        {
            if (index >= 0 && index < waypoints.Count)
            {
                waypoints.RemoveAt(index);
            }
        }

        public int WaypointCount => waypoints.Count;

        private void OnDrawGizmosSelected()
        {
            if (waypoints.Count < 2) return;

            Gizmos.color = Color.blue;

            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;

                Vector3 pos = waypoints[i].position;
                Gizmos.DrawWireSphere(pos, 0.3f);

                int nextIndex = (i + 1) % waypoints.Count;
                if (nextIndex == 0 && !loop) continue;

                if (waypoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(pos, waypoints[nextIndex].position);
                }

                UnityEditor.Handles.Label(pos, $"W{i}");
            }
        }
    }
}
