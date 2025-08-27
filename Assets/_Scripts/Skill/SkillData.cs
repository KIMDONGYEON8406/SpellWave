using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Data", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string skillName;
    [TextArea(2, 4)]
    public string description;
    public Sprite skillIcon;

    [Header("��ų Ÿ��")]
    public SkillType skillType;

    [Header("�⺻ ����")]
    public float baseDamage = 50f;
    public float baseCooldown = 2f;
    public float baseRange = 5f;
    public int maxLevel = 5;

    [Header("������ ������")]
    public float damagePerLevel = 10f;
    public float cooldownReductionPerLevel = 0.1f; // ������ ��Ÿ�� 10% ����
    public float rangeIncreasePerLevel = 0.5f;

    [Header("��ų ������")]
    public GameObject skillPrefab; // ���� ��ų ������ �� ������

    [Header("����Ʈ ������ (���û���)")]
    public GameObject hitEffectPrefab;
    public GameObject castEffectPrefab;

    [Header("��ų Ư��")]
    public bool canLevelUp = true;
    public bool isPassive = false; // �нú� ��ų ����

    // ������ ���� ������ ���
    public float GetDamageAtLevel(int level)
    {
        return baseDamage + (damagePerLevel * (level - 1));
    }

    // ������ ���� ��Ÿ�� ���
    public float GetCooldownAtLevel(int level)
    {
        float reduction = cooldownReductionPerLevel * (level - 1);
        return Mathf.Max(0.1f, baseCooldown * (1f - reduction));
    }

    // ������ ���� ���� ���
    public float GetRangeAtLevel(int level)
    {
        return baseRange + (rangeIncreasePerLevel * (level - 1));
    }
}

// ��ų Ÿ�� enum
public enum SkillType
{
    Projectile,     // �߻�ü (���̾)
    AreaAttack,     // �������� (���׿�)
    AreaDOT,        // ���ӵ����� (���̾��)
    Passive,        // �нú� (ȭ��)
    Buff,           // ���� (���ݷ� ����)
    Summon          // ��ȯ (�̴Ͼ� ��ȯ)
}