using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 

public class EnteiController : BossController
{
    [Header("Entei Pattern")]
    public GameObject fireballPrefab;   // 엔테이의 화염구 투사체 프리팹
    public float projectileSpeed = 8f;   // 투사체 속도
    public int burstCount = 3;    // 한 번 공격 시 연사할 횟수
    public float burstInterval = 0.2f;  // 연사 간격
    public float castTime = 1.0f;        // 공격 전 준비(캐스팅) 시간
    public float attackCooltime = 5.0f;  // 공격 후 쿨타임
    public float attackRange = 10.0f;

    //엔테이의 공격 패턴 정의
    private enum AttackPattern { GroundHazard, AerialAoE }

    //다음 공격 시 사용할 패턴을 추적
    private AttackPattern nextPattern = AttackPattern.GroundHazard;

    [Header("Entei Pattern 1: Ground Hazard")]
    public GameObject groundHazardPrefab; // 패턴 1 (지속 피해 장판) 프리팹
    public GameObject fireBulletPrefab; //장판 생성용 투사체 프리팹
    public float hazardDuration = 3.0f;     // 장판 지속 시간

    [Header("Entei Pattern 2: Aerial AoE")]
    public GameObject warningAreaPrefab;  // 패턴 2 (유성 낙하) 경고 장판 프리팹
    public GameObject meteorPrefab;       // 패턴 2 (유성 낙하) 투사체 프리팹
    public int meteorCount = 5;          // 낙하 횟수
    public float meteorInterval = 0.5f;  // 낙하 간격

    [Header("Sound Settings")]
    public AudioSource audioSource;           // 사운드 재생을 위한 AudioSource 컴포넌트
    public AudioClip attack1SoundClip;         // 공격 시 재생할 AudioClip
    public AudioClip attack1_1SoundClip;         // 공격 시 재생할 AudioClip
    public AudioClip attack2SoundClip;         // 공격 시 재생할 AudioClip

    // **애니메이션 이벤트 제어용 변수
    private bool canLaunchGroundHazard = false; // 패턴 1 발사 준비 플래그
    private bool canLaunchAerialAoE = false;   // 패턴 2 발사 시작 플래그
    private int groundHazardCount = 0;         // 패턴 1 발사 횟수 카운터

    protected override void Start()
    {
        base.Start();

        // 요청하신 대로, 스폰 시 쿨타임을 0으로 설정하여 즉시 공격 준비
        attackCooltimeTimer = 0f;
        currentAttackRange = attackRange;
        currentMoveSpeed = moveSpeed;

        // 보스 이름 설정 (BossController의 DieAnimationRoutine에서 사용)
        bossName = "Entei";

        //BossHPUI 연결 및 UI 활성화
        if (BossHPUI.Instance != null)
        {
            BossHPUI.Instance.ActivateBossUI(this);
        }
    }

