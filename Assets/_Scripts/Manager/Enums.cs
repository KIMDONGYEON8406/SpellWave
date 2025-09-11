using System;

// ===== 스킬 관련 Enums =====

// 주 스킬 타입
[Flags]
public enum PrimarySkillType
{
    None = 0,
    Projectile = 1,   // 발사체
    Area = 2,         // 영역
    DOT = 4           // 지속
}

// 부 스킬 타입 (특성)
[Flags]
public enum SecondarySkillType
{
    None = 0,
    Homing = 1,       // 유도
    Pierce = 2,       // 관통
    Instant = 4,      // 즉시
    SingleTarget = 8,  // 단일 대상
    MultiTarget = 16,  // 다중 대상
    Chain = 32,       // 연쇄
    Explosive = 64    // 폭발
}

// 스킬 태그 (호환성용)
public enum SkillTag
{
    Projectile,
    Area,
    DOT,
    Homing,
    Pierce,
    Instant,
    SingleTarget,
    MultiTarget
}

// ===== 원소 타입 =====
public enum ElementType
{
    None,       // 무속성
    Energy,     // 에너지 (기본)
    Fire,       // 불
    Water,      // 물
    Wind,       // 바람
    Earth,      // 땅
    Lightning,  // 번개 (물+바람)
    Ice,        // 얼음 (물+바람)
    Magma,      // 마그마 (불+땅)
    Steam,      // 증기 (불+물)
    Dust,       // 먼지 (바람+땅)
    Mud,        // 진흙 (물+땅)
    Light,      // 빛
    Dark,       // 어둠
    Poison      // 독
}

// ===== 패시브 타입 =====
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

// ===== 스탯 타입 =====
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

// ===== 카드 관련 =====
public enum CardType
{
    StatCard,    // 스탯 증가 카드
    SkillCard,   // 스킬 획득 카드
    EvolutionCard // 진화 카드
}

public enum CardRarity
{
    Common,     // 일반
    Rare,       // 희귀
    Epic,       // 에픽
    Legendary   // 전설
}