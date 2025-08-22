using UnityEngine;

public class ElementalProjectile : MonoBehaviour
{
    [Header("발사체 설정")]
    public float speed = 10f;
    public float lifetime = 3f;

    // 발사체 정보
    private float damage;
    private ElementType elementType;
    private PassiveEffect passiveEffect;
    private Vector3 direction;

    private Rigidbody rb;

    // 초기화
    public void Initialize(float dmg, ElementType element, PassiveEffect passive, Vector3 dir)
    {
        damage = dmg;
        elementType = element;
        passiveEffect = passive;
        direction = dir.normalized;

        // 속성에 따른 이펙트 색상 변경
        UpdateVisualByElement();

        // 일정 시간 후 자동 파괴
        Destroy(gameObject, lifetime);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        // 발사체 이동
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 적과 충돌
        if (other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                // 데미지 적용
                enemy.TakeDamage(damage);

                // 패시브 효과 적용
                ApplyPassiveToEnemy(enemy);

                Debug.Log($"{elementType} 발사체가 {other.name}에게 {damage} 데미지!");
            }

            // 발사체 파괴 (관통이 아닌 경우)
            var pierce = GetComponent<PierceComponent>();
            if (pierce == null || !pierce.CanPierce())
            {
                Destroy(gameObject);
            }
        }
    }

    // 패시브 효과 적용
    void ApplyPassiveToEnemy(EnemyAI enemy)
    {
        // 간단한 패시브 적용 (나중에 UnifiedPassiveEffect로 교체)
        switch (passiveEffect.type)
        {
            case PassiveType.Burn:
                Debug.Log($"화상 효과 적용: {passiveEffect.effectValue}/초");
                break;

            case PassiveType.Slow:
                Debug.Log($"둔화 효과 적용: {passiveEffect.effectValue}%");
                break;

            case PassiveType.Chain:
                ChainAttack(enemy.transform.position);
                break;
        }
    }

    // 연쇄 공격
    void ChainAttack(Vector3 origin)
    {
        if (passiveEffect.chainCount <= 0) return;

        Collider[] nearbyEnemies = Physics.OverlapSphere(origin, 5f, LayerMask.GetMask("Enemy"));
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

    // 속성별 시각 효과
    void UpdateVisualByElement()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            switch (elementType)
            {
                case ElementType.Fire:
                    renderer.material.color = Color.red;
                    break;
                case ElementType.Ice:
                    renderer.material.color = Color.cyan;
                    break;
                case ElementType.Lightning:
                    renderer.material.color = Color.yellow;
                    break;
                case ElementType.Poison:
                    renderer.material.color = Color.green;
                    break;
                case ElementType.Dark:
                    renderer.material.color = Color.black;
                    break;
                case ElementType.Light:
                    renderer.material.color = Color.white;
                    break;
                default:
                    renderer.material.color = new Color(0.5f, 0f, 1f); // 보라색 (에너지)
                    break;
            }
        }
    }
}