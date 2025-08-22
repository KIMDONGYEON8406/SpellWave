using UnityEngine;
using System;

public enum GameState
{
    MainMenu,
    CharacterSelect,
    Playing,
    Paused,
    CardSelection,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 상태")]
    public GameState currentState = GameState.Playing;
    public Character player;

    [Header("초기 장비")]
    public StaffData defaultStaff;

    [Header("타임라인 설정")]
    public TimelineConfig timelineConfig;

    [Header("타임라인 진행상황")]
    public float currentTime = 0f;
    public bool isStageActive = false;
    public int currentLevel = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;

    [Header("게임 타이머")]
    public float totalGameTime = 0f;

    // 이벤트들
    public static event Action<float> OnProgressChanged;
    public static event Action<int> OnLevelUp;
    public static event Action<int, int> OnExpChanged;
    public static event Action OnStageCompleted;
    public static event Action<GameState> OnGameStateChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (timelineConfig == null)
        {
            Debug.LogError("GameManager: TimelineConfig가 할당되지 않았습니다!");
            return;
        }

        if (player == null)
        {
            player = GameObject.FindObjectOfType<Character>();
            if (player != null && player.tag != "Player")
            {
                player.tag = "Player";
            }
        }

        InitializeStaff();
        StartStage();
    }

    void Update()
    {
        if (currentState == GameState.Playing && isStageActive)
        {
            UpdateTimeline();
        }
    }

    public void StartStage()
    {
        currentTime = 0f;
        isStageActive = true;
        currentLevel = 1;
        currentExp = 0;
        expToNextLevel = timelineConfig.GetExpToLevelUp(currentLevel);

        Debug.Log($"스테이지 시작! 목표 시간: {timelineConfig.totalDuration / 60f:F1}분");
        OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    void InitializeStaff()
    {
        if (StaffManager.Instance != null && defaultStaff != null)
        {
            StaffManager.Instance.UnlockStaff(defaultStaff);
            StaffManager.Instance.EquipStaff(defaultStaff);
            Debug.Log($"초기 지팡이 장착: {defaultStaff.staffName}");
        }
        else if (defaultStaff == null)
        {
            Debug.LogWarning("GameManager: 기본 지팡이가 설정되지 않았습니다!");
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
        Debug.Log($"게임 상태 변경: {currentState}");
    }

    private void UpdateTimeline()
    {
        currentTime += Time.deltaTime;
        totalGameTime += Time.deltaTime;

        float progress = timelineConfig.GetProgress(currentTime);
        OnProgressChanged?.Invoke(progress);

        if (timelineConfig.IsStageComplete(currentTime))
        {
            CompleteStage();
        }
    }

    public void AddExperience(int expAmount)
    {
        currentExp += expAmount;
        Debug.Log($"경험치 +{expAmount} (총: {currentExp}/{expToNextLevel})");

        while (currentExp >= expToNextLevel && currentLevel < timelineConfig.maxLevel)
        {
            LevelUp();
        }

        OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        currentLevel++;
        expToNextLevel = timelineConfig.GetExpToLevelUp(currentLevel);

        Debug.Log($"레벨업! 레벨 {currentLevel}");
        OnLevelUp?.Invoke(currentLevel);

        ShowCardSelection();
    }

    private void CompleteStage()
    {
        isStageActive = false;
        OnStageCompleted?.Invoke();

        Debug.Log($"스테이지 완료! 총 시간: {currentTime / 60f:F1}분, 최종 레벨: {currentLevel}");
        ChangeState(GameState.Victory);
    }

    private void ShowCardSelection()
    {
        ChangeState(GameState.CardSelection);

        if (CardManager.Instance != null)
        {
            CardManager.Instance.ShowRandomCards();
        }
        else
        {
            Debug.LogWarning("CardManager가 없어서 카드 선택을 건너뜁니다.");
            ChangeState(GameState.Playing);
        }
    }

    public void OnCardSelected(CardData selectedCard)
    {
        ApplyCardEffect(selectedCard);
        ChangeState(GameState.Playing);
    }

    private void ApplyCardEffect(CardData selectedCard)
    {
        Debug.Log($"GameManager: 카드 효과 적용 완료 - {selectedCard.cardName}");
    }

    // 정보 접근 메서드들
    public float GetStageProgress()
    {
        return timelineConfig != null ? timelineConfig.GetProgress(currentTime) : 0f;
    }

    public int GetRemainingTime()
    {
        if (timelineConfig == null) return 0;
        return Mathf.Max(0, Mathf.CeilToInt(timelineConfig.totalDuration - currentTime));
    }

    public string GetFormattedGameTime()
    {
        int minutes = Mathf.FloorToInt(totalGameTime / 60);
        int seconds = Mathf.FloorToInt(totalGameTime % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    public string GetFormattedStageTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    public TimelineConfig GetTimelineConfig()
    {
        return timelineConfig;
    }

    public float GetCurrentDifficultyMultiplier()
    {
        return timelineConfig != null ? timelineConfig.GetDifficultyMultiplier(currentTime) : 1f;
    }

    public float GetCurrentSpawnRate()
    {
        return timelineConfig != null ? timelineConfig.GetCurrentSpawnRate(currentTime) : 0.5f;
    }

    // ===== 디버그 메서드들 =====
    [ContextMenu("레벨업 테스트")]
    public void TestLevelUp()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("플레이 모드에서만 작동합니다!");
            return;
        }

        currentLevel++;
        Debug.Log($"강제 레벨업! 현재 레벨: {currentLevel}");

        if (currentLevel % 4 == 0)
        {
            Debug.Log(">>> 스킬 카드가 나와야 함!");
        }
        else
        {
            Debug.Log(">>> 스탯 카드가 나와야 함!");
        }

        OnLevelUp?.Invoke(currentLevel);
        ShowCardSelection();
    }

    [ContextMenu("경험치 +100")]
    public void AddTestExp()
    {
        if (Application.isPlaying)
        {
            AddExperience(100);
        }
    }

    [ContextMenu("강제 스테이지 완료")]
    public void ForceCompleteStage()
    {
        if (Application.isPlaying)
        {
            CompleteStage();
        }
    }

    [ContextMenu("스테이지 재시작")]
    public void RestartStage()
    {
        if (Application.isPlaying)
        {
            ChangeState(GameState.Playing);
            StartStage();
        }
    }
}