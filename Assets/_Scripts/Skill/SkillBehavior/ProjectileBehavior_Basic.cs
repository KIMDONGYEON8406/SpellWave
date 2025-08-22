using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Basic", menuName = "SpellWave/Skills/Behaviors/Projectile/Basic")]
public class ProjectileBehavior_Basic : ProjectileBehavior
{
    [Header("기본 발사체 설정")]
    public float defaultSpeed = 10f;
    public float defaultLifetime = 3f;

    public override void Execute(SkillExecutionContext context)
    {
        if (context.Target == null)
        {
            Debug.LogWarning("타겟이 없습니다!");
            return;
        }

        Vector3 direction = (context.Target.position - context.Caster.transform.position).normalized;
        Vector3 spawnPos = context.Caster.transform.position + Vector3.up;
        GameObject projectile = null;

        if (context.SkillPrefab != null)
        {
            // ✅ 올바른 Instantiate 사용
            projectile = Object.Instantiate(
                context.SkillPrefab,
                spawnPos,  // 위치
                Quaternion.LookRotation(direction),  // 회전
                null  // 부모 없음 (월드 공간)
            );

            projectile.name = "Bolt_Projectile";

            // 혹시 모를 부모 체크
            if (projectile.transform.parent != null)
            {
                Debug.LogError($"발사체가 {projectile.transform.parent.name}의 자식!");
                projectile.transform.SetParent(null);
            }
        }
        else
        {
            Debug.LogWarning("SkillPrefab이 null! 기본 구체 생성");

            projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.position = spawnPos;
            projectile.transform.localScale = Vector3.one * 0.3f;
            projectile.name = "Bolt_Default";

            var collider = projectile.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        // ElementalProjectile 설정
        var projScript = projectile.GetComponent<ElementalProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<ElementalProjectile>();
        }

        // Rigidbody 확인
        var rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;

        // 초기화
        projScript.speed = defaultSpeed;
        projScript.Initialize(
            context.Damage,
            context.Element,
            context.Passive,
            direction
        );

        // 속도 즉시 적용
        rb.velocity = direction * defaultSpeed;

       // Debug.Log($"발사! Parent={projectile.transform.parent}, Pos={projectile.transform.position}");
    }
}