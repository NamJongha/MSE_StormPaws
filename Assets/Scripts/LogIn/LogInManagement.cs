using UnityEngine;
using UnityEngine.SceneManagement;

public class LogInManagement : MonoBehaviour
{
    private bool isLogin = false;

    // Update is called once per frame
    void Update()
    {
        if (isLogin)
        {
            SceneManager.LoadScene("NextSceneName");
        }
    }
}
