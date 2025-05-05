using UnityEngine;
using UnityEngine.SceneManagement; // Scene 관리를 위해 필요

public class SceneNavigator : MonoBehaviour
{
    // Mypage 씬으로 이동하는 함수
    public void GoToMyPageScene()
    {
        // "mypage"는 실제 씬 파일의 이름과 정확히 일치해야 합니다 (확장자 .unity 제외)
        SceneManager.LoadScene("MyPage");
        Debug.Log("마이페이지 씬으로 이동 시도..."); // 확인용 로그
    }

    // Battle 씬으로 이동하는 함수
    public void GoToBattleScene()
    {
        // "battle"는 실제 씬 파일의 이름과 정확히 일치해야 합니다 (확장자 .unity 제외)
        SceneManager.LoadScene("Battle");
        Debug.Log("배틀 씬으로 이동 시도..."); // 확인용 로그
    }
}
