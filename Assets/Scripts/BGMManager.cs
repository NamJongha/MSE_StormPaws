using UnityEngine;

// BGM 관리 스크립트
public class BGMManager : MonoBehaviour
{
    public AudioSource bgm;

    void Awake()
    {
        bgm = GetComponent<AudioSource>();

        if (bgm != null)
        {
            DontDestroyOnLoad(bgm);
        }
        else
        {
            Destroy(bgm);
        }
    }
}
