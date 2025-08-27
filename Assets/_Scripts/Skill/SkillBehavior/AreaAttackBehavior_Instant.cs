using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Area Instant", menuName = "SpellWave/Skills/Behaviors/Area/Instant")]
public class AreaAttackBehavior_Instant : SkillBehavior
{
    [Header("익스플로전 설정")]
    public float explosionDelay = 0.5f;

    public override void Execute(SkillExecutionContext context)
    {
        Player character = context.Caster.GetComponent<Player>();
        float searchRange = character.AttackRange;

        var countModifier = context.Caster.GetComponent<ProjectileCountModifier>();
        int explosionCount = 1;

        if (countModifier != null)
        {
            // AreaCount를 체크하도록 수정
            explosionCount = countModifier.GetTotalCount("Explosion");
            if (explosionCount == 1)  // 기본값이면 context 체크
            {
                explosionCount = context.BaseProjectileCount > 0 ? context.BaseProjectileCount : 1;
            }
            DebugManager.LogSkill($"[Explosion] 개수: {explosionCount}");
        }

        Collider[] nearbyEnemies = Physics.OverlapSphere(
            context.Caster.transform.position,
            searchRange,
            LayerMask.GetMask("Enemy")
        );

        if (nearbyEnemies.Length == 0)
        {
            DebugManager.LogSkill("[Explosion] 범위 내 적 없음");
            return;
        }

        // ⭐ 여러 개의 폭발 생성
        for (int i = 0; i < explosionCount; i++)
        {
            CreateExplosion(context, nearbyEnemies, i, explosionCount);
        }
    }

    void CreateExplosion(SkillExecutionContext context, Collider[] enemies, int index, int totalCount)
    {
        // 타겟 선택 (각 폭발마다 다른 적 선택 가능)
        int targetIndex = (index < enemies.Length) ? index : Random.Range(0, enemies.Length);
        Vector3 explosionCenter = enemies[targetIndex].transform.position;

        // 다중시전 위치 오프셋 적용
        if (context.IsMultiCastInstance && context.PositionOffset != Vector3.zero)
        {
            explosionCenter += context.PositionOffset;
        }
        // 일반 다중 폭발 위치 분산
        else if (totalCount > 1)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 2f;
            explosionCenter += new Vector3(randomOffset.x, 0, randomOffset.y);
        }

        // 딜레이 적용 (각 폭발마다 약간의 시차)
        float adjustedDelay = explosionDelay + (index * 0.1f);

        if (adjustedDelay > 0)
        {
            context.Caster.GetComponent<MonoBehaviour>().StartCoroutine(
                DelayedExplosion(context, explosionCenter, index)
            );
        }
        else
        {
            PerformExplosion(context, explosionCenter, index);
        }
    }

    IEnumerator DelayedExplosion(SkillExecutionContext context, Vector3 explosionCenter, int index)
    {
        CreateWarningEffect(explosionCenter, context.Range);
        yield return new WaitForSeconds(explosionDelay);
        PerformExplosion(context, explosionCenter, index);
    }

    void PerformExplosion(SkillExecutionContext context, Vector3 explosionCenter, int index)
    {
        // Context의 Range 사용 (이미 보너스 적용된 값)
        float explosionRadius = context.Range;

        DebugManager.LogImportant($"[Explosion #{index + 1}] 실제 폭발 범위: {explosionRadius:F1}m");

        Collider[] victims = Physics.OverlapSphere(explosionCenter, explosionRadius, LayerMask.GetMask("Enemy"));

        DebugManager.LogCombat($"[Explosion #{index + 1}] 위치: {explosionCenter}, 범위: {explosionRadius:F1}, 피해자: {victims.Length}명");

        foreach (Collider enemy in victims)
        {
            var enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(context.Damage);

                var effectApplier = enemy.GetComponent<UnifiedPassiveEffect>();
                if (effectApplier == null)
                {
                    effectApplier = enemy.gameObject.AddComponent<UnifiedPassiveEffect>();
                }
                effectApplier.ApplyEffect(context.Passive.type, context.Passive, context.Damage);
            }
        }

        if (context.SkillPrefab != null)
        {
            GameObject explosion = Object.Instantiate(
                context.SkillPrefab,
                explosionCenter,
                Quaternion.identity
            );

            // 크기를 실제 범위에 맞게 조정
            float prefabBaseRadius = 0.5f;
            float scaleMultiplier = explosionRadius / prefabBaseRadius;
            explosion.transform.localScale = Vector3.one * scaleMultiplier;

            DebugManager.LogSkill($"[Explosion Visual] Scale: {scaleMultiplier:F2}x (범위: {explosionRadius:F1}m)");

            Object.Destroy(explosion, 2f);
        }
        else
        {
            CreateDefaultExplosion(explosionCenter, explosionRadius);
        }
    }

    void CreateDefaultExplosion(Vector3 position, float radius)
    {
        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.transform.position = position;
        explosion.transform.localScale = Vector3.one * radius * 2; // 직경으로 설정

        DebugManager.LogSkill($"[Default Explosion] 범위: {radius:F1}m, Scale: {radius * 2:F1}");

        Destroy(explosion.GetComponent<Collider>());

        var renderer = explosion.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 0.5f, 0f, 0.3f);
        }

        Object.Destroy(explosion, 0.5f);
    }

    void CreateWarningEffect(Vector3 position, float radius)
    {
        GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        warning.transform.position = position + Vector3.up * 0.01f;
        warning.transform.localScale = new Vector3(radius * 2, 0.01f, radius * 2); // 범위 반영

        DebugManager.LogSkill($"[Warning] 범위: {radius:F1}m");

        Object.Destroy(warning.GetComponent<Collider>());

        var renderer = warning.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 0f, 0f, 0.3f);
        }

        Object.Destroy(warning, explosionDelay);
    }

    public override bool RequiresTarget()
    {
        return false;
    }
}