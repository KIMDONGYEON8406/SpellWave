using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private PlayerStats playerStats; // ScriptableObject 참조
    public GameObject magicProjectilePrefab; // 마법 프리팹 참조
    public Transform firePoint; // 발사 위치

    // 런타임 변수들
    private float currentAttackRange;
    private float currentAttackSpeed;
    private float currentAttackDamage;

    private float lastAttackTime;
    private float lastTargetCheck;
    private Animator animator;
    private EnemyAI currentTarget;

    void Start()
    {
        animator = GetComponent<Animator>();

        // firePoint가 없으면 플레이어 위치에서 발사
        if (firePoint == null)
        {
            firePoint = transform;
        }

        // ScriptableObject에서 값들을 복사
        InitializeStats();
    }

    void InitializeStats()
    {
        if (playerStats != null)
        {
            currentAttackRange = playerStats.attackRange;
            currentAttackSpeed = playerStats.attackSpeed;
            currentAttackDamage = playerStats.attackDamage;
        }
        else
        {
            Debug.LogError("PlayerStats가 할당되지 않았습니다!");
        }
    }

    void Update()
    {
        // 런타임에 스탯이 변경될 수 있으므로 업데이트
        UpdateStatsFromSO();

        if (Time.time >= lastTargetCheck + 0.1f)
        {
            FindTarget();
            lastTargetCheck = Time.time;
        }

        if (Time.time >= lastAttackTime + (1f / currentAttackSpeed))
        {
            TryAttack();
        }
    }

    void UpdateStatsFromSO()
    {
        if (playerStats != null)
        {
            currentAttackRange = playerStats.attackRange;
            currentAttackSpeed = playerStats.attackSpeed;
            currentAttackDamage = playerStats.attackDamage;
        }
    }

    void FindTarget()
    {
        if (EnemyManager.instance != null)
        {
            currentTarget = EnemyManager.instance.GetNearestEnemy(transform.position);
        }
    }

    void TryAttack()
    {
        if (currentTarget != null && magicProjectilePrefab != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= currentAttackRange)
            {
                // 마법 발사!
                FireMagic();
                Debug.Log($"{currentTarget.name}에게 마법 발사! 데미지: {currentAttackDamage}");
                animator.SetTrigger("Attack");
                lastAttackTime = Time.time;
            }
            else
            {
                currentTarget = null;
            }
        }
    }

    void FireMagic()
    {
        // 마법 발사체 생성
        GameObject magic = Instantiate(magicProjectilePrefab, firePoint.position, Quaternion.identity);

        // 타겟 방향 계산
        Vector3 direction = (currentTarget.transform.position - firePoint.position).normalized;

        // 발사체에 방향과 데미지 설정
        MagicBullet projectile = magic.GetComponent<MagicBullet>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.SetDamage(currentAttackDamage); // 데미지도 설정
        }
    }

    void OnDrawGizmosSelected()
    {
        // 런타임에는 현재 공격 범위로, 에디터에서는 SO의 값으로 표시
        float displayRange = Application.isPlaying ? currentAttackRange :
                           (playerStats != null ? playerStats.attackRange : 8f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, displayRange);
    }
}