using UnityEngine;
using System.Collections.Generic;

public class SquirtleBullet : MonoBehaviour
{
    [Header("Orbital Settings")]
    [SerializeField] private float orbitRadius = 2.5f;
    [SerializeField] private float orbitSpeed = 150f;

    [Header("Damage Settings")]
    private float tickDamage = 5;
    public float tickInterval = 0.5f;
    private readonly Dictionary<EnemyController, float> hitMonsterTimers = new Dictionary<EnemyController, float>();

    [Header("Lifetime Settings")]
    [SerializeField] private float maxLifetime = 7f;

    private Transform centerOfRotation;
    private float currentAngle;
    private bool isInitialized = false;

    // 물줄기 관련 매개변수 (isBlastoise) 및 필드는 제거되었습니다.
    public void Initialize(Transform center, float radius, float speed, float damage, float interval, float lifetime, float initialAngle)
    {
        this.centerOfRotation = center;
        this.orbitRadius = radius;
        this.orbitSpeed = speed;
        this.tickDamage = damage;
        this.tickInterval = interval;
        this.maxLifetime = lifetime;
        currentAngle = initialAngle;
        isInitialized = true;
    }

    public float GetCurrentAngle()
    {
        return currentAngle;
    }

    public void SetAngle(float angle)
    {
        this.currentAngle = angle;
    }

    void Start()
    {
        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        if (!isInitialized || centerOfRotation == null)
        {
            Destroy(gameObject);
            return;
        }

        // 궤도 위치 계산 및 업데이트
        currentAngle += orbitSpeed * Time.deltaTime;
        float x = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * orbitRadius;
        float y = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * orbitRadius;
        transform.position = centerOfRotation.position + new Vector3(x, y, 0f);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(Mathf.RoundToInt(tickDamage));
                hitMonsterTimers.Add(enemy, 0f);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();

        if (other.CompareTag("Enemy") && enemy != null && hitMonsterTimers.ContainsKey(enemy))
        {
            float timer = hitMonsterTimers[enemy] + Time.deltaTime;
            hitMonsterTimers[enemy] = timer;

            if (timer >= tickInterval)
            {
                enemy.TakeDamage(Mathf.RoundToInt(tickDamage));
                hitMonsterTimers[enemy] = 0f;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null && hitMonsterTimers.ContainsKey(enemy))
        {
            hitMonsterTimers.Remove(enemy);
        }
    }
    // WaterJet 관련 메서드 (ActivateJet, DeactivateJet)는 제거되었습니다.
}