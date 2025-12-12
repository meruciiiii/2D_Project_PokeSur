using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharmanderAttack : MonoBehaviour, IAttackInterface
{
    [Header("CharBullet Field")]
    // CharBullet 프리팹을 연결할 수 있도록 이름 변경
    public GameObject charBulletPrefab;

    [Header("Stats")]
    [SerializeField] private float baseDamage = 5f; // 장판의 한 틱당 피해량
    [SerializeField] private float tickInterval = 0.5f; // 장판의 피해 주기
    [SerializeField] private float attackRate = 1.0f; // 초당 장판 소환 횟수

    [Header("Placement Settings")]
    [SerializeField] private float maxRandomRadius = 5f; // 플레이어 주변 최대 랜덤 반경

    private float attackCooldown;
    private float timeSinceLastAttack = 0f;

    [Header("Sound Settings")]
    public AudioSource audioSource;           // 사운드 재생을 위한 AudioSource 컴포넌트
    public AudioClip attackSoundClip;         // 공격 시 재생할 AudioClip

    public Animator Animator;
    private Transform playerTransform;

    void Start()
    {
        attackCooldown = 1f / attackRate;
        playerTransform = transform;
        Animator = GetComponent<Animator>();
    }

    void Update()
    {
        ExecuteAttack();
    }

    public void ExecuteAttack()
    {
        // 업데이트된 프리팹 변수 사용
        if (charBulletPrefab == null) return;

        timeSinceLastAttack += Time.deltaTime;

        if (timeSinceLastAttack >= attackCooldown)
        {
            timeSinceLastAttack = 0f;

            if (Animator != null)
            {
                Animator.SetBool("IsAttacking", true);
                if (audioSource != null && attackSoundClip != null)
                {
                    // PlayOneShot을 사용하여 다른 소리가 재생 중이어도 겹치지 않게 재생합니다.
                    audioSource.PlayOneShot(attackSoundClip);
                }
            }
            else
            {
                Event_SpawnField();
            }
        }
    }

    // 이 함수는 애니메이션 이벤트 또는 Animator가 없을 때 직접 호출됩니다.
    public void Event_SpawnField()
    {
        // 1. 플레이어 주변 360도 랜덤 위치 계산
        Vector2 randomOffset = Random.insideUnitCircle * maxRandomRadius;
        Vector3 spawnPosition = playerTransform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        // 2. 장판 생성 (업데이트된 프리팹 변수 사용)
        GameObject newField = Instantiate(charBulletPrefab, spawnPosition, Quaternion.identity);

        // 3. CharBullet 컴포넌트를 가져와 초기화합니다.
        CharBullet fieldController = newField.GetComponent<CharBullet>();

        if (fieldController != null)
        {
            fieldController.Initialize(baseDamage, tickInterval);
        }
        else
        {
            // 에러 메시지 업데이트
            Debug.LogError("CharBullet Prefab에 CharBullet 스크립트가 없습니다! CharBullet.cs 파일을 확인해주세요.");
            Destroy(newField);
        }

        if (Animator != null)
        {
            Animator.SetBool("IsAttacking", false);
        }
    }
}
