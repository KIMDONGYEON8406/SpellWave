using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Skill Data", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("스킬 동작")]
    public SkillBehavior skillBehavior;

    [Header("기본 정보")]
    public string baseSkillType = "Bolt";

    [Header("스킬 태그")]
    public List<SkillTag> skillTags = new List<SkillTag>();

    [TextArea(2, 4)]
    public string description;
    public Sprite skillIcon;

    [Header("기본 스탯")]
    public float baseDamage = 50f;
    public float baseCooldown = 2f;
    public float baseRange = 5f;
    public int maxLevel = 5;

    [Header("레벨별 증가량")]
    public float damagePerLevel = 10f;
    public float cooldownReductionPerLevel = 0.1f;
    public float rangeIncreasePerLevel = 0.5f;

    [Header("스킬 프리팹")]
    public GameObject skillPrefab;
    public GameObject hitEffectPrefab;
    public GameObject castEffectPrefab;

    [Header("스킬 특성")]
    public bool canLevelUp = true;
    public bool isPassive = false;
    public bool autoTarget = true;
    public float autoCastPriority = 1f;

    public bool HasTag(SkillTag tag)
    {
        return skillTags.Contains(tag);
    }

    public bool HasAnyTag(params SkillTag[] tags)
    {
        foreach (var tag in tags)
        {
            if (skillTags.Contains(tag))
                return true;
        }
        return false;
    }

    public string GetDisplayName(ElementType element)
    {
        return SkillNameGenerator.GetSkillName(baseSkillType, element);
    }
    public string GetTypeDescription()
    {
        List<string> types = new List<string>();

        if (HasTag(SkillTag.Projectile)) types.Add("발사체");
        if (HasTag(SkillTag.Area)) types.Add("영역");
        if (HasTag(SkillTag.DOT)) types.Add("지속");
        if (HasTag(SkillTag.Instant)) types.Add("즉시");
        if (HasTag(SkillTag.Homing)) types.Add("유도");

        if (types.Count == 0) return "기본";

        return string.Join(", ", types);
    }

    public string GetCardDescription(ElementType element)
    {
        string baseDesc = description;

        // 타입 표시 추가
        string typeInfo = GetTypeDescription();
        if (!string.IsNullOrEmpty(typeInfo))
        {
            baseDesc = $"타입: {typeInfo}\n{baseDesc}";
        }

        return baseDesc;
    }

    public float GetDamageAtLevel(int level)
    {
        return baseDamage + (damagePerLevel * (level - 1));
    }

    public float GetCooldownAtLevel(int level)
    {
        float reduction = cooldownReductionPerLevel * (level - 1);
        return Mathf.Max(0.1f, baseCooldown * (1f - reduction));
    }

    public float GetRangeAtLevel(int level)
    {
        return baseRange + (rangeIncreasePerLevel * (level - 1));
    }
}

[System.Flags]
public enum SkillTag
{
    SingleTarget = 1 << 0,
    MultiTarget = 1 << 1,
    Projectile = 1 << 2,
    Area = 1 << 3,
    DOT = 1 << 4,
    Homing = 1 << 5,
    Pierce = 1 << 6,
    Instant = 1 << 8,
}