// EnemyManager.cs - 백 마리 몬스터도 렉 안 걸리는 최적화 시스템
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // 싱글톤 패턴 - 게임 전체에서 하나만 존재
    public static EnemyManager instance;

    // 모든 적들의 리스트 (게임 시작부터 끝까지)
    private List<EnemyAI> allEnemies = new List<EnemyAI>();

    // 플레이어 공격 범위 안에 있는 적들만 따로 관리 (핵심 최적화!)
    // 100마리 중에 5마리만 범위에 있다면, 5마리만 검사하면 됨
    private List<EnemyAI> enemiesInRange = new List<EnemyAI>();

    void Awake()
    {
        // 싱글톤 설정 - 다른 스크립트에서 EnemyManager.instance로 접근 가능
        instance = this;
    }

    // 적이 생성될 때 호출 - 전체 리스트에 등록
    public void RegisterEnemy(EnemyAI enemy)
    {
        allEnemies.Add(enemy);
        DebugManager.LogTargetingSystem($"적 등록: 총 {allEnemies.Count}마리");
    }

    // 적이 죽을 때 호출 - 모든 리스트에서 제거
    public void UnregisterEnemy(EnemyAI enemy)
    {
        allEnemies.Remove(enemy);
        enemiesInRange.Remove(enemy);
        DebugManager.LogTargetingSystem($"적 제거: 남은 적 {allEnemies.Count}마리");
    }

    // 적이 플레이어 공격 범위에 들어왔을 때 호출
    // 이렇게 하면 멀리 있는 99마리는 신경 안 써도 됨!
    public void AddToAttackRange(EnemyAI enemy)
    {
        if (!enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
            DebugManager.LogTargetingSystem($"공격 범위 진입: {enemy.name}");
        }
    }

    // 적이 플레이어 공격 범위에서 나갔을 때 호출
    public void RemoveFromAttackRange(EnemyAI enemy)
    {
        enemiesInRange.Remove(enemy);
        DebugManager.LogTargetingSystem($"공격 범위 이탈: {enemy.name}");
    }

    // 플레이어가 호출 - 공격 범위 내 가장 가까운 적 반환
    // 핵심: 범위 안에 있는 몇 마리만 검사! (Find 안 씀)
    public EnemyAI GetNearestEnemy(Vector3 playerPos)
    {
        // 범위 내 적이 없으면 null 반환
        if (enemiesInRange.Count == 0) return null;

        EnemyAI nearest = null;
        float shortestSqrDist = float.MaxValue; // 가장 가까운 거리 저장

        // 범위 내 적들만 반복 (100마리가 아니라 5마리만!)
        for (int i = 0; i < enemiesInRange.Count; i++)
        {
            // 혹시 파괴된 적이 있으면 건너뛰기
            if (enemiesInRange[i] == null) continue;

            // 거리 제곱으로 계산 (Sqrt 안 써서 빠름)
            float sqrDist = (enemiesInRange[i].transform.position - playerPos).sqrMagnitude;

            // 더 가까운 적 발견시 업데이트
            if (sqrDist < shortestSqrDist)
            {
                shortestSqrDist = sqrDist;
                nearest = enemiesInRange[i];
            }
        }

        return nearest;
    }

    // 디버그용 - 현재 상태 출력
    public void PrintStatus()
    {
        DebugManager.LogTargetingSystem($"전체 적: {allEnemies.Count}마리, 공격 범위 내: {enemiesInRange.Count}마리");
    }

    // EnemyManager.cs ���� ��򰡿� ��ƿ��Ƽ �߰�(����)
    public int GetAllEnemiesCountSafe()
    {
        int c = 0;
        foreach (var e in allEnemies)
            if (e != null && e.gameObject.activeInHierarchy) c++;
        return c;
    }
}