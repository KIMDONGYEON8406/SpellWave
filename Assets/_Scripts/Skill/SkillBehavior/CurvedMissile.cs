using UnityEngine;

public class CurvedMissile : MonoBehaviour
{
    private Transform target;
    private float initialHeight;
    private float startTime;
    private HomingComponent homing;

    public void Initialize(Transform targetTransform, float height)
    {
        target = targetTransform;
        initialHeight = height;
        startTime = Time.time;
        homing = GetComponent<HomingComponent>();

        // 초반엔 유도 약하게
        if (homing != null)
        {
            StartCoroutine(GradualHoming());
        }
    }

    System.Collections.IEnumerator GradualHoming()
    {
        float originalSpeed = homing.rotationSpeed;
        homing.rotationSpeed = originalSpeed * 0.2f;  // 초반엔 20% 유도력

        yield return new WaitForSeconds(0.5f);

        // 점진적으로 유도력 증가
        float elapsed = 0;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            homing.rotationSpeed = Mathf.Lerp(originalSpeed * 0.2f, originalSpeed, elapsed / 0.5f);
            yield return null;
        }

        homing.rotationSpeed = originalSpeed;  // 최대 유도력
    }
}