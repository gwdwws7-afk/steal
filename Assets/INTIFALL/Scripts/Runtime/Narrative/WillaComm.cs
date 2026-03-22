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

        [Header("Messages - Level 3")]
        [SerializeField] private string[] L03_MissionStart = new string[]
        {
            "Aya-Tech 实验室...帝国的阴影。\n\n他们在这里制造那些怪物——Saqueos。\n用活人做实验，将失败品变成听话的兵器。\n\n拿到证据，Killa。\n这些实验记录会摧毁帝国的神话。\n\n但要小心，那些怪物比普通士兵难对付得多。"
        };
        [SerializeField] private string[] L03_IntelFound = new string[]
        {
            "那是...第一代 Saqueos 的记录。\n\n他们用志愿者做实验。\n最初的想法是创造超级士兵...但代价太大了。\n\n皇帝知道这一切。\n他选择掩盖真相，继续制造怪物。\n\n你的血统...也许是对抗他们的关键。"
        };
        [SerializeField] private string[] L03_MissionComplete = new string[]
        {
            "证据拿到了。\n\n这会终结帝国的神话。\n用我们的方式——真相。\n\n下一站是太阳殿。\nQhipu 核心就在那里。\n\nKilla，你的血统将是打开它的钥匙。"
        };

        [Header("Messages - Level 4")]
        [SerializeField] private string[] L04_MissionStart = new string[]
        {
            "Qhipu 核心在太阳殿深处。\n\n完整的预言就存储在那里。\n关于太阳的陨落...关于你血统的真正命运。\n\n小心 Quipucamayoc——数据祭司。\n他们会不惜一切代价保护 Qhipu。\n\n你的血统会带你找到答案。"
        };
        [SerializeField] private string[] L04_IntelFound = new string[]
        {
            "你看到了...预言的真相。\n\n太阳陨落不是毁灭——是选择。\n三个终局，三种命运。\n\n而你，Valleys 的孩子，\n将是唯一能做出选择的人。"
        };
        [SerializeField] private string[] L04_MissionComplete = new string[]
        {
            "真相大白了。\n\n预言不是诅咒，是工具。\n可以用来毁灭，也可以用来解放。\n\n终局就在太阳殿的核心。\n那里...将是你的战场。\n\n无论你选择什么，Killa，我们都与你同在。"
        };

        [Header("Messages - Level 5")]
        [SerializeField] private string[] L05_MissionStart = new string[]
        {
            "这是终局，Killa。\n\n三个选择摆在面前：\n执行预言，让帝国在崩溃中重建；\n封印预言，让太阳永恒照耀；\n或篡改预言，让你的血统成为新的帝王。\n\n每个选择都有代价。\n每个选择都会改变一切。\n\n无论你选什么...我们都与你同在。"
        };
        [SerializeField] private string[] L05_StoryReveal = new string[]
        {
            "现在你知道了一切。\n\n关于你的血统，关于预言，关于这个帝国的真正本质。\n\n选择吧，Valleys 的孩子。\n太阳将根据你的意志陨落或升起。"
        };
        [SerializeField] private string[] L05_Warning = new string[]
        {
            "小心！敌人就在你身后！\n\n时间不多了，Killa。\n做出你的选择。"
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
                (2, EWillaTrigger.MissionStart) => L03_MissionStart,
                (2, EWillaTrigger.IntelFound) => L03_IntelFound,
                (2, EWillaTrigger.MissionComplete) => L03_MissionComplete,
                (3, EWillaTrigger.MissionStart) => L04_MissionStart,
                (3, EWillaTrigger.IntelFound) => L04_IntelFound,
                (3, EWillaTrigger.MissionComplete) => L04_MissionComplete,
                (4, EWillaTrigger.MissionStart) => L05_MissionStart,
                (4, EWillaTrigger.StoryReveal) => L05_StoryReveal,
                (4, EWillaTrigger.Warning) => L05_Warning,
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