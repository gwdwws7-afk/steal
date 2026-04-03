using INTIFALL.Player;
using UnityEngine;

namespace INTIFALL.AI
{
    public class PerceptionModule : MonoBehaviour
    {
        [Header("Vision")]
        [SerializeField] private float visionDistance = 15f;
        [SerializeField] private float visionAngle = 60f;
        [SerializeField] private float crouchVisionMultiplier = 0.5f;

        [Header("Shadow Detection")]
        [SerializeField] private float shadowLuxThreshold = 30f;
        [SerializeField] private float shadowDetectionPenalty = 0.5f;

        [Header("Hearing")]
        [SerializeField] private float walkSoundRadius = 5f;
        [SerializeField] private float runSoundRadius = 12f;
        [SerializeField] private float crouchSoundRadius = 2f;

        [Header("Communication")]
        [SerializeField] private float commRange = 30f;

        [Header("References")]
        [SerializeField] private Transform eyes;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private LayerMask obstructionLayer;

        private Transform _currentTarget;
        private Vector3 _lastSeenPosition;

        private void Start()
        {
            if (eyes == null)
                eyes = transform;

            if (_currentTarget == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    _currentTarget = player.transform;
            }
        }

        public bool CanSeeTarget()
        {
            if (_currentTarget == null)
                return false;

            Vector3 targetPos = _currentTarget.position;
            if (!IsInVisionCone(targetPos))
                return false;

            if (!HasLineOfSight(targetPos))
                return false;

            Transform eyeTransform = eyes != null ? eyes : transform;
            float distance = Vector3.Distance(eyeTransform.position, targetPos);
            float effectiveDistance = GetEffectiveVisionDistance(targetPos);
            if (distance > effectiveDistance)
                return false;

            _lastSeenPosition = targetPos;
            return true;
        }

        public bool IsInShadow(Vector3 targetPos)
        {
            float lux = GetAmbientLux(targetPos);
            return lux < shadowLuxThreshold;
        }

        private float GetAmbientLux(Vector3 position)
        {
            Light[] lights = FindObjectsOfType<Light>();
            float totalLux = 0f;

            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    float angle = Vector3.Angle(light.transform.forward, position - light.transform.position);
                    if (angle < light.spotAngle * 0.5f)
                    {
                        float dist = Vector3.Distance(position, light.transform.position);
                        totalLux += light.intensity * 10000f / (dist * dist);
                    }
                }
                else if (light.type == LightType.Point)
                {
                    float dist = Vector3.Distance(position, light.transform.position);
                    if (dist < light.range)
                    {
                        totalLux += light.intensity * 100f / (dist * dist);
                    }
                }
            }

