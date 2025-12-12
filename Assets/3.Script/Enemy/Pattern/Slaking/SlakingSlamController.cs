using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SlakingSlamController : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 50; // 충격파가 주는 피해량

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와 충돌 시
        if (other.CompareTag("Player"))
        {
            // 1. 피해 로직 실행
            PlayerManager.Instance.TakeDamage(damage);

            Debug.Log($"Slaking Slam: {damage} 피해를 플레이어에게 입혔습니다.");

        }
    }
    public void DestroySlam()
    {
        Destroy(gameObject);
    }
}
