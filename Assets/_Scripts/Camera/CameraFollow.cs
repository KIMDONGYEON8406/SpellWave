using UnityEngine;

/// <summary>
/// �÷��̾ �ε巴�� ���󰡴� ī�޶�(���� ����):
/// - Rigidbody�� �̵��ϴ� Ÿ���� LateUpdate���� ����
/// - offset��ŭ ������ SmoothDamp�� �̵�
/// - �ڷ���Ʈ �߻� �� ���� �����ӿ� ��� ����(���� ����)
/// - Ŭ����(��ġ ����) ���� ����� ���� ����
/// </summary>
[DisallowMultipleComponent]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                // ���� ���(�÷��̾�)

    [Header("Position")]
    public bool useInitialOffset = true;    // Start���� ���� ������ �ڵ� ���
    public Vector3 offset = new Vector3(0f, 12f, -10f);
    public float smoothTime = 0.15f;        // 0�̸� ��� ����
    private Vector3 _vel;

    [Header("Teleport / Big Jump Snap")]
    public bool autoSnapOnTeleport = true;  // �ڷ���Ʈ �� ���� �����ӿ� ��� ����
    public float snapDistance = 8f;         // �� ������ �̵��� �� ������ ũ�� ��� ����

    [Header("Rotation")]
    public bool keepCameraRotation = true;  // true�� ȸ�� ����, false�� Ÿ���� �ٶ�

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

        // �ڷ���Ʈ �ܿ��� ū ������ �����Ǹ� ��� ����
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
    /// �ڷ���Ʈ �߻� �� ���� �����ӿ� ��� ����
    /// </summary>
    private void HandleTeleportSnap(Transform t)
    {
        if (!autoSnapOnTeleport || target == null || t != target) return;

        Vector3 desired = target.position + offset;
        transform.position = desired;   // ��� ����
        _vel = Vector3.zero;            // SmoothDamp ���� �ʱ�ȭ
        _lastTargetPos = target.position;
    }
}
