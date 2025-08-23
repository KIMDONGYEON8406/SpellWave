using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SkillManager : MonoBehaviour
{
    [Header("스킬 설정")]
    public int maxSkills = 3;

    private Dictionary<string, SkillInstance> equippedSkills = new Dictionary<string, SkillInstance>();
    private Character owner;

    void Start()
    {
        owner = GetComponent<Character>();

        if (owner == null)
        {
            Debug.LogError("Character 컴포넌트를 찾을 수 없습니다!");
            owner = gameObject.AddComponent<Character>();
            Debug.Log("Character 컴포넌트 자동 생성");
        }
    }

    public bool AddSkillFromData(SkillData skillData)
    {
        if (skillData == null) return false;

        if (owner == null)
        {
            owner = GetComponent<Character>();
            if (owner == null)
            {
                owner = gameObject.AddComponent<Character>();
            }
        }

        if (equippedSkills.Count < maxSkills)
        {
            GameObject skillObj = new GameObject($"Skill_{skillData.baseSkillType}");
            skillObj.transform.SetParent(transform, false);
            skillObj.transform.localPosition = Vector3.zero;

            SkillInstance skill = skillObj.AddComponent<SkillInstance>();
            skill.Initialize(owner, skillData);
            equippedSkills[skillData.baseSkillType] = skill;

            Debug.Log($"새 스킬 추가: {skillData.baseSkillType}");

            // 오라면 즉시 생성!
            if (skillData.baseSkillType == "Aura")
            {
                CreateAuraImmediately(skill);
            }

            return true;
        }

        return false;
    }

    public SkillInstance GetSkill(string skillName)
    {
        return equippedSkills.ContainsKey(skillName) ? equippedSkills[skillName] : null;
    }

    public List<SkillInstance> GetAllSkills()
    {
        return equippedSkills.Values.ToList();
    }

    public int GetSkillCount()
    {
        return equippedSkills.Count;
    }

    public bool RemoveSkill(string skillName)
    {
        if (equippedSkills.ContainsKey(skillName))
        {
            SkillInstance skillToRemove = equippedSkills[skillName];
            equippedSkills.Remove(skillName);

            if (skillToRemove != null)
            {
                Destroy(skillToRemove.gameObject);
            }

            Debug.Log($"스킬 제거: {skillName}");
            return true;
        }

        return false;
    }

    public void ClearAllSkills()
    {
        foreach (var skill in equippedSkills.Values)
        {
            if (skill != null)
            {
                Destroy(skill.gameObject);
            }
        }

        equippedSkills.Clear();
        Debug.Log("모든 스킬 제거");
    }

    public void PrintSkillInfo()
    {
        Debug.Log($"=== 보유 스킬 ({equippedSkills.Count}/{maxSkills}) ===");
        foreach (var kvp in equippedSkills)
        {
            SkillInstance skill = kvp.Value;
            Debug.Log($"- {skill.skillData.baseSkillType} Lv.{skill.currentLevel} " +
                     $"(데미지: {skill.CurrentDamage:F1}, 쿨타임: {skill.CurrentCooldown:F1}초)");
        }
    }
    
    private void CreateAuraImmediately(SkillInstance auraSkill)
    {
        Debug.Log("[SkillManager] 오라 스킬 획득! 즉시 생성합니다.");

        // 이미 오라가 있는지 확인
        Transform existingAura = transform.Find("PermanentAura");
        if (existingAura != null)
        {
            Debug.Log("오라가 이미 존재합니다.");
            return;
        }

        var element = CloakManager.Instance?.GetCurrentElement() ?? ElementType.Energy;
        var passive = CloakManager.Instance?.GetCurrentPassive() ?? new PassiveEffect();

        if (auraSkill.skillData.skillBehavior != null)
        {
            SkillExecutionContext context = new SkillExecutionContext
            {
                Caster = gameObject,
                Target = null,
                Damage = auraSkill.CurrentDamage,
                Range = auraSkill.CurrentRange,
                Element = element,
                Passive = passive,
                SkillPrefab = auraSkill.skillData.skillPrefab,
                HitEffectPrefab = auraSkill.skillData.hitEffectPrefab
            };

            // 오라 실행
            auraSkill.skillData.skillBehavior.Execute(context);
            auraSkill.RecordSkillUse();

            Debug.Log($"[오라 생성 완료] 범위: {auraSkill.CurrentRange}");
        }
    }

    public float GetPlayerAttackDamage()
    {
        return owner != null ? owner.AttackPower : 0f;
    }

    public float GetPlayerAttackRange()
    {
        return owner != null ? owner.AttackRange : 0f;
    }

    public float GetPlayerAttackSpeed()
    {
        return owner != null ? owner.AttackSpeed : 0f;
    }
}