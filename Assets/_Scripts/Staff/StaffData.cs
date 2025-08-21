using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Staff", menuName = "SpellWave/Staff Data")]
public class StaffData : ScriptableObject
{
    [Header("지팡이 정보")]
    public string staffName = "나무 지팡이";
    public Sprite staffIcon;
    public GameObject staffModelPrefab; // 3D 모델 (선택)
    [TextArea(2, 4)]
    public string description = "기본 지팡이입니다.";

    [Header("지팡이 등급")]
    public StaffRarity rarity = StaffRarity.Common;

    [Header("초기 제공 스킬 (5개)")]
    [Tooltip("지팡이 획득 시 기본 제공되는 스킬")]
    public List<SkillData> defaultSkills = new List<SkillData>(5);

    [Header("획득 가능 스킬 풀")]
    [Tooltip("이 지팡이로 획득 가능한 모든 스킬")]
    public List<SkillData> availableSkillPool = new List<SkillData>();
}

public enum StaffRarity
{
    Common,     // 일반
    Rare,       // 희귀
    Epic,       // 에픽
    Legendary   // 전설
}