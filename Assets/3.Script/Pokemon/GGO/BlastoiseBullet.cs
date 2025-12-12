using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


// 이 스크립트는 궤도 발사체 + WaterJet 활성화 로직을 포함합니다.
public class BlastoiseBullet : MonoBehaviour
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

    [Header("Blastoise Feature")]
    public GameObject waterJetObject; // WaterJet.cs 컴포넌트를 가진 자식 오브젝트 연결
    private WaterJet waterJetDamageComponent;

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

        if (waterJetObject != null)
        {
            waterJetDamageComponent = waterJetObject.GetComponent<WaterJet>();
            if (waterJetDamageComponent != null)
            {
                waterJetDamageComponent.Initialize(tickDamage, tickInterval);
            }

            // 물줄기 초기 상태 설정 (비활성화)
            waterJetObject.SetActive(false);
        }
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

        // 회전 (궤도 중심에서 바깥쪽을 바라보도록)
        Vector3 offset = transform.position - centerOfRotation.position;
        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
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

    // 물줄기를 활성화하고 타이머를 시작합니다.
    public void ActivateJet()
    {
        if (waterJetObject == null || waterJetDamageComponent == null) return;
        waterJetObject.SetActive(true);
        waterJetDamageComponent.StartJetTimer();
    }

    public void DeactivateJet()
    {
        if (waterJetObject != null)
        {
            waterJetObject.SetActive(false);
        }
    }
}
