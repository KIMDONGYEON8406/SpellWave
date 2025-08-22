using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Basic", menuName = "SpellWave/Skills/Behaviors/Projectile/Basic")]
public class ProjectileBehavior_Basic : ProjectileBehavior
{
    [Header("기본 발사체 설정")] // 기본 발사체 동작 볼타입
    public float defaultSpeed = 10f;
    public float defaultLifetime = 3f;

    public override void Execute(SkillExecutionContext context)
    {
        // 볼 특성: 단일 타겟, 직선 발사
        projectileSpeed = defaultSpeed;
        isHoming = false;
        pierceCount = 0;

        base.Execute(context);
    }
}