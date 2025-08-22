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
        // 초기 스킬 추가
        if (staff.defaultSkills != null)
        {
            foreach (var skill in staff.defaultSkills)
            {
                if (skill != null && ownedSkills.Count < 20)
                {
                    ownedSkills.Add(skill);
                }
            }

            // 처음 5개는 자동 장착
            for (int i = 0; i < Mathf.Min(5, ownedSkills.Count); i++)
            {
                equippedSkills.Add(ownedSkills[i]);
            }
        }
    }

    // 스킬 추가
    public bool AddSkill(SkillData newSkill)
    {
        if (ownedSkills.Count >= 20)
        {
            Debug.LogWarning("스킬 인벤토리가 가득 찼습니다! (20/20)");
            return false;
        }

        if (ownedSkills.Contains(newSkill))
        {
            Debug.LogWarning("이미 보유한 스킬입니다!");
            return false;
        }

        ownedSkills.Add(newSkill);
        Debug.Log($"{staffData.staffName}에 {newSkill.baseSkillType} 추가!");
        return true;
    }

    // 스킬 장착 (5개 슬롯)
    public bool EquipSkill(SkillData skill, int slotIndex)
    {
        if (!ownedSkills.Contains(skill))
        {
            Debug.LogError("보유하지 않은 스킬입니다!");
            return false;
        }

        if (slotIndex < 0 || slotIndex >= 5)
        {
            Debug.LogError("잘못된 슬롯 번호입니다! (0~4)");
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
}