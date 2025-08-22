using UnityEngine;

/*
[����]
- ���� ��� ������ �÷��̾ ����(���� ����/���� ���� ����/��Ż ���� ����).
- Ǯ��(��Ȱ������Ȱ��)�� EnemyManager ���/���� Ÿ�ֿ̹� ����.
- HP/������ EnemyStats(SO)�� EnemyRuntimeStats�� ���.

[����]
- EnemyStats ScriptableObject�� �ְ� CreateRuntimeStats(...)�� ��Ÿ�� ���� ����.
- Player�� Tag = "Player".
- �� �����տ� Rigidbody + Collider ����(����: isKinematic=true, useGravity=false, Interpolate).

[�ν�����]
- enemyStats           : �� ���� SO (�ʼ�)
- experienceOnDeath    : óġ �� �� ����ġ
- alwaysRotateToTarget : �̵� �������� ��� ������(�뷮�̸� false ����)
- rotationLerp         : ȸ�� ���� �ӵ�(�ʴ�)
*/
public class EnemyAI : MonoBehaviour
{
    [Header("�� ������(SO)")]
    [SerializeField] private EnemyStats enemyStats;   // ScriptableObject (�ʼ�)

    [Header("����/����ġ")]
    [SerializeField] private int experienceOnDeath = 25;

    [Header("���� �ɼ�(�뷮�� �� ȸ�� OFF ����)")]
    public bool alwaysRotateToTarget = false;
    public float rotationLerp = 12f;

    // ��Ÿ�� ����(�� ��ü ���� ������)
    private EnemyRuntimeStats stats;

    // ĳ��
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
        // EnemyManager ���(Ǯ ��Ȱ�� ����)
        if (EnemyManager.instance != null)
            EnemyManager.instance.RegisterEnemy(this);

        // ��Ÿ�� ���� ����/����
        if (stats == null)
        {
            // ���̺� ������ ���� �� �Ѵٸ� 1�� ����
            stats = (enemyStats != null)
                ? enemyStats.CreateRuntimeStats(1)
                : new EnemyRuntimeStats { maxHP = 60f, currentHP = 60f, moveSpeed = 2.5f, followRange = 9999f, attackRange = 2.5f, attackInterval = 1f, contactDamage = 10f };
        }
        else
        {
            stats.currentHP = stats.maxHP; // Ǯ ���� �� HP ����
        }
    }

    void OnDisable()
    {
        // EnemyManager ����(Ǯ ��Ȱ�� ����)
        if (EnemyManager.instance != null)
            EnemyManager.instance.UnregisterEnemy(this);
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // �׻� ����(����/���� üũ ����)
        Vector3 to = player.position - transform.position;
        to.y = 0f;

        Vector3 step = to.sqrMagnitude > 0.0001f
            ? to.normalized * stats.moveSpeed * Time.fixedDeltaTime
            : Vector3.zero;

        // ���� �ϰ��� ���� kinematic Rigidbody�� MovePosition ����
        if (rb != null) rb.MovePosition(rb.position + step);
        else transform.position += step;

        if (alwaysRotateToTarget && step.sqrMagnitude > 0f)
        {
            Quaternion look = Quaternion.LookRotation(step.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationLerp * Time.fixedDeltaTime);
        }
    }

    // ===== ����/��� =====
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

    // �������� ���̺� �ݿ��ϰ� ���� �� ȣ��
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
