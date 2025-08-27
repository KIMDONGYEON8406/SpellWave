using UnityEngine;

public class HomingComponent : MonoBehaviour
{
    [Header("유도 설정")]
    public Transform target;
    public float rotationSpeed = 5f;
    public float maxHomingAngle = 45f;

    [Header("타겟 재탐색")]
    public float searchRadius = 10f;
    public float retargetInterval = 0.5f;  // 0.5초마다만 탐색 (최적화)
    public bool autoRetarget = true;  // 자동 재탐색 on/off

    private Rigidbody rb;
    private float lastRetargetTime;
    private Transform originalCaster;
    private ProjectileOrientation projectileOrientation;
    private Vector3 lastValidDirection;  // 마지막 유효 방향
    private float currentSpeed = 8f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        projectileOrientation = GetComponent<ProjectileOrientation>();

        // 초기 방향 저장
        if (rb != null && rb.velocity != Vector3.zero)
        {
            lastValidDirection = rb.velocity.normalized;
            currentSpeed = rb.velocity.magnitude;
        }
        else
        {
            lastValidDirection = transform.forward;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            originalCaster = player.transform;
            Player character = player.GetComponent<Player>();
            if (character != null)
            {
                searchRadius = character.AttackRange;
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 타겟이 있고 살아있으면
        if (target != null && target.gameObject.activeInHierarchy)
        {
            // 정상 유도
            PerformHoming();
        }
        else
        {
            // 타겟이 없으면
            MaintainLastDirection();

            // 일정 간격으로만 새 타겟 찾기 (최적화)
            if (autoRetarget && Time.time - lastRetargetTime > retargetInterval)
            {
                FindNewTarget();
                lastRetargetTime = Time.time;
            }
        }
    }

    void PerformHoming()
    {
        // 타겟 방향 계산
        Vector3 targetDirection = (target.position - transform.position).normalized;

        // 현재 진행 방향
        Vector3 currentDirection = rb.velocity.normalized;
        if (rb.velocity.magnitude < 0.1f)
        {
            currentDirection = lastValidDirection;
        }

        // 부드럽게 방향 전환
        Vector3 newDirection = Vector3.Slerp(currentDirection, targetDirection, rotationSpeed * Time.fixedDeltaTime);

        // 마지막 유효 방향 업데이트
        lastValidDirection = newDirection;

        // 속도 유지
        if (currentSpeed < 0.1f) currentSpeed = 8f;
        rb.velocity = newDirection * currentSpeed;

        // 미사일 회전
        UpdateRotation(newDirection);
    }

    void MaintainLastDirection()
    {
        // 마지막 방향으로 계속 직진
        rb.velocity = lastValidDirection * currentSpeed;

        // 회전은 유지
        UpdateRotation(lastValidDirection);
    }

    void UpdateRotation(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 세로형 발사체면 추가 회전
        if (projectileOrientation != null && projectileOrientation.isVertical)
        {
            targetRotation = targetRotation * Quaternion.Euler(projectileOrientation.rotationOffset);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime * 2f);
    }

    void FindNewTarget()
    {
        // 성능 최적화: OverlapSphereNonAlloc 사용 가능
        Collider[] enemies = Physics.OverlapSphere(transform.position, searchRadius, LayerMask.GetMask("Enemy"));

        if (enemies.Length == 0) return;  // 적 없으면 바로 리턴

        // 가장 가까운 적 찾기
        Transform closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        if (closestEnemy != null)
        {
            target = closestEnemy;
            DebugManager.LogSkill($"[Projectile] 새 타겟 발견: {target.name}");
        }
    }

    void OnDrawGizmosSelected()
    {
        // 재탐색 범위 표시
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}