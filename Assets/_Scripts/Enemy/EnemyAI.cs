using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("�� ������")]
    [SerializeField] private EnemyStats enemyStats; // ScriptableObject ����

    // ��Ÿ�� ���� (ScriptableObject���� �����ؼ� ���)
    private EnemyRuntimeStats runtimeStats;

    // ���� ������
    private Transform player;
    private Rigidbody rb;
    private bool isInAttackRange = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // ScriptableObject���� ��Ÿ�� ���� ����
        InitializeStats();

        // EnemyManager�� �ڽ��� ���
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.RegisterEnemy(this);
        }

        Debug.Log($"�� ����: {name} - HP: {runtimeStats.maxHP}");
    }

    void InitializeStats()
    {
        if (enemyStats != null)
        {
            // ���� ���̺� ������ ���߿� WaveManager���� �޾ƿ� ����
            // �ϴ� ���̺� 1�� ����
            int currentWave = 1;
            runtimeStats = enemyStats.CreateRuntimeStats(currentWave);
        }
        else
        {
            Debug.LogError("EnemyStats�� �Ҵ���� �ʾҽ��ϴ�!");
            // �⺻������ ����
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
            direction.y = 0; // Y�� �̵� ����

            rb.velocity = new Vector3(
                direction.x * runtimeStats.moveSpeed,
                rb.velocity.y,
                direction.z * runtimeStats.moveSpeed
            );

            // �÷��̾� ���� �ٶ󺸱�
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

        // ���� ������ ��
        if (!isInAttackRange && distanceToPlayer <= runtimeStats.attackRange)
        {
            isInAttackRange = true;
            EnemyManager.instance.AddToAttackRange(this);
        }
        // ���� �������� ����
        else if (isInAttackRange && distanceToPlayer > runtimeStats.attackRange)
        {
            isInAttackRange = false;
            EnemyManager.instance.RemoveFromAttackRange(this);
        }
    }

    // ������ �ޱ� (EnemyHealth ����� ����)
    public void TakeDamage(float damage)
    {
        runtimeStats.TakeDamage(damage);
        Debug.Log($"{name} ������ {damage} ����! ���� ü��: {runtimeStats.currentHP}");

        if (runtimeStats.IsDead())
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{name} ���!");
        Destroy(gameObject);
    }

    // ���� �ı��� �� EnemyManager���� ����
    void OnDestroy()
    {
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.UnregisterEnemy(this);
        }
    }

    // �ܺο��� ��Ÿ�� ���ȿ� ������ �� �ִ� �Լ�
    public EnemyRuntimeStats GetRuntimeStats()
    {
        return runtimeStats;
    }

    // ���̺� �Ŵ������� ���̺꿡 �´� �������� ������Ʈ
    public void UpdateStatsForWave(int waveIndex)
    {
        if (enemyStats != null)
        {
            runtimeStats = enemyStats.CreateRuntimeStats(waveIndex);
            Debug.Log($"{name} ���̺� {waveIndex} ���� ���� - HP: {runtimeStats.maxHP}");
        }
    }
}