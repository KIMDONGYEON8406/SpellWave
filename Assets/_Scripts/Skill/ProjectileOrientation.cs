using UnityEngine;

public class ProjectileOrientation : MonoBehaviour
{
    [Header("발사체 방향 설정")]
    public bool isVertical = true;  // 세로형 발사체인지
    public Vector3 rotationOffset = new Vector3(90, 0, 0);  // 추가 회전값

    [Header("디버그")]
    public bool showDebugRay = true;

    void OnDrawGizmosSelected()
    {
        if (showDebugRay)
        {
            // 발사체 전방 표시
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);

            // 발사체 상단 표시
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.up * 1f);
        }
    }
}