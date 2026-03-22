using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.Input
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Key Bindings")]
        [SerializeField] private KeyCode moveForward = KeyCode.W;
        [SerializeField] private KeyCode moveBackward = KeyCode.S;
        [SerializeField] private KeyCode moveLeft = KeyCode.A;
        [SerializeField] private KeyCode moveRight = KeyCode.D;
        [SerializeField] private KeyCode sprint = KeyCode.LeftShift;
        [SerializeField] private KeyCode crouch = KeyCode.C;
        [SerializeField] private KeyCode jump = KeyCode.Space;
        [SerializeField] private KeyCode roll = KeyCode.Space;
        [SerializeField] private KeyCode interact = KeyCode.E;
        [SerializeField] private KeyCode cover = KeyCode.E;
        [SerializeField] private KeyCode tool1 = KeyCode.Alpha1;
        [SerializeField] private KeyCode tool2 = KeyCode.Alpha2;
        [SerializeField] private KeyCode tool3 = KeyCode.Alpha3;
        [SerializeField] private KeyCode tool4 = KeyCode.Alpha4;
        [SerializeField] private KeyCode useTool = KeyCode.Q;
        [SerializeField] private KeyCode pause = KeyCode.Escape;
        [SerializeField] private KeyCode arsenal = KeyCode.U;
        [SerializeField] private KeyCode toggleHUD = KeyCode.H;

        [Header("Mouse")]
        [SerializeField] private KeyCode mouseLeft = KeyCode.Mouse0;
        [SerializeField] private KeyCode mouseRight = KeyCode.Mouse1;

        [Header("Settings")]
        [SerializeField] private bool invertY = false;
        [SerializeField] private float mouseSensitivity = 2f;

        private bool _inputEnabled = true;

        public bool InputEnabled => _inputEnabled;

        public Vector2 MoveInput
        {
            get
            {
                if (!_inputEnabled) return Vector2.zero;

                float h = 0f;
                float v = 0f;

                if (Input.GetKey(moveForward)) v += 1f;
                if (Input.GetKey(moveBackward)) v -= 1f;
                if (Input.GetKey(moveRight)) h += 1f;
                if (Input.GetKey(moveLeft)) h -= 1f;

                return new Vector2(h, v).normalized;
            }
        }

        public bool IsSprinting => _inputEnabled && Input.GetKey(sprint);
        public bool IsCrouching => _inputEnabled && Input.GetKey(crouch);
        public bool IsJumping => _inputEnabled && Input.GetKeyDown(jump);
        public bool IsRolling => _inputEnabled && Input.GetKeyDown(roll);
        public bool IsInteracting => _inputEnabled && Input.GetKeyDown(interact);
        public bool IsUsingTool => _inputEnabled && Input.GetKeyDown(useTool);
        public bool IsPausing => _inputEnabled && Input.GetKeyDown(pause);
        public bool IsOpeningArsenal => _inputEnabled && Input.GetKeyDown(arsenal);
        public bool IsTogglingHUD => _inputEnabled && Input.GetKeyDown(toggleHUD);

        public bool IsMouseLeftDown => _inputEnabled && Input.GetKey(mouseLeft);
        public bool IsMouseRightDown => _inputEnabled && Input.GetKey(mouseRight);
        public bool IsMouseLeftPressed => _inputEnabled && Input.GetKeyDown(mouseLeft);
        public bool IsMouseRightPressed => _inputEnabled && Input.GetKeyDown(mouseRight);

        public Vector2 MouseDelta
        {
            get
            {
                if (!_inputEnabled) return Vector2.zero;

                float x = Input.GetAxis("Mouse X") * mouseSensitivity;
                float y = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? -1 : 1);

                return new Vector2(x, y);
            }
        }

        public float MouseScrollDelta => _inputEnabled ? Input.GetAxis("Mouse ScrollWheel") : 0f;

        public int SelectedToolSlot
        {
            get
            {
                if (!_inputEnabled) return -1;

                if (Input.GetKeyDown(tool1)) return 0;
                if (Input.GetKeyDown(tool2)) return 1;
                if (Input.GetKeyDown(tool3)) return 2;
                if (Input.GetKeyDown(tool4)) return 3;

                return -1;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!_inputEnabled) return;

            if (IsPausing)
            {
                EventBus.Publish(new PauseToggleEvent());
            }

            if (IsTogglingHUD)
            {
                EventBus.Publish(new HUDToggleEvent());
            }

            if (IsInteracting)
            {
                EventBus.Publish(new InteractEvent());
            }
        }

        public void EnableInput()
        {
            _inputEnabled = true;
        }

        public void DisableInput()
        {
            _inputEnabled = false;
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = Mathf.Max(0.1f, sensitivity);
        }

        public void SetInvertY(bool invert)
        {
            invertY = invert;
        }

        public struct PauseToggleEvent { }
        public struct HUDToggleEvent { }
        public struct InteractEvent { }
    }
}