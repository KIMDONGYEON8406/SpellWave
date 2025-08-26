using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// bl_Joystick
/// - UI 조이스틱(스틱+배경)을 드래그/터치로 움직이게 해주는 스크립트
/// - IPointerDownHandler / IDragHandler / IPointerUpHandler 이벤트로 동작
/// - 외부(플레이어 컨트롤러 등)에서는 Horizontal / Vertical 프로퍼티를 읽어서 입력으로 사용
/// 
/// 주의:
/// 1) 이 스크립트가 붙은 오브젝트(보통 조이스틱 배경 이미지)는 Canvas 하위에 있어야 함
/// 2) Hierarchy에 EventSystem이 있어야 UI 이벤트가 동작함
/// 3) StickRect(스틱 이미지) / CenterReference(중심 기준) 참조가 인스펙터에 연결되어 있어야 함
/// 4) 좌표 단위는 화면 좌표(UI 좌표)이며, 값 자체는 '픽셀 이동량 / Radio'로 계산됨
///    - Horizontal/Vertical이 반드시 [-1..1]로 정규화되는 것은 아님(설정에 따라 달라짐)
///    - 이동 입력으로 쓸 때는 보통 (x, y) 벡터를 정규화하거나, 최대값을 적절히 스케일링해서 사용
/// </summary>
public class bl_Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    // -------------------------
    // ─── 인스펙터 노출 설정값 ───
    // -------------------------

    [Header("Settings")]
    [SerializeField, Range(1, 15)]
    private float Radio = 5;
    // 조이스틱 반지름(정확히는 최대 이동 가능 거리 산출에 쓰는 스케일 팩터)
    // - OnDrag에서 스틱이 중심(Center)으로부터 벗어날 수 있는 최대 거리 계산에 사용
    // - 아래 private property 'radio'를 통해 실제 반지름 값으로 변환되어 사용됨

    [SerializeField, Range(0.01f, 1)]
    private float SmoothTime = 0.5f;
    // 스틱이 손을 떼었을 때 원위치(중심)로 부드럽게 돌아오는 속도(작을수록 빨리 복귀)
    // Vector3.SmoothDamp의 time 파라미터로 사용 (아래 smoothTime 프로퍼티 참고)

    [SerializeField, Range(0.5f, 4)]
    private float OnPressScale = 1.5f;
    // 스틱을 누르고 있을 때 스틱 이미지에 적용될 임시 확대 배율 (눌렀을 때 살짝 커지는 연출)

    public Color NormalColor = new Color(1, 1, 1, 1);
    // 기본 색상(스틱/배경)

    public Color PressColor = new Color(1, 1, 1, 1);
    // 눌렀을 때 바뀌는 색상(스틱/배경)

    [SerializeField, Range(0.1f, 5)]
    private float Duration = 1;
    // 색상 페이드 & 스케일 변경에 사용되는 보간 시간

    [Header("Reference")]
    [SerializeField]
    private RectTransform StickRect;
    // 실제로 움직이는 '스틱' 이미지 RectTransform (필수)

    [SerializeField]
    private RectTransform CenterReference;
    // 스틱이 돌아갈 '중심' 위치를 알려주는 기준 Transform
    // - 보통 조이스틱 배경의 중심에 놓인 빈 RectTransform
    // - 이 좌표(CenterReference.position)를 기준으로 반경 안에서만 스틱을 이동시킴


    // -------------------------
    // ─── 내부 상태값(런타임) ───
    // -------------------------

    private Vector3 DeathArea;          // 현재 프레임에서의 '중심' 좌표(= CenterReference.position을 복사)
    private Vector3 currentVelocity;    // SmoothDamp에 쓰이는 내부 속도 캐시
    private bool isFree = false;        // 손을 뗀 상태에서 '중심으로 복귀 중'이면 true, 드래그 중이면 false
    private int lastId = -2;            // 현재 조이스틱을 점유한 터치의 ID (멀티터치 간섭 방지)
    private Image stickImage;           // 스틱 이미지 (색상 페이드용)
    private Image backImage;            // 배경 이미지 (색상 페이드용)
    private Canvas m_Canvas;            // 소속 Canvas (화면 좌표 계산에 필요)
    private float diff;                 // 반지름 계산 보정용(초기 중심 위치의 벡터 크기)
    private Vector3 PressScaleVector;   // 눌렀을 때 목표 스케일 (OnPressScale, OnPressScale, OnPressScale)

    // ─────────────────────────────────────────────────────────────────────────────

    void Start()
    {
        // 필수 참조 체크: StickRect이 없으면 동작 불가
        if (StickRect == null)
        {
            Debug.LogError("Please add the stick for joystick work!.");
            this.enabled = false;
            return;
        }

        // 자신(또는 부모) 트리에서 Canvas 찾기 (최상단 또는 하위)
        if (transform.root.GetComponent<Canvas>() != null)
        {
            m_Canvas = transform.root.GetComponent<Canvas>();
        }
        else if (transform.root.GetComponentInChildren<Canvas>() != null)
        {
            m_Canvas = transform.root.GetComponentInChildren<Canvas>();
        }
        else
        {
            Debug.LogError("Required at lest one canvas for joystick work.!"); // 최소 1개 Canvas 필요
            this.enabled = false;
            return;
        }

        // 초기 중심 좌표 저장
        DeathArea = CenterReference.position;

        // 중심 좌표의 초기 '원점으로부터의 거리' (후에 반지름 보정값으로 사용)
        diff = CenterReference.position.magnitude;

        // 눌렀을 때 스케일 목표값 미리 계산
        PressScaleVector = new Vector3(OnPressScale, OnPressScale, OnPressScale);

        // 스틱/배경 이미지 캐시 + 기본 색상 세팅
        if (GetComponent<Image>() != null)
        {
            backImage = GetComponent<Image>();
            stickImage = StickRect.GetComponent<Image>();
            backImage.CrossFadeColor(NormalColor, 0.1f, true, true);
            stickImage.CrossFadeColor(NormalColor, 0.1f, true, true);
        }
    }

    void Update()
    {
        // 매 프레임, 중심 기준 좌표를 갱신 (CenterReference가 움직일 수도 있음)
        DeathArea = CenterReference.position;

        // 손이 '떨어져서'(isFree == true) 복귀 중일 때만 스무스 복귀 처리
        if (!isFree)
            return;

        // 스틱을 중심으로 부드럽게 이동 (감쇠 이동)
        StickRect.position = Vector3.SmoothDamp(StickRect.position, DeathArea, ref currentVelocity, smoothTime);

        // 중심과 충분히 가까워지면 복귀 완료 처리
        if (Vector3.Distance(StickRect.position, DeathArea) < .1f)
        {
            isFree = false;               // 더 이상 복귀 업데이트 필요 없음
            StickRect.position = DeathArea; // 위치를 정확히 중심으로 맞춤
        }
    }

    /// <summary>
    /// 터치/클릭이 시작될 때 호출(IPointerDownHandler)
    /// </summary>
    public void OnPointerDown(PointerEventData data)
    {
        // 현재 조이스틱을 점유한 터치가 없는 상태(-2)일 때만 점유
        if (lastId == -2)
        {
            // 이번 포인터의 ID를 저장하여, 같은 손가락/포인터의 드래그만 받도록 함
            lastId = data.pointerId;

            // 눌렀을 때 스케일 업/색상 페이드 코루틴 시작
            StopAllCoroutines();
            StartCoroutine(ScaleJoysctick(true));

            // 포인터 다운 시에도 즉시 위치 갱신(사용감 향상)
            OnDrag(data);

            // 색상 전환(기본 → Press)
            if (backImage != null)
            {
                backImage.CrossFadeColor(PressColor, Duration, true, true);
                stickImage.CrossFadeColor(PressColor, Duration, true, true);
            }
        }
    }

    /// <summary>
    /// 드래그 중일 때 호출(IDragHandler)
    /// - 저장된 lastId(조이스틱을 점유한 포인터)와 같은 포인터만 처리
    /// - 반지름(radio) 안에서만 StickRect를 이동
    /// </summary>
    public void OnDrag(PointerEventData data)
    {
        // 이 드래그가 현재 조이스틱을 점유한 손가락/포인터인가?
        if (data.pointerId == lastId)
        {
            isFree = false; // 드래그 중이므로 '복귀 상태'가 아님

            // 현재 포인터의 "화면상 좌표"를 m_Canvas 기준으로 얻어옴
            // (에셋에서 제공하는 유틸 함수, 멀티터치 인덱스 전달)
            Vector3 position = bl_JoystickUtils.TouchPosition(m_Canvas, GetTouchID);

            // 중심으로부터의 거리 계산 후, 반경을 넘지 않도록 제한
            if (Vector2.Distance(DeathArea, position) < radio)
            {
                // 반경 안이면 그대로 이동
                StickRect.position = position;
            }
            else
            {
                // 반경 밖이면, 중심 + (방향단위벡터 * 반지름) 위치로 클램프
                StickRect.position = DeathArea + (position - DeathArea).normalized * radio;
            }
        }
    }

    /// <summary>
    /// 터치/클릭을 뗐을 때 호출(IPointerUpHandler)
    /// - 조이스틱 점유 해제 + 색상/스케일 원복 + 중심으로 복귀 시작
    /// </summary>
    public void OnPointerUp(PointerEventData data)
    {
        // 손을 떼면 '복귀 모드' 시작
        isFree = true;
        currentVelocity = Vector3.zero;

        // 현재 포인터가 점유 포인터일 때만 해제
        if (data.pointerId == lastId)
        {
            lastId = -2; // 점유 해제(-1은 첫 터치 예약값이므로 -2 사용)

            // 스케일 다운/색상 복귀
            StopAllCoroutines();
            StartCoroutine(ScaleJoysctick(false));
            if (backImage != null)
            {
                backImage.CrossFadeColor(NormalColor, Duration, true, true);
                stickImage.CrossFadeColor(NormalColor, Duration, true, true);
            }
        }
    }

    /// <summary>
    /// 눌렀을 때/뗐을 때 스틱 이미지 스케일을 서서히 변경하는 코루틴
    /// increase == true  : 1 → OnPressScale
    /// increase == false : OnPressScale → 1
    /// </summary>
    IEnumerator ScaleJoysctick(bool increase)
    {
        float _time = 0;

        while (_time < Duration)
        {
            Vector3 v = StickRect.localScale;
            if (increase)
            {
                v = Vector3.Lerp(StickRect.localScale, PressScaleVector, (_time / Duration));
            }
            else
            {
                v = Vector3.Lerp(StickRect.localScale, Vector3.one, (_time / Duration));
            }
            StickRect.localScale = v;
            _time += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 현재 저장된 lastId(손가락 ID)와 같은 터치를
    /// Input.touches 배열에서 찾아 인덱스를 반환
    /// - 못 찾으면 -1 반환
    /// </summary>
    public int GetTouchID
    {
        get
        {
            // 모든 터치를 순회하며 같은 fingerId를 찾음
            for (int i = 0; i < Input.touches.Length; i++)
            {
                if (Input.touches[i].fingerId == lastId)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    // 실제 반경(픽셀 거리)을 계산하는 프로퍼티
    // - 기본 Radio 값에, 조이스틱 중심의 위치 변화량을 가미해 보정값을 더함
    // - CenterReference가 이동/확대되는 레이아웃에서도 반경이 크게 틀어지지 않도록 하는 장치
    private float radio
    {
        get
        {
            return (Radio * 5 + Mathf.Abs((diff - CenterReference.position.magnitude)));
        }
    }

    // SmoothDamp에 전달할 시간 상수(에셋 특유의 역변환)
    // - SmoothTime(0.01~1)을 1 - SmoothTime으로 변환해 사용
    private float smoothTime
    {
        get
        {
            return (1 - (SmoothTime));
        }
    }

    /// <summary>
    /// 가로 입력값(Horizontal)
    /// - (스틱의 X 위치 - 중심 X) / Radio
    /// - 값 범위는 설정/해상도에 따라 달라짐(대략 -1~1 근처)
    /// </summary>
    public float Horizontal
    {
        get
        {
            return (StickRect.position.x - DeathArea.x) / Radio;
        }
    }

    /// <summary>
    /// 세로 입력값(Vertical)
    /// - (스틱의 Y 위치 - 중심 Y) / Radio
    /// - 값 범위는 설정/해상도에 따라 달라짐(대략 -1~1 근처)
    /// </summary>
    public float Vertical
    {
        get
        {
            return (StickRect.position.y - DeathArea.y) / Radio;
        }
    }
}
