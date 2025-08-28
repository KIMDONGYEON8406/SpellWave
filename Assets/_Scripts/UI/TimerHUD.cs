using UnityEngine;
using TMPro;

public class TimerHUD : MonoBehaviour
{
    public TextMeshProUGUI textTimer;

    GameManager gm;

    void Start()
    {
        gm = GameManager.Instance; // �̱���
    }

    void Update()
    {
        if (textTimer == null) return;

        // GameManager�� ������ �������� �ð� ���� �״�� ���
        if (gm != null)
        {
            // GameManager.GetFormattedStageTime() : mm:ss ��ȯ
            textTimer.text = gm.GetFormattedStageTime();
        }
        else
        {
            // Ȥ�� GM�� ���� �� ���(����): �� �ε� �� ��� �ð����� ǥ��
            float t = Time.timeSinceLevelLoad;
            int m = Mathf.FloorToInt(t / 60f);
            int s = Mathf.FloorToInt(t % 60f);
            textTimer.text = $"{m:00}:{s:00}";
        }
    }
}
