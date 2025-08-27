using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "SpellWave/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("체력 설정")]
    public float maxHP = 100f;
    public float currentHP = 100f;

    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("전투 설정")]
    public float attackPower = 10f;    // 스킬 데미지 계산용
    public float attackRange = 10f;    // 스킬 자동시전 범위

    // 게임 시작 시 초기화
    public void ResetToDefault()
    {
        currentHP = maxHP;
    }

    // 데미지 받기
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            currentHP = 0;
        }
    }

    // 체력 회복
    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    // 최대 체력 증가 (카드 효과용 - 나중에 사용)
    public void IncreaseMaxHP(float percentage)
    {
        float oldMaxHP = maxHP;
        maxHP *= (1f + percentage / 100f);
        // 현재 체력도 비례 증가
        currentHP = (currentHP / oldMaxHP) * maxHP;
    }

    // 이동속도 증가 (카드 효과용 - 나중에 사용)
    public void IncreaseMoveSpeed(float percentage)
    {
        moveSpeed *= (1f + percentage / 100f);
    }

    // 공격력 증가 (카드 효과용 - 나중에 사용)
    public void IncreaseAttackPower(float percentage)
    {
        attackPower *= (1f + percentage / 100f);
    }
}