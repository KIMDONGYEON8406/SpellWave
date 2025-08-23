using UnityEngine;

public class ExplosionGizmoDrawer : MonoBehaviour
{
    [Header("기즈모 설정")]
    public float gizmoDuration = 1f;  // 기즈모 표시 시간
    public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.3f);  // 주황색 반투명

    private Vector3 explosionPosition;
    private float explosionRadius;
    private float explosionTime;
    private bool showGizmo;

    public void ShowExplosionGizmo(Vector3 position, float radius)
    {
        explosionPosition = position;
        explosionRadius = radius;
        explosionTime = Time.time;
        showGizmo = true;
    }

    void OnDrawGizmos()
    {
        if (showGizmo && Time.time - explosionTime < gizmoDuration)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(explosionPosition, explosionRadius);

            // 내부도 약간 채우기
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, gizmoColor.a * 0.3f);
            Gizmos.DrawSphere(explosionPosition, explosionRadius);
        }
        else
        {
            showGizmo = false;
        }
    }
}