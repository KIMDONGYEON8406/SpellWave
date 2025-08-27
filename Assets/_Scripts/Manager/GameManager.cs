using UnityEngine;
using System;
using System.Collections.Generic;

public enum GameState
{
    MainMenu,
    CharacterSelect,
    Playing,
    Paused,
    CardSelection,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 상태")]
    public GameState currentState = GameState.Playing;
    public Player player;

    [Header("초기 장비")]
    public StaffData defaultStaff;

    [Header("타임라인 설정")]
    public TimelineConfig timelineConfig;

    [Header("타임라인 진행상황")]
    public float currentTime = 0f;
    public bool isStageActive = false;
    public int currentLevel = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;

    [Header("게임 타이머")]
    public float totalGameTime = 0f;

    [Header("디버그용 스킬 목록")]
    [SerializeField] private List<SkillData> debugSkills = new List<SkillData>();

    // 이벤트들
    public static event Action<float> OnProgressChanged;
    public static event Action<int> OnLevelUp;
    public static event Action<int, int> OnExpChanged;
    public static event Action OnStageCompleted;
    public static event Action<GameState> OnGameStateChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DebugManager.LogGame("GameManager 싱글톤 초기화");
        }
        else
        {
            DebugManager.LogWarning(LogCategory.Game, "중복 GameManager 감지, 파괴됨");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (timelineConfig == null)
        {
            DebugManager.LogError(LogCategory.Game, "TimelineConfig가 할당되지 않았습니다!");
            return;
        }

        if (player == null)
        {
            player = GameObject.FindObjectOfType<Player>();
            if (player != null && player.tag != "Player")
            {
                player.tag = "Player";
            }
        }

        InitializeStaff();
        StartStage();
        CollectAllSkills();

        DebugManager.LogGame("GameManager 초기화 완료");
    }

    void CollectAllSkills()
    {
        debugSkills.Clear();
        debugSkills.AddRange(Resources.LoadAll<SkillData>("Skills"));

        var skills = Resources.LoadAll<SkillData>("");
        debugSkills.AddRange(skills);

        DebugManager.LogGame($"디버그용 스킬 {debugSkills.Count}개 로드");
    }

    void Update()
    {
        if (currentState == GameState.Playing && isStageActive)
        {
            UpdateTimeline();
        }
    }

    public void StartStage()
    {
        currentTime = 0f;
        isStageActive = true;
        currentLevel = 1;
        currentExp = 0;
        expToNextLevel = timelineConfig.GetExpToLevelUp(currentLevel);

        DebugManager.LogTimeline($"[timelineProgress] 스테이지 시작! 목표 시간: {timelineConfig.totalDuration / 60f:F1}분");
        OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    void InitializeStaff()
    {
        if (StaffManager.Instance != null && defaultStaff != null)
        {
            StaffManager.Instance.UnlockStaff(defaultStaff);
            StaffManager.Instance.EquipStaff(defaultStaff);
            DebugManager.LogGame($"초기 지팡이 장착: {defaultStaff.staffName}");
        }
        else if (defaultStaff == null)
        {
            DebugManager.LogWarning(LogCategory.Game, "기본 지팡이가 설정되지 않았습니다!");
        }
    }

    public void ChangeState(GameState newState)
    {
        GameState oldState = currentState;
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
        DebugManager.LogGameState($"[gameStateChanges] 게임 상태 변경: {oldState} → {currentState}");
    }

    private void UpdateTimeline()
    {
        currentTime += Time.deltaTime;
        totalGameTime += Time.deltaTime;

        float progress = timelineConfig.GetProgress(currentTime);
        OnProgressChanged?.Invoke(progress);

        // 진행률 디버그 (10% 단위로만)
        if (Mathf.FloorToInt(progress / 10f) != Mathf.FloorToInt((progress - Time.deltaTime * 100f / timelineConfig.totalDuration) / 10f))
        {
            DebugManager.LogTimeline($"[timelineProgress] 진행률: {progress:F0}%");
        }

        if (timelineConfig.IsStageComplete(currentTime))
        {
            CompleteStage();
        }
    }

    public void AddExperience(int expAmount)
    {
        currentExp += expAmount;
        DebugManager.LogExperience($"[experienceSystem] 경험치 +{expAmount} (총: {currentExp}/{expToNextLevel})");

        while (currentExp >= expToNextLevel && currentLevel < timelineConfig.maxLevel)
        {
            LevelUp();
        }

        OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        currentLevel++;
        expToNextLevel = timelineConfig.GetExpToLevelUp(currentLevel);

        DebugManager.LogLevelUp($"[levelUpSystem] 레벨업! 레벨 {currentLevel} (다음 레벨까지: {expToNextLevel})");
        OnLevelUp?.Invoke(currentLevel);

        ShowCardSelection();
    }

    private void CompleteStage()
    {
        isStageActive = false;
        OnStageCompleted?.Invoke();

        DebugManager.LogTimeline($"[timelineProgress] 스테이지 완료! 총 시간: {currentTime / 60f:F1}분, 최종 레벨: {currentLevel}");
        ChangeState(GameState.Victory);
    }

    private void ShowCardSelection()
    {
        ChangeState(GameState.CardSelection);

        if (CardManager.Instance != null)
        {
            CardManager.Instance.ShowRandomCards();
        }
        else
        {
            DebugManager.LogWarning(LogCategory.Game, "CardManager가 없어서 카드 선택을 건너뜁니다.");
            ChangeState(GameState.Playing);
        }
    }

    public void OnCardSelected(CardData selectedCard)
    {
        ApplyCardEffect(selectedCard);
        ChangeState(GameState.Playing);
    }

    private void ApplyCardEffect(CardData selectedCard)
    {
        DebugManager.LogGame($"카드 효과 적용 완료 - {selectedCard.cardName}");
    }

    // ===== 디버그 스킬 획득 메서드들 =====
    [ContextMenu("디버그/스킬 획득/오라 (Aura)")]
    public void DebugAddAuraSkill()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 오라 스킬 추가");
        AddSkillByName("Aura");
    }

    [ContextMenu("디버그/스킬 획득/볼 (Bolt)")]
    public void DebugAddBoltSkill()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 볼트 스킬 추가");
        AddSkillByName("Bolt");
    }

    [ContextMenu("디버그/스킬 획득/애로우 (Arrow)")]
    public void DebugAddArrowSkill()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 애로우 스킬 추가");
        AddSkillByName("Arrow");
    }

    [ContextMenu("디버그/스킬 획득/익스플로전 (Explosion)")]
    public void DebugAddExplosionSkill()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 익스플로전 스킬 추가");
        AddSkillByName("Explosion");
    }

    [ContextMenu("디버그/스킬 획득/미사일 (Missile)")]
    public void DebugAddMissileSkill()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 미사일 스킬 추가");
        AddSkillByName("Missile");
    }

    [ContextMenu("디버그/모든 기본 스킬 획득")]
    public void DebugAddAllBasicSkills()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 모든 기본 스킬 추가");
        string[] basicSkills = { "Bolt", "Arrow", "Explosion", "Missile" };
        foreach (string skillName in basicSkills)
        {
            AddSkillByName(skillName);
        }
    }

    [ContextMenu("디버그/스킬 획득/오라 (Aura) - 단일")]
    public void DebugAddAuraSingle()
    {
        var skillManager = player?.GetComponent<SkillManager>();
        if (skillManager != null)
        {
            var existingAura = skillManager.GetSkill("Aura");
            if (existingAura != null)
            {
                DebugManager.LogWarning(LogCategory.Game, "[debugCommands] 오라가 이미 존재합니다!");
                return;
            }
        }

        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 오라 단일 획득");
        AddSkillByName("Aura");
    }

    void AddSkillByName(string skillName)
    {
        SkillData skillToAdd = debugSkills.Find(s => s != null && s.baseSkillType == skillName);

        if (skillToAdd == null)
        {
            if (StaffManager.Instance != null && StaffManager.Instance.currentStaff != null)
            {
                var staff = StaffManager.Instance.currentStaff;
                skillToAdd = staff.defaultSkills.Find(s => s != null && s.baseSkillType == skillName);

                if (skillToAdd == null)
                {
                    skillToAdd = staff.availableSkillPool.Find(s => s != null && s.baseSkillType == skillName);
                }
            }
        }

        if (skillToAdd != null)
        {
            AddDebugSkill(skillToAdd);
        }
        else
        {
            DebugManager.LogWarning(LogCategory.Game, $"[debugCommands] 스킬을 찾을 수 없음: {skillName}");
        }
    }

    void AddDebugSkill(SkillData skillData)
    {
        if (skillData == null) return;

        var inventory = StaffManager.Instance?.GetCurrentInventory();
        if (inventory != null)
        {
            if (!inventory.ownedSkills.Contains(skillData))
            {
                inventory.ownedSkills.Add(skillData);
                DebugManager.LogDebugCommand($"[debugCommands] 인벤토리에 {skillData.baseSkillType} 추가");
            }

            if (!inventory.equippedSkills.Contains(skillData))
            {
                if (inventory.equippedSkills.Count < 5)
                {
                    inventory.equippedSkills.Add(skillData);
                    StaffManager.Instance.UpdateEquippedSkills(inventory.equippedSkills);
                    DebugManager.LogDebugCommand($"[debugCommands] {skillData.baseSkillType} 스킬 장착 완료! (슬롯 {inventory.equippedSkills.Count}/5)");
                }
                else
                {
                    DebugManager.LogWarning(LogCategory.Game, "[debugCommands] 스킬 슬롯이 가득 참 (5/5)");
                }
            }
            else
            {
                DebugManager.LogDebugCommand($"[debugCommands] {skillData.baseSkillType}은 이미 장착됨");
            }
        }
        else
        {
            DebugManager.LogError(LogCategory.Game, "[debugCommands] StaffInventory를 찾을 수 없음!");
        }
    }

    // ===== 카드 효과 즉시 적용 =====
    [ContextMenu("디버그/카드 효과/범위 +25%")]
    public void DebugApplyRangeCard()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 범위 +25% 적용");
        ApplyStatBoost(StatType.AllSkillRange, 25f);
    }

    [ContextMenu("디버그/카드 효과/범위 +50%")]
    public void DebugApplyRangeCard50()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 범위 +50% 적용");
        ApplyStatBoost(StatType.AllSkillRange, 50f);
    }

    [ContextMenu("디버그/카드 효과/데미지 +25%")]
    public void DebugApplyDamageCard()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 데미지 +25% 적용");
        ApplyStatBoost(StatType.AllSkillDamage, 25f);
    }

    [ContextMenu("디버그/카드 효과/쿨타임 -25%")]
    public void DebugApplyCooldownCard()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 쿨타임 -25% 적용");
        ApplyStatBoost(StatType.AllSkillCooldown, 25f);
    }

    void ApplyStatBoost(StatType statType, float percentage)
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        SkillManager skillManager = player?.GetComponent<SkillManager>();
        if (skillManager == null)
        {
            DebugManager.LogError(LogCategory.Game, "[debugCommands] SkillManager를 찾을 수 없습니다!");
            return;
        }

        var allSkills = skillManager.GetAllSkills();
        int affectedCount = 0;

        foreach (var skill in allSkills)
        {
            bool affected = false;

            switch (statType)
            {
                case StatType.AllSkillDamage:
                    skill.damageMultiplier += (percentage / 100f);
                    affected = true;
                    break;

                case StatType.AllSkillCooldown:
                    skill.cooldownMultiplier *= (1f - percentage / 100f);
                    affected = true;
                    break;

                case StatType.AllSkillRange:
                    skill.rangeMultiplier += (percentage / 100f);
                    affected = true;
                    break;

                case StatType.AreaRange:
                    if (skill.skillData.HasTag(SkillTag.Area) || skill.skillData.baseSkillType == "Aura")
                    {
                        skill.rangeMultiplier += (percentage / 100f);
                        affected = true;
                    }
                    break;
            }

            if (affected)
            {
                affectedCount++;
                DebugManager.LogDebugCommand($"[debugCommands] {skill.skillData.baseSkillType}: {statType} +{percentage}% 적용");
            }
        }

        DebugManager.LogDebugCommand($"[debugCommands] {affectedCount}개 스킬에 {statType} +{percentage}% 적용 완료!");
    }

    // ===== 기존 테스트 메서드들 =====
    [ContextMenu("레벨업 테스트")]
    public void TestLevelUp()
    {
        if (!Application.isPlaying)
        {
            DebugManager.LogWarning(LogCategory.Game, "[debugCommands] 플레이 모드에서만 작동합니다!");
            return;
        }

        currentLevel++;
        DebugManager.LogDebugCommand($"[debugCommands] 강제 레벨업! 현재 레벨: {currentLevel}");

        if (currentLevel % 4 == 0)
        {
            DebugManager.LogDebugCommand("[debugCommands] >>> 스킬 카드가 나와야 함!");
        }
        else
        {
            DebugManager.LogDebugCommand("[debugCommands] >>> 스탯 카드가 나와야 함!");
        }

        OnLevelUp?.Invoke(currentLevel);
        ShowCardSelection();
    }

    [ContextMenu("경험치 +100")]
    public void AddTestExp()
    {
        if (Application.isPlaying)
        {
            DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 경험치 +100");
            AddExperience(100);
        }
    }

    [ContextMenu("강제 스테이지 완료")]
    public void ForceCompleteStage()
    {
        if (Application.isPlaying)
        {
            DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 강제 스테이지 완료");
            CompleteStage();
        }
    }

    [ContextMenu("스테이지 재시작")]
    public void RestartStage()
    {
        if (Application.isPlaying)
        {
            DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 스테이지 재시작");
            ChangeState(GameState.Playing);
            StartStage();
        }
    }

    [ContextMenu("디버그/글로벌 스탯 확인")]
    public void DebugCheckGlobalStats()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 글로벌 스탯 확인");
        var modifier = SkillStatModifier.Instance;
        if (modifier != null)
        {
            modifier.PrintGlobalStats();
        }
    }

    [ContextMenu("디버그/발사체 개수 +2")]
    public void DebugAddProjectileCount()
    {
        DebugManager.LogDebugCommand("[debugCommands] 디버그 명령: 발사체 개수 +2");
        var modifier = player.GetComponent<ProjectileCountModifier>();
        if (modifier == null)
        {
            modifier = player.gameObject.AddComponent<ProjectileCountModifier>();
        }

        modifier.AddProjectileCountToAll(2);
        DebugManager.LogDebugCommand("[debugCommands] 모든 스킬 발사체 +2개!");
    }

    // 정보 접근 메서드들 (기존과 동일)
    public float GetStageProgress()
    {
        return timelineConfig != null ? timelineConfig.GetProgress(currentTime) : 0f;
    }

    public int GetRemainingTime()
    {
        if (timelineConfig == null) return 0;
        return Mathf.Max(0, Mathf.CeilToInt(timelineConfig.totalDuration - currentTime));
    }

    public string GetFormattedGameTime()
    {
        int minutes = Mathf.FloorToInt(totalGameTime / 60);
        int seconds = Mathf.FloorToInt(totalGameTime % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    public string GetFormattedStageTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    public TimelineConfig GetTimelineConfig()
    {
        return timelineConfig;
    }

    public float GetCurrentDifficultyMultiplier()
    {
        return timelineConfig != null ? timelineConfig.GetDifficultyMultiplier(currentTime) : 1f;
    }

    public float GetCurrentSpawnRate()
    {
        return timelineConfig != null ? timelineConfig.GetCurrentSpawnRate(currentTime) : 0.5f;
    }
}