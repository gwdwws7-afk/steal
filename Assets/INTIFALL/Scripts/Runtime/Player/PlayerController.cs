using UnityEngine;
using INTIFALL.Input;
using INTIFALL.System;
using INTIFALL.Tools;

namespace INTIFALL.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Parameters")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 7.0f;
        [SerializeField] private float crouchSpeed = 1.5f;
        [SerializeField] private float rollSpeed = 6.0f;
        [SerializeField] private float rollDistance = 3.0f;
        [SerializeField] private float rollDuration = 0.3f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float sprintStaminaDrain = 15f;
        [SerializeField] private float staminaRecovery = 10f;

        [Header("Rope")]
        [SerializeField] private float ropeSpeed = 3.0f;
        [SerializeField] private float ropeLateralSpeed = 2.0f;
        [SerializeField] private float ropeSnapRadius = 0.75f;
        [SerializeField] private float ropeMinYOffset = -1.25f;
        [SerializeField] private float ropeMaxYOffset = 2.0f;
        [SerializeField] private float ropeDetachJumpImpulse = 3.5f;
        [SerializeField] private bool autoAttachOnRopeToolEvent = true;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController _cc;
        private PlayerStateMachine _stateMachine;
        private CQCSystem _cqcSystem;

        private Vector3 _moveInput;
        private Vector3 _velocity;
        private bool _sprintHeld;
        private bool _crouchHeld;
        private bool _rollPressed;
        private bool _detachRopePressed;
        private bool _ropeAttached;
        private Vector3 _ropePoint;
        private float _ropeDetachAtTime = -1f;
        private float _currentSpeed;
        private float _currentStamina;
        private bool _isRolling;
        private float _rollTimer;
        private Vector3 _rollDirection;
        private Vector3 _pendingMoveInput;

        public float CurrentSpeed => _currentSpeed;
        public float CurrentStamina => _currentStamina;
        public float MaxStamina => maxStamina;
        public bool IsSprinting => _stateMachine.CurrentState == EPlayerState.Sprint;
        public bool IsCrouching => _stateMachine.CurrentState == EPlayerState.Crouch;
        public bool IsRolling => _isRolling;
        public bool IsOnRope => _ropeAttached;
        public EPlayerState State => _stateMachine.CurrentState;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _stateMachine = GetComponent<PlayerStateMachine>();
            _cqcSystem = GetComponent<CQCSystem>();
            _currentStamina = maxStamina;
            _currentSpeed = walkSpeed;
        }

        private void OnEnable()
        {
            if (autoAttachOnRopeToolEvent)
                EventBus.Subscribe<RopeUsedEvent>(OnRopeUsed);
        }

        private void OnDisable()
        {
            if (autoAttachOnRopeToolEvent)
                EventBus.Unsubscribe<RopeUsedEvent>(OnRopeUsed);
        }

        private void Update()
        {
            ReadInput();
            UpdateStamina();
            UpdateRoll();
            UpdateRope();
            ApplyMovement();
        }

        private void ReadInput()
        {
            float h = InputCompat.GetAxisRaw("Horizontal");
            float v = InputCompat.GetAxisRaw("Vertical");
            _moveInput = new Vector3(h, 0, v).normalized;
            if (_moveInput.sqrMagnitude > 0.0001f)
                _cqcSystem?.OnPlayerMove();

            _sprintHeld = InputCompat.GetKey(KeyCode.LeftShift);
            _crouchHeld = InputCompat.GetKey(KeyCode.C);
            _detachRopePressed = _ropeAttached && InputCompat.GetKeyDown(KeyCode.Space);

            if (InputCompat.GetKeyDown(KeyCode.Space) && !_isRolling && !_ropeAttached)
            {
                _rollPressed = true;
            }
        }

        private void UpdateStamina()
        {
            if (_stateMachine.CurrentState == EPlayerState.Sprint)
            {
                _currentStamina -= sprintStaminaDrain * Time.deltaTime;
                _currentStamina = Mathf.Max(0, _currentStamina);
            }
            else if (_currentStamina < maxStamina)
            {
                _currentStamina += staminaRecovery * Time.deltaTime;
                _currentStamina = Mathf.Min(maxStamina, _currentStamina);
            }

            if (_currentStamina <= 0 && _stateMachine.CurrentState == EPlayerState.Sprint)
            {
                _sprintHeld = false;
            }
        }

        private void UpdateRoll()
        {
            if (_isRolling)
            {
                _rollTimer += Time.deltaTime;
                if (_rollTimer >= rollDuration)
                {
                    _isRolling = false;
                    _cc.enabled = false;
                    transform.position += _rollDirection * rollDistance;
                    _cc.enabled = true;
                    _stateMachine.TransitionTo(EPlayerState.Idle);
                }
                return;
            }

            if (_rollPressed)
            {
                _rollPressed = false;
                Vector3 rollDir = _moveInput.magnitude > 0.1f ? _moveInput : transform.forward;
                StartRoll(rollDir);
                return;
            }
            _rollPressed = false;
        }

        private void StartRoll(Vector3 direction)
        {
            _isRolling = true;
            _rollTimer = 0f;
            _rollDirection = direction;
            _currentSpeed = rollSpeed;
            _stateMachine.TransitionTo(EPlayerState.Roll);
        }

        private void UpdateRope()
        {
            if (!_ropeAttached)
                return;

            if (_detachRopePressed)
            {
                _detachRopePressed = false;
                DetachFromRope(true);
                return;
            }

            if (_ropeDetachAtTime > 0f && Time.time >= _ropeDetachAtTime)
            {
                DetachFromRope();
                return;
            }

            if (_stateMachine.CurrentState != EPlayerState.Rope)
                _stateMachine.TransitionTo(EPlayerState.Rope);

            Vector3 currentPosition = transform.position;
            Vector3 horizontalOffset = currentPosition - _ropePoint;
            horizontalOffset.y = 0f;
            float maxHorizontalOffset = Mathf.Max(0.1f, ropeSnapRadius);
            if (horizontalOffset.sqrMagnitude > maxHorizontalOffset * maxHorizontalOffset)
            {
                horizontalOffset = horizontalOffset.normalized * maxHorizontalOffset;
            }

            float minY = _ropePoint.y + ropeMinYOffset;
            float maxY = _ropePoint.y + ropeMaxYOffset;
            float clampedY = Mathf.Clamp(currentPosition.y, minY, maxY);
            Vector3 constrained = new Vector3(
                _ropePoint.x + horizontalOffset.x,
                clampedY,
                _ropePoint.z + horizontalOffset.z);

            Vector3 correction = constrained - currentPosition;
            if (correction.sqrMagnitude > 0.000001f)
            {
                // CharacterController.Move can under-correct on large rope snap deltas in some runtimes.
                // Use direct reposition for large corrections to guarantee stable rope constraints.
                if (correction.sqrMagnitude > 1f)
                {
                    bool wasEnabled = _cc.enabled;
                    _cc.enabled = false;
                    transform.position = constrained;
                    _cc.enabled = wasEnabled;
                }
                else
                {
                    _cc.Move(correction);
                }
            }

            _velocity = Vector3.zero;
        }

        public void AttachToRope(Vector3 point)
        {
            AttachToRope(point, -1f);
        }

        public void AttachToRope(Vector3 point, float durationSeconds)
        {
            _ropeAttached = true;
            _ropePoint = point;
            _ropeDetachAtTime = durationSeconds > 0f ? Time.time + durationSeconds : -1f;
            _velocity = Vector3.zero;
            _stateMachine.TransitionTo(EPlayerState.Rope);
        }

        public void DetachFromRope()
        {
            DetachFromRope(false);
        }

        public void DetachFromRope(bool applyJumpImpulse)
        {
            _ropeAttached = false;
            _ropeDetachAtTime = -1f;
            _detachRopePressed = false;
            if (applyJumpImpulse)
            {
                _velocity = new Vector3(_velocity.x, ropeDetachJumpImpulse, _velocity.z);
            }
            _stateMachine.TransitionTo(EPlayerState.Idle);
        }

        private void ApplyMovement()
        {
            if (_isRolling)
            {
                _velocity = _rollDirection * rollSpeed;
                _velocity.y = 0;
                _cc.Move(_velocity * Time.deltaTime);
                return;
            }

            if (_ropeAttached)
            {
                float lateral = InputCompat.GetAxis("Horizontal");
                float vertical = InputCompat.GetAxis("Vertical");
                Vector3 radial = transform.position - _ropePoint;
                radial.y = 0f;
                Vector3 lateralAxis = radial.sqrMagnitude > 0.0001f ? Vector3.Cross(Vector3.up, radial.normalized) : transform.right;
                Vector3 lateralMove = lateralAxis * lateral * ropeLateralSpeed;
                Vector3 ropeMove = Vector3.up * vertical * ropeSpeed;
                _cc.Move((lateralMove + ropeMove) * Time.deltaTime);
                return;
            }

            switch (_stateMachine.CurrentState)
            {
                case EPlayerState.Idle:
                case EPlayerState.Walk:
                    HandleGroundMovement();
                    break;
                case EPlayerState.Sprint:
                    HandleSprintMovement();
                    break;
                case EPlayerState.Crouch:
                    HandleCrouchMovement();
                    break;
            }

            if (!_cc.isGrounded)
            {
                _velocity.y -= 20f * Time.deltaTime;
            }
            else
            {
                _velocity.y = 0;
            }

            _cc.Move(_velocity * Time.deltaTime);
        }

        private void HandleGroundMovement()
        {
            if (_crouchHeld)
            {
                _stateMachine.TransitionTo(EPlayerState.Crouch);
                _currentSpeed = crouchSpeed;
                return;
            }

            if (_moveInput.magnitude < 0.1f)
            {
                _stateMachine.TransitionTo(EPlayerState.Idle);
                _currentSpeed = 0;
                _velocity = Vector3.zero;
                return;
            }

            if (_sprintHeld && _currentStamina > 0)
            {
                _stateMachine.TransitionTo(EPlayerState.Sprint);
                _currentSpeed = sprintSpeed;
            }
            else
            {
                _stateMachine.TransitionTo(EPlayerState.Walk);
                _currentSpeed = walkSpeed;
            }

            Vector3 targetVelocity = transform.TransformDirection(_moveInput) * _currentSpeed;
            _velocity = new Vector3(targetVelocity.x, _velocity.y, targetVelocity.z);
        }

        private void HandleSprintMovement()
        {
            if (!_sprintHeld || _currentStamina <= 0 || _moveInput.magnitude < 0.1f)
            {
                _stateMachine.TransitionTo(EPlayerState.Walk);
                _currentSpeed = walkSpeed;
                return;
            }

            if (_crouchHeld)
            {
                _stateMachine.TransitionTo(EPlayerState.Crouch);
                _currentSpeed = crouchSpeed;
                return;
            }

            Vector3 targetVelocity = transform.TransformDirection(_moveInput) * sprintSpeed;
            _velocity = new Vector3(targetVelocity.x, _velocity.y, targetVelocity.z);
        }

        private void HandleCrouchMovement()
        {
            if (!_crouchHeld)
            {
                _stateMachine.TransitionTo(EPlayerState.Idle);
                _currentSpeed = 0;
                _velocity = Vector3.zero;
                return;
            }

            if (_moveInput.magnitude > 0.1f)
            {
                Vector3 targetVelocity = transform.TransformDirection(_moveInput) * crouchSpeed;
                _velocity = new Vector3(targetVelocity.x, _velocity.y, targetVelocity.z);
            }
            else
            {
                _velocity = new Vector3(0, _velocity.y, 0);
            }
        }

        private void OnRopeUsed(RopeUsedEvent evt)
        {
            if (!autoAttachOnRopeToolEvent)
                return;

            float effectiveRange = Mathf.Max(1f, evt.range);
            float distanceToUsePosition = Vector3.Distance(transform.position, evt.position);
            if (distanceToUsePosition > effectiveRange)
                return;

            Vector3 facingForward = GetFacingForward();
            Vector3 anchor = evt.position + facingForward * Mathf.Clamp(effectiveRange * 0.35f, 1.0f, 3.5f);
            anchor.y = Mathf.Max(evt.position.y + 0.75f, transform.position.y + 0.5f);
            AttachToRope(anchor, evt.duration);
        }

        private Vector3 GetFacingForward()
        {
            if (cameraTransform != null)
            {
                Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
                if (forward.sqrMagnitude > 0.0001f)
                    return forward.normalized;
            }

            Vector3 selfForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (selfForward.sqrMagnitude > 0.0001f)
                return selfForward.normalized;

            return Vector3.forward;
        }

        public bool IsMoving => _moveInput.magnitude > 0.1f || _isRolling;
        public Vector3 MoveInput => _moveInput;
    }
}

