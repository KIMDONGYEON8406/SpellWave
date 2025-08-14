using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("플레이어 데이터")]
    [SerializeField] private PlayerStats playerStats; // ScriptableObject 참조

    // 런타임 변수들 (ScriptableObject 값을 복사해서 사용)
    private float currentMoveSpeed;
    private float currentRotationSpeed;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // ScriptableObject에서 값들을 복사
        InitializeStats();

        // 프레임 제한 추가 (모바일 최적화)
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        Debug.Log("마법사 준비 완료!");
    }

    void InitializeStats()
    {
        if (playerStats != null)
        {
            currentMoveSpeed = playerStats.moveSpeed;
            currentRotationSpeed = playerStats.rotationSpeed;
            playerStats.ResetToDefault(); // 게임 시작 시 체력 리셋
        }
        else
        {
            Debug.LogError("PlayerStats가 할당되지 않았습니다!");
        }
    }

    void Update()
    {
        HandleInput();
        UpdateAnimation();

        // 런타임에 스탯이 변경될 수 있으므로 업데이트
        UpdateStatsFromSO();
    }

    void UpdateStatsFromSO()
    {
        if (playerStats != null)
        {
            currentMoveSpeed = playerStats.moveSpeed;
            currentRotationSpeed = playerStats.rotationSpeed;
        }
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
        if (moveDirection.magnitude > 0.1f)
        {
            rb.velocity = new Vector3(
                moveDirection.x * currentMoveSpeed,
                rb.velocity.y,
                moveDirection.z * currentMoveSpeed
            );

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                currentRotationSpeed * Time.deltaTime
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
        animator.SetBool("IsMoving", isMoving);
    }

    // 외부에서 PlayerStats에 접근할 수 있는 함수
    public PlayerStats GetPlayerStats()
    {
        return playerStats;
    }

    // 체력 관련 함수들
    public void TakeDamage(float damage)
    {
        if (playerStats != null)
        {
            playerStats.currentHP -= damage;
            if (playerStats.currentHP <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");
        // 게임 오버 처리
    }
}