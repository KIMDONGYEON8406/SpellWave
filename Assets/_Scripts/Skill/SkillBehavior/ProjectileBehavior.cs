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

        // Context에서 발사체 개수 가져오기
        int projectileCount = context.BaseProjectileCount;

        // 스킬 이름 확인
        string skillName = context.SkillName ?? "Unknown";

        DebugManager.LogSkill($"[{skillName}] 발사체 실행 - 개수: {projectileCount}");

        Vector3 baseDirection = (context.Target.position - context.Caster.transform.position).normalized;

        // 여러 개 발사
        if (projectileCount > 1)
        {
            var countModifier = context.Caster.GetComponent<ProjectileCountModifier>();
            if (countModifier != null)
            {
                Vector3[] directions = countModifier.GetProjectileDirections(skillName, baseDirection);
                for (int i = 0; i < directions.Length; i++)
                {
                    CreateProjectile(context, directions[i], i);
                }
            }
        }
        else
        {
            CreateProjectile(context, baseDirection, 0);
        }
    }

    void CreateProjectile(SkillExecutionContext context, Vector3 direction, int index)
    {
        Vector3 spawnPos = context.Caster.transform.position + Vector3.up;

        // 발사체 간 간격
        if (index > 0)
        {
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            float spacing = 0.2f;
            spawnPos += perpendicular * spacing * (index % 2 == 0 ? 1 : -1) * ((index + 1) / 2);
        }

        GameObject projectile = null;

        if (context.SkillPrefab != null)
        {
            projectile = Object.Instantiate(
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
            var collider = projectile.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;
        }

        // Rigidbody 설정
        var rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;

        var projScript = projectile.GetComponent<ElementalProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<ElementalProjectile>();
        }

        projScript.speed = projectileSpeed;
        projScript.Initialize(context.Damage, context.Element, context.Passive, direction);

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

        DebugManager.LogSkill($"[{context.SkillName}] 발사체 #{index + 1} 생성 완료");
    }
}