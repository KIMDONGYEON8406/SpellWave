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
        }
    }

    public bool AddSkillFromData(SkillData skillData)
    {
        if (skillData == null)
        {
            Debug.LogError("SkillData가 null입니다!");
            return false;
        }

        // 이미 가진 스킬이면 레벨업
        if (equippedSkills.ContainsKey(skillData.skillName))
        {
            equippedSkills[skillData.skillName].LevelUp();
            Debug.Log($"스킬 레벨업: {skillData.skillName}");
            return true;
        }

        // 새로운 스킬 추가
        if (equippedSkills.Count < maxSkills)
        {
            // 프리팹이 있으면 프리팹 사용, 없으면 빈 GameObject에 SkillInstance 추가
            GameObject skillObj;

            if (skillData.skillPrefab != null)
            {
                skillObj = Instantiate(skillData.skillPrefab, transform);
            }
            else
            {
                // 프리팹이 없는 경우 빈 GameObject 생성
                skillObj = new GameObject($"Skill_{skillData.skillName}");
                skillObj.transform.SetParent(transform);
            }

            SkillInstance skill = skillObj.GetComponent<SkillInstance>();
            if (skill == null)
            {
                skill = skillObj.AddComponent<SkillInstance>();
            }

            skill.Initialize(owner, skillData);
            equippedSkills[skillData.skillName] = skill;

            Debug.Log($"새 스킬 추가: {skillData.skillName} (총 {equippedSkills.Count}개)");
            return true;
        }
        else
        {
            Debug.LogWarning($"스킬 슬롯이 가득참! 최대 {maxSkills}개");
            return false;
        }
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

    // 특정 스킬 제거 (필요한 경우)
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

    // 모든 스킬 제거 (게임 재시작 시)
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

    // 스킬 정보 출력 (디버그용)
    public void PrintSkillInfo()
    {
        Debug.Log($"=== 보유 스킬 ({equippedSkills.Count}/{maxSkills}) ===");
        foreach (var kvp in equippedSkills)
        {
            SkillInstance skill = kvp.Value;
            Debug.Log($"- {skill.skillData.skillName} Lv.{skill.currentLevel} " +
                     $"(데미지: {skill.CurrentDamage:F1}, 쿨타임: {skill.CurrentCooldown:F1}초)");
        }
    }

    // PlayerStats에서 공격 관련 스탯 가져오기
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