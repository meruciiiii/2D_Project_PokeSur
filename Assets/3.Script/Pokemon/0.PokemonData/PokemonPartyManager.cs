using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class PokemonPartyManager : MonoBehaviour
{
    public static PokemonPartyManager Instance { get; private set; }

    [Header("Party Settings")]
    public int maxPartySize = 4;// 최대 파티 인원
    public List<PokemonInstanceData> currentParty = new List<PokemonInstanceData>(); // 현재 파티 목록

    [Header("Recruitment Pool")]
    // 영입 가능한 포켓몬 목록
    public List<PokemonSpecies> recruitmentPool = new List<PokemonSpecies>();

    [Header("Active & Target")]
    [Tooltip("현재 필드에서 플레이어와 함께 움직이며 경험치를 얻는 포켓몬")]
    public PokemonInstanceData activePokemon;

    // PokemonSpeciesData.cs에 정의된 SpeciesInfo 구조체를 사용합니다.
    private Dictionary<PokemonSpecies, PokemonSpeciesData.SpeciesInfo> pokemonDataMap =
        new Dictionary<PokemonSpecies, PokemonSpeciesData.SpeciesInfo>();

    // 포켓몬 데이터와 씬의 컨트롤러 오브젝트를 연결하는 딕셔너리
    private Dictionary<PokemonInstanceData, PlayerPokemonController> pokemonControllers =
        new Dictionary<PokemonInstanceData, PlayerPokemonController>();

    // [추가] 씬의 포켓몬 이동 컴포넌트 목록 (파티 순서대로 관리)
    private List<PokemonMove> pokemonMovers = new List<PokemonMove>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            pokemonDataMap = PokemonSpeciesData.AllSpeciesData.ToDictionary(entry => entry.Key, entry => entry.Value);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    private void Start()
    {
        // 초기 포켓몬 설정 (Pichu를 기본)
        if (currentParty.Count == 0)
        {
            PokemonInstanceData starter = new PokemonInstanceData(PokemonSpecies.Pichu);
            currentParty.Add(starter);
            activePokemon = starter;

            // 시작 시 포켓몬 소환
            SpawnPokemon(starter, Vector3.zero);
        }
        SetupRecruits();

        SetupFollowerChain();
    }
    
    // 씬에 포켓몬 오브젝트를 생성/교체하고 컨트롤러를 초기화합니다.
    public void SpawnPokemon(PokemonInstanceData data, Vector3 spawnPosition)
    {
        //// 1. 기존 오브젝트 제거
        //PlayerPokemonController oldController = GetController(data);
        //if (oldController != null)
        //{
        //    // 기존 오브젝트 파괴
        //    Destroy(oldController.gameObject);
        //    pokemonControllers.Remove(data); // 딕셔너리에서도 제거
        //}

        // 2. 새 오브젝트 생성 및 초기화
        // 데이터의 species를 사용하여 딕셔너리에서 프리팹을 찾습니다.
        GameObject prefabToSpawn = null;
        if (pokemonDataMap.TryGetValue(data.species, out PokemonSpeciesData.SpeciesInfo speciesInfo))
        {
            prefabToSpawn = speciesInfo.prefab;
        }

        if (prefabToSpawn != null)
        {
            GameObject newPokemonObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            PlayerPokemonController newController = newPokemonObject.GetComponent<PlayerPokemonController>();

            // PokemonMove 컴포넌트 가져오기
            PokemonMove newMover = newPokemonObject.GetComponent<PokemonMove>();

            if (newController != null)
            {
                newController.Initialize(data);
                RegisterController(data, newController);
            }
            else
            {
                Debug.LogError($"소환된 프리팹 ({data.species})에 PlayerPokemonController가 없습니다!");
            }
            //  씬에 오브젝트가 생성될 때마다 파티 연결 로직을 실행합니다.
            if (newMover != null)
            {
                SetupFollowerChain(); // 파티 연결 함수 호출
            }
            else
            {
                Debug.LogError($"소환된 프리팹 ({data.species})에 PokemonMove 컴포넌트가 없습니다! 꼬리잡기 불가.");
            }
            Debug.Log($"포켓몬 소환 완료: {data.species} at {spawnPosition}");
        }
        else
        {
            // Resource Load에 실패한 경우를 포함합니다.
            Debug.LogError($"프리팹 데이터가 없거나 로드되지 않았습니다. 종: ({data.species}). Resources 폴더에 프리팹이 있는지 확인하세요.");
        }
    }

    // 포켓몬 인스턴스 데이터에 해당하는 씬 오브젝트의 위치를 반환합니다. **(추가된 함수)**
    public Vector3 GetPokemonPosition(PokemonInstanceData data)
    {
        if (pokemonControllers.TryGetValue(data, out PlayerPokemonController controller) && controller != null)
        {
            return controller.transform.position;
        }
        // 오브젝트를 찾을 수 없는 경우 안전하게 맵 중앙을 반환하거나 플레이어 위치를 반환할 수 있습니다.
        if (PlayerManager.Instance != null)
        {
            return PlayerManager.Instance.transform.position;
        }
        return Vector3.zero;
    }

    // 현재 파티의 포켓몬들을 꼬리잡기 방식으로 연결합니다.
    private void SetupFollowerChain()
    {
        // 1. 현재 씬에 소환된 포켓몬 오브젝트들에서 PokemonMove 컴포넌트를 다시 가져옵니다.
        //    이 과정은 파티 순서(currentParty)와 씬 오브젝트 순서를 매칭시켜야 합니다.
        pokemonMovers.Clear(); // 목록 초기화

        foreach (PokemonInstanceData data in currentParty)
        {
            if (pokemonControllers.TryGetValue(data, out PlayerPokemonController controller))
            {
                PokemonMove mover = controller.GetComponent<PokemonMove>();
                if (mover != null)
                {
                    pokemonMovers.Add(mover);
                }
            }
        }

        // 2. 플레이어의 Transform을 가져옵니다.
        Transform playerTransform = PlayerManager.Instance?.transform;

        if (playerTransform == null)
        {
            Debug.LogError("PlayerManager가 없거나 플레이어 Transform을 찾을 수 없습니다.");
            return;
        }

        // 3. 꼬리잡기 체인 연결
        for (int i = 0; i < pokemonMovers.Count; i++)
        {
            PokemonMove currentMover = pokemonMovers[i];
            Transform target;

            if (i == 0)
            {
                //  1번 포켓몬: 트레이너(플레이어)를 타겟으로 설정
                target = playerTransform;
            }
            else
            {
                //  2번 포켓몬부터: 바로 앞 포켓몬(i-1)의 Transform을 타겟으로 설정
                target = pokemonMovers[i - 1].transform;
            }

            currentMover.SetTarget(target); // PokemonMove.cs의 SetTarget 함수 호출
        }
        Debug.Log($"Follower chain setup complete. {pokemonMovers.Count} followers linked.");
    }
    public PokemonSpecies GetNextEvolutionSpecies(PokemonSpecies currentSpecies)
    {
        if (pokemonDataMap.TryGetValue(currentSpecies, out PokemonSpeciesData.SpeciesInfo info))
        {
            return info.evolvesTo;
        }
        return PokemonSpecies.None;
    }

    private int GetRequiredEvolutionLevel(PokemonSpecies currentSpecies)
    {
        // 1. 현재 포켓몬의 다음 진화 종(Species)을 찾습니다.
        PokemonSpecies nextSpecies = GetNextEvolutionSpecies(currentSpecies);

        // 2. 다음 진화 종이 유효하다면, 해당 종의 evolutionLevel을 반환합니다.
        if (nextSpecies != PokemonSpecies.None)
        {
            if (pokemonDataMap.TryGetValue(nextSpecies, out PokemonSpeciesData.SpeciesInfo nextInfo))
            {
                return nextInfo.evolutionLevel;
            }
        }

        // 최종 진화형이거나 데이터 오류 시, 절대 도달할 수 없는 값 반환 (진화 불가 처리)
        return int.MaxValue;
    }

    // 이 포켓몬 종이 몇 단계 진화형인지 (1: 기본, 2: 1차 진화, 3: 2차 진화/최종) 반환합니다.
    // 이 정보는 GameManager에서 우선순위 결정에 사용됩니다.
    public int GetEvolutionStage(PokemonSpecies species)
    {
        if (pokemonDataMap.TryGetValue(species, out PokemonSpeciesData.SpeciesInfo info))
        {
            return info.evolutionStage;
        }
        return 0;
    }

    public PokemonSpecies GetBaseSpecies(PokemonSpecies species)
    {
        //pokemonDataMap에서 baseSpecies를 가져옵니다.
        if (pokemonDataMap.TryGetValue(species, out PokemonSpeciesData.SpeciesInfo info))
        {
            return info.baseSpecies;
        }
        return PokemonSpecies.None;
    }

    // 진화 후보 리스트에서 가장 낮은 진화 단계를 찾아서 반환합니다.
    // 이 반환 값은 GameManager가 우선순위가 낮은 포켓몬을 필터링하는 데 사용됩니다.
    public int FindLowestStage(List<PokemonInstanceData> candidates)
    {
        int minStage = int.MaxValue; // 가장 큰 값으로 초기화

        if (candidates == null || candidates.Count == 0)
        {
            return 0; // 후보가 없으면 0 반환
        }

        foreach (PokemonInstanceData pokemon in candidates)
        {
            int currentStage = GetEvolutionStage(pokemon.species);
            if (currentStage < minStage)
            {
                minStage = currentStage;
            }
        }

        return minStage;
    }

    public List<PokemonInstanceData> FindDropCandidates()
    {
        List<PokemonInstanceData> candidates = new List<PokemonInstanceData>();
        foreach (var pokemon in currentParty)
        {
            // 1. 진화 준비 상태이고 2. 아직 몬스터볼이 드롭되지 않은 포켓몬만 후보가 되어야 합니다.
            if (pokemon.isReadyToEvolve && !pokemon.hasMonsterBallDropped)
            {
                candidates.Add(pokemon);
            }
        }
        return candidates;
    }

    public PokemonInstanceData GetRandomRecruit()
    {
        if (recruitmentPool.Count == 0)
        {
            Debug.LogWarning("영입 풀(recruitmentPool)이 비어있습니다!");
            return null;
        }

        // 1. 현재 파티에 있는 포켓몬들의 계열 기본형(1단계) 목록을 만듭니다. (제외 목록)
        List<PokemonSpecies> speciesToExclude = new List<PokemonSpecies>();

        foreach (var pokemon in currentParty)
        {
            // 현재 종의 계열 기본형(1단계)을 가져옵니다.
            PokemonSpecies baseSpecies = GetBaseSpecies(pokemon.species);

            // 기본형이 유효하고 목록에 없다면 추가
            if (baseSpecies != PokemonSpecies.None && !speciesToExclude.Contains(baseSpecies))
            {
                speciesToExclude.Add(baseSpecies);
            }
        }

        List<PokemonSpecies> validRecruitPool = new List<PokemonSpecies>();

        // 2. 영입 풀을 순회하며 '1단계 포켓몬 (기본형)' 중 제외 목록에 없는 포켓몬만 필터링합니다.
        foreach (PokemonSpecies species in recruitmentPool)
        {
            int stage = GetEvolutionStage(species);

            // a. 1단계 포켓몬이어야 하고,
            // b. 파티에 이미 그 진화 계열이 없어야 합니다.
            if (stage == 1 && !speciesToExclude.Contains(species))
            {
                validRecruitPool.Add(species);
            }
        }

        if (validRecruitPool.Count == 0)
        {
            Debug.LogWarning("영입 풀에 유효한 기본 진화 포켓몬이 없습니다! 영입 불가 신호.");
            return null;
        }

        // 3. 유효한 풀에서 랜덤 종 선택
        PokemonSpecies randomSpecies = validRecruitPool[UnityEngine.Random.Range(0, validRecruitPool.Count)];

        // 레벨 1짜리 새 인스턴스를 생성하여 반환합니다.
        return new PokemonInstanceData(randomSpecies);
    }

    // 모든 포켓몬 종(Species)에서 현재 파티에 없는 포켓몬을 영입 풀에 추가합니다.
    private void SetupRecruits()
    {
        // 1. pokemonSpeciesData.AllSpeciesData의 키를 사용하여 영입 풀을 구성합니다.
        List<PokemonSpecies> allSpecies = new List<PokemonSpecies>(pokemonDataMap.Keys);

        foreach (PokemonInstanceData data in currentParty)
        {
            allSpecies.Remove(data.species);
        }

        recruitmentPool = allSpecies;
        Debug.Log($"Recruitment Pool Initialized with {recruitmentPool.Count} species.");
    }

    // --- 경험치 로직 ---

    // PlayerManager가 레벨업할 때 호출하여 파티 전체를 검사합니다.
    public void CheckPartyForEvolution()
    {
        // 파티 내 모든 포켓몬을 순회하며 진화 조건을 체크합니다.
        foreach (var pokemon in currentParty)
        {
            // 1. 현재 트레이너 레벨을 가져옵니다.
            int playerLevel = PlayerManager.Instance.currentLevel;

            // 2. 포켓몬의 다음 진화 종을 확인합니다.
            PokemonSpecies potentialNextSpecies = GetNextEvolutionSpecies(pokemon.species);

            if (potentialNextSpecies == PokemonSpecies.None)
                continue; // 최종 진화형이라면 건너뜁니다.

            // 3. 다음 진화형이 요구하는 트레이너 레벨을 가져옵니다.
            int requiredEvolutionLevel = GetRequiredEvolutionLevel(pokemon.species);

            // 최종 진화가 아니고, 아직 진화 준비 상태가 아니며, 레벨 조건 충족 시
            if (!pokemon.isReadyToEvolve && playerLevel >= requiredEvolutionLevel)
            {
                // 진화 준비 상태로 설정
                pokemon.isReadyToEvolve = true;
                pokemon.nextEvolutionSpecies = potentialNextSpecies; // 다음 진화 종 저장

                Debug.Log($"{pokemon.species} is now ready to evolve to {potentialNextSpecies}!");

                // 참고: 이 시점에 몬스터 볼 드롭을 요청하는 대신, 기존 로직처럼 적 처치 시 
                // GameManager에서 FindDropCandidates()를 통해 체크하게 남겨둡니다.
            }
        }
    }

    

    public void EvolvePokemon(PokemonInstanceData pokemon)
    {
        PokemonSpecies nextSpecies = pokemon.nextEvolutionSpecies;
        Debug.Log($"[Evolve Check] 요청된 진화 종: {nextSpecies}");
        if (nextSpecies != PokemonSpecies.None)
        {
            // 1. 이전 오브젝트의 컨트롤러를 가져옵니다.
            PlayerPokemonController oldController = GetController(pokemon);
            Vector3 spawnPosition = Vector3.zero;

            if (oldController != null)
            {
                // 이전 오브젝트에서 SquirtleAttack 컴포넌트 확인 및 파괴
                SquirtleAttack oldSquirtleAttack = oldController.GetComponent<SquirtleAttack>();
                if (oldSquirtleAttack != null)
                {
                    oldSquirtleAttack.DestroyAllOrbitals();
                    Debug.Log($"Evolution: {pokemon.species}의 궤도 발사체 (SquirtleAttack) 파괴 완료.");
                }

                // 이전 오브젝트에서 BlastoiseAttack 컴포넌트 확인 및 파괴
                // (이미 SquirtleAttack이 처리했으면 건너뛰지만, 안전을 위해 독립적으로 체크)
                BlastoiseAttack oldBlastoiseAttack = oldController.GetComponent<BlastoiseAttack>();
                if (oldBlastoiseAttack != null)
                {
                    oldBlastoiseAttack.DestroyAllOrbitals();
                    Debug.Log($"Evolution: {pokemon.species}의 궤도 발사체 (BlastoiseAttack) 파괴 완료.");
                }

                // 1-3. 파괴 직전의 위치를 저장하고 오브젝트를 파괴합니다.
                spawnPosition = oldController.transform.position;
                Destroy(oldController.gameObject);
                pokemonControllers.Remove(pokemon); // 딕셔너리에서도 제거
            }
            else
            {
                // 컨트롤러를 찾지 못할 경우 안전하게 플레이어 위치를 사용합니다.
                if (PlayerManager.Instance != null)
                {
                    spawnPosition = PlayerManager.Instance.transform.position;
                }
            }

            // 2. 포켓몬의 종(Species)을 새 진화형으로 업데이트
            pokemon.species = nextSpecies;

            // 3. 상태 플래그 리셋
            pokemon.isReadyToEvolve = false;
            pokemon.hasMonsterBallDropped = false;
            pokemon.nextEvolutionSpecies = PokemonSpecies.None;

            // 4. 저장된 위치를 사용하여 진화형 포켓몬을 스폰합니다.
            SpawnPokemon(pokemon, spawnPosition);

            SetupRecruits();
            Debug.Log($"Evolution complete: {pokemon.species} evolved at position: {spawnPosition}");
        }
    }
    
    public void RecruitPokemon(PokemonInstanceData newPokemon)
    {
        if (currentParty.Count >= maxPartySize)
        {
            Debug.LogWarning("Party is full. Cannot recruit new Pokmon.");
            return;
        }

        if (newPokemon != null)
        {
            // 1. 파티에 포켓몬을 추가합니다.
            currentParty.Add(newPokemon);

            //새로 영입된 포켓몬을 Active Pokemon으로 설정하여 경험치를 받게 합니다.
            activePokemon = newPokemon;

            //영입 후 영입 풀을 업데이트
            SetupRecruits();

            //새로 영입된 포켓몬 오브젝트를 씬에 생성
            SpawnPokemon(newPokemon, Vector3.zero);

            Debug.Log($"Recruit complete: {newPokemon.species} joined the party. Party Size: {currentParty.Count}/{maxPartySize}");
        }
    }
    // 씬에 소환된 PlayerPokemonController를 등록하여 딕셔너리에 추가합니다.
    public void RegisterController(PokemonInstanceData data, PlayerPokemonController controller)
    {
        if (!pokemonControllers.ContainsKey(data))
        {
            pokemonControllers.Add(data, controller);
        }
        else
        {
            pokemonControllers[data] = controller;
        }
    }
    // 딕셔너리에서 컨트롤러를 찾아 반환합니다. (SpawnPokemon의 기존 오브젝트 제거 로직에 사용됨)
    public PlayerPokemonController GetController(PokemonInstanceData data)
    {
        if (pokemonControllers.TryGetValue(data, out PlayerPokemonController controller))
        {
            return controller;
        }
        return null;
    }
}