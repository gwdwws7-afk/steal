using UnityEngine;

namespace INTIFALL.Audio
{
    public class AmbientManager : MonoBehaviour
    {
        [Header("Ambient Layers")]
        [SerializeField] private AudioClip[] baseAmbience;
        [SerializeField] private AudioClip[] weatherAmbience;
        [SerializeField] private AudioClip[] timeOfDayAmbience;

        [Header("Zone Ambience")]
        [SerializeField] private AudioClip[] industrialAmbience;
        [SerializeField] private AudioClip[] indoorAmbience;
        [SerializeField] private AudioClip[] exteriorAmbience;

        [Header("Alert States")]
        [SerializeField] private AudioClip normalAlertAmbient;
        [SerializeField] private AudioClip cautionAlertAmbient;
        [SerializeField] private AudioClip dangerAlertAmbient;
        [SerializeField] private AudioClip combatAlertAmbient;

        [Header("Settings")]
        [SerializeField] private float crossfadeDuration = 2f;
        [SerializeField] private float volumeLerpSpeed = 1f;

        private AudioSource _ambientSource1;
        private AudioSource _ambientSource2;
        private AudioClip _currentAmbient;
        private AudioClip _targetAmbient;
        private float _currentVolume = 0f;
        private float _targetVolume = 0f;
        private bool _useSource1 = true;

        public static AmbientManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _ambientSource1 = gameObject.AddComponent<AudioSource>();
            _ambientSource1.loop = true;
            _ambientSource1.playOnAwake = false;

            _ambientSource2 = gameObject.AddComponent<AudioSource>();
            _ambientSource2.loop = true;
            _ambientSource2.playOnAwake = false;
        }

        private void Update()
        {
            if (Mathf.Abs(_currentVolume - _targetVolume) > 0.01f)
            {
                _currentVolume = Mathf.Lerp(_currentVolume, _targetVolume, Time.deltaTime * volumeLerpSpeed);

                if (_useSource1)
                    _ambientSource1.volume = _currentVolume;
                else
                    _ambientSource2.volume = _currentVolume;
            }
        }

        public void PlayBaseAmbience(int index = 0)
        {
            if (baseAmbience == null || baseAmbience.Length == 0) return;
            if (index < 0 || index >= baseAmbience.Length) return;

            CrossfadeToAmbient(baseAmbience[index]);
        }

        public void PlayWeatherAmbience(int index = 0)
        {
            if (weatherAmbience == null || weatherAmbience.Length == 0) return;
            if (index < 0 || index >= weatherAmbience.Length) return;

            CrossfadeToAmbient(weatherAmbience[index]);
        }

        public void PlayZoneAmbience(EZoneType zone)
        {
            AudioClip[] clips = zone switch
            {
                EZoneType.Industrial => industrialAmbience,
                EZoneType.Indoor => indoorAmbience,
                EZoneType.Exterior => exteriorAmbience,
                _ => baseAmbience
            };

            if (clips == null || clips.Length == 0) return;

            CrossfadeToAmbient(clips[Random.Range(0, clips.Length)]);
        }

        public void SetAlertAmbient(EAlertLevel alertLevel)
        {
            AudioClip clip = alertLevel switch
            {
                EAlertLevel.Normal => normalAlertAmbient,
                EAlertLevel.Caution => cautionAlertAmbient,
                EAlertLevel.Danger => dangerAlertAmbient,
                EAlertLevel.Combat => combatAlertAmbient,
                _ => normalAlertAmbient
            };

            if (clip != null)
                CrossfadeToAmbient(clip);
        }

        private void CrossfadeToAmbient(AudioClip clip)
        {
            if (clip == null) return;
            if (_currentAmbient == clip) return;

            _targetAmbient = clip;

            if (_useSource1)
            {
                _ambientSource2.clip = clip;
                _ambientSource2.volume = 0f;
                _ambientSource2.Play();
                _targetVolume = AudioManager.Instance?.AmbientVolume ?? 0.3f;

                StartCoroutine(CrossfadeSources());
            }
            else
            {
                _ambientSource1.clip = clip;
                _ambientSource1.volume = 0f;
                _ambientSource1.Play();
                _targetVolume = AudioManager.Instance?.AmbientVolume ?? 0.3f;

                StartCoroutine(CrossfadeSources());
            }

            _useSource1 = !_useSource1;
            _currentAmbient = clip;
        }

        private System.Collections.IEnumerator CrossfadeSources()
        {
            float elapsed = 0f;
            float duration = crossfadeDuration;

            AudioSource fadingSource = _useSource1 ? _ambientSource1 : _ambientSource2;
            AudioSource risingSource = _useSource1 ? _ambientSource2 : _ambientSource1;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                fadingSource.volume = Mathf.Lerp(_currentVolume, 0f, t);
                risingSource.volume = Mathf.Lerp(_currentVolume, _targetVolume, t);

                yield return null;
            }

            fadingSource.Stop();
        }

        public void StopAmbient()
        {
            _targetVolume = 0f;
            _ambientSource1.Stop();
            _ambientSource2.Stop();
        }

        public enum EZoneType
        {
            Industrial,
            Indoor,
            Exterior
        }

        public enum EAlertLevel
        {
            Normal,
            Caution,
            Danger,
            Combat
        }
    }
}