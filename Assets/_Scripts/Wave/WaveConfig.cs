using UnityEngine;

[CreateAssetMenu(fileName = "New WaveConfig", menuName = "SpellWave/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("���̺� �⺻ ����")]
    public int waveIndex = 1;
    [Tooltip("���̺� ���� �ð� (��)")]
    public float durationSec = 60f;

    [Header("���̺� Ÿ��")]
    public WaveType waveType = WaveType.Normal;

    [Header("�Ϲ� ���� ���� ����")]
    [Tooltip("���̺� ���� �� ��� ������ ���� ��")]
    public int initialEnemies = 5;
    [Tooltip("�ʴ� ������ ���� ��")]
    public float spawnRatePerSec = 0.5f;
    [Tooltip("����� �⺻ ���� ������")]
    public EnemyStats monsterBase;

    [Header("���� ����")]
    [Tooltip("���� ���̺꿡�� ����� ���� ���� ������")]
    public EnemyStats bossEnemyStats;
    [Tooltip("�Ϲ� ���͸� ��� óġ�� �� ���� ���� ����")]
    public bool spawnBossAfterClear = true;

    [Header("���̵� �����ϸ�")]
    [Tooltip("���̺�� ü�� ���� ���� (1.2 = 20% ����)")]
    public float hpScalePerWave = 1.2f;
    [Tooltip("���̺�� ���ݷ� ���� ���� (1.15 = 15% ����)")]
    public float atkScalePerWave = 1.15f;
    [Tooltip("���̺�� �̵��ӵ� ���� ���� (1.1 = 10% ����)")]
    public float msScalePerWave = 1.1f;

    [Header("���� ��ġ ����")]
    [Tooltip("�÷��̾� �ֺ� ���� �ݰ�")]
    public float spawnRadius = 15f;
    [Tooltip("�÷��̾�κ��� �ּ� �Ÿ�")]
    public float minDistanceFromPlayer = 8f;

    [Header("���� ����")]
    [Tooltip("���̺� Ŭ���� �� ī�� ���� ����")]
    public bool showCardSelection = true;
    [Tooltip("��ų ī�带 �������� (���� ���̺꿡�� true)")]
    public bool canShowSkillCards = false;

    // ���̺� Ÿ�� ����
    public enum WaveType
    {
        Normal,  // �Ϲ� ���̺� (1,2,3,4...)
        Boss     // ���� ���̺� (5,10,15,20...)
    }

    // �Ϲ� ���� �����ϸ��� ���� ���
    public EnemyRuntimeStats GetScaledMonsterStats()
    {
        if (monsterBase == null)
        {
            Debug.LogError($"WaveConfig {name}: monsterBase�� �������� �ʾҽ��ϴ�!");
            return null;
        }

        return monsterBase.CreateRuntimeStats(waveIndex);
    }

    // ���� ���� �����ϸ��� ���� ���
    public EnemyRuntimeStats GetScaledBossStats()
    {
        if (bossEnemyStats == null)
        {
            Debug.LogError($"WaveConfig {name}: bossEnemyStats�� �������� �ʾҽ��ϴ�!");
            return null;
        }

        return bossEnemyStats.CreateRuntimeStats(waveIndex);
    }

    // ���� ���̺����� Ȯ��
    public bool IsBossWave()
    {
        return waveType == WaveType.Boss;
    }

    // ���̺� ��ȣ�� ���� ���̺����� �ڵ� �Ǵ� (5���̺긶��)
    public static bool IsBossWaveByNumber(int waveNumber)
    {
        return waveNumber % 5 == 0;
    }

    // �� ������ ���� �� ��� (���� ���̺�� �Ϲ� ���͸� ���)
    public int GetTotalSpawnCount()
    {
        int continuousSpawns = Mathf.FloorToInt(durationSec * spawnRatePerSec);
        return initialEnemies + continuousSpawns;
    }

    // ���� ���� ��� (���� ������)
    public float GetSpawnInterval()
    {
        return spawnRatePerSec > 0 ? 1f / spawnRatePerSec : float.MaxValue;
    }

    // ���̺� ������ ����׿����� ���
    public void PrintWaveInfo()
    {
        string waveTypeText = IsBossWave() ? "����" : "�Ϲ�";
        Debug.Log($"���̺� {waveIndex} ({waveTypeText}): {durationSec}��, " +
                  $"�ʱ� {initialEnemies}����, �ʴ� {spawnRatePerSec}����");

        if (IsBossWave() && spawnBossAfterClear)
        {
            Debug.Log($"�Ϲ� ���� óġ �� ���� ����!");
        }
    }

    // ���̺� ���� (�����Ϳ��� Ȯ�ο�)
    private void OnValidate()
    {
        // ���� ����
        waveIndex = Mathf.Max(1, waveIndex);
        durationSec = Mathf.Max(10f, durationSec);
        initialEnemies = Mathf.Max(0, initialEnemies);
        spawnRatePerSec = Mathf.Max(0f, spawnRatePerSec);
        spawnRadius = Mathf.Max(5f, spawnRadius);
        minDistanceFromPlayer = Mathf.Max(3f, minDistanceFromPlayer);

        // �����ϸ� �� ����
        hpScalePerWave = Mathf.Max(1f, hpScalePerWave);
        atkScalePerWave = Mathf.Max(1f, atkScalePerWave);
        msScalePerWave = Mathf.Max(1f, msScalePerWave);

        // ���� �Ÿ� ����
        if (minDistanceFromPlayer >= spawnRadius)
        {
            spawnRadius = minDistanceFromPlayer + 2f;
        }

        // ���� ���̺� �ڵ� ����
        if (IsBossWaveByNumber(waveIndex))
        {
            waveType = WaveType.Boss;
            canShowSkillCards = true; // ���� ���̺�� ��ų ī�� ǥ��
        }
        else
        {
            waveType = WaveType.Normal;
        }
    }
}