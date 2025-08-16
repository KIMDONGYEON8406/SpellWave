using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�÷��̾� ������")]
    [SerializeField] private PlayerStats playerStats; // ScriptableObject ����

    // ��Ÿ�� ������ (ScriptableObject ���� �����ؼ� ���)
    private float currentMoveSpeed;
    private float currentRotationSpeed;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // ScriptableObject���� ������ ����
        InitializeStats();

        // ������ ���� �߰� (����� ����ȭ)
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        Debug.Log("������ �غ� �Ϸ�!");
    }

    void InitializeStats()
    {
        if (playerStats != null)
        {
            currentMoveSpeed = playerStats.moveSpeed;
            currentRotationSpeed = playerStats.rotationSpeed;
            playerStats.ResetToDefault(); // ���� ���� �� ü�� ����
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

        // ��Ÿ�ӿ� ������ ����� �� �����Ƿ� ������Ʈ
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

    // �ܺο��� PlayerStats�� ������ �� �ִ� �Լ�
    public PlayerStats GetPlayerStats()
    {
        return playerStats;
    }

    // ü�� ���� �Լ���
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
        // ���� ���� ó��
    }
}