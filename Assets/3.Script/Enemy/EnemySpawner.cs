using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EnemySpawner : MonoBehaviour
{
    private Transform playerTransform;

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public float spawnDistance = 15f;
    private float timeSinceLastSpawn;
    private Camera mainCamera;
    private float minSpawnDistance;
    private float nextRoundStartTime; //다음 라운드가 시작되는 시간

    [Header("round Settings")]
    public int round1time = 10;
    public int round2time = 20;
    public int round3time = 30; // 새로 추가
    public int round4time = 40; // 새로 추가
    public int roundMiniBoss1 = 50; // 미니보스 1마리
    public int roundMiniBoss2 = 70; // 미니보스 2마리
    public int roundBoss = 100; // 최종 보스전 시작 시간

    [Header("Boss Prefabs")]
    public GameObject slakingPrefab; // 게을킹 프리팹
    public GameObject finalBossPrefab; // 최종 보스 프리팹 (Entei)

    public int currentRound = 0; // 현재 라운드 번호 (1부터 7까지)
    public string currentRoundTitle { get; private set; }
    float currentTime;

    void Start()
    {
        currentTime = 0;
        // 카메라와 플레이어 참조
        mainCamera = Camera.main; // 메인 카메라 참조
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        timeSinceLastSpawn = spawnInterval;
        if (mainCamera != null && mainCamera.orthographic)
        {
            // 1. 카메라 시야 크기 계산
            float camHeight = mainCamera.orthographicSize;
            float camWidth = camHeight * mainCamera.aspect;

            // 2. 최소 스폰 거리 계산 (피타고라스 정리: 화면 대각선 길이 + 안전 여백)
            // 화면 대각선 길이 + 안전 여백 2f를 최소 스폰 거리로 설정합니다.
            minSpawnDistance = Mathf.Sqrt(camWidth * camWidth + camHeight * camHeight) + 2f;
        }
        //시작 시 초기 라운드 정보 설정
        currentRound = 1;
        currentRoundTitle = "Round 1";
        nextRoundStartTime = round2time;
    }
    void Update()
    {
        if (playerTransform == null || mainCamera == null) return;

        currentTime = Time.time;

        RoundSetting();
        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= spawnInterval)
        {
            SpawnRandomEnemy();
            timeSinceLastSpawn = 0f;
        }
    }
    void SpawnRandomEnemy()
    {
        // 1. 스폰 위치 계산 (플레이어 시야 밖)

        // 무작위 각도 설정
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 spawnDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

        // minSpawnDistance 이상, 일정 최대 거리(minSpawnDistance + 10f) 이내에서 스폰 
        float randomDistance = Random.Range(minSpawnDistance, minSpawnDistance + 10f);

        Vector3 spawnPosition = playerTransform.position + spawnDirection * randomDistance;

        // 2. 스폰할 몬스터 선택
        string monsterToSpawn = SelectRandomMonster();

        // 3. 싱글톤 Instance를 통해 풀 매니저에 접근
        GameObject newEnemy = EnemyPoolManager.Instance.GetObject(monsterToSpawn);

        if (newEnemy != null)
        {
            newEnemy.transform.position = spawnPosition;
        }
    }
    private string SelectRandomMonster()
    {

        int rand = Random.Range(0, 3);
        int rand2 = Random.Range(0, 5); // 0, 1, 2, 3, 4 중 하나 (더 복잡한 라운드용)

        switch (currentRound)
        {
            case 1:
                // Round 1: Tauros만 스폰
                return "Tauros";

            case 2:
                // Round 2: Golem과 Tauros 스폰 (Golem 1/3, Tauros 2/3)
                if (rand == 0) return "Golem";
                return "Tauros";

            case 3:
                // Round 3: Golem과 Tauros의 비율 조정 (Golem 1/2, Tauros 1/2)
                if (rand < 2) return "Golem"; // 0 또는 1일 때 Golem
                return "Tauros";

            case 4:
                // Round 4: Muk, Golem, Tauros 모두 스폰 (Muk 1/3, Golem 1/3, Tauros 1/3)
                if (rand == 0) return "Golem";
                if (rand == 1) return "Muk";
                return "Tauros";

            case 5:
                // Mini-Boss Round 1: 일반 몬스터와 함께 Muk의 스폰 확률 증가
                if (rand2 == 0 || rand2 == 1) return "Muk"; // Muk 2/5
                if (rand2 == 2) return "Golem";            // Golem 1/5
                return "Tauros";                           // Tauros 2/5

            case 6:
                // Mini-Boss Round 2: Muk과 Golem의 스폰 확률이 높음
                if (rand2 < 3) return "Muk"; // Muk 3/5
                if (rand2 == 3) return "Golem"; // Golem 1/5
                return "Tauros"; // Tauros 1/5

            case 7:
                // FINAL BOSS ROUND: 
                if (rand == 0) return "Golem";
                if (rand == 1) return "Muk";
                return "Tauros";

            default:
                // 정의되지 않은 라운드 번호에 대한 안전 장치 (기본 몬스터)
                return "Tauros";
        }

    }

    private void RoundSetting()
    {
        int newRound = currentRound;
        float newNextRoundTime = nextRoundStartTime; // 새로운 다음 라운드 시간을 임시로 저장

        //  새로운 7단계 라운드 규칙 적용
        if (currentTime >= roundBoss)
        {
            newRound = 7;
            newNextRoundTime = roundBoss; // 최종 보스 라운드에서는 더 이상 카운트다운을 할 필요가 없습니다.
        }
        else if (currentTime >= roundMiniBoss2)
        {
            newRound = 6;
            newNextRoundTime = roundBoss; // 다음 라운드는 Final Boss
        }
        else if (currentTime >= roundMiniBoss1)
        {
            newRound = 5;
            newNextRoundTime = roundMiniBoss2; // 다음 라운드는 MiniBoss 2
        }
        else if (currentTime >= round4time)
        {
            newRound = 4;
            newNextRoundTime = roundMiniBoss1; // 다음 라운드는 MiniBoss 1
        }
        else if (currentTime >= round3time)
        {
            newRound = 3;
            newNextRoundTime = round4time; // 다음 라운드는 Round 4
        }
        else if (currentTime >= round2time)
        {
            newRound = 2;
            newNextRoundTime = round3time; // 다음 라운드는 Round 3
        }
        else
        {
            newRound = 1; // Round 1은 currentTime < round1time 조건을 만족하지 않으므로, 이 부분을 수정합니다.
            // 아래의 newRound != currentRound 로직에서 newRound가 2가 되는 순간을 처리합니다.
            newNextRoundTime = round2time;
        }
        switch (newRound)
        {
            case 5:
                currentRoundTitle = "Mini-Boss Round 1";
                break;
            case 6:
                currentRoundTitle = "Mini-Boss Round 2";
                break;
            case 7:
                currentRoundTitle = "FINAL BOSS ROUND";
                break;
            default:
                currentRoundTitle = $"Round {newRound}";
                break;
        }
        // 라운드가 변경되었을 때만 처리
        if (newRound != currentRound)
        {
            currentRound = newRound;
            nextRoundStartTime = newNextRoundTime; // 다음 라운드 시작 시간 업데이트

            // 라운드에 따른 스폰 간격 조절 (난이도 상승)
            spawnInterval = Mathf.Max(0.5f, spawnInterval - 0.5f); // 스폰 간격을 0.5초씩 줄이고 최소 0.5초 유지

            switch (currentRound)
            {
                case 5:
                    SpawnBoss(slakingPrefab);
                    break;
                case 6:
                    SpawnBoss(slakingPrefab);
                    StartCoroutine(DelayedBossSpawnRoutine(slakingPrefab, 3.0f));
                    break;
                case 7:
                    SpawnBoss(slakingPrefab);
                    SpawnBoss(finalBossPrefab);
                    break;
                default: 
                    break;
            }
            Debug.Log($"라운드 {currentRound} 시작! ({currentRoundTitle}) 스폰시간: {spawnInterval}");

        }
    }
    private IEnumerator DelayedBossSpawnRoutine(GameObject bossPrefab, float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간만큼 대기

        // SpawnBossOrMiniboss 함수를 호출하여 보스를 스폰합니다.
        SpawnBoss(bossPrefab);
    }
    //미니보스/보스를 스폰하는 함수 (풀링 없이 Instantiate 사용)
    private void SpawnBoss(GameObject bossPrefab)
    {
        if (playerTransform == null || bossPrefab == null) return;

        // 1. 스폰 위치 계산 (플레이어 시야 밖)
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 spawnDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

        // minSpawnDistance 이상, 일정 최대 거리 이내에서 스폰 
        float randomDistance = Random.Range(minSpawnDistance, minSpawnDistance + 10f);

        Vector3 spawnPosition = playerTransform.position + spawnDirection * randomDistance;

        // 2. Instantiate를 사용하여 프리팹 스폰
        Instantiate(bossPrefab, spawnPosition, Quaternion.identity);

        Debug.Log($"{bossPrefab.name}이/가 보스로 스폰되었습니다.");
    }
    public float GetTimeUntilNextRound()
    {
        // 다음 라운드 시작 시간 - 현재 게임 시간
        float timeRemaining = nextRoundStartTime - currentTime;

        // 시간이 마이너스가 되는 것을 방지하고, 최종 보스 라운드에서는 0을 반환합니다.
        if (currentRound >= 7) // 최종 보스 라운드에서는 타이머가 필요 없습니다.
        {
            return 0f;
        }

        return Mathf.Max(0f, timeRemaining);
    }
}
