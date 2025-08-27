using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Basic", menuName = "SpellWave/Skills/Behaviors/Projectile/Basic")]
public class ProjectileBehavior_Basic : ProjectileBehavior
{
    [Header("기본 발사체 설정")]
    public float defaultSpeed = 10f;
    public float defaultLifetime = 3f;

    public override void Execute(SkillExecutionContext context)
    {
        if (context.Target == null) return;

        int projectileCount = context.BaseProjectileCount;
        string skillName = context.SkillName ?? "Bolt";

        DebugManager.LogImportant($"[{skillName}] 발사 - 총 {projectileCount}개");

        Vector3 baseDir = (context.Target.position - context.Caster.transform.position).normalized;

        // 부채꼴 패턴 설정
        float spreadAngle = 30f; // 30도 범위

        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 fireDirection = baseDir;

            if (projectileCount > 1)
            {
                // 각 발사체마다 다른 각도
                float angle = Mathf.Lerp(-spreadAngle / 2, spreadAngle / 2, i / (float)(projectileCount - 1));
                fireDirection = Quaternion.Euler(0, angle, 0) * baseDir;
            }

            CreateSingleProjectile(context, fireDirection, i);
        }
    }

    private void CreateSingleProjectile(SkillExecutionContext context, Vector3 direction, int index)
    {
        Vector3 spawnPos = context.Caster.transform.position + Vector3.up * 0.5f;

        // 발사체 간격 추가 - 부채꼴 패턴
        if (context.BaseProjectileCount > 1)
        {
            float spreadAngle = 30f; // 전체 각도
            float angleStep = spreadAngle / (context.BaseProjectileCount - 1);
            float startAngle = -spreadAngle / 2f;
            float currentAngle = startAngle + (angleStep * index);

            // 방향 회전
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            direction = rotation * direction;

            // 옆으로도 약간 띄우기 (선택사항)
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            spawnPos += perpendicular * (index - (context.BaseProjectileCount - 1) * 0.5f) * 0.3f;
        }

        GameObject projectile = null;

        if (context.SkillPrefab != null)
        {
            projectile = Instantiate(
                context.SkillPrefab,
                spawnPos,
                Quaternion.LookRotation(direction)
            );
        }
        else
        {
            projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.position = spawnPos;
            projectile.transform.localScale = Vector3.one * 0.3f;
            var col = projectile.GetComponent<Collider>();
            if (col) col.isTrigger = true;
        }

        // Rigidbody 설정
        var rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.velocity = direction * defaultSpeed;

        var projComponent = projectile.GetComponent<ElementalProjectile>();
        if (projComponent == null)
        {
            projComponent = projectile.AddComponent<ElementalProjectile>();
        }

        projComponent.Initialize(
            context.Damage,
            context.Element,
            context.Passive,
            direction
        );
        projComponent.speed = defaultSpeed;
        projComponent.lifetime = defaultLifetime;

        DebugManager.LogSkill($"Bolt #{index + 1} - 각도: {Quaternion.LookRotation(direction).eulerAngles.y:F1}°");
    }
}