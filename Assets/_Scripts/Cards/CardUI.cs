using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("카드 UI 요소")]
    public Image cardBackground;
    public Image cardIcon;
    public Text cardTitle;
    public Text cardDescription;
    public Button cardButton;
    public Image rarityBorder;

    [Header("스킬 카드 전용")]
    public GameObject skillStatsPanel;
    public Text skillTypeText;
    public Text damageText;
    public Text cooldownText;
    public Text rangeText;

    [Header("호버 효과")]
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);
    public float hoverSpeed = 5f;

    private CardData cardData;
    private int cardIndex;
    private Vector3 originalScale;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;

        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }

    void Update()
    {
        Vector3 targetScale = isHovered ? hoverScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale,
                                           hoverSpeed * Time.unscaledDeltaTime);
    }

    public void Initialize(CardData data, int index)
    {
        cardData = data;
        cardIndex = index;

        SetupCardVisuals();
        SetupCardContent();
    }

    private void SetupCardVisuals()
    {
        // 배경 이미지
        if (cardBackground != null && cardData.cardBackground != null)
        {
            cardBackground.sprite = cardData.cardBackground;
        }

        // 아이콘
        if (cardIcon != null && cardData.cardIcon != null)
        {
            cardIcon.sprite = cardData.cardIcon;
        }

        // 등급 테두리 색상
        if (rarityBorder != null)
        {
            rarityBorder.color = cardData.rarityColor;
        }
    }

    private void SetupCardContent()
    {
        // 제목
        if (cardTitle != null)
        {
            cardTitle.text = cardData.cardName;
        }

        // 설명 - 새 시스템 사용
        if (cardDescription != null)
        {
            cardDescription.text = GetFormattedDescription();
        }

        // 스킬 카드인 경우 스킬 정보 표시
        ShowSkillInfoIfNeeded();
    }

    private string GetFormattedDescription()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(cardData.description);

        // 카드 효과 표시
        if (cardData.cardEffects != null && cardData.cardEffects.Count > 0)
        {
            sb.AppendLine();
            foreach (var entry in cardData.cardEffects)
            {
                if (entry.effect != null)
                {
                    sb.AppendLine($"<b>{entry.effect.GetPreviewText(entry.value)}</b>");
                }
            }
        }

        // 등급 표시
        sb.Append(GetRarityText());

        return sb.ToString();
    }

    private void ShowSkillInfoIfNeeded()
    {
        // 스킬 획득 효과가 있는지 확인
        SkillData targetSkill = null;

        foreach (var entry in cardData.cardEffects)
        {
            if (entry.effect is SkillAcquireEffect skillEffect)
            {
                targetSkill = skillEffect.skillToAdd;
                break;
            }
        }

        if (targetSkill != null && skillStatsPanel != null)
        {
            skillStatsPanel.SetActive(true);

            // 스킬 타입 표시
            if (skillTypeText != null)
            {
                skillTypeText.text = $"타입: {targetSkill.GetTypeDescription()}";
            }

            // 스킬 스탯 표시
            if (damageText != null)
                damageText.text = $"DMG: {targetSkill.baseDamage}";

            if (cooldownText != null)
                cooldownText.text = $"CD: {targetSkill.baseCooldown}s";

            if (rangeText != null)
                rangeText.text = $"RNG: {targetSkill.baseRange}m";

            // 이미 보유한 스킬인지 체크
            var skillManager = GameManager.Instance?.player?.GetComponent<SkillManager>();
            if (skillManager != null)
            {
                var existingSkill = skillManager.GetSkill(targetSkill.baseSkillType);
                if (existingSkill != null)
                {
                    if (cardDescription != null)
                    {
                        cardDescription.text += $"\n\n<color=yellow>이미 보유 중 (Lv.{existingSkill.currentLevel})</color>";
                    }
                }
            }
        }
        else if (skillStatsPanel != null)
        {
            skillStatsPanel.SetActive(false);
        }
    }

    private string GetRarityText()
    {
        switch (cardData.rarity)
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    private void OnCardClicked()
    {
        DebugManager.LogCard($"카드 선택: {cardData.cardName}");
        CardManager.Instance.SelectCard(cardData);
        StartCoroutine(SelectionEffect());
    }

    private IEnumerator SelectionEffect()
    {
        float duration = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float scale = 1f + Mathf.Sin(elapsedTime * 10f) * 0.1f;
            transform.localScale = originalScale * scale;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}