using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MukProjectileController : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;
    public float lifetime = 5f; // 5초 후에는 자동으로 파괴 (안 맞을 경우)

    private Vector3 targetPosition;
    private Vector3 direction;
    private bool isFired = false;
    private const float SPRITE_ROTATION_OFFSET = 90f;

    void Update()
    {
        if (isFired)
        {
            // 투사체는 타겟 방향으로 계속 이동합니다.
            transform.position += direction * speed * Time.deltaTime;
        }

        // 투사체 수명 관리
        lifetime -= Time.deltaTime;
        if (lifetime <= 0)
        {
            Destroy(gameObject);
        }
    }

    // MukController가 호출하여 발사 방향을 설정하는 함수
    public void FireAtTarget(Vector3 target)
    {
        targetPosition = target;
        // 1. 몬스터 위치에서 타겟 위치로 향하는 방향 벡터 계산
        direction = (targetPosition - transform.position).normalized;

        // 2. 방향 벡터를 기반으로 회전 각도 계산
        // Mathf.Atan2(y, x)는 라디안 값을 반환하므로, Deg로 변환합니다.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        angle -= SPRITE_ROTATION_OFFSET;

        // 3. 투사체 회전 적용
        // Z축을 기준으로 angle만큼 회전합니다. (2D 게임에서는 Z축 회전 사용)
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        isFired = true;
    }

    // 플레이어 충돌 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어에게 피해를 입히는 로직
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.TakeDamage(damage);
            }

            // 투사체는 플레이어에게 맞으면 사라집니다.
            Destroy(gameObject);
        }
        // 벽/장애물 충돌 처리 추가 가능
    }
}
