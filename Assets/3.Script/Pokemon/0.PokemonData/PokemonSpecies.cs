using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public enum PokemonSpecies
{
    None = 0,

    // 피츄 계열
    Pichu = 1,
    Pikachu = 2,
    Raichu = 3,

    // 치코리타 계열 
    Chikorita = 4,
    Bayleef = 5,
    Meganium = 6,

    // 꼬부기 계열 
    Squirtle = 7,
    Wartortle = 8,
    Blastoise = 9,

    // 파이리 계열 
    Charmander = 10,
    Charmeleon = 11,
    Charizard = 12,
}

public static class PokemonSpeciesData
{
    // 포켓몬의 종별 정보를 담는 구조체 (이전 PokemonPartyManager의 SpeciesInfo)
    [Serializable]
    public struct SpeciesInfo
    {
        public GameObject prefab;
        public int evolutionStage;      // 진화 단계 (1: 기본, 2: 1차, 3: 최종)
        public int evolutionLevel;
        public PokemonSpecies baseSpecies; // 이 계열의 기본 종
        public PokemonSpecies evolvesTo;   // 다음 진화 종 (최종 진화형은 PokemonSpecies.None)
    }

    // 모든 포켓몬 종별 데이터를 저장하는 정적 딕셔너리
    // PokemonPartyManager는 이 딕셔너리를 참조하여 데이터를 가져옵니다.
    public static readonly Dictionary<PokemonSpecies, SpeciesInfo> AllSpeciesData =
        new Dictionary<PokemonSpecies, SpeciesInfo>();

    // 정적 생성자: 클래스가 처음 사용될 때 모든 데이터를 초기화합니다.
    static PokemonSpeciesData()
    {
        InitializeData();
    }


    private static void InitializeData()
    {
        // --- Pichu 계열 ---
        AllSpeciesData.Add(PokemonSpecies.Pichu, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/Pica/0Pichu"),
            evolutionStage = 1,
            evolutionLevel = 1,          //  [값 설정] 기본형은 레벨 1부터 가능
            baseSpecies = PokemonSpecies.Pichu,
            evolvesTo = PokemonSpecies.Pikachu
        });
        AllSpeciesData.Add(PokemonSpecies.Pikachu, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/Pica/1Pikachu"),
            evolutionStage = 2,
            evolutionLevel = 10,         // [값 설정] 피카츄로 진화는 트레이너 레벨 10부터 가능
            baseSpecies = PokemonSpecies.Pichu,
            evolvesTo = PokemonSpecies.Raichu
        });
        AllSpeciesData.Add(PokemonSpecies.Raichu, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/Pica/2Raichu"),
            evolutionStage = 3,
            evolutionLevel = 20,         // [값 설정] 라이츄로 진화는 트레이너 레벨 20부터 가능
            baseSpecies = PokemonSpecies.Pichu,
            evolvesTo = PokemonSpecies.None // 최종 진화
        });

        // --- Chikorita 계열 --- (예시 레벨: 10, 20)
        AllSpeciesData.Add(PokemonSpecies.Chikorita, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/Chico/0Chikorita"),
            evolutionStage = 1,
            evolutionLevel = 1,
            baseSpecies = PokemonSpecies.Chikorita,
            evolvesTo = PokemonSpecies.Bayleef
        });
        AllSpeciesData.Add(PokemonSpecies.Bayleef, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/Chico/1Bayleef"),
            evolutionStage = 2,
            evolutionLevel = 10,
            baseSpecies = PokemonSpecies.Chikorita,
            evolvesTo = PokemonSpecies.Meganium
        });
        AllSpeciesData.Add(PokemonSpecies.Meganium, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/Chico/2Meganium"),
            evolutionStage = 3,
            evolutionLevel = 20,
            baseSpecies = PokemonSpecies.Chikorita,
            evolvesTo = PokemonSpecies.None
        });

        // --- Squirtle 계열 --- (예시 레벨: 10, 20)
        AllSpeciesData.Add(PokemonSpecies.Squirtle, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/GGo/0Squirtle"),
            evolutionStage = 1,
            evolutionLevel = 1,
            baseSpecies = PokemonSpecies.Squirtle,
            evolvesTo = PokemonSpecies.Wartortle
        });
        AllSpeciesData.Add(PokemonSpecies.Wartortle, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/GGo/1Wartortle"),
            evolutionStage = 2,
            evolutionLevel = 10,
            baseSpecies = PokemonSpecies.Squirtle,
            evolvesTo = PokemonSpecies.Blastoise
        });
        AllSpeciesData.Add(PokemonSpecies.Blastoise, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/GGo/2Blastoise"),
            evolutionStage = 3,
            evolutionLevel = 20,
            baseSpecies = PokemonSpecies.Squirtle,
            evolvesTo = PokemonSpecies.None
        });

        // --- Charmander 계열 --- (예시 레벨: 10, 20)
        AllSpeciesData.Add(PokemonSpecies.Charmander, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/Liza/0Charmander"),
            evolutionStage = 1,
            evolutionLevel = 1,
            baseSpecies = PokemonSpecies.Charmander,
            evolvesTo = PokemonSpecies.Charmeleon
        });
        AllSpeciesData.Add(PokemonSpecies.Charmeleon, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/Liza/1Charmeleon"),
            evolutionStage = 2,
            evolutionLevel = 10,
            baseSpecies = PokemonSpecies.Charmander,
            evolvesTo = PokemonSpecies.Charizard
        });
        AllSpeciesData.Add(PokemonSpecies.Charizard, new SpeciesInfo
        {
            prefab = Resources.Load<GameObject>("2.Model/Prefabs/Pokemon/Liza/2Charizard"),
            evolutionStage = 3,
            evolutionLevel = 20,
            baseSpecies = PokemonSpecies.Charmander,
            evolvesTo = PokemonSpecies.None
        });
        Debug.Log($"[PokemonSpeciesData] Loaded {AllSpeciesData.Count} Pokemon Species Data into static map.");
    }
}
