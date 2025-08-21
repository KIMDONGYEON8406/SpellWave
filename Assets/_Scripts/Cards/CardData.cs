using UnityEngine;

[CreateAssetMenu(fileName = "New Card Data", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("카드 기본 정보")]
    public string cardName;
    [TextArea(2, 4)]
    public string description;
    public Sprite cardIcon;
    public Sprite cardBackground;

    [Header("카드 타입")]
    public CardType cardType;

    [Header("스탯 카드용 (StatCard일 때만 사용)")]
    public StatType statType;
    public float increasePercentage = 10f; // 기본 10% 증가

    [Header("스킬 카드용 (SkillCard일 때만 사용)")]
    public SkillData skillToAdd;

    [Header("카드 등급")]
    public CardRarity rarity = CardRarity.Common;
    public Color rarityColor = Color.white;
}

// 카드 관련 Enum들
public enum CardType
{
    StatCard,    // 스탯 증가 카드
    SkillCard    // 스킬 획득 카드
}

public enum StatType
{
    AttackPower,    // 공격력
    MoveSpeed,      // 이동속도
    AttackRange,    // 공격 범위
    Health,         // 체력
    AttackSpeed     // 공격 속도
}

public enum CardRarity
{
    Common,     // 일반
    Rare,       // 희귀
    Epic,       // 에픽
    Legendary   // 전설
}