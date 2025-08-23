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
            // 스킬 카드 - 아직 장착 안 한 스킬만
            var inventory = StaffManager.Instance.GetCurrentInventory();

            if (inventory != null)
            {
                // 장착 안 한 스킬들 가져오기
                var unequipped = inventory.GetUnequippedSkills();

                foreach (var skill in unequipped)
                {
                    // 해당 스킬의 카드 찾기
                    var skillCard = allSkillCards.Find(c => c.skillToAdd == skill);
                    if (skillCard != null)
                    {
                        availableCards.Add(skillCard);
                    }
                }

                Debug.Log($"선택 가능한 스킬 카드: {availableCards.Count}개");
            }
        }
        else
        {
            // 스탯 카드
            availableCards.AddRange(allStatCards);
            Debug.Log($"스탯 카드 풀: {availableCards.Count}개");
        }

        return availableCards;
    }

    private bool ShouldShowSkillCards()
    {
        // 4레벨마다 스킬 카드
        return GameManager.Instance.currentLevel % 4 == 0;
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
                ApplySkillStatCard(card);
                break;
        }
    }

    private void ApplyStatCard(CardData card)
    {
        // 플레이어 스탯인지 체크
        switch (card.statType)
        {
            case StatType.PlayerHealth:
            case StatType.PlayerMoveSpeed:
            case StatType.PlayerAttackPower:
                ApplyPlayerStatCard(card);
                break;
            default:
                ApplySkillStatCard(card);
                break;
        }
    }

    private void ApplySkillStatCard(CardData card)
    {
        Player player = GameManager.Instance.player;
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
    // 플레이어 스탯 적용 (새 메서드)
    private void ApplyPlayerStatCard(CardData card)
    {
        Player player = GameManager.Instance.player;
        if (player == null)
        {
            Debug.LogError("플레이어를 찾을 수 없습니다!");
            return;
        }

        PlayerStats stats = player.GetPlayerStats();
        if (stats == null)
        {
            Debug.LogError("PlayerStats를 찾을 수 없습니다!");
            return;
        }

        switch (card.statType)
        {
            case StatType.PlayerHealth:
                float oldMax = stats.maxHP;
                stats.maxHP *= (1f + card.increasePercentage / 100f);
                stats.currentHP += (stats.maxHP - oldMax); // 증가분만큼 현재 체력도 증가
                Debug.Log($"최대 체력 {card.increasePercentage}% 증가! ({oldMax} → {stats.maxHP})");
                break;

            case StatType.PlayerMoveSpeed:
                float oldSpeed = stats.moveSpeed;
                stats.moveSpeed *= (1f + card.increasePercentage / 100f);
                Debug.Log($"이동속도 {card.increasePercentage}% 증가! ({oldSpeed} → {stats.moveSpeed})");
                break;

            case StatType.PlayerAttackPower:
                float oldPower = stats.attackPower;
                stats.attackPower *= (1f + card.increasePercentage / 100f);
                Debug.Log($"공격력 {card.increasePercentage}% 증가! ({oldPower} → {stats.attackPower})");
                break;
        }
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
            //case StatType.SingleTargetDamage:
            //    if (skill.skillData.HasTag(SkillTag.SingleTarget))
            //    {
            //        skill.damageMultiplier += (percentage / 100f);
            //        Debug.Log($"{skill.skillData.baseSkillType} - 단일 타겟 강화 적용!");
            //    }
            //    break;

            // 다중 타겟 강화
            //case StatType.MultiTargetDamage:
            //    if (skill.skillData.HasTag(SkillTag.MultiTarget))
            //    {
            //        skill.damageMultiplier += (percentage / 100f);
            //        Debug.Log($"{skill.skillData.baseSkillType} - 다중 타겟 강화 적용!");
            //    }
            //    break;

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
            // 플레이어 스탯 추가
            case StatType.PlayerHealth: return "최대 체력";
            case StatType.PlayerMoveSpeed: return "이동 속도";
            case StatType.PlayerAttackPower: return "공격력";

            // 기존 스킬 스탯
            case StatType.AllSkillDamage: return "모든 스킬 데미지";
            case StatType.AllSkillCooldown: return "모든 스킬 쿨타임";
            case StatType.AllSkillRange: return "모든 스킬 범위";
            //case StatType.SingleTargetDamage: return "단일 타겟 데미지";
            //case StatType.MultiTargetDamage: return "다중 타겟 데미지";
            case StatType.ProjectileDamage: return "발사체 데미지";
            case StatType.ProjectileSpeed: return "발사체 속도";
            case StatType.AreaDamage: return "범위 공격 데미지";
            case StatType.AreaRange: return "범위 크기";
            case StatType.DOTDamage: return "지속 데미지";
            case StatType.DOTDuration: return "지속 시간";
            default: return type.ToString();
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