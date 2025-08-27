using UnityEngine;
using System.Collections.Generic;

public class DebugManager : MonoBehaviour
{
    private static DebugManager instance;
    public static DebugManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("DebugManager");
                instance = go.AddComponent<DebugManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("전역 설정")]
    public bool masterSwitch = true;
    public LogLevel globalLogLevel = LogLevel.Info;

    [Header("시스템별 로그 설정")]
    public bool enableCardSystem = true;      // 카드 관련 전체
    public bool enableSkillSystem = true;     // 스킬 관련 전체
    public bool enableCombatSystem = true;    // 전투 관련 전체
    public bool enablePoolSystem = false;     // 풀링 관련 전체
    public bool enableSpawnSystem = false;    // 스폰 관련 전체
    public bool enableUISystem = false;       // UI 관련 전체

    //[Header("세부 카테고리")]
    [System.Serializable]
    public class DetailedSettings
    {
        [Header("스킬 시스템 세부")]
        public bool skillExecution = true;
        public bool skillCooldown = true;
        public bool skillDamage = true;
        public bool skillEffects = true;

        [Header("전투 시스템 세부")]
        public bool enemyDamage = true;
        public bool playerDamage = true;
        public bool passiveEffects = true;

        [Header("카드 시스템 세부")]
        public bool cardSelection = true;
        public bool cardEffectApply = true;
    }

    public DetailedSettings detailSettings = new DetailedSettings();

    [Header("출력 옵션")]
    public bool useColors = true;
    public bool showTimestamp = false;
    public int maxLogsPerSecond = 10;

    private Dictionary<string, int> logCounts = new Dictionary<string, int>();
    private float lastResetTime;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Time.time - lastResetTime > 1f)
        {
            logCounts.Clear();
            lastResetTime = Time.time;
        }
    }

    // 메인 로그 메서드
    public static void Log(LogCategory category, string message, LogLevel level = LogLevel.Info)
    {
        if (!Instance || !Instance.masterSwitch) return;
        if (level < Instance.globalLogLevel) return;
        if (!Instance.IsCategoryEnabled(category)) return;

        // 스팸 방지
        string key = $"{category}_{level}";
        if (!Instance.logCounts.ContainsKey(key))
            Instance.logCounts[key] = 0;

        Instance.logCounts[key]++;
        if (Instance.logCounts[key] > Instance.maxLogsPerSecond)
            return;

        string formattedMessage = Instance.FormatMessage(category, message, level);

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

    // 시스템별 간편 메서드
    public static void LogCard(string message) => Log(LogCategory.Card, message);
    public static void LogSkill(string message) => Log(LogCategory.Skill, message);
    public static void LogCombat(string message) => Log(LogCategory.Combat, message);
    public static void LogUI(string message) => Log(LogCategory.UI, message);
    public static void LogPool(string message) => Log(LogCategory.Pool, message);
    public static void LogSpawn(string message) => Log(LogCategory.Spawn, message);
    public static void LogError(LogCategory category, string message) => Log(category, message, LogLevel.Error);
    public static void LogWarning(LogCategory category, string message) => Log(category, message, LogLevel.Warning);

    // 중요 로그 (항상 출력)
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
            case LogCategory.System: return "blue";
            default: return "white";
        }
    }

    // 시스템별 일괄 켜기/끄기
    [ContextMenu("모든 로그 켜기")]
    public void EnableAllLogs()
    {
        masterSwitch = true;
        enableCardSystem = true;
        enableSkillSystem = true;
        enableCombatSystem = true;
        enablePoolSystem = true;
        enableSpawnSystem = true;
        enableUISystem = true;
    }

    [ContextMenu("모든 로그 끄기")]
    public void DisableAllLogs()
    {
        masterSwitch = false;
    }

    [ContextMenu("전투 관련만 켜기")]
    public void EnableOnlyCombat()
    {
        DisableAllLogs();
        masterSwitch = true;
        enableCombatSystem = true;
        enableSkillSystem = true;
    }

    [ContextMenu("성능 관련 끄기")]
    public void DisablePerformanceHeavy()
    {
        enablePoolSystem = false;
        enableSpawnSystem = false;
        enableCombatSystem = false;
    }
}

public enum LogCategory
{
    Card,
    Skill,
    Combat,
    UI,
    Pool,
    Spawn,
    System
}

public enum LogLevel
{
    None,
    Error,
    Warning,
    Info,
    Debug
}