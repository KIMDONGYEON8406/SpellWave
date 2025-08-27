using UnityEngine;
using System.Collections.Generic;

public class UnifiedPassiveEffect : MonoBehaviour
{
    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();
    private EnemyAI enemy;

    void Start()
    {
        enemy = GetComponent<EnemyAI>();
    }

    // 효과 적용
    public void ApplyEffect(PassiveType type, PassiveEffect effect, float damage = 0)
    {
        // 중복 체크
        foreach (var activeEffect in activeEffects)
        {
            if (activeEffect.type == type)
            {
                activeEffect.Refresh();  // 기존 효과 갱신
                return;
            }
        }

        // 새 효과 추가
        ActiveEffect newEffect = null;

        switch (type)
        {
            case PassiveType.Burn:
                newEffect = new BurnEffect(effect.effectValue, effect.duration);
                break;

            case PassiveType.Slow:
                newEffect = new SlowEffect(effect.effectValue, effect.duration, enemy);
                break;

            case PassiveType.Poison:
                newEffect = new PoisonEffect(effect.effectValue, effect.duration);
                break;
        }

        if (newEffect != null)
        {
            activeEffects.Add(newEffect);
            DebugManager.LogSkill($"{type} 효과 적용!");
        }
    }

    void Update()
    {
        // 모든 효과 업데이트
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].Update(enemy);

            if (activeEffects[i].IsExpired())
            {
                activeEffects[i].OnRemove();
                activeEffects.RemoveAt(i);
            }
        }
    }
}

// 효과 베이스 클래스
public abstract class ActiveEffect
{
    public PassiveType type;
    public float duration;
    public float startTime;

    public ActiveEffect(float dur)
    {
        duration = dur;
        startTime = Time.time;
    }

    public virtual bool IsExpired()
    {
        return Time.time - startTime >= duration;
    }

    public virtual void Refresh()
    {
        startTime = Time.time;
    }

    public abstract void Update(EnemyAI enemy);
    public abstract void OnRemove();
}

// 화상 효과
public class BurnEffect : ActiveEffect
{
    private float damagePerSecond;
    private float lastDamageTime;

    public BurnEffect(float dps, float dur) : base(dur)
    {
        type = PassiveType.Burn;
        damagePerSecond = dps;
    }

    public override void Update(EnemyAI enemy)
    {
        if (Time.time - lastDamageTime >= 1f)
        {
            if (enemy != null)
            {
                enemy.TakeDamage(damagePerSecond);
                DebugManager.LogSkill($"화상 데미지: {damagePerSecond}");
            }
            lastDamageTime = Time.time;
        }
    }

    public override void OnRemove()
    {
        DebugManager.LogSkill("화상 효과 종료");
    }
}

// 둔화 효과
public class SlowEffect : ActiveEffect
{
    private float slowPercent;
    private float originalSpeed;
    private EnemyAI targetEnemy;

    public SlowEffect(float slow, float dur, EnemyAI enemy) : base(dur)
    {
        type = PassiveType.Slow;
        slowPercent = slow;
        targetEnemy = enemy;

        if (enemy != null)
        {
            var stats = enemy.GetRuntimeStats();
            originalSpeed = stats.moveSpeed;
            stats.moveSpeed *= (1f - slowPercent / 100f);
        }
    }

    public override void Update(EnemyAI enemy)
    {
        // 업데이트 필요 없음
    }

    public override void OnRemove()
    {
        if (targetEnemy != null)
        {
            var stats = targetEnemy.GetRuntimeStats();
            stats.moveSpeed = originalSpeed;
        }
        DebugManager.LogSkill("둔화 효과 종료");
    }
}

// 독 효과
public class PoisonEffect : ActiveEffect
{
    private float damagePerSecond;
    private float lastDamageTime;

    public PoisonEffect(float dps, float dur) : base(dur)
    {
        type = PassiveType.Poison;
        damagePerSecond = dps;
    }

    public override void Update(EnemyAI enemy)
    {
        if (Time.time - lastDamageTime >= 1f)
        {
            if (enemy != null)
            {
                enemy.TakeDamage(damagePerSecond);
                DebugManager.LogSkill($"독 데미지: {damagePerSecond}");
            }
            lastDamageTime = Time.time;
        }
    }

    public override void OnRemove()
    {
        Debug.Log("독 효과 종료");
    }
}