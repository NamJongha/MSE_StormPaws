using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages BGM across scenes, except in specific scenes.
/// </summary>

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;
    private AudioSource bgm;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            bgm = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Stop BGM in these scenes
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MyDeckSelect") 
        {
            Destroy(gameObject);
        }
        else if (scene.name == "AIMode")
        {
            Destroy(gameObject);
        }
    }
}
