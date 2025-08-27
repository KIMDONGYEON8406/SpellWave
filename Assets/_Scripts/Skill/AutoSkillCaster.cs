using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AutoSkillCaster : MonoBehaviour
{
    [Header("자동 전투 설정")]
    public float targetSearchRadius = 10f;
    public LayerMask enemyLayer;

    [Header("디버그")]
    [SerializeField] private bool showTargetingLogs = false;
    [SerializeField] private bool showCastingLogs = false;
    [SerializeField] private float debugLogInterval = 1f;

    private List<SkillSlot> skillSlots = new List<SkillSlot>();
    private Transform currentTarget;
    private float lastDebugLogTime;
    private int frameCounter = 0;

    private SkillManager skillManager;
    private CloakManager cloakManager;

    private class SkillSlot
    {
        public SkillInstance skill;
        public float lastCastTime;
        public float cooldown;

        public bool IsReady => Time.time - lastCastTime >= cooldown;

        public void Cast()
        {
            lastCastTime = Time.time;
        }
    }

    void Start()
    {
        skillManager = GetComponent<SkillManager>();
        cloakManager = CloakManager.Instance;

        if (skillManager == null)
        {
            DebugManager.LogError(LogCategory.Combat, "SkillManager 컴포넌트를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        frameCounter++;

        FindNearestTarget();

        if (currentTarget != null)
        {
            AutoCastSkills();
        }

        if (showTargetingLogs && Time.time - lastDebugLogTime > debugLogInterval)
        {
            if (currentTarget != null)
            {
                DebugManager.LogCombat($"현재 타겟: {currentTarget.name}");
            }
            lastDebugLogTime = Time.time;
        }
    }

    void FindNearestTarget()
    {
        Player player = GetComponent<Player>();
        float searchRange = player != null ? player.AttackRange : 10f;

        Collider[] enemies = Physics.OverlapSphere(transform.position, targetSearchRadius, enemyLayer);

        if (enemies.Length == 0)
        {
            currentTarget = null;
            return;
        }

        float nearestDistance = float.MaxValue;
        Transform nearestEnemy = null;

        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }

        if (nearestEnemy != currentTarget && showTargetingLogs)
        {
            DebugManager.LogCombat($"새 타겟 발견: {nearestEnemy?.name}");
        }

        currentTarget = nearestEnemy;
    }

    void AutoCastSkills()
    {
        if (skillManager == null)
        {
            if (frameCounter % 300 == 0)
            {
                DebugManager.LogError(LogCategory.Combat, "SkillManager가 없습니다!");
            }
            return;
        }

        var skills = skillManager.GetAllSkills();

        if (skills.Count == 0)
        {
            if (frameCounter % 300 == 0)
            {
                DebugManager.Log(LogCategory.Combat, "스킬이 하나도 없습니다!", LogLevel.Warning);
            }
            return;
        }

        UpdateSkillSlots(skills);

        foreach (var slot in skillSlots)
        {
            if (slot.skill == null) continue;

            if (slot.IsReady)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.position);

                if (distance <= slot.skill.CurrentRange)
                {
                    CastSkill(slot);
                }
            }
        }
    }

    void UpdateSkillSlots(List<SkillInstance> skills)
    {
        while (skillSlots.Count < skills.Count)
        {
            skillSlots.Add(new SkillSlot());
        }

        for (int i = 0; i < skills.Count; i++)
        {
            if (skillSlots[i].skill != skills[i])
            {
                skillSlots[i].skill = skills[i];
                skillSlots[i].cooldown = skills[i].CurrentCooldown;
                skillSlots[i].lastCastTime = 0;

                if (showCastingLogs)
                {
                    DebugManager.LogSkill($"스킬 슬롯 {i + 1}: {skills[i].skillData.baseSkillType}");
                }
            }
            else
            {
                float newCooldown = skills[i].CurrentCooldown;
                if (Mathf.Abs(skillSlots[i].cooldown - newCooldown) > 0.01f)
                {
                    if (showCastingLogs)
                    {
                        DebugManager.LogSkill($"{skills[i].skillData.baseSkillType} 쿨타임 변경: {skillSlots[i].cooldown:F1} → {newCooldown:F1}");
                    }
                    skillSlots[i].cooldown = newCooldown;
                }
            }
        }
    }

    void CastSkill(SkillSlot slot)
    {
        var skill = slot.skill;
        var element = cloakManager?.GetCurrentElement() ?? ElementType.Energy;
        var passive = cloakManager?.GetCurrentPassive() ?? new PassiveEffect();

        if (skill.skillData.baseSkillType == "Aura")
        {
            Transform existingAura = transform.Find("PermanentAura");
            if (existingAura != null && existingAura.gameObject.activeInHierarchy)
            {
                return;
            }
        }

        // 디버그: 스킬별 스탯 확인
        if (skill.skillData.baseSkillType == "Explosion" || skill.skillData.baseSkillType == "Missile")
        {
            DebugManager.LogImportant($"[{skill.skillData.baseSkillType}] 시전 준비:");
            DebugManager.LogImportant($"  - Range: {skill.CurrentRange:F1}m (Multi: {skill.rangeMultiplier:F2}x)");
            DebugManager.LogImportant($"  - Damage: {skill.CurrentDamage:F1} (Multi: {skill.damageMultiplier:F2}x)");
        }

        if (skill.skillData.skillBehavior != null)
        {
            SkillExecutionContext context = new SkillExecutionContext
            {
                SkillName = skill.skillData.baseSkillType,
                Caster = gameObject,
                Target = currentTarget,
                Damage = skill.CurrentDamage,
                Range = skill.CurrentRange,
                Element = element,
                Passive = passive,
                SkillPrefab = skill.skillData.skillPrefab,
                HitEffectPrefab = skill.skillData.hitEffectPrefab,
                BaseProjectileCount = 1,
                MultiCastChance = 0f,
                IsMultiCastInstance = false
            };

            var countModifier = GetComponent<ProjectileCountModifier>();
            if (countModifier != null)
            {
                int totalCount = countModifier.GetTotalCount(skill.skillData.baseSkillType);
                context.BaseProjectileCount = totalCount;

                if (totalCount > 1)
                {
                    DebugManager.LogImportant($"[{skill.skillData.baseSkillType}] 개수: {totalCount}개");
                }
            }

            var multiCast = GetComponent<MultiCastSystem>();
            if (multiCast != null)
            {
                context.MultiCastChance = multiCast.GetMultiCastChance(skill.skillData.baseSkillType);
            }

            if (skill.skillData.skillBehavior.CanExecute(context))
            {
                skill.skillData.skillBehavior.Execute(context);

                if (context.MultiCastChance > 0 && !context.IsMultiCastInstance)
                {
                    multiCast?.ProcessMultiCast(skill, context, null);
                }

                slot.Cast();
                skill.RecordSkillUse();
            }
        }
    }

    private SkillExecutionContext CloneContext(SkillExecutionContext original)
    {
        return new SkillExecutionContext
        {
            Caster = original.Caster,
            Target = original.Target,
            Damage = original.Damage,
            Range = original.Range,
            Element = original.Element,
            Passive = original.Passive,
            SkillPrefab = original.SkillPrefab,
            HitEffectPrefab = original.HitEffectPrefab,
            BaseProjectileCount = original.BaseProjectileCount,
            MultiCastChance = original.MultiCastChance,
            IsMultiCastInstance = original.IsMultiCastInstance,
            MultiCastIndex = original.MultiCastIndex,
            TotalMultiCasts = original.TotalMultiCasts,
            PositionOffset = original.PositionOffset
        };
    }
    IEnumerator DelayedMultiCast(SkillInstance skill, SkillExecutionContext originalContext, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Context 복제
        var cloneContext = CloneContext(originalContext);
        cloneContext.IsMultiCastInstance = true;  // 무한루프 방지
        cloneContext.Damage *= 0.8f;  // 복제는 80% 데미지
        cloneContext.MultiCastChance = 0;  // 추가 다중시전 방지

        // 영역 스킬인 경우 위치 오프셋 적용
        if (skill.skillData.HasTag(SkillTag.Area))
        {
            Vector2 randomOffset = Random.insideUnitCircle * 2f;
            cloneContext.PositionOffset = new Vector3(randomOffset.x, 0, randomOffset.y);
        }

        // 복제 실행
        skill.skillData.skillBehavior.Execute(cloneContext);

        DebugManager.LogImportant($"[다중시전] {skill.skillData.baseSkillType} 복제 발동!");
    }
}
