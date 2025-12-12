using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 이 컴포넌트는 오직 BlastoiseBullet에 연결된 물줄기 프리팹에서만 사용되어야 합니다.
public class WaterJet : MonoBehaviour
{
    private float tickDamage;
    private float tickInterval;

    private readonly Dictionary<EnemyController, float> lastDamageTimes = new Dictionary<EnemyController, float>();

    [SerializeField] private string enemyTag = "Enemy";

    [Header("Lifetime Settings (1.0s Auto Deactivation)")]
    public float BLASTOISE_JET_DURATION = 1.5f; // 물줄기 지속 
    private Coroutine jetDisableCoroutine;

    // 부모로부터 데미지 정보 초기화
    public void Initialize(float damage, float interval)
    {
        tickDamage = damage;
        tickInterval = interval;
    }

    // OnEnable()이 제거되어 외부 명령(StartJetTimer)에만 반응합니다.

    public void StartJetTimer()
    {
        // 기존 타이머 코루틴 중지 및 리셋
        if (jetDisableCoroutine != null)
        {
            StopCoroutine(jetDisableCoroutine);
        }

        // 1.0초 뒤 물줄기를 끄는 코루틴 시작
        jetDisableCoroutine = StartCoroutine(DisableJetAfterDelay(BLASTOISE_JET_DURATION));
    }
    private IEnumerator DisableJetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DeactivateJet();
    }

    public void DeactivateJet()
    {
        gameObject.SetActive(false);
        jetDisableCoroutine = null;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(enemyTag)) return;

        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy == null) return;

        float currentTime = Time.time;
        lastDamageTimes.TryGetValue(enemy, out float lastDamageTime);

        if (currentTime >= lastDamageTime + tickInterval)
        {
            if (!enemy.gameObject.activeSelf)
            {
                if (lastDamageTimes.ContainsKey(enemy))
                {
                    lastDamageTimes.Remove(enemy);
                }
                return;
            }

            enemy.TakeDamage(Mathf.RoundToInt(tickDamage));
            lastDamageTimes[enemy] = currentTime;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null && lastDamageTimes.ContainsKey(enemy))
        {
            lastDamageTimes.Remove(enemy);
        }
    }

    private void OnDisable()
    {
        // 비활성화 시 모든 쿨다운 정보 및 코루틴 초기화
        lastDamageTimes.Clear();
        StopAllCoroutines();
        jetDisableCoroutine = null;
    }
}



