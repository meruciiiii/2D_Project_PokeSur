using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BossHPUI : MonoBehaviour
{
    public static BossHPUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject bossUIPanel;      // HP 바와 이름이 포함된 전체 UI 패널
    public Slider hpSlider;             // HP를 표시할 슬라이더
    public TextMeshProUGUI bossNameText;           // 보스 이름을 표시할 텍스트

    // 현재 UI에 연동된 보스 컨트롤러 참조
    private BossController currentBoss;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 초기에는 UI를 비활성화 상태로 시작
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(false);
        }
    }

    // 보스가 스폰될 때 호출되어 UI를 활성화하고 초기화합니다.
    public void ActivateBossUI(BossController boss)
    {
        currentBoss = boss;

        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(true);
        }

        // 텍스트와 슬라이더 초기화
        if (bossNameText != null)
        {
            bossNameText.text = boss.bossName.ToUpper();
        }

        if (hpSlider != null)
        {
            hpSlider.maxValue = boss.maxHealth;
            hpSlider.value = boss.maxHealth;
        }
    }

    //보스가 피해를 입었을 때 HP 바를 업데이트합니다.
    public void UpdateBossHP(int currentHealth)
    {
        if (hpSlider != null && currentBoss != null)
        {
            hpSlider.value = currentHealth;
        }
    }

    // 보스가 처치되었을 때 UI를 비활성화합니다.
    public void DeactivateBossUI()
    {
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(false);
        }
        currentBoss = null;
    }
}
