using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    private Dictionary<StatType, float> globalStatBoosts = new Dictionary<StatType, float>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    public void ApplyStatBoost(StatType statType, float percentage)
    {
        if (!globalStatBoosts.ContainsKey(statType))
            globalStatBoosts[statType] = 0f;

        globalStatBoosts[statType] += percentage;

        DebugManager.LogSkill($"글로벌 스탯 증가: {statType} +{percentage}% (총: {globalStatBoosts[statType]}%)");

        // 즉시 모든 스킬 갱신
        RefreshAllSkills();
    }

    private void RefreshAllSkills()
    {
        var skillManager = GetComponent<SkillManager>();
        if (skillManager != null)
        {
            var skills = skillManager.GetAllSkills();
            foreach (var skill in skills)
            {
                RecalculateSkillStats(skill);
            }

            DebugManager.LogImportant($"모든 스킬 스탯 갱신 완료 ({skills.Count}개)");
        }
    }

    public void OnSkillAdded(SkillInstance skill)
    {
        if (skill == null) return;
        RecalculateSkillStats(skill);
        DebugManager.LogSkill($"{skill.skillData.baseSkillType}에 글로벌 보너스 적용 완료");
    }

    private void RecalculateSkillStats(SkillInstance skill)
    {
        if (skill == null || skill.skillData == null) return;

        // 초기화 (1.0부터 시작)
        skill.damageMultiplier = 1f;
        skill.cooldownMultiplier = 1f;
        skill.rangeMultiplier = 1f;
        skill.projectileSpeedMultiplier = 1f;
        skill.durationMultiplier = 1f;

        // 모든 글로벌 스탯에 대해 체크
        foreach (var statBoost in globalStatBoosts)
        {
            StatType statType = statBoost.Key;
            float bonusValue = statBoost.Value;

            // 이 스킬에 적용되어야 하는 스탯인지 확인
            if (ShouldApplyToSkill(skill, statType))
            {
                ApplyStatToSkill(skill, statType, bonusValue);
            }
        }

        DebugManager.LogSkill($"{skill.skillData.baseSkillType} 최종 배율 - DMG:{skill.damageMultiplier:F2} CD:{skill.cooldownMultiplier:F2} RNG:{skill.rangeMultiplier:F2}");
    }

    // 스킬이 특정 StatType의 대상인지 확인 (다중 주타입 지원)
    private bool ShouldApplyToSkill(SkillInstance skill, StatType statType)
    {
        // AllSkill 계열은 모든 스킬에 적용
        if (statType.ToString().Contains("AllSkill"))
            return true;

        // StatType에 따른 주타입 매칭
        if (statType.ToString().Contains("Projectile"))
        {
            return skill.skillData.HasPrimaryType(PrimarySkillType.Projectile);
        }

        if (statType.ToString().Contains("Area"))
        {
            return skill.skillData.HasPrimaryType(PrimarySkillType.Area);
        }

        if (statType.ToString().Contains("DOT"))
        {
            return skill.skillData.HasPrimaryType(PrimarySkillType.DOT);
        }

        return false;
    }

    // 개별 스탯을 스킬에 적용
    private void ApplyStatToSkill(SkillInstance skill, StatType statType, float bonusValue)
    {
        float bonus = bonusValue / 100f;

        switch (statType)
        {
            // 데미지 관련
            case StatType.AllSkillDamage:
            case StatType.ProjectileDamage:
            case StatType.AreaDamage:
            case StatType.DOTDamage:
                skill.damageMultiplier *= (1f + bonus);
                break;

            // 쿨타임 관련
            case StatType.AllSkillCooldown:
            case StatType.ProjectileCooldown:
            case StatType.AreaCooldown:
                skill.cooldownMultiplier *= (1f - bonus);
                break;

            // 범위 관련
            case StatType.AllSkillRange:
            case StatType.AreaRange:
                skill.rangeMultiplier *= (1f + bonus);
                break;

            // 속도 관련
            case StatType.ProjectileSpeed:
                skill.projectileSpeedMultiplier *= (1f + bonus);
                break;

            // 지속시간 관련
            case StatType.DOTDuration:
                skill.durationMultiplier *= (1f + bonus);
                break;
        }
    }

    public float GetStatBoost(StatType statType)
    {
        return globalStatBoosts.ContainsKey(statType) ? globalStatBoosts[statType] : 0f;
    }

    public void PrintGlobalStats()
    {
        if (globalStatBoosts.Count == 0)
        {
            DebugManager.LogSkill("글로벌 스탯 보너스 없음");
            return;
        }

        DebugManager.LogSeparator("글로벌 스킬 스탯");
        foreach (var kvp in globalStatBoosts)
        {
            string sign = kvp.Key.ToString().Contains("Cooldown") ? "-" : "+";
            DebugManager.LogSkill($"{kvp.Key}: {sign}{kvp.Value}%");
        }
    }

    [ContextMenu("디버그/모든 스킬 스탯 갱신")]
    void DebugRefreshAllSkills()
    {
        RefreshAllSkills();
    }
}