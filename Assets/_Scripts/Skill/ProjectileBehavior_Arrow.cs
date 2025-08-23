using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Arrow", menuName = "SpellWave/Skills/Behaviors/Projectile/Arrow")]
public class ProjectileBehavior_Arrow : ProjectileBehavior
{
    [Header("화살 설정")]
    public float arrowSpeed = 15f;
    public float arrowLifetime = 2f;

    [Header("발사 패턴")]
    public int arrowCount = 3;
    public float spreadAngle = 15f;
    public bool fanPattern = true;

    [Header("관통 설정")]
    public bool enablePierce = true;
    public float damageReduction = 0.8f;

    public override void Execute(SkillExecutionContext context)
    {
        projectileSpeed = arrowSpeed;
        isHoming = false;

        if (fanPattern && arrowCount > 1)
        {
            FireFanPattern(context);
        }
        else
        {
            FireSingleArrow(context, context.Caster.transform.forward, 0);
        }
    }

    void FireFanPattern(SkillExecutionContext context)
    {
        float angleStep = spreadAngle / (arrowCount - 1);
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < arrowCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 direction = rotation * context.Caster.transform.forward;

            FireSingleArrow(context, direction, i);
        }
    }

    void FireSingleArrow(SkillExecutionContext context, Vector3 direction, int index)
    {
        Vector3 spawnPos = context.Caster.transform.position +
                          Vector3.up * 1f +
                          context.Caster.transform.forward * 0.5f;

        GameObject arrow = null;

        if (context.SkillPrefab != null)
        {
            arrow = Object.Instantiate(
                context.SkillPrefab,
                spawnPos,
                Quaternion.LookRotation(direction)
            );
        }
        else
        {
            arrow = CreateDefaultArrow();
            arrow.transform.position = spawnPos;
            arrow.transform.rotation = Quaternion.LookRotation(direction);
        }

        // Rigidbody 설정
        var rb = arrow.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = arrow.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;

        // ElementalProjectile 설정
        var projScript = arrow.GetComponent<ElementalProjectile>();
        if (projScript == null)
        {
            projScript = arrow.AddComponent<ElementalProjectile>();
        }

        projScript.speed = arrowSpeed;
        projScript.lifetime = arrowLifetime;
        projScript.isHoming = false;
        projScript.Initialize(
            context.Damage,
            context.Element,
            context.Passive,
            direction
        );

        // 관통 설정 - 부모의 pierceCount 사용!
        if (enablePierce && base.pierceCount > 0)  // base.pierceCount 또는 그냥 pierceCount
        {
            var pierce = arrow.GetComponent<PierceComponent>();
            if (pierce == null)
            {
                pierce = arrow.AddComponent<PierceComponent>();
            }
            pierce.maxPierceCount = base.pierceCount;  // 부모 필드 사용
            pierce.damageReductionPerPierce = damageReduction;
        }

        rb.velocity = direction * arrowSpeed;

        Debug.Log($"[Arrow #{index + 1}] 발사! 방향: {direction}");
    }

    GameObject CreateDefaultArrow()
    {
        GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arrow.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);

        var collider = arrow.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        return arrow;
    }

    public override bool RequiresTarget()
    {
        return false;
    }
}