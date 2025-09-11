using UnityEngine;

public abstract class SkillBehavior : ScriptableObject
{
    [Header("기본 설정")]
    public string behaviorName = "기본 동작";
    [TextArea(2, 3)]
    public string description = "스킬 동작 설명";

    public abstract void Execute(SkillExecutionContext context);

    public virtual bool CanExecute(SkillExecutionContext context)
    {
        return context.Target != null || !RequiresTarget();
    }

    public virtual bool RequiresTarget()
    {
        return true;
    }
}

[System.Serializable]
public class SkillExecutionContext
{
    public string SkillName;
    public GameObject Caster;
    public Transform Target;
    public float Damage;
    public float Range;
    public ElementType Element;
    public PassiveEffect Passive;

    // 이펙트 프리팹들
    public GameObject SkillPrefab;
    public GameObject HitEffectPrefab;
    public GameObject MuzzleEffectPrefab;    // 추가!
    public GameObject CastEffectPrefab;      // 추가!
    public float MuzzleEffectDuration = 1f;  // 추가!
    public float HitEffectDuration = 2f;     // 추가!

    // 발사체/영역 개수 (기본값)
    public int BaseProjectileCount = 1;

    // 다중시전 확률 (0~100)
    public float MultiCastChance = 0f;

    // 다중시전으로 생성된 것인지 (무한루프 방지)
    public bool IsMultiCastInstance = false;

    // 다중시전 정보
    public int MultiCastIndex = 0;      // 몇 번째 시전인지
    public int TotalMultiCasts = 1;     // 총 시전 횟수

    // 위치 오프셋 (영역 다중시전용)
    public Vector3 PositionOffset = Vector3.zero;
}