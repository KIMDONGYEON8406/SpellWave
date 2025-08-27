using System.Collections.Generic;
using UnityEngine;

public class ExplosionGizmoDrawer : MonoBehaviour
{
    [Header("기즈모 설정")]
    public float gizmoDuration = 2f;
    public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.3f);

    private static ExplosionGizmoDrawer instance;
    private List<ExplosionGizmo> activeGizmos = new List<ExplosionGizmo>();

    private class ExplosionGizmo
    {
        public Vector3 position;
        public float radius;
        public float createTime;
    }

    void Awake()
    {
        instance = this;
    }

    public static void ShowExplosion(Vector3 position, float radius)
    {
        if (instance != null)
        {
            instance.activeGizmos.Add(new ExplosionGizmo
            {
                position = position,
                radius = radius,
                createTime = Time.time
            });

            Debug.Log($"[Gizmo] 폭발 범위 표시: {radius:F1}m");
        }
    }

    void OnDrawGizmos()
    {
        activeGizmos.RemoveAll(g => Time.time - g.createTime > gizmoDuration);

        foreach (var gizmo in activeGizmos)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(gizmo.position, gizmo.radius);

            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, gizmoColor.a * 0.3f);
            Gizmos.DrawSphere(gizmo.position, gizmo.radius);
        }
    }
}