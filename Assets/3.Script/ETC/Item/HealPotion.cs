using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class HealPotion : MonoBehaviour
{
    [Header("Heal Settings")]
    public int healAmount = 20; // 힐 아이템 획득 시 회복할 체력 양을 인스펙터에서 설정

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와 충돌했는지 확인
        if (other.CompareTag("Player"))
        {
            if (PlayerManager.Instance != null)
            {
                // 플레이어 매니저의 Heal 함수를 호출합니다.
                PlayerManager.Instance.Heal(healAmount);
                Debug.Log($"{healAmount} 회복");
                // 아이템 획득 후 오브젝트 파괴
                Destroy(gameObject);
            }
        }
    }

}
