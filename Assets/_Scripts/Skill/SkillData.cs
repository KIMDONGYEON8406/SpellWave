using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Skill Data", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("스킬 동작")]
    public SkillBehavior skillBehavior;

    [Header("기본 정보")]
    public string baseSkillType = "Bolt";

    [Header("원소 속성")]
    public ElementType element = ElementType.Energy;

    [Header("주 타입 (여러 개 선택 가능)")]
    public List<PrimarySkillType> primaryTypes = new List<PrimarySkillType>();

    [Header("부 타입 - 특성 (여러 개 선택 가능)")]
    public List<SecondarySkillType> secondaryTypes = new List<SecondarySkillType>();

    [Header("발사체/영역 개수")]
    public int baseProjectileCount = 1;
    public int baseAreaCount = 1;

    [Header("발사 패턴")]
    public float spreadAngle = 15f;
    public bool useCircularPattern = false;

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

    [Header("추가 이펙트 (선택사항)")]
    public GameObject muzzleEffectPrefab;   // 머즐 이펙트
    public GameObject castEffectPrefab;      // 시전 이펙트
    public float muzzleEffectDuration = 1f;
    public float hitEffectDuration = 2f;

    [Header("진화 설정")]
    public SkillData evolvedVersion;        // 6단계 진화 스킬
    public ElementType fusionElement = ElementType.Energy;  // 융합 원소

    [Header("스킬 특성")]
    public bool canLevelUp = true;
    public bool isPassive = false;
    public bool autoTarget = true;
    public float autoCastPriority = 1f;

    // 주 타입 체크
    public bool HasPrimaryType(PrimarySkillType type)
    {
        return primaryTypes.Contains(type);
    }

    // 부 타입 체크
    public bool HasSecondaryType(SecondarySkillType type)
    {
        return secondaryTypes.Contains(type);
    }

    // 기존 SkillTag 호환성을 위한 메서드
    public bool HasTag(SkillTag tag)
    {
        switch (tag)
        {
            case SkillTag.Projectile:
                return HasPrimaryType(PrimarySkillType.Projectile);
            case SkillTag.Area:
                return HasPrimaryType(PrimarySkillType.Area);
            case SkillTag.DOT:
                return HasPrimaryType(PrimarySkillType.DOT);
            case SkillTag.Homing:
                return HasSecondaryType(SecondarySkillType.Homing);
            case SkillTag.Pierce:
                return HasSecondaryType(SecondarySkillType.Pierce);
            case SkillTag.Instant:
                return HasSecondaryType(SecondarySkillType.Instant);
            case SkillTag.SingleTarget:
                return HasSecondaryType(SecondarySkillType.SingleTarget);
            case SkillTag.MultiTarget:
                return HasSecondaryType(SecondarySkillType.MultiTarget);
        }
        return false;
    }

    public bool HasAnyTag(params SkillTag[] tags)
    {
        return tags.Any(tag => HasTag(tag));
    }

    public string GetDisplayName()
    {
        return SkillNameGenerator.GetSkillName(baseSkillType, element);
    }

    public string GetTypeDescription()
    {
        List<string> types = new List<string>();

        // 주 타입들 추가
        foreach (var primaryType in primaryTypes)
        {
            switch (primaryType)
            {
                case PrimarySkillType.Projectile:
                    types.Add("발사체");
                    break;
                case PrimarySkillType.Area:
                    types.Add("영역");
                    break;
                case PrimarySkillType.DOT:
                    types.Add("지속");
                    break;
            }
        }

        // 부 타입들 추가
        foreach (var secondaryType in secondaryTypes)
        {
            switch (secondaryType)
            {
                case SecondarySkillType.Homing:
                    types.Add("유도");
                    break;
                case SecondarySkillType.Pierce:
                    types.Add("관통");
                    break;
                case SecondarySkillType.Instant:
                    types.Add("즉시");
                    break;
            }
        }

        if (types.Count == 0) return "기본";
        return string.Join(", ", types);
    }

    public string GetCardDescription()
    {
        string baseDesc = description;
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