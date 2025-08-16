using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("�÷��̾� ����")]
    public PlayerStats playerStats;

    [Header("���� ����")]
    public float currentXP = 0f;
    public int level = 1;

    private SkillManager skillManager;

    // ���� ���� ������Ƽ�� (PlayerStats���� ���� ��������)
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
            // ���� ���� �� ���� ����
            playerStats.ResetToDefault();
        }
    }

    public void AddExperience(float xp)
    {
        currentXP += xp;
        // ������ üũ�� GameManager���� ó��
    }

    public void Heal(float amount)
    {
        playerStats.currentHP = Mathf.Min(playerStats.currentHP + amount, playerStats.maxHP);
    }


    // ī�� �ý��ۿ��� ȣ���� ���� ���� �޼����
    public void IncreaseAttackPower(float percentage)
    {
        playerStats.IncreaseAttackDamage(percentage);
        Debug.Log($"���ݷ� {percentage}% ����! ����: {playerStats.attackDamage:F1}");
    }

    public void IncreaseMoveSpeed(float percentage)
    {
        playerStats.IncreaseMoveSpeed(percentage);
        Debug.Log($"�̵��ӵ� {percentage}% ����! ����: {playerStats.moveSpeed:F1}");
    }

    public void IncreaseHealth(float percentage)
    {
        playerStats.IncreaseMaxHP(percentage);
        Debug.Log($"ü�� {percentage}% ����! ����: {playerStats.maxHP:F1}");
    }

    public void IncreaseAttackSpeed(float percentage)
    {
        playerStats.IncreaseAttackSpeed(percentage);
        Debug.Log($"���ݼӵ� {percentage}% ����! ����: {playerStats.attackSpeed:F1}");
    }

    public void IncreaseAttackRange(float percentage)
    {
        playerStats.attackRange *= (1f + percentage / 100f);
        Debug.Log($"���ݹ��� {percentage}% ����! ����: {playerStats.attackRange:F1}");
    }
}