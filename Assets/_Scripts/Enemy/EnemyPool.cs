using System.Collections.Generic;
using UnityEngine;

/*
[����]
- �ϳ��� �� �������� �̸� ������ �ΰ�(SetActive(false)) ����(Ǯ��)�Ѵ�.
- Get()���� ���� �� ��ġ/ȸ���� �����ϰ� Ȱ��ȭ�Ѵ�.
- Return()���� �ǵ����� ��Ȱ��ȭ�Ͽ� ť�� �ִ´�.

[�ν�����]
- enemyPrefab : Ǯ���� "�� ������"(PooledEnemy, EnemyAI ����)
- initialSize : ���� �� �̸� ����� �� ����

[���� ����]
1) Empty �� "EnemyPool" ���� �� �� ��ũ��Ʈ ����
2) enemyPrefab ���Կ� �� ������ �巡��
3) �� �����տ��� �ݵ�� "PooledEnemy" ������Ʈ �߰�
*/
public class EnemyPool : MonoBehaviour
{
    [Header("Ǯ�� ��� ������ (PooledEnemy, EnemyAI ����)")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("���� �� �̸� ������ ����")]
    [SerializeField] private int initialSize = 20;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        // �ʱ� ��������
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(enemyPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// <summary>Ǯ���� �ϳ� ���� Ȱ��ȭ �� ��ȯ. ������ ���� �����.</summary>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(enemyPrefab);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.transform.SetParent(null, true); // Ǯ ������Ʈ �ڽĿ��� �и�
        obj.SetActive(true);

        // Ǯ ���� ����(��� ���������� ������ ��ȯ ��ΰ� ��Ȯ��)
        var pe = obj.GetComponent<PooledEnemy>();
        if (pe != null) pe.SetPool(this);

        return obj;
    }

    /// <summary>�� ������Ʈ�� Ǯ�� �ǵ�����.</summary>
    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform, false);
        pool.Enqueue(obj);
    }
}
