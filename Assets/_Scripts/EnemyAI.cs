using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("적 설정")]
    public float moveSpeed = 2f;
    public float followRange = 10f;
    public float attackRange = 5f; // 플레이어 공격 범위와 같게

    private Transform player;
    private Rigidbody rb;
    private bool isInAttackRange = false; // 현재 공격 범위 안에 있는지

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // EnemyManager에 자신을 등록
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.RegisterEnemy(this);
        }

        Debug.Log($"적 생성: {name}");
    }

    void Update()
    {
        FollowPlayer();
        CheckAttackRange(); // 공격 범위 체크 추가
    }

    void FollowPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= followRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Y축 이동 제거

            rb.velocity = new Vector3(
                direction.x * moveSpeed,
                rb.velocity.y,
                direction.z * moveSpeed
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

    // 공격 범위 들어갔는지/나갔는지 체크 (최적화 핵심!)
    void CheckAttackRange()
    {
        if (player == null || EnemyManager.instance == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 공격 범위에 들어감
        if (!isInAttackRange && distanceToPlayer <= attackRange)
        {
            isInAttackRange = true;
            EnemyManager.instance.AddToAttackRange(this);
        }
        // 공격 범위에서 나감
        else if (isInAttackRange && distanceToPlayer > attackRange)
        {
            isInAttackRange = false;
            EnemyManager.instance.RemoveFromAttackRange(this);
        }
    }

    // 적이 파괴될 때 EnemyManager에서 제거
    void OnDestroy()
    {
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.UnregisterEnemy(this);
        }
    }
}