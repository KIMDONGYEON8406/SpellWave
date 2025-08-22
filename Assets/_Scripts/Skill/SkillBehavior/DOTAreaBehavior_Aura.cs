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
        duration = defaultDuration;
        tickInterval = defaultTickInterval;

        GameObject auraArea;

        // SkillPrefab이 없으면 기본 오라 생성
        if (context.SkillPrefab != null)
        {
            auraArea = Object.Instantiate(
                context.SkillPrefab,
                context.Caster.transform.position,
                Quaternion.identity
            );
        }
        else
        {
            // 프리팹 없으면 자동 생성
            auraArea = CreateDefaultAura();
            auraArea.transform.position = context.Caster.transform.position;
        }

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
        // 부모 GameObject (충돌 담당)
        GameObject auraParent = new GameObject("Aura_Area");

        // 시각 효과 자식 (실린더)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.transform.SetParent(auraParent.transform);
        visual.transform.localScale = new Vector3(1, 0.1f, 1);
        visual.transform.localPosition = Vector3.zero;

        // 시각 효과의 Collider 제거
        Destroy(visual.GetComponent<Collider>());

        // 부모에 SphereCollider 추가 (실제 범위)
        SphereCollider collider = auraParent.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 1f; // 기본 반지름

        // 반투명 설정
        var renderer = visual.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            var mat = renderer.material;
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            mat.color = new Color(0.5f, 0f, 1f, 0.3f); // 보라색 반투명
        }

        return auraParent;
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