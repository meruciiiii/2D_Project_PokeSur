using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Ball_Indicator : MonoBehaviour
{
    // GameManager에서 설정할 몬스터 볼의 Transform
    public Transform targetTransform;

    //[변경] 회전이 필요한 요소의 Transform (Rotating_Arrow)
    public Transform rotatingImageTransform;

    // [변경] 고정되어야 하는 요소의 Image 컴포넌트 (Fixed_Icon)
    public Image fixedImage;

    // 화면 가장자리 여백 (픽셀)
    [SerializeField] private float borderPadding = 30f;
    private Camera mainCamera; // 카메라 캐싱

    void Awake()
    {
        // gameObject.SetActive(true) 제거 (LateUpdate가 호출되도록 Root는 켜둡니다)

        // Image 컴포넌트 체크 대신, 새로운 필드가 연결되었는지 확인합니다.
        if (rotatingImageTransform == null)
        {
            Debug.LogError("Ball_Indicator: 회전할 요소(Rotating Image Transform)가 연결되지 않았습니다.");
        }
        if (fixedImage == null)
        {
            Debug.LogError("Ball_Indicator: 고정할 요소(Fixed Image)가 연결되지 않았습니다.");
        }

        mainCamera = Camera.main; // 메인 카메라 캐싱
        if (mainCamera == null)
        {
            Debug.LogError("Ball_Indicator: 씬에 'MainCamera' 태그를 가진 카메라가 없습니다!");
        }

        // 시작 시에는 숨깁니다. (두 이미지 모두 비활성화)
        SetIndicatorActive(false);
    }

    public void ManualUpdate()
    {
        if (targetTransform == null || mainCamera == null)
        {
            SetIndicatorActive(false);
            return;
        }


        // 1. 월드 좌표를 화면 좌표로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetTransform.position);

        // 2. 화면 안에 있는지 확인 (Z축은 카메라 앞에 있는지 확인)
        bool isOffScreen = screenPos.x <= 0 || screenPos.x >= Screen.width ||
                            screenPos.y <= 0 || screenPos.y >= Screen.height || screenPos.z < 0;


        if (isOffScreen)
        {
            SetIndicatorActive(true);

            // 3. 화면 중앙을 기준으로 방향 벡터 계산
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            Vector3 direction = screenPos - screenCenter;

            // 4. 방향 벡터를 화면 가장자리에 고정하고 회전
            direction.Normalize();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (rotatingImageTransform != null)
            {
                // 자식에게만 회전 적용
                rotatingImageTransform.rotation = Quaternion.Euler(0, 0, angle);
            }
            // Root 오브젝트의 회전은 건드리지 않아 자식인 Fixed_Icon은 고정됩니다.

            // 5. 화살표 위치 설정 (화면 가장자리에 고정)
            // [유지] 위치는 Root 오브젝트(this.transform)에 적용합니다.
            float halfWidth = Screen.width / 2f - borderPadding;
            float halfHeight = Screen.height / 2f - borderPadding;

            Vector2 position = direction;
            float ratio = Mathf.Min(halfWidth / Mathf.Abs(position.x), halfHeight / Mathf.Abs(position.y));
            position *= ratio;

            transform.position = screenCenter + (Vector3)position;

        }
        else
        {
            // 화면 안에 있으면 숨기기
            SetIndicatorActive(false);
        }
    }

    // 화살표 활성화/비활성화
    public void SetIndicatorActive(bool isActive)
    {

        // 1. 회전 이미지 활성화/비활성화
        if (rotatingImageTransform != null)
        {
            Image rotatingImg = rotatingImageTransform.GetComponent<Image>();
            if (rotatingImg != null && rotatingImg.enabled != isActive)
            {
                rotatingImg.enabled = isActive;
            }
        }

        // 2. 고정 이미지 활성화/비활성화
        if (fixedImage != null && fixedImage.enabled != isActive)
        {
            fixedImage.enabled = isActive;
        }
    }

    // 몬스터 볼이 드롭/생성될 때 호출
    public void SetTarget(Transform newTarget)
    {
        targetTransform = newTarget;
        // 타겟이 비었을 때는 인디케이터를 확실히 끕니다.
        if (newTarget == null)
        {
            SetIndicatorActive(false);
        }
    }
}