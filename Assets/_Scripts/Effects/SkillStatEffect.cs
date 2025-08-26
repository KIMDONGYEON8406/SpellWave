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

        // 발사체 개수는 별도 처리
        if (statToModify == StatType.ProjectileCount)
        {
            ApplyProjectileCount(player, value);
            return;
        }

        // 기존 글로벌 스탯 처리
        var statModifier = SkillStatModifier.Instance;
        if (statModifier == null)
        {
            DebugManager.LogError(LogCategory.Skill, "SkillStatModifier를 찾을 수 없음!");
            return;
        }

        statModifier.ApplyStatBoost(statToModify, value);

        SkillManager skillManager = player.GetComponent<SkillManager>();
        if (skillManager != null)
        {
            var skills = skillManager.GetAllSkills();
            if (skills.Count > 0)
            {
                DebugManager.LogImportant($"{effectName}: {skills.Count}개 스킬에 즉시 적용");
            }
            else
            {
                DebugManager.LogImportant($"{effectName}: 글로벌 보너스 저장 (나중에 획득할 스킬에 적용)");
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
                    // 모든 스킬에 적용
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
                if (!multiCast.GetMultiCastChance(skillName).Equals(0))
                    continue;

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

    private void ApplyProjectileCount(Player player, float value)
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

        // 타겟 모드에 따라 적용
        if (targetMode == SkillTargetMode.All)
        {
            // 오라 제외한 모든 스킬
            foreach (var skill in skills)
            {
                if (skill.skillData.baseSkillType != "Aura")
                {
                    countMod.AddProjectileCount(skill.skillData.baseSkillType, (int)value);
                    affectedCount++;
                }
            }
        }
        else
        {
            // 태그별 적용
            foreach (var skill in skills)
            {
                if (ShouldApplyToSkill(skill))
                {
                    countMod.AddProjectileCount(skill.skillData.baseSkillType, (int)value);
                    affectedCount++;
                }
            }
        }

        // 미래 스킬을 위한 기본값 설정
        if (targetMode == SkillTargetMode.All || affectedCount == 0)
        {
            string[] defaultSkills = { "Bolt", "Arrow", "Missile", "Explosion" };
            foreach (string skillName in defaultSkills)
            {
                if (!countMod.HasSkill(skillName))
                {
                    countMod.AddProjectileCount(skillName, (int)value);
                }
            }
            DebugManager.LogSkill($"발사체/영역 개수 +{(int)value} (미래 스킬에도 적용)");
        }

        if (affectedCount > 0)
        {
            DebugManager.LogImportant($"{affectedCount}개 스킬에 개수 +{(int)value}개 적용");
        }
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
        if (statToModify == StatType.ProjectileCount)
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
        else
        {
            return $"{targetText} {statText} +{value}%";
        }
    }

    private string GetTargetDescription()
    {
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
        if (statToModify == StatType.AllSkillMultiCast) return "모든 스킬";

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

    private void UpdateAuraSize(SkillInstance auraSkill)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Transform aura = player.transform.Find("PermanentAura");
        if (aura == null) return;

        float newRadius = auraSkill.CurrentRange;

        var dotArea = aura.GetComponent<ElementalDOTArea>();
        if (dotArea != null)
        {
            dotArea.radius = newRadius;
        }

        var collider = aura.GetComponent<SphereCollider>();
        if (collider != null)
        {
            collider.radius = newRadius;
        }

        foreach (Transform child in aura)
        {
            if (child.name.Contains("Freeze") || child.name.Contains("Visual"))
            {
                float scale = newRadius * 2f;
                child.localScale = new Vector3(scale, child.localScale.y, scale);
            }
        }

        DebugManager.LogSkill($"오라 크기 업데이트: {newRadius:F1}m");
    }
}