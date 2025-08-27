using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SkillStatEffect", menuName = "Game/Card Effects/Skill Stat")]
public class SkillStatEffect : CardEffect
{
    [Header("대상 설정")]
    public SkillTargetMode targetMode = SkillTargetMode.All;
    public List<SkillTag> requiredTags = new List<SkillTag>();

    [Header("강화할 스탯")]
    public StatType statToModify;

    [Header("디버그")]
    [SerializeField] private bool verboseLogging = false;

    public enum SkillTargetMode
    {
        All,
        ByTag,
        ByType
    }

    public override void ApplyEffect(Player player, float value)
    {
        // 다중시전 처리는 기존 코드 유지
        if (statToModify == StatType.ProjectileMultiCast ||
            statToModify == StatType.AreaMultiCast ||
            statToModify == StatType.DOTMultiCast ||
            statToModify == StatType.AllSkillMultiCast)
        {
            ApplyMultiCastWithType(player, value);
            return;
        }

        // 개수 증가 처리는 기존 코드 유지
        if (statToModify == StatType.ProjectileCount ||
            statToModify == StatType.AreaCount)
        {
            ApplyCountIncrease(player, value);
            return;
        }

        // 일반 스탯 처리 - 개선된 방식
        var statModifier = SkillStatModifier.Instance;
        if (statModifier == null)
        {
            DebugManager.LogError(LogCategory.Skill, "SkillStatModifier를 찾을 수 없음!");
            return;
        }

        // 글로벌 보너스 적용 및 즉시 갱신
        statModifier.ApplyStatBoost(statToModify, value);

        DebugManager.LogImportant($"{effectName} 효과 적용 완료: {GetStatDescription()} +{value}%");
    }

    private void ApplyMultiCastWithType(Player player, float value)
    {
        var multiCast = player.GetComponent<MultiCastSystem>();
        if (multiCast == null)
        {
            multiCast = player.gameObject.AddComponent<MultiCastSystem>();
            DebugManager.LogSkill("MultiCastSystem 컴포넌트 생성");
        }

        var skillManager = player.GetComponent<SkillManager>();
        var skills = skillManager.GetAllSkills();
        int affectedCount = 0;

        foreach (var skill in skills)
        {
            // 오라는 항상 제외
            if (skill.skillData.baseSkillType == "Aura")
            {
                continue;
            }

            bool shouldApply = false;
            StatType castType = StatType.ProjectileMultiCast;

            switch (statToModify)
            {
                case StatType.AllSkillMultiCast:
                    shouldApply = true;
                    castType = skill.skillData.HasTag(SkillTag.Projectile) ?
                        StatType.ProjectileMultiCast : StatType.AreaMultiCast;
                    break;

                case StatType.ProjectileMultiCast:
                    shouldApply = skill.skillData.HasTag(SkillTag.Projectile);
                    castType = StatType.ProjectileMultiCast;
                    break;

                case StatType.AreaMultiCast:
                    shouldApply = skill.skillData.HasTag(SkillTag.Area) ||
                                 skill.skillData.baseSkillType == "Explosion";
                    castType = StatType.AreaMultiCast;
                    break;

                case StatType.DOTMultiCast:
                    shouldApply = skill.skillData.HasTag(SkillTag.DOT);
                    castType = StatType.AreaMultiCast;
                    break;
            }

            if (shouldApply)
            {
                multiCast.AddMultiCastChance(skill.skillData.baseSkillType, value, castType);
                affectedCount++;
            }
        }

        // 미래 스킬을 위한 기본값
        if (statToModify == StatType.AllSkillMultiCast || affectedCount == 0)
        {
            string[] defaultSkills = { "Bolt", "Arrow", "Missile", "Explosion" };
            foreach (string skillName in defaultSkills)
            {
                StatType type = (skillName == "Explosion") ?
                    StatType.AreaMultiCast : StatType.ProjectileMultiCast;
                multiCast.AddMultiCastChance(skillName, value, type);
            }
        }

        if (affectedCount > 0)
        {
            string typeText = statToModify == StatType.AllSkillMultiCast ? "모든" :
                             statToModify == StatType.ProjectileMultiCast ? "발사체" :
                             statToModify == StatType.AreaMultiCast ? "영역" : "지속";

            DebugManager.LogImportant($"{typeText} 스킬 {affectedCount}개에 다중시전 +{value}% (오라 제외)");
        }
    }

