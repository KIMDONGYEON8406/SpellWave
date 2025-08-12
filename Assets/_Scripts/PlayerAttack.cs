using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("���� ����")]
    public float attackRange = 8f; // ���Ÿ��� ���� �ø�
    public float attackSpeed = 1f;
    public GameObject magicProjectilePrefab; // ���� ������ ����
    public Transform firePoint; // �߻� ��ġ

    private float lastAttackTime;
    private float lastTargetCheck;
    private Animator animator;
    private EnemyAI currentTarget;

    void Start()
    {
        animator = GetComponent<Animator>();

        // firePoint�� ������ �÷��̾� ��ġ���� �߻�
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void Update()
    {
        if (Time.time >= lastTargetCheck + 0.1f)
        {
            FindTarget();
            lastTargetCheck = Time.time;
        }

        if (Time.time >= lastAttackTime + (1f / attackSpeed))
        {
            TryAttack();
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

            if (distance <= attackRange)
            {
                // ���� �߻�!
                FireMagic();

                Debug.Log($" {currentTarget.name}���� ���� �߻�!");
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
        // ���� �߻�ü ����
        GameObject magic = Instantiate(magicProjectilePrefab, firePoint.position, Quaternion.identity);

        // Ÿ�� ���� ���
        Vector3 direction = (currentTarget.transform.position - firePoint.position).normalized;

        // �߻�ü�� ���� ����
        MagicBullet projectile = magic.GetComponent<MagicBullet>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue; // �����̴ϱ� �Ķ�������
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}