using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]                
public class PokemonInstanceData
{
    public PokemonSpecies species;           // 포켓몬 종류 (Pichu, Charmander 등)


    public PokemonSpecies nextEvolutionSpecies;

    // 몬스터 볼 드롭 조건 충족 여부
    public bool isReadyToEvolve = false;
    // 이 포켓몬 때문에 몬스터 볼이 이미 드롭되었는지 여부 (필드에 몬볼이 존재함)
    public bool hasMonsterBallDropped = false;

    // 생성자
    public PokemonInstanceData(PokemonSpecies speciesType)
    {
        species = speciesType;
        isReadyToEvolve = false;
        hasMonsterBallDropped = false;
        nextEvolutionSpecies = PokemonSpecies.None;
    }
}
