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

    [Header("���̺� Ÿ�̸�")]
    public float waveTimeLimit = 60f;        // ���̺�� ���� �ð� (60��)
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
                                                                // GameManager.cs�� �߰��ؾ� �� ������ �޼����

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
        if (isWaveActive)
        {
            currentWaveTime += Time.deltaTime;

            // UI ������Ʈ�� �̺�Ʈ (1�ʸ���)
            if (Mathf.FloorToInt(currentWaveTime) != Mathf.FloorToInt(currentWaveTime - Time.deltaTime))
            {
                int remainingTime = Mathf.Max(0, Mathf.CeilToInt(waveTimeLimit - currentWaveTime));
                OnWaveTimeChanged?.Invoke(remainingTime);
            }

            // ���̺� �ð� ���� üũ
            if (currentWaveTime >= waveTimeLimit)
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

        // ���̺꺰 �ð� ���� (���û���)
        AdjustWaveTime(waveNumber);

        OnWaveChanged?.Invoke(currentWave);

        Debug.Log($"���̺� {currentWave} ����! (���ѽð�: {waveTimeLimit}��)");
    }

    private void AdjustWaveTime(int waveNumber)
    {
        // ���̺갡 ����ɼ��� �ð��� ������ų� ª���� �� ����
        // ��: ���� ���̺�� �� ���
        if (waveNumber % 5 == 0) // ���� ���̺�
        {
            waveTimeLimit = 90f; // 90��
        }
        else
        {
            waveTimeLimit = 60f; // �⺻ 60��
        }
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

        // ī�� ���� ���� ����
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

    // ===== ī�� �ý��� =====
    private bool ShouldShowCards()
    {
        return currentWave % 3 == 0; // 3���̺긶��
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
        // ī�� ���� �� ���� ���̺�
        StartCoroutine(StartNextWaveWithDelay());
    }

    // ===== ���� ���� ���� �޼���� =====
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