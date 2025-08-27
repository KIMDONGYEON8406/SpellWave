using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("카드 풀 설정")]
    public List<CardData> allStatCards = new List<CardData>();
    public List<CardData> allSkillCards = new List<CardData>();

    [Header("카드 선택 설정")]
    public int cardsToShow = 3;

    [Header("UI 참조")]
    public CardSelectionUI cardSelectionUI;

    [Header("디버그")]
    [SerializeField] private bool detailedDebug = false;

    private HashSet<SkillData> playerSkills = new HashSet<SkillData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (cardSelectionUI == null)
        {
            cardSelectionUI = FindObjectOfType<CardSelectionUI>();
        }
    }

    public void ShowRandomCards()
    {
        if (cardSelectionUI == null)
        {
            cardSelectionUI = FindObjectOfType<CardSelectionUI>();
        }

        List<CardData> randomCards = GetRandomCards(cardsToShow);  // 이 메서드가 있어야 함

        if (randomCards.Count == 0)
        {
            DebugManager.LogError(LogCategory.Card, "사용 가능한 카드가 없습니다!");
            return;
        }

        if (cardSelectionUI == null)
        {
            DebugManager.LogError(LogCategory.UI, "CardSelectionUI를 찾을 수 없습니다!");
            return;
        }

        cardSelectionUI.DisplayCards(randomCards);
        DebugManager.LogCard($"카드 {randomCards.Count}장 표시");

        if (detailedDebug)
        {
            foreach (var card in randomCards)
            {
                DebugManager.LogCard($"  - {card.cardName}");
            }
        }
    }

    private List<CardData> GetRandomCards(int count)
    {
        List<CardData> availableCards = GetAvailableCards();
        List<CardData> selectedCards = new List<CardData>();

        // cardsToShow(3개) 제한 강제 적용
        int actualCount = Mathf.Min(count, availableCards.Count);
        actualCount = Mathf.Min(actualCount, cardsToShow);  // 이 줄 추가!

        for (int i = 0; i < actualCount; i++)
        {
            if (availableCards.Count == 0) break;

            int randomIndex = Random.Range(0, availableCards.Count);
            selectedCards.Add(availableCards[randomIndex]);
            availableCards.RemoveAt(randomIndex);
        }

        // 디버그 로그 추가
        if (selectedCards.Count != cardsToShow)
        {
            DebugManager.LogWarning(LogCategory.Card,
                $"카드 개수 불일치! 설정: {cardsToShow}, 실제: {selectedCards.Count}");
        }

        return selectedCards;
    }

    private List<CardData> GetAvailableCards()
    {
        List<CardData> availableCards = new List<CardData>();

        if (ShouldShowSkillCards())
        {
            // 레벨 4배수 - 스킬 카드
            var inventory = StaffManager.Instance.GetCurrentInventory();

            if (inventory != null)
            {
                var unequipped = inventory.GetUnequippedSkills();

                foreach (var skill in unequipped)
                {
                    var skillCard = allSkillCards.Find(c =>
                    {
                        foreach (var effect in c.cardEffects)
                        {
                            if (effect.effect is SkillAcquireEffect sae)
                            {
                                return sae.skillToAdd == skill;
                            }
                        }
                        return false;
                    });

                    if (skillCard != null)
                    {
                        availableCards.Add(skillCard);
                    }
                }

                if (detailedDebug)
                {
                    DebugManager.LogCard($"선택 가능한 스킬 카드: {availableCards.Count}개");
                }
            }
        }
        else
        {
            // 일반 레벨 - 스탯 카드
            availableCards.AddRange(allStatCards);
            if (detailedDebug)
            {
                DebugManager.LogCard($"스탯 카드 풀: {availableCards.Count}개");
            }
        }

        return availableCards;
    }

    private bool ShouldShowSkillCards()
    {
        return GameManager.Instance.currentLevel % 4 == 0;
    }

    public void SelectCard(CardData selectedCard)
    {
        DebugManager.LogImportant($"카드 선택: {selectedCard.cardName}");

        Player player = GameManager.Instance.player;
        if (player == null)
        {
            DebugManager.LogError(LogCategory.Card, "플레이어를 찾을 수 없습니다!");
            return;
        }

        // 새로운 효과 시스템으로 적용
        selectedCard.ApplyCardEffects(player);

        // 게임 매니저에 알림
        GameManager.Instance.OnCardSelected(selectedCard);

        // UI 닫기
        cardSelectionUI.HideCards();
    }

    public void ResetPlayerSkills()
    {
        playerSkills.Clear();
        if (detailedDebug)
        {
            DebugManager.LogCard("플레이어 스킬 목록 초기화");
        }
    }

    // 디버그용 메서드
    public void PrintAvailableCards()
    {
        List<CardData> available = GetAvailableCards();
        DebugManager.LogSeparator("사용 가능한 카드");
        DebugManager.LogCard($"총 {available.Count}장");
        foreach (CardData card in available)
        {
            DebugManager.LogCard($"- {card.cardName} ({card.cardType})");
        }
    }
}