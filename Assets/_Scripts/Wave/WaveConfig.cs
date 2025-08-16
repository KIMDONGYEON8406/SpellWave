using UnityEngine;

[CreateAssetMenu(fileName = "New WaveConfig", menuName = "SpellWave/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("웨이브 기본 정보")]
    public int waveIndex = 1;
    [Tooltip("웨이브 지속 시간 (초)")]
    public float durationSec = 60f;

    [Header("웨이브 타입")]
    public WaveType waveType = WaveType.Normal;

    [Header("일반 몬스터 스폰 설정")]
    [Tooltip("웨이브 시작 시 즉시 스폰할 몬스터 수")]
    public int initialEnemies = 5;
    [Tooltip("초당 스폰할 몬스터 수")]
    public float spawnRatePerSec = 0.5f;
    [Tooltip("사용할 기본 몬스터 데이터")]
    public EnemyStats monsterBase;

    [Header("보스 설정")]
    [Tooltip("보스 웨이브에서 사용할 보스 몬스터 데이터")]
    public EnemyStats bossEnemyStats;
    [Tooltip("일반 몬스터를 모두 처치한 후 보스 스폰 여부")]
    public bool spawnBossAfterClear = true;

    [Header("난이도 스케일링")]
    [Tooltip("웨이브당 체력 증가 배율 (1.2 = 20% 증가)")]
    public float hpScalePerWave = 1.2f;
    [Tooltip("웨이브당 공격력 증가 배율 (1.15 = 15% 증가)")]
    public float atkScalePerWave = 1.15f;
    [Tooltip("웨이브당 이동속도 증가 배율 (1.1 = 10% 증가)")]
    public float msScalePerWave = 1.1f;

    [Header("스폰 위치 설정")]
    [Tooltip("플레이어 주변 스폰 반경")]
    public float spawnRadius = 15f;
    [Tooltip("플레이어로부터 최소 거리")]
    public float minDistanceFromPlayer = 8f;

    [Header("보상 설정")]
    [Tooltip("웨이브 클리어 시 카드 선택 여부")]
    public bool showCardSelection = true;
    [Tooltip("스킬 카드를 보여줄지 (보스 웨이브에서 true)")]
    public bool canShowSkillCards = false;

    // 웨이브 타입 정의
    public enum WaveType
    {
        Normal,  // 일반 웨이브 (1,2,3,4...)
        Boss     // 보스 웨이브 (5,10,15,20...)
    }

    // 일반 몬스터 스케일링된 스탯 계산
    public EnemyRuntimeStats GetScaledMonsterStats()
    {
        if (monsterBase == null)
        {
            Debug.LogError($"WaveConfig {name}: monsterBase가 설정되지 않았습니다!");
            return null;
        }

        return monsterBase.CreateRuntimeStats(waveIndex);
    }

    // 보스 몬스터 스케일링된 스탯 계산
    public EnemyRuntimeStats GetScaledBossStats()
    {
        if (bossEnemyStats == null)
        {
            Debug.LogError($"WaveConfig {name}: bossEnemyStats가 설정되지 않았습니다!");
            return null;
        }

        return bossEnemyStats.CreateRuntimeStats(waveIndex);
    }

    // 보스 웨이브인지 확인
    public bool IsBossWave()
    {
        return waveType == WaveType.Boss;
    }

    // 웨이브 번호로 보스 웨이브인지 자동 판단 (5웨이브마다)
    public static bool IsBossWaveByNumber(int waveNumber)
    {
        return waveNumber % 5 == 0;
    }

    // 총 스폰될 몬스터 수 계산 (보스 웨이브는 일반 몬스터만 계산)
    public int GetTotalSpawnCount()
    {
        int continuousSpawns = Mathf.FloorToInt(durationSec * spawnRatePerSec);
        return initialEnemies + continuousSpawns;
    }

    // 스폰 간격 계산 (지속 스폰용)
    public float GetSpawnInterval()
    {
        return spawnRatePerSec > 0 ? 1f / spawnRatePerSec : float.MaxValue;
    }

    // 웨이브 정보를 디버그용으로 출력
    public void PrintWaveInfo()
    {
        string waveTypeText = IsBossWave() ? "보스" : "일반";
        Debug.Log($"웨이브 {waveIndex} ({waveTypeText}): {durationSec}초, " +
                  $"초기 {initialEnemies}마리, 초당 {spawnRatePerSec}마리");

        if (IsBossWave() && spawnBossAfterClear)
        {
            Debug.Log($"일반 몬스터 처치 후 보스 등장!");
        }
    }

    // 웨이브 검증 (에디터에서 확인용)
    private void OnValidate()
    {
        // 음수 방지
        waveIndex = Mathf.Max(1, waveIndex);
        durationSec = Mathf.Max(10f, durationSec);
        initialEnemies = Mathf.Max(0, initialEnemies);
        spawnRatePerSec = Mathf.Max(0f, spawnRatePerSec);
        spawnRadius = Mathf.Max(5f, spawnRadius);
        minDistanceFromPlayer = Mathf.Max(3f, minDistanceFromPlayer);

        // 스케일링 값 검증
        hpScalePerWave = Mathf.Max(1f, hpScalePerWave);
        atkScalePerWave = Mathf.Max(1f, atkScalePerWave);
        msScalePerWave = Mathf.Max(1f, msScalePerWave);

        // 스폰 거리 검증
        if (minDistanceFromPlayer >= spawnRadius)
        {
            spawnRadius = minDistanceFromPlayer + 2f;
        }

        // 보스 웨이브 자동 설정
        if (IsBossWaveByNumber(waveIndex))
        {
            waveType = WaveType.Boss;
            canShowSkillCards = true; // 보스 웨이브는 스킬 카드 표시
        }
        else
        {
            waveType = WaveType.Normal;
        }
    }
}