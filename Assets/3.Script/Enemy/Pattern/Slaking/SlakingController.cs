using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SlakingController : BossController
{
    [Header("Slaking Pattern")]
    public float attackCooltime = 6.0f; //  쿨타임

    public GameObject slamWarningPrefab;//경고 전용 프리팹 (콜라이더 없음)
    public GameObject slamDamagePrefab;// 실제 피해를 주는 프리팹 (콜라이더 있음)
    public float slamDelay = 0.5f;      // 애니메이션 시작 후 장판 생성까지의 딜레이
    private SlakingSlamController currentSlam; //  현재 생성된 데미지 장판을 추적
    private GameObject currentWarningSlam; // 현재 생성된 경고 장판 오브젝트 추적

    [Header("Slaking Jump/Slam")]
    public float slamAnimationDuration = 1.0f; // 슬램 애니메이션의 총 재생 시간
    public float jumpHeight = 2.5f;          // 점프 높이
    public float jumpDuration = 0.8f;        // 점프 이동에 걸리는 시간
    private Vector3 slamTargetPosition; // 착지 위치를 저장할 필드
    public float attackRange = 3.0f; // 공격을 시작할 사거리 (Inspector에서 설정)
    public float restTime = 3.0f;

    [Header("Sound Settings")]
    public AudioSource audioSource;           // 사운드 재생을 위한 AudioSource 컴포넌트
    public AudioClip attackSoundClip;         // 공격 시 재생할 AudioClip

    protected override void Start()
    {
        base.Start();
        attackCooltimeTimer = 0;

        currentAttackRange = attackRange;

        currentMoveSpeed = moveSpeed;
    }
    protected override void FixedUpdate()
    {

        // 2. 공격 중이거나 타겟이 없으면 강제 정지하고 리턴 (이동/정지 제어권 확보)
        if (isAttacking || playerTransform == null || rb == null)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // 강제 정지
            }
            return;
        }

        // 2. 쿨타임 감소 (공격 중이 아닐 때만)
        attackCooltimeTimer -= Time.fixedDeltaTime;

        // 3. 거리 체크
        Vector3 direction = playerTransform.position - transform.position;
        float distanceToPlayer = direction.magnitude;

        bool isCooltimeReady = attackCooltimeTimer <= 0;
        bool isInRange = distanceToPlayer <= currentAttackRange;

        if (isCooltimeReady && isInRange)
        {
            // 공격 조건 충족: 정지 및 공격 상태 전환
            rb.linearVelocity = Vector2.zero; // 강제 정지 (BossController의 base.FixedUpdate 호출 전에 정지)

            isAttacking = true; // 공격 시작 상태 설정
            // 쿨타임 초기화는 착지 시에 합니다.

            // 4. 공격 직전 타겟 위치 캡쳐 (점프 목표)
            slamTargetPosition = playerTransform.position;

            // 5. 애니메이션 시작 (이것이 Event_SlamAttack을 트리거합니다)
            if (Animator != null)
            {
                // Attack 상태로의 전환을 요청합니다.
                Animator.SetBool("IsAttacking", true);
            }
        }
        else
        {
            // 몬스터 방향 플립 로직 (EnemyController에 없다고 가정하고 추가)
            if (direction.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (direction.x > 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            // 추적 이동 실행
            Vector2 movement = direction.normalized * currentMoveSpeed;
            rb.linearVelocity = movement;
        }
    }

    // 포물선 점프 로직
    private IEnumerator JumpRoutine(Vector3 startPos, Vector3 endPos, float duration, float height)
    {
        float timer = 0f;
        if (audioSource != null && attackSoundClip != null)
        {
            // PlayOneShot을 사용하여 다른 소리가 재생 중이어도 겹치지 않게 재생합니다.
            audioSource.PlayOneShot(attackSoundClip);
        }
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.y += height * Mathf.Sin(t * Mathf.PI);

            transform.position = currentPos;
            yield return null;
        }
        transform.position = endPos; // 정확한 착지 위치

        CameraController.Instance.ShakeCamera(0.5f, 0.15f);

        //착지 완료 시 경고 장판 즉시 파괴
        if (currentWarningSlam != null)
        {
            Destroy(currentWarningSlam);
            currentWarningSlam = null; // 참조 해제
        }

        //착지 완료 후 데미지 프리팹 생성
        if (slamDamagePrefab != null)
        {
            // 기존 데미지 장판이 남아있다면 안전을 위해 정리
            if (currentSlam != null) currentSlam.DestroySlam();

            GameObject slamObject = Instantiate(slamDamagePrefab, endPos, Quaternion.identity);

            if (slamObject.TryGetComponent<SlakingSlamController>(out SlakingSlamController slamController))
            {
                currentSlam = slamController;
                
            }
        }

        // 애니메이션 Bool 해제를 먼저하고 
        if (Animator != null)
        {
            Animator.SetBool("IsAttacking", false);
        }

        //3초기다렷다가
        yield return new WaitForSeconds(restTime);
        
        // 휴식 후 데미지 장판 파괴 및 쿨타임 시작
        if (currentSlam != null)
        {
            currentSlam.DestroySlam(); // 장판 제거
            currentSlam = null; // 참조 해제 (두 번째 공격 시도 가능)
        }


        // [무한 루프/쿨타임 관리]: 점프 완료 시점에 상태 복구 및 쿨타임 초기화
        isAttacking = false; // 공격 상태 해제 -> FixedUpdate가 패턴 체크를 재개
        attackCooltimeTimer = attackCooltime; // 쿨타임 초기화 (강제 휴식 시작)
        currentMoveSpeed = moveSpeed; // 이동 속도 복구

    }
    public void Event_SlamAttack()
    {
        // 1. 장판 생성 
        // 이 장판은 콜라이더가 있으면 안됨!! 데미지가 날아갈때는 주면 안되니까
        if (slamWarningPrefab != null)
        {
            if (currentWarningSlam != null) Destroy(currentWarningSlam);

            GameObject warningObject = Instantiate(slamWarningPrefab, slamTargetPosition, Quaternion.identity);

            currentWarningSlam = warningObject;
        }
        // 2. 점프 이동 시작 (코루틴 시작)
        Vector3 startPos = transform.position;
        StartCoroutine(JumpRoutine(startPos, slamTargetPosition, jumpDuration, jumpHeight));

        // 점프가 완료 되었을때(착지 했을때) 새로운 장판이 콜라이더가 있어야함 

    }
    protected void ClearWarningArea()
    {
        if (currentWarningSlam != null)
        {
            // 몬스터가 죽으면 경고 장판을 즉시 파괴
            Destroy(currentWarningSlam);
            currentWarningSlam = null; // 참조 해제
        }
    }
    protected override IEnumerator DieAnimationRoutine()
    {
        ClearWarningArea(); // 경고 장판만 확실하게 정리합니다.

        // 1. 모든 행동 정지 (이동, 피격 처리 등)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // 콜라이더와 태그 제거 (히트박스 즉시 제거)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        gameObject.tag = "Untagged"; // 태그 제거

        // 2. 애니메이션 정지 및 빨간색으로 변경
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }

        // 3. 아이템 드롭 (사망 시점의 위치에 드롭)
        HandleGeneralItemDrop();

        float timer = 0f;
        Vector3 initialScale = transform.localScale;
        float duration = deathDuration > 0 ? deathDuration : 1.0f;

        // 4. 1초 동안 천천히 땅으로 사라지기
        while (timer < duration)
        {
            float t = timer / duration;

            // 투명도 감소 
            Color newColor = Color.Lerp(Color.red, Color.clear, t);
            spriteRenderer.color = newColor;

            // 아래로 천천히 이동
            transform.position += Vector3.down * Time.deltaTime * 0.5f;

            // 크기를 줄이는 효과
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
