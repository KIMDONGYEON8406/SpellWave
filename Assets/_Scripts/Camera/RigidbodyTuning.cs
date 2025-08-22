using UnityEngine;

/// <summary>
/// ����� Rigidbody�� �ڵ����� Ʃ��:
/// - Interpolation = Interpolate  (������ ���� �������� �ð��� ���� ����)
/// - Collision Detection = Continuous (���� �̵� �� �浹 ���� ����)
/// �÷��̾�/�� ������Ʈ�� ���̸� �˴ϴ�.
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
