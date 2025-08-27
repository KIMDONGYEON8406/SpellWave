using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    [Header("효과 정보")]
    public string effectName;
    [TextArea(2, 3)]
    public string description;

    // 효과 적용 (구현 필수)
    public abstract void ApplyEffect(Player player, float value);

    // UI 표시용 텍스트 (구현 필수)  
    public abstract string GetPreviewText(float value);

    // 적용 가능 여부 체크 (선택적)
    public virtual bool CanApply(Player player)
    {
        return true;
    }
}