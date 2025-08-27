using System;
using UnityEngine;

/// <summary>
/// �ڷ���Ʈ�� �߻������� ��ε�ĳ��Ʈ�ϴ� ���� ���.
/// - ī�޶� �� �����ڴ� �� �̺�Ʈ�� �޾� ���� �����ӿ� ���� ó��.
/// </summary>
public static class TeleportHub
{
    /// <param name="Transform">�ڷ���Ʈ�� ���(�ַ� Player)</param>
    public static event Action<Transform> OnTeleported;

    /// <summary>�ڷ���Ʈ �߻��� �˸�</summary>
    public static void Notify(Transform t)
    {
        OnTeleported?.Invoke(t);
    }
}
