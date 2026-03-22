# INTI FALL — 技术架构设计文档
## Tech Design Document (TDD)

---

## TDD 目录

- [T1. 项目架构总览](#t1-项目架构总览)
- [T2. 核心系统架构](#t2-核心系统架构)
- [T3. 敌人 AI 架构](#t3-敌人-ai-架构)
- [T4. 工具系统架构](#t4-工具系统架构)
- [T5. UI/HUD 架构](#t5-UI-HUD-架构)
- [T6. 存档系统](#t6-存档系统)
- [T7. 关卡管理系统](#t7-关卡管理系统)
- [T8. AI 美术资产 Pipeline](#t8-AI-美术资产-pipeline)
- [T9. 开发工作流](#t9-开发工作流)

---

## T1. 项目架构总览

### T1.1 项目结构

```
Assets/
│
├── _Project/
│   ├── Scripts/
│   │   ├── Core/                    # 核心系统
│   │   │   ├── GameManager.cs       # 全局管理
│   │   │   ├── GameState.cs         # 游戏状态机
│   │   │   ├── SaveManager.cs       # 存档管理
│   │   │   └── EventBus.cs          # 事件总线
│   │   │
│   │   ├── Player/                  # 玩家系统
│   │   │   ├── PlayerController.cs  # 玩家控制器
│   │   │   ├── PlayerMovement.cs    # 移动系统
│   │   │   ├── PlayerStealth.cs     # 潜行系统
│   │   │   ├── PlayerCombat.cs      # 战斗系统
│   │   │   ├── PlayerTools.cs       # 工具系统
│   │   │   ├── PlayerRope.cs         # 绳技系统
│   │   │   ├── StaminaSystem.cs     # 耐力系统
│   │   │   └── BloodlinePassive.cs  # 血统被动
│   │   │
│   │   ├── Enemy/                   # 敌人系统
│   │   │   ├── AI/
│   │   │   │   ├── EnemyAIController.cs
│   │   │   │   ├── EnemyStateMachine.cs
│   │   │   │   ├── EnemyVision.cs
│   │   │   │   ├── EnemyHearing.cs
│   │   │   │   ├── EnemyCommunication.cs
│   │   │   │   ├── EnemyPatrol.cs
│   │   │   │   └── EnemyType/
│   │   │   │       ├── NormalSoldier.cs
│   │   │   │       ├── EnhancedSoldier.cs
│   │   │   │       ├── HeavySoldier.cs
│   │   │   │       ├── Quipucamayoc.cs
│   │   │   │       └── Saqueos.cs
│   │   │   ├── RadioManager.cs      # 敌人通信管理
│   │   │   └── EnemySpawner.cs      # 敌人生成
│   │   │
│   │   ├── Tools/                   # 工具系统
│   │   │   ├── ToolBase.cs          # 工具基类
│   │   │   ├── SmokeGrenade.cs
│   │   │   ├── Flashbang.cs
│   │   │   ├── EMP.cs
│   │   │   ├── SoundBait.cs
│   │   │   ├── Drone.cs
│   │   │   ├── TimedNoise.cs
│   │   │   ├── SleepDart.cs
│   │   │   ├── WallBreak.cs
│   │   │   ├── VentSystem.cs
│   │   │   ├── LightControl.cs
│   │   │   └── HackingTerminal.cs
│   │   │
│   │   ├── Level/                  # 关卡系统
│   │   │   ├── LevelManager.cs
│   │   │   ├── Checkpoint.cs
│   │   │   ├── NavigationPoint.cs
│   │   │   ├── SupplyCrate.cs
│   │   │   └── ExitPoint.cs
│   │   │
│   │   ├── Narrative/              # 叙事系统
│   │   │   ├── NarrativeManager.cs
│   │   │   ├── WillaComm.cs
│   │   │   ├── QhipuVision.cs
│   │   │   ├── TerminalDocument.cs
│   │   │   └── BloodlineResonance.cs
│   │   │
│   │   ├── UI/                     # UI 系统
│   │   │   ├── HUDManager.cs
│   │   │   ├── EagleEyeUI.cs       # 鹰眼 UI
│   │   │   ├── AlertIndicator.cs   # 警戒状态 HUD
│   │   │   ├── ToolBarUI.cs        # 工具栏 UI
│   │   │   ├── ArmorSmithUI.cs     # 军械库 UI
│   │   │   └── MissionBriefingUI.cs # 任务简报 UI
│   │   │
│   │   ├── Environment/            # 环境系统
│   │   │   ├── LightingManager.cs
│   │   │   ├── VentEntrance.cs
│   │   │   ├── HangingPoint.cs
│   │   │   ├── BreakableWall.cs
│   │   │   ├── SurveillanceCamera.cs
│   │   │   └── ElectronicDoor.cs
│   │   │
│   │   └── Audio/                  # 音频系统
│   │       ├── AudioManager.cs
│   │       ├── FootstepSystem.cs
│   │       └── AmbientManager.cs
│   │
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── Enemy/
│   │   │   └── Types/             # 5 种敌人类型 Prefab
│   │   ├── Tools/                  # 12 种工具 Prefab
│   │   ├── Environment/
│   │   └── UI/
│   │
│   ├── ScriptableObjects/
│   │   ├── GameConfig.asset       # 全局配置
│   │   ├── LevelData/             # 每关关卡数据
│   │   │   ├── Level_01.asset
│   │   │   ├── Level_02.asset
│   │   │   ├── Level_03.asset
│   │   │   ├── Level_04.asset
│   │   │   └── Level_05.asset
│   │   ├── EnemyData/             # 敌人数值配置
│   │   ├── ToolData/              # 工具数值配置
│   │   ├── NarrativeData/         # 叙事内容数据
│   │   └── BloodlineData.asset    # 血统被动配置
│   │
│   └── Settings/
│       ├── Input.actions          # Input System 配置
│       └── AudioMixer.mixer       # 音频混音器
│
├── Art/
│   ├── Characters/
│   │   ├── Player/
│   │   └── Enemies/
│   ├── Environments/
│   │   ├── Level_01/
│   │   ├── Level_02/
│   │   ├── Level_03/
│   │   ├── Level_04/
│   │   └── Level_05/
│   ├── Tools/
│   ├── Effects/
│   └── UI/
│
├── Audio/
│   ├── SFX/
│   ├── Music/
│   └── Voice/
│
└── Scenes/
    ├── MainMenu.unity
    ├── Level_01.unity
    ├── Level_02.unity
    ├── Level_03.unity
    ├── Level_04.unity
    ├── Level_05.unity
    └── GameOver.unity
```

---

## T2. 核心系统架构

### T2.1 GameManager

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("游戏状态")]
    public GameState currentState;
    public int currentLevel = 1;
    public int credit = 0;
    public PlayerSaveData playerSave;
    
    [Header("系统引用")]
    public UIManager ui;
    public AudioManager audio;
    public SaveManager saveManager;
    public NarrativeManager narrative;
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void StartGame()
    {
        currentState = GameState.MainMenu;
        saveManager.LoadGame();
        LoadLevel(1);
    }
    
    public void LoadLevel(int levelIndex)
    {
        currentLevel = levelIndex;
        currentState = GameState.Loading;
        
        // 异步加载关卡
        StartCoroutine(AsyncLoadLevel(levelIndex));
    }
    
    IEnumerator AsyncLoadLevel(int index)
    {
        var op = SceneManager.LoadSceneAsync($"Level_{index:00}");
        yield return op;
        
        // 初始化关卡
        LevelManager.Instance.Initialize();
        
        currentState = GameState.Briefing;
        NarrativeManager.Instance.ShowBriefing();
    }
    
    public void StartInfiltration()
    {
        currentState = GameState.Infiltration;
        LevelManager.Instance.StartLevel();
        PlayerController.Instance.EnableControl();
    }
    
    public void MissionComplete(EvaluationGrade grade)
    {
        currentState = GameState.Result;
        int reward = CalculateReward(grade);
        credit += reward;
        saveManager.SaveGame();
        UIManager.Instance.ShowResult(grade, reward);
    }
    
    public void MissionFailed()
    {
        currentState = GameState.GameOver;
        UIManager.Instance.ShowGameOver();
    }
}

public enum GameState
{
    MainMenu,
    Loading,
    Briefing,
    Infiltration,
    Combat,
    Result,
    GameOver
}

public enum EvaluationGrade { S, A, B, C }
```

### T2.2 EventBus（事件总线）

```csharp
public static class EventBus
{
    // 事件类型
    public static readonly EventDef<EnemyState> OnEnemyStateChanged = new();
    public static readonly EventDef<PlayerState> OnPlayerStateChanged = new();
    public static readonly EventDef<Vector3, float> OnSoundEmitted = new();
    public static readonly EventDef<int> OnLevelCompleted = new();
    public static readonly EventDef<Vector3> OnQhipuTriggered = new();
    public static readonly EventDef<ToolUsedEvent> OnToolUsed = new();
    public static readonly EventDef<AlertLevel> OnAlertLevelChanged = new();
    
    // 简洁的事件定义
    public class EventDef<T>
    {
        private List<Action<T>> listeners = new();
        
        public void Subscribe(Action<T> callback) => listeners.Add(callback);
        public void Unsubscribe(Action<T> callback) => listeners.Remove(callback);
        public void Publish(T data) => listeners.ForEach(a => a(data));
    }
}

// 使用示例
// 敌人状态变化时通知所有订阅者
EventBus.OnEnemyStateChanged.Publish(new EnemyState { enemy = this, state = EnemyState.Alerted });

// 玩家订阅
EventBus.OnAlertLevelChanged.Subscribe(OnAlertChanged);
void OnAlertChanged(AlertLevel level) { /* 更新 HUD */ }
```

### T2.3 PlayerController

```csharp
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    
    [Header("子系统")]
    public PlayerMovement movement;
    public PlayerStealth stealth;
    public PlayerCombat combat;
    public PlayerTools tools;
    public PlayerRope rope;
    
    [Header("状态")]
    public bool controlEnabled = false;
    public int currentHP = 3;
    public int maxHP = 3;
    
    void Update()
    {
        if (!controlEnabled) return;
        
        movement.HandleInput();
        tools.HandleInput();
        
        if (Input.GetKeyDown(KeyCode.E)) TryInteract();
        if (Input.GetKeyDown(KeyCode.Q)) TryStealthKill();
        if (Input.GetKeyDown(KeyCode.C)) ToggleCrouch();
        if (Input.GetKeyDown(KeyCode.Space)) combat.TryDodge();
    }
    
    public void EnableControl() => controlEnabled = true;
    public void DisableControl() => controlEnabled = false;
    
    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        CombatFeedback.Instance.ShowDamage();
        
        if (currentHP <= 0)
        {
            if (HasEmergencyKit())
            {
                UseEmergencyKit();
            }
            else
            {
                GameManager.Instance.MissionFailed();
            }
        }
    }
    
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Environment"))
        {
            stealth.OnCollision(col);
        }
    }
}
```

---

## T3. 敌人 AI 架构

### T3.1 状态机架构

```csharp
// 敌人状态接口
public interface IEnemyState
{
    void Enter(EnemyAIController enemy);
    void Update(EnemyAIController enemy);
    void Exit(EnemyAIController enemy);
    EnemyState GetStateType();
}

// 未察觉状态
public class IdleState : IEnemyState
{
    public EnemyState GetStateType() => EnemyState.Idle;
    
    public void Enter(EnemyAIController enemy) { }
    
    public void Update(EnemyAIController enemy)
    {
        enemy.Patrol();
        
        if (enemy.CheckVision()) enemy.SetState(new AlertedState());
        else if (enemy.CheckHearing()) enemy.SetState(new SuspiciousState());
    }
    
    public void Exit(EnemyAIController enemy) { }
}

// 怀疑状态
public class SuspiciousState : IEnemyState
{
    float timer = 0f;
    
    public EnemyState GetStateType() => EnemyState.Suspicious;
    
    public void Enter(EnemyAIController enemy)
    {
        enemy.Stop();
        enemy.LookAt(enemy.lastSuspiciousDirection);
        enemy.PlayAudio("suspicious");
    }
    
    public void Update(EnemyAIController enemy)
    {
        timer += Time.deltaTime;
        
        if (enemy.CheckVision())
        {
            enemy.SetState(new AlertedState());
            return;
        }
        
        if (timer >= enemy.suspiciousDuration)
        {
            enemy.SetState(new SearchingState());
        }
    }
    
    public void Exit(EnemyAIController enemy) { }
}

// 搜索状态
public class SearchingState : IEnemyState
{
    float timer = 0f;
    
    public EnemyState GetStateType() => EnemyState.Searching;
    
    public void Enter(EnemyAIController enemy)
    {
        enemy.MoveToSuspiciousPoint();
        enemy.PlayAudio("searching");
    }
    
    public void Update(EnemyAIController enemy)
    {
        timer += Time.deltaTime;
        
        if (enemy.CheckVision())
        {
            enemy.SetState(new AlertedState());
            return;
        }
        
        if (enemy.ReachedDestination())
        {
            enemy.ReturnToPatrol();
            enemy.SetState(new IdleState());
        }
        
        if (timer >= enemy.searchDuration)
        {
            enemy.ReturnToPatrol();
            enemy.SetState(new IdleState());
        }
    }
    
    public void Exit(EnemyAIController enemy) { }
}

// 警戒状态
public class AlertedState : IEnemyState
{
    float timer = 0f;
    
    public EnemyState GetStateType() => EnemyState.Alerted;
    
    public void Enter(EnemyAIController enemy)
    {
        enemy.LockOn();
        enemy.PlayAudio("alert");
        RadioManager.Instance.BroadcastAlert(enemy.transform.position);
    }
    
    public void Update(EnemyAIController enemy)
    {
        if (enemy.CheckVision())
        {
            enemy.ChasePlayer();
            timer = 0f;
        }
        else
        {
            enemy.LoseSight();
            enemy.SetState(new SearchingState());
            return;
        }
        
        timer += Time.deltaTime;
        if (timer >= enemy.alertDuration)
        {
            enemy.SetState(new FullAlertState());
        }
    }
    
    public void Exit(EnemyAIController enemy) { }
}

// 全面警报状态
public class FullAlertState : IEnemyState
{
    float timer = 0f;
    
    public EnemyState GetStateType() => EnemyState.FullAlert;
    
    public void Enter(EnemyAIController enemy)
    {
        enemy.BroadcastFullAlert();
        enemy.PlayAudio("full_alert");
    }
    
    public void Update(EnemyAIController enemy)
    {
        if (enemy.CheckVision()) enemy.ChasePlayer();
        else enemy.SearchInZone();
        
        timer += Time.deltaTime;
        if (timer >= enemy.fullAlertDuration)
        {
            GameManager.Instance.MissionFailed();
        }
    }
    
    public void Exit(EnemyAIController enemy) { }
}

// 状态机主体
public class EnemyAIController : MonoBehaviour
{
    IEnemyState currentState;
    Dictionary<EnemyState, IEnemyState> statePool = new();
    
    void Awake()
    {
        // 注册所有状态
        statePool[EnemyState.Idle] = new IdleState();
        statePool[EnemyState.Suspicious] = new SuspiciousState();
        statePool[EnemyState.Searching] = new SearchingState();
        statePool[EnemyState.Alerted] = new AlertedState();
        statePool[EnemyState.FullAlert] = new FullAlertState();
        
        currentState = statePool[EnemyState.Idle];
        currentState.Enter(this);
    }
    
    public void SetState(IEnemyState newState)
    {
        currentState.Exit(this);
        currentState = newState;
        currentState.Enter(this);
        EventBus.OnEnemyStateChanged.Publish(new EnemyState { enemy = this, state = currentState.GetStateType() });
    }
    
    void Update()
    {
        currentState.Update(this);
    }
}
```

### T3.2 感知模块

```csharp
public class EnemyVision : MonoBehaviour
{
    public float distance = 15f;
    public float angle = 60f;
    public LayerMask obstacleLayer;
    
    public bool CanSeePlayer(Transform player, float multiplier = 1f)
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, dir);
        float dist = Vector3.Distance(transform.position, player.position);
        
        if (angleToPlayer > GetCurrentAngle() * multiplier) return false;
        if (dist > GetCurrentDistance() * multiplier) return false;
        if (Physics.Raycast(transform.position, dir, dist, obstacleLayer)) return false;
        
        // 阴影检测
        if (LightingManager.Instance.GetLuxAt(player.position) < 30f)
            return false;
        
        return true;
    }
    
    float GetCurrentDistance() => distance * stateMultiplier[(int)currentState];
    float GetCurrentAngle() => angle * stateMultiplier[(int)currentState];
    
    static float[] stateMultiplier = { 1f, 1.3f, 1.5f, 2f, 2f };
}

public class EnemyHearing : MonoBehaviour
{
    public float walkRange = 5f;
    public float runRange = 12f;
    public float toolRange = 8f;
    
    public bool CanHearPlayer(Transform player)
    {
        float dist = Vector3.Distance(transform.position, player.position);
        
        if (PlayerIsRunning() && dist <= runRange) return true;
        if (PlayerIsWalking() && dist <= walkRange) return true;
        if (PlayerUsedTool() && dist <= toolRange) return true;
        
        return false;
    }
}
```

---

## T4. 工具系统架构

### T4.1 工具基类

```csharp
public abstract class ToolBase : MonoBehaviour
{
    public string toolName;
    public int slotSize = 1;
    public Sprite icon;
    public float cooldown = 0f;
    public float currentCooldown = 0f;
    
    protected PlayerController player;
    
    public virtual void Initialize()
    {
        player = PlayerController.Instance;
    }
    
    public virtual void Use()
    {
        if (currentCooldown > 0) return;
        if (!CanUse()) return;
        
        OnUse();
        currentCooldown = cooldown;
    }
    
    protected abstract void OnUse();
    protected abstract bool CanUse();
    
    void Update()
    {
        if (currentCooldown > 0)
            currentCooldown -= Time.deltaTime;
    }
}

public class SmokeGrenade : ToolBase
{
    public float radius = 4f;
    public float duration = 8f;
    
    protected override void OnUse()
    {
        Vector3 pos = transform.position + transform.forward * 3f;
        var effect = PoolManager.Spawn("SmokeEffect", pos, Quaternion.identity);
        effect.GetComponent<SmokeEffect>().Initialize(radius, duration);
        
        // 通知所有敌人
        EventBus.OnToolUsed.Publish(new ToolUsedEvent { tool = this, position = pos });
    }
    
    protected override bool CanUse()
    {
        return player.GetRemainingSlots() >= slotSize;
    }
}
```

### T4.2 工具管理

```csharp
public class ToolManager : MonoBehaviour
{
    public static ToolManager Instance { get; private set; }
    
    [Header("工具栏")]
    public ToolBase[] slots = new ToolBase[4];
    public int selectedSlot = 0;
    
    [Header("当前工具")]
    public ToolBase currentTool;
    
    void Update()
    {
        // 数字键切换
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        
        // 鼠标滚轮切换
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0) SelectNext();
        if (scroll < 0) SelectPrev();
        
        // 鼠标右键使用
        if (Input.GetMouseButtonDown(1)) currentTool?.Use();
    }
    
    public void SelectSlot(int index)
    {
        if (slots[index] == null) return;
        selectedSlot = index;
        currentTool = slots[index];
        UIManager.Instance.toolBarUI.SelectSlot(index);
    }
    
    public void EquipTool(ToolBase tool, int slot)
    {
        slots[slot] = tool;
        tool.Initialize();
        if (slot == selectedSlot) currentTool = tool;
    }
}
```

---

## T5. UI/HUD 架构

### T5.1 HUDManager

```csharp
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }
    
    [Header("HUD 组件")]
    public AlertIndicator alertIndicator;
    public EagleEyeUI eagleEyeUI;
    public ToolBarUI toolBarUI;
    public StaminaBar staminaBar;
    public MiniMap miniMap;
    public InteractionPrompt interactionPrompt;
    
    [Header("状态 HUD")]
    public GameObject stealthHUDElements;
    public GameObject combatHUDElements;
    
    void Awake()
    {
        Instance = this;
    }
    
    public void ShowStealthHUD()
    {
        stealthHUDElements.SetActive(true);
        combatHUDElements.SetActive(false);
    }
    
    public void ShowCombatHUD()
    {
        stealthHUDElements.SetActive(false);
        combatHUDElements.SetActive(true);
    }
    
    public void UpdateAlertLevel(AlertLevel level)
    {
        alertIndicator.SetLevel(level);
        
        if (level >= AlertLevel.Alerted)
            ShowCombatHUD();
        else
            ShowStealthHUD();
    }
}

