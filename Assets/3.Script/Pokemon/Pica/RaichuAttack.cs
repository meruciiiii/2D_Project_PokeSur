using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RaichuAttack : MonoBehaviour, IAttackInterface
{
    [Header("Projectile & Cooldown")]
    public GameObject bulletPrefab;      // Inspector에 PichuBullet Prefab을 연결
    public float bulletMoveSpeed = 4f;   // 발사체의 이동 속도 
    public float attackCooldown = 2.5f;  // 공격 간격 
    public int numberOfProjectiles = 6;  // 발사할 탄환 수 
    public float spreadAngle = 60f;      // 탄환이 흩뿌려질 부채꼴 각도 (총 60도) 
    public float attackRange = 5f;

    private float cooldownTimer;         // 쿨다운 시간을 재는 타이머
    public Animator Animator;
    private bool isAttacking = false;
    private Transform targetEnemy; // 공격 시점을 위해 타겟을 저장


    [Header("Sound Settings")]
    public AudioSource audioSource;           // 사운드 재생을 위한 AudioSource 컴포넌트
    public AudioClip attackSoundClip;         // 공격 시 재생할 AudioClip


    void Start()
    {
        // 처음 게임 시작 시 바로 공격할 수 있도록 쿨다운을 초기화.
        cooldownTimer = attackCooldown - 0.5f; // 초기 쿨다운 설정
        Animator = GetComponent<Animator>();
    }
    void Update()
    {
        ExecuteAttack();
    }
    public void ExecuteAttack()
    {
        // 1. 쿨다운 타이머 증가
        cooldownTimer += Time.deltaTime;

        // 2. 쿨다운 체크 및 공격 실행 조건 확인
        if (cooldownTimer >= attackCooldown && !isAttacking)
        {
            targetEnemy = FindNearestEnemy(); // 가장 가까운 적을 찾아 저장

            if (targetEnemy != null)
            {
                // 3. 공격 실행 및 쿨다운 초기화
                isAttacking = true;
                cooldownTimer = 0f;

                if (audioSource != null && attackSoundClip != null)
                {
                    // PlayOneShot을 사용하여 다른 소리가 재생 중이어도 겹치지 않게 재생합니다.
                    audioSource.PlayOneShot(attackSoundClip);
                }
                if (Animator != null)
                {
                    Animator.SetBool("IsAttacking", true);
                }
            }
        }
    }
    // 씬에서 가장 가까운 "Enemy" 태그를 가진 적을 찾기.
    private Transform FindNearestEnemy()
    {
        // PichuAttack의 로직 그대로 ~
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform nearest = null;
        float minDistanceSq = Mathf.Infinity;
        // 최적화: attackRange의 제곱 값 미리 계산
        float attackRangeSq = attackRange * attackRange;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float distanceSq = (enemy.transform.position - currentPos).sqrMagnitude;

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
    // 6개의 탄환을 부채꼴로 발사
    private void LaunchSixBullets(Transform target)
    {
        if (target == null)
        {
            return; // 목표가 없으면 함수 실행을 중단합니다.
        }
        // 1. 기본 발사 방향 (타겟을 향하는 방향)
        Vector3 baseDirection = (target.position - transform.position).normalized;

        // 2. 기본 각도 (라디안)
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        // 3. 각 발사체 간의 각도 및 시작 각도 계산
        // - spreadAngle (60도)를 numberOfProjectiles (6개)로 나누면 간격은 12도가 아님!
        // - (6-1)개의 간격으로 나누고, 시작 각도를 중앙에서 퍼지도록 조정합니다.
        float angleStep = spreadAngle / (numberOfProjectiles > 1 ? numberOfProjectiles - 1 : 1);
        float startAngle = baseAngle - (spreadAngle / 2f); // 중심 각도에서 절반만큼 빼서 시작

        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float currentAngle = startAngle + (angleStep * i); // 현재 발사할 탄환의 각도

            // 4. 각도를 벡터로 변환
            float dirX = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            float dirY = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
            Vector3 direction = new Vector3(dirX, dirY, 0f).normalized;

            // 5. 발사체 생성 및 초기화 (PichuBullet 사용)
            GameObject bulletInstance = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            PichuBullet bulletController = bulletInstance.GetComponent<PichuBullet>();

            if (bulletInstance == null)
            {
                Debug.LogError("RaichuAttack: 발사체 생성 중 Instantiate에 실패했습니다. 다음 탄환으로 넘어갑니다.");
                continue;
            }

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
    }
    // Animation Event로 호출: 발사체 생성 타이밍
    public void Event_LaunchBullet()
    {
        // 공격 애니메이션의 적절한 프레임에서 이 함수가 호출됩니다.

        // 미리 저장해 둔 타겟을 향해 6개의 탄환을 발사합니다.
        LaunchSixBullets(targetEnemy);

        if (Animator != null)
        {
            Animator.SetBool("IsAttacking", false);
        }

        // 공격 상태 해제
        isAttacking = false;
    }
}