            return Mathf.Max(totalLux, 0.5f);
        }

        public bool CanHearTarget(Vector3 targetPosition, bool isSprinting, bool isCrouching)
        {
            float hearingRadius = isCrouching ? crouchSoundRadius :
                                 isSprinting ? runSoundRadius :
                                 walkSoundRadius;

            float distance = Vector3.Distance(transform.position, targetPosition);
            return distance <= hearingRadius;
        }

        public bool IsInVisionCone(Vector3 targetPos)
        {
            Transform eyeTransform = eyes != null ? eyes : transform;
            Vector3 directionToTarget = targetPos - eyeTransform.position;
            float distance = directionToTarget.magnitude;

            if (distance > visionDistance)
                return false;

            directionToTarget.y = 0;
            directionToTarget.Normalize();

            Vector3 forward = eyeTransform.forward;
            forward.y = 0;
            forward.Normalize();

            float angle = Vector3.Angle(forward, directionToTarget);
            return angle <= visionAngle * 0.5f;
        }

        public bool HasLineOfSight(Vector3 targetPos)
        {
            Transform eyeTransform = eyes != null ? eyes : transform;
            Vector3 direction = targetPos - eyeTransform.position;
            float distance = direction.magnitude;

            if (Physics.Raycast(eyeTransform.position, direction.normalized, out RaycastHit hit, distance, obstructionLayer))
            {
                return hit.transform == _currentTarget;
            }

            return true;
        }

        public bool CanHearCurrentTarget()
        {
            if (_currentTarget == null)
                return false;

            var player = _currentTarget.GetComponent<PlayerController>();
            bool isSprinting = player != null && player.IsSprinting;
            bool isCrouching = player != null && player.IsCrouching;

            return CanHearTarget(_currentTarget.position, isSprinting, isCrouching);
        }

        public bool IsInCommunicationRange(Vector3 targetPos)
        {
            return Vector3.Distance(transform.position, targetPos) <= commRange;
        }

        public void SetTarget(Transform target)
        {
            _currentTarget = target;
        }

        public void ClearTarget()
        {
            _currentTarget = null;
        }

        public Vector3 GetTargetPosition()
        {
            return _currentTarget != null ? _currentTarget.position : _lastSeenPosition;
        }

        public Vector3 GetLastSeenPosition()
        {
            return _lastSeenPosition;
        }

        public void SetVisionDistance(float distance)
        {
            visionDistance = Mathf.Max(0.1f, distance);
        }

        public void SetVisionAngle(float angle)
        {
            visionAngle = Mathf.Clamp(angle, 1f, 179f);
        }

        public void ConfigurePerceptionProfile(
            float visionDistanceMeters,
            float visionAngleDegrees,
            float crouchVisionMultiplierValue,
            float shadowLuxThresholdValue,
            float shadowDetectionPenaltyValue,
            float walkSoundRadiusMeters,
            float runSoundRadiusMeters,
            float crouchSoundRadiusMeters,
            float communicationRangeMeters)
        {
            visionDistance = Mathf.Max(0.1f, visionDistanceMeters);
            visionAngle = Mathf.Clamp(visionAngleDegrees, 1f, 179f);
            crouchVisionMultiplier = Mathf.Clamp(crouchVisionMultiplierValue, 0.1f, 1f);
            shadowLuxThreshold = Mathf.Max(0f, shadowLuxThresholdValue);
            shadowDetectionPenalty = Mathf.Clamp01(shadowDetectionPenaltyValue);
            walkSoundRadius = Mathf.Max(0.1f, walkSoundRadiusMeters);
            runSoundRadius = Mathf.Max(walkSoundRadius, runSoundRadiusMeters);
            crouchSoundRadius = Mathf.Clamp(crouchSoundRadiusMeters, 0.05f, runSoundRadius);
            commRange = Mathf.Max(0f, communicationRangeMeters);
        }

        public float GetVisionDistance() => visionDistance;
        public float GetVisionAngle() => visionAngle;
        public float CrouchVisionMultiplier => crouchVisionMultiplier;
        public float ShadowLuxThreshold => shadowLuxThreshold;
        public float ShadowDetectionPenalty => shadowDetectionPenalty;
        public float WalkSoundRadius => walkSoundRadius;
        public float RunSoundRadius => runSoundRadius;
        public float CrouchSoundRadius => crouchSoundRadius;
        public float CommunicationRange => commRange;

        public void ApplyEMPEffect(float duration)
        {
            StartCoroutine(EMPEffectCoroutine(duration));
        }

        private float GetEffectiveVisionDistance(Vector3 targetPos)
        {
            float visibilityMultiplier = 1f;

            if (IsCurrentTargetCrouching())
                visibilityMultiplier *= crouchVisionMultiplier;

            if (IsInShadow(targetPos))
                visibilityMultiplier *= 1f - shadowDetectionPenalty;

            visibilityMultiplier = Mathf.Clamp(visibilityMultiplier, 0.05f, 1f);
            return visionDistance * visibilityMultiplier;
        }

        private bool IsCurrentTargetCrouching()
        {
            if (_currentTarget == null)
                return false;

            PlayerController player = _currentTarget.GetComponent<PlayerController>();
            return player != null && player.IsCrouching;
        }

        private global::System.Collections.IEnumerator EMPEffectCoroutine(float duration)
        {
            float originalDistance = visionDistance;
            float originalAngle = visionAngle;
            visionDistance = 0f;
            visionAngle = 0f;
            yield return new WaitForSeconds(duration);
            visionDistance = originalDistance;
            visionAngle = originalAngle;
        }
    }
}
