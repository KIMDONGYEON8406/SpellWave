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

        // 버튼 이벤트 등록
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }

    void Update()
    {
        // 호버 효과 애니메이션
        Vector3 targetScale = isHovered ? hoverScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale,
                                           hoverSpeed * Time.unscaledDeltaTime);
    }

    // 카드 데이터로 UI 초기화
    public void Initialize(CardData data, int index)
    {
        cardData = data;
        cardIndex = index;

        SetupCardVisuals();
        SetupCardContent();
    }

    // 카드 비주얼 설정
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
    }

    // 카드 내용 설정
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
            cardTitle.text = cardData.cardName;
        }

        // 설명
        if (cardDescription != null)
        {
            cardDescription.text = GetFormattedDescription();
        }
    }

    // 카드 설명 포맷팅 (스탯 수치 포함)
    private string GetFormattedDescription()
    {
        string description = cardData.description;

        if (cardData.cardType == CardType.StatCard)
        {
            description += $"\n+{cardData.increasePercentage}% 증가";
        }

        return description;
    }

    // 마우스 호버 이벤트
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        Debug.Log($"카드 호버: {cardData.cardName}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    // 카드 클릭 이벤트
    private void OnCardClicked()
    {
        Debug.Log($"카드 선택: {cardData.cardName}");

        // CardManager에게 선택 알림
        CardManager.Instance.SelectCard(cardData);

        // 선택 효과 (선택사항)
        StartCoroutine(SelectionEffect());
    }

    // 선택 효과 애니메이션
    private IEnumerator SelectionEffect()
    {
        // 카드가 빛나는 효과 등
        float duration = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            // 선택 효과 애니메이션 (예: 색상 변경, 스케일 변경 등)
            yield return null;
        }
    }
}