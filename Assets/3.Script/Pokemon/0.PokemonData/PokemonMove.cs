using UnityEngine;

public class PokemonMove : MonoBehaviour
{
    // 트레이너를 따라가는 속도
    private float followSpeed = 9f;

    // 트레이너와 유지할 최소 거리 (이 거리보다 가까우면 멈춥니다)
    private float stopDistance = 0.7f;
    private float brakingDistance = 2.5f;
    // Rigidbody2D를 사용할 때 발생하는 떨림을 줄이는 추가 변수
    private float smoothingFactor = 0.5f;
    //애니메이션을 끄는 기준으로 사용할 최소 속도 임계값
    private float movementThreshold = 0.5f;

    private Transform targetTransform;
    private Rigidbody2D rb;
    private Animator animator;

    public void SetTarget(Transform newTarget)
    {
        targetTransform = newTarget;
    }
    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (targetTransform == null)
        {

            // 정지 상태 처리
            if (animator != null)
            {
                animator.SetBool("isRun", false);
            }
            return;
        }

        // 1. 트레이너와의 거리 벡터 및 실제 거리 계산
        Vector3 direction = targetTransform.position - transform.position;
        float distance = direction.magnitude;
        Vector2 directionNormalized = direction.normalized;


        Vector2 targetVelocity = Vector2.zero;

        // 2. 일정 거리(stopDistance) 이상 떨어져 있을 때만 움직입니다.
        if (distance > stopDistance)
        {
            float speedMultiplier = 1f;

            // 거리가 감속 시작 지점(brakingDistance)보다 작아지면, 속도를 줄입니다.
            if (distance < brakingDistance)
            {
                // 거리가 가까워질수록 속도 배율이 0에 가까워지도록 선형 보간합니다.
                // distance가 brakingDistance일 때 1, stopDistance일 때 0
                speedMultiplier = Mathf.Clamp01((distance - stopDistance) / (brakingDistance - stopDistance));
            }

            // 최종 목표 속도 계산 (감속 배율 적용)
            float targetSpeed = followSpeed * speedMultiplier;
            targetVelocity = directionNormalized * targetSpeed;

            // Rigidbody2D를 사용하여 위치를 이동시킵니다.
            // MovePosition 대신 velocity를 사용하여 물리 엔진에 의한 부드러운 이동을 유도합니다.
            // 또한, 현재 속도와 목표 속도를 보간하여 떨림을 줄입니다.
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, smoothingFactor);
            // rb.MovePosition(rb.position + targetVelocity * Time.fixedDeltaTime); // MovePosition
            // LocalScale.x의 절댓값을 사용하여 Flip 처리
            if (direction.x > 0)
            {
                // 오른쪽 이동: x 스케일 양수 (혹은 양수 절댓값)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (direction.x < 0)
            {
                // 왼쪽 이동: x 스케일 음수
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else
        {
            // 멈춰야 할 거리 안에 들어오면 속도를 0으로 설정합니다.
            rb.linearVelocity = Vector2.zero; 
        }
        if (animator != null)
        {
            bool isMoving = rb.linearVelocity.magnitude > movementThreshold;
            animator.SetBool("isRun", isMoving);
        }
    }
}