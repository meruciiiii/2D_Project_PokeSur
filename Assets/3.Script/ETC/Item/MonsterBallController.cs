using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MonsterBallController : MonoBehaviour
{
    // 이 몬스터 볼이 어떤 포켓몬의 진화를 위해 드롭되었는지 저장합니다.
    private PokemonInstanceData targetPokemon;

    // GameManager가 몬스터 볼 생성 시 호출하여 타겟 포켓몬을 설정합니다.
    public void SetTargetPokemon(PokemonInstanceData pokemon)
    {
        targetPokemon = pokemon;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 태그를 확인합니다. (플레이어 태그가 "Player"라고 가정)
        if (other.CompareTag("Player"))
        {
            CollectBall();
        }
    }

    private void CollectBall()
    {
        if (targetPokemon == null)
        {
            Debug.LogError("Monster Ball Collected but Target Pokemon is null! Cannot proceed.");
            return;
        }

        // 1. GameManager 상태 업데이트 (UI를 띄우고 시간 정지 준비)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MonsterBallCollected(targetPokemon);

        }

        // [핵심 수정]: 몬스터 볼을 획득했을 때만 플레이어 애니메이션을 시작합니다.
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.StartChoiceLoopAnimation();
        }
        // 3. 몬스터 볼 오브젝트 제거
        Destroy(gameObject);
    }
}