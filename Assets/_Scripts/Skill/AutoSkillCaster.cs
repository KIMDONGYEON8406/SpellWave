// 먼저 DebugManager.cs DetailedSettings에 추가할 부분:
/*
[Header("전투 시스템 세부")]
public bool enemyDamage = true;
public bool playerDamage = true;
public bool passiveEffects = true;
public bool autoSkillCasting = true;    // 새로 추가
public bool targetingSystem = true;     // 새로 추가
public bool skillCooldowns = true;      // 새로 추가
public bool multiCastSystem = true;     // 새로 추가
*/

// DebugManager.cs 간편 메서드에 추가:
/*
public static void LogAutoSkillCasting(string message)
{
    if (Instance?.detailSettings.autoSkillCasting == true)
        LogCombat($"[autoSkillCasting] {message}");
}

public static void LogTargetingSystem(string message)
{
    if (Instance?.detailSettings.targetingSystem == true)
        LogCombat($"[targetingSystem] {message}");
}

public static void LogSkillCooldowns(string message)
{
    if (Instance?.detailSettings.skillCooldowns == true)
        LogCombat($"[skillCooldowns] {message}");
}

public static void LogMultiCastSystem(string message)
{
    if (Instance?.detailSettings.multiCastSystem == true)
        LogCombat($"[multiCastSystem] {message}");
}
*/

