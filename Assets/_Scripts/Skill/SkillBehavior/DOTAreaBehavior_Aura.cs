using UnityEngine;

[CreateAssetMenu(fileName = "DOT Aura", menuName = "SpellWave/Skills/Behaviors/DOT/Aura")]
public class DOTAreaBehavior_Aura : DOTAreaBehavior
{
    [Header("오라 설정")] // 오라타입 
    public bool followCaster = true;
    public float defaultDuration = 5f;
    public float defaultTickInterval = 0.5f;
    public float visualPulseSpeed = 2f;

    public override void Execute(SkillExecutionContext context)
    {
        // 오라 특성: 플레이어 따라다님, 빠른 틱
        duration = defaultDuration;
        tickInterval = defaultTickInterval;

        GameObject auraArea = Object.Instantiate(
            context.SkillPrefab ?? CreateDefaultAura(),
            context.Caster.transform.position,
            Quaternion.identity
        );

        // 플레이어 자식으로 설정 (따라다니게)
        if (followCaster)
        {
            auraArea.transform.SetParent(context.Caster.transform);
            auraArea.transform.localPosition = Vector3.zero;
        }

        // ElementalDOTArea 설정
        var dotScript = auraArea.GetComponent<ElementalDOTArea>();
        if (dotScript == null)
            dotScript = auraArea.AddComponent<ElementalDOTArea>();

        dotScript.Initialize(
            context.Damage / tickInterval,
            context.Element,
            context.Passive,
            context.Range
        );
        dotScript.duration = duration;

        // 오라 시각 효과 추가
        AddAuraVisualEffect(auraArea, context.Element);
    }

    GameObject CreateDefaultAura()
    {
        GameObject aura = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        aura.transform.localScale = new Vector3(1, 0.1f, 1);

        // 반투명 설정
        var renderer = aura.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            var mat = renderer.material;
            mat.SetFloat("_Mode", 3); // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.3f);
        }

        return aura;
    }

    void AddAuraVisualEffect(GameObject auraObj, ElementType element)
    {
        // 속성별 파티클 효과 추가 (나중에 구현)
        Color elementColor = SkillNameGenerator.GetElementColor(element);

        var renderer = auraObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(
                elementColor.r,
                elementColor.g,
                elementColor.b,
                0.3f
            );
        }
    }

    public override bool RequiresTarget()
    {
        return false; // 오라는 타겟 불필요
    }
}