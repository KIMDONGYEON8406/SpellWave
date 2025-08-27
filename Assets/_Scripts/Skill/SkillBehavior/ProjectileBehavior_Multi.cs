using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileBehavior_Multi", menuName = "SpellWave/Behaviors/Projectile Multi")]
public class ProjectileBehavior_Multi : SkillBehavior
{
    [Header("다중 발사 설정")]
    public bool useProjectileCount = true;
    public int baseProjectileCount = 1;
    public float projectileSpeed = 10f;
    public float lifetime = 5f;

    public override bool CanExecute(SkillExecutionContext context)
    {
        return context.Target != null;
    }

    public override void Execute(SkillExecutionContext context)
    {
        if (context.SkillPrefab == null) return;

        var countModifier = context.Caster.GetComponent<ProjectileCountModifier>();
        string skillName = GetSkillNameFromContext(context);

        int projectileCount = baseProjectileCount;
        if (useProjectileCount && countModifier != null)
        {
            projectileCount = countModifier.GetTotalCount(skillName);
        }

        Vector3 baseDirection = (context.Target.position - context.Caster.transform.position).normalized;
        Vector3[] directions;

        if (countModifier != null && projectileCount > 1)
        {
            directions = countModifier.GetProjectileDirections(skillName, baseDirection);
        }
        else
        {
            directions = new Vector3[] { baseDirection };
        }

        // 발사체들 생성
        for (int i = 0; i < directions.Length; i++)
        {
            CreateProjectile(context, directions[i], i);
        }

        if (directions.Length > 1)
        {
            DebugManager.LogCombat($"{skillName}: {directions.Length}개 발사체 발사!");
        }
    }

    private void CreateProjectile(SkillExecutionContext context, Vector3 direction, int index)
    {
        // 발사 위치 (약간 위쪽에서 발사)
        Vector3 spawnPosition = context.Caster.transform.position + Vector3.up * 0.5f;

        // 발사체 간 간격 추가 (옵션)
        if (index > 0)
        {
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            float spacing = 0.3f;
            spawnPosition += perpendicular * spacing * (index % 2 == 0 ? 1 : -1) * ((index + 1) / 2);
        }

        GameObject projectile = Instantiate(
            context.SkillPrefab,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        var projComponent = projectile.GetComponent<ElementalProjectile>();
        if (projComponent != null)
        {
            // ElementalProjectile의 실제 Initialize 메서드 호출
            projComponent.Initialize(
                context.Damage,
                context.Element,
                context.Passive,
                direction  // Vector3 direction
            );

            // 속도 오버라이드 (필요한 경우)
            projComponent.speed = projectileSpeed;
        }

        // lifetime은 ElementalProjectile 내부에서 처리되므로 여기서는 설정 안 함
    }

    private string GetSkillNameFromContext(SkillExecutionContext context)
    {
        if (context.SkillPrefab != null)
        {
            string prefabName = context.SkillPrefab.name;
            // 예: "Projectile_Bolt" -> "Bolt"
            return prefabName.Replace("Projectile_", "").Replace("_Prefab", "");
        }
        return "Unknown";
    }
}