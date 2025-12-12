using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ExpItemController : MonoBehaviour
{
    private float expValue = 0f; // 몬스터로부터 전달받을 경험치 값

    // EnemyController에서 호출하여 경험치 양을 설정하는 함수
    public void SetExpValue(float value)
    {
        expValue = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어(Trainer) 태그에 충돌했는지 확인2
        if (other.CompareTag("Player"))
        {
            // 1. 경험치 부여: 포켓몬 파티 매니저에게 EXP를 전달합니다.
            if (PlayerManager.Instance != null && expValue > 0)
            {
                PlayerManager.Instance.GainExp(expValue);
            }
            // 2. 아이템 오브젝트 파괴 (획득 완료)
            Destroy(gameObject);
        }
    }
}
