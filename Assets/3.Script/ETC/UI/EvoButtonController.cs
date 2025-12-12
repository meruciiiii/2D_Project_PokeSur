using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class EvoButtonController : MonoBehaviour
{
    private GameObject myPanel;

    [Header("UI Components")]
    public TextMeshProUGUI pokemonNameText;// 현재 포켓몬 이름   
    //public Image pokemonImage;               // 포켓몬 이미지 표시 (현재 포켓몬)
    //public Image evolutionImage;             // 진화 후 포켓몬 이미지 (진화 시)

    [Header("Buttons")]
    public Button evolveButton;              // '진화' 버튼
    public Button recruitButton;             // '영입' 또는 '새 포켓몬 획득' 버튼

    // 현재 선택지에 사용될 포켓몬 데이터
    private PokemonInstanceData currentTargetPokemon;
    // 영입될 새로운 포켓몬의 데이터
    private PokemonInstanceData newRecruitData;

    // 이 스크립트가 붙은 오브젝트 자체를 참조하여 활성화/비활성화합니다.
    void Awake()
    {
        myPanel = gameObject;
    }
    void Start()
    {
        // 게임 시작 시 패널을 숨깁니다.
        myPanel.SetActive(false);

        // 버튼 클릭 이벤트 리스너 연결
        if (evolveButton != null) evolveButton.onClick.AddListener(OnEvolveButtonClicked);
        if (recruitButton != null) recruitButton.onClick.AddListener(OnRecruitButtonClicked);
    }
    public void SetupPanel(PokemonInstanceData targetPokemon, PokemonInstanceData recruitData)
    {
        currentTargetPokemon = targetPokemon;
        newRecruitData = recruitData;
         // 1. UI 활성화
         myPanel.SetActive(true);

        if (recruitButton != null)
        {
            recruitButton.gameObject.SetActive(true);
            TextMeshProUGUI recruitButtonTextComponent = recruitButton.GetComponentInChildren<TextMeshProUGUI>();

            if (newRecruitData != null)
            {
                recruitButton.interactable = true; // 영입 가능
                if (recruitButtonTextComponent != null)
                {
                    recruitButtonTextComponent.text = $"{newRecruitData.species}";
                }
            }
            else // newRecruitData == null (영입 풀이 비어있음)
            {
                recruitButton.interactable = false; // 비활성화: 클릭 불가능
                if (recruitButtonTextComponent != null)
                {
                    // 영입 불가 상태 표시
                    recruitButtonTextComponent.text = "Recruit Pool Empty";
                }
            }
        }

        // --- 진화 버튼 (Evolve Button) 설정 ---
        if (targetPokemon.isReadyToEvolve)
        {
            // 몬스터 볼 획득 시 전달된 다음 진화 정보 (None일 수 있음)
            PokemonSpecies nextEvo = targetPokemon.nextEvolutionSpecies;
            TextMeshProUGUI evoButtonTextComponent = evolveButton.GetComponentInChildren<TextMeshProUGUI>();

            if (nextEvo != PokemonSpecies.None)
            {
                // --- 진화 가능한 경우 ---
                if (evolveButton != null)
                {
                    evolveButton.gameObject.SetActive(true); // 버튼 활성화
                    evolveButton.interactable = true;
                }
                if (evoButtonTextComponent != null)
                {
                    // 진화 버튼 텍스트를 다음 진화 포켓몬 이름으로 설정
                    evoButtonTextComponent.text = $"{nextEvo}";
                    pokemonNameText.text = nextEvo.ToString(); // 상단 이름 표시
                }
            }
            else
            {
                // --- 진화 불가능한 경우 (최종 진화형) ---
                if (evolveButton != null)
                {
                    // 진화 버튼 상호작용만 비활성화
                    evolveButton.interactable = false;
                }
                if (evoButtonTextComponent != null)
                {
                    // 버튼이 숨겨져도 텍스트 컴포넌트에 정보 표시 (디버깅용)
                    evoButtonTextComponent.text = "Full Evo";
                }
                Debug.LogWarning("SetupPanel: 이 포켓몬은 최종 진화형입니다. 진화 버튼을 비활성화합니다.");
            }
        }
    }


    // --- 버튼 이벤트 핸들러 ---

    private void OnEvolveButtonClicked()
    {
        if (currentTargetPokemon == null) return;

        Debug.Log($"{currentTargetPokemon.species} 진화 선택.");

        // 1. PokemonPartyManager에 진화 로직 실행 요청
        if (PokemonPartyManager.Instance != null)
        {
            Debug.Log($"[Evo] {currentTargetPokemon.species} 진화 요청! ");
            PokemonPartyManager.Instance.EvolvePokemon(currentTargetPokemon);
        }

        // 2. 선택 완료 처리
        CompleteChoice();
    }

    private void OnRecruitButtonClicked()
    {
        if (newRecruitData == null) return;

        // 영입 로직 요청
        if (PokemonPartyManager.Instance != null)
        {
            PokemonPartyManager.Instance.RecruitPokemon(newRecruitData);
        }

        //영입 선택 시 진화 대기 상태도 해제합니다.
        if (currentTargetPokemon != null)
        {
            // 영입을 선택했으므로, 이 포켓몬의 진화 기회는 소멸한 것으로 간주하고 상태를 리셋합니다.
            currentTargetPokemon.isReadyToEvolve = false;
            // 디버깅을 위해 추가
            Debug.Log($"{currentTargetPokemon.species}의 진화 기회가 영입 선택으로 인해 취소되었습니다.");
        }

        // 2. 선택 완료 처리
        CompleteChoice();
    }

    private void CompleteChoice()
    {
        // 1. UI 숨기기
        myPanel.SetActive(false);

        // 2. 게임 재개
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }

        // 3. (선택적) 포켓몬 데이터 참조 초기화
        currentTargetPokemon = null;
        newRecruitData = null;
    }
}
