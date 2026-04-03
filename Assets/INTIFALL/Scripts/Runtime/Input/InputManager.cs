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

                if (InputCompat.GetKey(moveForward)) v += 1f;
                if (InputCompat.GetKey(moveBackward)) v -= 1f;
                if (InputCompat.GetKey(moveRight)) h += 1f;
                if (InputCompat.GetKey(moveLeft)) h -= 1f;

                return new Vector2(h, v).normalized;
            }
        }

        public bool IsSprinting => _inputEnabled && InputCompat.GetKey(sprint);
        public bool IsCrouching => _inputEnabled && InputCompat.GetKey(crouch);
        public bool IsJumping => _inputEnabled && InputCompat.GetKeyDown(jump);
        public bool IsRolling => _inputEnabled && InputCompat.GetKeyDown(roll);
        public bool IsInteracting => _inputEnabled && InputCompat.GetKeyDown(interact);
        public bool IsUsingTool => _inputEnabled && InputCompat.GetKeyDown(useTool);
        public bool IsPausing => _inputEnabled && InputCompat.GetKeyDown(pause);
        public bool IsOpeningArsenal => _inputEnabled && InputCompat.GetKeyDown(arsenal);
        public bool IsTogglingHUD => _inputEnabled && InputCompat.GetKeyDown(toggleHUD);

        public bool IsMouseLeftDown => _inputEnabled && InputCompat.GetKey(mouseLeft);
        public bool IsMouseRightDown => _inputEnabled && InputCompat.GetKey(mouseRight);
        public bool IsMouseLeftPressed => _inputEnabled && InputCompat.GetKeyDown(mouseLeft);
        public bool IsMouseRightPressed => _inputEnabled && InputCompat.GetKeyDown(mouseRight);

        public Vector2 MouseDelta
        {
            get
            {
                if (!_inputEnabled) return Vector2.zero;

                float x = InputCompat.GetAxis("Mouse X") * mouseSensitivity;
                float y = InputCompat.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? -1 : 1);

                return new Vector2(x, y);
            }
        }

        public float MouseScrollDelta => _inputEnabled ? InputCompat.GetAxis("Mouse ScrollWheel") : 0f;

        public int SelectedToolSlot
        {
            get
            {
                if (!_inputEnabled) return -1;

                if (InputCompat.GetKeyDown(tool1)) return 0;
                if (InputCompat.GetKeyDown(tool2)) return 1;
                if (InputCompat.GetKeyDown(tool3)) return 2;
                if (InputCompat.GetKeyDown(tool4)) return 3;

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
