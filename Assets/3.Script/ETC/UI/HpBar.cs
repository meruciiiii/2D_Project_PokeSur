using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class HpBar : MonoBehaviour
{
    [Header("Component Links")]
    // 이 HP 바가 표시할 대상 캐릭터의 HealthComponent (Inspector에서 연결)
    [SerializeField] private PlayerManager targetPlayer;
    private Slider hpSlider;

    void Start()
    {
        hpSlider = GetComponent<Slider>();
        targetPlayer = PlayerManager.Instance; // 싱글톤 인스턴스 참조

        if (hpSlider == null)
        {
            Debug.LogError("HpBar: Slider 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        if (targetPlayer != null)
        {
            // [수정] PlayerManager의 최대/현재 체력을 즉시 반영합니다.
            hpSlider.maxValue = targetPlayer.maxHealth;
            hpSlider.value = targetPlayer.currentHealth;

            // 이벤트 구독은 유지
            targetPlayer.OnHealthChanged.AddListener(UpdateHPBar);
        }
        else
        {
            Debug.LogError("HpBar: PlayerManager Instance를 찾을 수 없습니다. PlayerManager가 먼저 초기화되는지 확인해주세요.");
        }
    }

    // PlayerManager의 OnHealthChanged 이벤트에 의해 호출됨
    private void UpdateHPBar(float normalizedHealth)
    {
        if (targetPlayer != null)
        {
            // 0.0 ~ 1.0 비율을 Slider의 실제 값으로 변환하여 업데이트
            hpSlider.value = normalizedHealth * targetPlayer.maxHealth;
        }

        // 체력이 꽉 찼으면 HP 바를 숨김
        if (normalizedHealth >= 1.0f)
        {
            gameObject.SetActive(false);
        }
        else
        {
            // 데미지를 입었으면 HP 바를 표시
            gameObject.SetActive(true);
        }
    }

    // 컴포넌트가 파괴될 때 이벤트 리스너를 제거하여 메모리 누수를 방지
    void OnDestroy()
    {
        if (targetPlayer != null)
        {
            targetPlayer.OnHealthChanged.RemoveListener(UpdateHPBar);
        }
    }
}
