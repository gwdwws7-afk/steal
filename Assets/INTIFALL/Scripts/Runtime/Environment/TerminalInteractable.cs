using INTIFALL.Data;
using INTIFALL.Growth;
using INTIFALL.Input;
using INTIFALL.Narrative;
using INTIFALL.System;
using INTIFALL.AI;
using UnityEngine;
using UnityEngine.UI;

namespace INTIFALL.Environment
{
    public struct TerminalHackCompletedEvent
    {
        public string terminalId;
        public string terminalDisplayName;
        public int levelIndex;
        public bool unlockedDoors;
        public bool suppressedAlerts;
    }

    public struct TerminalAlertSuppressedEvent
    {
        public string sourceTerminalId;
        public int levelIndex;
        public float durationSeconds;
        public Vector3 sourcePosition;
        public float effectRadiusMeters;
        public int waveId;
    }

    public struct TerminalHackStartedEvent
    {
        public string terminalId;
        public int levelIndex;
        public float expectedDurationSeconds;
    }

    public struct TerminalHackProgressEvent
    {
        public string terminalId;
        public int levelIndex;
        public float progress;
    }

    public struct TerminalHackCancelledEvent
    {
        public string terminalId;
        public int levelIndex;
        public float progressAtCancel;
    }

    [RequireComponent(typeof(Collider))]
    public class TerminalInteractable : MonoBehaviour
    {
        [Header("Terminal Identity")]
        [SerializeField] private string terminalId = "terminal_00";
        [SerializeField] private string terminalDisplayName = "Terminal";
        [SerializeField] private int levelIndex;
        [SerializeField] private string terminalSummary = string.Empty;
        [SerializeField] private string[] scriptedNarrativeTriggers = global::System.Array.Empty<string>();

        [Header("Hack Settings")]
        [SerializeField] private float baseHackDurationSeconds = 3f;
        [SerializeField] private bool cancelHackWhenPlayerLeavesRange = true;

        [Header("Terminal Effects")]
        [SerializeField] private bool suppressActiveAlerts = true;
        [SerializeField] private float suppressAlertDurationSeconds = 8f;
        [SerializeField] private float suppressAlertRadiusMeters = 18f;
        [SerializeField] private bool unlockLinkedDoors = true;
        [SerializeField] private bool clearLinkedLightingAlertMode = true;
        [SerializeField] private ElectronicDoor[] linkedDoors;
        [SerializeField] private LightingManager[] linkedLightingManagers;

        [Header("Prompt UI (Optional)")]
        [SerializeField] private Text promptText;
        [SerializeField] private string readyPrompt = "Press E to hack terminal";
        [SerializeField] private string hackingPrompt = "Hacking... {0:P0}";
        [SerializeField] private string completePrompt = "Terminal unlocked";

        [Header("Audio (Optional)")]
        [SerializeField] private AudioClip hackStartSound;
        [SerializeField] private AudioClip hackSuccessSound;
        [SerializeField] private AudioClip hackCancelSound;
        [SerializeField] private float progressEventGranularity = 0.05f;

        private bool _playerInRange;
        private bool _isHacking;
        private bool _isHacked;
        private Coroutine _hackRoutine;
        private float _hackProgress;
        private float _lastPublishedProgress;
        private AudioSource _audioSource;

        public string TerminalId => terminalId;
        public string TerminalDisplayName => terminalDisplayName;
        public int LevelIndex => levelIndex;
        public bool IsPlayerInRange => _playerInRange;
        public bool IsHacking => _isHacking;
        public bool IsHacked => _isHacked;

        private void Awake()
        {
            EnsureTriggerCollider();
            EnsureAudioSource();
            RefreshPrompt();
        }

        private void Reset()
        {
            EnsureTriggerCollider();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<InputManager.InteractEvent>(OnPlayerInteract);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InputManager.InteractEvent>(OnPlayerInteract);

            if (_hackRoutine != null)
            {
                StopCoroutine(_hackRoutine);
                _hackRoutine = null;
            }

            _isHacking = false;
            _hackProgress = 0f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            _playerInRange = true;
            RefreshPrompt();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            _playerInRange = false;
            if (_isHacking && cancelHackWhenPlayerLeavesRange)
                CancelHack();
            else
                RefreshPrompt();
        }

