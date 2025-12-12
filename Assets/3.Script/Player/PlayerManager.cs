using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public UnityEvent<float> OnHealthChanged = new UnityEvent<float>();

    [Header("Player Score")]
    public int currentScore = 0; // 현재 점수
    public UnityEvent<int> OnScoreChanged = new UnityEvent<int>();
    public readonly int scorePerExp = 5; // EXP당 증가할 점수

    [Header("Player Level & EXP")]
    public int currentLevel = 1;         // 현재 레벨
    public float currentExp = 0f;        // 현재 경험치
    public float expToNextLevel = 100f;  // 다음 레벨까지 필요한 경험치
    public float incrementPerLevel = 3f; // 경험치 증가  / 선형 증가

    // [추가] EXP 변화 시 현재 EXP 비율(0.0f ~ 1.0f)을 전달하는 이벤트
    public UnityEvent<float> OnExpChanged = new UnityEvent<float>();
    // [추가] 레벨업 시 새 레벨을 전달하는 이벤트
    public UnityEvent<int, float> OnLevelUp = new UnityEvent<int, float>();

    [Header("Player Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    private Animator animator;
    [SerializeField] private string deathAnimationTrigger = "Die";
    private new SpriteRenderer renderer;

    [SerializeField] private string choiceLoopBoolName = "CollectBall"; // 애니메이션 트리거 변수
    [SerializeField] private PlayerMove playerMoveScript;

    [Header("Sound Settings")]
    public AudioSource audioSource;           // 사운드 재생을 위한 AudioSource 컴포넌트
    public AudioSource expAudioSource;   // 사운드 재생을 위한 AudioSource 컴포넌트
    public AudioSource healAudioSource;   // 사운드 재생을 위한 AudioSource 컴포넌트
    public AudioClip hitSoundClip;
    public AudioClip ExpGainClip;
    public AudioClip LevelUpClip;
    public AudioClip PostionClip;
    private bool hasLevelUpSoundPlayed = false;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 게임 시작 시 현재 HP를 최대 HP로 초기화
            currentHealth = maxHealth;
            animator = GetComponent<Animator>();
            OnHealthChanged.Invoke((float)currentHealth / maxHealth);
            OnExpChanged.Invoke(currentExp / expToNextLevel);
            OnScoreChanged.Invoke(currentScore);
            playerMoveScript = GetComponent<PlayerMove>();
        }
        else
        {
            Destroy(gameObject);
        }
        Debug.Log("플레이어 HP: " + currentHealth);
        renderer = GetComponent<SpriteRenderer>();

    }

    // [수정된 부분 시작: GainExp]
    // 몬스터 볼 이벤트 중복 드롭 방지 로직이 포함된 EXP 획득 함수
    public void GainExp(float expAmount)
    {
        int scoreIncrease = Mathf.RoundToInt(expAmount * scorePerExp);
        currentScore += scoreIncrease;
        OnScoreChanged.Invoke(currentScore); // UI에 점수 변경을 알립니다.

        currentExp += expAmount;

        // 1. 레벨업 조건을 충족했을 경우 처리
        if (currentExp >= expToNextLevel)
        {
            //  [핵심 수정]: currentExp를 expToNextLevel과 같게 만듭니다. (초과분 버리기)
            currentExp = expToNextLevel;

            // 1-1. 몬스터 볼이 필드에 없는 경우: 몬스터 볼 드롭을 시도합니다.
            if (GameManager.Instance != null && !GameManager.Instance.isMonsterBallOnField)
            {
                // 드롭 시도만 하고, 애니메이션 시작은 MonsterBallController로 완전히 이관합니다.
                TryDropMonsterBallIfReady();
                hasLevelUpSoundPlayed = false;
            }
            if (!hasLevelUpSoundPlayed)
            {
                expAudioSource.PlayOneShot(LevelUpClip);
                hasLevelUpSoundPlayed = true; // 소리를 재생했으므로 플래그 설정
            }
            else
            {
                // 몬스터 볼을 아직 먹지 못했지만, 레벨업 소리가 이미 났다면 일반 EXP 소리를 재생
                expAudioSource.PlayOneShot(ExpGainClip);
            }

            // XP는 몬스터 볼 획득 전까지 100%로 고정
            OnExpChanged.Invoke(1.0f);

            // 레벨업 조건 충족 시 이후 EXP 획득 및 업데이트를 중단하고 return합니다.
            return;
        }

        // 2. 레벨업 조건을 충족하지 못한 경우: EXP 바를 업데이트합니다.
            expAudioSource.PlayOneShot(ExpGainClip);
        OnExpChanged.Invoke(currentExp / expToNextLevel);
    }
    public void ResetLevelUpSoundFlag()
    {
        // GameManager.ResumeGame()에서 호출되어 레벨업 이벤트가 끝났음을 알립니다.
        hasLevelUpSoundPlayed = false;
    }
    // [수정된 부분 시작: TryDropMonsterBallIfReady]
    // [새 함수]: XP 조건을 체크하고 드롭을 실행할지 결정합니다. 애니메이션 로직은 제거되었습니다.
    private void TryDropMonsterBallIfReady()
    {
        // 드롭 시도 (XP는 유지됨). 성공 여부는 TryDropMonsterBall_Internal에서 플래그로 처리됩니다.
        if (TryDropMonsterBall_Internal())
        {
            // 몬스터 볼이 드롭되었으므로, 애니메이션 시작은 볼을 '획득'하는 시점으로 이관됩니다.
        }
    }

    // [수정된 부분 시작: TryLevelUp]
    // 몬스터 볼 이벤트 완료 후 GameManager.ResumeGame에서만 호출됩니다.
    // 누적된 XP를 처리하고 레벨업을 실제로 '실행'하는 역할만 수행합니다.
    public void TryLevelUp()
    {
        // 몬스터 볼 이벤트가 끝났으므로, 애니메이션 루프를 중단합니다. 
        EndChoiceLoopAnimation();

        // 몬스터 볼을 획득하여 isMonsterBallOnField가 false가 된 상태에서 호출됨
        while (currentExp >= expToNextLevel)
        {
            // Level Up!
            // 누적된 XP에서 다음 레벨 XP를 빼줍니다. (잔여 XP가 있을 수 있으므로)
            currentExp -= expToNextLevel;
            currentLevel++;
            Debug.Log($"Player Leveled Up! New Level: {currentLevel}. Remaining XP: {currentExp}");

            expToNextLevel = CalculateExpForNextLevel(currentLevel);
            OnLevelUp.Invoke(currentLevel, expToNextLevel);

            if (PokemonPartyManager.Instance != null)
            {
                PokemonPartyManager.Instance.CheckPartyForEvolution();
            }

            // 몬스터 볼 드롭 코드는 이 함수에서 제거됨.
        }

        // 최종 EXP 바 업데이트 (레벨업이 끝났거나 발생하지 않았을 경우)
        OnExpChanged.Invoke(currentExp / expToNextLevel);
    }

    // 드롭 성공 여부를 bool로 반환하도록 수정하고, 함수 이름을 변경했습니다.
    private bool TryDropMonsterBall_Internal()
    {
        if (GameManager.Instance == null || PokemonPartyManager.Instance == null) return false;

        // 몬스터 볼이 이미 필드에 있을 경우
        if (GameManager.Instance.isMonsterBallOnField)
        {
            Debug.LogWarning("몬스터 볼이 이미 필드에 있으므로 드롭을 건너뜁니다.");
            return false;
        }

        List<PokemonInstanceData> currentParty = PokemonPartyManager.Instance.currentParty;

        // 2. 드롭 전에 파티 전체의 진화 대기 상태를 초기화합니다. (필수 유지 로직)
        foreach (var pokemon in currentParty)
        {
            pokemon.isReadyToEvolve = false;
            pokemon.hasMonsterBallDropped = false;
            pokemon.nextEvolutionSpecies = PokemonSpecies.None;
        }

        // 3. 진화 가능한 포켓몬 (최종 진화형이 아닌 포켓몬)만 필터링합니다.
        List<PokemonInstanceData> droppableCandidates = new List<PokemonInstanceData>();

        foreach (var pokemon in currentParty)
        {
            PokemonSpecies nextEvo = PokemonPartyManager.Instance.GetNextEvolutionSpecies(pokemon.species);

            // 다음 진화 종이 None이 아닌 포켓몬만 후보 목록에 추가
            if (nextEvo != PokemonSpecies.None)
            {
                droppableCandidates.Add(pokemon);
            }
        }

        if (droppableCandidates.Count == 0 && currentParty.Count == PokemonPartyManager.Instance.maxPartySize) // maxPartySize 조건은 가정
        {
            // 몬스터 볼을 드롭하거나 씬을 멈출 필요가 없습니다. 점수 보너스만 제공합니다.

            // **점수 보너스 계산 및 적용**
            // 레벨업에 필요한 경험치(expToNextLevel)를 점수로 환산하여 보너스 제공
            int bonusScore = Mathf.RoundToInt(expToNextLevel * scorePerExp * 5); // 5배 보너스 가정
            currentScore += bonusScore;
            OnScoreChanged.Invoke(currentScore);

            // EXP 바 초기화 (다음 레벨 EXP를 0으로 만들고 다음 레벨 목표치 재설정)
            currentExp = 0f;
            currentLevel++;
            expToNextLevel = CalculateExpForNextLevel(currentLevel);

            OnExpChanged.Invoke(currentExp / expToNextLevel);
            OnLevelUp.Invoke(currentLevel, expToNextLevel);

            Debug.Log($" [Full Party/Evo 보너스] 점수 {bonusScore} 획득! 시간 멈춤 없이 다음 레벨로 전환됨.");

            return false; // 몬스터 볼 드롭 실패 (드롭하지 않음)
        }

        PokemonInstanceData targetPokemon;

        if (droppableCandidates.Count > 0)
        {
            // 3-1. 진화 가능한 포켓몬이 있다면, 그중에서 무작위로 선택합니다.
            int randomIndex = UnityEngine.Random.Range(0, droppableCandidates.Count);
            targetPokemon = droppableCandidates[randomIndex];
            Debug.Log($" [드롭 우선순위] 진화 가능 포켓몬 ({targetPokemon.species})에게 기회 부여.");
        }
        else
        {
            // 3-2. 진화 가능한 포켓몬이 없다면 (파티 전체가 최종 진화형), 파티 전체에서 무작위로 선택합니다.
            int randomIndex = UnityEngine.Random.Range(0, currentParty.Count);
            targetPokemon = currentParty[randomIndex];
            Debug.Log($" [드롭 우선순위] 진화 가능 포켓몬이 없어 최종 진화형 ({targetPokemon.species})에게 기회 부여.");
        }

        // 4. 선택된 포켓몬의 다음 진화 종을 가져옵니다. 
        PokemonSpecies potentialNextSpecies = PokemonPartyManager.Instance.GetNextEvolutionSpecies(targetPokemon.species);

        // 5. 선택된 포켓몬에게 진화 정보와 드롭 플래그를 강제로 부여합니다.
        targetPokemon.isReadyToEvolve = true;
        targetPokemon.nextEvolutionSpecies = potentialNextSpecies; // None일 수 있음!
        targetPokemon.hasMonsterBallDropped = true; // 몬스터 볼 드롭 플래그 설정

        // 5. 몬스터 볼 드롭 실행
        if (GameManager.Instance.monsterBallPrefab != null)
        {
            float randomX = UnityEngine.Random.Range(-19f, 20f);
            float randomY = UnityEngine.Random.Range(-11f, 12f);

            Vector3 dropPosition = new Vector3(randomX, randomY, 0f);

            GameObject ballObject = Instantiate(GameManager.Instance.monsterBallPrefab, dropPosition, Quaternion.identity);
            MonsterBallController ballController = ballObject.GetComponent<MonsterBallController>();

            if (ballController != null)
            {
                ballController.SetTargetPokemon(targetPokemon);
            }
            if (GameManager.Instance.ballIndicator != null)
            {
                GameManager.Instance.ballIndicator.SetTarget(ballObject.transform);
                //Debug.Log("인디케이터 타겟 설정 완료!");
            }
            GameManager.Instance.isMonsterBallOnField = true;
            return true; // 드롭 성공
        }

        return false; // 드롭 실패
    }


    // 다음 레벨에 필요한 경험치를 계산하는 함수 
    private float CalculateExpForNextLevel(int level)
    {
        return expToNextLevel + (level - 1) * incrementPerLevel;
    }


    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return; // 이미 죽었으면 무시

        currentHealth -= damageAmount;

        // HP가 0 미만으로 떨어지는 것을 방지
        currentHealth = Mathf.Max(0, currentHealth);
        OnHealthChanged.Invoke((float)currentHealth / maxHealth);

        audioSource.PlayOneShot(hitSoundClip);

        StopCoroutine("HitColor_Action_co");//기존 HitColor_Action_co이 실행 중이라면 멈춘다
        StartCoroutine("HitColor_Action_co");

        Debug.Log("플레이어가 피해를 입었습니다. 남은 HP: " + currentHealth);

        if (currentHealth == 0)
        {
            Die(); // HP가 0이 되면 사망 처리

            SceneManager.LoadScene("GameOver");
        }

    }
    public void Heal(int healAmount)
    {
        if (currentHealth >= maxHealth) return; // 이미 만피이면 무시

        currentHealth += healAmount;

        healAudioSource.PlayOneShot(PostionClip);

        // HP가 최대 HP를 초과하는 것을 방지
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        // maxHealth로 나눈 비율이 1.0f를 초과하지 않도록 Mathf.Clamp01로 제한합니다.
        float healthRatio = (float)currentHealth / maxHealth;
        if (currentHealth == maxHealth)
        {
            healthRatio = 0.9999f;
        }
        OnHealthChanged.Invoke(healthRatio); // 클리핑된 비율을 UI에 전달
    }
    private void Die()
    {
        //Debug.Log("플레이어가 사망했습니다!");
        if (animator != null)
        {
            animator.SetTrigger(deathAnimationTrigger);
            GetComponent<PlayerMove>().enabled = false;
        }
    }
    private IEnumerator HitColor_Action_co()
    {
        renderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        renderer.color = Color.white;
    }

    public void StartChoiceLoopAnimation()
    {
        if (animator != null)
        {
            // Bool 파라미터를 True로 설정하여 루프 애니메이션 상태로 즉시 전환
            animator.SetBool(choiceLoopBoolName, true);

            // (선택적으로, 만약을 대비해 IsMoving도 false로 설정)
            animator.SetBool("IsMoving", false);
        }
    }

    // [새 함수] 선택 루프 애니메이션을 종료하고 기본 상태로 돌아갑니다.
    public void EndChoiceLoopAnimation()
    {
        if (animator != null)
        {
            // Bool 파라미터를 False로 설정하여 Idle 상태로 돌아갑니다.
            animator.SetBool(choiceLoopBoolName, false);
        }
    }

    public void DisableMovement()
    {
        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = false;
            Debug.Log("플레이어 이동이 중지되었습니다.");
        }

        // 추가적으로 Rigidbody를 사용하는 경우 속도도 멈춰줍니다.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 물리적인 속도 정지
        }

        // 걷기 애니메이션도 멈춥니다.
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }
    }
}