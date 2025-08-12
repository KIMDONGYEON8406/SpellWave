using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("���� ����")]
    public float attackRange = 5f;
    public float attackSpeed = 1f;

    private float lastAttackTime;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Time.time >= lastAttackTime + (1f / attackSpeed))
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        Debug.Log("���� ����!");
        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;
    }
}