public enum AlertLevel
{
    Unaware,     // 绿色
    Suspicious,  // 黄色
    Searching,   // 橙色
    Alerted,    // 红色
    FullAlert   // 闪烁红色
}
```

### T5.2 鹰眼 UI

```csharp
public class EagleEyeUI : MonoBehaviour
{
    public Camera eagleEyeCamera;
    public RenderTexture outputTexture;
    public RawImage displayImage;
    
    [Header("显示内容")]
    public bool showEnemyVisionCone = true;
    public bool showPatrolPath = true;
    public bool showInteractable = true;
    
    float cooldown = 3f;
    float currentCooldown = 0f;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && currentCooldown <= 0)
        {
            ActivateEagleEye();
            currentCooldown = cooldown;
        }
        
        if (currentCooldown > 0)
            currentCooldown -= Time.deltaTime;
    }
    
    void ActivateEagleEye()
    {
        // 扫描所有敌人
        foreach (var enemy in EnemyManager.Instance.enemies)
        {
            enemy.ShowVisionCone(true);
            enemy.ShowPatrolPath(showPatrolPath);
        }
        
        // 显示所有可交互物
        foreach (var point in FindObjectsOfType<Interactable>())
        {
            point.Highlight(showInteractable);
        }
        
        StartCoroutine(DeactivateAfter(2f));
    }
    
    IEnumerator DeactivateAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        foreach (var enemy in EnemyManager.Instance.enemies)
        {
            enemy.ShowVisionCone(false);
            enemy.ShowPatrolPath(false);
        }
    }
}
```

---

## T6. 存档系统

### T6.1 SaveManager

```csharp
[System.Serializable]
public class PlayerSaveData
{
    public int currentLevel;           // 已完成最高关卡
    public int totalCredit;            // 累计信用点
    public int[] toolUpgrades;        // 工具升级状态
    public bool[] bloodlineUnlocked;   // 血统被动解锁
    public int[] collectiblesFound;   // 已收集情报
    public EvaluationGrade[] levelGrades; // 每关评价
    public int endingChoosen;         // 终局选择
}

