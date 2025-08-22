using UnityEngine;
using System.Collections.Generic;

public class AutoSkillCaster : MonoBehaviour
{
    [Header("자동 전투 설정")]
    public float targetSearchRadius = 10f;
    public LayerMask enemyLayer;

    [Header("스킬 발동 정보")]
    private List<SkillSlot> skillSlots = new List<SkillSlot>();
    private Transform currentTarget;

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
    }

    void Update()
    {
        FindNearestTarget();

        if (currentTarget != null)
        {
            AutoCastSkills();
        }
    }

    void FindNearestTarget()
    {
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

        currentTarget = nearestEnemy;
    }

    void AutoCastSkills()
    {
        if (skillManager == null) return;

        var skills = skillManager.GetAllSkills();
        UpdateSkillSlots(skills);

        foreach (var slot in skillSlots)
        {
            if (slot.IsReady && slot.skill != null)
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
            }
            else
            {
                // 쿨타임 업데이트 (카드로 변경됐을 수 있음)
                skillSlots[i].cooldown = skills[i].CurrentCooldown;
            }
        }
    }

    void CastSkill(SkillSlot slot)
    {
        var skill = slot.skill;
        var element = cloakManager?.GetCurrentElement() ?? ElementType.Energy;
        var passive = cloakManager?.GetCurrentPassive() ?? new PassiveEffect();

        // ⭐ SkillBehavior로 모든 스킬 처리
        if (skill.skillData.skillBehavior != null)
        {
            SkillExecutionContext context = new SkillExecutionContext
            {
                Caster = gameObject,
                Target = currentTarget,
                Damage = skill.CurrentDamage,
                Range = skill.CurrentRange,
                Element = element,
                Passive = passive,
                SkillPrefab = skill.skillData.skillPrefab,
                HitEffectPrefab = skill.skillData.hitEffectPrefab
            };

            if (skill.skillData.skillBehavior.CanExecute(context))
            {
                skill.skillData.skillBehavior.Execute(context);
                slot.Cast();
                skill.RecordSkillUse();  // SkillInstance에 사용 기록
            }
        }
    }
}