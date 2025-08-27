using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 벽(Trigger)에 붙여 쓰는 텔레포트 스크립트(OFFSET 불필요):
/// - 충돌한 대상의 X/Z를 'oppositeWall'의 X/Z로 즉시 이동시킨다. (Y는 유지)
/// - 목적지 Transform(oppositeWall)을 맵 안쪽으로 살짝 배치해두면 재충돌이 없다.
/// - 짧은 쿨다운으로 왕복 튕김을 방지하고, 텔레포트 이벤트를 브로드캐스트한다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WallTeleport : MonoBehaviour
{
    [Header("워프 목적지 (반대편 벽/빈 오브젝트 등)")]
    public Transform oppositeWall;   // 맵 안쪽으로 0.2~0.5m 정도 앞당겨 둔 목적지

    [Header("워프 대상 태그 (여기 포함된 태그만 워프)")]
    public string[] warpTags = { "Player", "Enemy", "Projectile" };

    [Header("재충돌 방지 쿨다운(초)")]
    public float cooldown = 0.1f;

    // 콜라이더별 마지막 워프 시각 -> 즉시 재트리거 방지
    private static readonly Dictionary<Collider, float> lastWarpTime = new Dictionary<Collider, float>();

    private void Reset()
    {
        // 벽은 반드시 Trigger로 사용
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (oppositeWall == null) return;
        if (!IsWarpTarget(other.tag)) return;

        // 같은 프레임/아주 짧은 시간 내 재트리거 방지
        if (lastWarpTime.TryGetValue(other, out float t) && Time.time - t < cooldown) return;

        // 현재 높이(Y)는 유지하고, X/Z만 목적지로 이동
        Vector3 cur = other.transform.position;
        Vector3 dst = new Vector3(oppositeWall.position.x, cur.y, oppositeWall.position.z);

        var rb = other.attachedRigidbody;
        if (rb != null) rb.position = dst;
        else other.transform.position = dst;

        lastWarpTime[other] = Time.time;

        // 텔레포트 알림(카메라가 같은 프레임에 즉시 스냅하도록)
        TeleportHub.Notify(other.transform);
    }

    private bool IsWarpTarget(string tag)
    {
        for (int i = 0; i < warpTags.Length; i++)
            if (warpTags[i] == tag) return true;
        return false;
    }
}
