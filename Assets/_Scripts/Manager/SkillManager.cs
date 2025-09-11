
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SkillManager : MonoBehaviour
{
    [Header("스킬 설정")]
    public int maxSkills = 5;

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = false;

    private Dictionary<string, SkillInstance> equippedSkills = new Dictionary<string, SkillInstance>();
    private Player owner;

    void Start()
    {
        owner = GetComponent<Player>();

        if (owner == null)
        {
            DebugManager.LogError(LogCategory.Skill, "[skillInitialization] Player 컴포넌트를 찾을 수 없습니다!");
            owner = gameObject.AddComponent<Player>();
            DebugManager.LogWarning(LogCategory.Skill, "[skillInitialization] Player 컴포넌트 자동 생성");
        }

        DebugManager.LogSkillInitialization($"SkillManager 초기화 완료 (최대 슬롯: {maxSkills})");
    }

    public bool AddSkillFromData(SkillData skillData)
    {
        if (skillData == null)
        {
            DebugManager.LogError(LogCategory.Skill, "[skillManagement] SkillData가 null입니다!");
            return false;
        }

        if (owner == null)
        {
            owner = GetComponent<Player>();
            if (owner == null)
            {
                owner = gameObject.AddComponent<Player>();
                DebugManager.LogSkillInitialization("Player 컴포넌트 자동 추가");
            }
        }

        // 이미 있는 스킬인지 체크
        if (equippedSkills.ContainsKey(skillData.baseSkillType))
        {
            DebugManager.LogSkillManagement($"{skillData.baseSkillType}는 이미 장착됨");
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
                DebugManager.LogSkillStats($"{skillData.baseSkillType}에 글로벌 보너스 적용");
            }

            DebugManager.LogSkillManagement($"새 스킬 추가: {skillData.baseSkillType} (현재 {equippedSkills.Count}/{maxSkills})");

            if (skillData.baseSkillType == "Aura")
            {
                CreateAuraImmediately(skill);
            }

            return true;
        }

        DebugManager.LogWarning(LogCategory.Skill, $"[skillManagement] 스킬 슬롯 가득 참 ({equippedSkills.Count}/{maxSkills})");
        return false;
    }

    public SkillInstance GetSkill(string skillName)
    {
        bool found = equippedSkills.ContainsKey(skillName);
        if (showDebugLogs)
        {
            DebugManager.LogSkillManagement($"스킬 조회: {skillName} ({(found ? "발견" : "없음")})");
        }

        return found ? equippedSkills[skillName] : null;
    }

    public List<SkillInstance> GetAllSkills()
    {
        var skillList = equippedSkills.Values.ToList();
        if (showDebugLogs)
        {
            DebugManager.LogSkillManagement($"모든 스킬 조회: {skillList.Count}개");
        }

        return skillList;
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

            DebugManager.LogSkillManagement($"스킬 제거: {skillName} (남은 스킬: {equippedSkills.Count}/{maxSkills})");
            return true;
        }

        DebugManager.LogWarning(LogCategory.Skill, $"[skillManagement] 제거할 스킬을 찾을 수 없음: {skillName}");
        return false;
    }

    public void ClearAllSkills()
    {
        int removedCount = equippedSkills.Count;

        foreach (var skill in equippedSkills.Values)
        {
            if (skill != null)
            {
                Destroy(skill.gameObject);
            }
        }

        equippedSkills.Clear();
        DebugManager.LogSkillManagement($"모든 스킬 제거 ({removedCount}개 스킬)");
    }

    public void PrintSkillInfo()
    {
        DebugManager.LogSeparator($"보유 스킬 ({equippedSkills.Count}/{maxSkills})");

        foreach (var kvp in equippedSkills)
        {
            SkillInstance skill = kvp.Value;

            DebugManager.LogSkillStats($"{skill.skillData.baseSkillType} Lv.{skill.currentLevel}");

            if (showDebugLogs)
            {
                DebugManager.LogSkillStats($"  데미지: {skill.CurrentDamage:F1}");
                DebugManager.LogSkillStats($"  쿨타임: {skill.CurrentCooldown:F1}초");
                DebugManager.LogSkillStats($"  범위: {skill.CurrentRange:F1}m");
            }
        }
    }

    private void CreateAuraImmediately(SkillInstance auraSkill)
    {
        DebugManager.LogSkillExecution("오라 스킬 획득! 즉시 생성 시작");

        // 이미 오라가 있는지 확인
        Transform existingAura = transform.Find("PermanentAura");
        if (existingAura != null)
        {
            DebugManager.LogWarning(LogCategory.Skill, "[skillExecution] 오라가 이미 존재합니다.");
            return;
        }

        var element = StaffManager.Instance?.GetCurrentElement() ?? ElementType.Energy;
        var passive = StaffManager.Instance?.GetCurrentPassive() ?? new PassiveEffect();

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

            DebugManager.LogSkillExecution($"오라 생성 완료! 범위: {auraSkill.CurrentRange:F1}m, 데미지: {auraSkill.CurrentDamage:F1}");
        }
        else
        {
            DebugManager.LogError(LogCategory.Skill, "[skillExecution] 오라 스킬에 Behavior가 없습니다!");
        }
    }

    // 스킬 레벨업 처리
    public bool LevelUpSkill(string skillName)
    {
        if (equippedSkills.ContainsKey(skillName))
        {
            var skill = equippedSkills[skillName];
            if (skill.currentLevel < skill.skillData.maxLevel)
            {
                int oldLevel = skill.currentLevel;
                skill.LevelUp();

                DebugManager.LogSkillLevelUp($"{skillName} 레벨업: Lv.{oldLevel} → Lv.{skill.currentLevel}");
                DebugManager.LogSkillStats($"  새 스탯 - 데미지:{skill.CurrentDamage:F1} 쿨타임:{skill.CurrentCooldown:F1} 범위:{skill.CurrentRange:F1}");

                return true;
            }
            else
            {
                DebugManager.LogWarning(LogCategory.Skill, $"[skillLevelUp] {skillName}은 이미 최대 레벨입니다 (Lv.{skill.skillData.maxLevel})");
                return false;
            }
        }

        DebugManager.LogError(LogCategory.Skill, $"[skillLevelUp] 레벨업할 스킬을 찾을 수 없음: {skillName}");
        return false;
    }

    // 플레이어 스탯 접근 메서드들
    public float GetPlayerAttackDamage()
    {
        float damage = owner != null ? owner.AttackPower : 0f;
        if (showDebugLogs)
        {
            DebugManager.LogSkillStats($"플레이어 공격력 조회: {damage:F1}");
        }
        return damage;
    }

    public float GetPlayerAttackRange()
    {
        float range = owner != null ? owner.AttackRange : 0f;
        if (showDebugLogs)
        {
            DebugManager.LogSkillStats($"플레이어 공격 범위 조회: {range:F1}");
        }
        return range;
    }

    public float GetPlayerAttackSpeed()
    {
        return 1f;  // 고정값
    }

    // 디버그 전용 메서드들
    [ContextMenu("디버그/스킬 정보 출력")]
    void DebugPrintSkillInfo()
    {
        PrintSkillInfo();
    }

    [ContextMenu("디버그/모든 스킬 레벨업")]
    void DebugLevelUpAllSkills()
    {
        DebugManager.LogSkillLevelUp("[skillLevelUp] 디버그 명령: 모든 스킬 레벨업");

        int leveledUpCount = 0;
        foreach (var skillName in equippedSkills.Keys.ToList())
        {
            if (LevelUpSkill(skillName))
            {
                leveledUpCount++;
            }
        }

        DebugManager.LogSkillLevelUp($"[skillLevelUp] {leveledUpCount}개 스킬 레벨업 완료");
    }

    [ContextMenu("디버그/스킬 슬롯 상태")]
    void DebugPrintSlotStatus()
    {
        DebugManager.LogSeparator("스킬 슬롯 상태");
        DebugManager.LogSkillManagement($"사용 중: {equippedSkills.Count}/{maxSkills}");
        DebugManager.LogSkillManagement($"남은 슬롯: {maxSkills - equippedSkills.Count}");

        if (equippedSkills.Count > 0)
        {
            DebugManager.LogSkillManagement("장착된 스킬 목록:");
            foreach (var skillName in equippedSkills.Keys)
            {
                DebugManager.LogSkillManagement($"  - {skillName}");
            }
        }
    }

    [ContextMenu("디버그/스킬 상세 스탯")]
    void DebugPrintDetailedStats()
    {
        DebugManager.LogSeparator("스킬 상세 스탯");

        foreach (var kvp in equippedSkills)
        {
            var skill = kvp.Value;
            DebugManager.LogSkillStats($"=== {skill.skillData.baseSkillType} ===");
            DebugManager.LogSkillStats($"레벨: {skill.currentLevel}/{skill.skillData.maxLevel}");
            DebugManager.LogSkillStats($"기본 데미지: {skill.skillData.GetDamageAtLevel(skill.currentLevel):F1}");
            DebugManager.LogSkillStats($"최종 데미지: {skill.CurrentDamage:F1} (배율: {skill.damageMultiplier:F2}x)");
            DebugManager.LogSkillStats($"기본 쿨타임: {skill.skillData.GetCooldownAtLevel(skill.currentLevel):F1}초");
            DebugManager.LogSkillStats($"최종 쿨타임: {skill.CurrentCooldown:F1}초 (배율: {skill.cooldownMultiplier:F2}x)");
            DebugManager.LogSkillStats($"기본 범위: {skill.skillData.GetRangeAtLevel(skill.currentLevel):F1}");
            DebugManager.LogSkillStats($"최종 범위: {skill.CurrentRange:F1} (배율: {skill.rangeMultiplier:F2}x)");
        }
    }

    // 스킬 존재 여부 체크
    public bool HasSkill(string skillName)
    {
        return equippedSkills.ContainsKey(skillName);
    }

    // 특정 태그를 가진 스킬들 가져오기
    public List<SkillInstance> GetSkillsByTag(SkillTag tag)
    {
        var matchingSkills = equippedSkills.Values.Where(skill => skill.skillData.HasTag(tag)).ToList();

        if (showDebugLogs)
        {
            DebugManager.LogSkillManagement($"{tag} 태그 스킬 조회: {matchingSkills.Count}개");
        }

        return matchingSkills;
    }

    // 스킬 타입별 개수 조회
    public int GetSkillCountByTag(SkillTag tag)
    {
        int count = equippedSkills.Values.Count(skill => skill.skillData.HasTag(tag));

        if (showDebugLogs)
        {
            DebugManager.LogSkillManagement($"{tag} 태그 스킬 개수: {count}");
        }

        return count;
    }
}