using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Arrow", menuName = "SpellWave/Skills/Behaviors/Projectile/Arrow")]
public class ProjectileBehavior_Arrow : ProjectileBehavior
{
    [Header("화살 설정")]
    public float arrowSpeed = 15f;
    public float arrowLifetime = 2f;

    [Header("발사 패턴")]
    public int arrowCount = 3;  // 기본 화살 개수
    public float spreadAngle = 15f;
    public bool fanPattern = true;

    [Header("관통 설정")]
    public bool enablePierce = true;
    public float damageReduction = 0.8f;

    public override void Execute(SkillExecutionContext context)
    {
        projectileSpeed = arrowSpeed;
        isHoming = false;

        // 발사체 개수 체크 (Context 우선, 없으면 기본값)
        int totalArrows = context.BaseProjectileCount > 1 ? context.BaseProjectileCount : arrowCount;

        // ProjectileCountModifier 체크 (더 정확한 방법)
        var countModifier = context.Caster.GetComponent<ProjectileCountModifier>();
        if (countModifier != null)
        {
            totalArrows = countModifier.GetTotalCount("Arrow");
        }

        if (fanPattern && totalArrows > 1)
        {
            FireFanPatternWithCount(context, totalArrows);
        }
        else
        {
            FireSingleArrow(context, context.Caster.transform.forward, 0);
        }

        // 다중시전 체크
        if (context.MultiCastChance > 0 && !context.IsMultiCastInstance)
        {
            CheckMultiCast(context);
        }
    }
    void FireFanPatternWithCount(SkillExecutionContext context, int count)
    {
        float angleStep = spreadAngle / (count - 1);
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 direction = rotation * context.Caster.transform.forward;

            FireSingleArrow(context, direction, i);
        }

        DebugManager.LogSkill($"[Arrow] 부채꼴 패턴: {count}개 발사!");
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

        // 관통 설정
        if (enablePierce && base.pierceCount > 0)
        {
            var pierce = arrow.GetComponent<PierceComponent>();
            if (pierce == null)
            {
                pierce = arrow.AddComponent<PierceComponent>();
            }
            pierce.maxPierceCount = base.pierceCount;
            pierce.damageReductionPerPierce = damageReduction;
        }

        rb.velocity = direction * arrowSpeed;

        DebugManager.LogSkill($"[Arrow #{index + 1}] 발사! 방향: {direction}");
    }

    // 다중시전 체크 함수 추가
    void CheckMultiCast(SkillExecutionContext context)
    {
        var multiCast = context.Caster.GetComponent<MultiCastSystem>();
        if (multiCast == null) return;

        var skillManager = context.Caster.GetComponent<SkillManager>();
        if (skillManager == null) return;

        var arrowSkill = skillManager.GetSkill("Arrow");
        if (arrowSkill == null) return;

        // 다중시전 처리를 위한 더미 리스트 (화살은 즉시 발사되므로)
        System.Collections.Generic.List<GameObject> dummyList = new System.Collections.Generic.List<GameObject>();
        multiCast.ProcessMultiCast(arrowSkill, context, dummyList);
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