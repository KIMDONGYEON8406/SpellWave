using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerStats", menuName = "SpellWave/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("체력 설정")]
    public float maxHP = 100f;
    public float currentHP = 100f;

    [Header("공격 설정")]
    public float attackRange = 8f;
    public float attackSpeed = 1f; // 초당 공격 횟수
    public float attackDamage = 25f;

    [Header("투사체 설정")]
    public float projectileSpeed = 10f;
    public float projectileLifetime = 3f;

    // 런타임에 스탯을 리셋하는 함수 (게임 시작 시 사용)
    public void ResetToDefault()
    {
        currentHP = maxHP;
    }

    // 카드 시스템에서 사용할 스탯 증가 함수들
    public void IncreaseAttackSpeed(float percentage)
    {
        attackSpeed *= (1f + percentage / 100f);
    }

    public void IncreaseMoveSpeed(float percentage)
    {
        moveSpeed *= (1f + percentage / 100f);
    }

    public void IncreaseMaxHP(float percentage)
    {
        float oldMaxHP = maxHP;
        maxHP *= (1f + percentage / 100f);
        // 최대 체력이 증가하면 현재 체력도 비례해서 증가
        currentHP = (currentHP / oldMaxHP) * maxHP;
    }

    public void IncreaseAttackDamage(float percentage)
    {
        attackDamage *= (1f + percentage / 100f);
    }

    public void HealPercentage(float percentage)
    {
        currentHP = Mathf.Min(currentHP + (maxHP * percentage / 100f), maxHP);
    }

    // 고정값 증가 함수들
    public void AddAttackSpeed(float value)
    {
        attackSpeed += value;
    }

    public void AddMoveSpeed(float value)
    {
        moveSpeed += value;
    }

    public void AddAttackDamage(float value)
    {
        attackDamage += value;
    }
}