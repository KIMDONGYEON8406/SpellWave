using UnityEngine;
using System;

public enum GameState
{
    MainMenu,      // ���� �޴�
    CharacterSelect, // ĳ���� ����
    Playing,       // ���� �÷��� ��
    Paused,        // �Ͻ�����
    CardSelection, // ī�� ���� ��
    GameOver,      // ���� ����
    Victory        // ���� Ŭ����
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("���� ����")]
    public GameState currentState = GameState.Playing;
    public Character player;

    [Header("Ÿ�Ӷ��� ����")] // [����] Timeline �ý��� - [KDY]
    public TimelineConfig timelineConfig;

    [Header("Ÿ�Ӷ��� �����Ȳ")] // [�߰�] Timeline ���� ���� - [KDY]
    public float currentTime = 0f;           // ���� �������� ���� �ð�
    public bool isStageActive = false;       // �������� ���� ������
    public int currentLevel = 1;             // ���� ����
    public int currentExp = 0;               // ���� ����ġ
    public int expToNextLevel = 100;         // ���� �������� �ʿ��� ����ġ

    [Header("���� Ÿ�̸�")]
    public float totalGameTime = 0f;         // ��ü ���� �ð�

    // ===== �̺�Ʈ�� ===== [����] Timeline ������� ���� - [KDY]
    public static event Action<float> OnProgressChanged;        // ��ô�� ���� (0~100%)
    public static event Action<int> OnLevelUp;                  // ������
    public static event Action<int, int> OnExpChanged;          // ����ġ ���� (�������ġ, �ʿ����ġ)
    public static event Action OnStageCompleted;               // �������� �Ϸ�
    public static event Action<GameState> OnGameStateChanged;   // ���� ����

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
        // [����] Timeline �ý��� ���� - [KDY]
        if (timelineConfig == null)
        {
            Debug.LogError("GameManager: TimelineConfig�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        StartStage();
    }

    // [�߰�] �������� ���� �Լ� - [KDY]
    public void StartStage()
    {
        currentTime = 0f;
        isStageActive = true;
        currentLevel = 1;
        currentExp = 0;
        expToNextLevel = timelineConfig.GetExpToLevelUp(currentLevel);

        //// �÷��̾� ���� �ʱ�ȭ (�������� ���� ��)
        //if (player != null && player.GetPlayerStats() != null)
        //{
        //    player.GetPlayerStats().ResetToDefault();
        //}

        //Debug.Log($"�������� ����! ��ǥ �ð�: {timelineConfig.totalDuration / 60f:F1}��");
        //OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    // ���� ���� ���� �޼���
    public void ChangeState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
        Debug.Log($"���� ���� ����: {currentState}");
    }

    private void ApplyCardEffect(CardData selectedCard)
    {
        Debug.Log($"GameManager: ī�� ȿ�� ���� �Ϸ� - {selectedCard.cardName}");
    }

    void Update()
    {
        if (currentState == GameState.Playing && isStageActive)
        {
            UpdateTimeline(); // [����] Timeline ������Ʈ - [KDY]
        }
    }

    // [�߰�] Timeline ������Ʈ �Լ� - [KDY]
    private void UpdateTimeline()
    {
        // �ð� ������Ʈ
        currentTime += Time.deltaTime;
        totalGameTime += Time.deltaTime;

        // ��ô�� ������Ʈ (0~100%)
        float progress = timelineConfig.GetProgress(currentTime);
        OnProgressChanged?.Invoke(progress);

        // �������� �Ϸ� üũ
        if (timelineConfig.IsStageComplete(currentTime))
        {
            CompleteStage();
        }
    }

    // [�߰�] ����ġ ȹ�� �Լ� - [KDY]
    public void AddExperience(int expAmount)
    {
        currentExp += expAmount;
        Debug.Log($"����ġ +{expAmount} (��: {currentExp}/{expToNextLevel})");

        // ������ üũ
        while (currentExp >= expToNextLevel && currentLevel < timelineConfig.maxLevel)
        {
            LevelUp();
        }

        OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    // [�߰�] ������ ó�� - [KDY]
    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        currentLevel++;
        expToNextLevel = timelineConfig.GetExpToLevelUp(currentLevel);

        Debug.Log($"������! ���� {currentLevel}");
        OnLevelUp?.Invoke(currentLevel);

        // ī�� ���� ǥ��
        ShowCardSelection();
    }

    // [�߰�] �������� �Ϸ� ó�� - [KDY]
    private void CompleteStage()
    {
        isStageActive = false;
        OnStageCompleted?.Invoke();

        Debug.Log($"�������� �Ϸ�! �� �ð�: {currentTime / 60f:F1}��, ���� ����: {currentLevel}");
        ChangeState(GameState.Victory);
    }

    // ===== ī�� �ý��� ===== [����] ������ ������� ���� - [KDY]
    private void ShowCardSelection()
    {
        ChangeState(GameState.CardSelection);

        // [����] CardManager null üũ �߰� - [KDY]
        if (CardManager.Instance != null)
        {
            CardManager.Instance.ShowRandomCards();
        }
        else
        {
            Debug.LogWarning("CardManager�� ��� ī�� ������ �ǳʶݴϴ�.");
            ChangeState(GameState.Playing);
        }
    }

    public void OnCardSelected(CardData selectedCard)
    {
        ApplyCardEffect(selectedCard);
        ChangeState(GameState.Playing);
    }

    // ===== ���� ���� ���� �޼���� ===== [����] Timeline ������� ���� - [KDY]
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

    // ===== Timeline ���� ���� ===== [�߰�] - [KDY]
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

    // ===== ����׿� ===== [����] Timeline ������� ���� - [KDY]
    [ContextMenu("����ġ +100")]
    public void AddTestExp()
    {
        if (Application.isPlaying)
        {
            AddExperience(100);
        }
    }

    [ContextMenu("���� �������� �Ϸ�")]
    public void ForceCompleteStage()
    {
        if (Application.isPlaying)
        {
            CompleteStage();
        }
    }

    [ContextMenu("�������� �����")]
    public void RestartStage()
    {
        if (Application.isPlaying)
        {
            ChangeState(GameState.Playing);
            StartStage();
        }
    }
}