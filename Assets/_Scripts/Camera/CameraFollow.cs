using UnityEngine;

/// <summary>
/// 플레이어를 부드럽게 따라가는 카메라(간단 버전):
/// - Rigidbody로 이동하는 타겟을 LateUpdate에서 추적
/// - offset만큼 떨어져 SmoothDamp로 이동
/// - 텔레포트 발생 시 같은 프레임에 즉시 스냅(끊김 제거)
/// - 클램프(위치 제한) 관련 기능은 전부 제거
/// </summary>
[DisallowMultipleComponent]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                // 따라갈 대상(플레이어)

    [Header("Position")]
    public bool useInitialOffset = true;    // Start에서 현재 오프셋 자동 계산
    public Vector3 offset = new Vector3(0f, 12f, -10f);
    public float smoothTime = 0.15f;        // 0이면 즉시 추적
    private Vector3 _vel;

    [Header("Teleport / Big Jump Snap")]
    public bool autoSnapOnTeleport = true;  // 텔레포트 시 같은 프레임에 즉시 스냅
    public float snapDistance = 8f;         // 한 프레임 이동이 이 값보다 크면 즉시 스냅

    [Header("Rotation")]
    public bool keepCameraRotation = true;  // true면 회전 고정, false면 타겟을 바라봄

    private Vector3 _lastTargetPos;

    void OnEnable()
    {
        TeleportHub.OnTeleported += HandleTeleportSnap;
    }

    void OnDisable()
    {
        TeleportHub.OnTeleported -= HandleTeleportSnap;
    }

    void Start()
    {
        if (target == null) return;

        if (useInitialOffset)
            offset = transform.position - target.position;

        _lastTargetPos = target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;

        // 텔레포트 외에도 큰 점프가 감지되면 즉시 스냅
        float moved = Vector3.Distance(target.position, _lastTargetPos);
        bool doSnap = moved > snapDistance;

        Vector3 nextPos = doSnap
            ? desired
            : Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);

        transform.position = nextPos;

        if (!keepCameraRotation)
            transform.LookAt(target);

        _lastTargetPos = target.position;
    }

    /// <summary>
    /// 텔레포트 발생 시 같은 프레임에 즉시 스냅
    /// </summary>
    private void HandleTeleportSnap(Transform t)
    {
        if (!autoSnapOnTeleport || target == null || t != target) return;

        Vector3 desired = target.position + offset;
        transform.position = desired;   // 즉시 스냅
        _vel = Vector3.zero;            // SmoothDamp 관성 초기화
        _lastTargetPos = target.position;
    }
}
