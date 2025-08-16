using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("ī�� UI ���")]
    public Image cardBackground;
    public Image cardIcon;
    public Text cardTitle;
    public Text cardDescription;
    public Button cardButton;
    public Image rarityBorder;

    [Header("ȣ�� ȿ��")]
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);
    public float hoverSpeed = 5f;

    private CardData cardData;
    private int cardIndex;
    private Vector3 originalScale;
    private bool isHovered = false;

    void Start()
    {
        originalScale = transform.localScale;

        // ��ư �̺�Ʈ ���
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }

    void Update()
    {
        // ȣ�� ȿ�� �ִϸ��̼�
        Vector3 targetScale = isHovered ? hoverScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale,
                                           hoverSpeed * Time.unscaledDeltaTime);
    }

    // ī�� �����ͷ� UI �ʱ�ȭ
    public void Initialize(CardData data, int index)
    {
        cardData = data;
        cardIndex = index;

        SetupCardVisuals();
        SetupCardContent();
    }

    // ī�� ���־� ����
    private void SetupCardVisuals()
    {
        // ��� �̹���
        if (cardBackground != null && cardData.cardBackground != null)
        {
            cardBackground.sprite = cardData.cardBackground;
        }

        // ��� �׵θ� ����
        if (rarityBorder != null)
        {
            rarityBorder.color = cardData.rarityColor;
        }
    }

    // ī�� ���� ����
    private void SetupCardContent()
    {
        // ������
        if (cardIcon != null && cardData.cardIcon != null)
        {
            cardIcon.sprite = cardData.cardIcon;
        }

        // ����
        if (cardTitle != null)
        {
            cardTitle.text = cardData.cardName;
        }

        // ����
        if (cardDescription != null)
        {
            cardDescription.text = GetFormattedDescription();
        }
    }

    // ī�� ���� ������ (���� ��ġ ����)
    private string GetFormattedDescription()
    {
        string description = cardData.description;

        if (cardData.cardType == CardType.StatCard)
        {
            description += $"\n+{cardData.increasePercentage}% ����";
        }

        return description;
    }

    // ���콺 ȣ�� �̺�Ʈ
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        Debug.Log($"ī�� ȣ��: {cardData.cardName}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    // ī�� Ŭ�� �̺�Ʈ
    private void OnCardClicked()
    {
        Debug.Log($"ī�� ����: {cardData.cardName}");

        // CardManager���� ���� �˸�
        CardManager.Instance.SelectCard(cardData);

        // ���� ȿ�� (���û���)
        StartCoroutine(SelectionEffect());
    }

    // ���� ȿ�� �ִϸ��̼�
    private IEnumerator SelectionEffect()
    {
        // ī�尡 ������ ȿ�� ��
        float duration = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            // ���� ȿ�� �ִϸ��̼� (��: ���� ����, ������ ���� ��)
            yield return null;
        }
    }
}