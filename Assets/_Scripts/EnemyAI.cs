using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("�� ����")]
    public float moveSpeed = 2f;
    public float followRange = 10f;
    public float attackRange = 5f; // �÷��̾� ���� ������ ����

    private Transform player;
    private Rigidbody rb;
    private bool isInAttackRange = false; // ���� ���� ���� �ȿ� �ִ���

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // EnemyManager�� �ڽ��� ���
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.RegisterEnemy(this);
        }

        Debug.Log($"�� ����: {name}");
    }

    void Update()
    {
        FollowPlayer();
        CheckAttackRange(); // ���� ���� üũ �߰�
    }

    void FollowPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= followRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Y�� �̵� ����

            rb.velocity = new Vector3(
                direction.x * moveSpeed,
                rb.velocity.y,
                direction.z * moveSpeed
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

    // ���� ���� ������/�������� üũ (����ȭ �ٽ�!)
    void CheckAttackRange()
    {
        if (player == null || EnemyManager.instance == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ���� ������ ��
        if (!isInAttackRange && distanceToPlayer <= attackRange)
        {
            isInAttackRange = true;
            EnemyManager.instance.AddToAttackRange(this);
        }
        // ���� �������� ����
        else if (isInAttackRange && distanceToPlayer > attackRange)
        {
            isInAttackRange = false;
            EnemyManager.instance.RemoveFromAttackRange(this);
        }
    }

    // ���� �ı��� �� EnemyManager���� ����
    void OnDestroy()
    {
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.UnregisterEnemy(this);
        }
    }
}