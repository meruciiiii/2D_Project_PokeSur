using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MukController : EnemyController
{
    [Header("Muk Pattern")]
    public float projectileCooltime = 3.0f;
    private float projectileCooltimeTimer;
    public GameObject projectilePrefab; // Muk이 던질 투사체 프리팹 (Inspector에서 설정)
    public float sightRange = 8f;        // 플레이어 감지 거리 (투사체 공격 시작 거리)

    // 공격 중에는 이동을 막기 위한 플래그
    private bool isAttacking = false;

    protected override void OnEnable()
    {
        base.OnEnable(); // 기본 EnemyController의 OnEnable 실행
        projectileCooltimeTimer = 0f; // 쿨타임 초기화
        isAttacking = false;

    }

    protected override void HandleSpecialPattern()
    {
        // 공격 중이거나 플레이어가 없으면 패턴 실행 안 함
        if (isAttacking || playerTransform == null) return;

        // 1. 플레이어가 시야에 들어왔는지 확인
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= sightRange)
        {
            projectileCooltimeTimer -= Time.deltaTime;

            if (projectileCooltimeTimer <= 0)
            {
                // 2. 공격 상태로 전환 및 쿨타임 초기화
                isAttacking = true;
                projectileCooltimeTimer = projectileCooltime;

                // 3. 이동 정지 (애니메이션 준비 시간)
                currentMoveSpeed = 0f;

                // 4. Muk의 공격 애니메이션 시작
                if (Animator != null)
                {
                    Animator.SetTrigger("StartProjectileAttack");
                }
            }
        }
    }
    public void Event_LaunchProjectile()
    {
        // 1. 투사체 발사 로직
        if (projectilePrefab != null && playerTransform != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            if (projectile.TryGetComponent<MukProjectileController>(out MukProjectileController projController))
            {
                // MukProjectileController에 수정된 회전 로직이 포함되어 있습니다.
                Vector3 targetPosition = playerTransform.position;
                projController.FireAtTarget(targetPosition);
            }
        }

        // 2. 투사체 발사와 동시에 이동 속도 복구 (던진 후 바로 움직이기 시작)
        currentMoveSpeed = moveSpeed;

        // 공격 플래그 해제 (다음 공격 가능)
        isAttacking = false;
    }

}
