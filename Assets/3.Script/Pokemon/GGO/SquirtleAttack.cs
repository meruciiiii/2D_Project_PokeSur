using UnityEngine;
using System.Collections.Generic;

public class SquirtleAttack : MonoBehaviour, IAttackInterface
{
    [Header("Orbital Projectile")]
    public GameObject bulletPrefab; // 반드시 SquirtleBullet 스크립트를 가진 프리팹을 연결해야 합니다.
    private List<GameObject> activeBullets = new List<GameObject>();

    [Header("Stats")]
    [SerializeField] private float tickDamage = 5f;
    [SerializeField] private float tickInterval = 0.5f;
    [SerializeField] private float bulletLifetime = 7f;
     public float attackRate = 5.0f;
    [SerializeField] private int maxOrbitals = 1;

    [Header("Orbital Specific Settings")]
    public float orbitRadius = 2.5f;
    public float orbitSpeed = 150f;
    private const float FULL_CIRCLE = 360f;

    [Header("Sound Settings")]
    public AudioSource audioSource;           // 사운드 재생을 위한 AudioSource 컴포넌트
    public AudioClip attackSoundClip;         // 공격 시 재생할 AudioClip

    private float attackCooldown;
    private float timeSinceLastAttack = 0f;
    private Transform playerTransform;

    public Animator Animator;
    private bool isAttacking = false;

    // IsBlastoise 필드는 완전히 제거되었습니다.

    void Start()
    {
        attackCooldown = 1f / attackRate;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        Animator = GetComponent<Animator>();
    }

    void Update()
    {
        ExecuteAttack();
    }

    public void ExecuteAttack()
    {
        if (playerTransform == null || bulletPrefab == null) return;

        activeBullets.RemoveAll(bullet => bullet == null);

        if (!isAttacking)
        {
            timeSinceLastAttack += Time.deltaTime;

            if (timeSinceLastAttack >= attackCooldown)
            {
                isAttacking = true;
                timeSinceLastAttack = 0f; // 쿨타임 초기화 (최대 탄환 상태에서도 쿨타임 작동)

                if (Animator != null)
                {
                    // 애니메이션 시작 (최대 탄환 상태에서도 애니메이션 실행)
                    Animator.SetBool("IsAttacking", true);
                    if (audioSource != null && attackSoundClip != null)
                    {
                        // PlayOneShot을 사용하여 다른 소리가 재생 중이어도 겹치지 않게 재생합니다.
                        audioSource.PlayOneShot(attackSoundClip);
                    }
                }
                else
                {
                    // 애니메이션이 없을 경우 즉시 실행. 이 경우 LaunchSquirtleBullet 내부에서 maxOrbitals를 체크함.
                    Event_LaunchOrbital();
                }
            }
        }
    }

    // SetAllJetsActive 메서드 호출이 완전히 제거되었습니다.
    public void Event_LaunchOrbital()
    {
        if (activeBullets.Count < maxOrbitals)
        {
            LaunchSquirtleBullet();

        }

        // 꼬부기는 물줄기를 활성화하지 않습니다.

        isAttacking = false;
        if (Animator != null)
        {
            Animator.SetBool("IsAttacking", false);
        }
    }
    public void DestroyAllOrbitals()
    {
        // 리스트에 남아있는 파괴된 오브젝트 (null)를 미리 정리합니다. (안전성 확보)
        activeBullets.RemoveAll(bullet => bullet == null);

        foreach (GameObject bullet in activeBullets)
        {
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }

        // 리스트를 완전히 비워 다음 궤도 생성이 0부터 시작되도록 합니다.
        activeBullets.Clear();

        Debug.Log("Squirtle Orbitals Cleared upon Evolution.");
    }

    private void RecalculateOrbitalPositions()
    {
        int count = activeBullets.Count;
        if (count == 0) return;
        float angleStep = FULL_CIRCLE / count;

        float anchorAngle = 0f;
        SquirtleBullet anchorBullet = activeBullets[0].GetComponent<SquirtleBullet>();
        if (anchorBullet != null)
        {
            anchorAngle = anchorBullet.GetCurrentAngle();
        }
        for (int i = 0; i < count; i++)
        {
            float newAngle = (anchorAngle + (angleStep * i)) % FULL_CIRCLE;
            SquirtleBullet bulletController = activeBullets[i].GetComponent<SquirtleBullet>();
            if (bulletController != null)
            {
                bulletController.SetAngle(newAngle);
            }
        }
    }

    private void LaunchSquirtleBullet()
    {
        GameObject newBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        SquirtleBullet bulletController = newBullet.GetComponent<SquirtleBullet>();

        if (bulletController != null)
        {
            bulletController.Initialize(
                playerTransform,
                orbitRadius,
                orbitSpeed,
                tickDamage,
                tickInterval,
                bulletLifetime,
                0f
            );

            activeBullets.Add(newBullet);
            RecalculateOrbitalPositions();
        }
        else
        {
            Debug.LogError("발사체 프리팹에 SquirtleBullet 스크립트가 없습니다!");
            Destroy(newBullet);
        }
    }
}