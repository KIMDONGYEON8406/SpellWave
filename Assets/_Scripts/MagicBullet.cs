using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("�߻�ü ����")]
    public float speed = 10f;
    public int damage = 25;
    public float lifeTime = 3f; // 3�� �� �ڵ� �Ҹ�

    private Vector3 direction;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // �߷� ���� �� �ް�
        rb.useGravity = false;

        // ���� �ð� �� �ڵ� �ı�
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        // ������ �������� ���ư���
        rb.velocity = direction * speed;
    }

    // �߻� ���� ���� �Լ�
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    // ���� �浹�� ó��
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // ������ ������ �ֱ�
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($" {other.name}���� {damage} ������!");
            }

            // �߻�ü �ı�
            Destroy(gameObject);
        }
    }
}