using UnityEngine;

[CreateAssetMenu(fileName = "Area Instant", menuName = "SpellWave/Skills/Behaviors/Area/Instant")]
public class AreaAttackBehavior_Instant : AreaAttackBehavior
{
    [Header("즉시 폭발 설정")] //익스플로전타입 
    public float explosionVisualScale = 1.5f;
    public float knockbackForce = 5f;
    public bool showShockwave = true;

    public override void Execute(SkillExecutionContext context)
    {
        // 익스플로전 특성: 플레이어 중심, 즉시 폭발
        centerOnCaster = true;
        explosionDelay = 0f;

        // 기본 폭발 처리
        base.Execute(context);

        // 폭발 시각 효과
        if (context.HitEffectPrefab != null)
        {
            GameObject explosion = Object.Instantiate(
                context.HitEffectPrefab,
                context.Caster.transform.position,
                Quaternion.identity
            );

            explosion.transform.localScale = Vector3.one * context.Range * explosionVisualScale;
            Object.Destroy(explosion, 2f);
        }

        // 충격파 효과 (선택)
        if (showShockwave)
        {
            CreateShockwave(context);
        }
    }

    void CreateShockwave(SkillExecutionContext context)
    {
        // 충격파 이펙트 생성 (나중에 구현)
        Debug.Log("폭발 충격파 발생!");
    }

    public override bool RequiresTarget()
    {
        return false; // 익스플로전은 타겟 불필요
    }
}