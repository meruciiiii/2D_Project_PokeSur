using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BlastoiseAttack : MonoBehaviour
{
    [Header("Orbital Projectile")]
    public GameObject bulletPrefab; // 반드시 BlastoiseBullet 스크립트를 가진 프리팹을 연결해야 합니다.
    private List<GameObject> activeBullets = new List<GameObject>();

    [Header("Stats")]
    [SerializeField] private float tickDamage = 5f;
    [SerializeField] private float tickInterval = 0.5f;
    [SerializeField] private float bulletLifetime = 7f;
    [SerializeField] private float attackRate = 1.0f;
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

    // IsBlastoise 필드는 제거되었습니다. 이 스크립트를 사용하면 무조건 물줄기가 나갑니다.

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
                    // 애니메이션이 없을 경우 즉시 실행.
                    Event_LaunchOrbital();
                }
            }
        }
    }

    // 모든 활성 물방울에 물줄기를 켜라고 명령합니다. (거북왕 전용)
    private void SetAllJetsActive(bool active)
    {
        foreach (GameObject bullet in activeBullets)
        {
            if (bullet != null)
            {
                // BlastoiseBullet 스크립트 참조
                BlastoiseBullet controller = bullet.GetComponent<BlastoiseBullet>();
                if (controller != null && active)
                {
                    controller.ActivateJet();
                }
            }
        }
    }

    public void Event_LaunchOrbital()
    {
        if (activeBullets.Count < maxOrbitals)
        {
            LaunchBlastoiseBullet();
        }

        // 거북왕은 공격 시 물줄기를 무조건 활성화합니다.
        if (activeBullets.Count > 0)
        {
            SetAllJetsActive(true);
        }

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
        // BlastoiseBullet 스크립트 참조
        BlastoiseBullet anchorBullet = activeBullets[0].GetComponent<BlastoiseBullet>();
        if (anchorBullet != null)
        {
            anchorAngle = anchorBullet.GetCurrentAngle();
        }
        for (int i = 0; i < count; i++)
        {
            float newAngle = (anchorAngle + (angleStep * i)) % FULL_CIRCLE;
            // BlastoiseBullet 스크립트 참조
            BlastoiseBullet bulletController = activeBullets[i].GetComponent<BlastoiseBullet>();
            if (bulletController != null)
            {
                bulletController.SetAngle(newAngle);
            }
        }
    }

    private void LaunchBlastoiseBullet()
    {
        GameObject newBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        // BlastoiseBullet 스크립트 참조
        BlastoiseBullet bulletController = newBullet.GetComponent<BlastoiseBullet>();

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
            Debug.LogError("발사체 프리팹에 BlastoiseBullet 스크립트가 없습니다!");
            Destroy(newBullet);
        }
    }
}
