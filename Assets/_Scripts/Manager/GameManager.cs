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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (timelineConfig == null)
        {
            Debug.LogError("GameManager: TimelineConfig가 할당되지 않았습니다!");
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

        // 디버그 스킬 자동 수집
        CollectAllSkills();
    }

    void CollectAllSkills()
    {
        // Resources 폴더에서 모든 SkillData 찾기
        debugSkills.Clear();
        debugSkills.AddRange(Resources.LoadAll<SkillData>("Skills"));

        // 또는 특정 경로에서 찾기
        var skills = Resources.LoadAll<SkillData>("");
        debugSkills.AddRange(skills);

        Debug.Log($"디버그용 스킬 {debugSkills.Count}개 로드");
    }

    // ===== 디버그 스킬 획득 메서드들 =====

    [ContextMenu("디버그/스킬 획득/오라 (Aura)")]
    public void DebugAddAuraSkill()
    {
        AddSkillByName("Aura");
    }

    [ContextMenu("디버그/스킬 획득/볼 (Bolt)")]
    public void DebugAddBoltSkill()
    {
        AddSkillByName("Bolt");
    }

    [ContextMenu("디버그/스킬 획득/애로우 (Arrow)")]
    public void DebugAddArrowSkill()
    {
        AddSkillByName("Arrow");
    }

    [ContextMenu("디버그/스킬 획득/익스플로전 (Explosion)")]
    public void DebugAddExplosionSkill()
    {
        AddSkillByName("Explosion");
    }

    [ContextMenu("디버그/스킬 획득/미사일 (Missile)")]
    public void DebugAddMissileSkill()
    {
        AddSkillByName("Missile");
    }

    [ContextMenu("디버그/모든 기본 스킬 획득")]
    public void DebugAddAllBasicSkills()
    {
        string[] basicSkills = { "Aura", "Bolt", "Arrow", "Explosion", "Missile" };
        foreach (string skillName in basicSkills)
        {
            AddSkillByName(skillName);
        }
    }

    // 스킬 이름으로 찾아서 추가
    void AddSkillByName(string skillName)
    {
        // debugSkills에서 찾기
        SkillData skillToAdd = debugSkills.Find(s => s != null && s.baseSkillType == skillName);

        if (skillToAdd == null)
        {
            // StaffData에서 찾기
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
            Debug.LogWarning($"스킬을 찾을 수 없음: {skillName}");
        }
    }

    // 실제 스킬 추가 로직
    void AddDebugSkill(SkillData skillData)
    {
        if (skillData == null) return;

        // StaffManager를 통해 인벤토리에 추가
        var inventory = StaffManager.Instance?.GetCurrentInventory();
        if (inventory != null)
        {
            // 인벤토리에 추가
            if (!inventory.ownedSkills.Contains(skillData))
            {
                inventory.ownedSkills.Add(skillData);
                Debug.Log($"[디버그] 인벤토리에 {skillData.baseSkillType} 추가");
            }

            // 바로 장착
            if (!inventory.equippedSkills.Contains(skillData))
            {
                if (inventory.equippedSkills.Count < 5)
                {
                    inventory.equippedSkills.Add(skillData);

                    // SkillManager 업데이트
                    StaffManager.Instance.UpdateEquippedSkills(inventory.equippedSkills);

                    Debug.Log($"[디버그] {skillData.baseSkillType} 스킬 장착 완료! (슬롯 {inventory.equippedSkills.Count}/5)");
                }
                else
                {
                    Debug.LogWarning("스킬 슬롯이 가득 참 (5/5)");
                }
            }
            else
            {
                Debug.Log($"{skillData.baseSkillType}은 이미 장착됨");
            }
        }
        else
        {
            Debug.LogError("StaffInventory를 찾을 수 없음!");
        }
    }

    // ===== 카드 효과 즉시 적용 =====

    [ContextMenu("디버그/카드 효과/범위 +25%")]
    public void DebugApplyRangeCard()
    {
        ApplyStatBoost(StatType.AllSkillRange, 25f);
    }

    [ContextMenu("디버그/카드 효과/범위 +50%")]
    public void DebugApplyRangeCard50()
    {
        ApplyStatBoost(StatType.AllSkillRange, 50f);
    }

    [ContextMenu("디버그/카드 효과/데미지 +25%")]
    public void DebugApplyDamageCard()
    {
        ApplyStatBoost(StatType.AllSkillDamage, 25f);
    }

    [ContextMenu("디버그/카드 효과/쿨타임 -25%")]
    public void DebugApplyCooldownCard()
    {
        ApplyStatBoost(StatType.AllSkillCooldown, 25f);
    }

    [ContextMenu("디버그/카드 효과/영역 스킬 범위 +50%")]
    public void DebugApplyAreaRangeCard()
    {
        ApplyStatBoost(StatType.AreaRange, 50f);
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
            Debug.LogError("SkillManager를 찾을 수 없습니다!");
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
                Debug.Log($"[디버그] {skill.skillData.baseSkillType}: {statType} +{percentage}% 적용");
                Debug.Log($"  → 현재 배율 - DMG: {skill.damageMultiplier:F2}, CD: {skill.cooldownMultiplier:F2}, Range: {skill.rangeMultiplier:F2}");
            }
        }

        Debug.Log($"[디버그 카드 효과] {affectedCount}개 스킬에 {statType} +{percentage}% 적용 완료!");
    }

    // ===== 스킬 정보 출력 =====

    [ContextMenu("디버그/현재 스킬 정보 출력")]
    public void DebugPrintSkillInfo()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        SkillManager skillManager = player?.GetComponent<SkillManager>();
        if (skillManager != null)
        {
            skillManager.PrintSkillInfo();

            // 추가 정보
            var skills = skillManager.GetAllSkills();
            foreach (var skill in skills)
            {
                Debug.Log($"[상세] {skill.skillData.baseSkillType}:");
                Debug.Log($"  - Damage Multiplier: {skill.damageMultiplier:F2}");
                Debug.Log($"  - Cooldown Multiplier: {skill.cooldownMultiplier:F2}");
                Debug.Log($"  - Range Multiplier: {skill.rangeMultiplier:F2}");
            }
        }
    }

    [ContextMenu("디버그/오라 상태 확인")]
    public void DebugCheckAuraStatus()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        Transform aura = player.transform.Find("PermanentAura");
        if (aura != null)
        {
            var dotArea = aura.GetComponent<ElementalDOTArea>();
            if (dotArea != null)
            {
                Debug.Log($"[오라 상태]");
                Debug.Log($"  - Radius: {dotArea.radius}");
                Debug.Log($"  - DPS: {dotArea.damagePerSecond}");

                var collider = aura.GetComponent<SphereCollider>();
                if (collider != null)
                {
                    Debug.Log($"  - Collider Radius: {collider.radius}");
                }

                var particle = aura.Find("Freeze circle")?.GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    var main = particle.main;
                    Debug.Log($"  - Particle Size: X={main.startSizeXMultiplier}, Y={main.startSizeYMultiplier}");
                    Debug.Log($"  - Particle Scale: {particle.transform.localScale}");
                }
            }
        }
        else
        {
            Debug.Log("오라가 없습니다!");
        }
    }

    // 기존 메서드들...
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

        Debug.Log($"스테이지 시작! 목표 시간: {timelineConfig.totalDuration / 60f:F1}분");
        OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    void InitializeStaff()
    {
        if (StaffManager.Instance != null && defaultStaff != null)
        {
            StaffManager.Instance.UnlockStaff(defaultStaff);
            StaffManager.Instance.EquipStaff(defaultStaff);
            Debug.Log($"초기 지팡이 장착: {defaultStaff.staffName}");
        }
        else if (defaultStaff == null)
        {
            Debug.LogWarning("GameManager: 기본 지팡이가 설정되지 않았습니다!");
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
        Debug.Log($"게임 상태 변경: {currentState}");
    }

    private void UpdateTimeline()
    {
        currentTime += Time.deltaTime;
        totalGameTime += Time.deltaTime;

        float progress = timelineConfig.GetProgress(currentTime);
        OnProgressChanged?.Invoke(progress);

        if (timelineConfig.IsStageComplete(currentTime))
        {
            CompleteStage();
        }
    }

    public void AddExperience(int expAmount)
    {
        currentExp += expAmount;
        //Debug.Log($"경험치 +{expAmount} (총: {currentExp}/{expToNextLevel})");

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

        Debug.Log($"레벨업! 레벨 {currentLevel}");
        OnLevelUp?.Invoke(currentLevel);

        ShowCardSelection();
    }

    private void CompleteStage()
    {
        isStageActive = false;
        OnStageCompleted?.Invoke();

        Debug.Log($"스테이지 완료! 총 시간: {currentTime / 60f:F1}분, 최종 레벨: {currentLevel}");
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
            Debug.LogWarning("CardManager가 없어서 카드 선택을 건너뜁니다.");
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
        Debug.Log($"GameManager: 카드 효과 적용 완료 - {selectedCard.cardName}");
    }

    // 정보 접근 메서드들
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

    // ===== 디버그 메서드들 =====
    [ContextMenu("레벨업 테스트")]
    public void TestLevelUp()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("플레이 모드에서만 작동합니다!");
            return;
        }

        currentLevel++;
        Debug.Log($"강제 레벨업! 현재 레벨: {currentLevel}");

        if (currentLevel % 4 == 0)
        {
            Debug.Log(">>> 스킬 카드가 나와야 함!");
        }
        else
        {
            Debug.Log(">>> 스탯 카드가 나와야 함!");
        }

        OnLevelUp?.Invoke(currentLevel);
        ShowCardSelection();
    }

    [ContextMenu("경험치 +100")]
    public void AddTestExp()
    {
        if (Application.isPlaying)
        {
            AddExperience(100);
        }
    }

    [ContextMenu("강제 스테이지 완료")]
    public void ForceCompleteStage()
    {
        if (Application.isPlaying)
        {
            CompleteStage();
        }
    }

    [ContextMenu("스테이지 재시작")]
    public void RestartStage()
    {
        if (Application.isPlaying)
        {
            ChangeState(GameState.Playing);
            StartStage();
        }
    }
}