    private void ApplyCountIncrease(Player player, float value)
    {
        var countMod = player.GetComponent<ProjectileCountModifier>();
        if (countMod == null)
        {
            countMod = player.gameObject.AddComponent<ProjectileCountModifier>();
            DebugManager.LogSkill("ProjectileCountModifier 컴포넌트 생성");
        }

        var skillManager = player.GetComponent<SkillManager>();
        var skills = skillManager.GetAllSkills();
        int affectedCount = 0;

        // 현재 보유한 스킬에 적용
        foreach (var skill in skills)
        {
            bool shouldApply = false;
            string skillName = skill.skillData.baseSkillType;

            switch (statToModify)
            {
                case StatType.ProjectileCount:
                    shouldApply = skill.skillData.HasTag(SkillTag.Projectile) ||
                                 skillName == "Bolt" || skillName == "Arrow" || skillName == "Missile";
                    break;
                case StatType.AreaCount:
                    shouldApply = skill.skillData.HasTag(SkillTag.Area) ||
                                 skillName == "Explosion";
                    break;
            }

            if (shouldApply)
            {
                countMod.AddProjectileCount(skillName, (int)value);
                affectedCount++;
                DebugManager.LogImportant($"{skillName} 개수 +{(int)value}");
            }
        }

        // Bolt가 없으면 미래를 위해 추가
        if (!countMod.HasSkill("Bolt"))
        {
            countMod.AddProjectileCount("Bolt", (int)value);
            DebugManager.LogImportant($"Bolt(미래) 개수 +{(int)value}");
        }

        string typeText = statToModify == StatType.ProjectileCount ? "발사체" : "영역";
        DebugManager.LogImportant($"{typeText} 스킬 {affectedCount}개에 개수 +{(int)value} 적용");
    }

    private bool ShouldApplyToSkill(SkillInstance skill)
    {
        if (targetMode == SkillTargetMode.All)
            return true;

        if (targetMode == SkillTargetMode.ByTag && requiredTags.Count > 0)
        {
            foreach (var tag in requiredTags)
            {
                if (skill.skillData.HasTag(tag))
                    return true;
            }
            return false;
        }

        return CheckStatTypeCompatibility(skill);
    }

    private bool CheckStatTypeCompatibility(SkillInstance skill)
    {
        switch (statToModify)
        {
            case StatType.ProjectileDamage:
            case StatType.ProjectileCooldown:
            case StatType.ProjectileSpeed:
            case StatType.ProjectileCount:
            case StatType.ProjectileMultiCast:
                return skill.skillData.HasTag(SkillTag.Projectile);

            case StatType.AreaDamage:
            case StatType.AreaCooldown:
            case StatType.AreaRange:
            case StatType.AreaCount:
                return skill.skillData.HasTag(SkillTag.Area) ||
                       skill.skillData.baseSkillType == "Explosion";
            case StatType.AreaMultiCast:
                return skill.skillData.HasTag(SkillTag.Area) ||
                       skill.skillData.baseSkillType == "Explosion" ||
                       skill.skillData.baseSkillType == "Aura";

            case StatType.DOTDamage:
            case StatType.DOTTickRate:
            case StatType.DOTDuration:
            case StatType.DOTMultiCast:
                return skill.skillData.HasTag(SkillTag.DOT);

            case StatType.AllSkillDamage:
            case StatType.AllSkillCooldown:
            case StatType.AllSkillRange:
            case StatType.AllSkillMultiCast:
                return true;

            default:
                return false;
        }
    }

    public override string GetPreviewText(float value)
    {
        string targetText = GetTargetDescription();
        string statText = GetStatDescription();

        // 특수 케이스 처리
        if (statToModify == StatType.ProjectileCount || statToModify == StatType.AreaCount)
        {
            return $"{targetText} 개수 +{(int)value}개";
        }
        else if (statToModify == StatType.ProjectileMultiCast ||
                 statToModify == StatType.AreaMultiCast ||
                 statToModify == StatType.DOTMultiCast ||
                 statToModify == StatType.AllSkillMultiCast)
        {
            return $"{targetText} 다중시전 확률 +{value}%";
        }
        else if (statToModify.ToString().Contains("Cooldown"))
        {
            return $"{targetText} {statText} -{value}%";
        }
        else if (statToModify == StatType.DOTTickRate)
        {
            return $"{targetText} 틱 속도 +{value}%";
        }
        else
        {
            return $"{targetText} {statText} +{value}%";
        }
    }

    private string GetTargetDescription()
    {
        if (statToModify == StatType.AllSkillMultiCast) return "모든 스킬";
        if (statToModify == StatType.ProjectileCount) return "발사체 스킬";
        if (statToModify == StatType.AreaCount) return "영역 스킬";

        if (targetMode == SkillTargetMode.All)
            return "모든 스킬";

        if (targetMode == SkillTargetMode.ByTag && requiredTags.Count > 0)
        {
            if (requiredTags.Contains(SkillTag.Projectile)) return "발사체 스킬";
            if (requiredTags.Contains(SkillTag.Area)) return "영역 스킬";
            if (requiredTags.Contains(SkillTag.DOT)) return "지속 스킬";
        }

        if (statToModify.ToString().Contains("Projectile")) return "발사체 스킬";
        if (statToModify.ToString().Contains("Area")) return "영역 스킬";
        if (statToModify.ToString().Contains("DOT")) return "지속 스킬";

        return "특정 스킬";
    }

    private string GetStatDescription()
    {
        if (statToModify.ToString().Contains("Damage")) return "데미지";
        if (statToModify.ToString().Contains("Cooldown")) return "쿨타임";
        if (statToModify.ToString().Contains("Range")) return "범위";
        if (statToModify.ToString().Contains("Speed")) return "속도";
        if (statToModify.ToString().Contains("Duration")) return "지속시간";
        if (statToModify.ToString().Contains("Count")) return "개수";
        if (statToModify.ToString().Contains("MultiCast")) return "다중시전 확률";
        if (statToModify.ToString().Contains("TickRate")) return "틱 속도";

        return statToModify.ToString();
    }

   
}