public class SaveManager
{
    private const string SAVE_KEY = "INTI_FALL_SAVE";
    
    public void SaveGame()
    {
        var data = new PlayerSaveData
        {
            currentLevel = GameManager.Instance.currentLevel,
            totalCredit = GameManager.Instance.credit,
            toolUpgrades = GetToolUpgradeState(),
            bloodlineUnlocked = GetBloodlineState(),
            collectiblesFound = GetCollectiblesState(),
            levelGrades = GetGradesState(),
            endingChoosen = GetEndingState()
        };
        
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }
    
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            // 新游戏
            GameManager.Instance.credit = 0;
            GameManager.Instance.currentLevel = 1;
            return;
        }
        
        string json = PlayerPrefs.GetString(SAVE_KEY);
        var data = JsonUtility.FromJson<PlayerSaveData>(json);
        
        ApplySaveData(data);
    }
    
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
    }
}
```

---

## T7. 关卡管理系统

### T7.1 LevelManager

```csharp
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    
    [Header("关卡数据")]
    public LevelData levelData;
    public int currentZoneIndex = 0;
    
    [Header("生成")]
    public List<EnemyAIController> spawnedEnemies = new();
    public List<Interactable> interactables = new();
    
    [Header("状态")]
    public bool levelStarted = false;
    public bool missionComplete = false;
    
    void Awake()
    {
        Instance = this;
    }
    
    public void Initialize()
    {
        currentZoneIndex = 0;
        missionComplete = false;
        
        // 生成敌人
        SpawnEnemies();
        
        // 生成交互物
        SetupInteractables();
        
        // 设置补给点
        SetupSupplyCrates();
        
        // 注册检查点
        RegisterCheckpoints();
        
        // 初始化照明
        LightingManager.Instance.Initialize(levelData.lightingConfig);
        
        EventBus.OnLevelCompleted.Subscribe(OnLevelComplete);
    }
    
    public void StartLevel()
    {
        levelStarted = true;
        
        // 激活所有敌人 AI
        foreach (var enemy in spawnedEnemies)
            enemy.EnableAI();
        
        // 激活交互物
        foreach (var obj in interactables)
            obj.Activate();
    }
    
    void SpawnEnemies()
    {
        foreach (var spawnData in levelData.enemySpawns)
        {
            var enemy = EnemySpawner.Spawn(spawnData);
            spawnedEnemies.Add(enemy);
        }
    }
    
    public void OnZoneEntered(int zoneIndex)
    {
        currentZoneIndex = zoneIndex;
        
        // 触发区域事件
        if (levelData.zoneEvents.TryGetValue(zoneIndex, out var evt))
        {
            evt.Invoke();
        }
    }
    
    public EvaluationGrade EvaluateMission()
    {
        int score = 100;
        
        if (alertTriggered) score -= 20;
        if (enemyKilled > 0) score -= 15 * enemyKilled;
        if (damageTaken > 0) score -= 10 * damageTaken;
        if (timeLimitExceeded) score -= 10;
        if (!secondaryObjectivesComplete) score -= 10;
        
        if (score >= 95) return EvaluationGrade.S;
        if (score >= 80) return EvaluationGrade.A;
        if (score >= 60) return EvaluationGrade.B;
        return EvaluationGrade.C;
    }
}
```

---

## T8. AI 美术资产 Pipeline

### T8.1 角色资产 Pipeline

```
Step 1: Meshy AI 生成
  ↓
  输入: "INTI FALL - Killa Incan Valle - Low-poly Game-ready"
  Meshy 设置: Stylized / High Detail / FBX
  输出: Killa_BaseMesh.fbx
  
