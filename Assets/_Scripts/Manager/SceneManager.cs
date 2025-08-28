using UnityEngine;
using UnityEngine.SceneManagement; // ����Ƽ ���� SceneManager(���� ������ ���)
using System;

namespace Project   // �� ���ӽ����̽��� �츮 Ŭ���� SceneManager �̸� �浹 ȸ��
{
    public class SceneManager : MonoBehaviour
    {
        [Header("��ȯ�� ���θ޴�(�κ�) �� �̸� (Build Settings ��� �ʼ�)")]
        public string lobbySceneName = "MainMenu"; // ���� �� �̸����� ����

        [Header("�ð� ����(��). 8�� = 480��")]
        public float timeLimitSec = 60f;//480f;

        [Header("Ʈ���� ���")]
        public bool useGameManagerEvents = true;   // true: �̺�Ʈ ���� / false: �ð� ����

        private bool _loading;
        private GameManager gm;

        void OnEnable()
        {
            gm = GameManager.Instance;

            if (useGameManagerEvents)
            {
                // �� GameManager�� �������� �Ϸ�/���� ���� �� �̺�Ʈ�� �߻���Ŵ
                GameManager.OnStageCompleted += HandleStageCompleted;        // �Ϸ� ��� ȣ��:contentReference[oaicite:1]{index=1}
                GameManager.OnGameStateChanged += HandleStateChanged;        // Victory ��ȯ �� ȣ��:contentReference[oaicite:2]{index=2}
            }
        }

        void OnDisable()
        {
            if (useGameManagerEvents)
            {
                GameManager.OnStageCompleted -= HandleStageCompleted;
                GameManager.OnGameStateChanged -= HandleStateChanged;
            }
        }

        void Update()
        {
            // ���� ����� ���� �ð� üũ
            if (_loading || useGameManagerEvents) return;

            float t = (gm != null) ? gm.currentTime : Time.timeSinceLevelLoad;
            if (t >= timeLimitSec)
                LoadLobby();
        }

        private void HandleStageCompleted()
        {
            // GameManager.UpdateTimeline() -> CompleteStage() �帧���� ȣ���:contentReference[oaicite:3]{index=3}
            LoadLobby();
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.Victory)        // CompleteStage() ���ο��� Victory�� �ٲ�:contentReference[oaicite:4]{index=4}
                LoadLobby();
        }

        private void LoadLobby()
        {
            if (_loading) return;
            _loading = true;

            // ��� UI�� ����� ���⼭ ��� ��� �� �ڷ�ƾ���� ��ȯ�ϸ� ��
            UnityEngine.SceneManagement.SceneManager.LoadScene("KDY", LoadSceneMode.Single);
        }

        public void LoadPlayScene()
        {
            // �ʿ��ϸ� �ߺ� ��ȯ ������ ���� _loading üũ ��� ����
            UnityEngine.SceneManagement.SceneManager.LoadScene("KDY", LoadSceneMode.Single);
        }

        //  (�̷���) 8�� �ȿ� ���� ��� �� �ܺ�(���� ��ũ��Ʈ)���� ���� Ŭ���� ȣ�� ����
        public void NotifyBossKilled()
        {
            LoadLobby();
        }
    }
}
