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

        // 디버그 추가
        if (skillManager == null)
        {
            Debug.LogError("SkillManager 컴포넌트를 찾을 수 없습니다!");
        }
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

        currentTarget = nearestEnemy;
    }

    void AutoCastSkills()
    {
        if (skillManager == null)
        {
            Debug.LogError("SkillManager가 없습니다!");
            return;
        }

        var skills = skillManager.GetAllSkills();

        if (skills.Count == 0)
        {
            Debug.LogWarning("스킬이 하나도 없습니다! StaffManager 확인 필요");
            return;
        }

        Debug.Log($"보유 스킬: {skills.Count}개, 타겟: {currentTarget?.name}");

        UpdateSkillSlots(skills);

        foreach (var slot in skillSlots)
        {
            if (slot.skill == null) continue;

            if (slot.IsReady)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.position);
                //Debug.Log($"{slot.skill.skillData.baseSkillType} - 거리: {distance:F1} / 범위: {slot.skill.CurrentRange:F1}");

                if (distance <= slot.skill.CurrentRange)
                {
                    Debug.Log($"발동: {slot.skill.skillData.baseSkillType}");
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

        // 오라는 이미 있으면 스킵
        if (skill.skillData.baseSkillType == "Aura")
        {
            Transform existingAura = transform.Find("PermanentAura");
            if (existingAura != null)
            {
                return;  // 이미 있으면 재시전 안 함
            }

            // 처음 한 번만 생성
            Debug.Log("[AutoSkillCaster] 오라 첫 생성");
        }

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
                skill.RecordSkillUse();
            }
        }
    }
}