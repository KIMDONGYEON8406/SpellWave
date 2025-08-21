using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("플레이어 스탯")]
    public PlayerStats playerStats;

    [Header("현재 상태")]
    public float currentXP = 0f;
    public int level = 1;

    private SkillManager skillManager;

    public float Health => playerStats.currentHP;
    public float MaxHealth => playerStats.maxHP;
    public float AttackPower => playerStats.attackDamage;
    public float MoveSpeed => playerStats.moveSpeed;
    public float AttackRange => playerStats.attackRange;
    public float AttackSpeed => playerStats.attackSpeed;

    void Awake()
    {
        skillManager = GetComponent<SkillManager>();

        if (skillManager == null)
            skillManager = gameObject.AddComponent<SkillManager>();
    }

    void Start()
    {
        InitializePlayer();
    }

    void InitializePlayer()
    {
        if (playerStats != null)
        {
            playerStats.ResetToDefault();
        }
    }

    public void AddExperience(float xp)
    {
        currentXP += xp;
    }

    public void Heal(float amount)
    {
        playerStats.currentHP = Mathf.Min(playerStats.currentHP + amount, playerStats.maxHP);
    }


    public void IncreaseAttackPower(float percentage)
    {
        playerStats.IncreaseAttackDamage(percentage);
        Debug.Log($"공격력 {percentage}% 증가! 현재: {playerStats.attackDamage:F1}");
    }

    public void IncreaseMoveSpeed(float percentage)
    {
        playerStats.IncreaseMoveSpeed(percentage);
        Debug.Log($"이동속도 {percentage}% 증가! 현재: {playerStats.moveSpeed:F1}");
    }

    public void IncreaseHealth(float percentage)
    {
        playerStats.IncreaseMaxHP(percentage);
        Debug.Log($"체력 {percentage}% 증가! 현재: {playerStats.maxHP:F1}");
    }

    public void IncreaseAttackSpeed(float percentage)
    {
        playerStats.IncreaseAttackSpeed(percentage);
        Debug.Log($"공격속도 {percentage}% 증가! 현재: {playerStats.attackSpeed:F1}");
    }

    public void IncreaseAttackRange(float percentage)
    {
        playerStats.attackRange *= (1f + percentage / 100f);
        Debug.Log($"공격범위 {percentage}% 증가! 현재: {playerStats.attackRange:F1}");
    }
}