using UnityEngine;

[CreateAssetMenu(fileName = "New Cloak", menuName = "SpellWave/Cloak Data")]
public class CloakData : ScriptableObject
{
    [Header("망토 정보")]
    public string cloakName = "에너지 망토";
    public Sprite cloakIcon;
    public GameObject cloakModelPrefab; // 3D 모델 (선택)
    [TextArea(2, 4)]
    public string description = "기본 망토입니다.";

    [Header("망토 등급")]
    public CloakRarity rarity = CloakRarity.Common;

    [Header("속성")]
    public ElementType elementType = ElementType.Energy;

    [Header("패시브 효과")]
    public PassiveEffect passiveEffect;

    [Header("스탯 보너스")]
    [Range(0, 100)]
    public float healthBonus = 0f;      // 체력 보너스 %
    [Range(0, 100)]
    public float attackBonus = 0f;      // 공격력 보너스 %
    [Range(0, 100)]
    public float defenseBonus = 0f;     // 방어력 보너스 %
    [Range(0, 100)]
    public float speedBonus = 0f;       // 이동속도 보너스 %
}

public enum CloakRarity
{
    Common,     // 일반
    Rare,       // 희귀  
    Epic,       // 에픽
    Legendary   // 전설
}

public enum ElementType
{
    Energy,     // 에너지 (보라)
    Fire,       // 화염 (빨강)
    Ice,        // 얼음 (파랑)
    Lightning,  // 번개 (노랑)
    Poison,     // 독 (초록)
    Dark,       // 어둠 (검정)
    Light       // 빛 (하양)
}