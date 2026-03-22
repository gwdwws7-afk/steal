using UnityEngine;
using UnityEngine.UI;
using INTIFALL.System;

namespace INTIFALL.Narrative
{
    public enum EWillaTrigger
    {
        MissionStart,
        IntelFound,
        MissionComplete,
        StoryReveal,
        Warning,
        Betrayal
    }

    public struct WillaMessageEvent
    {
        public EWillaTrigger trigger;
        public int levelIndex;
        public string messageKey;
    }

    public class WillaComm : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject commPanel;
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Text messageText;
        [SerializeField] private Image commIcon;
        [SerializeField] private float typingSpeed = 0.05f;
        [SerializeField] private float autoCloseDelay = 5f;

        [Header("Audio")]
        [SerializeField] private AudioClip incomingCommSound;
        [SerializeField] private AudioClip commEndSound;

        [Header("Messages - Level 1")]
        [SerializeField] private string[] L01_MissionStart = new string[]
        {
            "Killa，我知道你被派去工业区镇压反叛者。\n\n但我要你完成任务后，在目标区域找一个石板。\n一个刻着古印加克丘亚语的石板。\n\n不要问为什么。\n不要交给任何人。\n完成后，在旧港 7 号仓库找我。\n\n我们等了你很久了，Valleys 的孩子。"
        };
        [SerializeField] private string[] L01_IntelFound = new string[]
        {
            "你找到了。\n\n那是 Qhipu 石板，古代用来存储数据的。\n你能感受到它，对吗？和你的血统共振。\n\n你看到的是什么？\n画面？声音？\n\n无论如何，不要让帝国得到它。\n如果被发现了...你就是叛逃者了。\n\n做你该做的事，Killa。"
        };
        [SerializeField] private string[] L01_MissionComplete = new string[]
        {
            "你做到了。\n\n现在是叛逃者了。\n来旧港 7 号仓库，秘密通道在左边第三个集装箱后面。\n\n我会解释一切。\n关于你的血统，关于这个帝国，关于 Qhipu...\n\n快点，帝国的追兵已经在路上了。"
        };

        [Header("Messages - Level 2")]
        [SerializeField] private string[] L02_MissionStart = new string[]
        {
            "Valleys 家族档案里有你的过去。\n\n我们截获了消息，帝国将家族纹章封存在数据中心。\n那是你们家族曾经辉煌的证明...也是他们流放你们的原因。\n\n找到它，Killa。\n你会明白为什么他们害怕你的血统。"
        };
        [SerializeField] private string[] L02_IntelFound = new string[]
        {
            "那是你们家族的纹章...\n\n他们流放了你。\n十二家族投票，认定 Valleys 血脉是威胁。\n但你知道吗？\n\n正是这个血脉，包含着对抗 Qhipu 的密钥。\n\n继续前进。你需要知道全部真相。"
        };
        [SerializeField] private string[] L02_MissionComplete = new string[]
        {
            "你看到了真相。\n\nValleys 从不是叛徒。\n是帝国篡改了历史，将失败归咎于你们的家族。\n\n你的血统不是诅咒。\n它是古老预言的钥匙。\n\n下一站：Aya-Tech 实验室。\n那里有帝国最黑暗的秘密。"
        };

        private bool _isDisplaying;
        private bool _isTyping;
        private float _displayTimer;
        private AudioSource _audioSource;

        public bool IsDisplaying => _isDisplaying;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            if (commPanel != null)
                commPanel.SetActive(false);
        }

        private void Update()
        {
            if (_isDisplaying && !_isTyping)
            {
                _displayTimer += Time.deltaTime;
                if (_displayTimer >= autoCloseDelay)
                {
                    CloseComm();
                }
            }
        }

        public void TriggerComm(EWillaTrigger trigger, int levelIndex)
        {
            string[] messages = GetMessagesForTrigger(trigger, levelIndex);
            if (messages == null || messages.Length == 0) return;

            string message = messages[Random.Range(0, messages.Length)];
            DisplayMessage(trigger, levelIndex, message);
        }

        private string[] GetMessagesForTrigger(EWillaTrigger trigger, int levelIndex)
        {
            return (levelIndex, trigger) switch
            {
                (0, EWillaTrigger.MissionStart) => L01_MissionStart,
                (0, EWillaTrigger.IntelFound) => L01_IntelFound,
                (0, EWillaTrigger.MissionComplete) => L01_MissionComplete,
                (1, EWillaTrigger.MissionStart) => L02_MissionStart,
                (1, EWillaTrigger.IntelFound) => L02_IntelFound,
                (1, EWillaTrigger.MissionComplete) => L02_MissionComplete,
                _ => GetDefaultMessages(trigger)
            };
        }

        private string[] GetDefaultMessages(EWillaTrigger trigger)
        {
            return trigger switch
            {
                EWillaTrigger.MissionStart => new string[] { "专注于任务，Killa。" },
                EWillaTrigger.Warning => new string[] { "小心，Killa。危险临近。" },
                EWillaTrigger.Betrayal => new string[] { "情况有变，保持警惕。" },
                _ => new string[] { "继续前进。" }
            };
        }

        private void DisplayMessage(EWillaTrigger trigger, int levelIndex, string message)
        {
            if (_isDisplaying) return;

            _isDisplaying = true;
            _isTyping = true;
            _displayTimer = 0f;

            if (commPanel != null)
                commPanel.SetActive(true);

            if (speakerNameText != null)
                speakerNameText.text = "[鸦 - 加密频道]";

            if (messageText != null)
                messageText.text = "";

            if (incomingCommSound != null)
                _audioSource.PlayOneShot(incomingCommSound);

            StartCoroutine(TypeMessage(message));
        }

        private System.Collections.IEnumerator TypeMessage(string message)
        {
            _isTyping = true;

            if (messageText != null)
            {
                foreach (char c in message)
                {
                    messageText.text += c;
                    yield return new WaitForSeconds(typingSpeed);
                }
            }

            _isTyping = false;

            EventBus.Publish(new WillaMessageEvent
            {
                trigger = EWillaTrigger.MissionStart,
                levelIndex = 0,
                messageKey = message
            });
        }

        public void CloseComm()
        {
            if (!_isDisplaying) return;

            _isDisplaying = false;
            _isTyping = false;
            _displayTimer = 0f;

            if (commPanel != null)
                commPanel.SetActive(false);

            if (commEndSound != null)
                _audioSource.PlayOneShot(commEndSound);
        }

        public void SkipTyping()
        {
            if (!_isTyping) return;
            StopAllCoroutines();
            _isTyping = false;
        }
    }
}