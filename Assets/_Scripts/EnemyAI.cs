using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("�� ����")]
    public float moveSpeed = 2f;
    public float followRange = 10f;

    private Transform player;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        Debug.Log("�� ���� �Ϸ�!");
    }

    void Update()
    {
        FollowPlayer();
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
}