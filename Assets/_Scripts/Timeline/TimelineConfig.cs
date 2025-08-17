using UnityEngine;

[CreateAssetMenu(fileName = "New TimelineConfig", menuName = "SpellWave/Timeline Config")]
public class TimelineConfig : ScriptableObject
{
    [Header("Ÿ�Ӷ��� �⺻ ����")]
    [Tooltip("�������� �� �ð� (��) - 10��")]
    public float totalDuration = 600f; // 10��

    [Header("����ġ �ý���")]
    [Tooltip("�������� �ʿ��� �⺻ ����ġ")]
    public int baseExpToLevelUp = 100;
    [Tooltip("������ ����ġ ������")]
    public float expIncreasePerLevel = 1.2f;
    [Tooltip("�ִ� ����")]
    public int maxLevel = 50;

    [Header("���� ����")]
    [Tooltip("����� �⺻ ���� ������")]
    public EnemyStats monsterBase;
    [Tooltip("�ð��� ���� ���� ��ȭ ����")]
    public float difficultyScaleOverTime = 1.5f;

    [Header("���� ����")]
    [Tooltip("�ʱ� ���� ��")]
    public int initialEnemies = 3;
    [Tooltip("�ʴ� ���� ���� ��")]
    public float spawnRatePerSec = 0.3f;
    [Tooltip("�ð��� ���� ������ ����")]
    public float spawnRateIncrease = 1.3f;

    [Header("���� ��ġ")]
    public float spawnRadius = 15f;
    public float minDistanceFromPlayer = 8f;

    // ��ô�� ��� (0~100%)
    public float GetProgress(float currentTime)
    {
        return Mathf.Clamp01(currentTime / totalDuration) * 100f;
    }

    // ���� �ð��� ���� ���̵� ����
    public float GetDifficultyMultiplier(float currentTime)
    {
        float progress = currentTime / totalDuration;
        return 1f + (progress * difficultyScaleOverTime);
    }

    // ���� �ð��� ���� ������
    public float GetCurrentSpawnRate(float currentTime)
    {
        float progress = currentTime / totalDuration;
        return spawnRatePerSec * (1f + progress * spawnRateIncrease);
    }

    // ������ �ʿ� ����ġ ���
    public int GetExpToLevelUp(int currentLevel)
    {
        return Mathf.RoundToInt(baseExpToLevelUp * Mathf.Pow(expIncreasePerLevel, currentLevel - 1));
    }

    // �������� �Ϸ� ����
    public bool IsStageComplete(float currentTime)
    {
        return currentTime >= totalDuration;
    }
}