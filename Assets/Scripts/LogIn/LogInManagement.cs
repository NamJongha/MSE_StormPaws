using UnityEngine;
using UnityEngine.SceneManagement;

public class LogInManagement : MonoBehaviour
{
    private bool isLogin = false;

    void Update()
    {
        if (TokenManager.hasToken)
        {
            isLogin = true;
        }

        if (isLogin)
        {
            if (!SceneManager.GetSceneByName("HomeScreen").isLoaded)
            {
                SceneManager.LoadScene("HomeScreen");
            }
        }
    }
}