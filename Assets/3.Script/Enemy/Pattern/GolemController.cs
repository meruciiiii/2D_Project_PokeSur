using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GolemController : EnemyController
{
    [Header("Golem Pattern")]
    public float rollCooltime = 7.0f;
    private float rollCooltimeTimer;
    public float rollDuration = 5.0f;        // 구르기 가속 유지 시간
    public float rollSpeedMultiplier = 2.5f; // 구르기 시 속도 배율 (예: 2.5배)

    private bool isAttacking = false; //중복 패턴 실행 방지 플래그 

    protected override void OnEnable()
    {
        base.OnEnable(); // 기본 EnemyController의 OnEnable 실행 (속도 초기화 등)
        rollCooltimeTimer = rollCooltime; // 쿨타임 초기화
        isAttacking = false;

        if (Animator != null)
        {
            Animator.SetBool("IsRolling", false);
        }
    }

    protected override void HandleSpecialPattern()
    {
        if (isAttacking || playerTransform == null) return;

        rollCooltimeTimer -= Time.deltaTime;

        if (rollCooltimeTimer <= 0)
        {
            // 1. 공격 상태로 전환 및 쿨타임 초기화
            isAttacking = true;
            rollCooltimeTimer = rollCooltime;

            // 2. Golem의 준비 애니메이션을 시작 (애니메이션 이벤트로 Event_StartRoll 호출)
            if (Animator != null)
            {
                Animator.SetBool("IsRolling", true);
            }
        }
    }
    // 애니메이션 이벤트로 호출될 함수 - 구르기 가속 시작
    public void Event_StartRoll()
    {
        // 1. 구르기 애니메이션이 시작되는 시점: 가속 및 구르기 상태 설정
        currentMoveSpeed = moveSpeed * rollSpeedMultiplier; // 가속 시작

        // 2. 5초 후 구르기를 멈추는 코루틴 시작
        StartCoroutine(StopRollAfterDuration(rollDuration));
    }
    //구르기 지속 시간을 제어하는 코루틴
    private IEnumerator StopRollAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        // 5초 후 충돌 없이 패턴이 끝났을 때
        EndRoll();
    }
    public void EndRoll()
    {
        // 패턴 중지 로직: 원래 속도로 복구 및 플래그 해제
        currentMoveSpeed = moveSpeed;
        isAttacking = false;

        Animator.SetBool("IsRolling", false);
    }
   

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Golem의 공격 패턴은 플레이어와 닿았을 때만 적용됩니다.
        if (other.CompareTag("Player"))
        {
            // 1. 현재 구르는 중인지 확인
            if (isAttacking)
            {
                // 5초 시간 코루틴을 강제 종료
                StopCoroutine("StopRollAfterDuration");

                // 패턴 종료 로직 실행 (속도 복구, 플래그 해제)
                EndRoll();
            }
        }
    }

}
