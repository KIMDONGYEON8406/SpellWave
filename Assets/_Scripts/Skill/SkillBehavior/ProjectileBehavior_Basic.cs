using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Basic", menuName = "SpellWave/Skills/Behaviors/Projectile/Basic")]
public class ProjectileBehavior_Basic : ProjectileBehavior
{
    [Header("기본 발사체 설정")]
    public float defaultSpeed = 10f;
    public float defaultLifetime = 3f;

    public override void Execute(SkillExecutionContext context)
    {
        // 플레이어 정면 방향
        Vector3 direction = context.Caster.transform.forward;

        // 스폰 위치 (플레이어 중앙 + 약간 앞)
        Vector3 spawnPos = context.Caster.transform.position +
                          context.Caster.transform.forward * 0.5f +
                          Vector3.up * 1f;

        GameObject projectile = null;

        if (context.SkillPrefab != null)
        {
            projectile = Object.Instantiate(
                context.SkillPrefab,
                spawnPos,
                Quaternion.LookRotation(direction),
                null
            );
        }
        else
        {
            Debug.LogWarning($"Skill Prefab이 없어서 자동 생성!");
            projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.position = spawnPos;
            projectile.transform.localScale = Vector3.one * 0.3f;

            var collider = projectile.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;
        }

        // ElementalProjectile 설정
        var projScript = projectile.GetComponent<ElementalProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<ElementalProjectile>();
        }

        var rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;

        // 초기화 - Forward 방향으로!
        projScript.speed = defaultSpeed;
        projScript.Initialize(
            context.Damage,
            context.Element,
            context.Passive,
            direction  // Forward 방향
        );

        // 속도 즉시 적용
        rb.velocity = direction * defaultSpeed;

        //Debug.Log($"발사! 방향={direction}, 위치={projectile.transform.position}");
    }
}