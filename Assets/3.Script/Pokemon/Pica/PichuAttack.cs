using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// IAttackInterface를 구현하여 ExecuteAttack() 메서드를 강제
public class PichuAttack : MonoBehaviour, IAttackInterface
{
    [Header("Projectile & Cooldown")]
    public GameObject bulletPrefab;      // Inspector에 PichuBullet Prefab을 연결합니다.
    public float bulletMoveSpeed = 3f;   // 발사체의 이동 속도
    public float attackCooldown = 3f;    // 공격 간격 (3초)
    public float attackRange = 5f;

    private float cooldownTimer;         // 쿨다운 시간을 재는 타이머
    public Animator Animator;
    private bool isAttacking = false; //중복 트리거 방지

    [Header("Sound Settings")]
    public AudioSource audioSource;           // 사운드 재생을 위한 AudioSource 컴포넌트
    public AudioClip attackSoundClip;         // 공격 시 재생할 AudioClip

    void Start()
    {
        // 처음 게임 시작 시 바로 공격할 수 있도록 쿨다운을 초기화합니다.
        cooldownTimer = attackCooldown-1;
        Animator = GetComponent<Animator>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // 컴포넌트가 없으면 추가 (선택 사항)
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    
    void Update()
    {
        ExecuteAttack();
    }

    // IAttackInterface의 계약 메서드를 구현합니다.
    public void ExecuteAttack()
    {
        // 1. 쿨다운 타이머 증가
        cooldownTimer += Time.deltaTime;

        //Animator.SetTrigger("Attack");
        // 2. 쿨다운 체크 및 공격 실행 조건 확인
        if (cooldownTimer >= attackCooldown && !isAttacking)
        {
            Transform nearestEnemy = FindNearestEnemy();

            if (nearestEnemy != null)
            {
                // 3. 공격 실행 및 쿨다운 초기화
                isAttacking = true;         // 공격 상태 
                cooldownTimer = 0f;         // 쿨다운 초기화


                if (Animator != null)
                {
                    Animator.SetBool("IsAttacking", true);
                }
            }
            // 적이 없으면 타이머를 초기화하지 않고 계속 충전 상태를 유지합니다.
        }
    }
    // 씬에서 가장 가까운 "Enemy" 태그를 가진 적을 찾습니다.
    // 가장 가까운 적의 Transform, 없으면 null
    private Transform FindNearestEnemy()
    {
        // 모든 'Enemy' 태그 오브젝트를 찾습니다.
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform nearest = null;
        float minDistanceSq = Mathf.Infinity; // 제곱 거리를 사용하면 계산이 빠릅니다.

        // 최적화: attackRange의 제곱 값 미리 계산
        float attackRangeSq = attackRange * attackRange;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float distanceSq = (enemy.transform.position - currentPos).sqrMagnitude; // 제곱 거리 계산

            // 적과의 거리가 attackRange보다 가까울 때만 (제곱 거리로 비교)
            if (distanceSq <= attackRangeSq)
            {
                // 2. 사거리 내에 있는 적 중에서 가장 가까운 적을 찾습니다.
                if (distanceSq < minDistanceSq)
                {
                    minDistanceSq = distanceSq;
                    nearest = enemy.transform;
                }
            }
        }
        return nearest;
    }
    // 발사체를 생성하고 목표 방향으로 발사합니다.
    private void LaunchBullet(Transform target)
    {
        // 발사 방향 계산 및 정규화
        Vector3 direction = (target.position - transform.position).normalized;

        // 발사체 생성 (플레이어 위치에서 생성)
        GameObject bulletInstance = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // 발사체 스크립트 가져오기
        PichuBullet bulletController = bulletInstance.GetComponent<PichuBullet>();

        if (bulletController != null)
        {
            // 발사체 초기화: 방향과 속도 전달
            bulletController.Initialize(direction, bulletMoveSpeed);
        }
        else
        {
            Debug.LogError("발사체 프리팹에 PichuBullet 스크립트가 없습니다!");
        }
    }
    // Animation Event로 호출: 발사체 생성 타이밍
    public void Event_LaunchBullet()
    {

        // 발사 직전 다시 타겟을 찾아 해당 타겟을 향해 발사합니다.
        Transform nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null)
        {

            if (audioSource != null && attackSoundClip != null)
            {
                // PlayOneShot을 사용하여 다른 소리가 재생 중이어도 겹치지 않게 재생합니다.
                audioSource.PlayOneShot(attackSoundClip);
            }
            LaunchBullet(nearestEnemy);
            //Debug.Log(">> Animation Event: 발사체 발사!");
            Animator.SetBool("IsAttacking", false);
        }
        //애니메이션이 끝났음
        isAttacking = false;
    }
}
