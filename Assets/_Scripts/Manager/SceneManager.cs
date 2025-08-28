using UnityEngine;
using UnityEngine.SceneManagement; // 유니티 내장 SceneManager(완전 한정명 사용)
using System;

namespace Project   // ★ 네임스페이스로 우리 클래스 SceneManager 이름 충돌 회피
{
    public class SceneManager : MonoBehaviour
    {
        [Header("전환할 메인메뉴(로비) 씬 이름 (Build Settings 등록 필수)")]
        public string lobbySceneName = "MainMenu"; // 실제 씬 이름으로 변경

        [Header("시간 제한(초). 8분 = 480초")]
        public float timeLimitSec = 60f;//480f;

        [Header("트리거 방식")]
        public bool useGameManagerEvents = true;   // true: 이벤트 구독 / false: 시간 폴링

        private bool _loading;
        private GameManager gm;

        void OnEnable()
        {
            gm = GameManager.Instance;

            if (useGameManagerEvents)
            {
                // ★ GameManager가 스테이지 완료/상태 변경 시 이벤트를 발생시킴
                GameManager.OnStageCompleted += HandleStageCompleted;        // 완료 즉시 호출:contentReference[oaicite:1]{index=1}
                GameManager.OnGameStateChanged += HandleStateChanged;        // Victory 전환 시 호출:contentReference[oaicite:2]{index=2}
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
            // 폴링 모드일 때만 시간 체크
            if (_loading || useGameManagerEvents) return;

            float t = (gm != null) ? gm.currentTime : Time.timeSinceLevelLoad;
            if (t >= timeLimitSec)
                LoadLobby();
        }

        private void HandleStageCompleted()
        {
            // GameManager.UpdateTimeline() -> CompleteStage() 흐름에서 호출됨:contentReference[oaicite:3]{index=3}
            LoadLobby();
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.Victory)        // CompleteStage() 내부에서 Victory로 바꿈:contentReference[oaicite:4]{index=4}
                LoadLobby();
        }

        private void LoadLobby()
        {
            if (_loading) return;
            _loading = true;

            // 결과 UI가 생기면 여기서 잠깐 띄운 뒤 코루틴으로 전환하면 됨
            UnityEngine.SceneManagement.SceneManager.LoadScene("KDY", LoadSceneMode.Single);
        }

        public void LoadPlayScene()
        {
            // 필요하면 중복 전환 방지를 위해 _loading 체크 사용 가능
            UnityEngine.SceneManagement.SceneManager.LoadScene("KDY", LoadSceneMode.Single);
        }

        //  (미래용) 8분 안에 보스 사망 시 외부(보스 스크립트)에서 조기 클리어 호출 가능
        public void NotifyBossKilled()
        {
            LoadLobby();
        }
    }
}
