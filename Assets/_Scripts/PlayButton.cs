using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    // 버튼에서 호출할 메서드
    public void LoadPlayScene()
    {
        SceneManager.LoadScene("KDY");
    }
}
