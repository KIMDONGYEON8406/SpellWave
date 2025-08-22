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
        //return GameManager.Instance.currentWave % 3 == 0;
        // [수정] Timeline 시스템용
        return true; // 임시로 항상 스킬카드 표시
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

        SkillManager skillManager = player.GetComponent<SkillManager>();
        if (skillManager == null)
        {
            Debug.LogError("SkillManager를 찾을 수 없습니다!");
            return;
        }

        // 모든 스킬에 스탯 보너스 적용
        var allSkills = skillManager.GetAllSkills();

        foreach (var skill in allSkills)
        {
            ApplyStatToSkill(skill, card.statType, card.increasePercentage);
        }

        Debug.Log($"{GetStatName(card.statType)} {card.increasePercentage}% 증가 적용!");
    }

    private void ApplyStatToSkill(SkillInstance skill, StatType statType, float percentage)
    {
        if (skill == null || skill.skillData == null) return;

        switch (statType)
        {
            // 전체 스킬 강화
            case StatType.AllSkillDamage:
                skill.damageMultiplier += (percentage / 100f);
                break;

            case StatType.AllSkillCooldown:
                skill.cooldownMultiplier *= (1f - percentage / 100f);
                break;

            case StatType.AllSkillRange:
                skill.rangeMultiplier += (percentage / 100f);
                break;

            // 단일 타겟 강화
            case StatType.SingleTargetDamage:
                if (skill.skillData.HasTag(SkillTag.SingleTarget))
                {
                    skill.damageMultiplier += (percentage / 100f);
                    Debug.Log($"{skill.skillData.baseSkillType} - 단일 타겟 강화 적용!");
                }
                break;

            // 다중 타겟 강화
            case StatType.MultiTargetDamage:
                if (skill.skillData.HasTag(SkillTag.MultiTarget))
                {
                    skill.damageMultiplier += (percentage / 100f);
                    Debug.Log($"{skill.skillData.baseSkillType} - 다중 타겟 강화 적용!");
                }
                break;

            // 발사체 강화
            case StatType.ProjectileDamage:
                if (skill.skillData.HasTag(SkillTag.Projectile))
                {
                    skill.damageMultiplier += (percentage / 100f);
                    Debug.Log($"{skill.skillData.baseSkillType} - 발사체 강화 적용!");
                }
                break;

            case StatType.ProjectileSpeed:
                if (skill.skillData.HasTag(SkillTag.Projectile))
                {
                    skill.projectileSpeedMultiplier += (percentage / 100f);
                    Debug.Log($"{skill.skillData.baseSkillType} - 발사체 속도 증가!");
                }
                break;

            // 범위 강화
            case StatType.AreaDamage:
                if (skill.skillData.HasTag(SkillTag.Area))
                {
                    skill.damageMultiplier += (percentage / 100f);
                    Debug.Log($"{skill.skillData.baseSkillType} - 범위 강화 적용!");
                }
                break;

            case StatType.AreaRange:
                if (skill.skillData.HasTag(SkillTag.Area))
                {
                    skill.rangeMultiplier += (percentage / 100f);
                    Debug.Log($"{skill.skillData.baseSkillType} - 범위 크기 증가!");
                }
                break;

            // 지속 강화
            case StatType.DOTDamage:
                if (skill.skillData.HasTag(SkillTag.DOT))
                {
                    skill.damageMultiplier += (percentage / 100f);
                    Debug.Log($"{skill.skillData.baseSkillType} - 지속 데미지 강화!");
                }
                break;

            case StatType.DOTDuration:
                if (skill.skillData.HasTag(SkillTag.DOT))
                {
                    skill.durationMultiplier += (percentage / 100f);
                    Debug.Log($"{skill.skillData.baseSkillType} - 지속 시간 증가!");
                }
                break;
        }
    }

    private string GetStatName(StatType type)
    {
        switch (type)
        {
            case StatType.AllSkillDamage: return "모든 스킬 데미지";
            case StatType.AllSkillCooldown: return "모든 스킬 쿨타임";
            case StatType.AllSkillRange: return "모든 스킬 범위";
            case StatType.SingleTargetDamage: return "단일 타겟 데미지";
            case StatType.MultiTargetDamage: return "다중 타겟 데미지";
            case StatType.ProjectileDamage: return "발사체 데미지";
            case StatType.ProjectileSpeed: return "발사체 속도";
            case StatType.AreaDamage: return "범위 공격 데미지";
            case StatType.AreaRange: return "범위 크기";
            case StatType.DOTDamage: return "지속 데미지";
            case StatType.DOTDuration: return "지속 시간";
            default: return type.ToString();
        }
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
            Debug.Log($"스킬 획득: {card.skillToAdd.baseSkillType}");
        }
        else
        {
            Debug.LogWarning($"스킬 추가 실패: {card.skillToAdd.baseSkillType}");
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