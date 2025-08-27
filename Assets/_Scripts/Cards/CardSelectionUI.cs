using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CardSelectionUI : MonoBehaviour
{
    [Header("UI 패널")]
    public GameObject cardSelectionPanel;
    public CanvasGroup cardPanelCanvasGroup; // 페이드 효과용

    [Header("카드 컨테이너")]
    public Transform cardContainer;
    public GameObject cardPrefab;

    [Header("배경 및 제목")]
    public Text titleText;
    public Button backgroundButton; // 카드 외부 클릭 방지용

    [Header("애니메이션 설정")]
    public float fadeInDuration = 0.3f;
    public float cardAppearDelay = 0.1f;

    private List<GameObject> currentCardObjects = new List<GameObject>();

    void Start()
    {
        // 초기 상태 설정
        if (cardSelectionPanel != null)
        {
            cardSelectionPanel.SetActive(false);
        }

        // 배경 버튼 설정 (클릭 방지)
        if (backgroundButton != null)
        {
            backgroundButton.onClick.AddListener(() => {
                DebugManager.LogUI("카드를 선택해주세요!");
            });
        }
    }

    // 카드들 표시
    public void DisplayCards(List<CardData> cards)
    {
        cardSelectionPanel.SetActive(true);

        StartCoroutine(DisplayCardsCoroutine(cards));
    }

    // DisplayCardsCoroutine에서
    private IEnumerator DisplayCardsCoroutine(List<CardData> cards)
    {
        ClearCards();
        // 패널 활성화
        cardSelectionPanel.SetActive(true);
        Time.timeScale = 0f;

        // 제목 설정 (Title Text가 있을 때만)
        if (titleText != null)
        {
            SetTitle(cards);
        }

        // 페이드 인 효과 (Canvas Group이 있을 때만)
        if (cardPanelCanvasGroup != null)
        {
            yield return StartCoroutine(FadeIn());
        }

        // 카드 생성
        for (int i = 0; i < cards.Count; i++)
        {
            if (cardPrefab != null)
            {
                CreateCard(cards[i], i);
            }
            yield return new WaitForSecondsRealtime(cardAppearDelay);
        }
    }

    // 제목 설정
    private void SetTitle(List<CardData> cards)
    {
        if (titleText == null) return;

        if (cards.Count > 0 && cards[0].cardType == CardType.SkillCard)
        {
            titleText.text = "새로운 스킬을 선택하세요!";
        }
        else
        {
            titleText.text = "능력을 강화하세요!";
        }
    }

    // 페이드 인 효과
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

    // 개별 카드 생성
    private void CreateCard(CardData cardData, int index)
    {
        // cardContainer를 정확히 지정
        GameObject cardObj = Instantiate(cardPrefab, cardContainer);

        //Debug.Log($"카드 생성 위치: {cardContainer.name}"); // 디버그 추가

        CardUI cardUI = cardObj.GetComponent<CardUI>();

        if (cardUI != null)
        {
            cardUI.Initialize(cardData, index);
        }

        currentCardObjects.Add(cardObj);
    }

    // 카드 선택 완료 후 UI 닫기
    public void HideCards()
    {
        StartCoroutine(HideCardsCoroutine());
    }

    private IEnumerator HideCardsCoroutine()
    {
        // 페이드 아웃 효과 (선택사항)
        yield return new WaitForSecondsRealtime(0.2f);

        // 카드들 제거
        ClearCards();

        // 패널 비활성화
        cardSelectionPanel.SetActive(false);

        // 시간 재개
        Time.timeScale = 1f;

        DebugManager.LogUI("카드 선택 UI 닫힘");
    }

    // 기존 카드들 제거
    private void ClearCards()
    {
        // 더 확실한 제거
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

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