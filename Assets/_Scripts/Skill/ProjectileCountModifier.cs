using UnityEngine;
using System.Collections.Generic;

public class ProjectileCountModifier : MonoBehaviour
{
    [System.Serializable]
    public class ProjectileModifiers
    {
        public int baseCount = 1;
        public int additionalCount = 0;
        public float spreadAngle = 15f;
        public bool useCircularPattern = false;
    }

    private Dictionary<string, ProjectileModifiers> skillModifiers = new Dictionary<string, ProjectileModifiers>();

    void Awake()
    {
        // 모든 기본 스킬의 기본값을 미리 설정
        InitializeDefaultSkills();
    }

    void InitializeDefaultSkills()
    {
        // StaffManager에서 모든 스킬 SO 가져오기
        if (StaffManager.Instance != null && StaffManager.Instance.currentStaff != null)
        {
            var staff = StaffManager.Instance.currentStaff;

            // 기본 제공 스킬들
            foreach (var skill in staff.defaultSkills)
            {
                if (skill != null)
                {
                    int baseCount = GetSkillBaseCount(skill);
                    SetDefaultIfNotExists(skill.baseSkillType, baseCount);
                }
            }

            // 획득 가능한 스킬들
            foreach (var skill in staff.availableSkillPool)
            {
                if (skill != null)
                {
                    int baseCount = GetSkillBaseCount(skill);
                    SetDefaultIfNotExists(skill.baseSkillType, baseCount);
                }
            }

            DebugManager.LogSkill("SkillData SO에서 기본 개수 설정 완료");
        }
        else
        {
            // 폴백: StaffManager가 없을 때만 하드코딩 사용
            SetDefaultIfNotExists("Bolt", 1);
            SetDefaultIfNotExists("Arrow", 3);
            SetDefaultIfNotExists("Missile", 5);
            DebugManager.LogSkill("폴백: 하드코딩 기본값 사용");
        }
    }

    // 다중 주타입 지원으로 개수 계산 수정
    int GetSkillBaseCount(SkillData skillData)
    {
        // 발사체 타입이 있으면 발사체 개수 반환
        if (skillData.HasPrimaryType(PrimarySkillType.Projectile))
            return skillData.baseProjectileCount;

        // 영역 타입이 있으면 영역 개수 반환
        if (skillData.HasPrimaryType(PrimarySkillType.Area))
            return skillData.baseAreaCount;

        // 기본값
        return 1;
    }

    void SetDefaultIfNotExists(string skillName, int baseCount)
    {
        if (!skillModifiers.ContainsKey(skillName))
        {
            skillModifiers[skillName] = new ProjectileModifiers
            {
                baseCount = baseCount,
                additionalCount = 0,
                spreadAngle = GetDefaultSpreadAngle(skillName),
                useCircularPattern = false
            };
        }
    }

    float GetDefaultSpreadAngle(string skillName)
    {
        switch (skillName)
        {
            case "Bolt": return 20f;
            case "Arrow": return 15f;
            case "Missile": return 30f;
            case "Explosion": return 0f;  // 영역은 각도 없음
            default: return 15f;
        }
    }

    public void AddProjectileCount(string skillName, int count)
    {
        // 기본값이 없으면 자동으로 생성
        if (!skillModifiers.ContainsKey(skillName))
        {
            int defaultBase = GetDefaultBaseCount(skillName);

            skillModifiers[skillName] = new ProjectileModifiers
            {
                baseCount = defaultBase,
                additionalCount = 0,
                spreadAngle = GetDefaultSpreadAngle(skillName),
                useCircularPattern = false
            };

            DebugManager.LogSkill($"[{skillName}] 새 스킬 등록 - 기본: {defaultBase}개");
        }

        skillModifiers[skillName].additionalCount += count;

        int total = GetTotalCount(skillName);
        DebugManager.LogImportant($"[{skillName}] 개수 증가: {skillModifiers[skillName].baseCount}(base) + {skillModifiers[skillName].additionalCount}(add) = {total}개");
    }

    // 스킬별 기본 개수 설정
    int GetDefaultBaseCount(string skillName)
    {
        switch (skillName)
        {
            case "Missile": return 5;
            case "Arrow": return 3;
            case "Bolt": return 1;
            case "Pierce": return 1;
            case "Explosion": return 1;
            case "Nova": return 1;
            default: return 1;
        }
    }

    public bool HasSkill(string skillName)
    {
        return skillModifiers.ContainsKey(skillName);
    }

