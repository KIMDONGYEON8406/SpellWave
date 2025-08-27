using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("플레이어 스탯")]
    [SerializeField] private PlayerStats playerStats;

    [Header("입력 소스 (조이스틱 + 키보드)")]
    [Tooltip("캔버스에 배치한 조이스틱(예: UJoystick/Content/Prefab/Joystick) 컴포넌트를 연결하세요.")]
    [SerializeField] private bl_Joystick joystick;

    [Header("입력 설정")]
    [SerializeField] private float keyboardScale = 1f; // 키보드 감도 스케일
    [SerializeField] private float joystickScale = 1f; // 조이스틱 감도 스케일

    [Header("경험치 시스템")]
    public float currentXP = 0f;
    public int level = 1;

    // 컴포넌트
    private Rigidbody rb;
    private Animator animator;
    private SkillManager skillManager;

    // 이동 관련
    private Vector3 moveDirection;
    private float currentMoveSpeed;
    private float currentRotationSpeed;

    // 외부 접근용 프로퍼티 (Character.cs 대체)
    public float AttackPower => playerStats?.attackPower ?? 10f;
    public float AttackRange => playerStats?.attackRange ?? 10f;
    public float Health => playerStats?.currentHP ?? 100f;
    public float MaxHealth => playerStats?.maxHP ?? 100f;
    public float MoveSpeed => currentMoveSpeed;

    void Awake()
    {
        InitializeComponents();
    }

    void Start()
    {
        InitializeStats();
        SetupInitialState();
    }

    void Update()
    {
        HandleInput();
        UpdateAnimation();
        UpdateStatsFromSO();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        skillManager = GetComponent<SkillManager>();

        if (skillManager == null)
        {
            skillManager = gameObject.AddComponent<SkillManager>();
            DebugManager.LogPlayerComponents("SkillManager 자동 추가");
        }

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            DebugManager.LogPlayerComponents("Rigidbody 자동 추가");
        }

        // Rigidbody 설정
        rb.freezeRotation = true;
        rb.useGravity = true;

        DebugManager.LogPlayerComponents("플레이어 컴포넌트 초기화 완료");
    }

    void InitializeStats()
    {
        if (playerStats != null)
        {
            currentMoveSpeed = playerStats.moveSpeed;
            currentRotationSpeed = playerStats.rotationSpeed;
            playerStats.ResetToDefault();

            DebugManager.LogPlayerStats($"스탯 초기화 - HP:{playerStats.maxHP} 속도:{currentMoveSpeed} 공격력:{playerStats.attackPower}");
        }
        else
        {
            DebugManager.LogError(LogCategory.Player, "PlayerStats가 할당되지 않았습니다!");
            // 기본값 설정
            currentMoveSpeed = 5f;
            currentRotationSpeed = 10f;
        }
    }

    void SetupInitialState()
    {
        // 태그 설정
        if (gameObject.tag != "Player")
        {
            gameObject.tag = "Player";
        }

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;

        DebugManager.LogPlayer("플레이어 준비 완료!");
    }

    void UpdateStatsFromSO()
    {
        if (playerStats == null) return;

        // 스탯 변경 감지 및 갱신
        if (Mathf.Abs(currentMoveSpeed - playerStats.moveSpeed) > 0.01f)
        {
            float oldSpeed = currentMoveSpeed;
            currentMoveSpeed = playerStats.moveSpeed;
            DebugManager.LogPlayerStats($"이동속도 변경: {oldSpeed:F1} → {currentMoveSpeed:F1}");
        }

        if (Mathf.Abs(currentRotationSpeed - playerStats.rotationSpeed) > 0.01f)
        {
            currentRotationSpeed = playerStats.rotationSpeed;
        }
    }

    // ───────────────────────────────────────────────────────────
    // 입력 처리: 키보드 + 조이스틱을 합산 (PlayerController 로직)
    // ───────────────────────────────────────────────────────────
    void HandleInput()
    {
        // 1) 키보드 입력(WASD)
        Vector2 kb = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ) * keyboardScale;

        // 2) 조이스틱 입력(없으면 0)
        Vector2 js = Vector2.zero;
        if (joystick != null)
        {
            js = new Vector2(joystick.Horizontal, joystick.Vertical) * joystickScale;
        }

        // 3) 합산 후 클램프 (대각선/동시 입력 과도 속도 방지)
        Vector2 sum = kb + js;
        if (sum.sqrMagnitude > 1f) sum.Normalize();

        moveDirection = new Vector3(sum.x, 0f, sum.y);

        // 입력 디버그 (프레임 제한)
        if (Time.frameCount % 60 == 0 && moveDirection.magnitude > 0.1f)
        {
            DebugManager.LogPlayerInput($"KB({kb.x:F2},{kb.y:F2}) + JS({js.x:F2},{js.y:F2}) = 최종({moveDirection.x:F2},{moveDirection.z:F2})");
        }
    }

    void HandleMovement()
    {
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            // 이동
            Vector3 vel = new Vector3(
                moveDirection.x * currentMoveSpeed,
                rb.velocity.y,
                moveDirection.z * currentMoveSpeed
            );
            rb.velocity = vel;

            // 회전
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                currentRotationSpeed * Time.deltaTime
            );

            // 이동 디버그 (프레임 제한)
            if (Time.frameCount % 120 == 0)
            {
                DebugManager.LogPlayerMovement($"이동중 - 속도:{vel.magnitude:F1} 방향:({moveDirection.x:F2}, {moveDirection.z:F2})");
            }
        }
        else
        {
            // 정지 시 수평 속도 0
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    void UpdateAnimation()
    {
        bool isMoving = moveDirection.sqrMagnitude > 0.0001f;
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }
    }

    public void TakeDamage(float damage)
    {
        if (playerStats != null)
        {
            float oldHP = playerStats.currentHP;
            playerStats.TakeDamage(damage);

            DebugManager.LogPlayerHealth($"피해받음: {damage} ({oldHP:F0} → {playerStats.currentHP:F0})");

            if (playerStats.currentHP <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        DebugManager.LogPlayer("플레이어 사망!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.GameOver);
        }
    }

    public void AddExperience(float xp)
    {
        float oldXP = currentXP;
        currentXP += xp;

        DebugManager.LogPlayerExperience($"경험치 획득: +{xp} ({oldXP:F0} → {currentXP:F0})");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddExperience((int)xp);
        }
    }

    public void Heal(float amount)
    {
        if (playerStats != null)
        {
            float beforeHeal = playerStats.currentHP;
            playerStats.Heal(amount);

            DebugManager.LogPlayerHealth($"회복: +{amount} ({beforeHeal:F0} → {playerStats.currentHP:F0})");
        }
    }

    // 카드 시스템용 스탯 보너스 적용
    public void ApplyStatBonus(StatType statType, float bonusPercent)
    {
        if (playerStats == null) return;

        switch (statType)
        {
            case StatType.PlayerHealth:
                float oldMaxHP = playerStats.maxHP;
                playerStats.IncreaseMaxHP(bonusPercent);
                DebugManager.LogPlayerStats($"최대 체력 보너스 +{bonusPercent}% ({oldMaxHP:F0} → {playerStats.maxHP:F0})");
                break;

            case StatType.PlayerMoveSpeed:
                float oldSpeed = playerStats.moveSpeed;
                playerStats.IncreaseMoveSpeed(bonusPercent);
                DebugManager.LogPlayerStats($"이동속도 보너스 +{bonusPercent}% ({oldSpeed:F1} → {playerStats.moveSpeed:F1})");
                break;

            case StatType.PlayerAttackPower:
                float oldPower = playerStats.attackPower;
                playerStats.IncreaseAttackPower(bonusPercent);
                DebugManager.LogPlayerStats($"공격력 보너스 +{bonusPercent}% ({oldPower:F0} → {playerStats.attackPower:F0})");
                break;

            case StatType.InstantHeal:
                float healAmount = playerStats.maxHP * (bonusPercent / 100f);
                Heal(healAmount);
                break;
        }
    }

    // 조이스틱 설정 메서드 (확장성)
    public void SetJoystick(bl_Joystick newJoystick)
    {
        joystick = newJoystick;
        DebugManager.LogPlayerComponents("조이스틱 변경");
    }

    public void SetInputScale(float keyboardScl, float joystickScl)
    {
        keyboardScale = keyboardScl;
        joystickScale = joystickScl;
        DebugManager.LogPlayerComponents($"입력 감도 변경: KB={keyboardScale} JS={joystickScale}");
    }

    // 기존 호환성 메서드들
    public PlayerStats GetPlayerStats() => playerStats;

    // 디버그 명령어들
    [ContextMenu("디버그/현재 상태 출력")]
    void DebugPrintStatus()
    {
        DebugManager.LogSeparator("플레이어 상태");
        DebugManager.LogPlayer($"체력: {Health:F0}/{MaxHealth:F0}");
        DebugManager.LogPlayer($"이동속도: {currentMoveSpeed:F1}");
        DebugManager.LogPlayer($"공격력: {AttackPower:F0}");
        DebugManager.LogPlayer($"공격범위: {AttackRange:F1}");
        DebugManager.LogPlayer($"레벨: {level}, 경험치: {currentXP:F0}");
        DebugManager.LogPlayer($"조이스틱: {(joystick != null ? "연결됨" : "없음")}");
    }

    [ContextMenu("디버그/강제 피해 10")]
    void DebugTakeDamage()
    {
        TakeDamage(10f);
    }

    [ContextMenu("디버그/강제 회복 20")]
    void DebugHeal()
    {
        Heal(20f);
    }

    [ContextMenu("디버그/경험치 +50")]
    void DebugAddXP()
    {
        AddExperience(50f);
    }
}