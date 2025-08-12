using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        // 시작할 때 최대 체력으로 설정
        currentHealth = maxHealth;
    }

    // 데미지 받는 함수
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{name} 데미지 {damage} 받음! 남은 체력: {currentHealth}");

        // 체력이 0 이하가 되면 죽음
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 죽음 처리
    void Die()
    {
        Debug.Log($" {name} 사망!");

        // 적 오브젝트 파괴 (OnDestroy에서 자동으로 EnemyManager에서 제거됨)
        Destroy(gameObject);
    }
}