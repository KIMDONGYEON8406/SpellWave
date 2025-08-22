using UnityEngine;

public class SkillInstance : MonoBehaviour
{
    [Header("스킬 정보")]
    public SkillData skillData;
    public int currentLevel = 1;

    [HideInInspector] public float damageMultiplier = 1f;
    [HideInInspector] public float cooldownMultiplier = 1f;
    [HideInInspector] public float rangeMultiplier = 1f;
    [HideInInspector] public float projectileSpeedMultiplier = 1f;
    [HideInInspector] public float durationMultiplier = 1f;

    private float lastUseTime;
    private Character owner;

    public float CurrentDamage
    {
        get
        {
            float baseDamage = skillData.GetDamageAtLevel(currentLevel);
            return baseDamage * damageMultiplier;
        }
    }

    public float CurrentCooldown
    {
        get
        {
            float baseCooldown = skillData.GetCooldownAtLevel(currentLevel);
            return baseCooldown * cooldownMultiplier;
        }
    }

    public float CurrentRange
    {
        get
        {
            float baseRange = skillData.GetRangeAtLevel(currentLevel);
            return baseRange * rangeMultiplier;
        }
    }

    public float CurrentProjectileSpeed
    {
        get
        {
            return 10f * projectileSpeedMultiplier;
        }
    }

    public float CurrentDuration
    {
        get
        {
            return 5f * durationMultiplier;
        }
    }

    public void Initialize(Character character, SkillData data)
    {
        owner = character;
        skillData = data;
        currentLevel = 1;

        if (owner == null)
        {
            Debug.LogError("Character owner가 null입니다!");
        }

        if (skillData == null)
        {
            Debug.LogError("SkillData가 null입니다!");
        }

        Debug.Log($"스킬 초기화: {skillData?.baseSkillType} Lv.{currentLevel}");
    }

    public void LevelUp()
    {
        if (currentLevel < skillData.maxLevel)
        {
            currentLevel++;
            Debug.Log($"{skillData.baseSkillType} 레벨업! Lv.{currentLevel} " +
                     $"(데미지: {CurrentDamage:F1}, 쿨타임: {CurrentCooldown:F1}초)");
        }
        else
        {
            Debug.LogWarning($"{skillData.baseSkillType}은 이미 최대 레벨입니다! (Lv.{skillData.maxLevel})");
        }
    }

    public bool CanUseSkill()
    {
        return Time.time - lastUseTime >= CurrentCooldown;
    }

    public void RecordSkillUse()
    {
        lastUseTime = Time.time;
    }

    public string GetSkillInfo()
    {
        var element = CloakManager.Instance?.GetCurrentElement() ?? ElementType.Energy;
        string skillName = skillData.GetDisplayName(element);

        return $"{skillName} Lv.{currentLevel}\n" +
               $"데미지: {CurrentDamage:F1}\n" +
               $"쿨타임: {CurrentCooldown:F1}초\n" +
               $"범위: {CurrentRange:F1}";
    }
}