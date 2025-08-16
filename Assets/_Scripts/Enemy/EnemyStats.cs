using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyStats", menuName = "SpellWave/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("기본 스탯")]
    public float baseHP = 100f;
    public float baseATK = 25f;
    public float baseMoveSpeed = 2f;

    [Header("행동 설정")]
    public float followRange = 10f;
    public float attackRange = 5f;
    public float attackInterval = 1f; // 공격 주기 (초)
    public float contactDamage = 10f; // 접촉 피해

    [Header("몬스터 정보")]
    public string enemyName = "기본 적";
    [TextArea(2, 3)]
    public string description = "적 설명";

    // 웨이브별 스케일링 적용된 스탯을 계산하는 함수들
    public float GetScaledHP(int waveIndex, float hpScalePerWave = 1.2f)
    {
        return baseHP * Mathf.Pow(hpScalePerWave, waveIndex - 1);
    }

    public float GetScaledATK(int waveIndex, float atkScalePerWave = 1.15f)
    {
        return baseATK * Mathf.Pow(atkScalePerWave, waveIndex - 1);
    }

    public float GetScaledMoveSpeed(int waveIndex, float msScalePerWave = 1.1f)
    {
        return baseMoveSpeed * Mathf.Pow(msScalePerWave, waveIndex - 1);
    }

    // 몬스터 스탯을 복사해서 런타임 데이터 생성
    public EnemyRuntimeStats CreateRuntimeStats(int currentWave = 1)
    {
        EnemyRuntimeStats runtimeStats = new EnemyRuntimeStats();

        runtimeStats.maxHP = GetScaledHP(currentWave);
        runtimeStats.currentHP = runtimeStats.maxHP;
        runtimeStats.attackDamage = GetScaledATK(currentWave);
        runtimeStats.moveSpeed = GetScaledMoveSpeed(currentWave);
        runtimeStats.followRange = followRange;
        runtimeStats.attackRange = attackRange;
        runtimeStats.attackInterval = attackInterval;
        runtimeStats.contactDamage = contactDamage;

        return runtimeStats;
    }
}

// 런타임에 사용할 적 스탯 클래스
[System.Serializable]
public class EnemyRuntimeStats
{
    public float maxHP;
    public float currentHP;
    public float attackDamage;
    public float moveSpeed;
    public float followRange;
    public float attackRange;
    public float attackInterval;
    public float contactDamage;

    // 데미지 받기
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
    }

    // 죽었는지 확인
    public bool IsDead()
    {
        return currentHP <= 0;
    }

    // 체력 비율 반환 (UI용)
    public float GetHealthPercentage()
    {
        return maxHP > 0 ? currentHP / maxHP : 0f;
    }
}