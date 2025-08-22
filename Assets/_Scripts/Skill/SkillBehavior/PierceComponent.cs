using UnityEngine;
using System.Collections.Generic;

public class PierceComponent : MonoBehaviour
{
    [Header("관통 설정")]
    public int maxPierceCount = 3;
    public float damageReductionPerPierce = 0.8f;  // 관통할 때마다 80%로 감소

    private int currentPierceCount = 0;
    private List<GameObject> piercedEnemies = new List<GameObject>();

    // 관통 가능 여부
    public bool CanPierce()
    {
        return currentPierceCount < maxPierceCount;
    }

    // 관통 처리
    public void OnPierce(GameObject enemy)
    {
        // 이미 관통한 적은 스킵
        if (piercedEnemies.Contains(enemy)) return;

        piercedEnemies.Add(enemy);
        currentPierceCount++;

        // 데미지 감소 적용 (필요한 경우)
        var projectile = GetComponent<ElementalProjectile>();
        if (projectile != null && damageReductionPerPierce < 1f)
        {
            // 데미지 감소 로직
            Debug.Log($"관통! 남은 관통 횟수: {maxPierceCount - currentPierceCount}");
        }

        // 최대 관통 도달 시 파괴
        if (!CanPierce())
        {
            Destroy(gameObject, 0.1f);
        }
    }
}