using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("ü�� ����")]
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        // ������ �� �ִ� ü������ ����
        currentHealth = maxHealth;
    }

    // ������ �޴� �Լ�
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{name} ������ {damage} ����! ���� ü��: {currentHealth}");

        // ü���� 0 ���ϰ� �Ǹ� ����
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ���� ó��
    void Die()
    {
        Debug.Log($" {name} ���!");

        // �� ������Ʈ �ı� (OnDestroy���� �ڵ����� EnemyManager���� ���ŵ�)
        Destroy(gameObject);
    }
}