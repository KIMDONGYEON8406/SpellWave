using UnityEngine;

/*
[����]
- �ڽ��� �Ҽ� Ǯ(EnemyPool)�� ����� �ξ��ٰ� Despawn() �� Ǯ�� ��ȯ�Ѵ�.
- Ǯ ������ ������ ���������� ��Ȱ��ȭ(SetActive(false))�� ����.

[�ν�����]
- ������ �� ����. �� �����տ� �� ��ũ��Ʈ�� �߰�.

[���� ����]
1) �� ������ ���� �� Add Component �� PooledEnemy
2) EnemyPool.Get()�� ȣ��� �� SetPool()�� Ǯ ������ �ڵ� �����Ѵ�.
*/
[DisallowMultipleComponent]
public class PooledEnemy : MonoBehaviour
{
    private EnemyPool _pool;

    /// <summary>EnemyPool���� ȣ��: Ǯ ���� ����</summary>
    public void SetPool(EnemyPool pool) => _pool = pool;

    /// <summary>���/�Ҹ� �� ȣ��: �ı� ��� Ǯ�� ��ȯ</summary>
    public void Despawn()
    {
        if (_pool != null) _pool.Return(gameObject);
        else gameObject.SetActive(false); // ������
    }
}
