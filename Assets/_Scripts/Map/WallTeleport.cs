using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��(Trigger)�� �ٿ� ���� �ڷ���Ʈ ��ũ��Ʈ(OFFSET ���ʿ�):
/// - �浹�� ����� X/Z�� 'oppositeWall'�� X/Z�� ��� �̵���Ų��. (Y�� ����)
/// - ������ Transform(oppositeWall)�� �� �������� ��¦ ��ġ�صθ� ���浹�� ����.
/// - ª�� ��ٿ����� �պ� ƨ���� �����ϰ�, �ڷ���Ʈ �̺�Ʈ�� ��ε�ĳ��Ʈ�Ѵ�.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WallTeleport : MonoBehaviour
{
    [Header("���� ������ (�ݴ��� ��/�� ������Ʈ ��)")]
    public Transform oppositeWall;   // �� �������� 0.2~0.5m ���� �մ�� �� ������

    [Header("���� ��� �±� (���� ���Ե� �±׸� ����)")]
    public string[] warpTags = { "Player", "Enemy", "Projectile" };

    [Header("���浹 ���� ��ٿ�(��)")]
    public float cooldown = 0.1f;

    // �ݶ��̴��� ������ ���� �ð� -> ��� ��Ʈ���� ����
    private static readonly Dictionary<Collider, float> lastWarpTime = new Dictionary<Collider, float>();

    private void Reset()
    {
        // ���� �ݵ�� Trigger�� ���
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (oppositeWall == null) return;
        if (!IsWarpTarget(other.tag)) return;

        // ���� ������/���� ª�� �ð� �� ��Ʈ���� ����
        if (lastWarpTime.TryGetValue(other, out float t) && Time.time - t < cooldown) return;

        // ���� ����(Y)�� �����ϰ�, X/Z�� �������� �̵�
        Vector3 cur = other.transform.position;
        Vector3 dst = new Vector3(oppositeWall.position.x, cur.y, oppositeWall.position.z);

        var rb = other.attachedRigidbody;
        if (rb != null) rb.position = dst;
        else other.transform.position = dst;

        lastWarpTime[other] = Time.time;

        // �ڷ���Ʈ �˸�(ī�޶� ���� �����ӿ� ��� �����ϵ���)
        TeleportHub.Notify(other.transform);
    }

    private bool IsWarpTarget(string tag)
    {
        for (int i = 0; i < warpTags.Length; i++)
            if (warpTags[i] == tag) return true;
        return false;
    }
}
