using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public float attackRange = 8f; // 원거리라 범위 늘림
    public float attackSpeed = 1f;
    public GameObject magicProjectilePrefab; // 마법 프리팹 참조
    public Transform firePoint; // 발사 위치

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
                // 마법 발사!
                FireMagic();

                Debug.Log($" {currentTarget.name}에게 마법 발사!");
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

        // 발사체에 방향 설정
        MagicBullet projectile = magic.GetComponent<MagicBullet>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue; // 마법이니까 파란색으로
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}