using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] private PlayerStats playerStats; // ScriptableObject ����
    public GameObject magicProjectilePrefab; // ���� ������ ����
    public Transform firePoint; // �߻� ��ġ

    // ��Ÿ�� ������
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

        // firePoint�� ������ �÷��̾� ��ġ���� �߻�
        if (firePoint == null)
        {
            firePoint = transform;
        }

        // ScriptableObject���� ������ ����
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
            Debug.LogError("PlayerStats�� �Ҵ���� �ʾҽ��ϴ�!");
        }
    }

    void Update()
    {
        // ��Ÿ�ӿ� ������ ����� �� �����Ƿ� ������Ʈ
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
                // ���� �߻�!
                FireMagic();
                Debug.Log($"{currentTarget.name}���� ���� �߻�! ������: {currentAttackDamage}");
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

        // �߻�ü�� ����� ������ ����
        MagicBullet projectile = magic.GetComponent<MagicBullet>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.SetDamage(currentAttackDamage); // �������� ����
        }
    }

    void OnDrawGizmosSelected()
    {
        // ��Ÿ�ӿ��� ���� ���� ������, �����Ϳ����� SO�� ������ ǥ��
        float displayRange = Application.isPlaying ? currentAttackRange :
                           (playerStats != null ? playerStats.attackRange : 8f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, displayRange);
    }
}