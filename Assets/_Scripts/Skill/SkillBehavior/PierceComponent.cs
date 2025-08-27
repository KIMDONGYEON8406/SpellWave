using System.Collections.Generic;
using UnityEngine;

public class PierceComponent : MonoBehaviour
{
    [Header("관통 설정")]
    public int maxPierceCount = 3;
    public float damageReductionPerPierce = 0.8f;

    private int currentPierceCount = 0;
    private List<GameObject> piercedEnemies = new List<GameObject>();

    public bool CanPierce()
    {
        return currentPierceCount < maxPierceCount;
    }

    // 남은 관통 횟수를 가져오는 프로퍼티 추가!
    public int RemainingPierceCount
    {
        get { return maxPierceCount - currentPierceCount; }
    }

    public bool HasPierced(GameObject enemy)
    {
        return piercedEnemies.Contains(enemy);
    }

    public void OnPierce(GameObject enemy)
    {
        if (piercedEnemies.Contains(enemy)) return;

        piercedEnemies.Add(enemy);
        currentPierceCount++;

        DebugManager.LogSkill($"[Pierce] 관통 {currentPierceCount}/{maxPierceCount}");

        var projectile = GetComponent<ElementalProjectile>();
        if (projectile != null && damageReductionPerPierce < 1f)
        {
            // 데미지 감소 로직
        }
    }
}