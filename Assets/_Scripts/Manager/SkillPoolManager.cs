using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SkillPoolManager : MonoBehaviour
{
    public static SkillPoolManager Instance { get; private set; }

    [System.Serializable]
    public class SkillPool
    {
        public string skillName;
        public GameObject prefab;
        public int initialPoolSize = 10;
        public bool canExpand = true;

        [HideInInspector]
        public Queue<GameObject> pool = new Queue<GameObject>();
    }

    [Header("자동 수집 설정")]
    public bool autoCollectFromStaff = true;
    public int defaultPoolSize = 10;

    [Header("스킬 풀 설정 (수동)")]
    public List<SkillPool> manualSkillPools = new List<SkillPool>();

    [Header("런타임 풀 (자동 생성)")]
    [SerializeField] private List<SkillPool> runtimeSkillPools = new List<SkillPool>();

    [Header("풀 관리")]
    public Transform poolContainer;
    public bool warmupOnStart = true;

    private Dictionary<string, SkillPool> poolDictionary = new Dictionary<string, SkillPool>();
    private Dictionary<GameObject, string> activeObjects = new Dictionary<GameObject, string>();

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
            return;
        }
    }

    void Start()
    {
        // StaffManager가 초기화될 때까지 대기
        StartCoroutine(InitializeDelayed());
    }

    System.Collections.IEnumerator InitializeDelayed()
    {
        // StaffManager 대기
        while (StaffManager.Instance == null)
        {
            yield return null;
        }

        // 약간 더 대기 (Staff 초기화 완료)
        yield return new WaitForSeconds(0.1f);

        if (autoCollectFromStaff)
        {
            CollectSkillsFromStaff();
        }

        InitializePools();

        if (warmupOnStart)
        {
            WarmupPools();
        }
    }

    void CollectSkillsFromStaff()
    {
        if (StaffManager.Instance == null || StaffManager.Instance.currentStaff == null)
        {
            Debug.LogWarning("[SkillPoolManager] StaffManager 또는 currentStaff가 없습니다!");
            return;
        }

        var currentStaff = StaffManager.Instance.currentStaff;
        runtimeSkillPools.Clear();

        // 기본 제공 스킬들 수집
        if (currentStaff.defaultSkills != null)
        {
            foreach (var skillData in currentStaff.defaultSkills)
            {
                if (skillData != null && skillData.skillPrefab != null)
                {
                    AddSkillToPool(skillData);
                }
            }
        }

        // 획득 가능한 스킬들 수집
        if (currentStaff.availableSkillPool != null)
        {
            foreach (var skillData in currentStaff.availableSkillPool)
            {
                if (skillData != null && skillData.skillPrefab != null)
                {
                    // 중복 체크
                    if (!runtimeSkillPools.Any(p => p.skillName == skillData.baseSkillType))
                    {
                        AddSkillToPool(skillData);
                    }
                }
            }
        }

        Debug.Log($"[SkillPoolManager] {currentStaff.staffName}에서 {runtimeSkillPools.Count}개 스킬 자동 수집");
    }

    void AddSkillToPool(SkillData skillData)
    {
        var pool = new SkillPool
        {
            skillName = skillData.baseSkillType,
            prefab = skillData.skillPrefab,
            initialPoolSize = GetPoolSizeForSkill(skillData.baseSkillType),
            canExpand = true,
            pool = new Queue<GameObject>()
        };

        runtimeSkillPools.Add(pool);
        Debug.Log($"  - {skillData.baseSkillType} 추가 (풀 크기: {pool.initialPoolSize})");
    }

    int GetPoolSizeForSkill(string skillName)
    {
        // 스킬별 최적 풀 크기 설정
        switch (skillName)
        {
            case "Aura":
                return 1;  // 오라는 1개만
            case "Explosion":
                return 5;  // 폭발은 적게
            case "Bolt":
            case "Arrow":
            case "Missile":
                return 15; // 발사체는 많이
            default:
                return defaultPoolSize;
        }
    }

    void InitializePools()
    {
        // 풀 컨테이너 생성
        if (poolContainer == null)
        {
            GameObject container = new GameObject("SkillPoolContainer");
            container.transform.SetParent(transform);
            poolContainer = container.transform;
        }

        // 자동 수집된 풀 추가
        foreach (var skillPool in runtimeSkillPools)
        {
            if (!string.IsNullOrEmpty(skillPool.skillName) && skillPool.prefab != null)
            {
                poolDictionary[skillPool.skillName] = skillPool;
            }
        }

        // 수동 설정 풀 추가 (덮어쓰기)
        foreach (var skillPool in manualSkillPools)
        {
            if (!string.IsNullOrEmpty(skillPool.skillName) && skillPool.prefab != null)
            {
                poolDictionary[skillPool.skillName] = skillPool;
                Debug.Log($"[SkillPoolManager] 수동 풀 덮어쓰기: {skillPool.skillName}");
            }
        }
    }

    void WarmupPools()
    {
        foreach (var kvp in poolDictionary)
        {
            var pool = kvp.Value;
            for (int i = 0; i < pool.initialPoolSize; i++)
            {
                CreatePoolObject(pool);
            }
        }

        Debug.Log($"[SkillPoolManager] {poolDictionary.Count}개 풀 준비 완료");
        PrintPoolStatus();
    }

    GameObject CreatePoolObject(SkillPool pool)
    {
        GameObject obj = Instantiate(pool.prefab, poolContainer);
        obj.SetActive(false);
        obj.name = $"{pool.skillName}_Pooled";

        // 풀링 컴포넌트 추가
        var pooledSkill = obj.GetComponent<PooledSkill>();
        if (pooledSkill == null)
        {
            pooledSkill = obj.AddComponent<PooledSkill>();
        }

        if (pooledSkill != null)
        {
            pooledSkill.Initialize(pool.skillName);
        }

        pool.pool.Enqueue(obj);
        return obj;
    }

    // 스킬 오브젝트 가져오기
    public GameObject GetSkill(string skillName, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(skillName))
        {
            Debug.LogError($"[SkillPoolManager] '{skillName}' 풀이 존재하지 않습니다!");
            return null;
        }

        SkillPool pool = poolDictionary[skillName];
        GameObject obj = null;

        // 풀에서 가져오기
        if (pool.pool.Count > 0)
        {
            obj = pool.pool.Dequeue();
        }
        else if (pool.canExpand)
        {
            obj = CreatePoolObject(pool);
            Debug.Log($"[SkillPoolManager] '{skillName}' 풀 확장");
        }
        else
        {
            Debug.LogWarning($"[SkillPoolManager] '{skillName}' 풀이 비어있고 확장 불가!");
            return null;
        }

        // 오브젝트 설정
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            if (parent != null)
            {
                obj.transform.SetParent(parent, false);
                obj.transform.localPosition = Vector3.zero;
            }
            else
            {
                obj.transform.SetParent(null);
            }

            obj.SetActive(true);
            activeObjects[obj] = skillName;
        }

        return obj;
    }

    // 오브젝트 반환
    public void ReturnSkill(GameObject obj)
    {
        if (obj == null) return;

        if (activeObjects.ContainsKey(obj))
        {
            string skillName = activeObjects[obj];
            activeObjects.Remove(obj);

            if (poolDictionary.ContainsKey(skillName))
            {
                obj.SetActive(false);
                obj.transform.SetParent(poolContainer);
                poolDictionary[skillName].pool.Enqueue(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
        else
        {
            Debug.LogWarning($"[SkillPoolManager] 풀에 속하지 않은 오브젝트: {obj.name}");
            Destroy(obj);
        }
    }

    // 나머지 메서드들은 동일...

    public void PrintPoolStatus()
    {
        Debug.Log("=== Skill Pool Status ===");
        foreach (var kvp in poolDictionary)
        {
            int activeCount = 0;
            foreach (var active in activeObjects.Values)
            {
                if (active == kvp.Key) activeCount++;
            }

            Debug.Log($"{kvp.Key}: 풀={kvp.Value.pool.Count}, 활성={activeCount}");
        }
    }

    // 수동으로 스킬 풀 추가 (런타임)
    public void AddSkillPoolRuntime(string skillName, GameObject prefab, int poolSize = 10)
    {
        if (poolDictionary.ContainsKey(skillName))
        {
            Debug.LogWarning($"[SkillPoolManager] '{skillName}' 풀이 이미 존재합니다!");
            return;
        }

        var pool = new SkillPool
        {
            skillName = skillName,
            prefab = prefab,
            initialPoolSize = poolSize,
            canExpand = true,
            pool = new Queue<GameObject>()
        };

        poolDictionary[skillName] = pool;

        // 즉시 웜업
        for (int i = 0; i < poolSize; i++)
        {
            CreatePoolObject(pool);
        }

        Debug.Log($"[SkillPoolManager] '{skillName}' 풀 런타임 추가 완료");
    }

    // Staff 변경 시 재수집
    public void RefreshPoolsFromStaff()
    {
        if (!autoCollectFromStaff) return;

        Debug.Log("[SkillPoolManager] Staff 변경 감지, 풀 재구성");

        // 기존 활성 오브젝트 정리
        ReturnAllActiveSkills();

        // 재수집
        CollectSkillsFromStaff();
        InitializePools();
        WarmupPools();
    }

    void OnDestroy()
    {
        ReturnAllActiveSkills();
    }

    public void ReturnAllActiveSkills()
    {
        List<GameObject> toReturn = new List<GameObject>(activeObjects.Keys);

        foreach (var obj in toReturn)
        {
            ReturnSkill(obj);
        }
    }
}