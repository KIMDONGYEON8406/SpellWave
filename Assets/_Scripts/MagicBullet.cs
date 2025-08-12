using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("발사체 설정")]
    public float speed = 10f;
    public int damage = 25;
    public float lifeTime = 3f; // 3초 후 자동 소멸

    private Vector3 direction;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 중력 영향 안 받게
        rb.useGravity = false;

        // 일정 시간 후 자동 파괴
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        // 설정된 방향으로 날아가기
        rb.velocity = direction * speed;
    }

    // 발사 방향 설정 함수
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    // 적과 충돌시 처리
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 적에게 데미지 주기
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($" {other.name}에게 {damage} 데미지!");
            }

            // 발사체 파괴
            Destroy(gameObject);
        }
    }
}