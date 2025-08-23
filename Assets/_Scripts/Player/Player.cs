using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("플레이어 스탯")]
    [SerializeField] private PlayerStats playerStats;

    [Header("경험치 시스템")]
    public float currentXP = 0f;
    public int level = 1;

    // 컴포넌트
    private Rigidbody rb;
    private Animator animator;
    private SkillManager skillManager;
    private Vector3 moveDirection;

    // 외부 접근용 프로퍼티 (Character.cs 대체)
    public float AttackPower => playerStats?.attackPower ?? 10f;
    public float AttackRange => playerStats?.attackRange ?? 10f;
    public float Health => playerStats?.currentHP ?? 100f;
    public float MaxHealth => playerStats?.maxHP ?? 100f;
    public float MoveSpeed => playerStats?.moveSpeed ?? 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        skillManager = GetComponent<SkillManager>();

        if (skillManager == null)
            skillManager = gameObject.AddComponent<SkillManager>();
    }

    void Start()
    {
        if (playerStats != null)
        {
            playerStats.ResetToDefault();
        }

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        Debug.Log("플레이어 준비 완료!");
    }

    void Update()
    {
        HandleInput();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveDirection = new Vector3(horizontal, 0, vertical).normalized;
    }

    void HandleMovement()
    {
        if (moveDirection.magnitude > 0.1f && playerStats != null)
        {
            rb.velocity = new Vector3(
                moveDirection.x * playerStats.moveSpeed,
                rb.velocity.y,
                moveDirection.z * playerStats.moveSpeed
            );

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                playerStats.rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    void UpdateAnimation()
    {
        bool isMoving = moveDirection.magnitude > 0.1f;
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }
    }

    public void TakeDamage(float damage)
    {
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
            if (playerStats.currentHP <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        Debug.Log("플레이어 사망!");
        // GameManager가 있으면 사용
        if (GameManager.Instance != null)
        {
           // GameManager.Instance.GameOver();
        }
    }

    public void AddExperience(float xp)
    {
        currentXP += xp;
        // GameManager가 있으면 사용
        if (GameManager.Instance != null)
        {
            //GameManager.Instance.CheckLevelUp();
        }
    }

    public void Heal(float amount)
    {
        if (playerStats != null)
        {
            playerStats.Heal(amount);
        }
    }

    public PlayerStats GetPlayerStats() => playerStats;
}