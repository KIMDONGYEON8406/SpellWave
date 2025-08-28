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
        // 다중시전 처리
        if (statToModify == StatType.ProjectileMultiCast ||
            statToModify == StatType.AreaMultiCast ||
            statToModify == StatType.DOTMultiCast ||
            statToModify == StatType.AllSkillMultiCast)
        {
            ApplyMultiCastWithType(player, value);
            return;
        }

        // 개수 증가 처리
        if (statToModify == StatType.ProjectileCount ||
            statToModify == StatType.AreaCount)
        {
            ApplyCountIncrease(player, value);
            return;
        }

        // 일반 스탯 처리
        var statModifier = SkillStatModifier.Instance;
        if (statModifier == null)
        {
            DebugManager.LogError(LogCategory.Skill, "SkillStatModifier를 찾을 수 없음!");
            return;
        }

        // 글로벌 보너스 적용 및 즉시 갱신
        statModifier.ApplyStatBoost(statToModify, value);

        // 특별히 범위 관련 스탯인 경우 추가 로깅
        if (statToModify == StatType.AreaRange ||
            statToModify == StatType.AllSkillRange)
        {
            LogRangeBonus(player, value);
        }

        DebugManager.LogImportant($"{effectName} 효과 적용 완료: {GetStatDescription()} +{value}%");
    }

    private void LogRangeBonus(Player player, float value)
    {
        DebugManager.LogImportant($"영역 범위 증가 효과 적용: {GetStatDescription()} +{value}%");

        var skillManager = player.GetComponent<SkillManager>();
        if (skillManager != null)
        {
            var skills = skillManager.GetAllSkills();
            foreach (var skill in skills)
            {
                // 다중 주타입 지원: 영역 또는 DOT 타입 체크
                if (skill.skillData.HasPrimaryType(PrimarySkillType.Area) ||
                    skill.skillData.HasPrimaryType(PrimarySkillType.DOT))
                {
                    DebugManager.LogSkill($"{skill.skillData.baseSkillType} 새 범위: {skill.CurrentRange:F1}m (배율: {skill.rangeMultiplier:F2}x)");
                }
            }
        }
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
                    // 발사체 타입이면 발사체 다중시전, 아니면 영역 다중시전
                    castType = skill.skillData.HasPrimaryType(PrimarySkillType.Projectile) ?
                        StatType.ProjectileMultiCast : StatType.AreaMultiCast;
                    break;

                case StatType.ProjectileMultiCast:
                    shouldApply = skill.skillData.HasPrimaryType(PrimarySkillType.Projectile);
                    castType = StatType.ProjectileMultiCast;
                    break;

                case StatType.AreaMultiCast:
                    shouldApply = skill.skillData.HasPrimaryType(PrimarySkillType.Area);
                    castType = StatType.AreaMultiCast;
                    break;

                case StatType.DOTMultiCast:
                    shouldApply = skill.skillData.HasPrimaryType(PrimarySkillType.DOT);
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
            string typeText = GetMultiCastTypeText(statToModify);
            DebugManager.LogImportant($"{typeText} 스킬 {affectedCount}개에 다중시전 +{value}% (오라 제외)");
        }
    }

    private string GetMultiCastTypeText(StatType statType)
    {
        switch (statType)
        {
            case StatType.AllSkillMultiCast: return "모든";
            case StatType.ProjectileMultiCast: return "발사체";
            case StatType.AreaMultiCast: return "영역";
            case StatType.DOTMultiCast: return "지속";
            default: return "특정";
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

        switch (statToModify)
        {
            case StatType.ProjectileCount:
                countMod.AddProjectileCountToAll((int)value);
                DebugManager.LogImportant($"모든 발사체 스킬 개수 +{(int)value} (미래 스킬 포함)");
                break;

            case StatType.AreaCount:
                countMod.AddAreaCountToAll((int)value);
                DebugManager.LogImportant($"모든 영역 스킬 개수 +{(int)value} (미래 스킬 포함)");
                break;
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