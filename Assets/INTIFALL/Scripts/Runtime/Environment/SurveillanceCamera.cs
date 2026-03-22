using UnityEngine;
using INTIFALL.AI;
using INTIFALL.System;

namespace INTIFALL.Environment
{
    public class SurveillanceCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float visionDistance = 15f;
        [SerializeField] private float visionAngle = 60f;
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private float sweepPauseTime = 1f;

        [Header("Patrol Points")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private bool autoPatrol = true;

        [Header("Alert Settings")]
        [SerializeField] private bool triggersAlert = true;
        [SerializeField] private float alertDuration = 5f;

        [Header("References")]
        [SerializeField] private Transform cameraHead;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private LayerMask obstructionLayer;

        private int _currentPatrolIndex;
        private float _pauseTimer;
        private bool _isPaused;
        private Quaternion _targetRotation;
        private bool _playerDetected;
        private GameObject _player;

        public bool PlayerDetected => _playerDetected;

        private void Awake()
        {
            if (cameraHead == null)
                cameraHead = transform;

            _targetRotation = cameraHead.rotation;
        }

        private void Start()
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                _currentPatrolIndex = 0;
                _targetRotation = patrolPoints[0].rotation;
            }

            _player = GameObject.FindGameObjectWithTag("Player");
        }

        private void Update()
        {
            if (_playerDetected)
            {
                TrackPlayer();
                return;
            }

            if (autoPatrol && patrolPoints != null && patrolPoints.Length > 0)
            {
                Patrol();
            }
            else
            {
                ScanArea();
            }

            CheckForPlayer();
        }

        private void Patrol()
        {
            if (_isPaused)
            {
                _pauseTimer -= Time.deltaTime;
                if (_pauseTimer <= 0)
                {
                    _isPaused = false;
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
                    _targetRotation = patrolPoints[_currentPatrolIndex].rotation;
                }
                return;
            }

            cameraHead.rotation = Quaternion.RotateTowards(
                cameraHead.rotation,
                _targetRotation,
                rotationSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(cameraHead.rotation, _targetRotation) < 1f)
            {
                _isPaused = true;
                _pauseTimer = sweepPauseTime;
            }
        }

        private void ScanArea()
        {
            cameraHead.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        private void CheckForPlayer()
        {
            if (_player == null) return;

            Vector3 toPlayer = _player.transform.position - cameraHead.position;
            float distance = toPlayer.magnitude;

            if (distance > visionDistance) return;

            toPlayer.y = 0;
            toPlayer.Normalize();

            Vector3 forward = cameraHead.forward;
            forward.y = 0;
            forward.Normalize();

            float angle = Vector3.Angle(forward, toPlayer);

            if (angle <= visionAngle * 0.5f)
            {
                if (HasLineOfSight(_player.transform.position))
                {
                    OnPlayerDetected();
                }
            }
        }

        private bool HasLineOfSight(Vector3 targetPos)
        {
            Vector3 direction = targetPos - cameraHead.position;

            if (Physics.Raycast(cameraHead.position, direction.normalized, out RaycastHit hit, visionDistance, obstructionLayer))
            {
                return hit.transform == _player.transform;
            }

            return true;
        }

        private void OnPlayerDetected()
        {
            if (_playerDetected) return;

            _playerDetected = true;

            if (triggersAlert)
            {
                EventBus.Publish(new CameraDetectedPlayerEvent
                {
                    cameraPosition = transform.position,
                    duration = alertDuration
                });
            }

            AudioManager.Instance?.PlaySFX(null);
        }

        private void TrackPlayer()
        {
            if (_player == null)
            {
                _playerDetected = false;
                return;
            }

            Vector3 toPlayer = _player.transform.position - cameraHead.position;
            toPlayer.y = 0;

            Quaternion lookRotation = Quaternion.LookRotation(toPlayer);
            cameraHead.rotation = Quaternion.Slerp(cameraHead.rotation, lookRotation, rotationSpeed * 2f * Time.deltaTime);
        }

        public void DisableCamera()
        {
            this.enabled = false;

            if (TryGetComponent<Renderer>(out var renderer))
                renderer.enabled = false;
        }

        public void EnableCamera()
        {
            this.enabled = true;

            if (TryGetComponent<Renderer>(out var renderer))
                renderer.enabled = true;
        }

        public struct CameraDetectedPlayerEvent
        {
            public Vector3 cameraPosition;
            public float duration;
        }
    }
}