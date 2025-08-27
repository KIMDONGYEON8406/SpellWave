using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Data", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public string skillName;
    [TextArea(2, 4)]
    public string description;
    public Sprite skillIcon;

    [Header("스킬 타입")]
    public SkillType skillType;

    [Header("기본 스탯")]
    public float baseDamage = 50f;
    public float baseCooldown = 2f;
    public float baseRange = 5f;
    public int maxLevel = 5;

    [Header("레벨별 증가량")]
    public float damagePerLevel = 10f;
    public float cooldownReductionPerLevel = 0.1f; // 레벨당 쿨타임 10% 감소
    public float rangeIncreasePerLevel = 0.5f;

    [Header("스킬 프리팹")]
    public GameObject skillPrefab; // 실제 스킬 로직이 들어간 프리팹

    [Header("이펙트 프리팹 (선택사항)")]
    public GameObject hitEffectPrefab;
    public GameObject castEffectPrefab;

    [Header("스킬 특성")]
    public bool canLevelUp = true;
    public bool isPassive = false; // 패시브 스킬 여부

    // 레벨에 따른 데미지 계산
    public float GetDamageAtLevel(int level)
    {
        return baseDamage + (damagePerLevel * (level - 1));
    }

    // 레벨에 따른 쿨타임 계산
    public float GetCooldownAtLevel(int level)
    {
        float reduction = cooldownReductionPerLevel * (level - 1);
        return Mathf.Max(0.1f, baseCooldown * (1f - reduction));
    }

    // 레벨에 따른 범위 계산
    public float GetRangeAtLevel(int level)
    {
        return baseRange + (rangeIncreasePerLevel * (level - 1));
    }
}

// 스킬 타입 enum
public enum SkillType
{
    Projectile,     // 발사체 (파이어볼)
    AreaAttack,     // 범위공격 (메테오)
    AreaDOT,        // 지속데미지 (파이어쉴드)
    Passive,        // 패시브 (화상)
    Buff,           // 버프 (공격력 증가)
    Summon          // 소환 (미니언 소환)
}