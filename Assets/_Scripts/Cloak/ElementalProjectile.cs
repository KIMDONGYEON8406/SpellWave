using UnityEngine;

public class ElementalProjectile : MonoBehaviour
{
    [Header("발사체 설정")]
    public float speed = 10f;
    public float lifetime = 3f;

    private float damage;
    private ElementType elementType;
    private PassiveEffect passiveEffect;
    private Vector3 direction;
    private Rigidbody rb;
    private bool initialized = false;  // 초기화 체크

    public void Initialize(float dmg, ElementType element, PassiveEffect passive, Vector3 dir)
    {
        damage = dmg;
        elementType = element;
        passiveEffect = passive;
        direction = dir.normalized;
        initialized = true;  // 초기화 완료

        UpdateVisualByElement();
        Destroy(gameObject, lifetime);

        //Debug.Log($"발사체 초기화: 데미지={damage}, 방향={direction}, 속도={speed}");

        // Rigidbody 즉시 설정
        SetupRigidbody();
    }

    void SetupRigidbody()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.isKinematic = false;  // 물리 엔진 활성화

        // 즉시 속도 설정
        if (speed > 0 && direction != Vector3.zero)
        {
            rb.velocity = direction * speed;
            //Debug.Log($"초기 속도 설정: {rb.velocity}");
        }
    }

    void Start()
    {
        // Start에서도 한 번 더 체크
        if (!initialized)
        {
            Debug.LogWarning("ElementalProjectile이 Initialize 없이 시작됨!");
            return;
        }

        if (rb == null)
        {
            SetupRigidbody();
        }
    }

    void FixedUpdate()
    {
        // 계속 속도 유지
        if (rb != null && initialized && speed > 0)
        {
            rb.velocity = direction * speed;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"충돌: {other.name}, Tag: {other.tag}, Layer: {other.gameObject.layer}");

        if (other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                ApplyPassiveToEnemy(enemy);

                Debug.Log($"적 타격! {other.name}에게 {damage} 데미지!");
            }
            else
            {
                Debug.LogError($"{other.name}에 EnemyAI 없음!");
            }

            // 관통 체크
            var pierce = GetComponent<PierceComponent>();
            if (pierce == null || !pierce.CanPierce())
            {
                Debug.Log("발사체 파괴");
                Destroy(gameObject);
            }
        }
    }

    void ApplyPassiveToEnemy(EnemyAI enemy)
    {
        if (passiveEffect == null || passiveEffect.type == PassiveType.None) return;

        var effectSystem = enemy.GetComponent<UnifiedPassiveEffect>();
        if (effectSystem == null)
        {
            effectSystem = enemy.gameObject.AddComponent<UnifiedPassiveEffect>();
        }

        effectSystem.ApplyEffect(passiveEffect.type, passiveEffect, damage);

        if (passiveEffect.type == PassiveType.Chain)
        {
            ChainAttack(enemy.transform.position);
        }
    }

    void ChainAttack(Vector3 origin)
    {
        if (passiveEffect.chainCount <= 0) return;

        Collider[] nearbyEnemies = Physics.OverlapSphere(origin, passiveEffect.chainRange, LayerMask.GetMask("Enemy"));
        int chainedCount = 0;

        foreach (Collider col in nearbyEnemies)
        {
            if (chainedCount >= passiveEffect.chainCount) break;

            EnemyAI enemy = col.GetComponent<EnemyAI>();
            if (enemy != null && col.transform.position != origin)
            {
                float chainDamage = damage * passiveEffect.chainDamageRatio;
                enemy.TakeDamage(chainDamage);
                chainedCount++;

                Debug.Log($"연쇄 공격! {chainDamage} 데미지");
            }
        }
    }

    void UpdateVisualByElement()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color elementColor = SkillNameGenerator.GetElementColor(elementType);
            renderer.material.color = elementColor;
        }
    }
}