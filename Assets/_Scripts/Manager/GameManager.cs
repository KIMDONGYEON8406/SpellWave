using UnityEngine.TextCore.Text;
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
    public int currentWave = 1;
    public Character player;

    [Header("웨이브 설정")] // [추가] ScriptableObject 기반 웨이브 시스템 - [KDY]
    public WaveConfig normalWaveConfig;  // NormalWaveConfig 할당
    public WaveConfig bossWaveConfig;    // BossWaveConfig 할당
    private WaveConfig currentWaveConfig; // 현재 사용중인 설정

    [Header("웨이브 타이머")]
    // [삭제] public float waveTimeLimit = 60f; // WaveConfig.durationSec로 대체 - [KDY]
    public float currentWaveTime = 0f;       // 현재 웨이브 경과 시간
    public bool isWaveActive = false;        // 웨이브 진행 중인지

    [Header("게임 타이머")]
    public float totalGameTime = 0f;         // 전체 게임 시간

    // ===== 이벤트들 =====
    public static event Action<int> OnWaveChanged;              // 웨이브 변경
    public static event Action<int> OnWaveTimeChanged;          // 웨이브 남은 시간 (UI용)
    public static event Action OnWaveTimeUp;                    // 웨이브 시간 종료
    public static event Action OnWaveCompleted;                 // 웨이브 클리어
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
        // [추가] WaveConfig 유효성 검사 - [KDY]
        if (normalWaveConfig == null || bossWaveConfig == null)
        {
            Debug.LogError("GameManager: WaveConfig가 할당되지 않았습니다!");
            return;
        }

        // 게임 시작 시 첫 웨이브 시작
        StartWave(1);
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
        if (currentState == GameState.Playing)
        {
            UpdateTimers();
        }
    }

    private void UpdateTimers()
    {
        // 전체 게임 시간 업데이트
        totalGameTime += Time.deltaTime;

        // 웨이브가 활성화된 경우에만 웨이브 타이머 업데이트
        if (isWaveActive && currentWaveConfig != null) // [수정] WaveConfig null 체크 추가 - [KDY]
        {
            currentWaveTime += Time.deltaTime;

            // UI 업데이트용 이벤트 (1초마다)
            if (Mathf.FloorToInt(currentWaveTime) != Mathf.FloorToInt(currentWaveTime - Time.deltaTime))
            {
                // [수정] waveTimeLimit → currentWaveConfig.durationSec 변경 - [KDY]
                int remainingTime = Mathf.Max(0, Mathf.CeilToInt(currentWaveConfig.durationSec - currentWaveTime));
                OnWaveTimeChanged?.Invoke(remainingTime);
            }

            // [수정] 웨이브 시간 종료 체크를 WaveConfig 기반으로 변경 - [KDY]
            if (currentWaveTime >= currentWaveConfig.durationSec)
            {
                OnWaveTimeExpired();
            }
        }
    }

    // ===== 웨이브 관리 =====
    public void StartWave(int waveNumber)
    {
        currentWave = waveNumber;
        currentWaveTime = 0f;
        isWaveActive = true;

        // [추가] 웨이브 타입에 따라 WaveConfig 선택 - [KDY]
        SelectWaveConfig(waveNumber);

        OnWaveChanged?.Invoke(currentWave);

        // [수정] WaveConfig 기반 로그 출력 - [KDY]
        Debug.Log($"웨이브 {currentWave} 시작! ({GetWaveTypeText()}, 제한시간: {currentWaveConfig.durationSec}초)");

        // [추가] WaveConfig 정보 출력 - [KDY]
        currentWaveConfig.PrintWaveInfo();
    }

    // [추가] 웨이브 타입에 따른 WaveConfig 선택 함수 - [KDY]
    private void SelectWaveConfig(int waveNumber)
    {
        // 5웨이브마다 보스 웨이브
        if (WaveConfig.IsBossWaveByNumber(waveNumber))
        {
            currentWaveConfig = bossWaveConfig;
            // 보스 웨이브용으로 웨이브 인덱스 설정
            bossWaveConfig.waveIndex = waveNumber;
        }
        else
        {
            currentWaveConfig = normalWaveConfig;
            // 일반 웨이브용으로 웨이브 인덱스 설정
            normalWaveConfig.waveIndex = waveNumber;
        }
    }

    // [추가] 웨이브 타입 텍스트 반환 함수 - [KDY]
    private string GetWaveTypeText()
    {
        return currentWaveConfig != null && currentWaveConfig.IsBossWave() ? "보스 웨이브" : "일반 웨이브";
    }

    // ===== 웨이브 시간 종료 처리 =====
    private void OnWaveTimeExpired()
    {
        Debug.Log($"웨이브 {currentWave} 시간 종료!");

        isWaveActive = false;
        OnWaveTimeUp?.Invoke();

        // 강제로 웨이브 완료 처리
        CompleteWave();
    }

    // ===== 웨이브 완료 처리 =====
    public void CompleteWave()
    {
        isWaveActive = false;
        OnWaveCompleted?.Invoke();

        Debug.Log($"웨이브 {currentWave} 완료!");

        // 카드 선택 여부 결정 (WaveConfig 기반)
        if (ShouldShowCards())
        {
            ShowCardSelection();
        }
        else
        {
            // 짧은 휴식 후 다음 웨이브
            StartCoroutine(StartNextWaveWithDelay());
        }
    }

    private System.Collections.IEnumerator StartNextWaveWithDelay()
    {
        // 2초 휴식
        yield return new WaitForSeconds(2f);
        StartWave(currentWave + 1);
    }

    // ===== 수동 웨이브 완료 (모든 적 처치 시) =====
    public void ForceCompleteWave()
    {
        if (isWaveActive)
        {
            Debug.Log($"웨이브 {currentWave} 조기 완료! (모든 적 처치)");
            CompleteWave();
        }
    }

    // ===== 카드 시스템 (WaveConfig 기반) ===== [수정] - [KDY]
    private bool ShouldShowCards()
    {
        if (currentWaveConfig == null) return false;

        // [수정] WaveConfig의 showCardSelection 설정 사용 (기존: 3웨이브마다) - [KDY]
        return currentWaveConfig.showCardSelection;
    }

    private void ShowCardSelection()
    {
        ChangeState(GameState.CardSelection);

        // [추가] 보스 웨이브인지 확인해서 스킬 카드 표시 여부 결정 - [KDY]
        bool showSkillCards = currentWaveConfig != null && currentWaveConfig.canShowSkillCards;

        // CardManager에 스킬 카드 표시 여부 전달 (CardManager 함수 수정 필요할 수 있음)
        CardManager.Instance.ShowRandomCards();
    }

    public void OnCardSelected(CardData selectedCard)
    {
        ApplyCardEffect(selectedCard);
        ChangeState(GameState.Playing);
        // 카드 선택 후 다음 웨이브
        StartCoroutine(StartNextWaveWithDelay());
    }

    // ===== 게임 정보 접근 메서드들 ===== [수정] WaveConfig 기반으로 변경 - [KDY]
    public float GetWaveProgress()
    {
        // [수정] WaveConfig.durationSec 사용 - [KDY]
        if (!isWaveActive || currentWaveConfig == null) return 0f;
        return currentWaveTime / currentWaveConfig.durationSec;
    }

    public int GetRemainingWaveTime()
    {
        // [수정] WaveConfig.durationSec 사용 - [KDY]
        if (!isWaveActive || currentWaveConfig == null) return 0;
        return Mathf.Max(0, Mathf.CeilToInt(currentWaveConfig.durationSec - currentWaveTime));
    }

    public string GetFormattedGameTime()
    {
        int minutes = Mathf.FloorToInt(totalGameTime / 60);
        int seconds = Mathf.FloorToInt(totalGameTime % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    // ===== WaveConfig 정보 접근 ===== [추가] - [KDY]
    public WaveConfig GetCurrentWaveConfig()
    {
        return currentWaveConfig;
    }

    public bool IsCurrentWaveBoss()
    {
        return currentWaveConfig != null && currentWaveConfig.IsBossWave();
    }

    // ===== 디버그용 ===== [추가] - [KDY]
    [ContextMenu("강제 다음 웨이브")]
    public void ForceNextWave()
    {
        if (Application.isPlaying)
        {
            CompleteWave();
        }
    }

    [ContextMenu("강제 보스 웨이브")]
    public void ForceBossWave()
    {
        if (Application.isPlaying)
        {
            StartWave(5); // 5웨이브로 강제 이동
        }
    }
}