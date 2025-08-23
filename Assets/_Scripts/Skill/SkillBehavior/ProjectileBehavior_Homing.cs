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
    public bool singleShot = false;  // true: 1발, false: 다연장
    public int missileCount = 5;  // 다연장일 때 개수
    public float launchDelay = 0.1f;  // 다연장일 때 간격
    public float launchHeight = 1.5f;

    public override void Execute(SkillExecutionContext context)
    {
        Character character = context.Caster.GetComponent<Character>();
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

        if (singleShot)
        {
            // 단일 발사 - 가장 가까운 적 1명에게 1발
            Transform closestTarget = GetClosestEnemy(nearbyEnemies, context.Caster.transform);
            if (closestTarget != null)
            {
                LaunchSingleMissile(context, closestTarget, 0);
            }
        }
        else
        {
            // 다연장 발사 - 여러 발
            List<Transform> targets = AssignTargets(nearbyEnemies, missileCount);
            context.Caster.GetComponent<MonoBehaviour>().StartCoroutine(
                LaunchMissiles(context, targets)
            );
        }
    }
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
        // 기존 코드 동일...
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

        // Rigidbody 먼저 확인
        var rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;

        // 타겟 방향 계산
        Vector3 targetDirection = (target.position - launchPos).normalized;

        // 미사일이 타겟을 바라보도록 회전
        if (projectile != null)
        {
            projectile.transform.rotation = Quaternion.LookRotation(targetDirection);
        }

        // ElementalProjectile 설정
        var projScript = projectile.GetComponent<ElementalProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<ElementalProjectile>();
        }

        // speed 먼저 설정!
        projScript.speed = defaultSpeed;

        // Initialize 호출
        projScript.Initialize(
            context.Damage,
            context.Element,
            context.Passive,
            targetDirection
        );

        // Initialize 후에 다시 속도 설정 (덮어쓰기 방지)
        rb.velocity = targetDirection * defaultSpeed;

        // HomingComponent 설정
        var homing = projectile.GetComponent<HomingComponent>();
        if (homing == null)
        {
            homing = projectile.AddComponent<HomingComponent>();
        }

        homing.target = target;
        homing.rotationSpeed = homingRotationSpeed * 2f;  // 회전 속도 증가
        homing.maxHomingAngle = homingMaxAngle;

        Debug.Log($"[Missile #{index + 1}] 발사");
        Debug.Log($"  타겟: {target.name} at {target.position}");
        Debug.Log($"  방향: {targetDirection}");
        Debug.Log($"  속도: {rb.velocity}");
        Debug.Log($"  회전: {projectile.transform.rotation.eulerAngles}");
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
        return false;  // 자동 타겟팅
    }
}