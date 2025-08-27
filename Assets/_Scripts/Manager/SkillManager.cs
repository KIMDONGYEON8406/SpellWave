using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SkillManager : MonoBehaviour
{
    [Header("스킬 설정")]
    public int maxSkills = 5;  // 3에서 5로 변경

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = false;

    private Dictionary<string, SkillInstance> equippedSkills = new Dictionary<string, SkillInstance>();
    private Player owner;

    void Start()
    {
        owner = GetComponent<Player>();

        if (owner == null)
        {
            DebugManager.LogError(LogCategory.Skill, "Player 컴포넌트를 찾을 수 없습니다!");
            owner = gameObject.AddComponent<Player>();
            DebugManager.Log(LogCategory.Skill, "Player 컴포넌트 자동 생성", LogLevel.Warning);
        }
    }

    public bool AddSkillFromData(SkillData skillData)
    {
        if (skillData == null) return false;

        if (owner == null)
        {
            owner = GetComponent<Player>();
            if (owner == null)
            {
                owner = gameObject.AddComponent<Player>();
            }
        }

        // 이미 있는 스킬인지 체크
        if (equippedSkills.ContainsKey(skillData.baseSkillType))
        {
            DebugManager.LogSkill($"{skillData.baseSkillType}는 이미 장착됨");
            return false;
        }

        if (equippedSkills.Count < maxSkills)
        {
            GameObject skillObj = new GameObject($"Skill_{skillData.baseSkillType}");
            skillObj.transform.SetParent(transform, false);
            skillObj.transform.localPosition = Vector3.zero;

            SkillInstance skill = skillObj.AddComponent<SkillInstance>();
            skill.Initialize(owner, skillData);
            equippedSkills[skillData.baseSkillType] = skill;

            // 글로벌 보너스 즉시 적용
            var statModifier = SkillStatModifier.Instance;
            if (statModifier != null)
            {
                statModifier.OnSkillAdded(skill);
            }

            DebugManager.LogImportant($"새 스킬 추가: {skillData.baseSkillType}");

            if (skillData.baseSkillType == "Aura")
            {
                CreateAuraImmediately(skill);
            }

            return true;
        }

        DebugManager.Log(LogCategory.Skill, $"스킬 슬롯 가득 참 ({equippedSkills.Count}/{maxSkills})", LogLevel.Warning);
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

            DebugManager.LogSkill($"스킬 제거: {skillName}");
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

        if (showDebugLogs)
        {
            DebugManager.LogSkill("모든 스킬 제거");
        }
    }

    public void PrintSkillInfo()
    {
        DebugManager.LogSeparator($"보유 스킬 ({equippedSkills.Count}/{maxSkills})");
        foreach (var kvp in equippedSkills)
        {
            SkillInstance skill = kvp.Value;
            DebugManager.LogSkill($"{skill.skillData.baseSkillType} Lv.{skill.currentLevel}");

            if (showDebugLogs)
            {
                DebugManager.LogSkill($"  데미지: {skill.CurrentDamage:F1}");
                DebugManager.LogSkill($"  쿨타임: {skill.CurrentCooldown:F1}초");
                DebugManager.LogSkill($"  범위: {skill.CurrentRange:F1}m");
            }
        }
    }

    private void CreateAuraImmediately(SkillInstance auraSkill)
    {
        DebugManager.LogImportant("오라 스킬 획득! 즉시 생성합니다.");

        // 이미 오라가 있는지 확인
        Transform existingAura = transform.Find("PermanentAura");
        if (existingAura != null)
        {
            DebugManager.Log(LogCategory.Skill, "오라가 이미 존재합니다.", LogLevel.Warning);
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

            DebugManager.LogSkill($"오라 생성 완료! 범위: {auraSkill.CurrentRange}m");
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
        return 1f;  // 고정값 (사용 안 함)
    }
}