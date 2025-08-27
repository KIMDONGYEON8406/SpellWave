using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StaffInventory
{
    public StaffData staffData;
    public List<SkillData> ownedSkills = new List<SkillData>(20); // 최대 20개
    public List<SkillData> equippedSkills = new List<SkillData>(5); // 장착 중인 5개

    public StaffInventory(StaffData staff)
    {
        staffData = staff;

        // 모든 스킬을 인벤토리에 추가
        if (staff.defaultSkills != null)
        {
            foreach (var skill in staff.defaultSkills)
            {
                if (skill != null && ownedSkills.Count < 20)
                {
                    ownedSkills.Add(skill);
                }
            }
        }

        // 첫 번째 스킬만 자동 장착 (볼만)
        if (ownedSkills.Count > 0)
        {
            equippedSkills.Add(ownedSkills[0]);
            DebugManager.LogSkill($"초기 스킬 장착: {ownedSkills[0].baseSkillType}");
        }
    }

    // 스킬 추가 (새로운 스킬 획득 시)
    public bool AddSkill(SkillData newSkill)
    {
        if (ownedSkills.Count >= 20)
        {
            DebugManager.LogWarning(LogCategory.Skill, "스킬 인벤토리가 가득 찼습니다! (20/20)");
            return false;
        }

        if (ownedSkills.Contains(newSkill))
        {
            DebugManager.LogWarning(LogCategory.Skill, "이미 보유한 스킬입니다!");
            return false;
        }

        ownedSkills.Add(newSkill);
        DebugManager.LogSkill($"{staffData.staffName}에 {newSkill.baseSkillType} 추가!");
        return true;
    }

    // 스킬 장착 (카드로 선택 시)
    public bool EquipNextSkill(SkillData skill)
    {
        // 이미 장착된 스킬인지 체크
        if (equippedSkills.Contains(skill))
        {
            DebugManager.LogWarning(LogCategory.Skill, $"{skill.baseSkillType}은 이미 장착되어 있습니다!");
            return false;
        }

        // 보유한 스킬인지 체크
        if (!ownedSkills.Contains(skill))
        {
            DebugManager.LogError(LogCategory.Skill, $"{skill.baseSkillType}은 보유하지 않은 스킬입니다!");

            return false;
        }

        // 최대 5개까지만 장착
        if (equippedSkills.Count >= 5)
        {
            DebugManager.LogWarning(LogCategory.Skill, "스킬 슬롯이 가득 찼습니다! (5/5)");
            return false;
        }

        equippedSkills.Add(skill);
        DebugManager.LogSkill($"스킬 장착: {skill.baseSkillType} (현재 {equippedSkills.Count}/5)");
        return true;
    }

    // 특정 슬롯에 스킬 장착
    public bool EquipSkill(SkillData skill, int slotIndex)
    {
        if (!ownedSkills.Contains(skill))
        {
            DebugManager.LogError(LogCategory.Skill, "보유하지 않은 스킬입니다!");
            return false;
        }

        if (slotIndex < 0 || slotIndex >= 5)
        {
            DebugManager.LogError(LogCategory.Skill, "잘못된 슬롯 번호입니다! (0~4)");
            return false;
        }

        // 슬롯 크기 맞추기
        while (equippedSkills.Count <= slotIndex)
        {
            equippedSkills.Add(null);
        }

        equippedSkills[slotIndex] = skill;
        return true;
    }

    // 스킬 장착 해제
    public void UnequipSkill(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedSkills.Count)
        {
            equippedSkills[slotIndex] = null;
        }
    }

    // 장착 가능한 스킬 목록 가져오기
    public List<SkillData> GetUnequippedSkills()
    {
        List<SkillData> unequipped = new List<SkillData>();

        foreach (var skill in ownedSkills)
        {
            if (!equippedSkills.Contains(skill))
            {
                unequipped.Add(skill);
            }
        }

        return unequipped;
    }
}