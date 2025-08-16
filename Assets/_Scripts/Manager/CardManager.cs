using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("ī�� Ǯ ����")]
    public List<CardData> allStatCards = new List<CardData>();
    public List<CardData> allSkillCards = new List<CardData>();

    [Header("ī�� ���� ����")]
    public int cardsToShow = 3;

    [Header("UI ����")]
    public CardSelectionUI cardSelectionUI;

    // �÷��̾ �̹� ���� ��ų�� ����
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
        // �Ź� cardSelectionUI üũ (������ġ)
        if (cardSelectionUI == null)
        {
            cardSelectionUI = FindObjectOfType<CardSelectionUI>();
            Debug.Log("CardSelectionUI �ٽ� ã��");
        }

        List<CardData> randomCards = GetRandomCards(cardsToShow);

        if (randomCards.Count == 0)
        {
            Debug.LogError("��� ������ ī�尡 �����ϴ�!");
            return;
        }

        if (cardSelectionUI == null)
        {
            Debug.LogError("CardSelectionUI�� ã�� �� �����ϴ�!");
            return;
        }

        cardSelectionUI.DisplayCards(randomCards);
        Debug.Log($"ī�� {randomCards.Count}�� ǥ��");
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
            Debug.Log($"��ų ī�� Ǯ: {availableCards.Count}��");
        }
        else
        {
            availableCards.AddRange(allStatCards);
            Debug.Log($"���� ī�� Ǯ: {availableCards.Count}��");
        }

        return availableCards;
    }

    private bool ShouldShowSkillCards()
    {
        return GameManager.Instance.currentWave % 3 == 0;
    }

    public void SelectCard(CardData selectedCard)
    {
        Debug.Log($"ī�� ���õ�: {selectedCard.cardName}");

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
            Debug.LogError("�÷��̾ ã�� �� �����ϴ�!");
            return;
        }

        // PlayerStats ������� ���� ���� ����
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

        Debug.Log($"{card.statType} {card.increasePercentage}% ���� ����!");
    }

    private void ApplySkillCard(CardData card)
    {
        Character player = GameManager.Instance.player;
        if (player == null)
        {
            Debug.LogError("�÷��̾ ã�� �� �����ϴ�!");
            return;
        }

        SkillManager skillManager = player.GetComponent<SkillManager>();
        if (skillManager == null)
        {
            Debug.LogError("SkillManager�� ã�� �� �����ϴ�!");
            return;
        }

        bool success = skillManager.AddSkillFromData(card.skillToAdd);

        if (success)
        {
            playerSkills.Add(card.skillToAdd);
            Debug.Log($"��ų ȹ��: {card.skillToAdd.skillName}");
        }
        else
        {
            Debug.LogWarning($"��ų �߰� ����: {card.skillToAdd.skillName}");
        }
    }

    public void ResetPlayerSkills()
    {
        playerSkills.Clear();
        Debug.Log("�÷��̾� ��ų ��� �ʱ�ȭ");
    }

    // ����׿� �޼����
    public void PrintAvailableCards()
    {
        List<CardData> available = GetAvailableCards();
        Debug.Log($"��� ������ ī��: {available.Count}��");
        foreach (CardData card in available)
        {
            Debug.Log($"- {card.cardName} ({card.cardType})");
        }
    }
}