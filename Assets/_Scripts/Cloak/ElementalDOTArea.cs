using UnityEngine;
using System.Collections.Generic;

public class ElementalDOTArea : MonoBehaviour
{
    [Header("DOT 설정")]
    public float damagePerSecond;
    public float radius = 5f;
    public float duration = 5f;

    private ElementType elementType;
    private PassiveEffect passiveEffect;
    private float lastDamageTime;
    private HashSet<EnemyAI> enemiesInArea = new HashSet<EnemyAI>();

    public void Initialize(float dps, ElementType element, PassiveEffect passive, float areaRadius)
    {
        damagePerSecond = dps;
        elementType = element;
        passiveEffect = passive;
        radius = areaRadius;

        // Collider만 크기 조정
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider != null)
        {
            collider.radius = areaRadius;
        }

        // 시각 효과 크기 조정
        Transform visual = transform.GetChild(0);
        if (visual != null)
        {
            visual.localScale = new Vector3(areaRadius * 2, 0.2f, areaRadius * 2);
        }

        Destroy(gameObject, duration);
    }

    void Start()
    {
        // 콜라이더 설정
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
        }
        collider.isTrigger = true;
        collider.radius = radius;
    }

    void Update()
    {
        // 1초마다 데미지
        if (Time.time - lastDamageTime >= 1f)
        {
            ApplyDamageToEnemiesInArea();
            lastDamageTime = Time.time;
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
        foreach (EnemyAI enemy in enemiesInArea)
        {
            if (enemy != null)
            {
                enemy.TakeDamage(damagePerSecond);

                // 패시브 효과 적용
                var effectSystem = enemy.GetComponent<UnifiedPassiveEffect>();
                if (effectSystem == null)
                {
                    effectSystem = enemy.gameObject.AddComponent<UnifiedPassiveEffect>();
                }
                effectSystem.ApplyEffect(passiveEffect.type, passiveEffect, damagePerSecond);
            }
        }
    }

    void UpdateVisualByElement()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color color = Color.white;
            switch (elementType)
            {
                case ElementType.Fire:
                    color = new Color(1f, 0.3f, 0f, 0.5f); // 주황색
                    break;
                case ElementType.Ice:
                    color = new Color(0.3f, 0.7f, 1f, 0.5f); // 하늘색
                    break;
                case ElementType.Poison:
                    color = new Color(0.3f, 1f, 0.3f, 0.5f); // 연두색
                    break;
            }
            renderer.material.color = color;
        }
    }
}