    protected override void FixedUpdate()
    {
        // BossController의 FixedUpdate를 재정의하여 이동과 공격 패턴 체크를 분리
        if (isAttacking || playerTransform == null || rb == null)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // 공격 중/대기 중 강제 정지
            }
            return;
        }

        // 이동 로직 (BossController에서 가져옴)
        Vector3 direction = playerTransform.position - transform.position;
        Vector2 movement = direction.normalized * currentMoveSpeed;
        rb.linearVelocity = movement;

        // 스프라이트 좌우 반전 로직 (EnemyController에서 가져옴)
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    protected override void HandleSpecialPattern()
    {
        if (isAttacking || playerTransform == null) return;

        // 쿨타임 감소 (FixedUpdate가 이동을 담당하므로 Update에서 쿨타임 처리)
        attackCooltimeTimer -= Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);

        bool isCooltimeReady = attackCooltimeTimer <= 0;
        bool isInRange = distanceToPlayer <= currentAttackRange;

        if (isCooltimeReady && isInRange)
        {
            //  공격 시작!
            isAttacking = true;
            StartCoroutine(PatternManagerRoutine());
        }
        else if (!isAttacking)
        {
            // 공격 범위 밖이라면 이동 (`FixedUpdate`에서 이동 로직을 수행함)
        }
    }
    private IEnumerator PatternManagerRoutine()
    {
        // 1. 공격 애니메이션을 실행하여 캐스팅 시간(castTime)을 애니메이션 길이로 대체합니다.
        if (Animator != null)
        {
            if (nextPattern == AttackPattern.GroundHazard)
            {
                Animator.SetTrigger("Cast_GroundHazard");


            }
            else
            {
                Animator.SetTrigger("Cast_AerialAoE");
                if (audioSource != null && attack1_1SoundClip != null)
                {
                    // PlayOneShot을 사용하여 다른 소리가 재생 중이어도 겹치지 않게 재생합니다.
                    audioSource.PlayOneShot(attack1_1SoundClip);
                }
            }
        }

        switch (nextPattern)
        {
            case AttackPattern.GroundHazard:
                Debug.Log("Entei: 패턴 1 (지면 장판) 시작. 이벤트 대기 중...");
                yield return StartCoroutine(GroundHazardRoutine_EventDriven()); // 패턴 1 이벤트 기반 코루틴 호출
                nextPattern = AttackPattern.AerialAoE; // 다음 패턴으로 전환
                break;

            case AttackPattern.AerialAoE:
                Debug.Log("Entei: 패턴 2 (유성 낙하) 시작. 이벤트 대기 중...");
                yield return StartCoroutine(AerialAoERoutine_EventDriven()); // 패턴 2 이벤트 기반 코루틴 호출
                nextPattern = AttackPattern.GroundHazard; // 다음 패턴으로 전환
                break;
        }

        // 3. 공격 후 휴식/쿨타임 대기 (각 패턴 루틴이 끝난 후 실행됨)
        isAttacking = false;
        attackCooltimeTimer = attackCooltime;
    }

    //패턴 1: 지면 추적 장판 공격 (애니메이션 이벤트 기반)
    private IEnumerator GroundHazardRoutine_EventDriven()
    {
        groundHazardCount = 0; // 발사 횟수 초기화

        // 3회 발사 루프
        for (int i = 0; i < 3; i++)
        {
            canLaunchGroundHazard = false; // 발사 플래그 리셋

            // 애니메이션 이벤트(Event_LaunchGroundHazard)가 canLaunchGroundHazard를 true로 바꿀 때까지 대기
            yield return new WaitUntil(() => canLaunchGroundHazard);

            // [실제 발사 로직]
            if (fireBulletPrefab != null && playerTransform != null)
            {
                if (audioSource != null && attack1SoundClip != null)
                {
                    // PlayOneShot을 사용하여 다른 소리가 재생 중이어도 겹치지 않게 재생합니다.
                    audioSource.PlayOneShot(attack1SoundClip);
                }
                Vector3 targetPosition = playerTransform.position;
                Vector3 direction = (targetPosition - transform.position).normalized;

                Vector3 spawnOffset = direction * 2.0f; // 1.0f는 적절한 오프셋 값

                GameObject projectile = Instantiate(
                    fireBulletPrefab,
                    transform.position + spawnOffset,
                    Quaternion.identity
                );

                // 장판 투사체 초기화
                if (projectile.TryGetComponent<EnteiFireBullet>(out EnteiFireBullet projectileController))
                {
                    projectileController.Initialize(direction, projectileSpeed, groundHazardPrefab, hazardDuration);
                }
            }

            groundHazardCount++;

            // 다음 발사까지 1.0초 대기
            if (i < 2) // 마지막 발사 후에는 대기하지 않음
            {
                yield return new WaitForSeconds(0.5f);

                // 다음 발사를 위해 애니메이션을 다시 트리거하여 이벤트를 다시 받습니다.
                if (Animator != null)
                {
                    Animator.SetTrigger("Cast_GroundHazard");
                }
            }
        }
    }

    public void Event_LaunchGroundHazard()
    {
        canLaunchGroundHazard = true;
    }

    //패턴 2: 유성 낙하 공격 로직 (애니메이션 이벤트 기반)
    private IEnumerator AerialAoERoutine_EventDriven()
    {
        canLaunchAerialAoE = false; // 발사 대기 상태 설정

        // 애니메이션 이벤트(Event_LaunchAerialAoE)가 canLaunchAerialAoE를 true로 바꿀 때까지 대기
        yield return new WaitUntil(() => canLaunchAerialAoE);

        // [실제 유성 낙하 루프 시작]
        Debug.Log("패턴 2: 유성 낙하 루프 시작.");

        // 유성 낙하 반복 (총 meteorCount 횟수)
        for (int i = 0; i < meteorCount; i++)
        {
            if (playerTransform == null) break;

            // 2-1. 현재 플레이어 위치를 타겟으로 지정
            Vector3 targetPosition = playerTransform.position;

            // 2-2. 경고 장판 생성
            if (warningAreaPrefab != null)
            {
                // 경고 장판은 다음 낙하까지의 간격 동안 유지
                GameObject warning = Instantiate(warningAreaPrefab, targetPosition, Quaternion.identity);
                Destroy(warning, meteorInterval);
            }
            if (audioSource != null && attack2SoundClip != null)
            {
                // PlayOneShot을 사용하여 다른 소리가 재생 중이어도 겹치지 않게 재생합니다.
                audioSource.PlayOneShot(attack2SoundClip);
            }
            // 2-3. 경고 시간(meteorInterval)만큼 대기
            yield return new WaitForSeconds(meteorInterval);

            // 2-4. 유성 낙하 (유성 투사체 생성)
            if (meteorPrefab != null)
            {
                // 유성 생성 로직은 그대로 유지
                Vector3 spawnHeight = new Vector3(0f, 10f, 0f);
                Vector3 spawnPosition = targetPosition + spawnHeight;
                GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);

                if (meteor.TryGetComponent<EnteiMeteorController>(out EnteiMeteorController meteorController))
                {
                    // EnteiMeteorController에 필요한 모든 인수를 넘깁니다.
                    meteorController.Initialize(
                        targetPosition,
                        projectileSpeed * 2f,
                        fireBulletPrefab,     // 장판 투사체 프리팹
                        groundHazardPrefab,   // 장판 프리팹
                        hazardDuration        // 장판 지속 시간
                    );
                }
            }
            // 유성 투사체가 착지 시 FireXpettern을 호출하는 로직은 EnteiMeteorController에 있습니다.
        }
    }
    // (Animator에서 "Cast_AerialAoE" 애니메이션의 원하는 시작 시점에 연결)
    public void Event_LaunchAerialAoE()
    {
        canLaunchAerialAoE = true;
    }

    protected override IEnumerator DieAnimationRoutine()
    {
        this.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 물리적인 속도 정지
        }
        if (PlayerManager.Instance != null)
        {
            // PlayerManager에게 이동 스크립트 비활성화를 요청합니다.
            PlayerManager.Instance.DisableMovement();
        }
        if (CameraController.Instance != null)
        {
            // MoveToTarget을 호출하여 카메라를 엔테이 위치로 부드럽게 이동시킵니다.
            // CameraController의 moveDuration 변수를 사용합니다.
            float moveDuration = CameraController.Instance.moveDuration;
            CameraController.Instance.MoveToTarget(transform.position, moveDuration);

            // 카메라 이동 시간만큼 씬 로드를 지연하여 클리어 장면을 보여줍니다.
            yield return new WaitForSeconds(moveDuration);
        }
        if (CameraController.Instance != null)
        {
            CameraController.Instance.ShakeCamera(1.0f, 0.05f);
        }

        yield return base.DieAnimationRoutine();

        // 엔테이가 완전히 사라진 후, GameClear 씬을 로드합니다.
        SceneManager.LoadScene("GameClear");
    }
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (BossHPUI.Instance != null)
        {
            BossHPUI.Instance.UpdateBossHP(currentHealth);
        }
    }

}
