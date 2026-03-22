using UnityEngine;
using UnityEngine.UI;
using INTIFALL.Level;
using INTIFALL.System;

namespace INTIFALL.UI
{
    public class MissionBriefingUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject briefingPanel;
        [SerializeField] private Text missionIdText;
        [SerializeField] private Text missionNameText;
        [SerializeField] private Text missionLevelText;
        [SerializeField] private Text backgroundText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text intelText;
        [SerializeField] private Text warningText;
        [SerializeField] private Text contactText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button cancelButton;

        [Header("Level Data")]
        [SerializeField] private LevelFlowManager levelFlowManager;

        [Header("Briefing Content")]
        [SerializeField] private string[] missionIds = new string[]
        {
            "OP-2247-001",
            "OP-2247-002",
            "OP-2247-003",
            "OP-2247-004",
            "OP-2247-005"
        };

        [SerializeField] private string[] missionNames = new string[]
        {
            "黄金雨",
            "档案迷宫",
            "黄金血脉",
            "Qhipu 核心",
            "太阳陨落"
        };

        [SerializeField] private string[] missionLevels = new string[]
        {
            "机密",
            "机密",
            "最高机密",
            "最高机密",
            "最高机密"
        };

        [SerializeField] private string[] backgrounds = new string[]
        {
            "反叛组织\"Ayllu astral\"在工业区活动增加。\n总部决定由你的小队执行清剿行动。",
            "Valleys 家族档案被发现在数据中心。\n总部命令你获取这些文件。",
            "Aya-Tech 实验室在秘密制造强化人。\n总部要求你摧毁这个设施。",
            "Qhipu 核心被发现在太阳殿深处。\n总部命令你回收这个古老的设备。",
            "终局之战。Qhipu 预言即将实现。\n你的选择将决定帝国的命运。"
        };

        [SerializeField] private string[] objectives = new string[]
        {
            "● 主要目标：抵达目标位置，消灭叛军据点\n● 次要目标：收集反叛组织情报",
            "● 主要目标：进入数据中心，获取 Valleys 档案\n● 次要目标：绕过安保系统",
            "● 主要目标：摧毁 Aya-Tech 实验室\n● 次要目标：获取实验证据",
            "● 主要目标：进入 Qhipu 核心室\n● 次要目标：激活核心",
            "● 主要目标：做出终局选择\n● 次要目标：无"
        };

        [SerializeField] private string[] warnings = new string[]
        {
            "目标区域有 5-6 名武装人员。\n工业区有通风管道系统，可利用。",
            "数据中心有重型防御。\n小心 Quipucamayoc 守卫。",
            "Aya-Tech 有强化士兵（Saqueos）。\n不建议正面冲突。",
            "Qhipu 核心室有祭司守卫。\n你的血统可能是关键。",
            "这是终局。\n三个选择，三种命运。没有回头路。"
        };

        private int _currentLevelIndex = -1;
        private bool _isDisplayed;

        public bool IsDisplayed => _isDisplayed;

        private void Awake()
        {
            if (levelFlowManager == null)
                levelFlowManager = FindObjectOfType<LevelFlowManager>();

            if (briefingPanel != null)
                briefingPanel.SetActive(false);
        }

        private void Start()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartMission);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancel);
        }

        public void ShowBriefing(int levelIndex)
        {
            if (_isDisplayed) return;
            if (levelIndex < 0 || levelIndex >= missionIds.Length) return;

            _currentLevelIndex = levelIndex;
            _isDisplayed = true;

            UpdateBriefingContent();

            if (briefingPanel != null)
                briefingPanel.SetActive(true);

            Time.timeScale = 0f;
        }

        private void UpdateBriefingContent()
        {
            if (missionIdText != null)
                missionIdText.text = $"【任务编号】{missionIds[_currentLevelIndex]}";

            if (missionNameText != null)
                missionNameText.text = $"【任务名称】{missionNames[_currentLevelIndex]}";

            if (missionLevelText != null)
                missionLevelText.text = $"【任务等级】{missionLevels[_currentLevelIndex]}";

            if (backgroundText != null)
                backgroundText.text = $"【背景】\n{backgrounds[_currentLevelIndex]}";

            if (objectiveText != null)
                objectiveText.text = $"【目标】\n{objectives[_currentLevelIndex]}";

            if (intelText != null)
                intelText.text = $"【情报】\n{GetIntelHint(_currentLevelIndex)}";

            if (warningText != null)
                warningText.text = $"【注意事项】\n{warnings[_currentLevelIndex]}";

            if (contactText != null)
                contactText.text = $"【联络人】Willa\n━━━━━━━━━━━━━━━━━━";
        }

        private string GetIntelHint(int levelIndex)
        {
            return levelIndex switch
            {
                0 => "目标区域有通风管道系统，可利用。",
                1 => "终端文档可能包含 Valleys 家族信息。",
                2 => "实验记录存储在主终端中。",
                3 => "Qhipu 石板碎片分散在区域各处。",
                4 => "终局选择将影响游戏结局。",
                _ => ""
            };
        }

        private void OnStartMission()
        {
            CloseBriefing();

            if (levelFlowManager != null)
            {
                levelFlowManager.SelectLevel(_currentLevelIndex);
                levelFlowManager.LoadSelectedLevel();
            }
            else
            {
                EventBus.Publish(new LevelSelectedEvent { levelIndex = _currentLevelIndex });
            }
        }

        private void OnCancel()
        {
            CloseBriefing();
        }

        public void CloseBriefing()
        {
            _isDisplayed = false;

            if (briefingPanel != null)
                briefingPanel.SetActive(false);

            Time.timeScale = 1f;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LevelSelectedEvent>(OnLevelSelected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LevelSelectedEvent>(OnLevelSelected);
        }

        private void OnLevelSelected(LevelSelectedEvent evt)
        {
            ShowBriefing(evt.levelIndex);
        }
    }
}