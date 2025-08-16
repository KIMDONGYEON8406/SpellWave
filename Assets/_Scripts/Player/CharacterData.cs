using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Character Data", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("ĳ���� ����")]
    public string characterName;
    public Sprite portrait;
    public GameObject characterPrefab;
    [TextArea(2, 4)]
    public string description;

    [Header("�⺻ ����")]
    public float baseHealth = 100f;
    public float baseAttackPower = 50f;
    public float baseMoveSpeed = 5f;
    public float baseAttackRange = 3f;
    public float baseAttackSpeed = 1f;

    [Header("���� ��ų")]
    public SkillData basicAttackSkill;
    public SkillData passiveSkill;

    [Header("��� ������ ��ų��")]
    public List<SkillData> availableSkills = new List<SkillData>();
}