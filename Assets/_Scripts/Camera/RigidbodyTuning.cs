using UnityEngine;

/// <summary>
/// 대상의 Rigidbody를 자동으로 튜닝:
/// - Interpolation = Interpolate  (프레임 사이 보간으로 시각적 끊김 감소)
/// - Collision Detection = Continuous (빠른 이동 중 충돌 누락 방지)
/// 플레이어/적 오브젝트에 붙이면 됩니다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RigidbodyTuning : MonoBehaviour
{
    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }
}
