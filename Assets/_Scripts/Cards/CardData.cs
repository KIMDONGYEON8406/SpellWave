using UnityEngine;

[CreateAssetMenu(fileName = "New Card Data", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("ī�� �⺻ ����")]
    public string cardName;
    [TextArea(2, 4)]
    public string description;
    public Sprite cardIcon;
    public Sprite cardBackground;

    [Header("ī�� Ÿ��")]
    public CardType cardType;

    [Header("���� ī��� (StatCard�� ���� ���)")]
    public StatType statType;
    public float increasePercentage = 10f; // �⺻ 10% ����

    [Header("��ų ī��� (SkillCard�� ���� ���)")]
    public SkillData skillToAdd;

    [Header("ī�� ���")]
    public CardRarity rarity = CardRarity.Common;
    public Color rarityColor = Color.white;
}

// ī�� ���� Enum��
public enum CardType
{
    StatCard,    // ���� ���� ī��
    SkillCard    // ��ų ȹ�� ī��
}

public enum StatType
{
    AttackPower,    // ���ݷ�
    MoveSpeed,      // �̵��ӵ�
    AttackRange,    // ���� ����
    Health,         // ü��
    AttackSpeed     // ���� �ӵ�
}

public enum CardRarity
{
    Common,     // �Ϲ�
    Rare,       // ���
    Epic,       // ����
    Legendary   // ����
}