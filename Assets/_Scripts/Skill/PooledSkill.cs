using UnityEngine;

public class PooledSkill : MonoBehaviour
{
    private string skillName;
    private float lifetime;
    private float spawnTime;
    private bool hasLifetime = false;

    [Header("자동 반환 설정")]
    public bool autoReturn = false;
    public float maxLifetime = 10f;

    // Initialize 메서드 - SkillPoolManager에서 호출
    public void Initialize(string name)
    {
        skillName = name;
    }

    public void SetLifetime(float time)
    {
        lifetime = time;
        hasLifetime = true;
        spawnTime = Time.time;
    }

    void OnEnable()
    {
        spawnTime = Time.time;

        // 오라가 아닌 경우만 자동 반환
        if (autoReturn && !string.IsNullOrEmpty(skillName) && !skillName.Contains("Aura"))
        {
            hasLifetime = true;
            lifetime = maxLifetime;
        }
    }

    void Update()
    {
        if (hasLifetime && Time.time - spawnTime >= lifetime)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        if (SkillPoolManager.Instance != null)
        {
            SkillPoolManager.Instance.ReturnSkill(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        hasLifetime = false;
    }

    // 충돌 시 반환 (발사체용)
    public void OnProjectileHit()
    {
        ReturnToPool();
    }

    // 디버그용
    public string GetSkillName()
    {
        return skillName;
    }
}