using UnityEngine;

/*
[목적]
- 타겟 탐색/사거리 체크 없음.
- 플레이어가 바라보는 "정면 방향(transform.forward)"으로 주기적으로 발사.
- 공격 속도/데미지는 PlayerStats(SO)에서 읽어옴.

[인스펙터]
- playerStats           : 플레이어 스탯 SO (공격속도/데미지 사용)
- projectilePrefab      : 발사체 프리팹(MagicBullet 등)
- firePoint             : 발사 위치(없으면 플레이어 transform 사용)
- autoFire              : 자동 발사 ON/OFF
*/
public class PlayerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private PlayerStats playerStats;  // ScriptableObject 참조
    [SerializeField] private GameObject projectilePrefab; // 발사체 프리팹
    [SerializeField] private Transform firePoint;      // 발사 위치(없으면 transform)
    [SerializeField] private bool autoFire = true;     // 자동 발사 여부

    // 런타임 스탯 캐시
    private float currentAttackSpeed;  // 발사 횟수(회/초)
    private float currentAttackDamage; // 데미지

    // 타이밍/애니메이션
    private float lastAttackTime;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (firePoint == null) firePoint = transform;

        InitializeStats();
    }

    void InitializeStats()
    {
        if (playerStats != null)
        {
            currentAttackSpeed = playerStats.attackSpeed;
            currentAttackDamage = playerStats.attackDamage;
        }
        else
        {
            Debug.LogError("PlayerStats가 할당되지 않았습니다!");
            currentAttackSpeed = 1f;
            currentAttackDamage = 10f;
        }
    }

    void Update()
    {
        // 매 프레임 스탯 갱신 (레벨업/스킬 등 반영)
        if (playerStats != null)
        {
            currentAttackSpeed = playerStats.attackSpeed;
            currentAttackDamage = playerStats.attackDamage;
        }

        if (!autoFire) return;

        if (Time.time >= lastAttackTime + (1f / Mathf.Max(0.01f, currentAttackSpeed)))
        {
            FireForward();
            animator?.SetTrigger("Attack");
            lastAttackTime = Time.time;
        }
    }

    /// <summary>플레이어 정면(transform.forward)으로 발사</summary>
    public void FireForward()
    {
        if (projectilePrefab == null || firePoint == null) return;

        Vector3 dir = transform.forward;

        // 발사체 생성 + 회전 정렬
        Quaternion rot = dir.sqrMagnitude > 0f ? Quaternion.LookRotation(dir) : Quaternion.identity;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, rot);

        // 발사체에 방향/데미지 전달
        var bullet = proj.GetComponent<MagicBullet>();
        if (bullet != null)
        {
            bullet.SetDirection(dir.normalized);
            bullet.SetDamage(currentAttackDamage);
        }
    }
}
