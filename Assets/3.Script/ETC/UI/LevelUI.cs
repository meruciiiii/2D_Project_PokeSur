using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하는 경우 이 줄을 사용하세요. (표준 권장)
// using UnityEngine.UI; // 기본 Text 컴포넌트를 사용하는 경우 필요합니다.

public class LevelUI : MonoBehaviour
{
    [Header("UI Components")]
    // [필수] 경험치 바 슬라이더 (Inspector에서 연결)
    [SerializeField] private Slider expSlider;
    // [필수] 현재 레벨을 표시할 텍스트 (Inspector에서 연결)
    [SerializeField] private TextMeshProUGUI levelText; // 또는 public Text levelText;
    //점수를 표시할 텍스트 (Inspector에서 연결)
    [SerializeField] private TextMeshProUGUI scoreText;

    private PlayerManager targetPlayer;

    void Start()
    {
        targetPlayer = PlayerManager.Instance; // 싱글톤 인스턴스 참조

        if (targetPlayer == null)
        {
            Debug.LogError("LevelUI: PlayerManager Instance를 찾을 수 없습니다.");
            return;
        }

        if (expSlider == null || levelText == null)
        {
            Debug.LogError("LevelUI: 슬라이더 또는 레벨 텍스트 컴포넌트가 연결되지 않았습니다.");
            return;
        }
        if (scoreText == null)
        {
            Debug.LogError("LevelUI: 점수 텍스트 컴포넌트(scoreText)가 연결되지 않았습니다.");
            // return은 하지 않습니다. 다른 UI는 계속 작동해야 하므로.
        }

        // 초기 값 설정 (PlayerManager의 현재 상태 반영)
        UpdateLevelText(targetPlayer.currentLevel);

        // 초기 EXP 바는 0.0 ~ 1.0 비율로 설정 (Awake/Start 순서 문제 회피)
        UpdateExpBar(targetPlayer.currentExp / targetPlayer.expToNextLevel);

        // 이벤트 구독
        targetPlayer.OnExpChanged.AddListener(UpdateExpBar);
        targetPlayer.OnLevelUp.AddListener(OnPlayerLeveledUp);
        targetPlayer.OnScoreChanged.AddListener(UpdateScoreText);

        UpdateScoreText(targetPlayer.currentScore);
    }
    //점수 텍스트
    private void UpdateScoreText(int score)
    {
        if (scoreText != null)
        {
            // "Score: 12345" 형식으로 표시
            scoreText.text = $"Score: {score.ToString()}";
        }
    }
    // EXP 증가 시 호출 (0.0f ~ 1.0f 비율)
    private void UpdateExpBar(float normalizedExp)
    {
        // EXP 바는 항상 0에서 1까지의 비율로 채워지도록 설정합니다.
        expSlider.maxValue = 1f;
        expSlider.value = normalizedExp;
    }

    // 레벨업 이벤트 발생 시 호출
    private void OnPlayerLeveledUp(int newLevel, float newExpToNextLevel)
    {
        UpdateLevelText(newLevel);

        // 레벨업 시 EXP 바는 0으로 리셋되며, 그 직후 PlayerManager에서
        // OnExpChanged.Invoke(currentExp / expToNextLevel)가 호출되어 남은 EXP로 다시 채워집니다.
    }

    // 레벨 텍스트만 업데이트
    private void UpdateLevelText(int level)
    {
        // "Lv. 1" 형식으로 표시
        levelText.text = $"Lv. {level.ToString()}";
    }

    void OnDestroy()
    {
        if (targetPlayer != null)
        {
            // 이벤트 구독 해제
            targetPlayer.OnExpChanged.RemoveListener(UpdateExpBar);
            targetPlayer.OnLevelUp.RemoveListener(OnPlayerLeveledUp);
            targetPlayer.OnScoreChanged.RemoveListener(UpdateScoreText);
        }
    }
}