using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiCastSystem : MonoBehaviour
{
    [System.Serializable]
    public class MultiCastConfig
    {
        public float multiCastChance = 0f;      // 다중시전 확률 (0~100)
        public float castDelay = 0.2f;          // 복제 간 딜레이
        public float damageMultiplier = 0.8f;   // 복제 데미지 배율
        public float positionSpread = 2f;       // 위치 분산 범위
    }

    private Dictionary<string, MultiCastConfig> skillConfigs = new Dictionary<string, MultiCastConfig>();
    private SkillManager skillManager;

    void Start()
    {
        skillManager = GetComponent<SkillManager>();
    }

    // 다중시전 확률 추가
    public void AddMultiCastChance(string skillName, float chance, StatType statType)
    {
        if (!skillConfigs.ContainsKey(skillName))
        {
            skillConfigs[skillName] = new MultiCastConfig();
        }

        skillConfigs[skillName].multiCastChance += chance;

        string typeText = statType == StatType.ProjectileMultiCast ? "발사체" : "영역";
        DebugManager.LogSkill($"{skillName} {typeText} 다중시전 확률 +{chance}% (총: {GetMultiCastChance(skillName)}%)");
    }

    // 전체 스킬에 다중시전 확률 추가
    public void AddMultiCastToType(SkillTag tag, float chance)
    {
        if (skillManager != null)
        {
            var skills = skillManager.GetAllSkills();
            foreach (var skill in skills)
            {
                if (skill.skillData.HasTag(tag))
                {
                    var statType = tag == SkillTag.Projectile ?
                        StatType.ProjectileMultiCast : StatType.AreaMultiCast;
                    AddMultiCastChance(skill.skillData.baseSkillType, chance, statType);
                }
            }
        }
    }

    // 다중시전 확률 가져오기
    public float GetMultiCastChance(string skillName)
    {
        if (skillConfigs.ContainsKey(skillName))
        {
            return skillConfigs[skillName].multiCastChance;
        }
        return 0f;
    }

    // Context에서 확률 체크 후 복제 처리
    public void ProcessMultiCast(SkillInstance skill, SkillExecutionContext context, List<GameObject> createdObjects)
    {
        if (context.IsMultiCastInstance) return; // 무한루프 방지
        if (context.MultiCastChance <= 0) return;
        if (createdObjects == null || createdObjects.Count == 0) return;

        StartCoroutine(MultiCastCheckCoroutine(skill, context, createdObjects));
    }

    private IEnumerator MultiCastCheckCoroutine(SkillInstance skill, SkillExecutionContext originalContext, List<GameObject> objects)
    {
        var config = skillConfigs.ContainsKey(skill.skillData.baseSkillType) ?
                     skillConfigs[skill.skillData.baseSkillType] :
                     new MultiCastConfig();

        yield return new WaitForSeconds(config.castDelay);

        int successCount = 0;

        // 각 오브젝트마다 개별 확률 체크
        foreach (var obj in objects)
        {
            if (obj == null) continue;

            float roll = Random.Range(0f, 100f);
            if (roll <= originalContext.MultiCastChance)
            {
                successCount++;

                // 복제 Context 생성
                var cloneContext = CloneContext(originalContext);
                cloneContext.IsMultiCastInstance = true;  // 무한루프 방지
                cloneContext.Damage *= config.damageMultiplier;
                cloneContext.BaseProjectileCount = 1;  // 복제는 1개씩만

                // 위치 오프셋 적용
                if (skill.skillData.HasTag(SkillTag.Area))
                {
                    Vector2 randomOffset = Random.insideUnitCircle * config.positionSpread;
                    cloneContext.PositionOffset = new Vector3(randomOffset.x, 0, randomOffset.y);
                }

                // 복제 실행
                skill.skillData.skillBehavior.Execute(cloneContext);

                DebugManager.LogCombat($"[MultiCast] {skill.skillData.baseSkillType} 복제 성공! ({roll:F1}% <= {originalContext.MultiCastChance}%)");
            }
        }

        if (successCount > 0)
        {
            DebugManager.LogImportant($"{skill.skillData.baseSkillType} 다중시전: {successCount}/{objects.Count} 성공");
        }
    }

    private SkillExecutionContext CloneContext(SkillExecutionContext original)
    {
        return new SkillExecutionContext
        {
            Caster = original.Caster,
            Target = original.Target,
            Damage = original.Damage,
            Range = original.Range,
            Element = original.Element,
            Passive = original.Passive,
            SkillPrefab = original.SkillPrefab,
            HitEffectPrefab = original.HitEffectPrefab,
            BaseProjectileCount = original.BaseProjectileCount,
            MultiCastChance = original.MultiCastChance,
            IsMultiCastInstance = original.IsMultiCastInstance,
            MultiCastIndex = original.MultiCastIndex,
            TotalMultiCasts = original.TotalMultiCasts,
            PositionOffset = original.PositionOffset
        };
    }

    // 디버그용
    public void PrintStatus()
    {
        DebugManager.LogSeparator("다중시전 확률 상태");
        foreach (var kvp in skillConfigs)
        {
            DebugManager.LogSkill($"{kvp.Key}: {kvp.Value.multiCastChance}% 확률");
        }
    }
}