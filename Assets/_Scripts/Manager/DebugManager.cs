using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unity 게임용 통합 디버그 로그 관리 시스템
/// - 시스템별/세부별 로그 ON/OFF 제어
/// - 컬러 코딩 및 스팸 방지 기능
/// - 성능 최적화를 위한 조건부 로깅
/// </summary>
public class DebugManager : MonoBehaviour
{
    #region Singleton Pattern
    /// <summary>싱글톤 인스턴스</summary>
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
    #endregion

    #region Global Settings
    /// <summary>전역 디버그 설정</summary>
    [Header("전역 설정")]
    public bool masterSwitch = true;                // 전체 로그 마스터 스위치
    public LogLevel globalLogLevel = LogLevel.Info; // 최소 출력 로그 레벨
    public bool useColors = true;                   // 컬러 출력 활성화
    public bool showTimestamp = false;              // 타임스탬프 표시
    public int maxLogsPerSecond = 10;               // 초당 최대 로그 수 (스팸 방지)
    #endregion

    #region System Category Switches
    /// <summary>게임 시스템별 메인 로그 스위치</summary>
    [Header("시스템별 로그 설정")]
    public bool enableCardSystem = true;      // 카드 시스템 전체 (선택/효과/인벤토리 등)
    public bool enableSkillSystem = true;     // 스킬 시스템 전체 (실행/쿨다운/레벨업 등)
    public bool enableCombatSystem = true;    // 전투 시스템 전체 (데미지/타겟팅/AI 등)
    public bool enablePoolSystem = false;     // 오브젝트 풀링 (성능상 기본 OFF)
    public bool enableSpawnSystem = false;    // 스폰 시스템 (성능상 기본 OFF)
    public bool enableUISystem = false;       // UI 시스템 전체
    public bool enablePlayerSystem = true;    // 플레이어 관련 전체
    public bool enableGameSystem = true;      // 게임매니저/상태/진행도 등
    #endregion

    #region Detailed Sub-Category Settings
    /// <summary>각 시스템별 세부 로그 설정</summary>
    [System.Serializable]
    public class DetailedSettings
    {
        [Header("스킬 시스템 세부")]
        public bool skillExecution = true;        // 스킬 실행 로그
        public bool skillCooldown = true;         // 쿨다운 관련
        public bool skillDamage = true;           // 데미지 계산
        public bool skillEffects = true;          // 스킬 효과 적용
        public bool skillManagement = true;       // 스킬 추가/제거
        public bool skillInitialization = true;   // 스킬 초기화
        public bool skillLevelUp = true;          // 레벨업 처리
        public bool skillStats = true;            // 스킬 스탯 조회

        [Header("전투 시스템 세부")]
        public bool enemyDamage = true;           // 적 데미지 처리
        public bool playerDamage = true;          // 플레이어 데미지
        public bool passiveEffects = true;        // 패시브 효과
        public bool autoSkillCasting = true;      // 자동 스킬 시전
        public bool targetingSystem = true;       // 타겟팅 시스템
        public bool skillCooldowns = true;        // 스킬 쿨다운
        public bool multiCastSystem = true;       // 다중 시전 시스템

        [Header("카드 시스템 세부")]
        public bool cardSelection = true;         // 카드 선택
        public bool cardEffectApply = true;       // 카드 효과 적용
        public bool cardDisplay = true;           // 카드 UI 표시
        public bool cardInventory = true;         // 카드 인벤토리
        public bool cardReward = true;            // 카드 보상

        [Header("플레이어 시스템 세부")]
        public bool playerMovement = true;        // 이동 관련
        public bool playerInput = false;          // 입력 처리
        public bool playerStats = true;           // 스탯 변경
        public bool playerHealth = true;          // 체력 시스템
        public bool playerExperience = false;     // 경험치 시스템
        public bool playerComponents = false;     // 컴포넌트 관리

        [Header("게임 시스템 세부")]
        public bool gameStateChanges = true;      // 게임 상태 변경
        public bool experienceSystem = true;      // 경험치 시스템
        public bool levelUpSystem = true;         // 레벨업 시스템
        public bool timelineProgress = false;     // 타임라인 진행
        public bool debugCommands = true;         // 디버그 명령
        public bool waveSystem = true;            // 웨이브 시스템
        public bool scoreSystem = true;           // 점수 시스템
        public bool saveLoadSystem = true;        // 저장/로드

        [Header("풀링 시스템 세부")]
        public bool poolCreation = true;          // 풀 생성
        public bool poolExpansion = true;         // 풀 확장
        public bool poolReturn = true;            // 오브젝트 반환
        public bool poolStatus = false;           // 풀 상태 (성능상 OFF)

