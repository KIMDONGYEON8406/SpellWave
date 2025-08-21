using UnityEngine;
using System.Collections.Generic;

public class AutoSkillCaster : MonoBehaviour
{
    [Header("자동 전투 설정")]
    public float targetSearchRadius = 10f;  // 적 탐색 범위
    public LayerMask enemyLayer;           // 적 레이어

    [Header("스킬 발동 정보")]
    private List<SkillSlot> skillSlots = new List<SkillSlot>();
    private Transform currentTarget;

    private SkillManager skillManager;
    private CloakManager cloakManager;

    // 스킬 슬롯 정보
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

        // 스킬 슬롯 초기화는 스킬매니저에서 스킬이 추가될 때 처리
    }

    void Update()
    {
        // 타겟 찾기
        FindNearestTarget();

        // 자동 스킬 발동
        if (currentTarget != null)
        {
            AutoCastSkills();
        }
    }

    // 가장 가까운 적 찾기
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

    // 자동 스킬 발동
    void AutoCastSkills()
    {
        if (skillManager == null) return;

        var skills = skillManager.GetAllSkills();

        // 스킬 슬롯 업데이트
        UpdateSkillSlots(skills);

        // 쿨타임이 된 스킬 발동
        foreach (var slot in skillSlots)
        {
            if (slot.IsReady && slot.skill != null)
            {
                // 스킬 범위 체크
                float distance = Vector3.Distance(transform.position, currentTarget.position);
                if (distance <= slot.skill.CurrentRange)
                {
                    CastSkill(slot);
                }
            }
        }
    }

    // 스킬 슬롯 업데이트
    void UpdateSkillSlots(List<SkillInstance> skills)
    {
        // 슬롯 수 맞추기
        while (skillSlots.Count < skills.Count)
        {
            skillSlots.Add(new SkillSlot());
        }

        // 스킬 정보 업데이트
        for (int i = 0; i < skills.Count; i++)
        {
            if (skillSlots[i].skill != skills[i])
            {
                skillSlots[i].skill = skills[i];
                skillSlots[i].cooldown = skills[i].CurrentCooldown;
                skillSlots[i].lastCastTime = 0;
            }
        }
    }

    // 스킬 발동
    void CastSkill(SkillSlot slot)
    {
        var skill = slot.skill;
        var element = cloakManager.GetCurrentElement();
        var passive = cloakManager.GetCurrentPassive();

        Debug.Log($"자동 발동: {skill.skillData.skillName} ({element} 속성)");

        // 스킬 타입에 따른 처리
        switch (skill.skillData.skillType)
        {
            case SkillType.Projectile:
                CastProjectile(skill, element, passive);
                break;

            case SkillType.AreaAttack:
                CastAreaAttack(skill, element, passive);
                break;

            case SkillType.AreaDOT:
                CastAreaDOT(skill, element, passive);
                break;
        }

        slot.Cast();
    }

    // 발사체 스킬 발동
    void CastProjectile(SkillInstance skill, ElementType element, PassiveEffect passive)
    {
        if (currentTarget == null) return;

        // 프리팹이 있으면 생성
        if (skill.skillData.skillPrefab != null)
        {
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            GameObject projectile = Instantiate(skill.skillData.skillPrefab,
                                               transform.position + Vector3.up,
                                               Quaternion.LookRotation(direction));

            // 발사체에 속성 정보 전달
            //var projScript = projectile.GetComponent<ElementalProjectile>();
            //if (projScript != null)
            //{
            //    projScript.Initialize(skill.CurrentDamage, element, passive, direction);
            //}
        }
    }

    // 영역 공격 발동
    void CastAreaAttack(SkillInstance skill, ElementType element, PassiveEffect passive)
    {
        // 범위 내 모든 적 타격
        Collider[] enemies = Physics.OverlapSphere(transform.position, skill.CurrentRange, enemyLayer);

        foreach (Collider enemy in enemies)
        {
            var enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                // 데미지 적용
                enemyAI.TakeDamage(skill.CurrentDamage);

                // 패시브 효과 적용
                ApplyPassiveEffect(enemyAI, element, passive);
            }
        }

        // 이펙트 생성
        if (skill.skillData.hitEffectPrefab != null)
        {
            Instantiate(skill.skillData.hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    // 지속 영역 생성
    void CastAreaDOT(SkillInstance skill, ElementType element, PassiveEffect passive)
    {
        // DOT 영역 프리팹 생성
        if (skill.skillData.skillPrefab != null)
        {
            GameObject dotArea = Instantiate(skill.skillData.skillPrefab,
                                            transform.position,
                                            Quaternion.identity);

            // DOT 영역에 속성 정보 전달
            //var dotScript = dotArea.GetComponent<ElementalDOTArea>();
            //if (dotScript != null)
            //{
            //    dotScript.Initialize(skill.CurrentDamage, element, passive, skill.CurrentRange);
            //}
        }
    }

    // 패시브 효과 적용
    void ApplyPassiveEffect(EnemyAI enemy, ElementType element, PassiveEffect passive)
    {
        // 속성별 패시브 효과 적용 로직
        switch (passive.type)
        {
            case PassiveType.Burn:
                // 화상 효과 적용
                Debug.Log($"화상 효과 적용: {passive.effectValue}/초, {passive.duration}초");
                break;

            case PassiveType.Slow:
                // 둔화 효과 적용
                Debug.Log($"둔화 효과 적용: {passive.effectValue}% 감소");
                break;

            case PassiveType.Chain:
                // 연쇄 효과 적용
                Debug.Log($"연쇄 효과 적용: {passive.chainCount}명에게 전이");
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        // 탐색 범위 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetSearchRadius);
    }
}