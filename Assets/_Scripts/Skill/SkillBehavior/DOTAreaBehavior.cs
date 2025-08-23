using UnityEngine;

[CreateAssetMenu(fileName = "DOT Area", menuName = "SpellWave/Skills/Behaviors/DOT/Base")]
public class DOTAreaBehavior : SkillBehavior
{
    [Header("지속 영역 설정")]
    public float duration = 5f;
    public float tickInterval = 1f;

    public override void Execute(SkillExecutionContext context)
    {
        GameObject dotArea = Object.Instantiate(
            context.SkillPrefab,
            context.Caster.transform.position,
            Quaternion.identity
        );

        var dotScript = dotArea.GetComponent<ElementalDOTArea>();
        if (dotScript == null)
        {
            dotScript = dotArea.AddComponent<ElementalDOTArea>();
        }

        dotScript.Initialize(
            context.Damage / tickInterval,
            context.Element,
            context.Passive,
            context.Range
        );
        dotScript.duration = duration;
    }

    public override bool CanExecute(SkillExecutionContext context)
    {
        return true;
    }
}