        public void Configure(string id, int index, string displayName, string summary = "", string[] triggerEvents = null)
        {
            terminalId = string.IsNullOrWhiteSpace(id) ? "terminal_00" : id.Trim();
            levelIndex = Mathf.Max(0, index);
            terminalDisplayName = string.IsNullOrWhiteSpace(displayName) ? terminalId : displayName.Trim();
            terminalSummary = string.IsNullOrWhiteSpace(summary) ? string.Empty : summary.Trim();
            scriptedNarrativeTriggers = triggerEvents != null
                ? (string[])triggerEvents.Clone()
                : global::System.Array.Empty<string>();
            RefreshPrompt();
        }

        public void ApplyLevelTuning(
            float hackDurationSeconds,
            float suppressDurationSeconds,
            float suppressRadiusMeters,
            bool suppressAlerts,
            bool unlockDoors,
            bool clearLightingAlertMode)
        {
            baseHackDurationSeconds = Mathf.Max(0.1f, hackDurationSeconds);
            suppressAlertDurationSeconds = Mathf.Max(0.1f, suppressDurationSeconds);
            suppressAlertRadiusMeters = Mathf.Max(1f, suppressRadiusMeters);
            suppressActiveAlerts = suppressAlerts;
            unlockLinkedDoors = unlockDoors;
            clearLinkedLightingAlertMode = clearLightingAlertMode;
        }

        public bool TryStartHack()
        {
            if (_isHacked || _isHacking || !_playerInRange)
                return false;

            float duration = ResolveHackDurationSeconds();
            _hackRoutine = StartCoroutine(HackRoutine());
            _lastPublishedProgress = 0f;
            EventBus.Publish(new TerminalHackStartedEvent
            {
                terminalId = terminalId,
                levelIndex = levelIndex,
                expectedDurationSeconds = duration
            });
            PublishHackProgress(0f, force: true);
            PlayOneShot(hackStartSound);
            return true;
        }

        public void CompleteHackImmediate()
        {
            if (_isHacked)
                return;

            if (_hackRoutine != null)
            {
                StopCoroutine(_hackRoutine);
                _hackRoutine = null;
            }

            _isHacking = false;
            _hackProgress = 1f;
            PublishHackProgress(1f, force: true);
            CompleteHack();
        }

        private void OnPlayerInteract(InputManager.InteractEvent evt)
        {
            if (!_playerInRange)
                return;

            TryStartHack();
        }

        private global::System.Collections.IEnumerator HackRoutine()
        {
            _isHacking = true;
            _hackProgress = 0f;
            RefreshPrompt();

            float duration = ResolveHackDurationSeconds();
            while (_hackProgress < 1f)
            {
                if (cancelHackWhenPlayerLeavesRange && !_playerInRange)
                {
                    CancelHack();
                    yield break;
                }

                _hackProgress += Time.deltaTime / duration;
                PublishHackProgress(_hackProgress);
                RefreshPrompt();
                yield return null;
            }

            _hackRoutine = null;
            _isHacking = false;
            _hackProgress = 1f;
            PublishHackProgress(1f, force: true);
            CompleteHack();
        }

        private float ResolveHackDurationSeconds()
        {
            float duration = Mathf.Max(0.1f, baseHackDurationSeconds);
            BloodlineSystem bloodline = Object.FindFirstObjectByType<BloodlineSystem>();
            if (bloodline == null)
                return duration;

            float speedMultiplier = Mathf.Max(0.1f, bloodline.GetTerminalHackSpeedMultiplier());
            return duration / speedMultiplier;
        }

