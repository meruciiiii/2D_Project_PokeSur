using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    public float moveDuration = 1.5f;
    private float defaultZPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            defaultZPosition = transform.position.z;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void MoveToTarget(Vector3 targetPosition, float duration)
    {
        // 카메라 흔들림을 멈추고 이동 코루틴 시작
        StopAllCoroutines();
        StartCoroutine(MoveToTargetRoutine(targetPosition, duration));
    }
    private IEnumerator MoveToTargetRoutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        // 목표 위치의 Z축은 카메라의 원래 Z축을 유지합니다.
        Vector3 endPosition = new Vector3(targetPosition.x, targetPosition.y, defaultZPosition);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Lerp를 사용하여 부드럽게 이동 (ease-out 효과를 위해 t^2를 사용할 수도 있음)
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        // 정확한 위치로 설정
        transform.position = endPosition;
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        // 기존 흔들림 코루틴이 있다면 멈추고 새로 시작
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float decayFactor = 1f - (elapsed / duration);

            // X축은 0으로 고정하여 좌우 흔들림을 제거합니다.
            float y = Random.Range(-1f, 1f) * magnitude * decayFactor;

            // 카메라의 로컬 위치를 변경하여 흔들림 효과 생성
            transform.localPosition = originalPos + new Vector3(0, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 흔들림이 끝나면 원래 위치로 복귀
        transform.localPosition = originalPos;
    }

}
