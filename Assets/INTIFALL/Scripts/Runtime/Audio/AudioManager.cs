using UnityEngine;
using INTIFALL.System;

namespace INTIFALL.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource[] sfxSources = new AudioSource[4];
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource voiceSource;

        [Header("Volume Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float ambientVolume = 0.3f;
        [Range(0f, 1f)]
        [SerializeField] private float voiceVolume = 1f;

        [Header("Pool Settings")]
        [SerializeField] private int sfxPoolSize = 8;

        private System.Collections.Generic.Queue<AudioSource> _sfxPool;

        public float MasterVolume => masterVolume;
        public float SFXVolume => sfxVolume;
        public float MusicVolume => musicVolume;
        public float AmbientVolume => ambientVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSFXPool();
        }

        private void InitializeSFXPool()
        {
            _sfxPool = new System.Collections.Generic.Queue<AudioSource>();
            for (int i = 0; i < sfxPoolSize; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.volume = sfxVolume;
                _sfxPool.Enqueue(source);
            }
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetSFXSource();
            if (source == null) return;

            source.PlayOneShot(clip, volumeScale * sfxVolume);
            _sfxPool.Enqueue(source);
        }

        private AudioSource GetSFXSource()
        {
            if (_sfxPool.Count > 0)
                return _sfxPool.Dequeue();

            if (sfxSources.Length > 0)
                return sfxSources[0];

            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.loop = false;
            return newSource;
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        public void PlayAmbient(AudioClip clip, bool loop = true)
        {
            if (ambientSource == null || clip == null) return;

            ambientSource.clip = clip;
            ambientSource.loop = loop;
            ambientSource.volume = ambientVolume;
            ambientSource.Play();
        }

        public void StopAmbient()
        {
            if (ambientSource != null)
                ambientSource.Stop();
        }

        public void PlayVoice(AudioClip clip)
        {
            if (voiceSource == null || clip == null) return;

            voiceSource.clip = clip;
            voiceSource.volume = voiceVolume;
            voiceSource.Play();
        }

        public void StopVoice()
        {
            if (voiceSource != null)
                voiceSource.Stop();
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            AudioListener.volume = masterVolume;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            foreach (var source in sfxSources)
            {
                if (source != null)
                    source.volume = sfxVolume;
            }
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            if (ambientSource != null)
                ambientSource.volume = ambientVolume;
        }

        public void MuteAll(bool mute)
        {
            AudioListener.pause = mute;
        }
    }
}