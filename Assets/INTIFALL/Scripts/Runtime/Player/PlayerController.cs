using UnityEngine;

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

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController _cc;
        private PlayerStateMachine _stateMachine;

        private Vector3 _moveInput;
        private Vector3 _velocity;
        private bool _sprintHeld;
        private bool _crouchHeld;
        private bool _rollPressed;
        private bool _ropeAttached;
        private Vector3 _ropePoint;
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
            _currentStamina = maxStamina;
            _currentSpeed = walkSpeed;
        }

        private void Update()
        {
            _stateMachine.Update();
            ReadInput();
            UpdateStamina();
            UpdateRoll();
            UpdateRope();
            ApplyMovement();
        }

        private void ReadInput()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            _moveInput = new Vector3(h, 0, v).normalized;

            _sprintHeld = Input.GetKey(KeyCode.LeftShift);
            _crouchHeld = Input.GetKey(KeyCode.C);

            if (Input.GetKeyDown(KeyCode.Space) && !_isRolling && !_ropeAttached)
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
            if (!_ropeAttached) return;
        }

        public void AttachToRope(Vector3 point)
        {
            _ropeAttached = true;
            _ropePoint = point;
            _stateMachine.TransitionTo(EPlayerState.Rope);
        }

        public void DetachFromRope()
        {
            _ropeAttached = false;
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
                float lateral = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                Vector3 lateralMove = transform.right * lateral * ropeLateralSpeed;
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

        public bool IsMoving => _moveInput.magnitude > 0.1f || _isRolling;
        public Vector3 MoveInput => _moveInput;
    }
}