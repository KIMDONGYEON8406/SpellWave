using UnityEngine;

/*
[목적]
- 자신의 소속 풀(EnemyPool)을 기억해 두었다가 Despawn() 시 풀로 반환한다.
- 풀 참조가 없으면 안전망으로 비활성화(SetActive(false))만 수행.

[인스펙터]
- 설정할 것 없음. 적 프리팹에 이 스크립트만 추가.

[연결 순서]
1) 적 프리팹 선택 → Add Component → PooledEnemy
2) EnemyPool.Get()이 호출될 때 SetPool()로 풀 참조를 자동 주입한다.
*/
[DisallowMultipleComponent]
public class PooledEnemy : MonoBehaviour
{
    private EnemyPool _pool;

    /// <summary>EnemyPool에서 호출: 풀 참조 주입</summary>
    public void SetPool(EnemyPool pool) => _pool = pool;

    /// <summary>사망/소멸 시 호출: 파괴 대신 풀로 반환</summary>
    public void Despawn()
    {
        if (_pool != null) _pool.Return(gameObject);
        else gameObject.SetActive(false); // 안전망
    }
}
