using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SkillManager : MonoBehaviour
{
    [Header("��ų ����")]
    public int maxSkills = 3;

    private Dictionary<string, SkillInstance> equippedSkills = new Dictionary<string, SkillInstance>();
    private Character owner;

    void Start()
    {
        owner = GetComponent<Character>();

        if (owner == null)
        {
            Debug.LogError("Character ������Ʈ�� ã�� �� �����ϴ�!");
        }
    }

    public bool AddSkillFromData(SkillData skillData)
    {
        if (skillData == null)
        {
            Debug.LogError("SkillData�� null�Դϴ�!");
            return false;
        }

        // �̹� ���� ��ų�̸� ������
        if (equippedSkills.ContainsKey(skillData.skillName))
        {
            equippedSkills[skillData.skillName].LevelUp();
            Debug.Log($"��ų ������: {skillData.skillName}");
            return true;
        }

        // ���ο� ��ų �߰�
        if (equippedSkills.Count < maxSkills)
        {
            // �������� ������ ������ ���, ������ �� GameObject�� SkillInstance �߰�
            GameObject skillObj;

            if (skillData.skillPrefab != null)
            {
                skillObj = Instantiate(skillData.skillPrefab, transform);
            }
            else
            {
                // �������� ���� ��� �� GameObject ����
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

            Debug.Log($"�� ��ų �߰�: {skillData.skillName} (�� {equippedSkills.Count}��)");
            return true;
        }
        else
        {
            Debug.LogWarning($"��ų ������ ������! �ִ� {maxSkills}��");
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

    // Ư�� ��ų ���� (�ʿ��� ���)
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

            Debug.Log($"��ų ����: {skillName}");
            return true;
        }

        return false;
    }

    // ��� ��ų ���� (���� ����� ��)
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
        Debug.Log("��� ��ų ����");
    }

    // ��ų ���� ��� (����׿�)
    public void PrintSkillInfo()
    {
        Debug.Log($"=== ���� ��ų ({equippedSkills.Count}/{maxSkills}) ===");
        foreach (var kvp in equippedSkills)
        {
            SkillInstance skill = kvp.Value;
            Debug.Log($"- {skill.skillData.skillName} Lv.{skill.currentLevel} " +
                     $"(������: {skill.CurrentDamage:F1}, ��Ÿ��: {skill.CurrentCooldown:F1}��)");
        }
    }

    // PlayerStats���� ���� ���� ���� ��������
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