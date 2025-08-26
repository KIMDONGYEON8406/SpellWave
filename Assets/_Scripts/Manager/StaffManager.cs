using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StaffManager : MonoBehaviour
{
    public static StaffManager Instance { get; private set; }

    [Header("현재 장착 지팡이")]
    public StaffData currentStaff;

    [Header("보유 지팡이 목록")]
    public List<StaffData> unlockedStaffs = new List<StaffData>();

    [Header("지팡이별 인벤토리")]
    private Dictionary<string, StaffInventory> staffInventories = new Dictionary<string, StaffInventory>();

    [Header("디버그")]
    [SerializeField] private bool verboseLogging = false;

    private SkillManager skillManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        skillManager = FindObjectOfType<SkillManager>();

        if (currentStaff != null)
        {
            UnlockStaff(currentStaff);
            EquipStaff(currentStaff);
        }
    }

    // 지팡이 해금

    public void UnlockStaff(StaffData staff)
    {
        if (!unlockedStaffs.Contains(staff))
        {
            unlockedStaffs.Add(staff);

            if (!staffInventories.ContainsKey(staff.staffName))
            {
                staffInventories[staff.staffName] = new StaffInventory(staff);
            }

            DebugManager.LogImportant($"새 지팡이 해금: {staff.staffName}");
        }
    }

    // 지팡이 장착
    public void EquipStaff(StaffData newStaff)
    {
        if (newStaff == null || !unlockedStaffs.Contains(newStaff))
        {
            Debug.LogError("해금되지 않은 지팡이입니다!");
            return;
        }

        currentStaff = newStaff;

        // 스킬매니저 업데이트
        UpdateSkillManager();

        Debug.Log($"지팡이 장착: {newStaff.staffName}");
    }

    // 현재 지팡이의 장착된 스킬들을 스킬매니저에 적용
    private void UpdateSkillManager()
    {
        if (skillManager == null)
        {
            skillManager = FindObjectOfType<SkillManager>();
        }

        if (skillManager == null || currentStaff == null) return;

        var inventory = GetCurrentInventory();
        if (inventory != null)
        {

            foreach (var skill in inventory.equippedSkills)
            {
                if (skill != null)
                {
                    skillManager.AddSkillFromData(skill);
                }
            }

            if (verboseLogging)
            {
                DebugManager.LogSkill($"장착된 스킬: {inventory.equippedSkills.Count}개");
            }
        }
    }

    // 현재 지팡이 인벤토리 가져오기
    public StaffInventory GetCurrentInventory()
    {
        if (currentStaff == null) return null;

        if (staffInventories.ContainsKey(currentStaff.staffName))
        {
            return staffInventories[currentStaff.staffName];
        }

        return null;
    }

    // 현재 지팡이에 스킬 추가
    public bool AddSkillToCurrentStaff(SkillData skill)
    {
        var inventory = GetCurrentInventory();
        if (inventory != null)
        {
            return inventory.AddSkill(skill);
        }
        return false;
    }

    // 스킬 슬롯 변경
    public bool UpdateEquippedSkills(List<SkillData> newEquippedSkills)
    {
        var inventory = GetCurrentInventory();
        if (inventory != null)
        {
            inventory.equippedSkills = newEquippedSkills.Take(5).ToList();
            UpdateSkillManager();

            DebugManager.LogSkill($"스킬 슬롯 업데이트: {inventory.equippedSkills.Count}개");
            return true;
        }
        return false;
    }

}