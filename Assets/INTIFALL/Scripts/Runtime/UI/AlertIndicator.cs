using UnityEngine;
using UnityEngine.UI;

namespace INTIFALL.UI
{
    public class AlertIndicator : MonoBehaviour
    {
        [Header("Alert States")]
        [SerializeField] private Image alertBackground;
        [SerializeField] private Text alertText;
        [SerializeField] private Image alertIcon;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color cautionColor = Color.yellow;
        [SerializeField] private Color dangerColor = Color.red;
        [SerializeField] private Color combatColor = new Color(1f, 0.3f, 0f);

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 1f;
        [SerializeField] private float cautionThreshold = 0.5f;
        [SerializeField] private float dangerThreshold = 0.8f;

        private bool _isAlerted;
        private float _alertLevel;
        private float _currentPulse;

        public bool IsAlerted => _isAlerted;
        public float AlertLevel => _alertLevel;

        private void Update()
        {
            if (_isAlerted)
            {
                _currentPulse += Time.deltaTime * pulseSpeed;
                UpdatePulseEffect();
            }
        }

        public void SetAlertState(bool inCombat)
        {
            _isAlerted = inCombat;

            if (!_isAlerted)
            {
                ResetToNormal();
            }
            else
            {
                SetAlertColor(combatColor);
                _alertLevel = 1f;
            }
        }

        public void SetAlertLevel(float level)
        {
            _alertLevel = Mathf.Clamp01(level);

            if (_alertLevel < cautionThreshold)
            {
                SetAlertColor(normalColor);
            }
            else if (_alertLevel < dangerThreshold)
            {
                SetAlertColor(cautionColor);
            }
            else
            {
                SetAlertColor(dangerColor);
            }
        }

        private void SetAlertColor(Color color)
        {
            if (alertBackground != null)
                alertBackground.color = color;

            if (alertText != null)
                alertText.color = color;

            if (alertIcon != null)
                alertIcon.color = color;
        }

        private void UpdatePulseEffect()
        {
            float pulse = Mathf.Sin(_currentPulse) * 0.3f + 0.7f;

            if (alertBackground != null)
            {
                Color c = alertBackground.color;
                alertBackground.color = new Color(c.r, c.g, c.b, pulse);
            }
        }

        private void ResetToNormal()
        {
            SetAlertColor(normalColor);
            _alertLevel = 0f;
            _currentPulse = 0f;

            if (alertBackground != null)
            {
                Color c = alertBackground.color;
                alertBackground.color = new Color(c.r, c.g, c.b, 1f);
            }
        }

        public void ShowWarning(string message)
        {
            if (alertText != null)
                alertText.text = message;

            SetAlertColor(dangerColor);
            _alertLevel = 1f;
        }
    }
}