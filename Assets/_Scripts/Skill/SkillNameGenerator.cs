using System.Collections.Generic;
using UnityEngine;

public static class SkillNameGenerator
{
    // 스킬 기본 이름 매핑
    private static Dictionary<string, string> baseSkillNames = new Dictionary<string, string>
    {
        // 기본 5개 스킬
        { "Bolt", "볼" },
        { "Arrow", "애로우" },
        { "Explosion", "익스플로전" },
        { "Aura", "오라" },        // ⭐ 추가됨 (FrostNova → Aura)
        { "Missile", "미사일" },
        
        // 나중에 추가할 스킬들
        { "RapidShot", "래피드샷" },
        { "Meteor", "메테오" },
        { "ChainLightning", "체인 라이트닝" },
        { "Turret", "터렛" },
        { "Shield", "실드" }
    };

    // 속성 이름 매핑
    private static Dictionary<ElementType, string> elementNames = new Dictionary<ElementType, string>
    {
        { ElementType.Energy, "에너지" },
        { ElementType.Fire, "파이어" },
        { ElementType.Ice, "아이스" },
        { ElementType.Lightning, "라이트닝" },
        { ElementType.Poison, "포이즌" },
        { ElementType.Dark, "다크" },
        { ElementType.Light, "홀리" }
    };

    // 속성 색상 매핑
    private static Dictionary<ElementType, Color> elementColors = new Dictionary<ElementType, Color>
    {
        { ElementType.Energy, new Color(0.5f, 0f, 1f) },      // 보라색
        { ElementType.Fire, Color.red },                       // 빨간색
        { ElementType.Ice, Color.cyan },                       // 하늘색
        { ElementType.Lightning, Color.yellow },               // 노란색
        { ElementType.Poison, Color.green },                   // 초록색
        { ElementType.Dark, new Color(0.2f, 0f, 0.2f) },     // 어두운 보라
        { ElementType.Light, Color.white }                     // 흰색
    };

    public static string GetSkillName(string baseSkillType, ElementType element)
    {
        // 원소별 접두어
        string prefix = element switch
        {
            ElementType.Fire => "화염",
            ElementType.Water => "물의",
            ElementType.Wind => "바람의",
            ElementType.Earth => "대지의",
            ElementType.Lightning => "번개",
            ElementType.Ice => "얼음",
            _ => ""
        };

        // 스킬명 변환
        string skillName = baseSkillType switch
        {
            "Bolt" => "볼트",
            "Arrow" => "화살",
            "Missile" => "미사일",
            "Explosion" => "폭발",
            "Aura" => "오라",
            _ => baseSkillType
        };

        return string.IsNullOrEmpty(prefix) ? skillName : $"{prefix} {skillName}";
    }

    // 속성 이름만 가져오기
    public static string GetElementName(ElementType element)
    {
        return elementNames.ContainsKey(element) ? elementNames[element] : "에너지";
    }

    // 기본 스킬 이름만 가져오기
    public static string GetBaseSkillName(string baseType)
    {
        return baseSkillNames.ContainsKey(baseType) ? baseSkillNames[baseType] : baseType;
    }

    // 속성 색상 가져오기
    public static Color GetElementColor(ElementType element)
    {
        return elementColors.ContainsKey(element) ? elementColors[element] : Color.white;
    }

    // 속성 설명 가져오기 (카드 시스템용)
    public static string GetElementDescription(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return "화상 효과를 추가합니다";
            case ElementType.Ice: return "둔화 효과를 추가합니다";
            case ElementType.Lightning: return "연쇄 공격을 추가합니다";
            case ElementType.Poison: return "중독 효과를 추가합니다";
            case ElementType.Dark: return "생명력을 흡수합니다";
            case ElementType.Light: return "치명타 확률이 증가합니다";
            default: return "기본 데미지를 입힙니다";
        }
    }
}