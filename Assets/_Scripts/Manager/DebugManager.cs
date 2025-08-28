using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unity 게임용 통합 디버그 로그 관리 시스템 (이벤트 기반)
/// - 인스펙터에서 실시간 설정 제어
/// - 설정 변경 시 즉시 반영
/// - 플레이 모드에서 실시간 토글 가능
/// </summary>
public class DebugManager : MonoBehaviour
{
    #region Singleton Pattern
    private static DebugManager instance;
    public static DebugManager Instance
    {
        get
        {
            if (instance == null)
            {
                // 기존에 있는 DebugManager 찾기
                instance = FindObjectOfType<DebugManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DebugManager");
                    instance = go.AddComponent<DebugManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    #endregion

    #region 이벤트 시스템
    /// <summary>설정 변경 이벤트들</summary>
    public static System.Action<bool> OnMasterSwitchChanged;
    public static System.Action<LogCategory, bool> OnCategoryToggled;
    public static System.Action OnSettingsChanged;
    #endregion

    #region 인스펙터 설정 (실시간 반영)
    [Header("전역 설정")]
    [SerializeField] private bool _masterSwitch = true;
    [SerializeField] private LogLevel _globalLogLevel = LogLevel.Info;
    [SerializeField] private bool _useColors = true;
    [SerializeField] private bool _showTimestamp = false;
    [SerializeField] private int _maxLogsPerSecond = 10;

    [Header("시스템별 로그 설정")]
    [SerializeField] private bool _enableCardSystem = true;
    [SerializeField] private bool _enableSkillSystem = true;
    [SerializeField] private bool _enableCombatSystem = true;
    [SerializeField] private bool _enablePoolSystem = false;
    [SerializeField] private bool _enableSpawnSystem = false;
    [SerializeField] private bool _enableUISystem = false;
    [SerializeField] private bool _enablePlayerSystem = true;
    [SerializeField] private bool _enableGameSystem = true;

    [Header("세부 설정")]
    [SerializeField] private DetailedSettings _detailSettings = new DetailedSettings();
    #endregion

    #region 이전 값들 (변경 감지용)
    private bool prevMasterSwitch;
    private bool prevCardSystem;
    private bool prevSkillSystem;
    private bool prevCombatSystem;
    private bool prevPoolSystem;
    private bool prevSpawnSystem;
    private bool prevUISystem;
    private bool prevPlayerSystem;
    private bool prevGameSystem;
    #endregion

    #region 프로퍼티들 (외부 접근용)
    public bool masterSwitch => _masterSwitch;
    public LogLevel globalLogLevel => _globalLogLevel;
    public bool useColors => _useColors;
    public bool showTimestamp => _showTimestamp;
    public int maxLogsPerSecond => _maxLogsPerSecond;
    public bool enableCardSystem => _enableCardSystem;
    public bool enableSkillSystem => _enableSkillSystem;
    public bool enableCombatSystem => _enableCombatSystem;
    public bool enablePoolSystem => _enablePoolSystem;
    public bool enableSpawnSystem => _enableSpawnSystem;
    public bool enableUISystem => _enableUISystem;
    public bool enablePlayerSystem => _enablePlayerSystem;
    public bool enableGameSystem => _enableGameSystem;
    public DetailedSettings detailSettings => _detailSettings;
    #endregion

    #region Private Fields
    private Dictionary<string, int> logCounts = new Dictionary<string, int>();
    private float lastResetTime;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // 초기값 저장
        StorePreviousValues();
    }

    void Update()
    {
        // 설정 변경 감지 및 이벤트 발생
        CheckForSettingsChanges();

        // 1초마다 로그 카운터 리셋 (스팸 방지)
        if (Time.time - lastResetTime > 1f)
        {
            logCounts.Clear();
            lastResetTime = Time.time;
        }
    }
    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void OnValidate()
    {
        // 인스펙터에서 값이 변경될 때 즉시 반영
        if (Application.isPlaying && instance == this)
        {
            CheckForSettingsChanges();
        }
    }
    #endregion

    #region 설정 변경 감지
    /// <summary>인스펙터 설정 변경을 감지하고 이벤트 발생</summary>
    private void CheckForSettingsChanges()
    {
        bool anyChanged = false;

        // 마스터 스위치 체크
        if (prevMasterSwitch != _masterSwitch)
        {
            OnMasterSwitchChanged?.Invoke(_masterSwitch);
            prevMasterSwitch = _masterSwitch;
            anyChanged = true;
        }

        // 각 시스템 설정 체크
        if (prevCardSystem != _enableCardSystem)
        {
            OnCategoryToggled?.Invoke(LogCategory.Card, _enableCardSystem);
            prevCardSystem = _enableCardSystem;
            anyChanged = true;
        }

        if (prevSkillSystem != _enableSkillSystem)
        {
            OnCategoryToggled?.Invoke(LogCategory.Skill, _enableSkillSystem);
            prevSkillSystem = _enableSkillSystem;
            anyChanged = true;
        }

        if (prevCombatSystem != _enableCombatSystem)
        {
            OnCategoryToggled?.Invoke(LogCategory.Combat, _enableCombatSystem);
            prevCombatSystem = _enableCombatSystem;
            anyChanged = true;
        }

        if (prevPoolSystem != _enablePoolSystem)
        {
            OnCategoryToggled?.Invoke(LogCategory.Pool, _enablePoolSystem);
            prevPoolSystem = _enablePoolSystem;
            anyChanged = true;
        }

        if (prevSpawnSystem != _enableSpawnSystem)
        {
            OnCategoryToggled?.Invoke(LogCategory.Spawn, _enableSpawnSystem);
            prevSpawnSystem = _enableSpawnSystem;
            anyChanged = true;
        }

        if (prevUISystem != _enableUISystem)
        {
            OnCategoryToggled?.Invoke(LogCategory.UI, _enableUISystem);
            prevUISystem = _enableUISystem;
            anyChanged = true;
        }

        if (prevPlayerSystem != _enablePlayerSystem)
        {
            OnCategoryToggled?.Invoke(LogCategory.Player, _enablePlayerSystem);
            prevPlayerSystem = _enablePlayerSystem;
            anyChanged = true;
        }

        if (prevGameSystem != _enableGameSystem)
        {
            OnCategoryToggled?.Invoke(LogCategory.Game, _enableGameSystem);
            prevGameSystem = _enableGameSystem;
            anyChanged = true;
        }

        if (anyChanged)
        {
            OnSettingsChanged?.Invoke();
        }
    }

    /// <summary>현재 설정값들을 저장</summary>
    private void StorePreviousValues()
    {
        prevMasterSwitch = _masterSwitch;
        prevCardSystem = _enableCardSystem;
        prevSkillSystem = _enableSkillSystem;
        prevCombatSystem = _enableCombatSystem;
        prevPoolSystem = _enablePoolSystem;
        prevSpawnSystem = _enableSpawnSystem;
        prevUISystem = _enableUISystem;
        prevPlayerSystem = _enablePlayerSystem;
        prevGameSystem = _enableGameSystem;
    }
    #endregion

    #region 외부 설정 변경 메서드
    /// <summary>외부에서 설정을 변경할 때 사용</summary>
    public void SetMasterSwitch(bool value)
    {
        _masterSwitch = value;
    }

    public void SetCategoryEnabled(LogCategory category, bool enabled)
    {
        switch (category)
        {
            case LogCategory.Card: _enableCardSystem = enabled; break;
            case LogCategory.Skill: _enableSkillSystem = enabled; break;
            case LogCategory.Combat: _enableCombatSystem = enabled; break;
            case LogCategory.Pool: _enablePoolSystem = enabled; break;
            case LogCategory.Spawn: _enableSpawnSystem = enabled; break;
            case LogCategory.UI: _enableUISystem = enabled; break;
            case LogCategory.Player: _enablePlayerSystem = enabled; break;
            case LogCategory.Game: _enableGameSystem = enabled; break;
        }
    }
    #endregion

    #region Core Logging Methods (기존과 동일)
    public static void Log(LogCategory category, string message, LogLevel level = LogLevel.Info)
    {
        if (!Instance || !Instance.masterSwitch) return;
        if (level < Instance.globalLogLevel) return;
        if (!Instance.IsCategoryEnabled(category)) return;

        // 스팸 방지 체크
        string key = $"{category}_{level}";
        if (!Instance.logCounts.ContainsKey(key))
            Instance.logCounts[key] = 0;

        Instance.logCounts[key]++;
        if (Instance.logCounts[key] > Instance.maxLogsPerSecond)
            return;

        string formattedMessage = Instance.FormatMessage(category, message, level);

        // Unity Console에 출력
        switch (level)
        {
            case LogLevel.None:
                return;
            case LogLevel.Error:
                Debug.LogError(formattedMessage);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(formattedMessage);
                break;
            default:
                Debug.Log(formattedMessage);
                break;
        }
    }
    #endregion

    #region System-Level Log Methods (기존과 동일)
    public static void LogCard(string message) => Log(LogCategory.Card, message);
    public static void LogSkill(string message) => Log(LogCategory.Skill, message);
    public static void LogCombat(string message) => Log(LogCategory.Combat, message);
    public static void LogUI(string message) => Log(LogCategory.UI, message);
    public static void LogPool(string message) => Log(LogCategory.Pool, message);
    public static void LogSpawn(string message) => Log(LogCategory.Spawn, message);
    public static void LogPlayer(string message) => Log(LogCategory.Player, message);
    public static void LogSystem(string message) => Log(LogCategory.System, message);
    public static void LogGame(string message) => Log(LogCategory.Game, message);
    public static void LogError(LogCategory category, string message) => Log(category, message, LogLevel.Error);
    public static void LogWarning(LogCategory category, string message) => Log(category, message, LogLevel.Warning);

    public static void LogImportant(string message)
    {
        if (!Instance || !Instance.masterSwitch) return;
        Debug.Log($"<color=yellow><b>★ {message}</b></color>");
    }

    public static void LogSeparator(string title = "")
    {
        if (!Instance || !Instance.masterSwitch) return;
        if (string.IsNullOrEmpty(title))
            Debug.Log("════════════════════════════════════════");
        else
            Debug.Log($"═══════════ {title} ═══════════");
    }
    #endregion

    #region Detail Log Methods (기존 메서드들 유지)
    // 스킬 시스템 세부 로그
    public static void LogSkillExecution(string message)
    {
        if (Instance?.detailSettings.skillExecution == true)
            LogSkill($"[Execution] {message}");
    }

    public static void LogSkillCooldown(string message)
    {
        if (Instance?.detailSettings.skillCooldown == true)
            LogSkill($"[Cooldown] {message}");
    }

    public static void LogSkillDamage(string message)
    {
        if (Instance?.detailSettings.skillDamage == true)
            LogSkill($"[Damage] {message}");
    }

    public static void LogSkillEffects(string message)
    {
        if (Instance?.detailSettings.skillEffects == true)
            LogSkill($"[Effects] {message}");
    }

    public static void LogSkillManagement(string message)
    {
        if (Instance?.detailSettings.skillManagement == true)
            LogSkill($"[Management] {message}");
    }

    public static void LogSkillInitialization(string message)
    {
        if (Instance?.detailSettings.skillInitialization == true)
            LogSkill($"[Init] {message}");
    }

    public static void LogSkillLevelUp(string message)
    {
        if (Instance?.detailSettings.skillLevelUp == true)
            LogSkill($"[LevelUp] {message}");
    }

    public static void LogSkillStats(string message)
    {
        if (Instance?.detailSettings.skillStats == true)
            LogSkill($"[Stats] {message}");
    }

    // 전투 시스템 세부 로그
    public static void LogEnemyDamage(string message)
    {
        if (Instance?.detailSettings.enemyDamage == true)
            LogCombat($"[EnemyDamage] {message}");
    }

    public static void LogPlayerDamage(string message)
    {
        if (Instance?.detailSettings.playerDamage == true)
            LogCombat($"[PlayerDamage] {message}");
    }

    public static void LogPassiveEffects(string message)
    {
        if (Instance?.detailSettings.passiveEffects == true)
            LogCombat($"[Passive] {message}");
    }

    public static void LogAutoSkillCasting(string message)
    {
        if (Instance?.detailSettings.autoSkillCasting == true)
            LogCombat($"[AutoSkill] {message}");
    }

    public static void LogTargetingSystem(string message)
    {
        if (Instance?.detailSettings.targetingSystem == true)
            LogCombat($"[Targeting] {message}");
    }

    public static void LogSkillCooldowns(string message)
    {
        if (Instance?.detailSettings.skillCooldowns == true)
            LogCombat($"[Cooldown] {message}");
    }

    public static void LogMultiCastSystem(string message)
    {
        if (Instance?.detailSettings.multiCastSystem == true)
            LogCombat($"[MultiCast] {message}");
    }

    // 카드 시스템 세부 로그
    public static void LogCardSelection(string message)
    {
        if (Instance?.detailSettings.cardSelection == true)
            LogCard($"[Selection] {message}");
    }

    public static void LogCardEffectApply(string message)
    {
        if (Instance?.detailSettings.cardEffectApply == true)
            LogCard($"[Effect] {message}");
    }

    public static void LogCardDisplay(string message)
    {
        if (Instance?.detailSettings.cardDisplay == true)
            LogCard($"[Display] {message}");
    }

    public static void LogCardInventory(string message)
    {
        if (Instance?.detailSettings.cardInventory == true)
            LogCard($"[Inventory] {message}");
    }

    // 플레이어 시스템 세부 로그
    public static void LogPlayerMovement(string message)
    {
        if (Instance?.detailSettings.playerMovement == true)
            LogPlayer($"[Movement] {message}");
    }

    public static void LogPlayerInput(string message)
    {
        if (Instance?.detailSettings.playerInput == true)
            LogPlayer($"[Input] {message}");
    }

    public static void LogPlayerStats(string message)
    {
        if (Instance?.detailSettings.playerStats == true)
            LogPlayer($"[Stats] {message}");
    }

    public static void LogPlayerHealth(string message)
    {
        if (Instance?.detailSettings.playerHealth == true)
            LogPlayer($"[Health] {message}");
    }

    public static void LogPlayerExperience(string message)
    {
        if (Instance?.detailSettings.playerExperience == true)
            LogPlayer($"[XP] {message}");
    }

    public static void LogPlayerComponents(string message)
    {
        if (Instance?.detailSettings.playerComponents == true)
            LogPlayer($"[Component] {message}");
    }

    // 게임 시스템 세부 로그
    public static void LogGameState(string message)
    {
        if (Instance?.detailSettings.gameStateChanges == true)
            LogGame($"[State] {message}");
    }

    public static void LogExperience(string message)
    {
        if (Instance?.detailSettings.experienceSystem == true)
            LogGame($"[XP] {message}");
    }

    public static void LogLevelUp(string message)
    {
        if (Instance?.detailSettings.levelUpSystem == true)
            LogGame($"[Level] {message}");
    }

    public static void LogTimeline(string message)
    {
        if (Instance?.detailSettings.timelineProgress == true)
            LogGame($"[Timeline] {message}");
    }

    public static void LogDebugCommand(string message)
    {
        if (Instance?.detailSettings.debugCommands == true)
            LogGame($"[Debug] {message}");
    }

    // 오브젝트 풀링 시스템 세부 로그
    public static void LogPoolCreation(string message)
    {
        if (Instance?.detailSettings.poolCreation == true)
            LogPool($"[Creation] {message}");
    }

    public static void LogPoolExpansion(string message)
    {
        if (Instance?.detailSettings.poolExpansion == true)
            LogPool($"[Expansion] {message}");
    }

    public static void LogPoolReturn(string message)
    {
        if (Instance?.detailSettings.poolReturn == true)
            LogPool($"[Return] {message}");
    }

    public static void LogPoolStatus(string message)
    {
        if (Instance?.detailSettings.poolStatus == true)
            LogPool($"[Status] {message}");
    }

    // 적 시스템 세부 로그
    public static void LogEnemySpawn(string message)
    {
        if (Instance?.detailSettings.enemySpawn == true)
            LogSpawn($"[Enemy] {message}");
    }

    public static void LogEnemyDeath(string message)
    {
        if (Instance?.detailSettings.enemyDeath == true)
            LogCombat($"[Death] {message}");
    }

    public static void LogEnemyBehavior(string message)
    {
        if (Instance?.detailSettings.enemyBehavior == true)
            LogCombat($"[AI] {message}");
    }

    public static void LogEnemyTargeting(string message)
    {
        if (Instance?.detailSettings.enemyTargeting == true)
            LogCombat($"[Targeting] {message}");
    }

    // 스폰 시스템 세부 로그
    public static void LogSpawnRate(string message)
    {
        if (Instance?.detailSettings.spawnRate == true)
            LogSpawn($"[Rate] {message}");
    }

    public static void LogSpawnPosition(string message)
    {
        if (Instance?.detailSettings.spawnPosition == true)
            LogSpawn($"[Position] {message}");
    }

    public static void LogSpawnWave(string message)
    {
        if (Instance?.detailSettings.spawnWave == true)
            LogSpawn($"[Wave] {message}");
    }
    #endregion

    #region Private Helper Methods
    private bool IsCategoryEnabled(LogCategory category)
    {
        switch (category)
        {
            case LogCategory.Card: return enableCardSystem;
            case LogCategory.Skill: return enableSkillSystem;
            case LogCategory.Combat: return enableCombatSystem;
            case LogCategory.UI: return enableUISystem;
            case LogCategory.Pool: return enablePoolSystem;
            case LogCategory.Spawn: return enableSpawnSystem;
            case LogCategory.Player: return enablePlayerSystem;
            case LogCategory.Game: return enableGameSystem;
            case LogCategory.System: return true;
            default: return true;
        }
    }

    private string FormatMessage(LogCategory category, string message, LogLevel level)
    {
        string prefix = "";

        if (showTimestamp)
            prefix += $"[{Time.time:F2}] ";

        string categoryStr = $"[{category}]";

        if (useColors)
        {
            string color = GetColor(category, level);
            return $"<color={color}>{prefix}{categoryStr} {message}</color>";
        }

        return $"{prefix}{categoryStr} {message}";
    }

    private string GetColor(LogCategory category, LogLevel level)
    {
        if (level == LogLevel.Error) return "red";
        if (level == LogLevel.Warning) return "yellow";

        switch (category)
        {
            case LogCategory.Card: return "cyan";
            case LogCategory.Skill: return "lime";
            case LogCategory.Combat: return "orange";
            case LogCategory.UI: return "magenta";
            case LogCategory.Pool: return "gray";
            case LogCategory.Spawn: return "white";
            case LogCategory.Player: return "lightblue";
            case LogCategory.System: return "blue";
            case LogCategory.Game: return "yellow";
            default: return "white";
        }
    }
    #endregion

    #region Context Menu Quick Actions (기존과 동일)
    [ContextMenu("모든 로그 켜기")]
    public void EnableAllLogs()
    {
        _masterSwitch = true;
        _enableCardSystem = true;
        _enableSkillSystem = true;
        _enableCombatSystem = true;
        _enablePoolSystem = true;
        _enableSpawnSystem = true;
        _enableUISystem = true;
        _enablePlayerSystem = true;
        _enableGameSystem = true;
    }

    [ContextMenu("모든 로그 끄기")]
    public void DisableAllLogs()
    {
        _masterSwitch = false;
    }

    [ContextMenu("전투 관련만 켜기")]
    public void EnableOnlyCombat()
    {
        DisableAllLogs();
        _masterSwitch = true;
        _enableCombatSystem = true;
        _enableSkillSystem = true;
    }

    [ContextMenu("플레이어 관련만 켜기")]
    public void EnableOnlyPlayer()
    {
        DisableAllLogs();
        _masterSwitch = true;
        _enablePlayerSystem = true;
    }

    [ContextMenu("성능 관련 끄기")]
    public void DisablePerformanceHeavy()
    {
        _enablePoolSystem = false;
        _enableSpawnSystem = false;
        _detailSettings.poolStatus = false;
        _detailSettings.spawnPosition = false;
        _detailSettings.enemyBehavior = false;
    }
    #endregion
}

#region Enums and Classes (기존과 동일)
public enum LogCategory
{
    Card, Skill, Combat, UI, Pool, Spawn, Player, System, Game
}

public enum LogLevel
{
    None, Error, Warning, Info, Debug
}

[System.Serializable]
public class DetailedSettings
{
    [Header("스킬 시스템 세부")]
    public bool skillExecution = true;
    public bool skillCooldown = true;
    public bool skillDamage = true;
    public bool skillEffects = true;
    public bool skillManagement = true;
    public bool skillInitialization = true;
    public bool skillLevelUp = true;
    public bool skillStats = true;

    [Header("전투 시스템 세부")]
    public bool enemyDamage = true;
    public bool playerDamage = true;
    public bool passiveEffects = true;
    public bool autoSkillCasting = true;
    public bool targetingSystem = true;
    public bool skillCooldowns = true;
    public bool multiCastSystem = true;

    [Header("카드 시스템 세부")]
    public bool cardSelection = true;
    public bool cardEffectApply = true;
    public bool cardDisplay = true;
    public bool cardInventory = true;
    public bool cardReward = true;

    [Header("플레이어 시스템 세부")]
    public bool playerMovement = true;
    public bool playerInput = false;
    public bool playerStats = true;
    public bool playerHealth = true;
    public bool playerExperience = false;
    public bool playerComponents = false;

    [Header("게임 시스템 세부")]
    public bool gameStateChanges = true;
    public bool experienceSystem = true;
    public bool levelUpSystem = true;
    public bool timelineProgress = false;
    public bool debugCommands = true;
    public bool waveSystem = true;
    public bool scoreSystem = true;
    public bool saveLoadSystem = true;

    [Header("풀링 시스템 세부")]
    public bool poolCreation = true;
    public bool poolExpansion = true;
    public bool poolReturn = true;
    public bool poolStatus = false;

    [Header("적 시스템 세부")]
    public bool enemySpawn = true;
    public bool enemyDeath = true;
    public bool enemyBehavior = false;
    public bool enemyTargeting = true;

    [Header("스폰 시스템 세부")]
    public bool spawnRate = true;
    public bool spawnPosition = false;
    public bool spawnWave = true;
}
#endregion