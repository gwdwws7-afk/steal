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

        public bool CanSeeTarget()
        {
            if (_currentTarget == null) return false;
            if (IsInVisionCone(_currentTarget.position))
            {
                if (HasLineOfSight(_currentTarget.position))
                {
                    if (IsInShadow(_currentTarget.position))
                    {
                        return Random.value > shadowDetectionPenalty;
                    }
                    _lastSeenPosition = _currentTarget.position;
                    return true;
                }
            }
            return false;
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
            Vector3 directionToTarget = targetPos - eyes.position;
            float distance = directionToTarget.magnitude;

            if (distance > visionDistance) return false;

            directionToTarget.y = 0;
            directionToTarget.Normalize();

            Vector3 forward = eyes.forward;
            forward.y = 0;
            forward.Normalize();

            float angle = Vector3.Angle(forward, directionToTarget);
            return angle <= visionAngle * 0.5f;
        }

        public bool HasLineOfSight(Vector3 targetPos)
        {
            Vector3 direction = targetPos - eyes.position;
            float distance = direction.magnitude;

            if (Physics.Raycast(eyes.position, direction.normalized, out RaycastHit hit, distance, obstructionLayer))
            {
                return hit.transform == _currentTarget;
            }

            return true;
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
            visionDistance = distance;
        }

        public void SetVisionAngle(float angle)
        {
            visionAngle = angle;
        }

        public float GetVisionDistance() => visionDistance;
        public float GetVisionAngle() => visionAngle;

        public void ApplyEMPEffect(float duration)
        {
            StartCoroutine(EMPEffectCoroutine(duration));
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
