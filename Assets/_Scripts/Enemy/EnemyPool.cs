using System.Collections.Generic;
using UnityEngine;

/*
[목적]
- 하나의 적 프리팹을 미리 생성해 두고(SetActive(false)) 재사용(풀링)한다.
- Get()으로 꺼낼 때 위치/회전을 설정하고 활성화한다.
- Return()으로 되돌리면 비활성화하여 큐에 넣는다.

[인스펙터]
- enemyPrefab : 풀링할 "적 프리팹"(PooledEnemy, EnemyAI 포함)
- initialSize : 시작 시 미리 만들어 둘 개수

[연결 순서]
1) Empty → "EnemyPool" 생성 → 본 스크립트 부착
2) enemyPrefab 슬롯에 적 프리팹 드래그
3) 적 프리팹에는 반드시 "PooledEnemy" 컴포넌트 추가
*/
public class EnemyPool : MonoBehaviour
{
    [Header("풀링 대상 프리팹 (PooledEnemy, EnemyAI 포함)")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("시작 시 미리 만들어둘 개수")]
    [SerializeField] private int initialSize = 20;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        // 초기 프리워밍
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(enemyPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// <summary>풀에서 하나 꺼내 활성화 후 반환. 없으면 새로 만든다.</summary>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(enemyPrefab);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.transform.SetParent(null, true); // 풀 오브젝트 자식에서 분리
        obj.SetActive(true);

        // 풀 참조 주입(없어도 동작하지만 있으면 반환 경로가 정확함)
        var pe = obj.GetComponent<PooledEnemy>();
        if (pe != null) pe.SetPool(this);

        return obj;
    }

    /// <summary>적 오브젝트를 풀로 되돌린다.</summary>
    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform, false);
        pool.Enqueue(obj);
    }
}
