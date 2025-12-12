using UnityEngine;
using UnityEngine.SceneManagement;

public class OneMoreGame : MonoBehaviour
{
    public void OnButtonPressed()
    {
        // 씬 로드
        SceneManager.LoadScene("Intro");

        Debug.Log("다시 시작 버튼이 눌려 'Intro' 씬을 로드합니다.");
    }
}
