using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Projectile Homing", menuName = "SpellWave/Skills/Behaviors/Projectile/Homing")]
public class ProjectileBehavior_Homing : ProjectileBehavior
{
    [Header("유도 발사체 설정")]
    public float defaultSpeed = 8f;
    public float homingRotationSpeed = 5f;
    public float homingMaxAngle = 45f;

    [Header("발사 모드")]
    public bool singleShot = false;
    public int missileCount = 5;
    public float launchDelay = 0.1f;
    public float launchHeight = 1.5f;

    public override void Execute(SkillExecutionContext context)
    {
        Player character = context.Caster.GetComponent<Player>();
        float searchRange = character != null ? character.AttackRange : 10f;

        Collider[] nearbyEnemies = Physics.OverlapSphere(
            context.Caster.transform.position,
            searchRange,
            LayerMask.GetMask("Enemy")
        );

        if (nearbyEnemies.Length == 0)
        {
            Debug.Log("[Homing] 타겟 없음");
            return;
        }

        // Context에서 개수 가져오기, 없으면 기본값 사용
        int totalMissiles = context.BaseProjectileCount > 0 ?
                           context.BaseProjectileCount :
                           missileCount;

        DebugManager.LogImportant($"[Missile] 발사 개수: {totalMissiles} (Context: {context.BaseProjectileCount}, Default: {missileCount})");

        if (singleShot)
        {
            Transform closestTarget = GetClosestEnemy(nearbyEnemies, context.Caster.transform);
            if (closestTarget != null)
            {
                LaunchSingleMissile(context, closestTarget, 0);
            }
        }
        else
        {
            List<Transform> targets = AssignTargets(nearbyEnemies, totalMissiles);
            context.Caster.GetComponent<MonoBehaviour>().StartCoroutine(
                LaunchMissiles(context, targets)
            );
        }
    }

    void LaunchMultipleProjectiles(SkillExecutionContext context, Collider[] enemies, int count)
    {
        var countModifier = context.Caster.GetComponent<ProjectileCountModifier>();
        Transform mainTarget = GetClosestEnemy(enemies, context.Caster.transform);

        if (mainTarget == null) return;

        Vector3 baseDirection = (mainTarget.position - context.Caster.transform.position).normalized;
        Vector3[] directions = countModifier.GetProjectileDirections("Missile", baseDirection);

        for (int i = 0; i < directions.Length; i++)
        {
            // 각 방향으로 미사일 발사
            LaunchDirectionalMissile(context, mainTarget, directions[i], i);
        }
    }

    void LaunchDirectionalMissile(SkillExecutionContext context, Transform target, Vector3 direction, int index)
    {
        Vector3 launchPos = context.Caster.transform.position +
                           Vector3.up * launchHeight +
                           direction * 0.5f;

        GameObject projectile = null;

        if (context.SkillPrefab != null)
        {
            projectile = Object.Instantiate(
                context.SkillPrefab,
                launchPos,
                Quaternion.LookRotation(direction)
            );
        }
        else
        {
            projectile = CreateDefaultMissile();
            projectile.transform.position = launchPos;
            projectile.transform.rotation = Quaternion.LookRotation(direction);
        }

        var rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.velocity = direction * defaultSpeed;

        var projScript = projectile.GetComponent<ElementalProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<ElementalProjectile>();
        }

        projScript.speed = defaultSpeed;
        projScript.Initialize(
            context.Damage,
            context.Element,
            context.Passive,
            direction
        );

        var homing = projectile.GetComponent<HomingComponent>();
        if (homing == null)
        {
            homing = projectile.AddComponent<HomingComponent>();
        }

        homing.target = target;
        homing.rotationSpeed = homingRotationSpeed * 2f;
        homing.maxHomingAngle = homingMaxAngle;

        Debug.Log($"[Missile #{index + 1}] 발사 (다중 발사체)");
    }

    // 나머지 메서드들은 기존과 동일...
    Transform GetClosestEnemy(Collider[] enemies, Transform caster)
    {
        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(caster.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemy.transform;
            }
        }

        return closest;
    }

    List<Transform> AssignTargets(Collider[] enemies, int missileNum)
    {
        List<Transform> targets = new List<Transform>();

        if (enemies.Length >= missileNum)
        {
            for (int i = 0; i < missileNum; i++)
            {
                targets.Add(enemies[i].transform);
            }
        }
        else
        {
            for (int i = 0; i < missileNum; i++)
            {
                targets.Add(enemies[i % enemies.Length].transform);
            }
        }

        return targets;
    }

    IEnumerator LaunchMissiles(SkillExecutionContext context, List<Transform> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                LaunchSingleMissile(context, targets[i], i);
            }

            if (launchDelay > 0)
            {
                yield return new WaitForSeconds(launchDelay);
            }
        }
    }

    void LaunchSingleMissile(SkillExecutionContext context, Transform target, int index)
    {
        Vector3 spreadOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            0,
            Random.Range(-0.5f, 0.5f)
        );

        Vector3 launchPos = context.Caster.transform.position +
                           Vector3.up * launchHeight +
                           spreadOffset;

        GameObject projectile = null;

        if (context.SkillPrefab != null)
        {
            projectile = Object.Instantiate(
                context.SkillPrefab,
                launchPos,
                Quaternion.identity
            );
        }
        else
        {
            projectile = CreateDefaultMissile();
            projectile.transform.position = launchPos;
        }

        var rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;

        Vector3 targetDirection = (target.position - launchPos).normalized;
        projectile.transform.rotation = Quaternion.LookRotation(targetDirection);

        var projScript = projectile.GetComponent<ElementalProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<ElementalProjectile>();
        }

        projScript.speed = defaultSpeed;
        projScript.Initialize(
            context.Damage,
            context.Element,
            context.Passive,
            targetDirection
        );

        rb.velocity = targetDirection * defaultSpeed;

        var homing = projectile.GetComponent<HomingComponent>();
        if (homing == null)
        {
            homing = projectile.AddComponent<HomingComponent>();
        }

        homing.target = target;
        homing.rotationSpeed = homingRotationSpeed * 2f;
        homing.maxHomingAngle = homingMaxAngle;

        Debug.Log($"[Missile #{index + 1}] 발사");
    }

    GameObject CreateDefaultMissile()
    {
        GameObject missile = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        missile.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);

        var collider = missile.GetComponent<Collider>();
        if (collider != null) collider.isTrigger = true;

        var rb = missile.AddComponent<Rigidbody>();
        rb.useGravity = false;

        return missile;
    }

    public override bool RequiresTarget()
    {
        return false;
    }
}