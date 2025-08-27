using UnityEngine;
using System.Collections.Generic;

public class CloakManager : MonoBehaviour
{
    public static CloakManager Instance { get; private set; }

    [Header("현재 장착 망토")]
    public CloakData currentCloak;

    [Header("보유 망토 목록")]
    public List<CloakData> unlockedCloaks = new List<CloakData>();

    private Player player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 초기 망토 설정 (에너지 망토)
        if (currentCloak != null && !unlockedCloaks.Contains(currentCloak))
        {
            UnlockCloak(currentCloak);
        }
    }

    public void UnlockCloak(CloakData cloak)
    {
        if (!unlockedCloaks.Contains(cloak))
        {
            unlockedCloaks.Add(cloak);
            DebugManager.LogSystem($"새 망토 해금: {cloak.cloakName}");
        }
    }

    public void EquipCloak(CloakData newCloak)
    {
        if (newCloak == null || !unlockedCloaks.Contains(newCloak))
        {
            DebugManager.LogError(LogCategory.System, "해금되지 않은 망토입니다!");
            return;
        }

        currentCloak = newCloak;
        ApplyCloakStats();

        DebugManager.LogSystem($"망토 장착: {newCloak.cloakName} (속성: {newCloak.elementType})");
    }

    private void ApplyCloakStats()
    {
        if (player == null)
        {
            player = GameManager.Instance.player;
        }

        if (player == null || currentCloak == null) return;

        // 스탯 보너스는 나중에 구현
        DebugManager.LogSystem($"망토 효과 적용: {currentCloak.elementType} 속성");
    }

    public ElementType GetCurrentElement()
    {
        return currentCloak != null ? currentCloak.elementType : ElementType.Energy;
    }

    public PassiveEffect GetCurrentPassive()
    {
        return currentCloak != null ? currentCloak.passiveEffect : new PassiveEffect();
    }
}