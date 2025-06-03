using UnityEngine;
using UnityEngine.SceneManagement; // for Scene Management

/// <summary>
/// Scene Controller
/// </summary>

public class SceneNavigator : MonoBehaviour
{
    // Method for moving to MyPage
    public void GoToMyPageScene()
    {
        SceneManager.LoadScene("MyPage");
        Debug.Log("Try to move to MyPage..."); // Debugging Log
    }

    // Method for moving to Opponent
    public void GoToOpponentSelect()
    {
        SceneManager.LoadScene("Opponent");
        Debug.Log("Try to move to Opponent..."); // Debugging Log
    }

    public void GoToMyDeckScene()
    {
        SceneManager.LoadScene("MyDeckSelect");
    }

    public void GoToHomeScene()
    {
        SceneManager.LoadScene("HomeScreen");
    }

    public void GoToBattleScene()
    {
        SceneManager.LoadScene("Battle");
    }
}
