using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("�߻�ü ����")]
    public float speed = 10f;
    public float damage = 25f; // int���� float���� ���� (ScriptableObject�� ����)
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

    // ������ ���� �Լ� (PlayerAttack���� ���)
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    // ���� �浹�� ó��
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // EnemyAI���� ���� TakeDamage ȣ��
            EnemyAI enemyAI = other.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(damage);
                Debug.Log($"{other.name}���� {damage} ������!");
            }
            else
            {
                Debug.LogWarning($"{other.name}�� EnemyAI ������Ʈ�� �����ϴ�!");
            }

            // �߻�ü �ı�
            Destroy(gameObject);
        }
    }
}