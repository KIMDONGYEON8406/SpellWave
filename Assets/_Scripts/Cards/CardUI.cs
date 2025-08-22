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
    public GameObject skillStatsPanel;  // 스킬 스탯 표시 패널
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

        // 등급 테두리 색상
        if (rarityBorder != null)
        {
            rarityBorder.color = cardData.rarityColor;
        }

        // 카드 타입별 색상 조정
        if (cardData.cardType == CardType.SkillCard && cardData.skillToAdd != null)
        {
            var element = CloakManager.Instance?.GetCurrentElement() ?? ElementType.Energy;
            Color elementColor = SkillNameGenerator.GetElementColor(element);

            // 배경에 속성 색상 적용 (약하게)
            if (cardBackground != null)
            {
                cardBackground.color = Color.Lerp(Color.white, elementColor, 0.3f);
            }
        }
    }

    private void SetupCardContent()
    {
        // 아이콘
        if (cardIcon != null && cardData.cardIcon != null)
        {
            cardIcon.sprite = cardData.cardIcon;
        }

        // 제목
        if (cardTitle != null)
        {
            cardTitle.text = GetCardTitle();
        }

        // 설명
        if (cardDescription != null)
        {
            cardDescription.text = GetFormattedDescription();
        }

        // 스킬 카드면 스탯 표시
        if (cardData.cardType == CardType.SkillCard)
        {
            ShowSkillStats();
        }
        else if (skillStatsPanel != null)
        {
            skillStatsPanel.SetActive(false);
        }
    }

    private string GetCardTitle()
    {
        if (cardData.cardType == CardType.SkillCard && cardData.skillToAdd != null)
        {
            var element = CloakManager.Instance?.GetCurrentElement() ?? ElementType.Energy;
            return cardData.skillToAdd.GetDisplayName(element);
        }
        else
        {
            return cardData.cardName;
        }
    }

    private string GetFormattedDescription()
    {
        switch (cardData.cardType)
        {
            case CardType.StatCard:
                return GetStatCardDescription();

            case CardType.SkillCard:
                return GetSkillCardDescription();

            default:
                return cardData.description;
        }
    }

    private string GetStatCardDescription()
    {
        string desc = cardData.description;

        // 스탯 증가량 표시
       //string statName = GetStatName(cardData.statType);
       //desc += $"\n\n<b>{statName} +{cardData.increasePercentage}%</b>";

        // 등급 표시
        desc += GetRarityText();

        return desc;
    }

    private string GetSkillCardDescription()
    {
        if (cardData.skillToAdd == null)
            return cardData.description;

        var skill = cardData.skillToAdd;
        var element = CloakManager.Instance?.GetCurrentElement() ?? ElementType.Energy;

        // 스킬 설명 + 속성 효과
        string desc = skill.GetCardDescription(element);

        // 이미 보유한 스킬인지 체크
        var skillManager = GameManager.Instance?.player?.GetComponent<SkillManager>();
        if (skillManager != null)
        {
            var existingSkill = skillManager.GetSkill(skill.baseSkillType);
            if (existingSkill != null)
            {
                desc += $"\n\n<color=yellow>⚠️ 이미 보유 중 (Lv.{existingSkill.currentLevel} → Lv.{existingSkill.currentLevel + 1})</color>";
            }
        }

        return desc;
    }

    private void ShowSkillStats()
    {
        if (skillStatsPanel == null || cardData.skillToAdd == null)
            return;

        skillStatsPanel.SetActive(true);

        var skill = cardData.skillToAdd;

        if (damageText != null)
            damageText.text = $"DMG: {skill.baseDamage}";

        if (cooldownText != null)
            cooldownText.text = $"CD: {skill.baseCooldown}s";

        if (rangeText != null)
            rangeText.text = $"RNG: {skill.baseRange}m";
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
        Debug.Log($"카드 호버: {GetCardTitle()}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    private void OnCardClicked()
    {
        Debug.Log($"카드 선택: {GetCardTitle()}");
        CardManager.Instance.SelectCard(cardData);
        StartCoroutine(SelectionEffect());
    }

    private IEnumerator SelectionEffect()
    {
        // 선택 애니메이션
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