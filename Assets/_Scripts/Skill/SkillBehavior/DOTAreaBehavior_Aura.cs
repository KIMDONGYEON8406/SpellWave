using UnityEngine;

[CreateAssetMenu(fileName = "DOT Aura", menuName = "SpellWave/Skills/Behaviors/DOT/Aura")]
public class DOTAreaBehavior_Aura : SkillBehavior
{
    [Header("오라 설정")]
    public bool followCaster = true;
    public float defaultTickInterval = 0.5f;
    public bool isPermanent = true;

    public override void Execute(SkillExecutionContext context)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            DebugManager.LogSkill("[Aura] Player not found!");
            return;
        }

        Transform existingAura = player.transform.Find("PermanentAura");
        if (existingAura != null)
        {
            DebugManager.LogError(LogCategory.Skill, "오라가 이미 있습니다!");
            return;
        }

        GameObject auraArea = null;

        // 오라는 풀링 사용 안 함! 직접 생성!
        if (context.SkillPrefab != null)
        {
            auraArea = Object.Instantiate(context.SkillPrefab);
        }
        else
        {
            auraArea = CreateDefaultAura();
        }

        // 즉시 플레이어 자식으로
        auraArea.transform.SetParent(player.transform, false);
        auraArea.transform.localPosition = Vector3.zero;
        auraArea.transform.localRotation = Quaternion.identity;
        auraArea.name = "PermanentAura";

        var dotScript = auraArea.GetComponent<ElementalDOTArea>();
        if (dotScript == null)
        {
            dotScript = auraArea.AddComponent<ElementalDOTArea>();
        }

        SkillInstance auraSkill = null;
        var skillManager = player.GetComponent<SkillManager>();
        if (skillManager != null)
        {
            auraSkill = skillManager.GetSkill("Aura");
        }

        dotScript.InitializePermanent(
            context.Damage * defaultTickInterval,
            context.Element,
            context.Passive,
            context.Range,
            defaultTickInterval,
            isPermanent,
            auraSkill
        );

        DebugManager.LogSkill($"[오라 생성 완료] 플레이어 자식으로 직접 생성");
    }

    GameObject CreateDefaultAura()
    {
        GameObject auraParent = new GameObject("Aura_Default");

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.transform.SetParent(auraParent.transform);
        visual.transform.localScale = new Vector3(6, 0.1f, 6);
        visual.transform.localPosition = new Vector3(0, 0.1f, 0);

        Destroy(visual.GetComponent<Collider>());

        SphereCollider collider = auraParent.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 3f;

        var renderer = visual.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Color elementColor = SkillNameGenerator.GetElementColor(
                CloakManager.Instance?.GetCurrentElement() ?? ElementType.Energy
            );
            renderer.material.color = elementColor;
        }

        return auraParent;
    }

    public override bool RequiresTarget()
    {
        return false;
    }
}