using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("적 데이터")]
    [SerializeField] private EnemyStats enemyStats; // ScriptableObject 참조

    // 런타임 스탯 (ScriptableObject에서 복사해서 사용)
    private EnemyRuntimeStats runtimeStats;

    // 기존 변수들
    private Transform player;
    private Rigidbody rb;
    private bool isInAttackRange = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // ScriptableObject에서 런타임 스탯 생성
        InitializeStats();

        // EnemyManager에 자신을 등록
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.RegisterEnemy(this);
        }

        Debug.Log($"적 생성: {name} - HP: {runtimeStats.maxHP}");
    }

    void InitializeStats()
    {
        if (enemyStats != null)
        {
            // 현재 웨이브 정보는 나중에 WaveManager에서 받아올 예정
            // 일단 웨이브 1로 설정
            int currentWave = 1;
            runtimeStats = enemyStats.CreateRuntimeStats(currentWave);
        }
        else
        {
            Debug.LogError("EnemyStats가 할당되지 않았습니다!");
            // 기본값으로 설정
            runtimeStats = new EnemyRuntimeStats
            {
                maxHP = 100,
                currentHP = 100,
                moveSpeed = 2f,
                followRange = 10f,
                attackRange = 5f,
                attackInterval = 1f,
                contactDamage = 10f
            };
        }
    }

    void Update()
    {
        FollowPlayer();
        CheckAttackRange();
    }

    void FollowPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= runtimeStats.followRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Y축 이동 제거

            rb.velocity = new Vector3(
                direction.x * runtimeStats.moveSpeed,
                rb.velocity.y,
                direction.z * runtimeStats.moveSpeed
            );

            // 플레이어 방향 바라보기
            if (direction.magnitude > 0.1f)
            {
                transform.LookAt(player.position);
            }
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    void CheckAttackRange()
    {
        if (player == null || EnemyManager.instance == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 공격 범위에 들어감
        if (!isInAttackRange && distanceToPlayer <= runtimeStats.attackRange)
        {
            isInAttackRange = true;
            EnemyManager.instance.AddToAttackRange(this);
        }
        // 공격 범위에서 나감
        else if (isInAttackRange && distanceToPlayer > runtimeStats.attackRange)
        {
            isInAttackRange = false;
            EnemyManager.instance.RemoveFromAttackRange(this);
        }
    }

    // 데미지 받기 (EnemyHealth 기능을 통합)
    public void TakeDamage(float damage)
    {
        runtimeStats.TakeDamage(damage);
        Debug.Log($"{name} 데미지 {damage} 받음! 남은 체력: {runtimeStats.currentHP}");

        if (runtimeStats.IsDead())
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{name} 사망!");

        // [추가] 경험치 지급
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddExperience(25); // 몬스터당 25 경험치
        }

        Destroy(gameObject);
    }

    // 적이 파괴될 때 EnemyManager에서 제거
    void OnDestroy()
    {
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.UnregisterEnemy(this);
        }
    }

    // 외부에서 런타임 스탯에 접근할 수 있는 함수
    public EnemyRuntimeStats GetRuntimeStats()
    {
        return runtimeStats;
    }

    // 웨이브 매니저에서 웨이브에 맞는 스탯으로 업데이트
    public void UpdateStatsForWave(int waveIndex)
    {
        if (enemyStats != null)
        {
            runtimeStats = enemyStats.CreateRuntimeStats(waveIndex);
            Debug.Log($"{name} 웨이브 {waveIndex} 스탯 적용 - HP: {runtimeStats.maxHP}");
        }
    }
}