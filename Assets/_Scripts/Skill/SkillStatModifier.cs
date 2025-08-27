using UnityEngine;
using System.Collections.Generic;

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

        // 초기화
        skill.damageMultiplier = 1f;
        skill.cooldownMultiplier = 1f;
        skill.rangeMultiplier = 1f;
        skill.projectileSpeedMultiplier = 1f;
        skill.durationMultiplier = 1f;

        // 전체 스킬 보너스 적용
        ApplyBonus(skill, StatType.AllSkillDamage, ref skill.damageMultiplier, false);
        ApplyBonus(skill, StatType.AllSkillCooldown, ref skill.cooldownMultiplier, true);
        ApplyBonus(skill, StatType.AllSkillRange, ref skill.rangeMultiplier, false);

        // 발사체 타입 보너스
        if (skill.skillData.HasTag(SkillTag.Projectile))
        {
            ApplyBonus(skill, StatType.ProjectileDamage, ref skill.damageMultiplier, false);
            ApplyBonus(skill, StatType.ProjectileCooldown, ref skill.cooldownMultiplier, true);
            ApplyBonus(skill, StatType.ProjectileSpeed, ref skill.projectileSpeedMultiplier, false);
        }

        // 영역 타입 보너스
        if (skill.skillData.HasTag(SkillTag.Area) || skill.skillData.baseSkillType == "Aura")
        {
            ApplyBonus(skill, StatType.AreaDamage, ref skill.damageMultiplier, false);
            ApplyBonus(skill, StatType.AreaCooldown, ref skill.cooldownMultiplier, true);
            ApplyBonus(skill, StatType.AreaRange, ref skill.rangeMultiplier, false);
        }

        // DOT 타입 보너스
        if (skill.skillData.HasTag(SkillTag.DOT))
        {
            ApplyBonus(skill, StatType.DOTDamage, ref skill.damageMultiplier, false);
            ApplyBonus(skill, StatType.DOTDuration, ref skill.durationMultiplier, false);
        }

        DebugManager.LogSkill($"{skill.skillData.baseSkillType} 최종 배율 - DMG:{skill.damageMultiplier:F2} CD:{skill.cooldownMultiplier:F2} RNG:{skill.rangeMultiplier:F2}");
    }

    private void ApplyBonus(SkillInstance skill, StatType statType, ref float multiplier, bool isReduction)
    {
        if (globalStatBoosts.ContainsKey(statType))
        {
            float bonus = globalStatBoosts[statType] / 100f;

            if (isReduction)
                multiplier *= (1f - bonus);  // 쿨타임 감소
            else
                multiplier += bonus;  // 증가
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