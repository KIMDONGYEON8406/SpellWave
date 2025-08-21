using UnityEngine;

public class SkillInstance : MonoBehaviour
{
    [Header("스킬 정보")]
    public SkillData skillData;
    public int currentLevel = 1;

    private float lastUseTime;
    private Character owner;

    // 레벨에 따른 스탯 계산 (PlayerStats 고려)
    public float CurrentDamage
    {
        get
        {
            float baseDamage = skillData.GetDamageAtLevel(currentLevel);
            // PlayerStats의 공격력 보너스 적용
            float playerAttackBonus = owner != null ? owner.AttackPower : 0f;
            return baseDamage + (playerAttackBonus * 0.5f); // 플레이어 공격력의 50% 추가
        }
    }

    public float CurrentCooldown => skillData.GetCooldownAtLevel(currentLevel);

    public float CurrentRange
    {
        get
        {
            float baseRange = skillData.GetRangeAtLevel(currentLevel);
            // PlayerStats의 공격 범위 보너스 적용
            float playerRangeBonus = owner != null ? owner.AttackRange : 0f;
            return baseRange + (playerRangeBonus * 0.3f); // 플레이어 범위의 30% 추가
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
            Debug.LogError("Character owner가 null입니다!");
        }

        if (skillData == null)
        {
            Debug.LogError("SkillData가 null입니다!");
        }

        Debug.Log($"스킬 초기화: {skillData?.skillName} Lv.{currentLevel}");
    }

    public void LevelUp()
    {
        if (CanLevelUp)
        {
            currentLevel++;
            Debug.Log($"{skillData.skillName} 레벨업! Lv.{currentLevel} " +
                     $"(데미지: {CurrentDamage:F1}, 쿨타임: {CurrentCooldown:F1}초)");
        }
        else
        {
            Debug.LogWarning($"{skillData.skillName}은 이미 최대 레벨입니다! (Lv.{MaxLevel})");
        }
    }

    void Update()
    {
        // 패시브 스킬은 자동 발동하지 않음
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

        // 주변에 적이 있는지 체크
        Collider[] enemies = Physics.OverlapSphere(owner.transform.position, CurrentRange, LayerMask.GetMask("Enemy"));
        return enemies.Length > 0;
    }

    void UseSkill()
    {
        if (skillData == null || owner == null) return;

        // 스킬 타입에 따라 다르게 처리
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
                // 패시브는 여기서 처리하지 않음
                break;
        }
    }

    void UseProjectileSkill()
    {
        // 가장 가까운 적 찾기
        Collider[] enemies = Physics.OverlapSphere(owner.transform.position, CurrentRange, LayerMask.GetMask("Enemy"));

        if (enemies.Length > 0)
        {
            Transform target = GetNearestEnemy(enemies);

            if (target != null)
            {
                // 타겟 방향으로 발사
                Vector3 direction = (target.position - owner.transform.position).normalized;

                // 이펙트가 있으면 생성
                if (skillData.castEffectPrefab != null)
                {
                    Instantiate(skillData.castEffectPrefab, owner.transform.position,
                               Quaternion.LookRotation(direction));
                }

                Debug.Log($"{skillData.skillName} 발사! 타겟: {target.name}, 데미지: {CurrentDamage:F1}");

                // 실제 발사체 생성은 여기서 구현 (프리팹이 있는 경우)
                // TODO: 발사체 생성 로직
            }
        }
    }

    void UseAreaAttackSkill()
    {
        // 범위 내 모든 적에게 데미지
        Collider[] enemies = Physics.OverlapSphere(owner.transform.position, CurrentRange, LayerMask.GetMask("Enemy"));

        foreach (Collider enemy in enemies)
        {
            // TODO: 적에게 데미지 적용 로직
            Debug.Log($"{skillData.skillName} 범위 공격! {enemy.name}에게 {CurrentDamage:F1} 데미지");
        }

        // 이펙트 생성
        if (skillData.hitEffectPrefab != null)
        {
            Instantiate(skillData.hitEffectPrefab, owner.transform.position, Quaternion.identity);
        }
    }

    void UseAreaDOTSkill()
    {
        // 지속 데미지 구역 생성
        Debug.Log($"{skillData.skillName} 지속 데미지 구역 생성! 데미지: {CurrentDamage:F1}/초");

        // TODO: 지속 데미지 구역 생성 로직
    }

    void UseBuffSkill()
    {
        // 플레이어 스탯 임시 증가
        Debug.Log($"{skillData.skillName} 버프 적용!");

        // TODO: 임시 버프 효과 적용
    }

    void UseSummonSkill()
    {
        // 미니언 소환
        Debug.Log($"{skillData.skillName} 소환!");

        // TODO: 미니언 소환 로직
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

    // 스킬 정보 문자열 반환 (UI용)
    public string GetSkillInfo()
    {
        return $"{skillData.skillName} Lv.{currentLevel}\n" +
               $"데미지: {CurrentDamage:F1}\n" +
               $"쿨타임: {CurrentCooldown:F1}초\n" +
               $"범위: {CurrentRange:F1}";
    }
}