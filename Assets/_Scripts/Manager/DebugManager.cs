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

    [System.Serializable]
    public class DebugSettings
    {
        public bool enableCardLogs = true;
        public bool enableSkillLogs = true;
        public bool enableCombatLogs = true;
        public bool enableUILogs = false;
        public bool enableAILogs = false;
    }

    [Header("디버그 설정")]
    public DebugSettings settings = new DebugSettings();

    [Header("전체 설정")]
    public bool masterSwitch = true;  // 모든 로그 on/off
    public bool useColors = true;
    public bool showTimestamp = false;

    // 카테고리별 카운터 (너무 많은 로그 방지)
    private Dictionary<string, int> logCounts = new Dictionary<string, int>();
    private float resetCounterTime = 1f;
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
        // 1초마다 카운터 리셋
        if (Time.time - lastResetTime > resetCounterTime)
        {
            logCounts.Clear();
            lastResetTime = Time.time;
        }
    }

    // 메인 로그 메서드
    public static void Log(LogCategory category, string message, LogLevel level = LogLevel.Info)
    {
        if (!Instance.masterSwitch) return;
        if (!Instance.IsCategoryEnabled(category)) return;

        // 스팸 방지 (1초에 같은 카테고리 10개 이상 막기)
        string key = $"{category}_{level}";
        if (!Instance.logCounts.ContainsKey(key))
            Instance.logCounts[key] = 0;

        Instance.logCounts[key]++;
        if (Instance.logCounts[key] > 10)
            return;  // 너무 많은 로그 무시

        string formattedMessage = Instance.FormatMessage(category, message, level);

        switch (level)
        {
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

    // 간편 메서드들
    public static void LogCard(string message) => Log(LogCategory.Card, message);
    public static void LogSkill(string message) => Log(LogCategory.Skill, message);
    public static void LogCombat(string message) => Log(LogCategory.Combat, message);
    public static void LogError(LogCategory category, string message) => Log(category, message, LogLevel.Error);

    // 중요 이벤트용 (항상 출력)
    public static void LogImportant(string message)
    {
        Debug.Log($"<color=yellow><b>⭐ {message}</b></color>");
    }

    // 구분선 출력
    public static void LogSeparator(string title = "")
    {
        if (!Instance.masterSwitch) return;

        if (string.IsNullOrEmpty(title))
            Debug.Log("════════════════════════════════════════");
        else
            Debug.Log($"═══════════ {title} ═══════════");
    }

    private bool IsCategoryEnabled(LogCategory category)
    {
        switch (category)
        {
            case LogCategory.Card: return settings.enableCardLogs;
            case LogCategory.Skill: return settings.enableSkillLogs;
            case LogCategory.Combat: return settings.enableCombatLogs;
            case LogCategory.UI: return settings.enableUILogs;
            case LogCategory.AI: return settings.enableAILogs;
            default: return true;
        }
    }

    private string FormatMessage(LogCategory category, string message, LogLevel level)
    {
        string prefix = "";

        // 타임스탬프
        if (showTimestamp)
            prefix += $"[{Time.time:F2}] ";

        // 카테고리
        string categoryStr = $"[{category}]";

        // 색상 적용
        if (useColors)
        {
            string color = GetColor(category, level);
            return $"<color={color}>{prefix}{categoryStr} {message}</color>";
        }

        return $"{prefix}{categoryStr} {message}";
    }

    private string GetColor(LogCategory category, LogLevel level)
    {
        // 레벨 우선
        if (level == LogLevel.Error) return "red";
        if (level == LogLevel.Warning) return "yellow";

        // 카테고리별 색상
        switch (category)
        {
            case LogCategory.Card: return "cyan";
            case LogCategory.Skill: return "lime";
            case LogCategory.Combat: return "orange";
            case LogCategory.UI: return "magenta";
            case LogCategory.AI: return "white";
            default: return "white";
        }
    }
}

public enum LogCategory
{
    Card,
    Skill,
    Combat,
    UI,
    AI,
    System
}

public enum LogLevel
{
    Info,
    Warning,
    Error
}