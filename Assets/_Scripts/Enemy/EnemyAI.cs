using UnityEngine;

/*
[목적]
- 스폰 즉시 무조건 플레이어를 따라감(감지 범위/공격 범위 진입/이탈 로직 없음).
- 풀링(비활성↔재활성)과 EnemyManager 등록/해제 타이밍에 맞춤.
- HP/스탯은 EnemyStats(SO)의 EnemyRuntimeStats만 사용.

[전제]
- EnemyStats ScriptableObject가 있고 CreateRuntimeStats(...)로 런타임 스탯 생성.
- Player의 Tag = "Player".
- 적 프리팹에 Rigidbody + Collider 존재(권장: isKinematic=true, useGravity=false, Interpolate).

[인스펙터]
- enemyStats           : 적 스탯 SO (필수)
- experienceOnDeath    : 처치 시 줄 경험치
- alwaysRotateToTarget : 이동 방향으로 고개 돌릴지(대량이면 false 권장)
- rotationLerp         : 회전 보간 속도(초당)
*/
public class EnemyAI : MonoBehaviour
{
    [Header("적 데이터(SO)")]
    [SerializeField] private EnemyStats enemyStats;   // ScriptableObject (필수)

    [Header("전투/경험치")]
    [SerializeField] private int experienceOnDeath = 25;

    [Header("추적 옵션(대량일 때 회전 OFF 권장)")]
    public bool alwaysRotateToTarget = false;
    public float rotationLerp = 12f;

    // 런타임 스탯(이 개체 전용 복제본)
    private EnemyRuntimeStats stats;

    // 캐시
    private Transform player;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void OnEnable()
    {
        // EnemyManager 등록(풀 재활성 포함)
        if (EnemyManager.instance != null)
            EnemyManager.instance.RegisterEnemy(this);

        // 런타임 스탯 생성/리셋
        if (stats == null)
        {
            // 웨이브 주입을 아직 안 한다면 1로 생성
            stats = (enemyStats != null)
                ? enemyStats.CreateRuntimeStats(1)
                : new EnemyRuntimeStats { maxHP = 60f, currentHP = 60f, moveSpeed = 2.5f, followRange = 9999f, attackRange = 2.5f, attackInterval = 1f, contactDamage = 10f };
        }
        else
        {
            stats.currentHP = stats.maxHP; // 풀 재사용 시 HP 리셋
        }
    }

    void OnDisable()
    {
        // EnemyManager 해제(풀 비활성 포함)
        if (EnemyManager.instance != null)
            EnemyManager.instance.UnregisterEnemy(this);
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // 항상 추적(감지/범위 체크 없음)
        Vector3 to = player.position - transform.position;
        to.y = 0f;

        Vector3 step = to.sqrMagnitude > 0.0001f
            ? to.normalized * stats.moveSpeed * Time.fixedDeltaTime
            : Vector3.zero;

        // 물리 일관성 위해 kinematic Rigidbody면 MovePosition 권장
        if (rb != null) rb.MovePosition(rb.position + step);
        else transform.position += step;

        if (alwaysRotateToTarget && step.sqrMagnitude > 0f)
        {
            Quaternion look = Quaternion.LookRotation(step.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationLerp * Time.fixedDeltaTime);
        }
    }

    // ===== 피해/사망 =====
    public void TakeDamage(float damage)
    {
        stats.TakeDamage(damage);
        if (stats.IsDead())
            Die();
    }

    void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddExperience(experienceOnDeath);

        var pooled = GetComponent<PooledEnemy>();
        if (pooled != null) pooled.Despawn();
        else gameObject.SetActive(false);
    }

    // 스폰러가 웨이브 반영하고 싶을 때 호출
    public void InitializeForWave(int waveIndex)
    {
        if (enemyStats != null)
            stats = enemyStats.CreateRuntimeStats(waveIndex);
        else
        {
            if (stats == null)
                stats = new EnemyRuntimeStats { maxHP = 60f, currentHP = 60f, moveSpeed = 2.5f, followRange = 9999f, attackRange = 2.5f, attackInterval = 1f, contactDamage = 10f };
            else
                stats.currentHP = stats.maxHP;
        }
    }

    public EnemyRuntimeStats GetRuntimeStats() => stats;
}
