using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    protected float currentMoveSpeed; // 실제 이동 시 사용되는 속도 (슬로우 디버프

    [Header("Damage Settings")]
    public int damageAmount = 10;          // 한 번에 입힐 피해량
    public float damageInterval = 0.5f;    // 피해를 입힐 간격 (0.5초)
    private float lastDamageTime;     
    // 마지막으로 피해를 입힌 시간
    [Header("Health Settings")]
    public int maxHealth = 3;       // 몬스터의 최대 HP (Inspector에서 설정)
    protected int currentHealth;      // 현재 HP
    public float expValue = 5f;     // 몬스터를 처치했을 때 얻는 경험치 양

    [Header("Pool Settings")]
    public string monsterName; // 예: "Golem", "Muk", "Tauros" 입력
    protected SpriteRenderer spriteRenderer;
    public Animator Animator;
   
    [Header("Drop Settings")]
    public GameObject ExpPrefab;         // 90% 확률
    public GameObject HealItemPrefab;    // 10% 확률

    [Header("Debuff Settings")]
    private Coroutine slowDebuffCoroutine; // 중복 슬로우 효과 관리를 위한 코루틴

    [Header("Death Settings")]
    protected float deathDuration = 0.3f; // 사라지는 총 시간


    protected Transform playerTransform;
    protected Rigidbody2D rb;

    protected virtual void Start()
    {
        // Rigidbody2D 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();

        // 트레이너(플레이어) 오브젝트를 찾습니다.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
    }
    protected virtual void OnEnable()
    {
        //랜덤 속도로 달려오게
        //각 객체마다 다르게

        float minSpeed = 0f;
        float maxSpeed = 0f;

        switch (monsterName)
        {
            case "Muk":
                minSpeed = 0.5f; 
                maxSpeed = 1.0f; 
                break;
            case "Golem":
                minSpeed = 1.0f; 
                maxSpeed = 2.0f; 
                break;
            case "Tauros":
                minSpeed = 2.5f; 
                maxSpeed = 3.5f; 
                break;
            case "Slaking":
                minSpeed = 1.5f;
                maxSpeed = 2.5f;
                break;
            case "Entei":
                minSpeed = 3.5f;
                maxSpeed = 4.5f;
                break;
            default:
                Debug.LogError($"Monster Name '{monsterName}' Mismatch! Setting default speed.");
                minSpeed = 2f;
                maxSpeed = 4f;
                break;
        }
        // 할당된 범위 내에서 랜덤 속도 설정
        moveSpeed = Random.Range(minSpeed, maxSpeed);
        //Debug.Log($"[OnEnable] {gameObject.name} Final Speed: {moveSpeed}");
        currentMoveSpeed = moveSpeed;

        currentHealth = maxHealth;

        // 비활성화 전에 색상이 바뀌었을 수 있으므로 활성화 시 흰색으로 초기화합니다.
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        // 기존에 진행 중이던 코루틴이 있다면 확실히 멈춥니다.
        if (slowDebuffCoroutine != null)
        {
            StopCoroutine(slowDebuffCoroutine);
            slowDebuffCoroutine = null;
        }

    }
    void Update()
    {
        // 이동에 영향을 주지 않는 패턴이나 쿨타임 체크는 Update에서 처리
        HandleSpecialPattern();
    }


    protected virtual void FixedUpdate()
    {
        if (playerTransform == null || rb == null) return;

        // 1. 플레이어를 향하는 방향 벡터 계산 
        Vector3 direction = playerTransform.position - transform.position;

        // 슬로우 디버프가 적용되면 currentMoveSpeed가 낮아져 느리게 움직입니다.
        Vector2 movement = direction.normalized * currentMoveSpeed;

        // 3. Rigidbody2D의 velocity를 사용하여 플레이어를 향해 이동
        rb.linearVelocity = movement;

        // 4. 스프라이트 좌우 반전 (플레이어를 바라보게)
        // 스프라이트가 기본적으로 오른쪽을 바라본다고 가정합니다.
        if (direction.x > 0)
        {
            // 플레이어가 왼쪽에 있을 때: 스프라이트 반전
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            // 플레이어가 오른쪽에 있을 때: 스프라이트 원래 방향
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

    }
    protected virtual void HandleSpecialPattern()
    {
        // Tauros처럼 패턴이 없는 몬스터는 기본적으로 아무것도 하지 않습니다.
    }
    // 몬스터가 풀에 반환되어 비활성화될 때 속도를 0으로 초기화합니다.
    private void OnDisable()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // 활성화된 슬로우 코루틴이 있다면 중지하고 참조를 해제합니다.
        if (slowDebuffCoroutine != null)
        {
            StopCoroutine(slowDebuffCoroutine);
            slowDebuffCoroutine = null;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 1. 플레이어 태그 확인
        if (other.CompareTag("Player"))
        {
            // 2. 피해 간격 확인
            // Time.time: 게임 시작 후 현재까지의 총 경과 시간
            if (Time.time >= lastDamageTime + damageInterval)
            {
                // 3. 피해 주기
                if (PlayerManager.Instance != null)
                {
                    PlayerManager.Instance.TakeDamage(damageAmount);
                }

                // 4. 다음 피해 시간을 현재 시간으로 갱신
                lastDamageTime = Time.time;
            }
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        //Debug.Log(gameObject.name + " 피해 입음. 남은 HP: " + currentHealth);
        if (damage > 0)
        {
            StopCoroutine("HitColor_Action_co");
            StartCoroutine("HitColor_Action_co");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        StartCoroutine(DieAnimationRoutine());
    }
    protected virtual IEnumerator DieAnimationRoutine()
    {
        // 1. 모든 행동 정지 (이동, 피격 처리 등)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 이동 정지
            rb.simulated = false;             // 물리 시뮬레이션 중지
        }
        //콜라이더 히트박스 삭제 
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        gameObject.tag = "Untagged";

        // 2. 애니메이션 정지 및 빨간색으로 변경
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }

        // 3. 아이템 드롭 (사망 시점의 위치에 드롭)
        HandleGeneralItemDrop();

        float timer = 0f;
        Vector3 initialScale = transform.localScale;

        // 4. 1초 동안 천천히 땅으로 사라지기
        while (timer < deathDuration)
        {
            float t = timer / deathDuration;

            // 투명도 감소 
            Color newColor = Color.Lerp(Color.red, Color.clear, t);
            spriteRenderer.color = newColor;

            // 아래로 천천히 이동 (사라지는 듯한 효과)
            // Y축 위치를 시간에 따라 감소시킵니다.
            transform.position += Vector3.down * Time.deltaTime * 0.5f; // 0.5f는 사라지는 속도 조절

            // 크기를 줄이는 효과
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // 5. 오브젝트 풀에 반환
        string poolKey = monsterName;
        if (EnemyPoolManager.Instance != null)
        {
            EnemyPoolManager.Instance.ReturnObject(gameObject, poolKey);
        }

        // 6. 상태 초기화 (재활용을 위해)
        if (rb != null)
        {
            rb.simulated = true;
        }
    }
    protected void HandleGeneralItemDrop()
    {

        //  일반 아이템 드랍 
        float chance = Random.Range(0f, 1.0f);

        if (chance <= 0.1f) // 10% (0.0 ~ 0.1)
        {
            // 힐 아이템 드랍
            if (HealItemPrefab != null)
            {
                Instantiate(HealItemPrefab, transform.position, Quaternion.identity);
            }
        }
        else // 90% (0.1 ~ 1.0)
        {
            if (ExpPrefab != null)
            {
                GameObject expObject = Instantiate(ExpPrefab, transform.position, Quaternion.identity);
                // 이 코드를 통해 몬스터의 expValue를 드롭된 아이템에 전달합니다.
                if (expObject.TryGetComponent<ExpItemController>(out ExpItemController expItem))
                {
                    expItem.SetExpValue(expValue);
                }
            }
        }
    }
    private IEnumerator HitColor_Action_co()
    {
        // 복구할 색상을 기본 흰색으로 설정
        Color restoreColor = Color.white;

        // **슬로우 코루틴이 활성화되어 있다면 복구 색상을 시안(Cyan)으로 설정**
        if (slowDebuffCoroutine != null)
        {
            restoreColor = Color.cyan;
        }

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = restoreColor;

    }
    // --- 치코리타의 탄환이 호출하는 함수: 슬로우 디버프 적용 ---
    public void ApplySlowDebuff(float slowPercentage, float duration)
    {
        // 이미 슬로우 코루틴이 실행 중이라면, 이전 코루틴을 중지하고 갱신합니다.
        if (slowDebuffCoroutine != null)
        {
            StopCoroutine(slowDebuffCoroutine);
        }

        // 새로운 슬로우 디버프 코루틴을 시작합니다.
        slowDebuffCoroutine = StartCoroutine(SlowDebuffRoutine(slowPercentage, duration));

        //Debug.Log(gameObject.name + "에 " + (slowPercentage * 100) + "% 슬로우 디버프 적용됨. (" + duration + "초)");
    }
    // 슬로우 디버프 효과를 처리하고 시간 경과 후 해제하는 코루틴입니다.
    private IEnumerator SlowDebuffRoutine(float slowPercentage, float duration)
    {
        // 1. 슬로우 효과 적용
        // 1f - slowPercentage를 곱하여 속도를 감소시킵니다. (예: 0.5f이면 50% 속도)
        float slowMultiplier = 1f - Mathf.Clamp01(slowPercentage);

        // 현재 이동 속도 = 기본 속도 * 슬로우 배율
        currentMoveSpeed = moveSpeed * slowMultiplier;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.cyan;
        }

        // 2. 지정된 시간(duration)만큼 대기
        yield return new WaitForSeconds(duration);

        // 3. 슬로우 효과 해제 (원래 속도로 복구)
        currentMoveSpeed = moveSpeed;

        //코루틴이 정상적으로 끝났고, renderer가 유효할 때만 흰색으로 복구
        if (spriteRenderer != null && slowDebuffCoroutine != null)
        {
            spriteRenderer.color = Color.white;
        }
        // 코루틴 종료 후 참조를 해제합니다.
        slowDebuffCoroutine = null;
    }
}
