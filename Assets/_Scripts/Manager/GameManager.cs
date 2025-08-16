using UnityEngine.TextCore.Text;
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
    public int currentWave = 1;
    public Character player;

    [Header("���̺� ����")] // [�߰�] ScriptableObject ��� ���̺� �ý��� - [KDY]
    public WaveConfig normalWaveConfig;  // NormalWaveConfig �Ҵ�
    public WaveConfig bossWaveConfig;    // BossWaveConfig �Ҵ�
    private WaveConfig currentWaveConfig; // ���� ������� ����

    [Header("���̺� Ÿ�̸�")]
    // [����] public float waveTimeLimit = 60f; // WaveConfig.durationSec�� ��ü - [KDY]
    public float currentWaveTime = 0f;       // ���� ���̺� ��� �ð�
    public bool isWaveActive = false;        // ���̺� ���� ������

    [Header("���� Ÿ�̸�")]
    public float totalGameTime = 0f;         // ��ü ���� �ð�

    // ===== �̺�Ʈ�� =====
    public static event Action<int> OnWaveChanged;              // ���̺� ����
    public static event Action<int> OnWaveTimeChanged;          // ���̺� ���� �ð� (UI��)
    public static event Action OnWaveTimeUp;                    // ���̺� �ð� ����
    public static event Action OnWaveCompleted;                 // ���̺� Ŭ����
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
        // [�߰�] WaveConfig ��ȿ�� �˻� - [KDY]
        if (normalWaveConfig == null || bossWaveConfig == null)
        {
            Debug.LogError("GameManager: WaveConfig�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // ���� ���� �� ù ���̺� ����
        StartWave(1);
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
        if (currentState == GameState.Playing)
        {
            UpdateTimers();
        }
    }

    private void UpdateTimers()
    {
        // ��ü ���� �ð� ������Ʈ
        totalGameTime += Time.deltaTime;

        // ���̺갡 Ȱ��ȭ�� ��쿡�� ���̺� Ÿ�̸� ������Ʈ
        if (isWaveActive && currentWaveConfig != null) // [����] WaveConfig null üũ �߰� - [KDY]
        {
            currentWaveTime += Time.deltaTime;

            // UI ������Ʈ�� �̺�Ʈ (1�ʸ���)
            if (Mathf.FloorToInt(currentWaveTime) != Mathf.FloorToInt(currentWaveTime - Time.deltaTime))
            {
                // [����] waveTimeLimit �� currentWaveConfig.durationSec ���� - [KDY]
                int remainingTime = Mathf.Max(0, Mathf.CeilToInt(currentWaveConfig.durationSec - currentWaveTime));
                OnWaveTimeChanged?.Invoke(remainingTime);
            }

            // [����] ���̺� �ð� ���� üũ�� WaveConfig ������� ���� - [KDY]
            if (currentWaveTime >= currentWaveConfig.durationSec)
            {
                OnWaveTimeExpired();
            }
        }
    }

    // ===== ���̺� ���� =====
    public void StartWave(int waveNumber)
    {
        currentWave = waveNumber;
        currentWaveTime = 0f;
        isWaveActive = true;

        // [�߰�] ���̺� Ÿ�Կ� ���� WaveConfig ���� - [KDY]
        SelectWaveConfig(waveNumber);

        OnWaveChanged?.Invoke(currentWave);

        // [����] WaveConfig ��� �α� ��� - [KDY]
        Debug.Log($"���̺� {currentWave} ����! ({GetWaveTypeText()}, ���ѽð�: {currentWaveConfig.durationSec}��)");

        // [�߰�] WaveConfig ���� ��� - [KDY]
        currentWaveConfig.PrintWaveInfo();
    }

    // [�߰�] ���̺� Ÿ�Կ� ���� WaveConfig ���� �Լ� - [KDY]
    private void SelectWaveConfig(int waveNumber)
    {
        // 5���̺긶�� ���� ���̺�
        if (WaveConfig.IsBossWaveByNumber(waveNumber))
        {
            currentWaveConfig = bossWaveConfig;
            // ���� ���̺������ ���̺� �ε��� ����
            bossWaveConfig.waveIndex = waveNumber;
        }
        else
        {
            currentWaveConfig = normalWaveConfig;
            // �Ϲ� ���̺������ ���̺� �ε��� ����
            normalWaveConfig.waveIndex = waveNumber;
        }
    }

    // [�߰�] ���̺� Ÿ�� �ؽ�Ʈ ��ȯ �Լ� - [KDY]
    private string GetWaveTypeText()
    {
        return currentWaveConfig != null && currentWaveConfig.IsBossWave() ? "���� ���̺�" : "�Ϲ� ���̺�";
    }

    // ===== ���̺� �ð� ���� ó�� =====
    private void OnWaveTimeExpired()
    {
        Debug.Log($"���̺� {currentWave} �ð� ����!");

        isWaveActive = false;
        OnWaveTimeUp?.Invoke();

        // ������ ���̺� �Ϸ� ó��
        CompleteWave();
    }

    // ===== ���̺� �Ϸ� ó�� =====
    public void CompleteWave()
    {
        isWaveActive = false;
        OnWaveCompleted?.Invoke();

        Debug.Log($"���̺� {currentWave} �Ϸ�!");

        // ī�� ���� ���� ���� (WaveConfig ���)
        if (ShouldShowCards())
        {
            ShowCardSelection();
        }
        else
        {
            // ª�� �޽� �� ���� ���̺�
            StartCoroutine(StartNextWaveWithDelay());
        }
    }

    private System.Collections.IEnumerator StartNextWaveWithDelay()
    {
        // 2�� �޽�
        yield return new WaitForSeconds(2f);
        StartWave(currentWave + 1);
    }

    // ===== ���� ���̺� �Ϸ� (��� �� óġ ��) =====
    public void ForceCompleteWave()
    {
        if (isWaveActive)
        {
            Debug.Log($"���̺� {currentWave} ���� �Ϸ�! (��� �� óġ)");
            CompleteWave();
        }
    }

    // ===== ī�� �ý��� (WaveConfig ���) ===== [����] - [KDY]
    private bool ShouldShowCards()
    {
        if (currentWaveConfig == null) return false;

        // [����] WaveConfig�� showCardSelection ���� ��� (����: 3���̺긶��) - [KDY]
        return currentWaveConfig.showCardSelection;
    }

    private void ShowCardSelection()
    {
        ChangeState(GameState.CardSelection);

        // [�߰�] ���� ���̺����� Ȯ���ؼ� ��ų ī�� ǥ�� ���� ���� - [KDY]
        bool showSkillCards = currentWaveConfig != null && currentWaveConfig.canShowSkillCards;

        // CardManager�� ��ų ī�� ǥ�� ���� ���� (CardManager �Լ� ���� �ʿ��� �� ����)
        CardManager.Instance.ShowRandomCards();
    }

    public void OnCardSelected(CardData selectedCard)
    {
        ApplyCardEffect(selectedCard);
        ChangeState(GameState.Playing);
        // ī�� ���� �� ���� ���̺�
        StartCoroutine(StartNextWaveWithDelay());
    }

    // ===== ���� ���� ���� �޼���� ===== [����] WaveConfig ������� ���� - [KDY]
    public float GetWaveProgress()
    {
        // [����] WaveConfig.durationSec ��� - [KDY]
        if (!isWaveActive || currentWaveConfig == null) return 0f;
        return currentWaveTime / currentWaveConfig.durationSec;
    }

    public int GetRemainingWaveTime()
    {
        // [����] WaveConfig.durationSec ��� - [KDY]
        if (!isWaveActive || currentWaveConfig == null) return 0;
        return Mathf.Max(0, Mathf.CeilToInt(currentWaveConfig.durationSec - currentWaveTime));
    }

    public string GetFormattedGameTime()
    {
        int minutes = Mathf.FloorToInt(totalGameTime / 60);
        int seconds = Mathf.FloorToInt(totalGameTime % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    // ===== WaveConfig ���� ���� ===== [�߰�] - [KDY]
    public WaveConfig GetCurrentWaveConfig()
    {
        return currentWaveConfig;
    }

    public bool IsCurrentWaveBoss()
    {
        return currentWaveConfig != null && currentWaveConfig.IsBossWave();
    }

    // ===== ����׿� ===== [�߰�] - [KDY]
    [ContextMenu("���� ���� ���̺�")]
    public void ForceNextWave()
    {
        if (Application.isPlaying)
        {
            CompleteWave();
        }
    }

    [ContextMenu("���� ���� ���̺�")]
    public void ForceBossWave()
    {
        if (Application.isPlaying)
        {
            StartWave(5); // 5���̺�� ���� �̵�
        }
    }
}