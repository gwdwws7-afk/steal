using UnityEngine;

namespace INTIFALL.Input
{
    public static class InputCompat
    {
        private static bool _legacyInputAvailable = true;

        public static bool GetKey(KeyCode key)
        {
            if (!TryUseLegacyInput())
                return false;

            return UnityEngine.Input.GetKey(key);
        }

        public static bool GetKeyDown(KeyCode key)
        {
            if (!TryUseLegacyInput())
                return false;

            return UnityEngine.Input.GetKeyDown(key);
        }

        public static bool GetKeyUp(KeyCode key)
        {
            if (!TryUseLegacyInput())
                return false;

            return UnityEngine.Input.GetKeyUp(key);
        }

        public static float GetAxis(string axisName)
        {
            if (!TryUseLegacyInput())
                return 0f;

            return UnityEngine.Input.GetAxis(axisName);
        }

        public static float GetAxisRaw(string axisName)
        {
            if (!TryUseLegacyInput())
                return 0f;

            return UnityEngine.Input.GetAxisRaw(axisName);
        }

        public static Vector3 MousePosition
        {
            get
            {
                if (!TryUseLegacyInput())
                    return Vector3.zero;

                return UnityEngine.Input.mousePosition;
            }
        }

        private static bool TryUseLegacyInput()
        {
            if (!_legacyInputAvailable)
                return false;

            try
            {
                _ = UnityEngine.Input.anyKey;
                return true;
            }
            catch (global::System.InvalidOperationException)
            {
                _legacyInputAvailable = false;
                return false;
            }
        }
    }
}
