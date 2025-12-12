using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ChikoBullet : MonoBehaviour
{
    [Header("탄환 능력치")]
    [SerializeField] private float speed = 10f;             // 탄환의 이동 속도
    [SerializeField] private int damage = 1;                // 탄환이 주는 기본 피해량
    [SerializeField] private float slowPercentage = 0.5f;   // 적용할 슬로우 비율 (50% = 0.5)
    [SerializeField] private float slowDuration = 2f;

    [Header("Lifetime Settings")]
    public float maxLifetime = 7f;

    private Vector3 moveDirection;

    void Start()
    {
        // n초 후 자동으로 이 발사체를 파괴
        Destroy(gameObject, maxLifetime);
    }

    // ChikoritaAttack.cs에서 탄환 생성 시 이 함수를 호출하여 초기화합니다.
    public void Initialize(Vector3 direction)
    {
        moveDirection = direction.normalized;
        // 1. 방향 벡터(moveDirection)의 각도를 라디안에서 (Degree)로 변환합니다.
        //float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        //angle -= 90f;
        //transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        // 탄환을 지정된 방향으로 이동시킵니다.
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 'Enemy' 태그를 가진 오브젝트와 충돌했는지 확인합니다.
        if (other.CompareTag("Enemy"))
        {
            // EnemyController를 찾습니다. (이 스크립트는 적 몬스터에게 붙어있어야 합니다)
            EnemyController enemy = other.GetComponent<EnemyController>();

            if (enemy != null)
            {
                // 1. 피해를 적용합니다.
                enemy.TakeDamage(damage);

                if (enemy.gameObject.activeInHierarchy)
                {
                    // 3. 슬로우 디버프 적용
                    enemy.ApplySlowDebuff(slowPercentage, slowDuration);
                }
                else
                {
                    // Debug.Log("몬스터가 이미 비활성화되어 슬로우 디버프 적용을 건너뜁니다.");
                }
            }
        }

        // 충돌 후 탄환을 파괴합니다. (적에게 닿거나 화면 밖으로 나가면 파괴)
        return;
    }

}
