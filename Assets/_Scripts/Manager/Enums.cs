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
    // ===== 플레이어 스탯 (0~99) =====
    PlayerHealth = 0,           // 최대 체력 증가 (%)
    PlayerMoveSpeed = 1,         // 이동 속도 증가 (%)
    PlayerAttackPower = 2,       // 공격력 증가 (%)
    InstantHeal = 3,            // 즉시 체력 회복 (최대 체력의 %)
    PlayerAttackRange = 4,       // 공격 범위 증가 (%)

    // ===== 전체 스킬 강화 (100~199) =====
    AllSkillDamage = 100,        // 모든 스킬 데미지 증가 (%)
    AllSkillCooldown = 101,      // 모든 스킬 쿨타임 감소 (%)
    AllSkillRange = 102,         // 모든 스킬 범위 증가 (%)
    AllSkillMultiCast = 103,     // 모든 스킬 다중시전 확률 추가 (%)

    // ===== 발사체 타입 강화 (200~299) =====
    ProjectileDamage = 200,      // 발사체 스킬 데미지 증가 (%)
    ProjectileCooldown = 201,    // 발사체 스킬 쿨타임 감소 (%)
    ProjectileCount = 202,       // 발사체 개수 증가 (+n개)
    ProjectileSpeed = 203,       // 발사체 속도 증가 (%)
    ProjectileMultiCast = 204,   // 발사체 다중시전 확률 추가 (%)

    // ===== 영역 타입 강화 (300~399) =====
    AreaDamage = 300,           // 영역 스킬 데미지 증가 (%)
    AreaCooldown = 301,         // 영역 스킬 쿨타임 감소 (%)
    AreaRange = 302,            // 영역 스킬 범위 증가 (%)
    AreaMultiCast = 303,        // 영역 스킬 다중시전 확률 추가 (%)
    AreaCount = 304,            // 영역 스킬 개수 증가 (+n개, 폭발 등)

    // ===== 지속 효과 타입 강화 (400~499) =====
    DOTDamage = 400,            // 지속 데미지 증가 (%)
    DOTTickRate = 401,          // 틱 속도 증가 (데미지 주기 단축 %)
    DOTDuration = 402,          // 지속 시간 증가 (%)
    DOTMultiCast = 403          // 지속 효과 다중시전 확률 추가 (%)
}

// 카드 등급
public enum CardRarity
{
    Common,     // 일반
    Rare,       // 희귀
    Epic,       // 에픽
    Legendary   // 전설
}