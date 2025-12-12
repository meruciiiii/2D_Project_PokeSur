using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EnteiFireController : MonoBehaviour
{
    public int dotDamage = 2;              // 틱당 피해량
    public float damageTickInterval = 0.5f; // 데미지를 입힐 간격
    public float hazardDuration = 3.0f;     // 장판 유지 시간 (EnteiController에서 설정)

    // 장판 안에 있는 플레이어를 추적
    private HashSet<Collider2D> playersInHazard = new HashSet<Collider2D>();

    void Start()
    {
        // 1. 장판 유지 시간 후 스스로 파괴
        Destroy(gameObject, hazardDuration);

        // 2. 데미지 코루틴 시작
        StartCoroutine(DamageOverTimeRoutine());
    }

    // 장판 진입/탈출 감지
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersInHazard.Add(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersInHazard.Remove(other);
        }
    }

    // 데미지를 정기적으로 주는 코루틴
    private IEnumerator DamageOverTimeRoutine()
    {
        // 장판이 파괴될 때까지 반복
        while (true)
        {
            yield return new WaitForSeconds(damageTickInterval);

            // 장판 안에 있는 모든 플레이어에게 데미지 적용
            foreach (Collider2D playerCol in playersInHazard)
            {
                // 플레이어가 유효한지 확인하고 데미지 적용
                if (playerCol != null && PlayerManager.Instance != null)
                {
                    PlayerManager.Instance.TakeDamage(dotDamage);
                    Debug.Log($"Entei Hazard: Player took {dotDamage} DoT damage.");
                }
            }
        }
    }
}
