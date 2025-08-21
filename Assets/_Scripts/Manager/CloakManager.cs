using UnityEngine;
using System.Collections.Generic;

public class CloakManager : MonoBehaviour
{
    public static CloakManager Instance { get; private set; }

    [Header("현재 장착 망토")]
    public CloakData currentCloak;

    [Header("보유 망토 목록")]
    public List<CloakData> unlockedCloaks = new List<CloakData>();

    private Character player;

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
        // 초기 망토 설정
        if (currentCloak != null && !unlockedCloaks.Contains(currentCloak))
        {
            UnlockCloak(currentCloak);
        }
    }

    // 망토 해금
    public void UnlockCloak(CloakData cloak)
    {
        if (!unlockedCloaks.Contains(cloak))
        {
            unlockedCloaks.Add(cloak);
            Debug.Log($"새 망토 해금: {cloak.cloakName}");
        }
    }

    // 망토 장착
    public void EquipCloak(CloakData newCloak)
    {
        if (newCloak == null || !unlockedCloaks.Contains(newCloak))
        {
            Debug.LogError("해금되지 않은 망토입니다!");
            return;
        }

        currentCloak = newCloak;
        ApplyCloakStats();

        Debug.Log($"망토 장착: {newCloak.cloakName}");
    }

    // 망토 스탯 적용
    private void ApplyCloakStats()
    {
        if (player == null)
        {
            player = GameManager.Instance.player;
        }

        if (player == null || currentCloak == null) return;

        // 스탯 보너스 적용 (PlayerStats에 반영)
        // 기존 보너스 제거하고 새로 적용하는 로직 필요
        Debug.Log($"망토 스탯 적용: 체력+{currentCloak.healthBonus}%, 공격력+{currentCloak.attackBonus}%");
    }

    // 현재 망토의 속성 가져오기
    public ElementType GetCurrentElement()
    {
        return currentCloak != null ? currentCloak.elementType : ElementType.Energy;
    }

    // 현재 망토의 패시브 효과 가져오기
    public PassiveEffect GetCurrentPassive()
    {
        return currentCloak != null ? currentCloak.passiveEffect : new PassiveEffect();
    }
}