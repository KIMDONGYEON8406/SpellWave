using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Homing", menuName = "SpellWave/Skills/Behaviors/Projectile/Homing")]
public class ProjectileBehavior_Homing : ProjectileBehavior
{
    [Header("유도 발사체 설정")] //미사일타입 , 유도공격
    public float defaultSpeed = 8f;
    public float homingRotationSpeed = 5f;
    public float homingMaxAngle = 45f;
    public float targetSearchRadius = 15f;

    public override void Execute(SkillExecutionContext context)
    {
        // 미사일 특성: 느린 속도, 유도 기능
        projectileSpeed = defaultSpeed;
        isHoming = true;
        pierceCount = 0;

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

            // HomingComponent 설정
            var homing = projectile.GetComponent<HomingComponent>();
            if (homing == null)
                homing = projectile.AddComponent<HomingComponent>();

            homing.target = context.Target;
            homing.rotationSpeed = homingRotationSpeed;
            homing.maxHomingAngle = homingMaxAngle;
        }
    }

    public override bool RequiresTarget()
    {
        return true; // 미사일은 타겟 필수
    }
}