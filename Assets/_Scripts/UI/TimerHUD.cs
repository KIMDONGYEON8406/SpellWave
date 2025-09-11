using UnityEngine;
using TMPro;

public class TimerHUD : MonoBehaviour
{
    public TextMeshProUGUI textTimer;

    GameManager gm;

    void Start()
    {
        gm = GameManager.Instance; // 싱글톤
    }

    void Update()
    {
        if (textTimer == null) return;

        // GameManager가 있으면 스테이지 시간 포맷 그대로 사용
        if (gm != null)
        {
            // GameManager.GetFormattedStageTime() : mm:ss 반환
            textTimer.text = gm.GetFormattedStageTime();
        }
        else
        {
            // 혹시 GM이 없을 때 대비(예비): 씬 로드 후 경과 시간으로 표시
            float t = Time.timeSinceLevelLoad;
            int m = Mathf.FloorToInt(t / 60f);
            int s = Mathf.FloorToInt(t % 60f);
            textTimer.text = $"{m:00}:{s:00}";
        }
    }
}
