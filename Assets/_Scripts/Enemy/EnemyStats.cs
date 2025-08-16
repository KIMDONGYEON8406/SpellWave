using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyStats", menuName = "SpellWave/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("�⺻ ����")]
    public float baseHP = 100f;
    public float baseATK = 25f;
    public float baseMoveSpeed = 2f;

    [Header("�ൿ ����")]
    public float followRange = 10f;
    public float attackRange = 5f;
    public float attackInterval = 1f; // ���� �ֱ� (��)
    public float contactDamage = 10f; // ���� ����

    [Header("���� ����")]
    public string enemyName = "�⺻ ��";
    [TextArea(2, 3)]
    public string description = "�� ����";

    // ���̺꺰 �����ϸ� ����� ������ ����ϴ� �Լ���
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

    // ���� ������ �����ؼ� ��Ÿ�� ������ ����
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

// ��Ÿ�ӿ� ����� �� ���� Ŭ����
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

    // ������ �ޱ�
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
    }

    // �׾����� Ȯ��
    public bool IsDead()
    {
        return currentHP <= 0;
    }

    // ü�� ���� ��ȯ (UI��)
    public float GetHealthPercentage()
    {
        return maxHP > 0 ? currentHP / maxHP : 0f;
    }
}