using UnityEngine;
using INTIFALL.Input;

namespace INTIFALL.Player
{
    public enum ECoverType
    {
        Low,      // < 1m - stand blocks legs, crouch blocks all
        High,     // 1-2m - stand blocks all, crouch blocks legs
        Full      // > 2m - blocks all vision
    }

    public class CoverData
    {
        public GameObject CoverObject;
        public ECoverType CoverType;
        public Vector3 CoverNormal;
        public Vector3 BestPeekLeft;
        public Vector3 BestPeekRight;
        public bool HasLeftPeek;
        public bool HasRightPeek;
    }

    public class CoverSystem : MonoBehaviour
    {
        [Header("Cover Detection")]
        [SerializeField] private float coverDetectionRange = 1.5f;
        [SerializeField] private float coverDetectionAngle = 60f;
        [SerializeField] private LayerMask coverLayerMask = ~0;

        [Header("Cover Movement")]
        [SerializeField] private float peekMoveSpeed = 2f;
        [SerializeField] private float coverToCoverDistance = 0.5f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private PlayerController _player;
        private CharacterController _cc;
        private CoverData _currentCover;
        private bool _isInCover;
        private bool _isPeekingLeft;
        private bool _isPeekingRight;
        private Vector3 _coverPosition;
        private Vector3 _coverForward;
        private float _originalHeight;
        private Vector3 _originalCenter;

        public bool IsInCover => _isInCover;
        public bool IsPeeking => _isPeekingLeft || _isPeekingRight;
        public CoverData CurrentCover => _currentCover;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _cc = GetComponent<CharacterController>();
            _originalHeight = _cc.height;
            _originalCenter = _cc.center;
        }

        private void Update()
        {
            if (_isInCover)
            {
                HandleCoverInput();
            }
            else
            {
                TryEnterCover();
            }
        }

        private void TryEnterCover()
        {
            if (_player.State == EPlayerState.Roll || 
                _player.State == EPlayerState.Rope || 
                _player.State == EPlayerState.Crouch)
                return;

            if (!InputCompat.GetKeyDown(KeyCode.E)) return;

            if (DetectCover(out CoverData cover))
            {
                EnterCover(cover);
            }
        }

        private bool DetectCover(out CoverData cover)
        {
            cover = null;
            Vector3 playerPos = transform.position + Vector3.up * 1f;
            Collider[] hits = Physics.OverlapSphere(playerPos, coverDetectionRange, coverLayerMask);

            foreach (Collider hit in hits)
            {
                if (hit.gameObject.CompareTag("Cover"))
                {
                    Vector3 toCover = hit.transform.position - transform.position;
                    toCover.y = 0;
                    float angle = Vector3.Angle(transform.forward, toCover);
                    if (angle < coverDetectionAngle)
                    {
                        cover = new CoverData
                        {
                            CoverObject = hit.gameObject,
                            CoverNormal = -toCover.normalized,
                            CoverType = DetermineCoverType(hit)
                        };
                        return true;
                    }
                }
            }
            return false;
        }

        private ECoverType DetermineCoverType(Collider cover)
        {
            Vector3 closest = cover.ClosestPointOnBounds(transform.position);
            float height = closest.y - transform.position.y;
            if (height < 1f) return ECoverType.Low;
            if (height < 2f) return ECoverType.High;
            return ECoverType.Full;
        }

        private void EnterCover(CoverData cover)
        {
            _currentCover = cover;
            _isInCover = true;
            _coverPosition = cover.CoverObject.transform.position;
            _coverForward = cover.CoverNormal;

            Vector3 enterOffset = cover.CoverNormal * 0.3f;
            _coverPosition += enterOffset;

            transform.position = _coverPosition;
            transform.forward = cover.CoverNormal;

            if (cover.CoverType == ECoverType.Low)
            {
                _cc.height = _originalHeight * 0.5f;
                _cc.center = new Vector3(0, _originalCenter.y * 0.5f, 0);
            }

            _player.enabled = false;
        }

        private void ExitCover()
        {
            if (_currentCover != null && _currentCover.CoverType == ECoverType.Low)
            {
                _cc.height = _originalHeight;
                _cc.center = _originalCenter;
            }

            _isInCover = false;
            _isPeekingLeft = false;
            _isPeekingRight = false;
            _currentCover = null;
            _player.enabled = true;
        }

        private void HandleCoverInput()
        {
            if (InputCompat.GetKeyDown(KeyCode.E))
            {
                ExitCover();
                return;
            }

            float h = InputCompat.GetAxis("Horizontal");

            if (h < -0.5f && !_isPeekingLeft)
            {
                StartPeek(true);
            }
            else if (h > 0.5f && !_isPeekingRight)
            {
                StartPeek(false);
            }
            else if (Mathf.Abs(h) < 0.3f && ( _isPeekingLeft || _isPeekingRight))
            {
                StopPeek();
            }

            if (_isPeekingLeft)
            {
                Vector3 leftDir = Vector3.Cross(_coverForward, Vector3.up).normalized;
                transform.position += leftDir * peekMoveSpeed * Time.deltaTime * Mathf.Abs(h);
            }
            else if (_isPeekingRight)
            {
                Vector3 rightDir = Vector3.Cross(Vector3.up, _coverForward).normalized;
                transform.position += rightDir * peekMoveSpeed * Time.deltaTime * Mathf.Abs(h);
            }
        }

        private void StartPeek(bool isLeft)
        {
            if (isLeft)
            {
                _isPeekingLeft = true;
                _isPeekingRight = false;
                transform.forward = Vector3.Cross(_coverForward, Vector3.up);
            }
            else
            {
                _isPeekingRight = true;
                _isPeekingLeft = false;
                transform.forward = Vector3.Cross(Vector3.up, _coverForward);
            }
        }

        private void StopPeek()
        {
            _isPeekingLeft = false;
            _isPeekingRight = false;
            transform.forward = _coverForward;
        }

        public bool IsInCoverZone()
        {
            return DetectCover(out _);
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;
            Gizmos.color = _isInCover ? Color.green : Color.yellow;
            Vector3 pos = transform.position + Vector3.up;
            Gizmos.DrawWireSphere(pos, coverDetectionRange);
        }
    }
}
