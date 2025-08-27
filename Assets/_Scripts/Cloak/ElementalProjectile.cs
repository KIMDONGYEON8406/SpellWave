using UnityEngine;

public class ElementalProjectile : MonoBehaviour
{
    [Header("발사체 설정")]
    public float speed = 10f;
    public float lifetime = 3f;
    public bool isHoming = false;  // 유도 여부 추가

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
        initialized = true;

        // HomingComponent 체크
        isHoming = GetComponent<HomingComponent>() != null;

        UpdateVisualByElement();
        Destroy(gameObject, lifetime);

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
            DebugManager.LogCombat($"초기 속도 설정: {rb.velocity}");
        }
    }

    void Start()
    {
        // Start에서도 한 번 더 체크
        if (!initialized)
        {
            DebugManager.LogCombat("ElementalProjectile이 Initialize 없이 시작됨!");
            return;
        }

        if (rb == null)
        {
            SetupRigidbody();
        }
    }

    void FixedUpdate()
    {
        // 유도 미사일이 아닌 경우만 속도 제어
        if (!isHoming && rb != null && initialized && speed > 0)
        {
            rb.velocity = direction * speed;

            // 직진 발사체도 회전 처리 추가!
            var projectileOrientation = GetComponent<ProjectileOrientation>();
            if (projectileOrientation != null && projectileOrientation.isVertical)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                targetRotation = targetRotation * Quaternion.Euler(projectileOrientation.rotationOffset);
                transform.rotation = targetRotation;
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        DebugManager.LogCombat($"충돌: {other.name}, Tag: {other.tag}, Layer: {other.gameObject.layer}");

        if (other.CompareTag("Enemy"))
        {
            // 관통 컴포넌트 확인
            var pierce = GetComponent<PierceComponent>();

            // 이미 관통한 적인지 확인
            if (pierce != null && pierce.HasPierced(other.gameObject))
            {
                DebugManager.LogCombat($"이미 관통한 적: {other.name} - 스킵");
                return;  // 이미 데미지 준 적은 무시
            }

            // 데미지 처리
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                ApplyPassiveToEnemy(enemy);
                DebugManager.LogCombat($"적 타격! {other.name}에게 {damage} 데미지!");
            }

            // 관통 처리 부분
            if (pierce != null)
            {
                pierce.OnPierce(other.gameObject);

                if (pierce.CanPierce())
                {
                    DebugManager.LogCombat($"관통! 남은 횟수: {pierce.RemainingPierceCount}");  // 프로퍼티 사용
                    return;
                }
            }

            // 관통 불가능하면 파괴
            DebugManager.LogCombat("발사체 파괴");
            Destroy(gameObject);
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

                DebugManager.LogCombat($"연쇄 공격! {chainDamage} 데미지");
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