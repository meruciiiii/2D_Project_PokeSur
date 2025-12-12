using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChikoAttack : MonoBehaviour, IAttackInterface
{
    [Header("공격 설정")]
    public GameObject chikoBulletPrefab;

    [SerializeField] private float attackRate = 1.0f; // 초당 공격 횟수 (1초에 1번 공격)
    [SerializeField] private float attackRange = 10f; // 공격 사거리 (추적 범위)

    public Animator animator;
    // 애니메이터에 설정할 공격 트리거 또는 Bool 이름
    [SerializeField] private string attackBoolName = "IsAttacking";
    private bool isAttacking = false;

    // 플레이어의 현재 위치와 방향을 가져오기 위해 필요합니다.
    private Transform playerTransform;
    private float attackCooldown;
    private float timeSinceLastAttack = 0f;
    private Transform currentTarget; // 현재 타겟을 저장할 변수

    [Header("Sound Settings")]
    public AudioSource audioSource;           // 사운드 재생을 위한 AudioSource 컴포넌트
    public AudioClip attackSoundClip;         // 공격 시 재생할 AudioClip

    void Start()
    {
        // 공격 쿨타임을 계산합니다.
        attackCooldown = 1f / attackRate;
        playerTransform = transform;
        if (animator == null) animator = GetComponent<Animator>();

    }

    void Update()
    {
        // 매 프레임 ExecuteAttack()을 호출하여 공격할 시점인지 확인합니다.
        ExecuteAttack();
    }
    public void ExecuteAttack()
    {
        if (chikoBulletPrefab == null)
        {
            Debug.LogError("ChikoBullet Prefab이 설정되지 않았습니다. 인스펙터 창에서 프리팹을 연결해주세요.");
            return;
        }

        // isAttacking이 false일 때만 공격 시도
        if (!isAttacking)
        {
            timeSinceLastAttack += Time.deltaTime;

            if (timeSinceLastAttack >= attackCooldown)
            {
                // 1. 타겟 찾기
                currentTarget = FindNearestEnemy();

                if (currentTarget != null)
                {
                    // 2. 공격 상태 진입 및 쿨타임 리셋
                    isAttacking = true;
                    timeSinceLastAttack = 0f;

                    // 3. 애니메이션 시작 (Event_SpawnBullet이 호출될 때까지 대기)
                    if (animator != null)
                    {
                        animator.SetBool(attackBoolName, true);
                        if (audioSource != null && attackSoundClip != null)
                        {
                            // PlayOneShot을 사용하여 다른 소리가 재생 중이어도 겹치지 않게 재생합니다.
                            audioSource.PlayOneShot(attackSoundClip);
                        }
                    }
                    else
                    {
                        // 애니메이터가 없으면 즉시 발사 (폴백)
                        Event_SpawnBullet();
                    }
                }
            }
        }
    }
    public void Event_SpawnBullet()
    {
        // 1. 발사 로직
        if (currentTarget != null)
        {
            SpawnBullet(currentTarget.position);
        }
        else
        {
            //Debug.LogWarning("Event_SpawnBullet: 타겟이 사라져 발사하지 못했습니다.");
        }

        // 2. 상태 초기화
        isAttacking = false;
        currentTarget = null;

        // 3. 애니메이션 종료
        if (animator != null)
        {
            animator.SetBool(attackBoolName, false);
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);

        Transform nearestEnemy = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 directionToTarget = hit.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude; // 거리 제곱 (성능 향상)

                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    nearestEnemy = hit.transform;
                }
            }
        }
        return nearestEnemy;
    }
    // 실제 탄환을 생성하고 발사하는 로직
    private void SpawnBullet(Vector3 targetPosition)
    {
        // 1. 탄환이 생성될 위치 (치코리타 위치)
        Vector3 spawnPosition = playerTransform.position;

        // 2. 탄환이 날아갈 방향 설정 (타겟 방향)
        Vector3 bulletDirection = targetPosition - spawnPosition;

        // 3. 탄환 생성
        GameObject newBullet = Instantiate(chikoBulletPrefab, spawnPosition, Quaternion.identity);

        // 4. ChikoBullet 스크립트를 찾아 초기화합니다.
        ChikoBullet bulletController = newBullet.GetComponent<ChikoBullet>();

        if (bulletController != null)
        {
            // 탄환의 이동 방향을 전달합니다.
            bulletController.Initialize(bulletDirection);
        }
        else
        {
            Debug.LogError("ChikoBullet 프리팹에 ChikoBullet.cs 스크립트가 없습니다!");
            Destroy(newBullet);
        }
    }
}
