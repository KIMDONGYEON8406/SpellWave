using UnityEngine;

[CreateAssetMenu(fileName = "New TimelineConfig", menuName = "SpellWave/Timeline Config")]
public class TimelineConfig : ScriptableObject
{
    [Header("타임라인 기본 설정")]
    [Tooltip("스테이지 총 시간 (초) - 10분")]
    public float totalDuration = 600f; // 10분

    [Header("경험치 시스템")]
    [Tooltip("레벨업에 필요한 기본 경험치")]
    public int baseExpToLevelUp = 100;
    [Tooltip("레벨당 경험치 증가량")]
    public float expIncreasePerLevel = 1.2f;
    [Tooltip("최대 레벨")]
    public int maxLevel = 50;

    [Header("몬스터 설정")]
    [Tooltip("사용할 기본 몬스터 데이터")]
    public EnemyStats monsterBase;
    [Tooltip("시간에 따른 몬스터 강화 배율")]
    public float difficultyScaleOverTime = 1.5f;

    [Header("스폰 설정")]
    [Tooltip("초기 몬스터 수")]
    public int initialEnemies = 3;
    [Tooltip("초당 스폰 몬스터 수")]
    public float spawnRatePerSec = 0.3f;
    [Tooltip("시간에 따른 스폰률 증가")]
    public float spawnRateIncrease = 1.3f;

    [Header("스폰 위치")]
    public float spawnRadius = 15f;
    public float minDistanceFromPlayer = 8f;

    // 진척도 계산 (0~100%)
    public float GetProgress(float currentTime)
    {
        return Mathf.Clamp01(currentTime / totalDuration) * 100f;
    }

    // 현재 시간에 따른 난이도 배율
    public float GetDifficultyMultiplier(float currentTime)
    {
        float progress = currentTime / totalDuration;
        return 1f + (progress * difficultyScaleOverTime);
    }

    // 현재 시간에 따른 스폰률
    public float GetCurrentSpawnRate(float currentTime)
    {
        float progress = currentTime / totalDuration;
        return spawnRatePerSec * (1f + progress * spawnRateIncrease);
    }

    // 레벨별 필요 경험치 계산
    public int GetExpToLevelUp(int currentLevel)
    {
        return Mathf.RoundToInt(baseExpToLevelUp * Mathf.Pow(expIncreasePerLevel, currentLevel - 1));
    }

    // 스테이지 완료 여부
    public bool IsStageComplete(float currentTime)
    {
        return currentTime >= totalDuration;
    }
}