using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerStats", menuName = "SpellWave/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [Header("�̵� ����")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("ü�� ����")]
    public float maxHP = 100f;
    public float currentHP = 100f;

    [Header("���� ����")]
    public float attackRange = 8f;
    public float attackSpeed = 1f; // �ʴ� ���� Ƚ��
    public float attackDamage = 25f;

    [Header("����ü ����")]
    public float projectileSpeed = 10f;
    public float projectileLifetime = 3f;

    // ��Ÿ�ӿ� ������ �����ϴ� �Լ� (���� ���� �� ���)
    public void ResetToDefault()
    {
        currentHP = maxHP;
    }

    // ī�� �ý��ۿ��� ����� ���� ���� �Լ���
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
        // �ִ� ü���� �����ϸ� ���� ü�µ� ����ؼ� ����
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

    // ������ ���� �Լ���
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