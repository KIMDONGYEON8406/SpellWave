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

        Collider[] nearbyEnemies = Physics.OverlapSphere(
            context.Caster.transform.position,
            searchRange,
            LayerMask.GetMask("Enemy")
        );

        if (nearbyEnemies.Length == 0)
        {
            Debug.Log("[Explosion] 범위 내 적 없음");
            return;
        }

        int randomIndex = Random.Range(0, nearbyEnemies.Length);
        Vector3 explosionCenter = nearbyEnemies[randomIndex].transform.position;

        if (explosionDelay > 0)
        {
            context.Caster.GetComponent<MonoBehaviour>().StartCoroutine(
                DelayedExplosion(context, explosionCenter)
            );
        }
        else
        {
            PerformExplosion(context, explosionCenter);
        }
    }

    IEnumerator DelayedExplosion(SkillExecutionContext context, Vector3 explosionCenter)
    {
        CreateWarningEffect(explosionCenter, context.Range);
        yield return new WaitForSeconds(explosionDelay); 
        PerformExplosion(context, explosionCenter);
    }

    void PerformExplosion(SkillExecutionContext context, Vector3 explosionCenter)
    {
        float explosionRadius = context.Range;

        Collider[] victims = Physics.OverlapSphere(explosionCenter, explosionRadius, LayerMask.GetMask("Enemy"));

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

            // 프리팹 콜라이더가 0.5라면, 2배로 스케일
            float prefabRadius = 0.5f;  // 프리팹의 콜라이더 반지름
            float scaleMultiplier = explosionRadius / prefabRadius;

            explosion.transform.localScale = Vector3.one * scaleMultiplier;
            // Base Range 2 → 스케일 4
            // Base Range 3 → 스케일 6

            Object.Destroy(explosion, 2f);
        }

        Debug.Log($"[Explosion] 폭발! 위치: {explosionCenter}, 피해자: {victims.Length}명");
    }

    void CreateWarningEffect(Vector3 position, float radius)
    {
        GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        warning.transform.position = position + Vector3.up * 0.01f;
        warning.transform.localScale = new Vector3(radius * 2, 0.01f, radius * 2);

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