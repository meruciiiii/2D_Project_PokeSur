using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EnteiMeteorController : MonoBehaviour
{
    public GameObject fireBulletPrefab;
    public GameObject groundHazardPrefab;
    public float hazardDuration;

    public int damage = 10;            // 유성 투사체의 단일 충돌 피해량
    public float fallDuration = 0.5f;  // 낙하가 완료되는 시간
    public float shrinkDuration = 0.1f;  //  축소 애니메이션이 진행될 시간

    private Vector3 targetGroundPosition; // 유성이 착지할 땅의 위치
    private float currentSpeed;
    private float XPatternSpeed = 6;
    private Rigidbody2D rb;
    private static bool isCrossPattern = true;

    private bool isShrinking = false; // 축소 코루틴의 중복 실행을 막는 플래그

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector3 targetPos, float speed, GameObject bulletPrefab, GameObject hazardPrefab, float duration)
    {
        targetGroundPosition = targetPos;
        targetGroundPosition.z = 0f;

        currentSpeed = speed;
        fireBulletPrefab = bulletPrefab;
        groundHazardPrefab = hazardPrefab;
        hazardDuration = duration;

        // 낙하 방향은 생성 위치(높은 곳)에서 목표 지면(낮은 곳)을 향합니다.
        Vector3 direction = (targetGroundPosition - transform.position).normalized;

        if (rb != null)
        {
            // 속도를 일정하게 유지하며 낙하
            rb.linearVelocity = direction * currentSpeed;
        }

        // 특정 시간 후 목표 지점에 도달하도록 보장 (선택 사항: 코루틴으로 보간하여 정확도를 높일 수 있으나 단순 구현)
        // 여기서는 rigidbody를 사용하고, 충돌 시 파괴되도록 하겠습니다.
    }
    void FixedUpdate()
    {
        // 목표 위치에 도달했는지 확인합니다.
        if (transform.position.y <= targetGroundPosition.y)
        {
            // 이미 축소 중이라면 (코루틴이 실행 중이라면) 다시 시작하지 않습니다.
            if (!isShrinking)
            {
                StartCoroutine(ShrinkAndDestroy());
                CameraController.Instance.ShakeCamera(0.5f, 0.08f);

            }

        }
    }
    // 유성 착지 또는 플레이어 충돌 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isShrinking) return; // 이미 축소 중이면 중복 실행 방지

        if (other.CompareTag("Player"))
        {
            // 플레이어에게 데미지 적용
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.TakeDamage(damage);
                Debug.Log($"Entei Meteor: Player took {damage} damage.");
            }
        }
    }
    
    private IEnumerator ShrinkAndDestroy()
    {
        isShrinking = true;

        // Rigidbody 움직임을 즉시 멈춥니다.
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        //유성이 완전히 사라지기 직전, 패턴을 발사합니다.
        FireXpettern(targetGroundPosition);

        yield return new WaitForSeconds(1);

        // 오브젝트 파괴
        Destroy(gameObject);
    }
    
    private void FireXpettern(Vector3 targetPosition)
    {
        // 투사체의 크기 설정
        float scaleFactor = 3f;

        if (isCrossPattern)
        {
            FireCrossPattern(targetPosition, scaleFactor);
        }
        else
        {
            FirePlusPattern(targetPosition, scaleFactor);
        }

        // 패턴 전환 (X -> + / + -> X)
        isCrossPattern = !isCrossPattern;
    }

    /// <summary>
    /// X자 (대각선) 패턴 발사
    /// </summary>
    private void FireCrossPattern(Vector3 targetPosition, float scale)
    {
        // X자 패턴 각도: 45°, 135°, 225°, 315°
        float[] angles = { 45f, 135f, 225f, 315f };
        foreach (float angle in angles)
        {
            InstantiateFireBullet(angle, targetPosition, scale);
        }
    }

    /// <summary>
    /// +자 (직선) 패턴 발사
    /// </summary>
    private void FirePlusPattern(Vector3 targetPosition, float scale)
    {
        // +자 패턴 각도: 0°, 90°, 180°, 270°
        float[] angles = { 0f, 90f, 180f, 270f };
        foreach (float angle in angles)
        {
            InstantiateFireBullet(angle, targetPosition, scale);
        }
    }

    private void InstantiateFireBullet(float angle, Vector3 position, float scale)
    {
        // 1. 발사 방향 계산
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Vector2 direction = rotation * Vector2.right;

        // 2. 투사체 생성
        GameObject bullet = Instantiate(fireBulletPrefab, position, rotation);

        // 3. 스케일을 설정 (절반 크기)
        bullet.transform.localScale = Vector3.one * scale;

        // 4. 스크립트 초기화 (이동 시작)
        EnteiFireBullet bulletController = bullet.GetComponent<EnteiFireBullet>();
        if (bulletController != null)
        {
            bulletController.Initialize(direction, XPatternSpeed, groundHazardPrefab, hazardDuration);
        }
    }
}
