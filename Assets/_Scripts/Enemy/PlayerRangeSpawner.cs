using UnityEngine;

/*
[목적]
- 벽/아레나 좌표 없이, "플레이어 위치 기준 원형 범위(도넛)"에서 적을 스폰한다.
- 초당 스폰률을 적산(accumulator)하여 다량 스폰을 지원한다.
- 동시 생존 수 cap, 프레임당 생성 상한으로 급스파이크를 방지한다.
- 스폰 시 플레이어를 향해 보게(회전) 하며, 필요 시 카메라 정면 팝인 방지도 지원한다.
- 풀링(EnemyPool)과 EnemyAI(풀링 대응) 구조와 완전히 호환된다.

[인스펙터]
- player                : 플레이어 Transform (필수)
- pool                  : EnemyPool (필수)
- maxAlive              : 동시 출현 수 상한(예: 300)
- maxSpawnsPerFrame     : 1프레임 최대 스폰 수(예: 20)
- minRadiusFromPlayer   : 플레이어로부터 최소 반지름(예: 6)
- maxRadiusFromPlayer   : 플레이어로부터 최대 반지름(예: 16~24)
- snapToGround          : 바닥에 Raycast로 붙일지 여부
- groundMask            : 바닥 레이어(예: Ground)
- raycastHeight         : 레이 시작 높이(플레이어 위에서 쏘면 안전)
- ySpawnOffset          : snapToGround가 false일 때 Y 보정치
- avoidFrontOfCamera    : 카메라 정면 팝인 방지(뒤/측면 위주로 스폰)
- cameraTransform       : 메인 카메라 Transform (avoidFrontOfCamera true면 할당)
- manualRateOverride    : 0 이상이면 수동 스폰률(초당), -1이면 GameManager.GetCurrentSpawnRate() 사용

[연결 순서]
1) Empty → "PlayerRangeSpawner" 생성 → 본 스크립트 부착
2) player = Player, pool = EnemyPool 드래그
3) manualRateOverride를 테스트 시 30~60 등으로 설정(혹은 -1로 두고 GameManager API 사용)
4) 필요 시 avoidFrontOfCamera = true, cameraTransform = Main Camera
5) 기존 EnemySpawner는 비활성화/삭제

[주의]
- 플레이어/적 Rigidbody는 Interpolate 권장(대량 스폰 시 시각 끊김 완화).
- EnemyManager가 있다면 AliveCount/Count 전용 메서드 사용이 성능상 유리.
*/

public class PlayerRangeSpawner : MonoBehaviour
{
    [Header("필수 참조")]
    public Transform player;
    public EnemyPool pool;

    [Header("동시 수 / 프레임 상한")]
    [Tooltip("동시에 존재 가능한 적 최대 수")]
    public int maxAlive = 300;
    [Tooltip("한 프레임에 생성 가능한 최대 수(프레임 급스파이크 방지)")]
    public int maxSpawnsPerFrame = 20;

    [Header("플레이어 중심 스폰 범위(도넛)")]
    [Tooltip("플레이어로부터 최소 반지름")]
    public float minRadiusFromPlayer = 6f;
    [Tooltip("플레이어로부터 최대 반지름")]
    public float maxRadiusFromPlayer = 18f;

    [Header("바닥 붙이기(선택)")]
    public bool snapToGround = false;
    public LayerMask groundMask;
    [Tooltip("스폰 위치 상공에서 아래로 쏠 레이 시작 높이")]
    public float raycastHeight = 5f;

    [Header("Y 보정(ground snap 미사용 시)")]
    public float ySpawnOffset = 0f;

    [Header("카메라 정면 팝인 방지(선택)")]
    public bool avoidFrontOfCamera = false;
    public Transform cameraTransform;
    [Tooltip("카메라 정면 회피 시 최대 시도 횟수(무한루프 방지)")]
    public int maxAngleTries = 8;

    [Header("스폰률(초당)")]
    [Tooltip("0 이상이면 수동 스폰률 사용, -1이면 GameManager.GetCurrentSpawnRate() 사용")]
    public float manualRateOverride = -1f;

    private float _acc;

    void Update()
    {
        if (player == null || pool == null) return;
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameState.Playing) return;

        float rate = GetRatePerSec();
        _acc += rate * Time.deltaTime;

        int spawnedThisFrame = 0;
        while (_acc >= 1f && spawnedThisFrame < maxSpawnsPerFrame)
        {
            if (!TrySpawnOne()) break;
            _acc -= 1f;
            spawnedThisFrame++;
        }
    }

    private float GetRatePerSec()
    {
        if (manualRateOverride >= 0f) return manualRateOverride;
        if (GameManager.Instance != null) return GameManager.Instance.GetCurrentSpawnRate();
        return 1f; // 안전 기본값
    }

    private bool TrySpawnOne()
    {
        int alive = CountAliveEnemies();
        if (alive >= maxAlive) return false;

        // 플레이어 중심 원형 범위에서 위치 샘플링
        Vector3 pos = SamplePositionAroundPlayer();

        // 바닥 스냅 또는 Y 보정
        if (snapToGround)
        {
            Vector3 start = pos + Vector3.up * raycastHeight;
            if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
                pos = hit.point;
            else
                pos.y = player.position.y + ySpawnOffset; // 실패 시 안전 대체
        }
        else
        {
            pos.y = player.position.y + ySpawnOffset;
        }

        // 플레이어를 바라보는 초기 회전
        Vector3 to = player.position - pos; to.y = 0f;
        if (to.sqrMagnitude < 0.0001f) to = Vector3.forward;
        Quaternion rot = Quaternion.LookRotation(to.normalized);

        pool.Get(pos, rot);
        return true;
    }

    private int CountAliveEnemies()
    {
        if (EnemyManager.instance != null)
        {
            // EnemyManager에 카운트 util이 있다면 그것 사용(성능 유리)
            // return EnemyManager.instance.AliveCount;
            return EnemyManager.instance.GetAllEnemiesCountSafe();
        }
        return FindObjectsOfType<EnemyAI>().Length; // 최후의 대체(비용 큼)
    }

    private Vector3 SamplePositionAroundPlayer()
    {
        // 플레이어 기준 랜덤 각/반지름 샘플
        int tries = 0;
        while (true)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(minRadiusFromPlayer, maxRadiusFromPlayer);

            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Vector3 candidate = player.position + offset;

            // 카메라 정면 팝인 회피: 카메라 전방과 offset의 내적이 0보다 크면 정면
            if (avoidFrontOfCamera && cameraTransform != null)
            {
                Vector3 dirFromPlayer = (candidate - player.position).normalized;
                float dot = Vector3.Dot(cameraTransform.forward.normalized, dirFromPlayer);
                if (dot > 0f) // 정면(0~1) → 피한다
                {
                    tries++;
                    if (tries < maxAngleTries) continue; // 재시도
                }
            }

            return candidate;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;
        // 바깥 원
        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.DrawWireDisc(player.position, Vector3.up, maxRadiusFromPlayer);
        // 안쪽 원
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(player.position, Vector3.up, minRadiusFromPlayer);
    }
#endif
}
