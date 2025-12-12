using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class RoundUIManager : MonoBehaviour
{
    public TextMeshProUGUI roundText;
    private EnemySpawner enemySpawner;
    public TextMeshProUGUI timerText;

    [Header("BGM Settings")]
    public AudioSource bgmSource;           // BGM 재생을 위한 AudioSource
    public AudioClip defaultBGM;            // 기본 라운드 BGM (Round 1-4)
    public AudioClip bossBGM;
    public AudioClip pauseBGM;               // 일시정지 이벤트 BGM

    private bool isBossBGMPlaying = false;
    private bool isPauseBGMPlaying = false;

    private Coroutine bgmFadeCoroutine;
    private float savedBGMTime = 0f;        // ⭐️ 추가: 일시정지 시 BGM의 현재 재생 시간을 저장할 변수

    void Start()
    {
        // 1. EnemySpawner 참조
        enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (enemySpawner == null)
        {
            Debug.LogError("EnemySpawner 스크립트를 찾을 수 없습니다. 라운드 UI를 업데이트할 수 없습니다.");
            return;
        }

        // 2. UI 컴포넌트 참조 확인 (생략)
        if (roundText == null)
        {
            if (TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI tmpText))
            {
                roundText = tmpText;
            }
            if (roundText == null)
            {
                Debug.LogError("라운드 UI를 표시할 Text/TextMeshProUGUI 컴포넌트를 찾을 수 없습니다. roundText 변수를 Inspector에서 연결해주세요.");
                return;
            }
        }

        // 3. BGM 재생 시작 (Unscaled Time 버전 사용)
        if (bgmSource != null && defaultBGM != null)
        {
            PlayBGMUnscaled(defaultBGM, 0.5f);
        }

        // 4. 초기 UI 업데이트
        UpdateRoundUI(enemySpawner.currentRoundTitle);
    }

    void Update()
    {
        // 매 프레임 EnemySpawner의 최신 정보를 가져와 업데이트합니다.
        if (enemySpawner != null)
        {
            UpdateRoundUI(enemySpawner.currentRoundTitle);
            UpdateTimerUI(enemySpawner.GetTimeUntilNextRound());
        }
    }

    void LateUpdate() // LateUpdate 사용: Time.timeScale의 영향을 덜 받기 위해
    {
        if (enemySpawner == null) return;

        // 1. 몬스터 볼/일시정지 상태 체크 (최우선)
        if (Time.timeScale == 0f)
        {
            if (!isPauseBGMPlaying)
            {
                // ⭐️ 핵심 수정 A: 메인 BGM이 재생 중이라면 그 시간을 저장합니다.
                if (bgmSource.isPlaying && (bgmSource.clip == defaultBGM || bgmSource.clip == bossBGM))
                {
                    savedBGMTime = bgmSource.time;
                }

                // Pause BGM으로 전환
                PlayBGMUnscaled(pauseBGM, 0.5f);
                isPauseBGMPlaying = true;
                isBossBGMPlaying = false; // 다른 플래그 초기화
                Debug.Log("BGM 전환: 일시정지/이벤트 BGM 시작.");
            }
        }
        // 2. 일반 게임 플레이 상태 체크
        else // Time.timeScale > 0f
        {
            if (isPauseBGMPlaying)
            {
                // 1. 돌아갈 BGM 결정
                AudioClip clipToReturnTo;
                if (enemySpawner.currentRound >= 5)
                {
                    clipToReturnTo = bossBGM;
                }
                else
                {
                    clipToReturnTo = defaultBGM;
                }

                // ⭐️ 핵심 수정 B: 저장된 시간부터 재생하는 새로운 함수 호출
                ResumeMainBGM(clipToReturnTo);

                // 플래그 업데이트
                isPauseBGMPlaying = false;
                isBossBGMPlaying = (enemySpawner.currentRound >= 5);

                Debug.Log("BGM 전환: 게임 재개 후 이전 BGM으로 복귀 및 멈춘 지점부터 재생.");
            }

            // 3. 라운드별 BGM 전환 로직 (PauseBGM이 재생 중이 아닐 때만 실행)
            CheckAndSwitchBGM(enemySpawner.currentRound);
        }
    }

    private void UpdateRoundUI(string title)
    {
        if (roundText != null)
        {
            roundText.text = title;
        }
    }

    private void UpdateTimerUI(float timeRemaining)
    {
        if (timerText != null)
        {
            if (timeRemaining > 1)
            {
                int seconds = Mathf.FloorToInt(timeRemaining);
                timerText.text = $"Next Round in: {seconds}s";
            }
            else if (enemySpawner.currentRound >= 7)
            {
                timerText.text = "FINAL BATTLE";
            }
            else
            {
                timerText.text = "STARTING NEW ROUND...";
            }
        }
    }

    private void CheckAndSwitchBGM(int currentRound)
    {
        // 보스 라운드 시작 조건 (라운드 5부터 보스 BGM)
        if (currentRound >= 5)
        {
            if (!isBossBGMPlaying)
            {
                PlayBGMUnscaled(bossBGM, 0.5f);
                isBossBGMPlaying = true;
                Debug.Log("BGM 전환: 보스 라운드 BGM 시작.");
            }
        }
        // 일반 라운드 조건 (라운드 1~4)
        else
        {
            if (isBossBGMPlaying)
            {
                PlayBGMUnscaled(defaultBGM, 0.5f);
                isBossBGMPlaying = false;
                Debug.Log("BGM 전환: 기본 BGM으로 복귀.");
            }
        }
    }

    // ⭐️ 추가: 저장된 시간부터 재생하는 코루틴을 시작하는 헬퍼 함수
    private void ResumeMainBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;

        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
        }

        // savedBGMTime부터 재생하는 새로운 코루틴 시작
        bgmFadeCoroutine = StartCoroutine(ResumeMainBGM_co(new BGMTransitionData { targetClip = clip, duration = 0.5f }));
    }

    // Unscaled Time을 사용하는 BGM 재생 함수 (Time.timeScale=0에서도 작동)
    private void PlayBGMUnscaled(AudioClip clip, float fadeDuration)
    {
        if (bgmSource == null || clip == null || bgmSource.clip == clip) return;

        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
        }

        // Unscaled 버전 코루틴 시작
        bgmFadeCoroutine = StartCoroutine(FadeBGMUnscaled_co(new BGMTransitionData { targetClip = clip, duration = fadeDuration }));
    }

    // Unscaled Time을 사용하여 페이드 인/아웃을 처리하는 코루틴
    private IEnumerator FadeBGMUnscaled_co(BGMTransitionData data)
    {
        float startVolume = bgmSource.volume;
        float fadeOutTime = data.duration / 2f;
        float fadeInTime = data.duration / 2f;
        float targetVolume = 0.5f;

        // 1. Fade Out
        if (bgmSource.isPlaying)
        {
            float timer = 0f;
            while (timer < fadeOutTime)
            {
                timer += Time.unscaledDeltaTime; // Time.timeScale=0에서도 작동
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutTime);
                yield return null;
            }
            bgmSource.volume = 0f;
            bgmSource.Stop();
        }

        // 2. Clip Change and Fade In
        bgmSource.clip = data.targetClip;
        bgmSource.loop = true;
        bgmSource.Play();
        bgmSource.volume = 0f;

        float timer2 = 0f;
        while (timer2 < fadeInTime)
        {
            timer2 += Time.unscaledDeltaTime; // Time.timeScale=0에서도 작동
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, timer2 / fadeInTime);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }

    // ⭐️ 추가: 저장된 시간부터 재생하는 코루틴
    private IEnumerator ResumeMainBGM_co(BGMTransitionData data)
    {
        float startVolume = bgmSource.volume;
        float fadeOutTime = data.duration / 2f;
        float fadeInTime = data.duration / 2f;
        float targetVolume = 0.5f;

        // 1. Fade Out (Pause BGM)
        if (bgmSource.isPlaying)
        {
            float timer = 0f;
            while (timer < fadeOutTime)
            {
                timer += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutTime);
                yield return null;
            }
            bgmSource.volume = 0f;
            bgmSource.Stop();
        }

        // 2. Clip Change, Time Restore, and Fade In
        bgmSource.clip = data.targetClip;

        // ⭐️ 핵심: 저장된 시간으로 복원
        bgmSource.time = savedBGMTime;

        bgmSource.loop = true;
        bgmSource.Play();
        bgmSource.volume = 0f;

        float timer2 = 0f;
        while (timer2 < fadeInTime)
        {
            timer2 += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, timer2 / fadeInTime);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }

    // 코루틴 인수를 전달하기 위한 도우미 구조체
    private struct BGMTransitionData
    {
        public AudioClip targetClip;
        public float duration;
    }
}