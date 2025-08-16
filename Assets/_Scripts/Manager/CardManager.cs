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

    // 플레이어가 이미 가진 스킬들 추적
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
        // 매번 cardSelectionUI 체크 (안전장치)
        if (cardSelectionUI == null)
        {
            cardSelectionUI = FindObjectOfType<CardSelectionUI>();
            Debug.Log("CardSelectionUI 다시 찾음");
        }

        List<CardData> randomCards = GetRandomCards(cardsToShow);

        if (randomCards.Count == 0)
        {
            Debug.LogError("사용 가능한 카드가 없습니다!");
            return;
        }

        if (cardSelectionUI == null)
        {
            Debug.LogError("CardSelectionUI를 찾을 수 없습니다!");
            return;
        }

        cardSelectionUI.DisplayCards(randomCards);
        Debug.Log($"카드 {randomCards.Count}장 표시");
    }

    private List<CardData> GetRandomCards(int count)
    {
        List<CardData> availableCards = GetAvailableCards();
        List<CardData> selectedCards = new List<CardData>();

        int actualCount = Mathf.Min(count, availableCards.Count);

        for (int i = 0; i < actualCount; i++)
        {
            if (availableCards.Count == 0) break;

            int randomIndex = Random.Range(0, availableCards.Count);
            selectedCards.Add(availableCards[randomIndex]);
            availableCards.RemoveAt(randomIndex);
        }

        return selectedCards;
    }

    private List<CardData> GetAvailableCards()
    {
        List<CardData> availableCards = new List<CardData>();

        if (ShouldShowSkillCards())
        {
            foreach (CardData skillCard in allSkillCards)
            {
                if (skillCard.skillToAdd != null && !playerSkills.Contains(skillCard.skillToAdd))
                {
                    availableCards.Add(skillCard);
                }
            }
            Debug.Log($"스킬 카드 풀: {availableCards.Count}장");
        }
        else
        {
            availableCards.AddRange(allStatCards);
            Debug.Log($"스탯 카드 풀: {availableCards.Count}장");
        }

        return availableCards;
    }

    private bool ShouldShowSkillCards()
    {
        return GameManager.Instance.currentWave % 3 == 0;
    }

    public void SelectCard(CardData selectedCard)
    {
        Debug.Log($"카드 선택됨: {selectedCard.cardName}");

        ApplyCardEffect(selectedCard);
        GameManager.Instance.OnCardSelected(selectedCard);
        cardSelectionUI.HideCards();
    }

    private void ApplyCardEffect(CardData card)
    {
        switch (card.cardType)
        {
            case CardType.StatCard:
                ApplyStatCard(card);
                break;
            case CardType.SkillCard:
                ApplySkillCard(card);
                break;
        }
    }

    private void ApplyStatCard(CardData card)
    {
        Character player = GameManager.Instance.player;
        if (player == null)
        {
            Debug.LogError("플레이어를 찾을 수 없습니다!");
            return;
        }

        // PlayerStats 방식으로 스탯 증가 적용
        switch (card.statType)
        {
            case StatType.AttackPower:
                player.IncreaseAttackPower(card.increasePercentage);
                break;
            case StatType.MoveSpeed:
                player.IncreaseMoveSpeed(card.increasePercentage);
                break;
            case StatType.AttackRange:
                player.IncreaseAttackRange(card.increasePercentage);
                break;
            case StatType.Health:
                player.IncreaseHealth(card.increasePercentage);
                break;
            case StatType.AttackSpeed:
                player.IncreaseAttackSpeed(card.increasePercentage);
                break;
        }

        Debug.Log($"{card.statType} {card.increasePercentage}% 증가 적용!");
    }

    private void ApplySkillCard(CardData card)
    {
        Character player = GameManager.Instance.player;
        if (player == null)
        {
            Debug.LogError("플레이어를 찾을 수 없습니다!");
            return;
        }

        SkillManager skillManager = player.GetComponent<SkillManager>();
        if (skillManager == null)
        {
            Debug.LogError("SkillManager를 찾을 수 없습니다!");
            return;
        }

        bool success = skillManager.AddSkillFromData(card.skillToAdd);

        if (success)
        {
            playerSkills.Add(card.skillToAdd);
            Debug.Log($"스킬 획득: {card.skillToAdd.skillName}");
        }
        else
        {
            Debug.LogWarning($"스킬 추가 실패: {card.skillToAdd.skillName}");
        }
    }

    public void ResetPlayerSkills()
    {
        playerSkills.Clear();
        Debug.Log("플레이어 스킬 목록 초기화");
    }

    // 디버그용 메서드들
    public void PrintAvailableCards()
    {
        List<CardData> available = GetAvailableCards();
        Debug.Log($"사용 가능한 카드: {available.Count}장");
        foreach (CardData card in available)
        {
            Debug.Log($"- {card.cardName} ({card.cardType})");
        }
    }
}