using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RuntimeStat
{
    public float baseValue;
    public float bonusValue;
    public float multiplier = 1f;

    public float FinalValue => (baseValue + bonusValue) * multiplier;
}

public class StatsSystem : MonoBehaviour
{
    private Dictionary<string, RuntimeStat> stats = new Dictionary<string, RuntimeStat>();

    public void InitializeStat(string statName, float baseValue)
    {
        if (!stats.ContainsKey(statName))
        {
            stats[statName] = new RuntimeStat { baseValue = baseValue };
        }
    }

    public void AddBonus(string statName, float bonus)
    {
        if (stats.ContainsKey(statName))
        {
            stats[statName].bonusValue += bonus;
        }
    }

    public void SetMultiplier(string statName, float multiplier)
    {
        if (stats.ContainsKey(statName))
        {
            stats[statName].multiplier = multiplier;
        }
    }

    public float GetStat(string statName)
    {
        return stats.ContainsKey(statName) ? stats[statName].FinalValue : 0f;
    }
}