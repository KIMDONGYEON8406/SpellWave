// 카드 관련 Enum들을 한 곳에 정의

// 카드 타입
public enum CardType
{
    StatCard,    // 스탯 증가 카드
    SkillCard    // 스킬 획득 카드
}

// 스탯 타입
public enum StatType
{
    // 플레이어 스탯
    PlayerHealth,        // 최대 체력
    PlayerMoveSpeed,     // 이동속도
    PlayerAttackPower,   // 공격력
    InstantHeal,         // 즉시 회복

    // 전체 스킬 강화
    AllSkillDamage,      // 모든 스킬 데미지
    AllSkillCooldown,    // 모든 스킬 쿨타임
    AllSkillRange,       // 모든 스킬 범위
    AllSkillMultiCast,   // 모든 스킬 다중시전

    // 발사체 타입 강화
    ProjectileDamage,      // 발사체 데미지
    ProjectileCooldown,    // 발사체 쿨타임
    ProjectileCount,       // 발사체 개수 +1
    ProjectileSpeed,       // 발사체 속도
    ProjectileMultiCast,   // 발사체 다중시전

    // 영역 타입 강화
    AreaDamage,           // 영역 데미지
    AreaCooldown,         // 영역 쿨타임
    AreaRange,            // 영역 범위
    AreaMultiCast,        // 영역 다중시전
    AreaCount,            // 영역 개수 +1

    // DOT 타입 강화
    DOTDamage,            // 지속 데미지
    DOTTickRate,          // 틱 간격 (데미지 주기)
    DOTDuration,          // 지속 시간
    DOTMultiCast          // DOT 다중시전
}

// 카드 등급
public enum CardRarity
{
    Common,     // 일반
    Rare,       // 희귀
    Epic,       // 에픽
    Legendary   // 전설
}