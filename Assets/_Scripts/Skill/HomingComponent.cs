using UnityEngine;

public class HomingComponent : MonoBehaviour
{
    [Header("유도 설정")]
    public Transform target;
    public float rotationSpeed = 5f;
    public float maxHomingAngle = 45f;  // 최대 유도 각도

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (target == null || rb == null) return;

        // 타겟 방향 계산
        Vector3 direction = (target.position - transform.position).normalized;

        // 현재 속도 방향
        Vector3 currentDirection = rb.velocity.normalized;

        // 부드럽게 회전
        Vector3 newDirection = Vector3.Slerp(currentDirection, direction, rotationSpeed * Time.fixedDeltaTime);

        // 최대 각도 제한
        float angle = Vector3.Angle(currentDirection, newDirection);
        if (angle > maxHomingAngle * Time.fixedDeltaTime)
        {
            newDirection = Vector3.Slerp(currentDirection, direction, maxHomingAngle / angle * Time.fixedDeltaTime);
        }

        // 속도 적용
        float speed = rb.velocity.magnitude;
        rb.velocity = newDirection * speed;

        // 회전도 맞춤
        if (rb.velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }
}