Step 2: 导入 Unity 后处理
  ↓
  2.1 LOD 分级
      - LOD0: 5K 面（主视角）
      - LOD1: 2K 面（远距离）
      - LOD2: 1K 面（超远/群体）
      
  2.2 骨骼绑定（可选）
      - 如需动画，用 Mixamo Retargeter
      - 推荐骨骼：Humanoid
      
  2.3 动画重定向
      - Mixamo 动作库
      - 关键动作：Idle / Walk / Run / Crouch / Stealth Kill / Roll / Hang
      
Step 3: 材质设置
  ↓
  3.1 Shader: URP/Lit
  3.2 调色板强制:
      - Primary: #1a1a3e (深靛蓝)
      - Secondary: #d4a017 (金色)
      - Accent: #b87333 (铜色)
  3.3 贴图规格:
      - Albedo: 2048x2048
      - Normal: 2048x2048
      - Metallic: 1024x1024
      
Step 4: Prefab 组装
  ↓
  Killa_Prefab
  ├── MeshRenderer (LOD0)
  ├── Animator (动画控制器)
  ├── PlayerController.cs
  ├── Colliders
  └── VFX_Holder (特效挂点)
```

### T8.2 场景资产 Pipeline

```
Step 1: Meshy / 传统 DCC 生成基础模块
  ↓
  场景模块类型:
  - 地板/墙壁单元 (模块化设计，1m/2m/4m 网格)
  - 道具 (箱子/机械/终端)
  - 装饰 (管道/电线/涂鸦)
  
