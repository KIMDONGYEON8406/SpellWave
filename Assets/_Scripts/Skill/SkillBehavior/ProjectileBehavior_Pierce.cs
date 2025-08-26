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

        // 발사체 개수 체크 추가
        var countModifier = context.Caster.GetComponent<ProjectileCountModifier>();
        int totalProjectiles = 1;

        // 스킬 이름 추출 (프리팹 이름에서 가져오기)
        string skillName = "Pierce";
        if (context.SkillPrefab != null)
        {
            skillName = context.SkillPrefab.name.Replace("Projectile_", "").Replace("_Prefab", "");
        }

        if (countModifier != null)
        {
            totalProjectiles = countModifier.GetTotalCount(skillName);
        }

        Vector3 baseDirection = context.Caster.transform.forward;

        // 다중 발사체 처리
        if (totalProjectiles > 1 && countModifier != null)
        {
            Vector3[] directions = countModifier.GetProjectileDirections(skillName, baseDirection);
            for (int i = 0; i < directions.Length; i++)
            {
                CreatePierceProjectile(context, directions[i], i);
            }

            Debug.Log($"[Pierce] {totalProjectiles}개 관통 발사체 발사!");
        }
        else
        {
            CreatePierceProjectile(context, baseDirection, 0);
        }

        // ⭐ 다중시전 체크
        if (context.MultiCastChance > 0 && !context.IsMultiCastInstance)
        {
            CheckMultiCast(context, skillName);
        }
    }

    // 새로 추가: 개별 발사체 생성 함수
    void CreatePierceProjectile(SkillExecutionContext context, Vector3 direction, int index)
    {
        // 발사 위치 (약간 위쪽에서, 인덱스별로 살짝 옆으로)
        Vector3 spawnPos = context.Caster.transform.position + Vector3.up;

        // 발사체 간 간격 추가 (선택사항)
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
            projectile = CreateDefaultPierceProjectile();
            projectile.transform.position = spawnPos;
            projectile.transform.rotation = Quaternion.LookRotation(direction);
        }

        // Rigidbody 설정
        var rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;

        // ElementalProjectile 설정
        var projScript = projectile.GetComponent<ElementalProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<ElementalProjectile>();
        }

        projScript.speed = projectileSpeed;
        projScript.Initialize(context.Damage, context.Element, context.Passive, direction);

        // 관통 컴포넌트 설정
        var pierce = projectile.GetComponent<PierceComponent>();
        if (pierce == null)
        {
            pierce = projectile.AddComponent<PierceComponent>();
        }

        pierce.maxPierceCount = pierceCount;
        pierce.damageReductionPerPierce = damageReductionPerPierce;

        // 속도 설정
        rb.velocity = direction * projectileSpeed;

        Debug.Log($"[Pierce #{index + 1}] 발사! 관통: {pierceCount}회");
    }

    // 새로 추가: 기본 관통 발사체 생성
    GameObject CreateDefaultPierceProjectile()
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        projectile.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);

        var collider = projectile.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        return projectile;
    }

    // 새로 추가: 다중시전 체크
    void CheckMultiCast(SkillExecutionContext context, string skillName)
    {
        var multiCast = context.Caster.GetComponent<MultiCastSystem>();
        if (multiCast == null) return;

        var skillManager = context.Caster.GetComponent<SkillManager>();
        if (skillManager == null) return;

        var skill = skillManager.GetSkill(skillName);
        if (skill == null) return;

        // 다중시전 처리를 위한 더미 리스트
        System.Collections.Generic.List<GameObject> dummyList = new System.Collections.Generic.List<GameObject>();
        multiCast.ProcessMultiCast(skill, context, dummyList);
    }

    public override bool RequiresTarget()
    {
        return true;  // Pierce는 타겟 필요
    }
}