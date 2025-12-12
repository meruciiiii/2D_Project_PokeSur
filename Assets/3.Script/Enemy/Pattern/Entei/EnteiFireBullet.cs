using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EnteiFireBullet : MonoBehaviour
{
    // === [단일 충돌 피해 관련] ===
    public int damage = 5;               //  직접 충돌 시 피해량
    public float lifetime = 8.0f;        // 투사체 자체의 유지 시간

    // === [장판 생성 관련] ===
    private GameObject firePrefab;        // 장판 프리팹 (groundHazardPrefab)
    private float hazardDuration;        // 장판 지속 시간
    public float hazardSpawnInterval = 0.15f; // 장판을 깔 간격

    // === [이동 관련] ===
    private float projectileSpeed;       // 투사체 속도
    private Vector2 moveDirection;       // 이동 방향
    private Rigidbody2D rb;
    private float lastSpawnTime;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Error: Rigidbody2D 컴포넌트를 찾을 수 없습니다! 프리팹에 Rigidbody2D를 추가했는지 확인하세요.");
        }

        // 일정 시간 후 스스로 파괴
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 direction, float speed, GameObject fire, float duration)
    {
        moveDirection = direction.normalized;
        projectileSpeed = speed;
        firePrefab = fire;
        hazardDuration = duration;


        // 첫 장판 생성
        SpawnFire();
        lastSpawnTime = Time.time;
    }

    void Update()
    {
        transform.position += (Vector3)moveDirection * projectileSpeed * Time.deltaTime;

        // 정해진 간격마다 장판 생성
        if (Time.time > lastSpawnTime + hazardSpawnInterval)
        {
            SpawnFire();
            lastSpawnTime = Time.time;
        }
    }

    private void SpawnFire()
    {
        if (firePrefab != null)
        {
            GameObject fire = Instantiate(firePrefab, transform.position, Quaternion.identity);

            // 장판 컨트롤러에 지속 시간을 전달합니다.
            if (fire.TryGetComponent<EnteiFireController>(out EnteiFireController fireController))
            {
                fireController.hazardDuration = this.hazardDuration;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 플레이어 충돌 감지
        if (other.CompareTag("Player"))
        {
            // 2. 플레이어에게 데미지 적용
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.TakeDamage(damage);
                Debug.Log($"Entei Fire Bullet: Player took {damage} direct damage.");
            }
        }
    }
}
