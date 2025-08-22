using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Behavior", menuName = "SpellWave/Skills/Behaviors/Projectile")]
public class ProjectileBehavior : SkillBehavior
{
    [Header("발사체 설정")]
    public float projectileSpeed = 10f;
    public bool isHoming = false;
    public int pierceCount = 0;

    public override void Execute(SkillExecutionContext context)
    {
        if (context.Target == null) return;

        Vector3 direction = (context.Target.position - context.Caster.transform.position).normalized;

        if (context.SkillPrefab != null)
        {
            GameObject projectile = Object.Instantiate(
                context.SkillPrefab,
                context.Caster.transform.position + Vector3.up,
                Quaternion.LookRotation(direction)
            );

            var projScript = projectile.GetComponent<ElementalProjectile>();
            if (projScript == null)
            {
                projScript = projectile.AddComponent<ElementalProjectile>();
            }

            projScript.Initialize(context.Damage, context.Element, context.Passive, direction);
            projScript.speed = projectileSpeed;

            if (isHoming)
            {
                var homing = projectile.AddComponent<HomingComponent>();
                homing.target = context.Target;
            }

            if (pierceCount > 0)
            {
                var pierce = projectile.AddComponent<PierceComponent>();
                pierce.maxPierceCount = pierceCount;
            }
        }
    }
}