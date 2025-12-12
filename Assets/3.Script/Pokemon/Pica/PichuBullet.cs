using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PichuBullet : MonoBehaviour
{
    // PichuAttack.cs에서 받아올 이동 관련 변수
    private Vector3 moveDirection;
    private float currentSpeed;

    [Header("Damage Over Time (DoT) Settings")]
    public int tickDamage = 1;              // 한 틱당 입힐 피해량 (1)
    public float tickInterval = 0.3f;       // 피해를 입힐 간격 (0.3초)

    // 피해 간격을 잴 Dictionary (다수의 몬스터에게 개별적으로 DoT를 적용하기 위함)
    private readonly Dictionary<EnemyController, float> hitMonsterTimers = new Dictionary<EnemyController, float>();

    [Header("Lifetime Settings")]
    public float maxLifetime = 5f;          // 발사체의 최대 수명 (8초 후 자동 파괴)
    
    
    void Start()
    {
        // n초 후 자동으로 이 발사체를 파괴
        Destroy(gameObject, maxLifetime);
    }

    // PichuAttack.cs에서 호출하여 발사체를 초기화합니다.
    public void Initialize(Vector3 shootDirection, float moveSpeed)
    {
        this.moveDirection = shootDirection;
        this.currentSpeed = moveSpeed;
    }

    void Update()
    {
        // 1. 발사체 이동 (매 프레임 일직선으로 이동)
        transform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);
    }

    //충돌 발생 시: 첫 프레임 데미지 처리 (반드시 맞음)
    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();

        if (other.CompareTag("Enemy"))
        {
            // 1. 첫 타 데미지 적용
            enemy.TakeDamage(tickDamage);
            //Debug.Log($"[First Hit] {enemy.gameObject.name}에게 첫 타 피해 ({tickDamage}) 입힘.");

            // 2. 이 몬스터를 Dictionary에 추가하고, 다음 틱 타이머를 0으로 초기화
            // Dictionary에 이미 있는 몬스터는 (두 번째 발사체가 닿는 등의 경우) 무시합니다.
            if (!hitMonsterTimers.ContainsKey(enemy))
            {
                // 첫 타가 들어갔으므로, 쿨타임을 tickInterval만큼 주지 않고 바로 0부터 카운트 시작
                hitMonsterTimers.Add(enemy, 0f);
            }
            Destroy(gameObject);
        }
    }


    // 몬스터의 Collider에 머무르는 동안 주기적으로 피해를 입힙니다.
    // 이 함수가 작동하려면 발사체의 Collider에 Is Trigger가 체크되어야 합니다.
    private void OnTriggerStay2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();

        // Dictionary에 있는 몬스터만 지속 피해 대상입니다. (첫 타를 맞은 적)
        if (enemy != null && hitMonsterTimers.ContainsKey(enemy))
        {
            float timer = hitMonsterTimers[enemy] + Time.deltaTime;
            hitMonsterTimers[enemy] = timer; // 타이머 업데이트

            // 타이머가 간격보다 크거나 같을 때
            if (timer >= tickInterval)
            {
                // 1. 피해 적용
                enemy.TakeDamage(tickDamage);
                //Debug.Log($"[DoT Success] {enemy.gameObject.name}에게 지속 피해 ({tickDamage}) 입힘.");

                // 2. 타이머 초기화 (잔여 시간 없이 정확히 0으로)
                hitMonsterTimers[enemy] = 0f;
            }
        }
    }

    //충돌 종료 시: Dictionary에서 제거
    private void OnTriggerExit2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();

        if (enemy != null && hitMonsterTimers.ContainsKey(enemy))
        {
            hitMonsterTimers.Remove(enemy);
        }
    }
}
