using UnityEngine;
using System.Collections.Generic;

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

    [Header("카드 효과")]
    public List<CardEffectEntry> cardEffects = new List<CardEffectEntry>();

    [Header("카드 등급")]
    public CardRarity rarity = CardRarity.Common;
    public Color rarityColor = Color.white;

    [Header("디버그")]
    [SerializeField] private bool showApplyLogs = false;

    public void ApplyCardEffects(Player player)
    {
        if (cardEffects == null || cardEffects.Count == 0)
        {
            DebugManager.Log(LogCategory.Card, $"{cardName}에 효과가 설정되지 않음!", LogLevel.Warning);
            return;
        }

        DebugManager.LogImportant($"{cardName} 적용 시작");

        int appliedCount = 0;
        foreach (var entry in cardEffects)
        {
            if (entry.effect == null)
            {
                DebugManager.Log(LogCategory.Card, "효과가 null입니다!", LogLevel.Warning);
                continue;
            }

            if (entry.effect.CanApply(player))
            {
                entry.effect.ApplyEffect(player, entry.value);
                appliedCount++;

                if (showApplyLogs)
                {
                    DebugManager.LogCard($"  → {entry.effect.effectName} 적용");
                }
            }
            else
            {
                if (showApplyLogs)
                {
                    DebugManager.LogCard($"  → {entry.effect.effectName} 조건 미충족");
                }
            }
        }

        if (appliedCount > 0)
        {
            DebugManager.LogImportant($"{cardName} 완료 ({appliedCount}개 효과)");
        }
    }

    public string GetFullDescription()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(description);

        if (cardEffects.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("효과:");

            foreach (var entry in cardEffects)
            {
                if (entry.effect != null)
                {
                    sb.AppendLine($"- {entry.effect.GetPreviewText(entry.value)}");
                }
            }
        }

        sb.Append(GetRarityText());
        return sb.ToString();
    }

    private string GetRarityText()
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return "\n<color=white>[일반]</color>";
            case CardRarity.Rare:
                return "\n<color=#3366ff>[레어]</color>";
            case CardRarity.Epic:
                return "\n<color=#9933ff>[에픽]</color>";
            case CardRarity.Legendary:
                return "\n<color=#ff9900>[전설]</color>";
            default:
                return "";
        }
    }
}

// 이 클래스가 누락되었습니다! 추가해야 합니다
[System.Serializable]
public class CardEffectEntry
{
    [Tooltip("적용할 효과 (CardEffect ScriptableObject)")]
    public CardEffect effect;

    [Tooltip("효과 수치 (퍼센트 또는 레벨)")]
    public float value = 10f;

    [Tooltip("메모 (에디터용)")]
    public string note = "";
}