using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Pierce", menuName = "SpellWave/Skills/Behaviors/Projectile/Pierce")]
public class ProjectileBehavior_Pierce : ProjectileBehavior
{
    [Header("관통 발사체 설정")] //애로우타입, 관통공격
    public float defaultSpeed = 15f;
    public int defaultPierceCount = 3;
    public float damageReductionPerPierce = 0.8f;

    public override void Execute(SkillExecutionContext context)
    {
        // 애로우 특성: 빠른 속도, 3명 관통
        projectileSpeed = defaultSpeed;
        isHoming = false;
        pierceCount = defaultPierceCount;

        if (context.Target != null && context.SkillPrefab != null)
        {
            Vector3 direction = (context.Target.position - context.Caster.transform.position).normalized;

            GameObject projectile = Object.Instantiate(
                context.SkillPrefab,
                context.Caster.transform.position + Vector3.up,
                Quaternion.LookRotation(direction)
            );

            // ElementalProjectile 설정
            var projScript = projectile.GetComponent<ElementalProjectile>();
            if (projScript == null)
                projScript = projectile.AddComponent<ElementalProjectile>();

            projScript.Initialize(context.Damage, context.Element, context.Passive, direction);
            projScript.speed = projectileSpeed;

            // PierceComponent 설정
            var pierce = projectile.GetComponent<PierceComponent>();
            if (pierce == null)
                pierce = projectile.AddComponent<PierceComponent>();

            pierce.maxPierceCount = pierceCount;
            pierce.damageReductionPerPierce = damageReductionPerPierce;
        }
    }
}