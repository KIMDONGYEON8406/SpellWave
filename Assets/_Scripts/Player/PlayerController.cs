using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�÷��̾� ������")]
    [SerializeField] private PlayerStats playerStats; // ScriptableObject ����

    [Header("�Է� �ҽ�")]
    [Tooltip("ĵ������ ��ġ�� ���̽�ƽ(��: UJoystick/Content/Prefab/Joystick) ������Ʈ�� �����ϼ���.")]
    [SerializeField] private bl_Joystick joystick;


    [Header("�̵� ����")]
    [SerializeField] private float keyboardScale = 1f; // Ű���� ���� ������
    [SerializeField] private float joystickScale = 1f; // ���̽�ƽ ���� ������

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
            Debug.LogError("PlayerStats�� �Ҵ���� �ʾҽ��ϴ�!");
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

    // ����������������������������������������������������������������������������������������������������������������������
    // �Է� ó��: Ű���� + ���̽�ƽ�� �ջ� (�켱����/������ ����)
    // ����������������������������������������������������������������������������������������������������������������������
    void HandleInput()
    {
        // 1) Ű���� �Է�(WASD)
        Vector2 kb = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ) * keyboardScale;

        // 2) ���̽�ƽ �Է�(������ 0)
        Vector2 js = Vector2.zero;
        if (joystick != null)
        {
            js = new Vector2(joystick.Horizontal, joystick.Vertical) * joystickScale;
        }

        // 3) �ջ� �� Ŭ���� (�밢��/���� �Է� ���� �ӵ� ����)
        Vector2 sum = kb + js;
        if (sum.sqrMagnitude > 1f) sum.Normalize();

        moveDirection = new Vector3(sum.x, 0f, sum.y);
    }

    void HandleMovement()
    {
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            // �̵�
            Vector3 vel = new Vector3(
                moveDirection.x * currentMoveSpeed,
                rb.velocity.y,
                moveDirection.z * currentMoveSpeed
            );
            rb.velocity = vel;

            // ȸ��
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                currentRotationSpeed * Time.deltaTime
            );
        }
        else
        {
            // ���� �� ���� �ӵ� 0
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;
        bool isMoving = moveDirection.sqrMagnitude > 0.0001f;
        animator.SetBool("IsMoving", isMoving);
    }

    // �������������������������� ü��/��� ó�� (���� �׽�Ʈ ���� ����) ��������������������������
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
        Debug.Log("�÷��̾� ���!");
        // TODO: ���� ���� ó��
    }

    public PlayerStats GetPlayerStats() => playerStats;
}
