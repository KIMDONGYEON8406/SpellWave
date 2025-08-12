using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        Debug.Log("마법사 준비 완료!");
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
        if (moveDirection.magnitude > 0.1f)
        {
            rb.velocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.velocity.y,
                moveDirection.z * moveSpeed
            );

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
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
}