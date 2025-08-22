using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Homing", menuName = "SpellWave/Skills/Behaviors/Projectile/Homing")]
public class ProjectileBehavior_Homing : ProjectileBehavior
{
    [Header("유도 발사체 설정")]
    public float defaultSpeed = 8f;
    public float homingRotationSpeed = 5f;
    public float homingMaxAngle = 45f;

    public override void Execute(SkillExecutionContext context)
    {
        projectileSpeed = defaultSpeed;
        isHoming = true;
        pierceCount = 0;

        if (context.Target == null) return;

        Vector3 direction = (context.Target.position - context.Caster.transform.position).normalized;
        GameObject projectile = null;

        if (context.SkillPrefab != null)
        {
            projectile = Object.Instantiate(
                context.SkillPrefab,
                context.Caster.transform.position + Vector3.up,
                Quaternion.LookRotation(direction)
            );
        }
        else
        {
            projectile = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            projectile.transform.position = context.Caster.transform.position + Vector3.up;
            projectile.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);

            var collider = projectile.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;

            var rb = projectile.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }

        var projScript = projectile.GetComponent<ElementalProjectile>();
        if (projScript == null)
            projScript = projectile.AddComponent<ElementalProjectile>();

        projScript.speed = projectileSpeed;
        projScript.Initialize(context.Damage, context.Element, context.Passive, direction);

        var homing = projectile.GetComponent<HomingComponent>();
        if (homing == null)
            homing = projectile.AddComponent<HomingComponent>();

        homing.target = context.Target;
        homing.rotationSpeed = homingRotationSpeed;
        homing.maxHomingAngle = homingMaxAngle;
    }

    public override bool RequiresTarget()
    {
        return true;
    }
}