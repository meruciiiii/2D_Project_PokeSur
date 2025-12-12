using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CharBullet : MonoBehaviour
{
    [Header("Field Settings")]
    [SerializeField] private float fieldLifetime = 10f; // 장판의 지속 시간 (10초)

    [Header("Damage Settings")]
    private float tickDamage = 5f; // 한 틱당 피해량
    [SerializeField] private float tickInterval = 0.5f; // 피해를 입히는 주기

    // 장판 안에 들어온 적(EnemyController)과 다음 피해 타이밍을 저장하는 딕셔너리
    private readonly Dictionary<EnemyController, float> hitEnemyTimers = new Dictionary<EnemyController, float>();

    // CharmanderAttack 스크립트에서 호출하여 피해량 정보를 초기화합니다.
    public void Initialize(float baseDmg, float interval)
    {
        this.tickDamage = baseDmg;
        this.tickInterval = interval;

        // 장판 수명 타이머 시작
        Destroy(gameObject, fieldLifetime);
    }

    void Update()
    {
        // 딕셔너리에 있는 모든 적들에 대해 DoT 로직을 실행합니다.
        List<EnemyController> enemiesToUpdate = new List<EnemyController>(hitEnemyTimers.Keys);

        foreach (EnemyController enemy in enemiesToUpdate)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                // 적이 파괴되었거나 비활성화되었으면 리스트에서 제거합니다.
                hitEnemyTimers.Remove(enemy);
                continue;
            }

            // 타이머 업데이트
            float timer = hitEnemyTimers[enemy] + Time.deltaTime;
            hitEnemyTimers[enemy] = timer;

            // 틱 간격이 되었는지 확인
            if (timer >= tickInterval)
            {
                // 피해를 입힙니다.
                enemy.TakeDamage(Mathf.RoundToInt(tickDamage));
                // 타이머를 리셋합니다.
                hitEnemyTimers[enemy] = 0f;
            }
        }
    }

    // 적이 장판에 진입했을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null && !hitEnemyTimers.ContainsKey(enemy))
            {
                // 적이 들어오면 딕셔너리에 추가합니다.
                hitEnemyTimers.Add(enemy, 0f);
            }
        }
    }

    // 적이 장판에서 이탈했을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null && hitEnemyTimers.ContainsKey(enemy))
            {
                // 적이 장판 밖으로 나가면 DoT 타이머에서 제거합니다.
                hitEnemyTimers.Remove(enemy);
            }
        }
    }

    // 장판이 파괴될 때 딕셔너리 정리
    private void OnDestroy()
    {
        hitEnemyTimers.Clear();
    }

}
