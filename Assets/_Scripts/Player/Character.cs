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

}