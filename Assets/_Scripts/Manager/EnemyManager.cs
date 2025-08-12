// EnemyManager.cs - �� ���� ���͵� �� �� �ɸ��� ����ȭ �ý���
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // �̱��� ���� - ���� ��ü���� �ϳ��� ����
    public static EnemyManager instance;

    // ��� ������ ����Ʈ (���� ���ۺ��� ������)
    private List<EnemyAI> allEnemies = new List<EnemyAI>();

    // �÷��̾� ���� ���� �ȿ� �ִ� ���鸸 ���� ���� (�ٽ� ����ȭ!)
    // 100���� �߿� 5������ ������ �ִٸ�, 5������ �˻��ϸ� ��
    private List<EnemyAI> enemiesInRange = new List<EnemyAI>();

    void Awake()
    {
        // �̱��� ���� - �ٸ� ��ũ��Ʈ���� EnemyManager.instance�� ���� ����
        instance = this;
    }

    // ���� ������ �� ȣ�� - ��ü ����Ʈ�� ���
    public void RegisterEnemy(EnemyAI enemy)
    {
        allEnemies.Add(enemy);
        Debug.Log($"�� ���: �� {allEnemies.Count}����");
    }

    // ���� ���� �� ȣ�� - ��� ����Ʈ���� ����
    public void UnregisterEnemy(EnemyAI enemy)
    {
        allEnemies.Remove(enemy);
        enemiesInRange.Remove(enemy);
        Debug.Log($"�� ����: ���� �� {allEnemies.Count}����");
    }

    // ���� �÷��̾� ���� ������ ������ �� ȣ��
    // �̷��� �ϸ� �ָ� �ִ� 99������ �Ű� �� �ᵵ ��!
    public void AddToAttackRange(EnemyAI enemy)
    {
        if (!enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
            Debug.Log($"���� ���� ����: {enemy.name}");
        }
    }

    // ���� �÷��̾� ���� �������� ������ �� ȣ��
    public void RemoveFromAttackRange(EnemyAI enemy)
    {
        enemiesInRange.Remove(enemy);
        Debug.Log($"���� ���� ��Ż: {enemy.name}");
    }

    // �÷��̾ ȣ�� - ���� ���� �� ���� ����� �� ��ȯ
    // �ٽ�: ���� �ȿ� �ִ� �� ������ �˻�! (Find �� ��)
    public EnemyAI GetNearestEnemy(Vector3 playerPos)
    {
        // ���� �� ���� ������ null ��ȯ
        if (enemiesInRange.Count == 0) return null;

        EnemyAI nearest = null;
        float shortestSqrDist = float.MaxValue; // ���� ����� �Ÿ� ����

        // ���� �� ���鸸 �ݺ� (100������ �ƴ϶� 5������!)
        for (int i = 0; i < enemiesInRange.Count; i++)
        {
            // Ȥ�� �ı��� ���� ������ �ǳʶٱ�
            if (enemiesInRange[i] == null) continue;

            // �Ÿ� �������� ��� (Sqrt �� �Ἥ ����)
            float sqrDist = (enemiesInRange[i].transform.position - playerPos).sqrMagnitude;

            // �� ����� �� �߽߰� ������Ʈ
            if (sqrDist < shortestSqrDist)
            {
                shortestSqrDist = sqrDist;
                nearest = enemiesInRange[i];
            }
        }

        return nearest;
    }

    // ����׿� - ���� ���� ���
    public void PrintStatus()
    {
        Debug.Log($"��ü ��: {allEnemies.Count}����, ���� ���� ��: {enemiesInRange.Count}����");
    }
}