using UnityEngine;

[CreateAssetMenu(fileName = "SkillAcquireEffect", menuName = "Game/Card Effects/Skill Acquire")]
public class SkillAcquireEffect : CardEffect
{
    [Header("획득할 스킬")]
    public SkillData skillToAdd;

    [Header("획득 옵션")]
    public bool autoEquip = true;  // 자동 장착 여부
    public bool levelUpIfOwned = true;  // 이미 보유 시 레벨업

    public override void ApplyEffect(Player player, float value)
    {
        if (skillToAdd == null)
        {
            Debug.LogError("[CardEffect] 획득할 스킬이 설정되지 않음!");
            return;
        }

        // StaffManager를 통한 인벤토리 접근
        var inventory = StaffManager.Instance?.GetCurrentInventory();
        if (inventory == null)
        {
            Debug.LogError("[CardEffect] StaffInventory를 찾을 수 없음!");
            return;
        }

        // SkillManager 확인
        var skillManager = player.GetComponent<SkillManager>();
        if (skillManager == null)
        {
            Debug.LogError("[CardEffect] SkillManager를 찾을 수 없음!");
            return;
        }

        // 이미 장착된 스킬인지 확인
        var existingSkill = skillManager.GetSkill(skillToAdd.baseSkillType);

        if (existingSkill != null)
        {
            // 이미 보유한 스킬 처리
            if (levelUpIfOwned)
            {
                existingSkill.LevelUp();
                Debug.Log($"[CardEffect] {skillToAdd.baseSkillType} 레벨업! (Lv.{existingSkill.currentLevel})");
            }
            else
            {
                Debug.Log($"[CardEffect] {skillToAdd.baseSkillType}는 이미 보유 중");
            }
            return;
        }

        // 새 스킬 획득 처리
        if (!inventory.ownedSkills.Contains(skillToAdd))
        {
            inventory.ownedSkills.Add(skillToAdd);
            Debug.Log($"[CardEffect] 인벤토리에 {skillToAdd.baseSkillType} 추가");
        }

        // 자동 장착
        if (autoEquip)
        {
            if (inventory.equippedSkills.Count < 5)  // 최대 5개 슬롯
            {
                if (!inventory.equippedSkills.Contains(skillToAdd))
                {
                    inventory.equippedSkills.Add(skillToAdd);

                    // SkillManager 업데이트
                    StaffManager.Instance.UpdateEquippedSkills(inventory.equippedSkills);

                    Debug.Log($"[CardEffect] {skillToAdd.baseSkillType} 장착 완료! " +
                             $"(슬롯 {inventory.equippedSkills.Count}/5)");

                    // 오라 특별 처리
                    if (skillToAdd.baseSkillType == "Aura")
                    {
                        TriggerAuraCreation(skillManager);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[CardEffect] 스킬 슬롯이 가득 참! (5/5) - {skillToAdd.baseSkillType} 장착 실패");
            }
        }

        // value 파라미터를 초기 레벨 부스트로 활용 (선택사항)
        if (value > 0 && existingSkill == null)
        {
            // 방금 추가한 스킬 찾아서 레벨 설정
            var newSkill = skillManager.GetSkill(skillToAdd.baseSkillType);
            if (newSkill != null && value > 1)
            {
                for (int i = 1; i < value && i < skillToAdd.maxLevel; i++)
                {
                    newSkill.LevelUp();
                }
                Debug.Log($"[CardEffect] {skillToAdd.baseSkillType} 초기 레벨 {newSkill.currentLevel} 설정");
            }
        }
    }

    private void TriggerAuraCreation(SkillManager skillManager)
    {
        // SkillManager의 오라 즉시 생성 메서드 호출
        var auraSkill = skillManager.GetSkill("Aura");
        if (auraSkill != null)
        {
            Debug.Log("[CardEffect] 오라 스킬 획득 - 즉시 생성 트리거");

            // AutoSkillCaster가 있으면 강제 시전
            var autoCaster = skillManager.GetComponent<AutoSkillCaster>();
            if (autoCaster != null)
            {
                autoCaster.enabled = false;
                autoCaster.enabled = true;  // 리셋하여 즉시 체크
            }
        }
    }

    public override string GetPreviewText(float value)
    {
        if (skillToAdd == null)
            return "스킬 획득";

        var element = CloakManager.Instance?.GetCurrentElement() ?? ElementType.Energy;
        string skillName = skillToAdd.GetDisplayName(element);

        if (value > 1)
        {
            return $"{skillName} 획득 (Lv.{value})";
        }
        else
        {
            return $"{skillName} 획득";
        }
    }

    public override bool CanApply(Player player)
    {
        if (skillToAdd == null) return false;

        var skillManager = player.GetComponent<SkillManager>();
        if (skillManager == null) return false;

        // 이미 최대 레벨인지 확인
        var existing = skillManager.GetSkill(skillToAdd.baseSkillType);
        if (existing != null && existing.currentLevel >= skillToAdd.maxLevel)
        {
            return false;  // 이미 최대 레벨
        }

        // 슬롯이 가득 찼는지 확인 (새 스킬인 경우만)
        if (existing == null && skillManager.GetSkillCount() >= 5)
        {
            return autoEquip ? false : true;  // 자동장착이면 불가, 아니면 인벤토리에만 추가 가능
        }

        return true;
    }
}