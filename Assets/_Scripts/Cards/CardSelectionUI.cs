using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CardSelectionUI : MonoBehaviour
{
    [Header("UI �г�")]
    public GameObject cardSelectionPanel;
    public CanvasGroup cardPanelCanvasGroup; // ���̵� ȿ����

    [Header("ī�� �����̳�")]
    public Transform cardContainer;
    public GameObject cardPrefab;

    [Header("��� �� ����")]
    public Text titleText;
    public Button backgroundButton; // ī�� �ܺ� Ŭ�� ������

    [Header("�ִϸ��̼� ����")]
    public float fadeInDuration = 0.3f;
    public float cardAppearDelay = 0.1f;

    private List<GameObject> currentCardObjects = new List<GameObject>();

    void Start()
    {
        // �ʱ� ���� ����
        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(false);
        }

        // ��� ��ư ���� (Ŭ�� ����)
        if (backgroundButton != null)
        {
            backgroundButton.onClick.AddListener(() => {
                Debug.Log("ī�带 �������ּ���!");
            });
        }
    }

    // ī��� ǥ��
    public void DisplayCards(List<CardData> cards)
    {
        cardSelectionPanel.SetActive(true);

        StartCoroutine(DisplayCardsCoroutine(cards));
    }

    // DisplayCardsCoroutine����
    private IEnumerator DisplayCardsCoroutine(List<CardData> cards)
    {
        ClearCards();
        // �г� Ȱ��ȭ
        cardSelectionPanel.SetActive(true);
        Time.timeScale = 0f;

        // ���� ���� (Title Text�� ���� ����)
        if (titleText != null)
        {
            SetTitle(cards);
        }

        // ���̵� �� ȿ�� (Canvas Group�� ���� ����)
        if (cardPanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeIn());
        }

        // ī�� ����
        for (int i = 0; i < cards.Count; i++)
        {
            if (cardPrefab != null)
            {
                CreateCard(cards[i], i);
            }
            yield return new WaitForSecondsRealtime(cardAppearDelay);
        }
    }

    // ���� ����
    private void SetTitle(List<CardData> cards)
    {
        if (titleText == null) return;

        if (cards.Count > 0 && cards[0].cardType == CardType.SkillCard)
        {
            titleText.text = "���ο� ��ų�� �����ϼ���!";
        }
        else
        {
            titleText.text = "�ɷ��� ��ȭ�ϼ���!";
        }
    }

    // ���̵� �� ȿ��
    private IEnumerator FadeIn()
    {
        if (cardPanelCanvasGroup == null) yield break;

        cardPanelCanvasGroup.alpha = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            cardPanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }

        cardPanelCanvasGroup.alpha = 1f;
    }

    // ���� ī�� ����
    private void CreateCard(CardData cardData, int index)
    {
        // cardContainer�� ��Ȯ�� ����
        GameObject cardObj = Instantiate(cardPrefab, cardContainer);

        Debug.Log($"ī�� ���� ��ġ: {cardContainer.name}"); // ����� �߰�

        CardUI cardUI = cardObj.GetComponent<CardUI>();

        if (cardUI != null)
        {
            cardUI.Initialize(cardData, index);
        }

        currentCardObjects.Add(cardObj);
    }

    // ī�� ���� �Ϸ� �� UI �ݱ�
    public void HideCards()
    {
        StartCoroutine(HideCardsCoroutine());
    }

    private IEnumerator HideCardsCoroutine()
    {
        // ���̵� �ƿ� ȿ�� (���û���)
        yield return new WaitForSecondsRealtime(0.2f);

        // ī��� ����
        ClearCards();

        // �г� ��Ȱ��ȭ
        cardSelectionPanel.SetActive(false);

        // �ð� �簳
        Time.timeScale = 1f;

        Debug.Log("ī�� ���� UI ����");
    }

    // ���� ī��� ����
    private void ClearCards()
    {
        foreach (GameObject cardObj in currentCardObjects)
        {
            if (cardObj != null)
            {
                Destroy(cardObj);
            }
        }
        currentCardObjects.Clear();
    }
}