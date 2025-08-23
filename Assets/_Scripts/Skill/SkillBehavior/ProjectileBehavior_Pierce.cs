using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Pierce", menuName = "SpellWave/Skills/Behaviors/Projectile/Pierce")]
public class ProjectileBehavior_Pierce : ProjectileBehavior
{
    [Header("관통 발사체 설정")]
    public float defaultSpeed = 15f;
    public int defaultPierceCount = 3;
    public float damageReductionPerPierce = 0.8f;

    public override void Execute(SkillExecutionContext context)
    {
        projectileSpeed = defaultSpeed;
        isHoming = false;
        pierceCount = defaultPierceCount;

        if (context.Target == null) return;

        Vector3 direction = context.Caster.transform.forward;
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
            projectile = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            projectile.transform.position = context.Caster.transform.position + Vector3.up;
            projectile.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            projectile.transform.rotation = Quaternion.LookRotation(direction);

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

        var pierce = projectile.GetComponent<PierceComponent>();
        if (pierce == null)
            pierce = projectile.AddComponent<PierceComponent>();

        pierce.maxPierceCount = pierceCount;
        pierce.damageReductionPerPierce = damageReductionPerPierce;
    }
}