using UnityEngine;

namespace INTIFALL.Environment
{
    public class LightingManager : MonoBehaviour
    {
        [Header("Light Sources")]
        [SerializeField] private Light[] sceneLights;
        [SerializeField] private GameObject[] lightFlickerObjects;

        [Header("Time of Day")]
        [SerializeField] private float timeOfDay = 0.25f;
        [SerializeField] private float dayNightCycleDuration = 120f;

        [Header("Lighting Colors")]
        [SerializeField] private Color dayColor = new Color(1f, 0.95f, 0.85f);
        [SerializeField] private Color nightColor = new Color(0.3f, 0.35f, 0.5f);
        [SerializeField] private Color alertColor = new Color(1f, 0.5f, 0.3f);

        [Header("Intensity Settings")]
        [SerializeField] private float dayIntensity = 1.2f;
        [SerializeField] private float nightIntensity = 0.3f;
        [SerializeField] private float alertIntensityMultiplier = 1.5f;

        private bool _isAlertMode;
        private float _originalIntensity;

        public float TimeOfDay => timeOfDay;
        public bool IsAlertMode => _isAlertMode;

        private void Update()
        {
        }

        public void SetAlertMode(bool alert)
        {
            _isAlertMode = alert;

            if (sceneLights == null) return;

            foreach (var light in sceneLights)
            {
                if (light == null) continue;

                if (alert)
                {
                    light.color = alertColor;
                    light.intensity *= alertIntensityMultiplier;
                }
                else
                {
                    light.color = GetTimeBasedColor();
                    light.intensity = GetTimeBasedIntensity();
                }
            }
        }

        private Color GetTimeBasedColor()
        {
            return Color.Lerp(nightColor, dayColor, timeOfDay);
        }

        private float GetTimeBasedIntensity()
        {
            return Mathf.Lerp(nightIntensity, dayIntensity, timeOfDay);
        }

        public void SetTimeOfDay(float time)
        {
            timeOfDay = Mathf.Clamp01(time);

            if (sceneLights == null || _isAlertMode) return;

            foreach (var light in sceneLights)
            {
                if (light == null) continue;

                light.color = GetTimeBasedColor();
                light.intensity = GetTimeBasedIntensity();
            }
        }

        public void EnableFlicker(int index)
        {
            if (lightFlickerObjects == null || index < 0 || index >= lightFlickerObjects.Length)
                return;

            var flicker = lightFlickerObjects[index]?.GetComponent<LightFlicker>();
            if (flicker != null)
                flicker.StartFlicker();
        }

        public void DisableFlicker(int index)
        {
            if (lightFlickerObjects == null || index < 0 || index >= lightFlickerObjects.Length)
                return;

            var flicker = lightFlickerObjects[index]?.GetComponent<LightFlicker>();
            if (flicker != null)
                flicker.StopFlicker();
        }

        public class LightFlicker : MonoBehaviour
        {
            [SerializeField] private float minIntensity = 0.2f;
            [SerializeField] private float maxIntensity = 1f;
            [SerializeField] private float flickerSpeed = 0.1f;

            private Light _light;
            private bool _isFlickering;
            private float _timer;

            private void Awake()
            {
                _light = GetComponent<Light>();
            }

            public void StartFlicker()
            {
                _isFlickering = true;
            }

            public void StopFlicker()
            {
                _isFlickering = false;
                if (_light != null)
                    _light.intensity = maxIntensity;
            }

            private void Update()
            {
                if (!_isFlickering || _light == null) return;

                _timer += Time.deltaTime;
                if (_timer >= flickerSpeed)
                {
                    _timer = 0f;
                    _light.intensity = Random.Range(minIntensity, maxIntensity);
                }
            }
        }
    }
}