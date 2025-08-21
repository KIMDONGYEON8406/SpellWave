using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("발사체 설정")]
    public float speed = 10f;
    public float damage = 25f; // int에서 float으로 변경 (ScriptableObject와 맞춤)
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

    // 데미지 설정 함수 (PlayerAttack에서 사용)
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    // 적과 충돌시 처리
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // EnemyAI에서 직접 TakeDamage 호출
            EnemyAI enemyAI = other.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(damage);
                Debug.Log($"{other.name}에게 {damage} 데미지!");
            }
            else
            {
                Debug.LogWarning($"{other.name}에 EnemyAI 컴포넌트가 없습니다!");
            }

            // 발사체 파괴
            Destroy(gameObject);
        }
    }
}