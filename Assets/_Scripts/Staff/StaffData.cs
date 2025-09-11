using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Staff", menuName = "SpellWave/Staff Data")]
public class StaffData : ScriptableObject
{
    [Header("지팡이 정보")]
    public string staffName = "원소마스터 지팡이";
    public Sprite staffIcon;
    public GameObject staffModelPrefab;
    [TextArea(2, 4)]
    public string description = "기본 지팡이입니다.";

    [Header("지팡이 등급")]
    public StaffRarity rarity = StaffRarity.Common;

    [Header("패시브 효과")]
    public string passiveName = "엘리멘탈 마스터";
    [TextArea(2, 4)]
    public string passiveDescription = "마법의 기본 원소 공격을 합니다";
    public PassiveEffect passiveEffect;

    [Header("기본 원소 설정 (추후 구현)")]
    public ElementType primaryElement = ElementType.Energy;
    public ElementType[] availableElements = new ElementType[]
    {
        ElementType.Fire,    // 불
        ElementType.Ice,     // 물/얼음
        ElementType.Lightning, // 번개
        ElementType.Poison,  // 독/땅
        ElementType.Light    // 빛/바람
    };

    [Header("초기 제공 스킬 (5개)")]
    [Tooltip("지팡이 획득 시 기본 제공되는 스킬")]
    public List<SkillData> defaultSkills = new List<SkillData>(5);

    [Header("획득 가능 스킬 풀")]
    [Tooltip("이 지팡이로 획득 가능한 모든 스킬")]
    public List<SkillData> availableSkillPool = new List<SkillData>();

    // 현재 선택된 원소 가져오기 (나중에 구현)
    public ElementType GetCurrentElement()
    {
        return primaryElement;
    }

    // 현재 패시브 효과 가져오기
    public PassiveEffect GetPassiveEffect()
    {
        return passiveEffect;
    }
}

public enum StaffRarity
{
    Common,     // 일반
    Rare,       // 희귀
    Epic,       // 에픽
    Legendary   // 전설
}