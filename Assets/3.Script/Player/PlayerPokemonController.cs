using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 이 스크립트는 씬의 각 포켓몬 오브젝트에 부착됩니다.
public class PlayerPokemonController : MonoBehaviour
{
    // PokemonPartyManager에 저장된 데이터의 인스턴스를 참조합니다. (링크 역할)
    public PokemonInstanceData data;

    // 포켓몬의 스프라이트 렌더러 (실제 시각적 요소를 제어)
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // 스프라이트 렌더러 컴포넌트를 가져옵니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerPokemonController requires a SpriteRenderer component.");
        }
    }

    // PokemonPartyManager가 이 컨트롤러를 초기화할 때 호출됩니다.
    // 이 함수가 호출된 후 이 오브젝트는 씬에서 활성화됩니다.
    public void Initialize(PokemonInstanceData instanceData)
    {
        this.data = instanceData;

        // 초기 시각적 요소 설정 (진화 시에도 이 로직이 실행되어 오브젝트가 교체됩니다.)
        UpdateVisuals();

        //이 컨트롤러가 활성화될 때 PokemonPartyManager에 자신을 등록합니다.
        if (PokemonPartyManager.Instance != null)
        {
            // 이 등록을 통해 PartyManager가 진화 시 기존 오브젝트를 찾고 파괴할 수 있습니다.
            PokemonPartyManager.Instance.RegisterController(this.data, this);
        }
    }

 
    public void UpdateVisuals()
    {
        if (data == null)
        {
            Debug.LogError("PokemonInstanceData is null. Cannot update visuals.");
            return;
        }

        // 이 로직은 현재 포켓몬의 종(data.species)에 맞게 스프라이트를 로드해야 합니다.
        if (spriteRenderer != null)
        {
            // 예: spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/" + data.species.ToString());
            Debug.Log($"[Visual Load] 현재 종: {data.species}.");
        }

    }
}