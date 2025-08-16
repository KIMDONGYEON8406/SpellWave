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

    [Header("웨이브 타이머")]
    public float waveTimeLimit = 60f;        // 웨이브당 제한 시간 (60초)
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
                                                                // GameManager.cs에 추가해야 할 누락된 메서드들

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
        if (isWaveActive)
        {
            currentWaveTime += Time.deltaTime;

            // UI 업데이트용 이벤트 (1초마다)
            if (Mathf.FloorToInt(currentWaveTime) != Mathf.FloorToInt(currentWaveTime - Time.deltaTime))
            {
                int remainingTime = Mathf.Max(0, Mathf.CeilToInt(waveTimeLimit - currentWaveTime));
                OnWaveTimeChanged?.Invoke(remainingTime);
            }

            // 웨이브 시간 종료 체크
            if (currentWaveTime >= waveTimeLimit)
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

        // 웨이브별 시간 조정 (선택사항)
        AdjustWaveTime(waveNumber);

        OnWaveChanged?.Invoke(currentWave);

        Debug.Log($"웨이브 {currentWave} 시작! (제한시간: {waveTimeLimit}초)");
    }

    private void AdjustWaveTime(int waveNumber)
    {
        // 웨이브가 진행될수록 시간이 길어지거나 짧아질 수 있음
        // 예: 보스 웨이브는 더 길게
        if (waveNumber % 5 == 0) // 보스 웨이브
        {
            waveTimeLimit = 90f; // 90초
        }
        else
        {
            waveTimeLimit = 60f; // 기본 60초
        }
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

        // 카드 선택 여부 결정
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

    // ===== 카드 시스템 =====
    private bool ShouldShowCards()
    {
        return currentWave % 3 == 0; // 3웨이브마다
    }

    private void ShowCardSelection()
    {
        ChangeState(GameState.CardSelection);
        CardManager.Instance.ShowRandomCards();
    }

    public void OnCardSelected(CardData selectedCard)
    {
        ApplyCardEffect(selectedCard);
        ChangeState(GameState.Playing);
        // 카드 선택 후 다음 웨이브
        StartCoroutine(StartNextWaveWithDelay());
    }

    // ===== 게임 정보 접근 메서드들 =====
    public float GetWaveProgress()
    {
        return isWaveActive ? (currentWaveTime / waveTimeLimit) : 0f;
    }

    public int GetRemainingWaveTime()
    {
        return isWaveActive ? Mathf.Max(0, Mathf.CeilToInt(waveTimeLimit - currentWaveTime)) : 0;
    }

    public string GetFormattedGameTime()
    {
        int minutes = Mathf.FloorToInt(totalGameTime / 60);
        int seconds = Mathf.FloorToInt(totalGameTime % 60);
        return $"{minutes:00}:{seconds:00}";
    }
}