        private void CompleteHack()
        {
            if (_isHacked)
                return;

            _isHacked = true;
            _isHacking = false;

            NarrativeManager narrative = NarrativeManager.Instance;
            if (narrative == null)
                narrative = Object.FindFirstObjectByType<NarrativeManager>();

            narrative?.ReadTerminal(
                terminalId,
                levelIndex,
                terminalDisplayName,
                terminalSummary,
                scriptedNarrativeTriggers);
            narrative?.CollectIntel(terminalId, levelIndex);

            EventBus.Publish(new IntelCollectedInSceneEvent
            {
                intelId = terminalId,
                levelIndex = levelIndex,
                intelType = EIntelType.TerminalDocument
            });

            bool unlockedAnyDoor = false;
            if (unlockLinkedDoors && linkedDoors != null)
            {
                for (int i = 0; i < linkedDoors.Length; i++)
                {
                    ElectronicDoor door = linkedDoors[i];
                    if (door == null)
                        continue;

                    door.Unlock();
                    unlockedAnyDoor = true;
                }
            }

            if (clearLinkedLightingAlertMode && linkedLightingManagers != null)
            {
                for (int i = 0; i < linkedLightingManagers.Length; i++)
                {
                    LightingManager lighting = linkedLightingManagers[i];
                    if (lighting == null)
                        continue;

                    lighting.SetAlertMode(false);
                }
            }

            if (suppressActiveAlerts)
            {
                int suppressionWaveId = EnemySquadCoordinator.NextWaveId();
                EventBus.Publish(new TerminalAlertSuppressedEvent
                {
                    sourceTerminalId = terminalId,
                    levelIndex = levelIndex,
                    durationSeconds = Mathf.Max(0.1f, suppressAlertDurationSeconds),
                    sourcePosition = transform.position,
                    effectRadiusMeters = Mathf.Max(1f, suppressAlertRadiusMeters),
                    waveId = suppressionWaveId
                });
            }

            EventBus.Publish(new TerminalHackCompletedEvent
            {
                terminalId = terminalId,
                terminalDisplayName = terminalDisplayName,
                levelIndex = levelIndex,
                unlockedDoors = unlockedAnyDoor,
                suppressedAlerts = suppressActiveAlerts
            });

            PlayOneShot(hackSuccessSound);
            RefreshPrompt();
        }

        private void CancelHack()
        {
            float progressAtCancel = Mathf.Clamp01(_hackProgress);
            if (_hackRoutine != null)
            {
                StopCoroutine(_hackRoutine);
                _hackRoutine = null;
            }

            _isHacking = false;
            _hackProgress = 0f;
            EventBus.Publish(new TerminalHackCancelledEvent
            {
                terminalId = terminalId,
                levelIndex = levelIndex,
                progressAtCancel = progressAtCancel
            });
            PlayOneShot(hackCancelSound);
            RefreshPrompt();
        }

        private void RefreshPrompt()
        {
            if (promptText == null)
                return;

            if (!_playerInRange)
            {
                promptText.text = string.Empty;
                return;
            }

            if (_isHacked)
            {
                promptText.text = completePrompt;
                return;
            }

            if (_isHacking)
            {
                promptText.text = string.Format(
                    string.IsNullOrWhiteSpace(hackingPrompt) ? "Hacking... {0:P0}" : hackingPrompt,
                    Mathf.Clamp01(_hackProgress));
                return;
            }

            promptText.text = readyPrompt;
        }

        private void EnsureTriggerCollider()
        {
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(1.2f, 1.8f, 1.2f);
                box.isTrigger = true;
                return;
            }

            col.isTrigger = true;
        }

        private void EnsureAudioSource()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (_audioSource == null || clip == null)
                return;

            _audioSource.PlayOneShot(clip);
        }

        private void PublishHackProgress(float progress, bool force = false)
        {
            float clamped = Mathf.Clamp01(progress);
            float granularity = Mathf.Clamp(progressEventGranularity, 0.01f, 0.5f);
            if (!force && Mathf.Abs(clamped - _lastPublishedProgress) < granularity)
                return;

            _lastPublishedProgress = clamped;
            EventBus.Publish(new TerminalHackProgressEvent
            {
                terminalId = terminalId,
                levelIndex = levelIndex,
                progress = clamped
            });
        }
    }
}