// =======================================================================================
// AutoSkillCaster.cs 수정 버전
// =======================================================================================
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
            DebugManager.LogError(LogCategory.Combat, "[autoSkillCasting] SkillManager 컴포넌트를 찾을 수 없습니다!");
        }
        else
        {
            DebugManager.LogAutoSkillCasting("AutoSkillCaster 초기화 완료");
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
                DebugManager.LogTargetingSystem($"현재 타겟: {currentTarget.name}");
            }
            else
            {
                DebugManager.LogTargetingSystem("타겟 없음");
            }
            lastDebugLogTime = Time.time;
        }
    }

    void FindNearestTarget()
    {
        Player player = GetComponent<Player>();
        float searchRange = player != null ? player.AttackRange : targetSearchRadius;

        Collider[] enemies = Physics.OverlapSphere(transform.position, searchRange, enemyLayer);

        if (enemies.Length == 0)
        {
            if (currentTarget != null && showTargetingLogs)
            {
                DebugManager.LogTargetingSystem("타겟 범위 이탈");
            }
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
            if (nearestEnemy != null)
            {
                DebugManager.LogTargetingSystem($"새 타겟 발견: {nearestEnemy.name} (거리: {nearestDistance:F1}m)");
            }
        }

        currentTarget = nearestEnemy;
    }

    void AutoCastSkills()
    {
        if (skillManager == null)
        {
            if (frameCounter % 300 == 0)
            {
                DebugManager.LogError(LogCategory.Combat, "[autoSkillCasting] SkillManager가 없습니다!");
            }
            return;
        }

        var skills = skillManager.GetAllSkills();

        if (skills.Count == 0)
        {
            if (frameCounter % 300 == 0)
            {
                DebugManager.LogWarning(LogCategory.Combat, "[autoSkillCasting] 스킬이 하나도 없습니다!");
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
                else if (showCastingLogs && frameCounter % 120 == 0)
                {
                    DebugManager.LogAutoSkillCasting($"{slot.skill.skillData.baseSkillType} 사거리 부족 (거리: {distance:F1}m, 범위: {slot.skill.CurrentRange:F1}m)");
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
                    DebugManager.LogAutoSkillCasting($"스킬 슬롯 {i + 1}: {skills[i].skillData.baseSkillType} (쿨타임: {skillSlots[i].cooldown:F1}초)");
                }
            }
            else
            {
                float newCooldown = skills[i].CurrentCooldown;
                if (Mathf.Abs(skillSlots[i].cooldown - newCooldown) > 0.01f)
                {
                    if (showCastingLogs)
                    {
                        DebugManager.LogSkillCooldowns($"{skills[i].skillData.baseSkillType} 쿨타임 변경: {skillSlots[i].cooldown:F1} → {newCooldown:F1}초");
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

        // 오라 중복 생성 방지
        if (skill.skillData.baseSkillType == "Aura")
        {
            Transform existingAura = transform.Find("PermanentAura");
            if (existingAura != null && existingAura.gameObject.activeInHierarchy)
            {
                if (showCastingLogs && frameCounter % 300 == 0)
                {
                    DebugManager.LogAutoSkillCasting("오라가 이미 존재하여 스킵");
                }
                return;
            }
        }

        // 스킬별 상세 디버그 (특정 스킬만)
        if (skill.skillData.baseSkillType == "Explosion" || skill.skillData.baseSkillType == "Missile")
        {
            DebugManager.LogAutoSkillCasting($"{skill.skillData.baseSkillType} 시전 준비:");
            DebugManager.LogAutoSkillCasting($"  범위: {skill.CurrentRange:F1}m (배율: {skill.rangeMultiplier:F2}x)");
            DebugManager.LogAutoSkillCasting($"  데미지: {skill.CurrentDamage:F1} (배율: {skill.damageMultiplier:F2}x)");
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

            // 발사체 개수 모디파이어 적용
            var countModifier = GetComponent<ProjectileCountModifier>();
            if (countModifier != null)
            {
                int totalCount = countModifier.GetTotalCount(skill.skillData.baseSkillType);
                context.BaseProjectileCount = totalCount;

                if (totalCount > 1)
                {
                    DebugManager.LogAutoSkillCasting($"{skill.skillData.baseSkillType} 개수: {totalCount}개");
                }
            }

            // 다중시전 확률 적용
            var multiCast = GetComponent<MultiCastSystem>();
            if (multiCast != null)
            {
                context.MultiCastChance = multiCast.GetMultiCastChance(skill.skillData.baseSkillType);

                if (context.MultiCastChance > 0)
                {
                    DebugManager.LogMultiCastSystem($"{skill.skillData.baseSkillType} 다중시전 확률: {context.MultiCastChance:F1}%");
                }
            }

            // 스킬 실행 가능 여부 체크
            if (skill.skillData.skillBehavior.CanExecute(context))
            {
                DebugManager.LogAutoSkillCasting($"{skill.skillData.baseSkillType} 시전! (타겟: {currentTarget.name})");

                skill.skillData.skillBehavior.Execute(context);

                // 다중시전 처리
                if (context.MultiCastChance > 0 && !context.IsMultiCastInstance)
                {
                    multiCast?.ProcessMultiCast(skill, context, null);
                }

                slot.Cast();
                skill.RecordSkillUse();

                DebugManager.LogSkillCooldowns($"{skill.skillData.baseSkillType} 쿨타임 시작 ({slot.cooldown:F1}초)");
            }
            else
            {
                if (showCastingLogs)
                {
                    DebugManager.LogAutoSkillCasting($"{skill.skillData.baseSkillType} 실행 조건 불충족");
                }
            }
        }
        else
        {
            DebugManager.LogError(LogCategory.Combat, $"[autoSkillCasting] {skill.skillData.baseSkillType}에 SkillBehavior가 없습니다!");
        }
    }

    // 디버그 전용 메서드들
    [ContextMenu("디버그/타겟팅 정보")]
    void DebugPrintTargetInfo()
    {
        DebugManager.LogSeparator("타겟팅 정보");

        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            DebugManager.LogTargetingSystem($"현재 타겟: {currentTarget.name}");
            DebugManager.LogTargetingSystem($"거리: {distance:F1}m");
        }
        else
        {
            DebugManager.LogTargetingSystem("현재 타겟 없음");
        }

        Player player = GetComponent<Player>();
        float searchRange = player != null ? player.AttackRange : targetSearchRadius;
        DebugManager.LogTargetingSystem($"탐색 범위: {searchRange:F1}m");

        Collider[] enemies = Physics.OverlapSphere(transform.position, searchRange, enemyLayer);
        DebugManager.LogTargetingSystem($"범위 내 적: {enemies.Length}마리");
    }

    [ContextMenu("디버그/스킬 슬롯 상태")]
    void DebugPrintSkillSlots()
    {
        DebugManager.LogSeparator("스킬 슬롯 상태");

        for (int i = 0; i < skillSlots.Count; i++)
        {
            var slot = skillSlots[i];
            if (slot.skill != null)
            {
                float remainingCooldown = Mathf.Max(0, slot.cooldown - (Time.time - slot.lastCastTime));
                string status = slot.IsReady ? "준비완료" : $"쿨타임 {remainingCooldown:F1}초";

                DebugManager.LogSkillCooldowns($"슬롯 {i + 1}: {slot.skill.skillData.baseSkillType} - {status}");
            }
        }
    }

    [ContextMenu("디버그/모든 스킬 강제 시전")]
    void DebugForceAllSkills()
    {
        if (currentTarget == null)
        {
            DebugManager.LogWarning(LogCategory.Combat, "[autoSkillCasting] 타겟이 없어서 강제 시전 불가");
            return;
        }

        DebugManager.LogAutoSkillCasting("디버그 명령: 모든 스킬 강제 시전");

        foreach (var slot in skillSlots)
        {
            if (slot.skill != null)
            {
                slot.lastCastTime = 0; // 쿨타임 리셋
                CastSkill(slot);
            }
        }
    }

    [ContextMenu("디버그/쿨타임 모두 리셋")]
    void DebugResetAllCooldowns()
    {
        DebugManager.LogSkillCooldowns("디버그 명령: 모든 쿨타임 리셋");

        foreach (var slot in skillSlots)
        {
            if (slot.skill != null)
            {
                slot.lastCastTime = 0;
            }
        }

        DebugManager.LogSkillCooldowns($"{skillSlots.Count}개 스킬 쿨타임 리셋 완료");
    }

    // 타겟 강제 설정 (디버그용)
    public void SetDebugTarget(Transform target)
    {
        currentTarget = target;
        DebugManager.LogTargetingSystem($"디버그 타겟 설정: {(target != null ? target.name : "null")}");
    }

    // 현재 타겟 가져오기
    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }

    // 스킬 슬롯 정보 가져오기
    //public List<SkillSlot> GetSkillSlots()
    //{
    //    return skillSlots;
    //}
}