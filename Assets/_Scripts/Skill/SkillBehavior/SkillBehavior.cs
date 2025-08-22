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
    public GameObject Caster;
    public Transform Target;
    public float Damage;
    public float Range;
    public ElementType Element;
    public PassiveEffect Passive;
    public GameObject SkillPrefab;
    public GameObject HitEffectPrefab;
}