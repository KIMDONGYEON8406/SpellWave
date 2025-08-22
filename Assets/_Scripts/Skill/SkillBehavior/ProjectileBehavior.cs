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

        GameObject projectile = null;

        if (context.SkillPrefab != null)
        {
            // 프리팹이 있으면 사용
            projectile = Object.Instantiate(
                context.SkillPrefab,
                context.Caster.transform.position + Vector3.up,
                Quaternion.LookRotation(direction)
            );
        }
        else
        {
            // 프리팹이 없으면 기본 생성
            projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.position = context.Caster.transform.position + Vector3.up;
            projectile.transform.localScale = Vector3.one * 0.3f;

            var collider = projectile.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;

            var rb = projectile.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }

        // ElementalProjectile 설정 (망토 효과용)
        var projScript = projectile.GetComponent<ElementalProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<ElementalProjectile>();
        }

        // 중요: speed를 먼저 설정
        projScript.speed = projectileSpeed;
        projScript.Initialize(context.Damage, context.Element, context.Passive, direction);

        // 추가 컴포넌트
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