using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Resources")]
    public GameObject monsterBallPrefab;
    public bool isMonsterBallOnField = false;
    public bool isGamePaused = false;

    [Header("References")]
    //진화/영입 선택 UI 컨트롤러 참조
    public EvoButtonController evoPanelController;
    private PokemonInstanceData currentMonsterBallTarget;
    public Ball_Indicator ballIndicator;
    [Header("Sound Settings")]
    public AudioSource audioSource;           // 사운드 재생을 위한 AudioSource 컴포넌트
    public AudioClip attackSoundClip;         // 공격 시 재생할 AudioClip
    [HideInInspector] public Vector3 targetPokemonLastPosition;

    private void Awake()
    {
        // 싱글톤 인스턴스 초기화
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MonsterBallCollected(PokemonInstanceData targetPokemon)
    {
        if (audioSource != null && attackSoundClip != null)
        {
            // PlayOneShot을 사용하여 다른 소리가 재생 중이어도 겹치지 않게 재생합니다.
            audioSource.PlayOneShot(attackSoundClip);
        }

        // 1. 진화 후보 포켓몬을 저장합니다.
        currentMonsterBallTarget = targetPokemon;
        if (ballIndicator != null)
        {
            ballIndicator.SetTarget(null); // 타겟을 null로 설정하여 인디케이터를 숨김
        }

        if (PokemonPartyManager.Instance != null && targetPokemon != null)
        {
            // PokemonPartyManager에 현재 씬 오브젝트 위치를 요청하여 저장합니다.
            targetPokemonLastPosition = PokemonPartyManager.Instance.GetPokemonPosition(targetPokemon);
            Debug.Log($"몬스터 볼 수집. 진화 포켓몬 스폰 위치 저장: {targetPokemonLastPosition}");
        }
        else if (PlayerManager.Instance != null)
        {
            // 포켓몬 오브젝트를 찾을 수 없는 경우 플레이어 위치를 사용합니다.
            targetPokemonLastPosition = PlayerManager.Instance.transform.position;
        }

        PauseGame();
    }


    //영입 풀에서 랜덤 포켓몬을 가져오는 F 헬퍼 함수
    public PokemonInstanceData GetRandomRecruit()
    {
        if (PokemonPartyManager.Instance == null || PokemonPartyManager.Instance.recruitmentPool.Count == 0)
        {
            Debug.LogWarning("영입 풀(recruitmentPool)이 비어있거나 PokemonPartyManager가 없습니다!");
            return null; // 영입할 포켓몬이 없음
        }

        //현재 파티에 있는 포켓몬들의 계열 기본형(1단계) 목록을 만듭니다.
        List<PokemonSpecies> speciesToExclude = new List<PokemonSpecies>();

        foreach (var pokemon in PokemonPartyManager.Instance.currentParty)
        {
            PokemonSpecies baseSpecies = PokemonPartyManager.Instance.GetBaseSpecies(pokemon.species);
            // 기본형이 유효하고 (None이 아니며) 목록에 없다면 추가
            if (baseSpecies != PokemonSpecies.None && !speciesToExclude.Contains(baseSpecies))
            {
                speciesToExclude.Add(baseSpecies);
            }
        }

        // 2. 영입 풀(파티에 없는 모든 종)을 가져옵니다.
        List<PokemonSpecies> recruitmentPool = PokemonPartyManager.Instance.recruitmentPool;

        if (recruitmentPool.Count == 0)
        {
            Debug.LogWarning("영입 풀(recruitmentPool)이 비어있습니다!");
            return null; // 영입할 포켓몬이 없음
        }

        List<PokemonSpecies> validRecruitPool = new List<PokemonSpecies>();

        // 3. 영입 풀을 순회하며 '1단계 포켓몬 (기본형)'만 필터링하고, 제외 목록에 없는 포켓몬만 선택합니다.
        foreach (PokemonSpecies species in recruitmentPool)
        {
            int stage = PokemonPartyManager.Instance.GetEvolutionStage(species);

            // a. 1단계 포켓몬이어야 하고,
            // b. 파티에 이미 그 진화 계열이 없어야 합니다.
            if (stage == 1 && !speciesToExclude.Contains(species))
            {
                validRecruitPool.Add(species);
            }
        }

        //validPool이 비었는지 확인 후 null 반환
        if (validRecruitPool.Count == 0)
        {
            Debug.LogWarning("영입 풀에 유효한 기본 진화 포켓몬이 없습니다! 영입 불가 신호.");
            return null;
        }

        // 4. 유효한 풀에서 랜덤 종 선택
        PokemonSpecies randomSpecies = validRecruitPool[UnityEngine.Random.Range(0, validRecruitPool.Count)];

        // 레벨 1짜리 임시 인스턴스를 생성하여 전달합니다.
        return new PokemonInstanceData(randomSpecies);
    }

    //  [수정된 부분 시작: ResumeGame]
    // 재개 함수 (선택 UI에서 버튼 클릭 시 호출)
    public void ResumeGame()
    {
        ClearMonsterBallState();
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.EndChoiceLoopAnimation(); // 애니메이션 종료
            PlayerManager.Instance.ResetLevelUpSoundFlag();
            // 몬스터 볼 이벤트 완료 후, TryLevelUp을 호출하여 누적된 XP를 기반으로 레벨업을 실행합니다.
            PlayerManager.Instance.TryLevelUp();
        }

        isGamePaused = false;
        Time.timeScale = 1f;
        Debug.Log("Game Resumed.");
    }
    // [수정된 부분 끝: ResumeGame]

    public void PauseGame()
    {
        if (isGamePaused) return;
        isGamePaused = true;

        // 1. 시간 정지
        Time.timeScale = 0f;
        Debug.Log("Game Paused. Starting Evo/Recruit Choice.");

        // UIManager 대신 EvoButtonController를 직접 호출
        PokemonInstanceData newRecruitData = GetRandomRecruit();

        // 3. UIManager를 호출하여 UI를 띄웁니다.
        if (evoPanelController != null)
        {
            evoPanelController.SetupPanel(currentMonsterBallTarget, newRecruitData);
        }
        else
        {
            Debug.LogError("EvoButtonController가 GameManager에 설정되지 않았습니다!");
        }
    }

    // UI가 닫히기 직전에 호출되어 몬스터 볼 상태를 리셋합니다.
    public void ClearMonsterBallState()
    {
        // 필드에 몬스터 볼이 없음을 표시하여 다음 드롭을 허용합니다.
        isMonsterBallOnField = false;
        if (currentMonsterBallTarget != null)
        {
            currentMonsterBallTarget.hasMonsterBallDropped = false;
            currentMonsterBallTarget = null;
        }
    }

    void LateUpdate()
    {
        if (isGamePaused) return;

        // 몬스터 볼 인디케이터의 타겟이 설정되어 있을 때만 인디케이터를 업데이트합니다.
        if (ballIndicator != null && ballIndicator.targetTransform != null)
        {
            ballIndicator.ManualUpdate();
        }
    }
}