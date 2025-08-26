using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("플레이어 데이터")]
    [SerializeField] private PlayerStats playerStats; // ScriptableObject 참조

    [Header("입력 소스")]
    [Tooltip("캔버스에 배치한 조이스틱(예: UJoystick/Content/Prefab/Joystick) 컴포넌트를 연결하세요.")]
    [SerializeField] private bl_Joystick joystick;


    [Header("이동 설정")]
    [SerializeField] private float keyboardScale = 1f; // 키보드 감도 스케일
    [SerializeField] private float joystickScale = 1f; // 조이스틱 감도 스케일

    private float currentMoveSpeed;
    private float currentRotationSpeed;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        InitializeStats();

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }

    void InitializeStats()
    {
        if (playerStats != null)
        {
            currentMoveSpeed = playerStats.moveSpeed;
            currentRotationSpeed = playerStats.rotationSpeed;
            playerStats.ResetToDefault();
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
        UpdateStatsFromSO();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void UpdateStatsFromSO()
    {
        if (playerStats == null) return;
        currentMoveSpeed = playerStats.moveSpeed;
        currentRotationSpeed = playerStats.rotationSpeed;
    }

    // ───────────────────────────────────────────────────────────
    // 입력 처리: 키보드 + 조이스틱을 합산 (우선순위/데드존 없음)
    // ───────────────────────────────────────────────────────────
    void HandleInput()
    {
        // 1) 키보드 입력(WASD)
        Vector2 kb = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ) * keyboardScale;

        // 2) 조이스틱 입력(없으면 0)
        Vector2 js = Vector2.zero;
        if (joystick != null)
        {
            js = new Vector2(joystick.Horizontal, joystick.Vertical) * joystickScale;
        }

        // 3) 합산 후 클램프 (대각선/동시 입력 과도 속도 방지)
        Vector2 sum = kb + js;
        if (sum.sqrMagnitude > 1f) sum.Normalize();

        moveDirection = new Vector3(sum.x, 0f, sum.y);
    }

    void HandleMovement()
    {
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            // 이동
            Vector3 vel = new Vector3(
                moveDirection.x * currentMoveSpeed,
                rb.velocity.y,
                moveDirection.z * currentMoveSpeed
            );
            rb.velocity = vel;

            // 회전
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                currentRotationSpeed * Time.deltaTime
            );
        }
        else
        {
            // 정지 시 수평 속도 0
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;
        bool isMoving = moveDirection.sqrMagnitude > 0.0001f;
        animator.SetBool("IsMoving", isMoving);
    }

    // ───────────── 체력/사망 처리 (기존 테스트 로직 유지) ─────────────
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
        // TODO: 게임 오버 처리
    }

    public PlayerStats GetPlayerStats() => playerStats;
}