    // 다중 주타입 지원으로 수정
    public void AddProjectileCountToAll(int count)
    {
        var skillManager = GetComponent<SkillManager>();
        if (skillManager != null)
        {
            var skills = skillManager.GetAllSkills();
            foreach (var skill in skills)
            {
                // 발사체 주타입을 가진 스킬에만 적용 (오라 제외)
                if (skill.skillData.HasPrimaryType(PrimarySkillType.Projectile) &&
                    skill.skillData.baseSkillType != "Aura")
                {
                    AddProjectileCount(skill.skillData.baseSkillType, count);
                }
            }
        }

        // 미래 발사체 스킬을 위한 기본값 (오라 제외)
        string[] projectileSkills = { "Bolt", "Arrow", "Missile", "Pierce" };

        foreach (string skillName in projectileSkills)
        {
            AddProjectileCount(skillName, count);
        }

        DebugManager.LogImportant($"모든 발사체 스킬 개수 +{count} (미래 스킬 포함)");
    }

    // 다중 주타입 지원으로 수정
    public void AddAreaCountToAll(int count)
    {
        var skillManager = GetComponent<SkillManager>();
        if (skillManager != null)
        {
            var skills = skillManager.GetAllSkills();
            foreach (var skill in skills)
            {
                // 영역 주타입을 가진 스킬에만 적용
                if (skill.skillData.HasPrimaryType(PrimarySkillType.Area))
                {
                    AddProjectileCount(skill.skillData.baseSkillType, count);
                }
            }
        }

        // 미래 영역 스킬을 위한 기본값
        string[] areaSkills = { "Explosion", "Nova", "Aura" };

        foreach (string skillName in areaSkills)
        {
            AddProjectileCount(skillName, count);
        }

        DebugManager.LogImportant($"모든 영역 스킬 개수 +{count} (미래 스킬 포함)");
    }

    public int GetTotalCount(string skillName)
    {
        if (skillModifiers.ContainsKey(skillName))
        {
            var mod = skillModifiers[skillName];
            int total = mod.baseCount + mod.additionalCount;
            return total;
        }

        // 등록되지 않은 스킬도 기본값 반환
        int defaultCount = GetDefaultBaseCount(skillName);
        DebugManager.LogSkill($"[GetTotalCount] {skillName}: 기본값 {defaultCount} 반환");
        return defaultCount;
    }

    public Vector3[] GetProjectileDirections(string skillName, Vector3 baseDirection)
    {
        int count = GetTotalCount(skillName);
        Vector3[] directions = new Vector3[count];

        if (count == 1)
        {
            directions[0] = baseDirection;
            return directions;
        }

        float spreadAngle = 15f;
        bool circular = false;

        if (skillModifiers.ContainsKey(skillName))
        {
            spreadAngle = skillModifiers[skillName].spreadAngle;
            circular = skillModifiers[skillName].useCircularPattern;
        }

        if (circular)
        {
            // 원형 패턴
            float angleStep = 360f / count;
            for (int i = 0; i < count; i++)
            {
                float angle = angleStep * i;
                directions[i] = Quaternion.Euler(0, angle, 0) * baseDirection;
            }
        }
        else
        {
            // 부채꼴 패턴
            float totalSpread = spreadAngle * (count - 1);
            float startAngle = -totalSpread / 2f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (spreadAngle * i);
                directions[i] = Quaternion.Euler(0, angle, 0) * baseDirection;
            }
        }

        return directions;
    }

    public void PrintStatus()
    {
        DebugManager.LogSeparator("발사체 개수 상태");

        if (skillModifiers.Count == 0)
        {
            DebugManager.LogSkill("등록된 스킬 없음");
            return;
        }

        foreach (var kvp in skillModifiers)
        {
            var mod = kvp.Value;
            int total = mod.baseCount + mod.additionalCount;

            if (mod.additionalCount > 0)
            {
                DebugManager.LogSkill($"{kvp.Key}: {mod.baseCount}(기본) + {mod.additionalCount}(추가) = {total}개");
            }
            else
            {
                DebugManager.LogSkill($"{kvp.Key}: {total}개 (기본)");
            }
        }
    }

    // 다중 주타입 지원으로 수정
    public void AddCountByTag(PrimarySkillType primaryType, int count)
    {
        if (primaryType == PrimarySkillType.Projectile)
        {
            AddProjectileCountToAll(count);
        }
        else if (primaryType == PrimarySkillType.Area)
        {
            AddAreaCountToAll(count);
        }
    }
}