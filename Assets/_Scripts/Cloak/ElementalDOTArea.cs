using UnityEngine;
using System.Collections.Generic;

public class ElementalDOTArea : MonoBehaviour
{
    [Header("DOT Settings")]
    public float damagePerSecond;
    public float radius = 5f;  // 이것만 설정! 나머지는 자동
    public float duration = 5f;

    [Header("Dynamic Scaling")]
    private SkillInstance parentSkill;
    private float baseRadius;
    private ParticleSystem freezeCircleParticle;
    private Transform freezeCircleTransform;
    private SphereCollider sphereCollider;
    private float currentMultiplier = 1f;

    private ElementType elementType;
    private PassiveEffect passiveEffect;
    private float lastDamageTime;
    private HashSet<EnemyAI> enemiesInArea = new HashSet<EnemyAI>();

    public void InitializePermanent(float dps, ElementType element, PassiveEffect passive,
        float areaRadius, float tickInterval, bool permanent = false, SkillInstance skill = null)
    {
        damagePerSecond = dps;
        elementType = element;
        passiveEffect = passive;
        parentSkill = skill;

        // SO 값 우선
        if (areaRadius > 0)
        {
            radius = areaRadius;
        }

        baseRadius = radius;

        DebugManager.LogSkill($"[Aura Init] Final Radius: {radius}");

        FindComponents();
        SetInitialSize();  // 초기 크기 설정
        UpdateVisualByElement();

        if (!permanent)
        {
            Destroy(gameObject, duration);
        }
    }

    void FindComponents()
    {
        if (sphereCollider == null)
        {
            sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider == null)
            {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
            }
        }

        if (freezeCircleTransform == null)
        {
            freezeCircleTransform = transform.Find("Freeze circle");
            if (freezeCircleTransform != null)
            {
                freezeCircleParticle = freezeCircleTransform.GetComponent<ParticleSystem>();
            }
        }
    }

    void SetInitialSize()
    {
        // 1. 콜라이더: radius의 절반
        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
            sphereCollider.radius = radius / 2f;
            DebugManager.LogSkill($"[Size] Collider Radius: {sphereCollider.radius}");
        }

        // 2. 파티클: 프리팹이 Start Size 2로 설정되어 있다고 가정
        if (freezeCircleTransform != null)
        {
            // Start Size가 2라고 가정하고 계산
            float assumedStartSize = 2f;  // 프리팹의 Start Size
            float targetScale = radius / assumedStartSize;

            freezeCircleTransform.localScale = Vector3.one * targetScale;
            freezeCircleTransform.localPosition = Vector3.zero;

            DebugManager.LogSkill($"[Size] Assumed StartSize: {assumedStartSize}, Target Scale: {targetScale}");
        }
    }

    void Start()
    {
        FindComponents();

        if (baseRadius <= 0)
        {
            baseRadius = radius;
        }

        SetInitialSize();  // Start에서도 한번 더

        if (transform.parent != null && transform.parent.CompareTag("Player"))
        {
            transform.localPosition = Vector3.zero;

            if (freezeCircleTransform != null)
            {
                freezeCircleTransform.localPosition = Vector3.zero;
            }
        }
    }

    void Update()
    {
        if (parentSkill != null)
        {
            UpdateRangeFromSkill();
        }

        if (Time.time - lastDamageTime >= 1f)
        {
            ApplyDamageToEnemiesInArea();
            lastDamageTime = Time.time;
        }
    }
    public void Initialize(float dps, ElementType element, PassiveEffect passive, float areaRadius)
    {
        damagePerSecond = dps;
        elementType = element;
        passiveEffect = passive;

        if (areaRadius > 0)
        {
            radius = areaRadius;
        }

        baseRadius = radius;

        DebugManager.LogSkill($"[DOT Init] Radius: {radius}");

        FindComponents();
        SetInitialSize();
        UpdateVisualByElement();

        if (duration > 0)
        {
            Destroy(gameObject, duration);
        }
    }
    void UpdateRangeFromSkill()
    {
        if (parentSkill == null) return;

        float newMultiplier = parentSkill.rangeMultiplier;

        if (Mathf.Abs(newMultiplier - currentMultiplier) > 0.01f)
        {
            currentMultiplier = newMultiplier;
            radius = baseRadius * currentMultiplier;

            DebugManager.LogSkill($"[Range Update] New Radius: {radius:F1}");

            UpdateVisualScale();
        }
    }

    void UpdateVisualScale()
    {
        if (sphereCollider != null)
        {
            sphereCollider.radius = radius / 2f;
        }

        if (freezeCircleTransform != null)
        {
            // Start Size가 2라고 가정
            float assumedStartSize = 2f;
            float targetScale = radius / assumedStartSize;

            freezeCircleTransform.localScale = Vector3.one * targetScale;
        }

        DebugManager.LogSkill($"[Scale Update] Radius: {radius}, Collider: {sphereCollider?.radius}, Particle Scale: {freezeCircleTransform?.localScale.x}");
    }

    void UpdateVisualByElement()
    {
        if (freezeCircleParticle != null)
        {
            var main = freezeCircleParticle.main;
            Color elementColor = SkillNameGenerator.GetElementColor(elementType);
            main.startColor = new Color(elementColor.r, elementColor.g, elementColor.b, 0.5f);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemiesInArea.Add(enemy);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemiesInArea.Remove(enemy);
            }
        }
    }

    void ApplyDamageToEnemiesInArea()
    {
        var enemiesCopy = new List<EnemyAI>(enemiesInArea);

        foreach (EnemyAI enemy in enemiesCopy)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.TakeDamage(damagePerSecond);

                var effectSystem = enemy.GetComponent<UnifiedPassiveEffect>();
                if (effectSystem == null)
                {
                    effectSystem = enemy.gameObject.AddComponent<UnifiedPassiveEffect>();
                }
                effectSystem.ApplyEffect(passiveEffect.type, passiveEffect, damagePerSecond);
            }
            else
            {
                enemiesInArea.Remove(enemy);
            }
        }
    }
}