using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Basic", menuName = "SpellWave/Skills/Behaviors/Projectile/Basic")]
public class ProjectileBehavior_Basic : ProjectileBehavior
{
    [Header("기본 발사체 설정")]
    public float defaultSpeed = 10f;
    public float defaultLifetime = 3f;

    public override void Execute(SkillExecutionContext context)
    {
        if (context.SkillPrefab == null || context.Target == null) return;

        // 다중 발사 체크
        var countModifier = context.Caster.GetComponent<ProjectileCountModifier>();
        if (countModifier != null)
        {
            string skillName = context.SkillPrefab.name.Replace("Projectile_", "");
            int count = countModifier.GetTotalCount(skillName);

            if (count > 1)
            {
                // 다중 발사 처리
                Vector3 baseDir = (context.Target.position - context.Caster.transform.position).normalized;
                Vector3[] directions = countModifier.GetProjectileDirections(skillName, baseDir);

                foreach (var dir in directions)
                {
                    CreateSingleProjectile(context, dir);
                }
                return;
            }
        }

        // 단일 발사
        Vector3 direction = (context.Target.position - context.Caster.transform.position).normalized;
        CreateSingleProjectile(context, direction);
    }
    private void CreateSingleProjectile(SkillExecutionContext context, Vector3 direction)
    {
        GameObject projectile = Instantiate(
            context.SkillPrefab,
            context.Caster.transform.position + Vector3.up * 0.5f,
            Quaternion.LookRotation(direction)
        );

        var projComponent = projectile.GetComponent<ElementalProjectile>();
        if (projComponent != null)
        {
            projComponent.Initialize(
                context.Damage,
                context.Element,
                context.Passive,
                direction
            );
            projComponent.speed = projectileSpeed;
        }
    }
}