using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : EnemyController
{
    [Header("Boss General Stats")]
    public string bossName; // Slaking 또는 Entei
    protected float currentAttackRange = 3.0f; // 몬스터의 공격 사거리 (기본값 설정)
    protected float attackCooltimeTimer;

    [Header("State")]
    protected bool isAttacking = false;

    protected override void Start()
    {
        base.Start();

    }



    protected override void FixedUpdate()
    {
        if (isAttacking || playerTransform == null || rb == null)
        {
            // 공격 중이거나 비활성화된 상태에서는 속도를 0으로 설정하여 정지 상태를 유지합니다.
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }
    }

    void Update()
    {
        HandleSpecialPattern();
    }
    //public new void TakeDamage(int damage)
    //{
    //    // 1. 부모의 TakeDamage(float damage) 호출: HP 감소, 사망 체크 등을 수행
    //    base.TakeDamage(damage);
    //
    //}

    protected override IEnumerator DieAnimationRoutine()
    {
        // 1. 모든 행동 정지 및 색상 변경 (부모 로직)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }

        // 2. 보스 유형에 따른 드롭 및 클리어 처리
        if (bossName == "Slaking")
        {
            HandleSlakingExpDrop(); // 미니보스 전용 드롭
        }
        else if (bossName == "Entei")
        {
            // TODO: 게임 클리어 로직 (6단계)
            Debug.Log("Game Clear! (Entei Defeated)");
        }

        // 3. 애니메이션 (1초 동안 아래로 사라지기)
        float timer = 0f;
        float deathDuration = 1.0f;

        while (timer < deathDuration)
        {
            // 아래로 천천히 이동
            transform.position += Vector3.down * Time.deltaTime * 0.5f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.red, Color.clear, timer / deathDuration);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // 4. HP UI 비활성화 요청
        if (BossHPUI.Instance != null)
        {
            BossHPUI.Instance.DeactivateBossUI();
        }

        // 5. 오브젝트 파괴 (Boss는 풀에 반환하지 않고 Destroy)
        Destroy(gameObject);
    }

    // Slaking 처치 시 EXP 아이템 5개 드롭 함수
    protected void HandleSlakingExpDrop()
    {
        if (ExpPrefab == null)
        {
            Debug.LogError("EXP Prefab not set in BossController!");
            return;
        }

        // 미니보스는 경험치 아이템 5개를 드롭합니다.
        int dropCount = 5;
        for (int i = 0; i < dropCount; i++)
        {
            // 약간의 랜덤 위치 오프셋을 주어 겹치지 않게 드롭
            Vector3 dropPosition = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);

            GameObject expObject = Instantiate(ExpPrefab, dropPosition, Quaternion.identity);

            // ExpItemController에 expDropValue 전달
            if (expObject.TryGetComponent<ExpItemController>(out ExpItemController expItem))
            {
                expItem.SetExpValue(expValue);
            }
        }
        Debug.Log($"Slaking 처치! EXP 아이템 {dropCount}개 드롭.");
    }

    protected override void HandleSpecialPattern()
    {
        // 1. 보스마다 고유한 패턴 로직을 여기에 작성하거나,
        //     SlakingController, EnteiController 같은 하위 클래스에서
        //     이 메서드를 override 하여 사용합니다.

        // 2. BossController 자체에는 특별한 기본 패턴 로직이 없으므로,
        //     비워두거나 Debug 로그를 남길 수 있습니다.

        // Debug.Log("BossController의 기본 패턴 체크.");
    }
}
