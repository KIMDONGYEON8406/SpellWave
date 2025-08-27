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
        // Bolt 기본값 설정
        if (!skillModifiers.ContainsKey("Bolt"))
        {
            skillModifiers["Bolt"] = new ProjectileModifiers { baseCount = 1, additionalCount = 0 };
        }
    }

    public void AddProjectileCount(string skillName, int count)
    {
        if (!skillModifiers.ContainsKey(skillName))
        {
            int defaultBase = 1;

            // 스킬별 기본값 설정
            if (skillName == "Missile") defaultBase = 5;
            else if (skillName == "Arrow") defaultBase = 3;
            else if (skillName == "Bolt") defaultBase = 1;

            skillModifiers[skillName] = new ProjectileModifiers
            {
                baseCount = defaultBase,
                additionalCount = 0,
                spreadAngle = 15f,
                useCircularPattern = false
            };

            DebugManager.LogSkill($"[{skillName}] 초기 설정 - base: {defaultBase}");
        }

        skillModifiers[skillName].additionalCount += count;

        int total = GetTotalCount(skillName);
        DebugManager.LogImportant($"[{skillName}] 개수 증가: {skillModifiers[skillName].baseCount}(base) + {skillModifiers[skillName].additionalCount}(add) = {total}개");
    }
    public bool HasSkill(string skillName)
    {
        return skillModifiers.ContainsKey(skillName);
    }

    // 전체 발사체 개수 증가
    public void AddProjectileCountToAll(int count)
    {
        var skillManager = GetComponent<SkillManager>();
        if (skillManager != null)
        {
            var skills = skillManager.GetAllSkills();
            foreach (var skill in skills)
            {
                // ⭐ 오라 제외, 나머지 모든 스킬에 적용
                if (skill.skillData.baseSkillType != "Aura")
                {
                    AddProjectileCount(skill.skillData.baseSkillType, count);
                }
            }
        }

        // 미래 스킬을 위한 기본값 (영역 스킬 포함!)
        string[] defaultSkills = {
        "Bolt", "Arrow", "Missile",      // 발사체
        "Explosion", "Nova"  // 영역
    };

        foreach (string skillName in defaultSkills)
        {
            if (!skillModifiers.ContainsKey(skillName))
            {
                AddProjectileCount(skillName, count);
            }
        }

        Debug.Log($"모든 스킬 개수 +{count} (오라 제외)");
    }

    // 총 발사체 개수 가져오기 - 로그 추가
    public int GetTotalCount(string skillName)
    {
        if (skillModifiers.ContainsKey(skillName))
        {
            var mod = skillModifiers[skillName];
            int total = mod.baseCount + mod.additionalCount;
            DebugManager.LogSkill($"[GetTotalCount] {skillName}: base={mod.baseCount}, additional={mod.additionalCount}, total={total}");
            return total;
        }

        DebugManager.LogSkill($"[GetTotalCount] {skillName}: 기본값 1 반환");
        return 1;
    }

    // 발사 각도 계산
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
        foreach (var kvp in skillModifiers)
        {
            int total = kvp.Value.baseCount + kvp.Value.additionalCount;
            DebugManager.LogSkill($"{kvp.Key}: {total}개");
        }
    }
}