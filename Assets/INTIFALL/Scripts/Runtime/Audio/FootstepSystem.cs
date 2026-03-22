using UnityEngine;
using INTIFALL.Player;

namespace INTIFALL.Audio
{
    public class FootstepSystem : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioClip[] walkFootsteps;
        [SerializeField] private AudioClip[] runFootsteps;
        [SerializeField] private AudioClip[] crouchFootsteps;
        [SerializeField] private AudioClip[] ropeFootsteps;
        [SerializeField] private AudioClip[] landFootsteps;

        [Header("Timing")]
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float runStepInterval = 0.3f;
        [SerializeField] private float crouchStepInterval = 0.8f;
        [SerializeField] private float ropeStepInterval = 0.4f;

        [Header("Volume")]
        [SerializeField] private float walkVolume = 0.3f;
        [SerializeField] private float runVolume = 0.5f;
        [SerializeField] private float crouchVolume = 0.1f;
        [SerializeField] private float ropeVolume = 0.2f;

        [Header("References")]
        [SerializeField] private Transform feetTransform;

        private PlayerController _playerController;
        private PlayerStateMachine _stateMachine;
        private AudioSource _footstepSource;
        private float _stepTimer;
        private bool _isMoving;

        private void Awake()
        {
            _footstepSource = GetComponent<AudioSource>();
            if (_footstepSource == null)
                _footstepSource = gameObject.AddComponent<AudioSource>();

            _footstepSource.playOnAwake = false;
            _footstepSource.loop = false;
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerController = player.GetComponent<PlayerController>();
                _stateMachine = player.GetComponent<PlayerStateMachine>();
            }

            if (feetTransform == null)
                feetTransform = transform;
        }

        private void Update()
        {
            if (_playerController == null || _stateMachine == null) return;

            _isMoving = _playerController.IsMoving;

            if (!_isMoving)
            {
                _stepTimer = 0f;
                return;
            }

            _stepTimer += Time.deltaTime;

            float interval = GetStepInterval();
            if (_stepTimer >= interval)
            {
                _stepTimer = 0f;
                PlayFootstep();
            }
        }

        private float GetStepInterval()
        {
            if (!_playerController.IsMoving) return float.MaxValue;

            return _stateMachine.CurrentState switch
            {
                EPlayerState.Walk => walkStepInterval,
                EPlayerState.Sprint => runStepInterval,
                EPlayerState.Crouch => crouchStepInterval,
                EPlayerState.Rope => ropeStepInterval,
                _ => walkStepInterval
            };
        }

        private void PlayFootstep()
        {
            AudioClip[] clips = GetFootstepClips();
            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            float volume = GetFootstepVolume();

            AudioManager.Instance?.PlaySFX(clip, volume);
        }

        private AudioClip[] GetFootstepClips()
        {
            if (_playerController == null) return walkFootsteps;

            if (_stateMachine.CurrentState == EPlayerState.Sprint)
                return runFootsteps.Length > 0 ? runFootsteps : walkFootsteps;
            else if (_stateMachine.CurrentState == EPlayerState.Crouch)
                return crouchFootsteps.Length > 0 ? crouchFootsteps : walkFootsteps;
            else if (_stateMachine.CurrentState == EPlayerState.Rope)
                return ropeFootsteps.Length > 0 ? ropeFootsteps : walkFootsteps;

            return walkFootsteps;
        }

        private float GetFootstepVolume()
        {
            if (_stateMachine == null || _playerController == null) return walkVolume;

            if (_stateMachine.CurrentState == EPlayerState.Sprint)
                return runVolume;
            else if (_stateMachine.CurrentState == EPlayerState.Crouch)
                return crouchVolume;
            else if (_stateMachine.CurrentState == EPlayerState.Rope)
                return ropeVolume;

            return walkVolume;
        }

        public void PlayLandingSound()
        {
            if (landFootsteps == null || landFootsteps.Length == 0) return;

            AudioClip clip = landFootsteps[Random.Range(0, landFootsteps.Length)];
            AudioManager.Instance?.PlaySFX(clip, runVolume);
        }
    }
}