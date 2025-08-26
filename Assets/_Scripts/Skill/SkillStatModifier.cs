using UnityEngine;
using System.Collections.Generic;

// 플레이어에 붙어서 전역 스킬 스탯 관리
public class SkillStatModifier : MonoBehaviour
{
    private static SkillStatModifier instance;
    public static SkillStatModifier Instance
    {
        get
        {
            if (instance == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    instance = player.GetComponent<SkillStatModifier>();
                    if (instance == null)
                    {
                        instance = player.AddComponent<SkillStatModifier>();
                    }
                }
            }
            return instance;
        }
    }

    [System.Serializable]
    public class StatModifiers
    {
        public float allDamageBonus = 0f;        // 모든 스킬 데미지 보너스 (%)
        public float allCooldownReduction = 0f;   // 모든 스킬 쿨타임 감소 (%)
        public float allRangeBonus = 0f;         // 모든 스킬 범위 보너스 (%)

        public float projectileDamageBonus = 0f;
        public float projectileCooldownReduction = 0f;
        public float projectileSpeedBonus = 0f;

        public float areaDamageBonus = 0f;
        public float areaCooldownReduction = 0f;
        public float areaRangeBonus = 0f;

        public float dotDamageBonus = 0f;
        public float dotDurationBonus = 0f;
    }

    [Header("글로벌 스킬 스탯 보너스")]
    public StatModifiers globalModifiers = new StatModifiers();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    // 스탯 증가 적용
    public void ApplyStatBoost(StatType statType, float percentage)
    {
        DebugManager.LogSkill($"글로벌 스탯 증가: {statType} +{percentage}%");

        switch (statType)
        {
            case StatType.AllSkillDamage:
                globalModifiers.allDamageBonus += percentage;
                break;
            case StatType.AllSkillCooldown:
                globalModifiers.allCooldownReduction += percentage;
                break;
            case StatType.AllSkillRange:
                globalModifiers.allRangeBonus += percentage;
                break;

            case StatType.ProjectileDamage:
                globalModifiers.projectileDamageBonus += percentage;
                break;
            case StatType.ProjectileCooldown:
                globalModifiers.projectileCooldownReduction += percentage;
                break;
            case StatType.ProjectileSpeed:
                globalModifiers.projectileSpeedBonus += percentage;
                break;

            case StatType.AreaDamage:
                globalModifiers.areaDamageBonus += percentage;
                break;
            case StatType.AreaCooldown:
                globalModifiers.areaCooldownReduction += percentage;
                break;
            case StatType.AreaRange:
                globalModifiers.areaRangeBonus += percentage;
                break;

            case StatType.DOTDamage:
                globalModifiers.dotDamageBonus += percentage;
                break;
            case StatType.DOTDuration:
                globalModifiers.dotDurationBonus += percentage;
                break;
        }

        // 현재 있는 스킬들에 즉시 적용
        ApplyToExistingSkills();
    }

    // 현재 보유한 스킬들에 적용
    private void ApplyToExistingSkills()
    {
        var skillManager = GetComponent<SkillManager>();
        if (skillManager != null)
        {
            var skills = skillManager.GetAllSkills();
            foreach (var skill in skills)
            {
                ApplyModifiersToSkill(skill);
            }

            if (skills.Count > 0)
            {
                DebugManager.LogSkill($"{skills.Count}개 스킬에 글로벌 보너스 적용");
            }
        }
    }

    // 새 스킬 획득 시 호출
    public void OnSkillAdded(SkillInstance skill)
    {
        if (skill == null) return;

        ApplyModifiersToSkill(skill);
        DebugManager.LogSkill($"{skill.skillData.baseSkillType}에 글로벌 보너스 적용");
    }

    // 개별 스킬에 보너스 적용
    private void ApplyModifiersToSkill(SkillInstance skill)
    {
        if (skill == null || skill.skillData == null) return;

        // 초기화 (중복 방지)
        skill.damageMultiplier = 1f;
        skill.cooldownMultiplier = 1f;
        skill.rangeMultiplier = 1f;
        skill.projectileSpeedMultiplier = 1f;
        skill.durationMultiplier = 1f;

        // 전체 스킬 보너스
        skill.damageMultiplier += globalModifiers.allDamageBonus / 100f;
        skill.cooldownMultiplier *= (1f - globalModifiers.allCooldownReduction / 100f);
        skill.rangeMultiplier += globalModifiers.allRangeBonus / 100f;

        // 타입별 보너스
        if (skill.skillData.HasTag(SkillTag.Projectile))
        {
            skill.damageMultiplier += globalModifiers.projectileDamageBonus / 100f;
            skill.cooldownMultiplier *= (1f - globalModifiers.projectileCooldownReduction / 100f);
            skill.projectileSpeedMultiplier += globalModifiers.projectileSpeedBonus / 100f;
        }

        if (skill.skillData.HasTag(SkillTag.Area) || skill.skillData.baseSkillType == "Aura")
        {
            skill.damageMultiplier += globalModifiers.areaDamageBonus / 100f;
            skill.cooldownMultiplier *= (1f - globalModifiers.areaCooldownReduction / 100f);
            skill.rangeMultiplier += globalModifiers.areaRangeBonus / 100f;
        }

        if (skill.skillData.HasTag(SkillTag.DOT))
        {
            skill.damageMultiplier += globalModifiers.dotDamageBonus / 100f;
            skill.durationMultiplier += globalModifiers.dotDurationBonus / 100f;
        }
    }

    // 디버그용 상태 출력
    public void PrintGlobalStats()
    {
        DebugManager.LogSeparator("글로벌 스킬 스탯");
        DebugManager.LogSkill($"전체 데미지: +{globalModifiers.allDamageBonus}%");
        DebugManager.LogSkill($"전체 쿨타임: -{globalModifiers.allCooldownReduction}%");
        DebugManager.LogSkill($"전체 범위: +{globalModifiers.allRangeBonus}%");

        if (globalModifiers.projectileDamageBonus > 0)
            DebugManager.LogSkill($"발사체 데미지: +{globalModifiers.projectileDamageBonus}%");
        if (globalModifiers.areaDamageBonus > 0)
            DebugManager.LogSkill($"영역 데미지: +{globalModifiers.areaDamageBonus}%");
    }
}