using UnityEngine;

[System.Serializable]
public class PassiveEffect
{
    public PassiveType type = PassiveType.None;

    [Header("효과 수치")]
    public float effectValue = 0f;      // 효과 수치 (데미지, 감속률 등)
    public float duration = 3f;          // 지속 시간

    [Header("연쇄 효과 (번개용)")]
    public int chainCount = 0;           // 연쇄 횟수
    public float chainRange = 5f;        // 연쇄 범위
    public float chainDamageRatio = 0.5f; // 연쇄 데미지 비율

    [Header("추가 효과")]
    public bool stackable = false;       // 중첩 가능 여부
    public int maxStacks = 1;           // 최대 중첩 수
}

public enum PassiveType
{
    None,       // 없음
    Burn,       // 화상 (도트)
    Slow,       // 둔화
    Freeze,     // 빙결
    Chain,      // 연쇄
    Poison,     // 중독
    LifeSteal,  // 흡혈
    Critical    // 치명타
}