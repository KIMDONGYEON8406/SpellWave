using UnityEngine;

/*
[����]
- ��/�Ʒ��� ��ǥ ����, "�÷��̾� ��ġ ���� ���� ����(����)"���� ���� �����Ѵ�.
- �ʴ� �������� ����(accumulator)�Ͽ� �ٷ� ������ �����Ѵ�.
- ���� ���� �� cap, �����Ӵ� ���� �������� �޽�����ũ�� �����Ѵ�.
- ���� �� �÷��̾ ���� ����(ȸ��) �ϸ�, �ʿ� �� ī�޶� ���� ���� ������ �����Ѵ�.
- Ǯ��(EnemyPool)�� EnemyAI(Ǯ�� ����) ������ ������ ȣȯ�ȴ�.

[�ν�����]
- player                : �÷��̾� Transform (�ʼ�)
- pool                  : EnemyPool (�ʼ�)
- maxAlive              : ���� ���� �� ����(��: 300)
- maxSpawnsPerFrame     : 1������ �ִ� ���� ��(��: 20)
- minRadiusFromPlayer   : �÷��̾�κ��� �ּ� ������(��: 6)
- maxRadiusFromPlayer   : �÷��̾�κ��� �ִ� ������(��: 16~24)
- snapToGround          : �ٴڿ� Raycast�� ������ ����
- groundMask            : �ٴ� ���̾�(��: Ground)
- raycastHeight         : ���� ���� ����(�÷��̾� ������ ��� ����)
- ySpawnOffset          : snapToGround�� false�� �� Y ����ġ
- avoidFrontOfCamera    : ī�޶� ���� ���� ����(��/���� ���ַ� ����)
- cameraTransform       : ���� ī�޶� Transform (avoidFrontOfCamera true�� �Ҵ�)
- manualRateOverride    : 0 �̻��̸� ���� ������(�ʴ�), -1�̸� GameManager.GetCurrentSpawnRate() ���

[���� ����]
1) Empty �� "PlayerRangeSpawner" ���� �� �� ��ũ��Ʈ ����
2) player = Player, pool = EnemyPool �巡��
3) manualRateOverride�� �׽�Ʈ �� 30~60 ������ ����(Ȥ�� -1�� �ΰ� GameManager API ���)
4) �ʿ� �� avoidFrontOfCamera = true, cameraTransform = Main Camera
5) ���� EnemySpawner�� ��Ȱ��ȭ/����

[����]
- �÷��̾�/�� Rigidbody�� Interpolate ����(�뷮 ���� �� �ð� ���� ��ȭ).
- EnemyManager�� �ִٸ� AliveCount/Count ���� �޼��� ����� ���ɻ� ����.
*/

public class PlayerRangeSpawner : MonoBehaviour
{
    [Header("�ʼ� ����")]
    public Transform player;
    public EnemyPool pool;

    [Header("���� �� / ������ ����")]
    [Tooltip("���ÿ� ���� ������ �� �ִ� ��")]
    public int maxAlive = 300;
    [Tooltip("�� �����ӿ� ���� ������ �ִ� ��(������ �޽�����ũ ����)")]
    public int maxSpawnsPerFrame = 20;

    [Header("�÷��̾� �߽� ���� ����(����)")]
    [Tooltip("�÷��̾�κ��� �ּ� ������")]
    public float minRadiusFromPlayer = 6f;
    [Tooltip("�÷��̾�κ��� �ִ� ������")]
    public float maxRadiusFromPlayer = 18f;

    [Header("�ٴ� ���̱�(����)")]
    public bool snapToGround = false;
    public LayerMask groundMask;
    [Tooltip("���� ��ġ ������� �Ʒ��� �� ���� ���� ����")]
    public float raycastHeight = 5f;

    [Header("Y ����(ground snap �̻�� ��)")]
    public float ySpawnOffset = 0f;

    [Header("ī�޶� ���� ���� ����(����)")]
    public bool avoidFrontOfCamera = false;
    public Transform cameraTransform;
    [Tooltip("ī�޶� ���� ȸ�� �� �ִ� �õ� Ƚ��(���ѷ��� ����)")]
    public int maxAngleTries = 8;

    [Header("������(�ʴ�)")]
    [Tooltip("0 �̻��̸� ���� ������ ���, -1�̸� GameManager.GetCurrentSpawnRate() ���")]
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
        return 1f; // ���� �⺻��
    }

    private bool TrySpawnOne()
    {
        int alive = CountAliveEnemies();
        if (alive >= maxAlive) return false;

        // �÷��̾� �߽� ���� �������� ��ġ ���ø�
        Vector3 pos = SamplePositionAroundPlayer();

        // �ٴ� ���� �Ǵ� Y ����
        if (snapToGround)
        {
            Vector3 start = pos + Vector3.up * raycastHeight;
            if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
                pos = hit.point;
            else
                pos.y = player.position.y + ySpawnOffset; // ���� �� ���� ��ü
        }
        else
        {
            pos.y = player.position.y + ySpawnOffset;
        }

        // �÷��̾ �ٶ󺸴� �ʱ� ȸ��
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
            // EnemyManager�� ī��Ʈ util�� �ִٸ� �װ� ���(���� ����)
            // return EnemyManager.instance.AliveCount;
            return EnemyManager.instance.GetAllEnemiesCountSafe();
        }
        return FindObjectsOfType<EnemyAI>().Length; // ������ ��ü(��� ŭ)
    }

    private Vector3 SamplePositionAroundPlayer()
    {
        // �÷��̾� ���� ���� ��/������ ����
        int tries = 0;
        while (true)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(minRadiusFromPlayer, maxRadiusFromPlayer);

            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Vector3 candidate = player.position + offset;

            // ī�޶� ���� ���� ȸ��: ī�޶� ����� offset�� ������ 0���� ũ�� ����
            if (avoidFrontOfCamera && cameraTransform != null)
            {
                Vector3 dirFromPlayer = (candidate - player.position).normalized;
                float dot = Vector3.Dot(cameraTransform.forward.normalized, dirFromPlayer);
                if (dot > 0f) // ����(0~1) �� ���Ѵ�
                {
                    tries++;
                    if (tries < maxAngleTries) continue; // ��õ�
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
        // �ٱ� ��
        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.DrawWireDisc(player.position, Vector3.up, maxRadiusFromPlayer);
        // ���� ��
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(player.position, Vector3.up, minRadiusFromPlayer);
    }
#endif
}
