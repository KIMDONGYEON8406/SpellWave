using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatEffect", menuName = "Game/Card Effects/Player Stat")]
public class PlayerStatEffect : CardEffect
{
    [Header("적용할 스탯")]
    public StatType targetStat;
    public bool isPercentage = true;

    [Header("디버그")]
    [SerializeField] private bool showDetailedLog = false;

    public override void ApplyEffect(Player player, float value)
    {
        if (player == null)
        {
            DebugManager.LogError(LogCategory.Card, "Player is null!");
            return;
        }

        PlayerStats stats = player.GetPlayerStats();
        if (stats == null)
        {
            DebugManager.LogError(LogCategory.Card, "PlayerStats not found!");
            return;
        }

        switch (targetStat)
        {
            case StatType.PlayerHealth:
                float oldMax = stats.maxHP;
                stats.maxHP *= (1f + value / 100f);
                stats.currentHP += (stats.maxHP - oldMax);
                DebugManager.LogCard($"최대 체력 +{value}% ({oldMax:F0} → {stats.maxHP:F0})");
                break;

            case StatType.PlayerMoveSpeed:
                float oldSpeed = stats.moveSpeed;
                stats.moveSpeed *= (1f + value / 100f);
                DebugManager.LogCard($"이동속도 +{value}% ({oldSpeed:F1} → {stats.moveSpeed:F1})");
                break;

            case StatType.PlayerAttackPower:
                float oldPower = stats.attackPower;
                stats.attackPower *= (1f + value / 100f);
                DebugManager.LogCard($"공격력 +{value}% ({oldPower:F0} → {stats.attackPower:F0})");
                break;

            case StatType.InstantHeal:
                float healAmount = stats.maxHP * (value / 100f);
                float beforeHeal = stats.currentHP;
                stats.currentHP = Mathf.Min(stats.currentHP + healAmount, stats.maxHP);
                DebugManager.LogCard($"체력 회복 {healAmount:F0} ({beforeHeal:F0} → {stats.currentHP:F0})");
                break;

            default:
                DebugManager.Log(LogCategory.Card, $"처리되지 않은 스탯 타입: {targetStat}", LogLevel.Warning);
                break;
        }
    }

    public override string GetPreviewText(float value)
    {
        string statName = GetStatDisplayName();

        if (targetStat == StatType.InstantHeal)
        {
            return $"최대 체력의 {value}% 즉시 회복";
        }

        return isPercentage ? $"{statName} +{value}%" : $"{statName} +{value}";
    }

    private string GetStatDisplayName()
    {
        switch (targetStat)
        {
            case StatType.PlayerHealth: return "최대 체력";
            case StatType.PlayerMoveSpeed: return "이동 속도";
            case StatType.PlayerAttackPower: return "공격력";
            case StatType.InstantHeal: return "체력 회복";
            default: return targetStat.ToString();
        }
    }

    public override bool CanApply(Player player)
    {
        if (targetStat == StatType.InstantHeal)
        {
            var stats = player.GetPlayerStats();
            if (stats != null)
            {
                bool canHeal = stats.currentHP < stats.maxHP;
                if (!canHeal && showDetailedLog)
                {
                    DebugManager.LogCard("체력이 이미 최대치입니다");
                }
                return canHeal;
            }
        }
        return true;
    }
}