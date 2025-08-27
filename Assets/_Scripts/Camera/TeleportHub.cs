using System;
using UnityEngine;

/// <summary>
/// 텔레포트가 발생했음을 브로드캐스트하는 정적 허브.
/// - 카메라 등 구독자는 이 이벤트를 받아 같은 프레임에 스냅 처리.
/// </summary>
public static class TeleportHub
{
    /// <param name="Transform">텔레포트된 대상(주로 Player)</param>
    public static event Action<Transform> OnTeleported;

    /// <summary>텔레포트 발생을 알림</summary>
    public static void Notify(Transform t)
    {
        OnTeleported?.Invoke(t);
    }
}
