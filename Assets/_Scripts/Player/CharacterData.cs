using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Character Data", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("캐릭터 정보")]
    public string characterName;
    public Sprite portrait;
    public GameObject characterPrefab;
    [TextArea(2, 4)]
    public string description;

    [Header("기본 스탯")]
    public float baseHealth = 100f;
    public float baseAttackPower = 50f;
    public float baseMoveSpeed = 5f;
    public float baseAttackRange = 3f;
    public float baseAttackSpeed = 1f;

    [Header("고유 스킬")]
    public SkillData basicAttackSkill;
    public SkillData passiveSkill;

    [Header("사용 가능한 스킬들")]
    public List<SkillData> availableSkills = new List<SkillData>();
}