        [Header("적 시스템 세부")]
        public bool enemySpawn = true;            // 적 스폰
        public bool enemyDeath = true;            // 적 사망
        public bool enemyBehavior = false;        // 적 AI (성능상 OFF)
        public bool enemyTargeting = true;        // 적 타겟팅

        [Header("스폰 시스템 세부")]
        public bool spawnRate = true;             // 스폰률 변경
        public bool spawnPosition = false;        // 스폰 위치 (성능상 OFF)
        public bool spawnWave = true;             // 웨이브 정보
    }

    public DetailedSettings detailSettings = new DetailedSettings();
    #endregion

    #region Private Fields
    /// <summary>스팸 방지용 로그 카운터</summary>
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
    }

    void Update()
    {
        // 1초마다 로그 카운터 리셋 (스팸 방지)
        if (Time.time - lastResetTime > 1f)
        {
            logCounts.Clear();
            lastResetTime = Time.time;
        }
    }
    #endregion

    #region Core Logging Methods
    /// <summary>
    /// 메인 로그 메서드 - 모든 로그는 이 메서드를 통해 처리됨
    /// </summary>
    /// <param name="category">로그 카테고리</param>
    /// <param name="message">로그 메시지</param>
    /// <param name="level">로그 레벨</param>
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

    #region System-Level Log Methods
    /// <summary>시스템별 간편 로그 메서드들 - 각 시스템에서 직접 호출</summary>
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

    /// <summary>중요 로그 - 항상 출력됨</summary>
    public static void LogImportant(string message)
    {
        if (!Instance || !Instance.masterSwitch) return;
        Debug.Log($"<color=yellow><b>★ {message}</b></color>");
    }

    /// <summary>구분선 출력</summary>
    public static void LogSeparator(string title = "")
    {
        if (!Instance || !Instance.masterSwitch) return;
        if (string.IsNullOrEmpty(title))
            Debug.Log("════════════════════════════════════════");
        else
            Debug.Log($"═══════════ {title} ═══════════");
    }
    #endregion

    #region Pool System Detail Methods
    /// <summary>오브젝트 풀링 시스템 세부 로그</summary>
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
    #endregion

    #region Enemy System Detail Methods
    /// <summary>적 시스템 세부 로그</summary>
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
    #endregion

    #region Spawn System Detail Methods
    /// <summary>스폰 시스템 세부 로그</summary>
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

    #region Combat System Detail Methods
    /// <summary>전투 시스템 세부 로그</summary>
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
    #endregion

    #region Skill System Detail Methods
    /// <summary>스킬 시스템 세부 로그</summary>
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
    #endregion

    #region Card System Detail Methods
    /// <summary>카드 시스템 세부 로그</summary>
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
    #endregion

    #region Game System Detail Methods
    /// <summary>게임 시스템 세부 로그</summary>
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
    #endregion

    #region Player System Detail Methods
    /// <summary>플레이어 시스템 세부 로그</summary>
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
    #endregion

    #region Private Helper Methods
    /// <summary>카테고리 활성화 상태 체크</summary>
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

    /// <summary>로그 메시지 포맷팅</summary>
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

    /// <summary>카테고리별 컬러 설정</summary>
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

    #region Context Menu Quick Actions
    /// <summary>에디터 우클릭 메뉴를 통한 빠른 설정</summary>
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
        enablePlayerSystem = true;
        enableGameSystem = true;
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

    [ContextMenu("플레이어 관련만 켜기")]
    public void EnableOnlyPlayer()
    {
        DisableAllLogs();
        masterSwitch = true;
        enablePlayerSystem = true;
    }

    [ContextMenu("성능 관련 끄기")]
    public void DisablePerformanceHeavy()
    {
        enablePoolSystem = false;
        enableSpawnSystem = false;
        detailSettings.poolStatus = false;
        detailSettings.spawnPosition = false;
        detailSettings.enemyBehavior = false;
    }

    [ContextMenu("게임 관련만 켜기")]
    public void EnableOnlyGame()
    {
        DisableAllLogs();
        masterSwitch = true;
        enableGameSystem = true;
    }
    #endregion
}

#region Enums
/// <summary>로그 카테고리 정의</summary>
public enum LogCategory
{
    Card,      // 카드 시스템
    Skill,     // 스킬 시스템
    Combat,    // 전투 시스템
    UI,        // UI 시스템
    Pool,      // 오브젝트 풀링
    Spawn,     // 스폰 시스템
    Player,    // 플레이어 시스템
    System,    // 시스템 전반
    Game       // 게임매니저
}

/// <summary>로그 레벨 정의</summary>
public enum LogLevel
{
    None,      // 출력 안함
    Error,     // 에러만
    Warning,   // 경고 이상
    Info,      // 정보 이상
    Debug      // 모든 로그
}
#endregion