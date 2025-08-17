using UnityEngine;
using System;

public enum GameState
{
    MainMenu,      // 메인 메뉴
    CharacterSelect, // 캐릭터 선택
    Playing,       // 게임 플레이 중
    Paused,        // 일시정지
    CardSelection, // 카드 선택 중
    GameOver,      // 게임 오버
    Victory        // 게임 클리어
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 상태")]
    public GameState currentState = GameState.Playing;
    public Character player;

    [Header("타임라인 설정")] // [수정] Timeline 시스템 - [KDY]
    public TimelineConfig timelineConfig;

    [Header("타임라인 진행상황")] // [추가] Timeline 진행 관리 - [KDY]
    public float currentTime = 0f;           // 현재 스테이지 진행 시간
    public bool isStageActive = false;       // 스테이지 진행 중인지
    public int currentLevel = 1;             // 현재 레벨
    public int currentExp = 0;               // 현재 경험치
    public int expToNextLevel = 100;         // 다음 레벨까지 필요한 경험치

    [Header("게임 타이머")]
    public float totalGameTime = 0f;         // 전체 게임 시간

    // ===== 이벤트들 ===== [수정] Timeline 기반으로 변경 - [KDY]
    public static event Action<float> OnProgressChanged;        // 진척도 변경 (0~100%)
    public static event Action<int> OnLevelUp;                  // 레벨업
    public static event Action<int, int> OnExpChanged;          // 경험치 변경 (현재경험치, 필요경험치)
    public static event Action OnStageCompleted;               // 스테이지 완료
    public static event Action<GameState> OnGameStateChanged;   // 상태 변경

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
        // [수정] Timeline 시스템 시작 - [KDY]
        if (timelineConfig == null)
        {
            Debug.LogError("GameManager: TimelineConfig가 할당되지 않았습니다!");
            return;
        }

        StartStage();
    }

    // [추가] 스테이지 시작 함수 - [KDY]
    public void StartStage()
    {
        currentTime = 0f;
        isStageActive = true;
        currentLevel = 1;
        currentExp = 0;
        expToNextLevel = timelineConfig.GetExpToLevelUp(currentLevel);

        //// 플레이어 스탯 초기화 (스테이지 시작 시)
        //if (player != null && player.GetPlayerStats() != null)
        //{
        //    player.GetPlayerStats().ResetToDefault();
        //}

        //Debug.Log($"스테이지 시작! 목표 시간: {timelineConfig.totalDuration / 60f:F1}분");
        //OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    // 게임 상태 변경 메서드
    public void ChangeState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
        Debug.Log($"게임 상태 변경: {currentState}");
    }

    private void ApplyCardEffect(CardData selectedCard)
    {
        Debug.Log($"GameManager: 카드 효과 적용 완료 - {selectedCard.cardName}");
    }

    void Update()
    {
        if (currentState == GameState.Playing && isStageActive)
        {
            UpdateTimeline(); // [수정] Timeline 업데이트 - [KDY]
        }
    }

    // [추가] Timeline 업데이트 함수 - [KDY]
    private void UpdateTimeline()
    {
        // 시간 업데이트
        currentTime += Time.deltaTime;
        totalGameTime += Time.deltaTime;

        // 진척도 업데이트 (0~100%)
        float progress = timelineConfig.GetProgress(currentTime);
        OnProgressChanged?.Invoke(progress);

        // 스테이지 완료 체크
        if (timelineConfig.IsStageComplete(currentTime))
        {
            CompleteStage();
        }
    }

    // [추가] 경험치 획득 함수 - [KDY]
    public void AddExperience(int expAmount)
    {
        currentExp += expAmount;
        Debug.Log($"경험치 +{expAmount} (총: {currentExp}/{expToNextLevel})");

        // 레벨업 체크
        while (currentExp >= expToNextLevel && currentLevel < timelineConfig.maxLevel)
        {
            LevelUp();
        }

        OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    // [추가] 레벨업 처리 - [KDY]
    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        currentLevel++;
        expToNextLevel = timelineConfig.GetExpToLevelUp(currentLevel);

        Debug.Log($"레벨업! 레벨 {currentLevel}");
        OnLevelUp?.Invoke(currentLevel);

        // 카드 선택 표시
        ShowCardSelection();
    }

    // [추가] 스테이지 완료 처리 - [KDY]
    private void CompleteStage()
    {
        isStageActive = false;
        OnStageCompleted?.Invoke();

        Debug.Log($"스테이지 완료! 총 시간: {currentTime / 60f:F1}분, 최종 레벨: {currentLevel}");
        ChangeState(GameState.Victory);
    }

    // ===== 카드 시스템 ===== [수정] 레벨업 기반으로 변경 - [KDY]
    private void ShowCardSelection()
    {
        ChangeState(GameState.CardSelection);

        // [수정] CardManager null 체크 추가 - [KDY]
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

    // ===== 게임 정보 접근 메서드들 ===== [수정] Timeline 기반으로 변경 - [KDY]
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

    // ===== Timeline 정보 접근 ===== [추가] - [KDY]
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

    // ===== 디버그용 ===== [수정] Timeline 기반으로 변경 - [KDY]
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