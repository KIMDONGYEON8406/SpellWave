using UnityEngine;

[CreateAssetMenu(fileName = "New Cloak", menuName = "SpellWave/Cloak Data")]
public class CloakData : ScriptableObject
{
    [Header("망토 정보")]
    public string cloakName = "에너지 망토";
    public Sprite cloakIcon;
    public Color cloakColor = Color.white;
    [TextArea(2, 4)]
    public string description = "기본 망토입니다.";

    [Header("속성 타입")]
    public ElementType elementType = ElementType.Energy;

    [Header("패시브 효과")]
    public PassiveEffect passiveEffect;

    [Header("스탯 보너스")]
    [Range(0, 50)]
    public float healthBonus = 0f;      // 체력 증가 %
    [Range(0, 50)]
    public float attackBonus = 0f;      // 공격력 증가 %
    [Range(0, 50)]
    public float moveSpeedBonus = 0f;   // 이동속도 증가 %
}

// 속성 타입
public enum ElementType
{
    Energy,     // 에너지 (기본)
    Fire,       // 화염
    Ice,        // 얼음
    Lightning,  // 번개
    Poison,     // 독
    Dark,       // 어둠
    Light       // 빛
}

// 패시브 효과 구조체
[System.Serializable]
public class PassiveEffect
{
    [Header("효과 타입")]
    public PassiveType type = PassiveType.None;

    [Header("효과 수치")]
    public float effectValue = 0f;      // 효과 강도 (데미지, 확률 등)
    public float duration = 0f;         // 지속 시간
    public float tickInterval = 0f;     // 틱 간격 (도트 데미지용)

    [Header("추가 효과")]
    public bool hasChainEffect = false;  // 연쇄 효과
    public int chainCount = 0;           // 연쇄 횟수
    public float chainDamageRatio = 0.5f; // 연쇄 데미지 비율
}

public enum PassiveType
{
    None,           // 효과 없음
    Burn,           // 화상 (도트)
    Slow,           // 둔화
    Chain,          // 연쇄
    Poison,         // 중독
    LifeSteal,      // 흡혈
    CriticalBoost   // 치명타
}