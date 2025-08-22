using UnityEngine;

[CreateAssetMenu(fileName = "Area Attack Behavior", menuName = "SpellWave/Skills/Behaviors/AreaAttack")]
public class AreaAttackBehavior : SkillBehavior
{
    [Header("영역 공격 설정")]
    public bool centerOnCaster = true;
    public float explosionDelay = 0f;

    public override void Execute(SkillExecutionContext context)
    {
        Vector3 center = centerOnCaster ?
            context.Caster.transform.position :
            context.Target?.position ?? context.Caster.transform.position;

        Collider[] enemies = Physics.OverlapSphere(center, context.Range, LayerMask.GetMask("Enemy"));

        foreach (Collider enemy in enemies)
        {
            var enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(context.Damage);
                // context.Damage 추가로 전달
                ApplyPassiveEffect(enemyAI, context.Element, context.Passive, context.Damage);
            }
        }

        if (context.HitEffectPrefab != null)
        {
            Object.Instantiate(context.HitEffectPrefab, center, Quaternion.identity);
        }
    }

    public override bool CanExecute(SkillExecutionContext context)
    {
        return true;
    }

    // damage 매개변수 추가
    void ApplyPassiveEffect(EnemyAI enemy, ElementType element, PassiveEffect passive, float damage)
    {
        var effectApplier = enemy.GetComponent<UnifiedPassiveEffect>();
        if (effectApplier == null)
        {
            effectApplier = enemy.gameObject.AddComponent<UnifiedPassiveEffect>();
        }
        effectApplier.ApplyEffect(passive.type, passive, damage);
    }
}