Step 2: 风格统一规范
  ↓
  场景调色板:
  - 地板: #2a2a3e (暗蓝灰)
  - 墙壁: #3a3a4e (中蓝灰)
  - 金属: #4a4a5e (亮蓝灰)
  - 霓虹: #ff4500 / #00ffff
  - 黄金装饰: #d4a017
  - 神圣元素: #f5f5f5
  
Step 3: 场景构建
  ↓
  Unity ProBuilder 或 Tiled Editor
  遵循关卡 Layout 的网格系统
  出口/入口与 Layout 对齐
```

---

## T9. 开发工作流

### T9.1 开发阶段

| 阶段 | 周期 | 目标 | 交付物 |
|---|---|---|---|
| **Proto 0** | 1-2 周 | 角色移动 + 单个敌人 AI + 烟雾弹 | 可运行 Demo |
| **Proto 1** | 4-6 周 | 全部敌人 AI + 全部工具 + 鹰眼 UI | 可玩 Demo |
| **Proto 2** | 4-6 周 | 战斗系统 + 经济系统 + HP | 系统 Demo |
| **Proto 3** | 3-4 周 | 成长系统 + 叙事通讯 | 内容 Demo |
| **关卡 01** | 4-6 周 | 完整第一关 + 美术资产 | 首个完整关卡 |
| **关卡 02-05** | 8-12 周 | 剩余 4 关 | 5 关完整内容 |
| **打磨** | 4-6 周 | 平衡 + Bug fix + 优化 | Release Candidate |

### T9.2 Git 分支策略

```
main (稳定分支)
  ├── dev (开发集成分支)
  │     ├── feature/player-movement
  │     ├── feature/enemy-ai
  │     ├── feature/tools
  │     ├── feature/combat
  │     ├── feature/level-01
  │     ├── feature/level-02
  │     ├── feature/level-03
  │     ├── feature/level-04
  │     ├── feature/level-05
  │     ├── feature/narrative
  │     └── feature/ui
  └── release (发布分支)
```

### T9.3 代码审查清单

每个 PR 必须满足：
- [ ] 无编译错误
- [ ] 所有公共方法有 XML 注释
- [ ] 使用 EventBus 而非直接引用通信
- [ ] 所有数值使用 ScriptableObject 配置
- [ ] 所有敌人类型继承自 EnemyAIController
- [ ] 所有工具继承自 ToolBase
- [ ] 无空引用异常风险
- [ ] 性能：避免每帧 GC 分配

---

*文档版本：v0.1 | 状态：技术架构完成*
