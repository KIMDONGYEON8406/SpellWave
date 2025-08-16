using UnityEngine;

public class SkillInstance : MonoBehaviour
{
    [Header("��ų ����")]
    public SkillData skillData;
    public int currentLevel = 1;

    private float lastUseTime;
    private Character owner;

    // ������ ���� ���� ��� (PlayerStats ���)
    public float CurrentDamage
    {
        get
        {
            float baseDamage = skillData.GetDamageAtLevel(currentLevel);
            // PlayerStats�� ���ݷ� ���ʽ� ����
            float playerAttackBonus = owner != null ? owner.AttackPower : 0f;
            return baseDamage + (playerAttackBonus * 0.5f); // �÷��̾� ���ݷ��� 50% �߰�
        }
    }

    public float CurrentCooldown => skillData.GetCooldownAtLevel(currentLevel);

    public float CurrentRange
    {
        get
        {
            float baseRange = skillData.GetRangeAtLevel(currentLevel);
            // PlayerStats�� ���� ���� ���ʽ� ����
            float playerRangeBonus = owner != null ? owner.AttackRange : 0f;
            return baseRange + (playerRangeBonus * 0.3f); // �÷��̾� ������ 30% �߰�
        }
    }

    public int MaxLevel => skillData.maxLevel;
    public bool CanLevelUp => currentLevel < MaxLevel;

    public void Initialize(Character character, SkillData data)
    {
        owner = character;
        skillData = data;
        currentLevel = 1;

        if (owner == null)
        {
            Debug.LogError("Character owner�� null�Դϴ�!");
        }

        if (skillData == null)
        {
            Debug.LogError("SkillData�� null�Դϴ�!");
        }

        Debug.Log($"��ų �ʱ�ȭ: {skillData?.skillName} Lv.{currentLevel}");
    }

    public void LevelUp()
    {
        if (CanLevelUp)
        {
            currentLevel++;
            Debug.Log($"{skillData.skillName} ������! Lv.{currentLevel} " +
                     $"(������: {CurrentDamage:F1}, ��Ÿ��: {CurrentCooldown:F1}��)");
        }
        else
        {
            Debug.LogWarning($"{skillData.skillName}�� �̹� �ִ� �����Դϴ�! (Lv.{MaxLevel})");
        }
    }

    void Update()
    {
        // �нú� ��ų�� �ڵ� �ߵ����� ����
        if (!skillData.isPassive && CanUseSkill())
        {
            if (HasTargetsInRange())
            {
                UseSkill();
                lastUseTime = Time.time;
            }
        }
    }

    bool CanUseSkill()
    {
        return Time.time - lastUseTime >= CurrentCooldown;
    }

    bool HasTargetsInRange()
    {
        if (owner == null) return false;

        // �ֺ��� ���� �ִ��� üũ
        Collider[] enemies = Physics.OverlapSphere(owner.transform.position, CurrentRange, LayerMask.GetMask("Enemy"));
        return enemies.Length > 0;
    }

    void UseSkill()
    {
        if (skillData == null || owner == null) return;

        // ��ų Ÿ�Կ� ���� �ٸ��� ó��
        switch (skillData.skillType)
        {
            case SkillType.Projectile:
                UseProjectileSkill();
                break;
            case SkillType.AreaAttack:
                UseAreaAttackSkill();
                break;
            case SkillType.AreaDOT:
                UseAreaDOTSkill();
                break;
            case SkillType.Buff:
                UseBuffSkill();
                break;
            case SkillType.Summon:
                UseSummonSkill();
                break;
            case SkillType.Passive:
                // �нú�� ���⼭ ó������ ����
                break;
        }
    }

    void UseProjectileSkill()
    {
        // ���� ����� �� ã��
        Collider[] enemies = Physics.OverlapSphere(owner.transform.position, CurrentRange, LayerMask.GetMask("Enemy"));

        if (enemies.Length > 0)
        {
            Transform target = GetNearestEnemy(enemies);

            if (target != null)
            {
                // Ÿ�� �������� �߻�
                Vector3 direction = (target.position - owner.transform.position).normalized;

                // ����Ʈ�� ������ ����
                if (skillData.castEffectPrefab != null)
                {
                    Instantiate(skillData.castEffectPrefab, owner.transform.position,
                               Quaternion.LookRotation(direction));
                }

                Debug.Log($"{skillData.skillName} �߻�! Ÿ��: {target.name}, ������: {CurrentDamage:F1}");

                // ���� �߻�ü ������ ���⼭ ���� (�������� �ִ� ���)
                // TODO: �߻�ü ���� ����
            }
        }
    }

    void UseAreaAttackSkill()
    {
        // ���� �� ��� ������ ������
        Collider[] enemies = Physics.OverlapSphere(owner.transform.position, CurrentRange, LayerMask.GetMask("Enemy"));

        foreach (Collider enemy in enemies)
        {
            // TODO: ������ ������ ���� ����
            Debug.Log($"{skillData.skillName} ���� ����! {enemy.name}���� {CurrentDamage:F1} ������");
        }

        // ����Ʈ ����
        if (skillData.hitEffectPrefab != null)
        {
            Instantiate(skillData.hitEffectPrefab, owner.transform.position, Quaternion.identity);
        }
    }

    void UseAreaDOTSkill()
    {
        // ���� ������ ���� ����
        Debug.Log($"{skillData.skillName} ���� ������ ���� ����! ������: {CurrentDamage:F1}/��");

        // TODO: ���� ������ ���� ���� ����
    }

    void UseBuffSkill()
    {
        // �÷��̾� ���� �ӽ� ����
        Debug.Log($"{skillData.skillName} ���� ����!");

        // TODO: �ӽ� ���� ȿ�� ����
    }

    void UseSummonSkill()
    {
        // �̴Ͼ� ��ȯ
        Debug.Log($"{skillData.skillName} ��ȯ!");

        // TODO: �̴Ͼ� ��ȯ ����
    }

    Transform GetNearestEnemy(Collider[] enemies)
    {
        Transform nearest = null;
        float shortestDistance = float.MaxValue;

        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(owner.transform.position, enemy.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    // ��ų ���� ���ڿ� ��ȯ (UI��)
    public string GetSkillInfo()
    {
        return $"{skillData.skillName} Lv.{currentLevel}\n" +
               $"������: {CurrentDamage:F1}\n" +
               $"��Ÿ��: {CurrentCooldown:F1}��\n" +
               $"����: {CurrentRange:F1}";
    }
}