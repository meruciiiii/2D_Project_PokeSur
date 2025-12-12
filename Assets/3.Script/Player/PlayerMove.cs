using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float decelerationRate = 10f;//미끄러지는 거리

    private Rigidbody2D rb;
    private Vector2 movementInput; // 키 입력 값만 저장
    private Vector2 currentVelocity; // 현재 실제 움직이는 속도 (관성 포함)
    private Animator animator;

    void Start()
    {
        // 오브젝트에 붙어있는 Rigidbody2D 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // 입력 처리: 프레임 속도에 맞춰 입력 받음
    void Update()
    {
        // isGamePaused가 true이면, 입력 감지 및 방향 전환 로직을 건너뛰고 바로 종료합니다.
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
        {
            // 입력 감지 및 방향 전환을 막고, 애니메이션을 '멈춘 상태'로 설정
            animator.SetBool("IsMoving", false);
            return;
        }

        // WASD 또는 방향키 입력 (X: 좌우, Y: 상하)
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        movementInput.Normalize();

        if (movementInput.x < 0)
        {
            // 오른쪽 이동: X 스케일을 양수(1)로 설정 (정방향)
            transform.localScale = new Vector3(3.5f, 3.5f, 3.5f);
        }
        else if (movementInput.x > 0)
        {
            // 왼쪽 이동: X 스케일을 음수(-1)로 설정 (반전)
            transform.localScale = new Vector3(-3.5f, 3.5f, 3.5f);
        }

        bool isMoving = movementInput.magnitude > 0; // movement 벡터의 크기가 0보다 크면 이동 중

        // Animator의 "IsMoving" 파라미터 값을 설정
        animator.SetBool("IsMoving", isMoving);

    }

    // 물리 처리: 일정한 시간 간격(Fixed Update)으로 처리
    void FixedUpdate()
    {
        // Time.timeScale = 0f 일 때 FixedUpdate는 호출되지만, 안전하게 한 번 더 막습니다.
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
        {
            // 속도를 0으로 초기화하여 잔여 관성 움직임도 막습니다.
            currentVelocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero; // Rigidbody의 자체 속도도 초기화 (안전용)
            return;
        }

        // 1. 가속/감속 로직 (미끄러짐 효과 구현)
        if (movementInput.magnitude > 0)
        {
            // 키를 누르고 있을 때: 입력 방향으로 바로 가속
            currentVelocity = movementInput * moveSpeed;
        }
        else
        {
            // 키를 떼었을 때: 현재 속도를 서서히 감소 (감속/제동)
            // Vector2.MoveTowards는 currentVelocity를 (0,0) 방향으로 decelerationRate만큼 이동시킴
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero,
                                                  decelerationRate * Time.fixedDeltaTime);
        }

        // 2. Kinematic Rigidbody 이동
        // 계산된 currentVelocity를 사용하여 위치를 이동
        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
    }
}
